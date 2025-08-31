namespace Asas.Core.Paging;
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int Size)
{
    public static PagedResult<T> Empty(int page = 1, int size = 20) =>
        new(Array.Empty<T>(), 0, page, size);
}
