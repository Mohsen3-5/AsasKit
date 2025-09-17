using Asas.Core.Paging;
using Microsoft.EntityFrameworkCore;

namespace Asas.Core.Exceptions;
public static class IQueryableExtensions
{
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var count = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<T>(items, count, pageNumber, pageSize);
    }
}
