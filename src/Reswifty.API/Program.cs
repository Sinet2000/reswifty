using Reswifty.API.Extensions;
using Reswifty.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();