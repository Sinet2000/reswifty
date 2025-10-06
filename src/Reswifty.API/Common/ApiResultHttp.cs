using Microsoft.AspNetCore.Http.HttpResults;
using Reswifty.API.Application.Common;

namespace Reswifty.API.Common;

public static class ApiResultHttp
{
    public static Ok<ApiResult<T>> Ok<T>(T data) => TypedResults.Ok(ApiResult<T>.Ok(data));

    public static Created<ApiResult<T>> Created<T>(string uri, T data) => TypedResults.Created(uri, ApiResult<T>.Ok(data));

    public static NotFound<ApiResult<string>> NotFound(string msg) => TypedResults.NotFound(ApiResult<string>.Fail(msg, "not_found"));

    public static Conflict<ApiResult<string>> Conflict(string msg) =>
        TypedResults.Conflict(ApiResult<string>.Fail(msg, "concurrency_conflict"));

    public static BadRequest<ApiResult<string>> Bad(string msg) => TypedResults.BadRequest(ApiResult<string>.Fail(msg, "bad_request"));

    public static NoContent NoContent() => TypedResults.NoContent();
}