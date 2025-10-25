using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models;

/// <summary>
/// Represents a paginated list of items
/// </summary>
/// <typeparam name="T">The type of items in the list</typeparam>
/// <remarks>
/// This class implements pagination functionality with validation
/// to ensure proper page size and number values
/// </remarks>
public class PaginatedList<T>
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    private int _pageNumber = 1;

    /// <summary>
    /// The current page number
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = Math.Max(1, value);
    }

    /// <summary>
    /// The number of items per page
    /// </summary>
    [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    /// <summary>
    /// The total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// The items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// Get the SQL-style skip value for the current page
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;
}