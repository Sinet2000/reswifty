using Reswifty.API.Application.Abstractions.Identity;
using Reswifty.API.Application.Auth.Commands;
using Reswifty.API.Common;
using Reswifty.API.Common.Exceptions.Auth;
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

        api.MapPost("/refresh", async (HttpContext http, RefreshTokenRequest req, IAuthService auth, CancellationToken ct) =>
            {
                try
                {
                    var cmd = new RefreshTokenCommand(
                        RefreshToken: req.RefreshToken,
                        IpAddress: http.Connection.RemoteIpAddress?.ToString(),
                        UserAgent: http.Request.Headers.UserAgent.ToString());

                    var r = await auth.RefreshAsync(cmd, ct);
                    var resp = new LoginResponse(r.AccessToken, r.RefreshToken, r.ExpiresAtUtc);
                    return ApiResultHttp.Ok(resp, http.TraceIdentifier);
                }
                catch (InvalidRefreshTokenException) { return ApiResultHttp.Bad("invalid_refresh_token", http.TraceIdentifier); }
                catch (RefreshTokenExpiredException) { return ApiResultHttp.Bad("refresh_token_expired", http.TraceIdentifier); }
                catch (RefreshTokenRevokedException) { return ApiResultHttp.Bad("refresh_token_revoked", http.TraceIdentifier); }
                catch (AccountUnavailableException) { return ApiResultHttp.Bad("account_unavailable", http.TraceIdentifier); }
                catch (Exception ex) { return ApiResultHttp.FromException(ex, http); }
            })
            .Produces<ApiResult<LoginResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
