namespace Reswifty.API.Contracts.Common;

public sealed record ApiResult<T>(
    bool Success,
    T? Data,
    string? Error,
    string? Code,
    string? TraceId
)
{
    public static ApiResult<T> Ok(T data, string? traceId = null)
        => new(true, data, null, null, traceId);

    public static ApiResult<T> Fail(string message, string? code = null, string? traceId = null)
        => new(false, default, message, code, traceId);
}

public record PagedResult<T>(int Page, int PageSize, int Total, IReadOnlyList<T> Items);
