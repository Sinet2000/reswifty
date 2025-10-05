## Architecture

### Folder Structure

```bash
src/Reswifty
 ├─ Program.cs
 ├─ appsettings.json
 ├─ appsettings.Development.json
 ├─ Endpoints/                      # ✅ minimal API mappers live here (root)
 │   ├─ CompaniesEndpoints.cs
 │   ├─ ServicesEndpoints.cs
 │   ├─ ClientsEndpoints.cs
 │   └─ BookingsEndpoints.cs
 ├─ Core/
 │   ├─ Abstractions/               # interfaces (ports)
 │   ├─ Entities/                   # POCOs
 │   ├─ DTOs/                       # request/response contracts
 │   └─ Rules/                      # domain/business rules & validators (optional)
 ├─ Infrastructure/
 │   ├─ Persistence/                # DbContext + repositories if you want
 │   ├─ Integrations/
 │   │   ├─ Email/
 │   │   └─ Telegram/
 │   ├─ Background/                 # BackgroundService workers
 │   └─ Mapping/                    # mapping helpers (optional)
 └─ Common/
     ├─ Results/
     ├─ Errors/
     └─ Extensions/

```

### Options classes
```csharp
// Infrastructure/Config/AppOptions.cs
namespace Reswifty.Infrastructure.Config;
public class AppOptions { public string? PublicBaseUrl { get; set; } }

// Infrastructure/Config/EmailOptions.cs
namespace Reswifty.Infrastructure.Config;
public class EmailOptions { public string? From { get; set; } }

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));

```

### Core (business center)
```csharp
// Core/Entities/Company.cs
namespace Reswifty.Core.Entities;
public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? TelegramBotUsername { get; set; }
}

```

### DTOs
```csharp
namespace Reswifty.Core.DTOs;
public record CompanyCreate(string Name, string? TelegramBotUsername);
public record CompanyResponse(Guid Id, string Name, string? TelegramBotUsername);
```

### Abstractions (ports)
```csharp
// Core/Abstractions/IEmailSender.cs
namespace Reswifty.Core.Abstractions;
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}

namespace Reswifty.Core.Abstractions;
public interface ITelegramGateway
{
    Task SendMessageAsync(string telegramUserId, string message, CancellationToken ct = default);
}

// Core/Abstractions/IBookingService.cs
using Reswifty.Core.DTOs;
namespace Reswifty.Core.Abstractions;
public interface IBookingService
{
    Task<BookingResponse> CreateAsync(BookingCreate req, CancellationToken ct = default);
    Task<bool> CancelAsync(Guid bookingId, CancellationToken ct = default);
}
```

### Common (Result wrapper)
```csharp
// Common/Results/Result.cs
namespace Reswifty.Common.Results;
public readonly struct Result<T>
{
    public bool Success { get; }
    public T? Value { get; }
    public string? Error { get; }
    private Result(bool success, T? value, string? error) => (Success, Value, Error) = (success, value, error);
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}

```


### Infrastructure
```csharp
// Infrastructure/Persistence/AppDbContext.cs
using Reswifty.Core.Entities;

namespace Reswifty.Infrastructure.Persistence;

public class AppDbContext
{
    public List<Company> Companies { get; } = new();
    public List<ServiceItem> Services { get; } = new();
    public List<Client> Clients { get; } = new();
    public List<Booking> Bookings { get; } = new();
}

```


### Integrations
```csharp
// Infrastructure/Integrations/Email/ConsoleEmailSender.cs
using Reswifty.Core.Abstractions;
using Reswifty.Infrastructure.Config;
using Microsoft.Extensions.Options;

namespace Reswifty.Infrastructure.Integrations.Email;

public class ConsoleEmailSender : IEmailSender
{
    private readonly EmailOptions _opts;
    public ConsoleEmailSender(IOptions<EmailOptions> opts) => _opts = opts.Value;

    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        Console.WriteLine($"[EMAIL] From:{_opts.From} To:{to} | {subject}\n{body}");
        return Task.CompletedTask;
    }
}

```

