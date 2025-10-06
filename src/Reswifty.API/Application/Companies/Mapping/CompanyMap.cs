using Reswifty.API.Application.Companies.DTOs;
using Reswifty.API.Domain.Companies;

namespace Reswifty.API.Application.Companies.Mapping;

public static class CompanyMap
{
    public static CompanyDto ToDto(this Company x) =>
        new(x.Id, x.Name, x.Type, x.Description, x.IsActive, x.CreatedAt, x.UpdatedAt,
            x.RowVersion is null ? null : Convert.ToBase64String(x.RowVersion));
}