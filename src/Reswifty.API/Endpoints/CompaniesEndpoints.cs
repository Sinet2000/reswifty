using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Reswifty.API.Application.Common;
using Reswifty.API.Application.Companies.DTOs;
using Reswifty.API.Application.Companies.Services;
using Reswifty.API.Common;
using Reswifty.API.Contracts.Common;

namespace Reswifty.API.Endpoints;

public static class CompaniesEndpoints
{
    public static IEndpointRouteBuilder MapCompaniesEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/companies").WithTags("Companies");

        // GET /api/companies?page=0&pageSize=20
        api.MapGet("/",
            async Task<Ok<ApiResult<PagedResult<CompanyDto>>>> (int page, int pageSize, ICompanyService svc, CancellationToken ct) =>
            {
                var result = await svc.GetPagedAsync(page, pageSize, ct);
                return ApiResultHttp.Ok(result);
            });

        // GET /api/companies/{id}
        api.MapGet("/{id:guid}",
            async Task<Results<Ok<ApiResult<CompanyDto>>, NotFound<ApiResult<string>>>> (Guid id, ICompanyService svc,
                CancellationToken ct) =>
            {
                var dto = await svc.GetAsync(id, ct);
                return dto is null ? ApiResultHttp.NotFound("Company not found") : ApiResultHttp.Ok(dto);
            });

        // POST /api/companies
        api.MapPost("/", async Task<Created<ApiResult<CompanyDto>>> (CompanyCreateDto dto, ICompanyService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateAsync(dto, ct);
            return ApiResultHttp.Created($"/api/companies/{created.Id}", created);
        });

        // PUT /api/companies/{id}
        api.MapPut("/{id:guid}",
            async Task<Results<Ok<ApiResult<CompanyDto>>, NotFound<ApiResult<string>>, Conflict<ApiResult<string>>>> (Guid id,
                CompanyUpdateDto dto, ICompanyService svc, CancellationToken ct) =>
            {
                try
                {
                    var updated = await svc.UpdateAsync(id, dto, ct);
                    return updated is null ? ApiResultHttp.NotFound("Company not found") : ApiResultHttp.Ok(updated);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return ApiResultHttp.Conflict("The record was modified by another process.");
                }
            });

        // DELETE /api/companies/{id}
        api.MapDelete("/{id:guid}",
            async Task<Results<NoContent, NotFound<ApiResult<string>>>> (Guid id, ICompanyService svc, CancellationToken ct) =>
            {
                var ok = await svc.DeleteAsync(id, ct);
                return ok ? ApiResultHttp.NoContent() : ApiResultHttp.NotFound("Company not found");
            });

        return api;
    }
}
