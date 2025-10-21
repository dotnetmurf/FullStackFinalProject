using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

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

// Enable CORS
app.UseCors("AllowBlazorClient");

app.MapGet("/api/productlist", (IMemoryCache cache) =>
{
    const string cacheKey = "productlist";
    
    // Check cache first
    if (cache.TryGetValue(cacheKey, out var cachedProducts))
    {
        return cachedProducts;
    }
    
    // Generate product data (simulate database call)
    var products = new[]
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
        }
    };
    
    // Cache for 5 minutes
    var cacheOptions = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    };
    
    cache.Set(cacheKey, products, cacheOptions);
    
    return products;
});

app.Run();
