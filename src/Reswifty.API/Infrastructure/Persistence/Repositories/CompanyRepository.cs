using Microsoft.EntityFrameworkCore;
using Reswifty.API.Application.Abstractions.Repositories;
using Reswifty.API.Domain.Companies;

namespace Reswifty.API.Infrastructure.Persistence.Repositories;

public class CompanyRepository(AppDbContext db) : ICompanyRepository
{
    public async Task<(IReadOnlyList<Company> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct)
    {
        var q = db.Companies.AsNoTracking().OrderBy(x => x.Name);
        var total = await q.CountAsync(ct);
        var items = await q.Skip(page * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Companies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Company?> GetTrackedAsync(Guid id, CancellationToken ct)
        => db.Companies.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Company entity, CancellationToken ct)
        => await db.Companies.AddAsync(entity, ct);

    public void Remove(Company entity) => db.Companies.Remove(entity);

    public void SetOriginalRowVersion(Company entity, byte[] original)
        => db.Entry(entity).Property(e => e.RowVersion!).OriginalValue = original;
}