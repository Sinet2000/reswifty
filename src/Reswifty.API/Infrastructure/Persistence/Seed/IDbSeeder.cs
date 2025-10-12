using Microsoft.EntityFrameworkCore;

namespace Reswifty.API.Infrastructure.Persistence.Seed;

public interface IDbSeeder<in TContext> where TContext : DbContext
{
    Task SeedAsync(TContext context);
}