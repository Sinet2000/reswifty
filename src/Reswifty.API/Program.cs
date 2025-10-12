using Reswifty.API.Endpoints;
using Reswifty.API.Infrastructure;
using Reswifty.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureAppSettings();
builder.AddServiceDefaults();

// Application (use-cases)
builder.AddApplication();

// Infrastructure (adapters that don't need CS)
builder.Services.AddInfrastructure(builder.Configuration);

// Persistence (DbContext, repos, UoW, migrations, seed)
builder.AddPersistence();

// Web layer stuff
builder.Services.AddProblemDetails();

builder.AddDefaultOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapCompaniesEndpoints();
app.MapAuthEndpoints();

app.UseDefaultOpenApi();
app.Run();
