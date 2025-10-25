using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Reflection;
using ServerApp.Models;

var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Configure services for the InventoryHub application
/// </summary>
/// <remarks>
/// Features:
/// - In-memory caching with 5-minute expiration
/// - Performance monitoring and logging
/// - CORS support for Blazor client
/// - Error handling and monitoring
/// </remarks>

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "InventoryHub API",
        Version = "v1",
        Description = "A minimal API for product inventory management",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@inventoryhub.com"
        }
    });

    // Enable XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Enable annotations
    options.EnableAnnotations();

    // Add security definition if needed
    /*
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    */
});

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

// Configure logging for performance monitoring and diagnostics
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add in-memory caching service for performance optimization
// Cache duration: 5 minutes absolute, 2 minutes sliding
builder.Services.AddMemoryCache();

// Configure CORS policy for Blazor client
// Allows specific origins with any headers and methods
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy => policy.WithOrigins("http://localhost:5037", "https://localhost:5037")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Get logger for startup messages
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Enable CORS
app.UseCors();

// Log application startup
logger.LogInformation("InventoryHub ServerApp starting up - Performance monitoring enabled");

/// <summary>
/// Returns an empty paginated list of products
/// </summary>
PaginatedList<Product> GetEmptyPaginatedList() => new()
{
    PageNumber = 1,
    PageSize = 10,
    TotalCount = 0,
    TotalPages = 0,
    Items = Array.Empty<Product>()
};

// GET /api/productlist - Retrieves all products with caching
// - Uses memory cache with 5-minute expiration
// - Implements performance monitoring
// - Returns paginated list of products with categories
app.MapGet("/api/productlist", (HttpContext context, IMemoryCache cache, ILogger<Program> logger) =>
{
    var sw = Stopwatch.StartNew();
    const string cacheKey = "productlist";
    
    try
    {
        if (cache.TryGetValue(cacheKey, out PaginatedList<Product>? cachedProducts))
        {
            sw.Stop();
            logger.LogInformation("GET /api/productlist responded in {ElapsedMs} ms (CACHE HIT)", sw.ElapsedMilliseconds);
            return Results.Ok(cachedProducts ?? GetEmptyPaginatedList());
        }
        
        logger.LogInformation("Cache miss - generating product data");
        var products = GetProductData();
        var paginatedList = new PaginatedList<Product>
        {
            Items = products,
            PageNumber = 1,
            PageSize = products.Length,
            TotalCount = products.Length,
            TotalPages = 1
        };
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };
        
        cache.Set(cacheKey, paginatedList, cacheOptions);
        
        sw.Stop();
        logger.LogInformation("GET /api/productlist responded in {ElapsedMs} ms (CACHE MISS - Data generated and cached)", sw.ElapsedMilliseconds);
        return Results.Ok(paginatedList);
    }
    catch (Exception ex)
    {
        sw.Stop();
        logger.LogError(ex, "GET /api/productlist failed after {ElapsedMs} ms", sw.ElapsedMilliseconds);
        throw;
    }
});

// GET /api/product/{id} - Retrieves a specific product by ID
// - Uses individual product caching (5-minute expiration)
// - Implements performance monitoring
// - Returns 200 with product, 404 if not found
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

// POST /api/product - Creates a new product
// - Validates input data
// - Updates cache to reflect new product
// - Returns 201 with created product, 400 if invalid
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

// PUT /api/product/{id} - Updates an existing product
// - Validates input data
// - Updates cache to reflect changes
// - Returns 200 with updated product, 400 if invalid, 404 if not found
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

// DELETE /api/product/{id} - Removes a product
// - Validates product exists
// - Updates cache to reflect deletion
// - Returns 204 on success, 404 if not found
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

// Configure static files first
app.UseStaticFiles();

// Configure Swagger and Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "InventoryHub API v1");
        options.RoutePrefix = string.Empty; // Set to empty to serve the Swagger UI at the root
        options.DocumentTitle = "InventoryHub API Documentation";
        options.EnableTryItOutByDefault();
        options.DisplayRequestDuration();
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
    });
}

logger.LogInformation("InventoryHub ServerApp configured and ready to serve requests");

app.Run();
