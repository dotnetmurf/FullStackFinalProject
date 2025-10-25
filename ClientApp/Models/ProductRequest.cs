namespace ClientApp.Models;

public class ProductRequest
{
    /// <summary>
    /// Gets or sets the page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the search term
    /// </summary>
    public string? SearchTerm { get; set; }
}