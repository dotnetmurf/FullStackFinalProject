using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Optimized product data method with required nested Category structure
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

// Configure logging for performance monitoring
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add memory cache for performance optimization
builder.Services.AddMemoryCache();

// Add CORS services
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

app.MapGet("/api/productlist", (IMemoryCache cache, ILogger<Program> logger) =>
{
    // Start performance monitoring
    var sw = Stopwatch.StartNew();
    const string cacheKey = "productlist";
    
    try
    {
        // Check cache first
        if (cache.TryGetValue(cacheKey, out object[]? cachedProducts))
        {
            sw.Stop();
            logger.LogInformation("GET /api/productlist responded in {ElapsedMs} ms (CACHE HIT)", sw.ElapsedMilliseconds);
            return cachedProducts;
        }
        
        // Generate product data (simulate database call)
        logger.LogInformation("Cache miss - generating product data");
        var products = GetProductData();
        
        // Cache for 5 minutes
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

logger.LogInformation("InventoryHub ServerApp configured and ready to serve requests");

app.Run();
