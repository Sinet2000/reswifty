using Reswifty.API.Application.Abstractions.Identity;
using Reswifty.API.Application.Auth.Commands;
using Reswifty.API.Common;
using Reswifty.API.Contracts.Auth.Requests;
using Reswifty.API.Contracts.Auth.Responses;
using Reswifty.API.Contracts.Common;

namespace Reswifty.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/auth").WithTags("Auth");

        api.MapPost("/login", async (HttpContext http, LoginRequest req, IAuthService auth, CancellationToken ct) =>
            {
                try
                {
                    var result = await auth.LoginAsync(new LoginCommand(req.Email, req.Password), ct);

                    var response = new LoginResponse(result.AccessToken, result.RefreshToken, result.ExpiresAtUtc);
                    return ApiResultHttp.Ok(response, http.TraceIdentifier);
                }
                catch (Exception ex)
                {
                    return ApiResultHttp.FromException(ex, http);
                }
            })
            .WithTags("Auth")
            .Produces<ApiResult<LoginResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
