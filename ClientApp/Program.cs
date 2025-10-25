/// <summary>
/// Blazor WebAssembly Client Application Entry Point
/// Sets up the client-side application infrastructure and DI container
/// </summary>
/// <remarks>
/// Configures:
/// - Root components (App and HeadOutlet)
/// - HTTP client with base address
/// - Memory cache
/// - Logging services
/// - Scoped services (ProductService)
/// </remarks>

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using ClientApp;
using ClientApp.Services;
using System.Net.Http.Json;

// Create the WebAssembly host builder with default configuration
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register root components for the application
builder.RootComponents.Add<App>("#app");                    // Main application component
builder.RootComponents.Add<HeadOutlet>("head::after");      // Head content management

// Load configuration
var apiBaseUrl = builder.Configuration.GetValue<string>("API:BaseUrl") ?? "http://localhost:5132";
var cacheExpiration = builder.Configuration.GetValue<int>("Cache:DefaultExpirationMinutes", 5);

// Configure services for dependency injection
// Configure HttpClient with base URL and default timeout
builder.Services.AddScoped(sp => 
{
    var client = new HttpClient 
    { 
        BaseAddress = new Uri(apiBaseUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };
    
    // Add default headers
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    
    return client;
});

// Add memory cache service with default expiration
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024;
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
});

// Add logging services
builder.Services.AddLogging(logging => 
{
    logging.SetMinimumLevel(LogLevel.Information);
    
    #if DEBUG
    logging.SetMinimumLevel(LogLevel.Debug);
    #endif
});

// Register the ProductService as a scoped service
// This ensures one instance per user session
builder.Services.AddScoped<ProductService>();

// Build and run the application
await builder.Build().RunAsync();
