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
            // Original Products
            new() { Id = 1, Name = "Laptop", Price = 1200.50M, Stock = 25, Category = new Category { Id = 101, Name = "Electronics" } },
            new() { Id = 2, Name = "Headphones", Price = 50.00M, Stock = 100, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 3, Name = "Mouse", Price = 25.99M, Stock = 150, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 4, Name = "Keyboard", Price = 75.50M, Stock = 80, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 5, Name = "Monitor", Price = 299.99M, Stock = 45, Category = new Category { Id = 101, Name = "Electronics" } },
            new() { Id = 6, Name = "Webcam", Price = 89.99M, Stock = 75, Category = new Category { Id = 101, Name = "Electronics" } },

            // Additional Electronics (Category 101)
            new() { Id = 7, Name = "Smartphone", Price = 899.99M, Stock = 50, Category = new Category { Id = 101, Name = "Electronics" } },
            new() { Id = 8, Name = "Tablet", Price = 599.99M, Stock = 35, Category = new Category { Id = 101, Name = "Electronics" } },
            new() { Id = 9, Name = "Smart Watch", Price = 299.99M, Stock = 60, Category = new Category { Id = 101, Name = "Electronics" } },
            new() { Id = 10, Name = "Wireless Speaker", Price = 129.99M, Stock = 85, Category = new Category { Id = 101, Name = "Electronics" } },

            // Additional Accessories (Category 102)
            new() { Id = 11, Name = "Mouse Pad", Price = 15.99M, Stock = 200, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 12, Name = "USB Hub", Price = 29.99M, Stock = 120, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 13, Name = "Laptop Stand", Price = 45.99M, Stock = 90, Category = new Category { Id = 102, Name = "Accessories" } },
            new() { Id = 14, Name = "Cable Organizer", Price = 19.99M, Stock = 150, Category = new Category { Id = 102, Name = "Accessories" } },

            // Gaming (Category 103)
            new() { Id = 15, Name = "Gaming Console", Price = 499.99M, Stock = 30, Category = new Category { Id = 103, Name = "Gaming" } },
            new() { Id = 16, Name = "Gaming Controller", Price = 69.99M, Stock = 100, Category = new Category { Id = 103, Name = "Gaming" } },
            new() { Id = 17, Name = "Gaming Headset", Price = 129.99M, Stock = 75, Category = new Category { Id = 103, Name = "Gaming" } },
            new() { Id = 18, Name = "Gaming Chair", Price = 299.99M, Stock = 40, Category = new Category { Id = 103, Name = "Gaming" } },
            new() { Id = 19, Name = "Gaming Mouse", Price = 89.99M, Stock = 95, Category = new Category { Id = 103, Name = "Gaming" } },

            // Networking (Category 104)
            new() { Id = 20, Name = "Wireless Router", Price = 159.99M, Stock = 55, Category = new Category { Id = 104, Name = "Networking" } },
            new() { Id = 21, Name = "Network Switch", Price = 89.99M, Stock = 45, Category = new Category { Id = 104, Name = "Networking" } },
            new() { Id = 22, Name = "Wi-Fi Extender", Price = 49.99M, Stock = 70, Category = new Category { Id = 104, Name = "Networking" } },
            new() { Id = 23, Name = "Network Cable", Price = 12.99M, Stock = 200, Category = new Category { Id = 104, Name = "Networking" } },

            // Storage (Category 105)
            new() { Id = 24, Name = "External SSD", Price = 179.99M, Stock = 65, Category = new Category { Id = 105, Name = "Storage" } },
            new() { Id = 25, Name = "USB Flash Drive", Price = 29.99M, Stock = 180, Category = new Category { Id = 105, Name = "Storage" } },
            new() { Id = 26, Name = "Memory Card", Price = 39.99M, Stock = 120, Category = new Category { Id = 105, Name = "Storage" } },
            new() { Id = 27, Name = "Hard Drive Enclosure", Price = 25.99M, Stock = 90, Category = new Category { Id = 105, Name = "Storage" } },

            // Software (Category 106)
            new() { Id = 28, Name = "Antivirus Software", Price = 49.99M, Stock = 100, Category = new Category { Id = 106, Name = "Software" } },
            new() { Id = 29, Name = "Office Suite", Price = 199.99M, Stock = 85, Category = new Category { Id = 106, Name = "Software" } },
            new() { Id = 30, Name = "Design Software", Price = 599.99M, Stock = 40, Category = new Category { Id = 106, Name = "Software" } },
            new() { Id = 31, Name = "Development IDE", Price = 299.99M, Stock = 55, Category = new Category { Id = 106, Name = "Software" } },

            // Photography (Category 107)
            new() { Id = 32, Name = "Digital Camera", Price = 799.99M, Stock = 30, Category = new Category { Id = 107, Name = "Photography" } },
            new() { Id = 33, Name = "Camera Lens", Price = 599.99M, Stock = 25, Category = new Category { Id = 107, Name = "Photography" } },
            new() { Id = 34, Name = "Camera Tripod", Price = 79.99M, Stock = 60, Category = new Category { Id = 107, Name = "Photography" } },
            new() { Id = 35, Name = "Camera Bag", Price = 89.99M, Stock = 70, Category = new Category { Id = 107, Name = "Photography" } },
            new() { Id = 36, Name = "Memory Card Reader", Price = 29.99M, Stock = 85, Category = new Category { Id = 107, Name = "Photography" } }
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