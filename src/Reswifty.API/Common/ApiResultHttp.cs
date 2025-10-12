using Microsoft.AspNetCore.Http.HttpResults;
using Reswifty.API.Application.Common;
using Reswifty.API.Contracts.Common;

namespace Reswifty.API.Common;

public static class ApiResultHttp
{
    public static Ok<ApiResult<T>> Ok<T>(T data, string? traceId = null)
        => TypedResults.Ok(ApiResult<T>.Ok(data, traceId));

    public static Created<ApiResult<T>> Created<T>(string uri, T data, string? traceId = null)
        => TypedResults.Created(uri, ApiResult<T>.Ok(data, traceId));

    public static NoContent NoContent() => TypedResults.NoContent();

    public static NotFound<ApiResult<string>> NotFound(string msg, string? traceId = null)
        => TypedResults.NotFound(ApiResult<string>.Fail(msg, "not_found", traceId));

    public static Conflict<ApiResult<string>> Conflict(string msg, string? traceId = null)
        => TypedResults.Conflict(ApiResult<string>.Fail(msg, "concurrency_conflict", traceId));

    public static BadRequest<ApiResult<string>> Bad(string msg, string? traceId = null)
        => TypedResults.BadRequest(ApiResult<string>.Fail(msg, "bad_request", traceId));

    public static UnauthorizedHttpResult Unauthorized(string msg = "Unauthorized", string? traceId = null)
        => TypedResults.Unauthorized(); // body-less by spec; log msg/traceId if you want

    public static ForbidHttpResult Forbidden()
        => TypedResults.Forbid();

    // Convenience: map known exceptions to a consistent response
    public static IResult FromException(Exception ex, HttpContext ctx)
    {
        var traceId = ctx.TraceIdentifier;
        return ex.Message switch
        {
            "Invalid credentials." => Unauthorized(), // 401
            "Email not confirmed." => TypedResults.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                detail: ApiResult<string>.Fail(ex.Message, "email_not_confirmed", traceId).Error),
            _ => TypedResults.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: ApiResult<string>.Fail("Unexpected error", "server_error", traceId).Error)
        };
    }
}
