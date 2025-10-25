using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Reflection;
using ServerApp.Data;
using ServerApp.Models;
using ServerApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services for the InventoryHub application
// Features:
// - In-memory caching with 5-minute expiration
// - Performance monitoring and logging
// - CORS support for Blazor client
// - Error handling and monitoring

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Add Entity Framework Core with InMemory provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("InventoryHub"));

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
        policy => policy.WithOrigins("http://localhost:5019", "https://localhost:7253")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Get logger for startup messages
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Initialize the database with seed data
await DbInitializerService.InitializeAsync(app.Services);

// Enable CORS
app.UseCors();

// Log application startup
logger.LogInformation("InventoryHub ServerApp starting up - Performance monitoring enabled");

// Returns an empty paginated list of products
PaginatedList<Product> GetEmptyPaginatedList() => new()
{
    PageNumber = 1,
    PageSize = 10,
    TotalCount = 0,
    TotalPages = 0,
    Items = Array.Empty<Product>()
};

// GET /api/products - Retrieves all products with caching
// - Uses memory cache with 5-minute expiration
// - Implements performance monitoring
// - Returns paginated list of products with categories
app.MapGet("/api/products", async (HttpContext context, IMemoryCache cache, ILogger<Program> logger) =>
{
    var sw = Stopwatch.StartNew();
    const string cacheKey = "products";
    
    try
    {
        if (cache.TryGetValue(cacheKey, out PaginatedList<Product>? cachedProducts))
        {
            sw.Stop();
            logger.LogInformation("GET /api/products responded in {ElapsedMs} ms (CACHE HIT)", sw.ElapsedMilliseconds);
            return Results.Ok(cachedProducts ?? GetEmptyPaginatedList());
        }
        
        logger.LogInformation("Cache miss - retrieving products from database");
        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
        var products = await dbContext.Products.ToListAsync();
        var paginatedList = new PaginatedList<Product>
        {
            Items = products,
            PageNumber = 1,
            PageSize = products.Count,
            TotalCount = products.Count,
            TotalPages = 1
        };
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };
        
        cache.Set(cacheKey, paginatedList, cacheOptions);
        
        sw.Stop();
        logger.LogInformation("GET /api/products responded in {ElapsedMs} ms (CACHE MISS - Data generated and cached)", sw.ElapsedMilliseconds);
        return Results.Ok(paginatedList);
    }
    catch (Exception ex)
    {
        sw.Stop();
        logger.LogError(ex, "GET /api/products failed after {ElapsedMs} ms", sw.ElapsedMilliseconds);
        throw;
    }
});

// GET /api/product/{id} - Retrieves a specific product by ID
// - Uses individual product caching (5-minute expiration)
// - Implements performance monitoring
// - Returns 200 with product, 404 if not found
app.MapGet("/api/product/{id}", async (int id, HttpContext context, IMemoryCache cache, ILogger<Program> logger) =>
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
        
        logger.LogInformation("Cache miss - retrieving product {Id} from database", id);
        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
        var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        
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
app.MapPost("/api/product", async (CreateProductRequest request, HttpContext context, IMemoryCache cache, ILogger<Program> logger) =>
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
        
        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
        var maxId = await dbContext.Products.MaxAsync(p => p.Id);
        int newId = maxId + 1;
        
        var newProduct = new Product
        {
            Id = newId,
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category
        };
        
        await dbContext.Products.AddAsync(newProduct);
        await dbContext.SaveChangesAsync();
        
        // Invalidate relevant caches
        cache.Remove("products");
        
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
app.MapPut("/api/product/{id}", async (int id, UpdateProductRequest request, HttpContext context, IMemoryCache cache, ILogger<Program> logger) =>
{
    var sw = Stopwatch.StartNew();
    
    try
    {
        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
        var existingProduct = await dbContext.Products.FindAsync(id);
        
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
        
        existingProduct.Name = request.Name;
        existingProduct.Price = request.Price;
        existingProduct.Stock = request.Stock;
        existingProduct.Category = request.Category;
        
        await dbContext.SaveChangesAsync();
        
        // Invalidate relevant caches
        cache.Remove("products");
        cache.Remove($"product_{id}");
        
        sw.Stop();
        logger.LogInformation("PUT /api/product/{Id} updated in {ElapsedMs} ms", id, sw.ElapsedMilliseconds);
        return Results.Ok(existingProduct);
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
app.MapDelete("/api/product/{id}", async (int id, HttpContext context, IMemoryCache cache, ILogger<Program> logger) =>
{
    var sw = Stopwatch.StartNew();
    
    try
    {
        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
        var existingProduct = await dbContext.Products.FindAsync(id);
        
        if (existingProduct == null)
        {
            sw.Stop();
            logger.LogWarning("DELETE /api/product/{Id} - Product not found", id);
            return Results.NotFound();
        }
        
        dbContext.Products.Remove(existingProduct);
        await dbContext.SaveChangesAsync();
        
        // Invalidate relevant caches
        cache.Remove("products");
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
