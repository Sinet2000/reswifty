using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Reswifty.API.Domain.Companies;

namespace Reswifty.API.Infrastructure.Persistence.Seed;

public sealed class AppDbContextSeed(
    IWebHostEnvironment env,
    ILogger<AppDbContextSeed> logger
) : IDbSeeder<AppDbContext>
{
    public async Task SeedAsync(AppDbContext context)
    {
        // bail if anything already exists
        if (await context.Companies.AsNoTracking().AnyAsync())
        {
            logger.LogInformation("Companies already present. Skipping seed.");
            return;
        }

        var contentRoot = env.ContentRootPath;
        var jsonPath = Path.Combine(contentRoot, "Setup", "companies.json");

        CompanySeedDto[] source;

        if (File.Exists(jsonPath))
        {
            logger.LogInformation("Seeding Companies from {Path}", jsonPath);
            var json = await File.ReadAllTextAsync(jsonPath);
            source = JsonSerializer.Deserialize<CompanySeedDto[]>(json,
                         new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        else
        {
            logger.LogWarning("No companies.json found at {Path}. Using built-in defaults.", jsonPath);
            source = DefaultCompanies();
        }

        // Normalize + de-dupe by name
        var normalized = source
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToArray();

        var companies = new List<Company>(normalized.Length);
        foreach (var s in normalized)
        {
            // NOTE: adjust cast if your CompanyType is not an int-backed enum
            var type = (CompanyType)s.Type;

            var c = new Company(
                name: s.Name!.Trim(),
                type: type,
                description: string.IsNullOrWhiteSpace(s.Description) ? null : s.Description!.Trim()
            );

            // Optional: start some inactive if you want
            if (s.IsActive.HasValue && !s.IsActive.Value)
                c.Deactivate();

            companies.Add(c);
        }

        await context.Companies.AddRangeAsync(companies);
        var saved = await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} companies.", saved);
    }

    private static CompanySeedDto[] DefaultCompanies() =>
    [
        new CompanySeedDto { Name = "Contoso Ltd.", Type = (int)CompanyType.Salon, Description = "Primary supplier" },
        new CompanySeedDto { Name = "Fabrikam, Inc.", Type = (int)CompanyType.Clinic, Description = "Enterprise customer" },
        new CompanySeedDto { Name = "Tailspin Toys", Type = (int)CompanyType.Gym, Description = "Channel partner" }
    ];

    private sealed class CompanySeedDto
    {
        public string? Name { get; set; }
        public int Type { get; set; } // int backing for your CompanyType enum
        public string? Description { get; set; }
        public bool? IsActive { get; set; } // optional; defaults to true in entity
    }
}