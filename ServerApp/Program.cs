using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Concurrent;
using ServerApp.Data;
using ServerApp.Endpoints;
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

// Register CacheService for centralized cache management
builder.Services.AddSingleton<CacheService>();

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

// ============================================
// Product Endpoints (extracted to ProductEndpoints.cs)
// ============================================
app.MapProductEndpoints();

// ============================================
// Category Endpoints
// ============================================

// GET /api/categories - Retrieves all available categories
// - Returns distinct categories from seed data
// - Useful for dropdown selectors in forms
app.MapGet("/api/categories", (ILogger<Program> logger) =>
{
    var sw = Stopwatch.StartNew();
    
    try
    {
        var categories = SeedingService.GetCategories();
        sw.Stop();
        logger.LogInformation("GET /api/categories responded in {ElapsedMs} ms with {Count} categories", 
            sw.ElapsedMilliseconds, categories.Length);
        return Results.Ok(categories);
    }
    catch (Exception ex)
    {
        sw.Stop();
        logger.LogError(ex, "GET /api/categories failed after {ElapsedMs} ms", sw.ElapsedMilliseconds);
        throw;
    }
})
.WithName("GetCategories")
.WithTags("Categories")
.Produces<Category[]>(StatusCodes.Status200OK);

// ============================================
// Application Configuration
// ============================================

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
