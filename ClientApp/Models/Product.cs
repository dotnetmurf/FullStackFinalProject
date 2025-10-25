using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models;

/// <summary>
/// Represents a product in the inventory system
/// </summary>
/// <remarks>
/// This class contains all essential product information including
/// basic details, pricing, inventory status, and categorization
/// </remarks>
public class Product
{
    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the product
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 200 characters")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Price of the product
    /// </summary>
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
    [Display(Name = "Price ($)")]
    public double Price { get; set; }

    /// <summary>
    /// Current stock level
    /// </summary>
    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    [Display(Name = "Stock Quantity")]
    public int Stock { get; set; }

    /// <summary>
    /// Category information for the product
    /// </summary>
    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public Category Category { get; set; } = new();

    /// <summary>
    /// Date and time when the product was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the product was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}