namespace Reswifty.API.Application.Abstractions.Persistence;

public interface IAppDbContext
{
    /// <summary>Persist pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}