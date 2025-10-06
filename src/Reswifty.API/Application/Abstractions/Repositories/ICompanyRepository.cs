using Reswifty.API.Domain.Companies;

namespace Reswifty.API.Application.Abstractions.Repositories;

public interface ICompanyRepository
{
    Task<(IReadOnlyList<Company> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct);

    Task<Company?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<Company?> GetTrackedAsync(Guid id, CancellationToken ct);

    Task AddAsync(Company entity, CancellationToken ct);

    void Remove(Company entity);

    void SetOriginalRowVersion(Company entity, byte[] original);
}