namespace Asas.Core.Paging;

public class PagedRequest
{
    private const int MaxPageSize = 100;

    private int _pageSize = 10;
    public int PageNumber { get; set; } = 0;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? SortBy { get; set; }
    public bool Descending { get; set; } = false;
}
