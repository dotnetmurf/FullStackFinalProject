using ServerApp.Models;

namespace ServerApp.Services;

/// <summary>
/// Provides seed data for the InventoryHub application
/// </summary>
public static class SeedingService
{
    /// <summary>
    /// Gets sample product data with nested category information
    /// </summary>
    /// <returns>Array of sample products with categories</returns>
    public static Product[] GetSampleProducts()
    {
        return new Product[]
        {
            new()
            {
                Id = 1,
                Name = "Laptop",
                Price = 1200.50M,
                Stock = 25,
                Category = new Category { Id = 101, Name = "Electronics" }
            },
            new()
            {
                Id = 2,
                Name = "Headphones",
                Price = 50.00M,
                Stock = 100,
                Category = new Category { Id = 102, Name = "Accessories" }
            },
            new()
            {
                Id = 3,
                Name = "Mouse",
                Price = 25.99M,
                Stock = 150,
                Category = new Category { Id = 102, Name = "Accessories" }
            },
            new()
            {
                Id = 4,
                Name = "Keyboard",
                Price = 75.50M,
                Stock = 80,
                Category = new Category { Id = 102, Name = "Accessories" }
            },
            new()
            {
                Id = 5,
                Name = "Monitor",
                Price = 299.99M,
                Stock = 45,
                Category = new Category { Id = 101, Name = "Electronics" }
            },
            new()
            {
                Id = 6,
                Name = "Webcam",
                Price = 89.99M,
                Stock = 75,
                Category = new Category { Id = 101, Name = "Electronics" }
            }
        };
    }

    /// <summary>
    /// Gets all available categories from the sample data
    /// </summary>
    /// <returns>Array of unique categories</returns>
    public static Category[] GetCategories()
    {
        return GetSampleProducts()
            .Select(p => p.Category)
            .DistinctBy(c => c.Id)
            .ToArray();
    }
}