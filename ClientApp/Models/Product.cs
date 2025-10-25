using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models;

/// <summary>
/// Represents a product in the inventory system
/// </summary>
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
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Price of the product
    /// </summary>
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
    public double Price { get; set; }

    /// <summary>
    /// Current stock level
    /// </summary>
    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    public int Stock { get; set; }

    /// <summary>
    /// Category information for the product
    /// </summary>
    [Required(ErrorMessage = "Category is required")]
    public Category? Category { get; set; }
}