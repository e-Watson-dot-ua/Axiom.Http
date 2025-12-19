using Axiom.Primitives.Guards;

namespace Axiom.Http.Responses;

/// <summary>
/// Represents a paginated response containing a subset of data and pagination metadata.
/// </summary>
/// <typeparam name="TData">The type of data items in the response.</typeparam>
public sealed class PagedResponse<TData>
{
    /// <summary>
    /// Gets the collection of data items for the current page.
    /// </summary>
    public IReadOnlyList<TData> Data { get; init; } = Array.Empty<TData>();

    /// <summary>
    /// Gets the pagination metadata.
    /// </summary>
    public PageMetadata Metadata { get; init; } = PageMetadata.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedResponse{TData}"/> class.
    /// </summary>
    private PagedResponse() { }

    /// <summary>
    /// Creates a new paged response.
    /// </summary>
    /// <param name="data">The data items for the current page.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <returns>A new <see cref="PagedResponse{TData}"/> instance.</returns>
    public static PagedResponse<TData> Create(IReadOnlyList<TData> data,
        int pageNumber, int pageSize, int totalCount)
    {
        Guard.Against.Null(data, nameof(data));

        return new PagedResponse<TData>
        {
            Data = data,
            Metadata = PageMetadata.Create(pageNumber, pageSize, totalCount)
        };
    }

    /// <summary>
    /// Creates a new paged response from an <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static PagedResponse<TData> Create(IEnumerable<TData> data,
        int pageNumber, int pageSize, int totalCount)
    {
        Guard.Against.Null(data, nameof(data));

        // Materialize to avoid multiple enumeration and to satisfy IReadOnlyList.
        var list = data as IReadOnlyList<TData> ?? data.ToArray();

        return Create(list, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Creates a new paged response and optionally clamps the page number to the available range.
    /// Useful when the requested page can be out of range.
    /// </summary>
    public static PagedResponse<TData> CreateClamped(IReadOnlyList<TData> data,
        int pageNumber, int pageSize, int totalCount)
    {
        Guard.Against.Null(data, nameof(data));
        Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));
        Guard.Against.Negative(totalCount, nameof(totalCount));

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var clampedPageNumber = totalPages == 0 ? 1 : Math.Min(Math.Max(pageNumber, 1), totalPages);

        return new PagedResponse<TData>
        {
            Data = data,
            Metadata = PageMetadata.Create(clampedPageNumber, pageSize, totalCount)
        };
    }

    /// <summary>
    /// Creates an empty paged response.
    /// </summary>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>An empty <see cref="PagedResponse{TData}"/> instance.</returns>
    public static PagedResponse<TData> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PagedResponse<TData>
        {
            Data = [],
            Metadata = PageMetadata.Create(pageNumber, pageSize, 0)
        };
    }
}

/// <summary>
/// Represents pagination metadata.
/// </summary>
public sealed class PageMetadata
{
    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Gets the number of items in the current page.
    /// </summary>
    public int CurrentPageSize { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageMetadata"/> class.
    /// </summary>
    private PageMetadata() { }

    /// <summary>
    /// Creates pagination metadata.
    /// </summary>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <returns>A new <see cref="PageMetadata"/> instance.</returns>
    public static PageMetadata Create(int pageNumber, int pageSize, int totalCount)
    {
        Guard.Against.NegativeOrZero(pageNumber, nameof(pageNumber));
        Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));
        Guard.Against.Negative(totalCount, nameof(totalCount));

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var currentPageSize = Math.Min(pageSize, Math.Max(0, totalCount - (pageNumber - 1) * pageSize));

        return new PageMetadata
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPageSize = currentPageSize
        };
    }

    /// <summary>
    /// Empty pagination metadata.
    /// </summary>
    public static readonly PageMetadata Empty = Create(pageNumber: 1, pageSize: 10, totalCount: 0);
}
