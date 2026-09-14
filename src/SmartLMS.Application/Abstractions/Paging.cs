using Microsoft.EntityFrameworkCore;

namespace SmartLMS.Application.Abstractions;

/// <summary>Uniform paging envelope returned by list endpoints.</summary>
public sealed record Paged<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)Total / PageSize);
}

public static class Paging
{
    public const int DefaultPageSize = 25;

    /// <summary>Normalizes a page/pageSize pair for an unpaged (all-rows) request.</summary>
    private static (bool paged, int size, int currentPage, int skip) Normalize(int? page, int? pageSize)
    {
        if (page is null || pageSize is null)
        {
            return (false, 0, 1, 0);
        }

        var size = pageSize.Value <= 0 ? DefaultPageSize : pageSize.Value;
        var currentPage = page.Value <= 0 ? 1 : page.Value;
        return (true, size, currentPage, (currentPage - 1) * size);
    }

    /// <summary>
    /// Materializes the source with optional paging. When <paramref name="page"/> or
    /// <paramref name="pageSize"/> is null, all rows are returned unpaged (single page).
    /// </summary>
    public static async Task<Paged<T>> PageAsync<T>(
        this IQueryable<T> source,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var total = await source.CountAsync(cancellationToken);
        var (paged, size, currentPage, skip) = Normalize(page, pageSize);
        if (!paged)
        {
            var items = await source.ToListAsync(cancellationToken);
            return new Paged<T>(items, total, 1, items.Count);
        }

        var pagedItems = await source.Skip(skip).Take(size).ToListAsync(cancellationToken);
        return new Paged<T>(pagedItems, total, currentPage, size);
    }

    /// <summary>Paging helper for data already materialized in memory.</summary>
    public static Paged<T> PageInMemory<T>(this IReadOnlyList<T> source, int? page, int? pageSize)
    {
        var total = source.Count;
        var (paged, size, currentPage, skip) = Normalize(page, pageSize);
        if (!paged)
        {
            return new Paged<T>(source, total, 1, total);
        }

        var items = source.Skip(skip).Take(size).ToList();
        return new Paged<T>(items, total, currentPage, size);
    }
}