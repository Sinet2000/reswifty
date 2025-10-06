namespace Reswifty.API.Application.Common;

public record ApiResult<T>(bool Success, T? Data, string? Error = null, string? Code = null)
{
    public static ApiResult<T> Ok(T data) => new(true, data);
    public static ApiResult<T> Fail(string error, string? code = null) => new(false, default, error, code);
}

public record PagedResult<T>(int Page, int PageSize, int Total, IReadOnlyList<T> Items);