### Services (application logic in Infra implementing ports)
```csharp
// Infrastructure/Services/BookingService.cs
using Reswifty.Core.Abstractions;
using Reswifty.Core.DTOs;
using Reswifty.Infrastructure.Persistence;

namespace Reswifty.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private readonly IEmailSender _email;
    private readonly ITelegramGateway _tg;

    public BookingService(AppDbContext db, IEmailSender email, ITelegramGateway tg)
        => (_db, _email, _tg) = (db, email, tg);

    public async Task<BookingResponse> CreateAsync(BookingCreate req, CancellationToken ct = default)
    {
        // Look up service to compute end time (fallback 60m)
        var svc = _db.Services.FirstOrDefault(x => x.Id == req.ServiceItemId);
        var duration = svc?.DurationMinutes ?? 60;

        var booking = new Core.Entities.Booking
        {
            Id = Guid.NewGuid(),
            CompanyId = req.CompanyId,
            ServiceItemId = req.ServiceItemId,
            ClientId = req.ClientId,
            StartUtc = req.StartUtc,
            EndUtc = req.StartUtc.AddMinutes(duration),
            Status = Core.Entities.BookingStatus.Pending
        };

        _db.Bookings.Add(booking);

        await _tg.SendMessageAsync(req.ClientId.ToString(), $"Booking {booking.Id} created.", ct);
        await _email.SendAsync("ops@reswifty.local", "New booking", $"Booking {booking.Id} created.", ct);

        return new BookingResponse(
            booking.Id, booking.CompanyId, booking.ServiceItemId, booking.ClientId,
            booking.StartUtc, booking.EndUtc, booking.Status.ToString());
    }

    public Task<bool> CancelAsync(Guid bookingId, CancellationToken ct = default)
    {
        var b = _db.Bookings.FirstOrDefault(x => x.Id == bookingId);
        if (b is null) return Task.FromResult(false);
        b.Status = Core.Entities.BookingStatus.Cancelled;
        return Task.FromResult(true);
    }
}

```


### Endpoints (root /Endpoints) — Minimal APIs only
```csharp
// Endpoints/CompaniesEndpoints.cs
using Reswifty.Core.DTOs;
using Reswifty.Infrastructure.Persistence;

namespace Reswifty.Endpoints;

public static class CompaniesEndpoints
{
    public static IEndpointRouteBuilder MapCompanies(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/companies");

        g.MapPost("/", (CompanyCreate req, AppDbContext db) =>
        {
            var entity = new Core.Entities.Company { Id = Guid.NewGuid(), Name = req.Name, TelegramBotUsername = req.TelegramBotUsername };
            db.Companies.Add(entity);
            var res = new CompanyResponse(entity.Id, entity.Name, entity.TelegramBotUsername);
            return Results.Created($"/companies/{res.Id}", res);
        });

        g.MapGet("/", (AppDbContext db) =>
            Results.Ok(db.Companies.Select(c => new CompanyResponse(c.Id, c.Name, c.TelegramBotUsername))));

        return app;
    }
}

// Endpoints/BookingsEndpoints.cs
using Reswifty.Core.Abstractions;
using Reswifty.Core.DTOs;

namespace Reswifty.Endpoints;

public static class BookingsEndpoints
{
    public static IEndpointRouteBuilder MapBookings(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/bookings");

        // Create booking -> calls service -> writes db -> returns DTO
        g.MapPost("/", async (BookingCreate req, IBookingService svc, CancellationToken ct) =>
        {
            var res = await svc.CreateAsync(req, ct);
            return Results.Created($"/bookings/{res.Id}", res);
        });

        // Cancel booking
        g.MapDelete("/{id:guid}", async (Guid id, IBookingService svc, CancellationToken ct) =>
        {
            var ok = await svc.CancelAsync(id, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}

```