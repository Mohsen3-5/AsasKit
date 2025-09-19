using Asas.Core.Paging;
using Microsoft.EntityFrameworkCore;
using EFF = Microsoft.EntityFrameworkCore.EF;

namespace Asas.Core.Exceptions;

public static class IQueryableExtensions
{
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        PagedRequest request,
        CancellationToken ct = default)
    {
        // Safety guards
        if (request.PageNumber < 0) request.PageNumber = 0;
        if (request.PageSize < 1) request.PageSize = 10;


        // Apply sorting if provided
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var propertyName = char.ToUpper(request.SortBy[0]) + request.SortBy.Substring(1);
            query = request.Descending
                ? query.OrderByDescending(e => EFF.Property<object>(e, propertyName))
                : query.OrderBy(e => EFF.Property<object>(e, propertyName));
        }
        else
        {
            query = query.OrderBy(e => EFF.Property<object>(e, "CreatedAtUtc")); // ✅ fallback
        }


        var count = await query.CountAsync(ct);

        var items = await query
            .Skip(request.PageNumber * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResponse<T>(items, count, request.PageNumber, request.PageSize);
    }
}
