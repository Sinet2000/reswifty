namespace Reswifty.API.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    /// <summary>Start a database transaction.</summary>
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>Persist pending changes (outside of explicit transactions).</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}