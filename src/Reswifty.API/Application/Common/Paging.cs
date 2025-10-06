namespace Reswifty.API.Application.Common;

public static class Paging
{
    public static (int page, int pageSize) Coerce(int page, int pageSize, int max = 200)
        => (Math.Max(0, page), Math.Clamp(pageSize == 0 ? 20 : pageSize, 1, max));
}