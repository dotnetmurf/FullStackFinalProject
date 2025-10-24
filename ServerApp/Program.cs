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
using ServerApp.Models;

var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Provides sample product data with nested category information
/// </summary>
/// <returns>Array of anonymous objects representing products with categories</returns>
/// <remarks>
/// This method simulates a database call and provides structured product data
/// Each product includes: Id, Name, Price, Stock, and Category (Id, Name)
/// </remarks>
static Product[] GetProductData()
{
    return new Product[]
    {
        new()
        {
            Id = 1,
            Name = "Laptop",
            Price = 1200.50,
            Stock = 25,
            Category = new Category { Id = 101, Name = "Electronics" }
        },
        new()
        {
            Id = 2,
            Name = "Headphones",
            Price = 50.00,
            Stock = 100,
            Category = new Category { Id = 102, Name = "Accessories" }
        },
        new()
        {
            Id = 3,
            Name = "Mouse",
            Price = 25.99,
            Stock = 150,
            Category = new Category { Id = 102, Name = "Accessories" }
        },
        new()
        {
            Id = 4,
            Name = "Keyboard",
            Price = 75.50,
            Stock = 80,
            Category = new Category { Id = 102, Name = "Accessories" }
        },
        new()
        {
            Id = 5,
            Name = "Monitor",
            Price = 299.99,
            Stock = 45,
            Category = new Category { Id = 101, Name = "Electronics" }
        },
        new()
        {
            Id = 6,
            Name = "Webcam",
            Price = 89.99,
            Stock = 75,
            Category = new Category { Id = 101, Name = "Electronics" }
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
/// <summary>
/// GET /api/productlist - Retrieves all products with caching
/// </summary>
app.MapGet("/api/productlist", (IMemoryCache cache, ILogger<Program> logger) =>
{
    var sw = Stopwatch.StartNew();
    const string cacheKey = "productlist";
    
    try
    {
        if (cache.TryGetValue(cacheKey, out object[]? cachedProducts))
        {
            sw.Stop();
            logger.LogInformation("GET /api/productlist responded in {ElapsedMs} ms (CACHE HIT)", sw.ElapsedMilliseconds);
            return cachedProducts;
        }
        
        logger.LogInformation("Cache miss - generating product data");
        var products = GetProductData();
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };
        
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

/// <summary>
/// GET /api/product/{id} - Retrieves a specific product by ID with caching
/// </summary>
/// <remarks>
/// Cache Strategy:
/// - Individual product caching
/// - 5 minute absolute expiration
/// - 2 minute sliding window
/// Performance Monitoring:
/// - Logs response time and cache status
/// </remarks>
/// <response code="200">Returns the requested product</response>
/// <response code="404">If product not found</response>
/// <response code="500">If an error occurs during processing</response>
app.MapGet("/api/product/{id}", (int id, IMemoryCache cache, ILogger<Program> logger) =>
{
    var sw = Stopwatch.StartNew();
    var cacheKey = $"product_{id}";
    
    try
    {
        if (cache.TryGetValue(cacheKey, out Product? cachedProduct))
        {
            sw.Stop();
            logger.LogInformation("GET /api/product/{Id} responded in {ElapsedMs} ms (CACHE HIT)", id, sw.ElapsedMilliseconds);
            return cachedProduct is not null ? Results.Ok(cachedProduct) : Results.NotFound();
        }
        
        logger.LogInformation("Cache miss - retrieving product {Id}", id);
        var product = GetProductData().FirstOrDefault(p => p.Id == id);
        
        if (product == null)
        {
            sw.Stop();
            logger.LogInformation("GET /api/product/{Id} - Product not found", id);
            return Results.NotFound();
        }
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };
        
        cache.Set(cacheKey, product, cacheOptions);
        
        sw.Stop();
        logger.LogInformation("GET /api/product/{Id} responded in {ElapsedMs} ms (CACHE MISS)", id, sw.ElapsedMilliseconds);
        return Results.Ok(product);
    }
    catch (Exception ex)
    {
        sw.Stop();
        logger.LogError(ex, "GET /api/product/{Id} failed after {ElapsedMs} ms", id, sw.ElapsedMilliseconds);
        throw;
    }
});

/// <summary>
/// POST /api/product - Creates a new product
/// </summary>
/// <remarks>
/// - Validates input data
/// - Updates cache to reflect new product
/// - Logs creation details
/// </remarks>
/// <response code="201">Returns the created product</response>
/// <response code="400">If the product data is invalid</response>
/// <response code="500">If an error occurs during processing</response>
app.MapPost("/api/product", (CreateProductRequest request, IMemoryCache cache, ILogger<Program> logger) =>
{
    var sw = Stopwatch.StartNew();
    
    try
    {
        // Validate required fields
        if (string.IsNullOrEmpty(request.Name))
        {
            sw.Stop();
            logger.LogWarning("POST /api/product - Invalid product data provided: Name is required");
            return Results.BadRequest("Name is required");
        }
        
        // Simulate database insert by adding to existing data
        var products = GetProductData().ToList();
        int newId = products.Max(p => p.Id) + 1;
        
        var newProduct = new Product
        {
            Id = newId,
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category
        };
        
        // Invalidate relevant caches
        cache.Remove("productlist");
        
        sw.Stop();
        logger.LogInformation("POST /api/product created product {Id} in {ElapsedMs} ms", newId, sw.ElapsedMilliseconds);
        return Results.Created($"/api/product/{newId}", newProduct);
    }
    catch (Exception ex)
    {
        sw.Stop();
        logger.LogError(ex, "POST /api/product failed after {ElapsedMs} ms", sw.ElapsedMilliseconds);
        throw;
    }
});

/// <summary>
/// PUT /api/product/{id} - Updates an existing product
/// </summary>
/// <remarks>
/// - Validates input data
/// - Updates cache to reflect changes
/// - Logs update details
/// </remarks>
/// <response code="200">Returns the updated product</response>
/// <response code="400">If the product data is invalid</response>
/// <response code="404">If product not found</response>
/// <response code="500">If an error occurs during processing</response>
app.MapPut("/api/product/{id}", (int id, UpdateProductRequest request, IMemoryCache cache, ILogger<Program> logger) =>
{
    var sw = Stopwatch.StartNew();
    
    try
    {
        var products = GetProductData();
        var existingProduct = products.FirstOrDefault(p => p.Id == id);
        
        if (existingProduct == null)
        {
            sw.Stop();
            logger.LogWarning("PUT /api/product/{Id} - Product not found", id);
            return Results.NotFound();
        }
        
        if (string.IsNullOrEmpty(request.Name))
        {
            sw.Stop();
            logger.LogWarning("PUT /api/product/{Id} - Invalid product data provided: Name is required", id);
            return Results.BadRequest("Name is required");
        }
        
        var updatedProduct = new Product
        {
            Id = id,
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category
        };
        
        // Invalidate relevant caches
        cache.Remove("productlist");
        cache.Remove($"product_{id}");
        
        sw.Stop();
        logger.LogInformation("PUT /api/product/{Id} updated in {ElapsedMs} ms", id, sw.ElapsedMilliseconds);
        return Results.Ok(updatedProduct);
    }
    catch (Exception ex)
    {
        sw.Stop();
        logger.LogError(ex, "PUT /api/product/{Id} failed after {ElapsedMs} ms", id, sw.ElapsedMilliseconds);
        throw;
    }
});

/// <summary>
/// DELETE /api/product/{id} - Removes a product
/// </summary>
/// <remarks>
/// - Validates product exists
/// - Updates cache to reflect deletion
/// - Logs deletion details
/// </remarks>
/// <response code="204">If product was successfully deleted</response>
/// <response code="404">If product not found</response>
/// <response code="500">If an error occurs during processing</response>
app.MapDelete("/api/product/{id}", (int id, IMemoryCache cache, ILogger<Program> logger) =>
{
    var sw = Stopwatch.StartNew();
    
    try
    {
        var products = GetProductData();
        var existingProduct = products.FirstOrDefault(p => ((dynamic)p).Id == id);
        
        if (existingProduct == null)
        {
            sw.Stop();
            logger.LogWarning("DELETE /api/product/{Id} - Product not found", id);
            return Results.NotFound();
        }
        
        // Invalidate relevant caches
        cache.Remove("productlist");
        cache.Remove($"product_{id}");
        
        sw.Stop();
        logger.LogInformation("DELETE /api/product/{Id} completed in {ElapsedMs} ms", id, sw.ElapsedMilliseconds);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        sw.Stop();
        logger.LogError(ex, "DELETE /api/product/{Id} failed after {ElapsedMs} ms", id, sw.ElapsedMilliseconds);
        throw;
    }
});

logger.LogInformation("InventoryHub ServerApp configured and ready to serve requests");

app.Run();
