/// <summary>
/// InventoryHub Server Application
/// Provides a minimal API for product inventory management with performance optimizations
/// </summary>
/// <remarks>
/// Features:
/// - In-memory caching with 5-minute expiration
/// - Performance monitoring and logging
/// - CORS support for Blazor client
/// - Error handling and monitoring
/// </remarks>

using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Provides sample product data with nested category information
/// </summary>
/// <returns>Array of anonymous objects representing products with categories</returns>
/// <remarks>
/// This method simulates a database call and provides structured product data
/// Each product includes: Id, Name, Price, Stock, and Category (Id, Name)
/// </remarks>
static object[] GetProductData()
{
    return new object[]
    {
        new
        {
            Id = 1,
            Name = "Laptop",
            Price = 1200.50,
            Stock = 25,
            Category = new { Id = 101, Name = "Electronics" }
        },
        new
        {
            Id = 2,
            Name = "Headphones",
            Price = 50.00,
            Stock = 100,
            Category = new { Id = 102, Name = "Accessories" }
        },
        new
        {
            Id = 3,
            Name = "Mouse",
            Price = 25.99,
            Stock = 150,
            Category = new { Id = 102, Name = "Accessories" }
        },
        new
        {
            Id = 4,
            Name = "Keyboard",
            Price = 75.50,
            Stock = 80,
            Category = new { Id = 102, Name = "Accessories" }
        },
        new
        {
            Id = 5,
            Name = "Monitor",
            Price = 299.99,
            Stock = 45,
            Category = new { Id = 101, Name = "Electronics" }
        },
        new
        {
            Id = 6,
            Name = "Webcam",
            Price = 89.99,
            Stock = 75,
            Category = new { Id = 101, Name = "Electronics" }
        }
    };
}

// Configure application services and middleware

/// <summary>
/// Configure logging for performance monitoring and diagnostics
/// - Clears default providers
/// - Adds console logging
/// - Sets minimum log level to Information
/// </summary>
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

/// <summary>
/// Add in-memory caching service for performance optimization
/// Cache duration: 5 minutes absolute, 2 minutes sliding
/// </summary>
builder.Services.AddMemoryCache();

/// <summary>
/// Configure CORS policy for Blazor client
/// Allows specific origins with any headers and methods
/// </summary>
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy => policy.WithOrigins("http://localhost:5036", "https://localhost:5036")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Get logger for startup messages
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Enable CORS
app.UseCors("AllowBlazorClient");

// Log application startup
logger.LogInformation("InventoryHub ServerApp starting up - Performance monitoring enabled");

/// <summary>
/// Endpoint: GET /api/productlist
/// Retrieves a list of products with caching and performance monitoring
/// </summary>
/// <remarks>
/// Cache Strategy:
/// - 5 minute absolute expiration
/// - 2 minute sliding window
/// - Returns cached data when available
/// 
/// Performance Monitoring:
/// - Logs response time for cache hits and misses
/// - Tracks exceptions with timing information
/// 
/// Response Format:
/// Returns an array of product objects with categories
/// </remarks>
/// <response code="200">Returns the list of products</response>
/// <response code="500">If an error occurs during processing</response>
app.MapGet("/api/productlist", (IMemoryCache cache, ILogger<Program> logger) =>
{
    // Start performance monitoring for request timing
    var sw = Stopwatch.StartNew();
    const string cacheKey = "productlist";
    
    try
    {
        // Try to retrieve products from cache
        // Returns immediately if cache hit, avoiding database call
        if (cache.TryGetValue(cacheKey, out object[]? cachedProducts))
        {
            sw.Stop();
            logger.LogInformation("GET /api/productlist responded in {ElapsedMs} ms (CACHE HIT)", sw.ElapsedMilliseconds);
            return cachedProducts;
        }
        
        // Cache miss: Generate fresh product data
        // In production, this would be a database call
        logger.LogInformation("Cache miss - generating product data");
        var products = GetProductData();
        
        // Configure cache options:
        // - Absolute expiration: Data is invalid after 5 minutes
        // - Sliding expiration: Extend life by 2 minutes on each access
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };
        
        // Store generated data in cache for subsequent requests
        cache.Set(cacheKey, products, cacheOptions);
        
        sw.Stop();
        logger.LogInformation("GET /api/productlist responded in {ElapsedMs} ms (CACHE MISS - Data generated and cached)", sw.ElapsedMilliseconds);
        return products;
    }
    catch (Exception ex)
    {
        sw.Stop();
        logger.LogError(ex, "GET /api/productlist failed after {ElapsedMs} ms", sw.ElapsedMilliseconds);
        throw;
    }
});

logger.LogInformation("InventoryHub ServerApp configured and ready to serve requests");

app.Run();
