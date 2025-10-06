using Reswifty.API.Domain.Companies;

namespace Reswifty.API.Application.Companies.DTOs;

public sealed record CompanyCreateDto(string Name, CompanyType Type, string? Description);

public sealed record CompanyUpdateDto(string? Name, CompanyType? Type, string? Description, bool? IsActive, string? RowVersion);

public sealed record CompanyDto(
    Guid Id,
    string Name,
    CompanyType Type,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? RowVersion);