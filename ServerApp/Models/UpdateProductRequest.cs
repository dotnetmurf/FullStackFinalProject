using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models;

/// <summary>
/// Request model for updating an existing product with validation rules
/// </summary>
public class UpdateProductRequest
{
    /// <summary>
    /// Updated name of the product
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Updated description of the product
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Updated price of the product
    /// </summary>
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
    public decimal Price { get; set; }

    /// <summary>
    /// Updated stock quantity
    /// </summary>
    [Required(ErrorMessage = "Stock is required")]
    [Range(0, 999999, ErrorMessage = "Stock must be between 0 and 999,999")]
    public int Stock { get; set; }

    /// <summary>
    /// Updated category information
    /// </summary>
    [Required(ErrorMessage = "Category is required")]
    public Category Category { get; set; } = new();
}
