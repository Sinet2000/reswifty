using Microsoft.EntityFrameworkCore;
using Reswifty.API.Application.Abstractions.Persistence;
using Reswifty.API.Application.Abstractions.Repositories;
using Reswifty.API.Application.Common;
using Reswifty.API.Application.Companies.DTOs;
using Reswifty.API.Application.Companies.Mapping;
using Reswifty.API.Domain.Companies;

namespace Reswifty.API.Application.Companies.Services;

public interface ICompanyService
{
    Task<PagedResult<CompanyDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct);

    Task<CompanyDto?> GetAsync(Guid id, CancellationToken ct);

    Task<CompanyDto> CreateAsync(CompanyCreateDto dto, CancellationToken ct);

    Task<CompanyDto?> UpdateAsync(Guid id, CompanyUpdateDto dto, CancellationToken ct);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public class CompanyService(ICompanyRepository repo, IUnitOfWork uow) : ICompanyService
{
    public async Task<PagedResult<CompanyDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
    {
        (page, pageSize) = Paging.Coerce(page, pageSize);
        var (items, total) = await repo.GetPagedAsync(page, pageSize, ct);
        return new(page, pageSize, total, items.Select(e => e.ToDto()).ToList());
    }

    public async Task<CompanyDto?> GetAsync(Guid id, CancellationToken ct)
        => (await repo.GetByIdAsync(id, ct))?.ToDto();

    public async Task<CompanyDto> CreateAsync(CompanyCreateDto dto, CancellationToken ct)
    {
        var entity = new Company(dto.Name, dto.Type, dto.Description);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.ToDto();
    }

    public async Task<CompanyDto?> UpdateAsync(Guid id, CompanyUpdateDto dto, CancellationToken ct)
    {
        var entity = await repo.GetTrackedAsync(id, ct);
        if (entity is null) return null;

        // optimistic concurrency via RowVersion
        if (!string.IsNullOrWhiteSpace(dto.RowVersion))
        {
            var original = Convert.FromBase64String(dto.RowVersion);
            repo.SetOriginalRowVersion(entity, original);
        }

        if (!string.IsNullOrWhiteSpace(dto.Name)) entity.SetName(dto.Name);
        if (dto.Type is { } t) entity.SetType(t);
        if (dto.Description is not null) entity.SetDescription(dto.Description);
        if (dto.IsActive is { } active)
        {
            if (active) entity.Activate();
            else entity.Deactivate();
        }

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw; // endpoint maps to 409
        }

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await repo.GetTrackedAsync(id, ct);
        if (entity is null) return false;
        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}