namespace ClientApp.Models;

public class GetProductsParams
{
    /// <summary>
    /// Gets or sets the page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the search term to filter products
    /// </summary>
    public string? SearchTerm { get; set; }
}