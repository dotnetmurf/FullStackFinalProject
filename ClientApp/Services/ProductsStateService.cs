namespace ClientApp.Services;

/// <summary>
/// Service for managing Products page state across navigation
/// </summary>
/// <remarks>
/// Maintains user preferences like page size and current page number
/// so they persist when navigating away and returning to the Products page
/// </remarks>
public class ProductsStateService
{
    /// <summary>
    /// Gets or sets the user's selected page size
    /// </summary>
    /// <remarks>
    /// Default is 12 items per page
    /// </remarks>
    public int PageSize { get; set; } = 12;

    /// <summary>
    /// Gets or sets the current page number
    /// </summary>
    /// <remarks>
    /// Default is page 1
    /// </remarks>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Event that fires when the state changes
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Updates the page size and notifies subscribers
    /// </summary>
    /// <param name="pageSize">New page size</param>
    public void SetPageSize(int pageSize)
    {
        PageSize = pageSize;
        PageNumber = 1; // Reset to page 1 when page size changes
        NotifyStateChanged();
    }

    /// <summary>
    /// Updates the page number and notifies subscribers
    /// </summary>
    /// <param name="pageNumber">New page number</param>
    public void SetPageNumber(int pageNumber)
    {
        PageNumber = pageNumber;
        NotifyStateChanged();
    }

    /// <summary>
    /// Notifies all subscribers that the state has changed
    /// </summary>
    private void NotifyStateChanged() => OnChange?.Invoke();
}
