/// <summary>
/// Blazor WebAssembly Client Application Entry Point
/// Sets up the client-side application infrastructure and DI container
/// </summary>
/// <remarks>
/// Configures:
/// - Root components (App and HeadOutlet)
/// - HTTP client with base address
/// - Scoped services (ProductService)
/// </remarks>

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ClientApp;

// Create the WebAssembly host builder with default configuration
var builder = WebAssemblyHostBuilder.CreateDefault(args);
// Register root components for the application
builder.RootComponents.Add<App>("#app");                    // Main application component
builder.RootComponents.Add<HeadOutlet>("head::after");      // Head content management

// Configure services for dependency injection
builder.Services.AddScoped(sp => new HttpClient 
{ 
    // Configure HttpClient with the application's base address
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// Register the ProductService as a scoped service
// This ensures one instance per user session
builder.Services.AddScoped<ClientApp.Services.ProductService>();

// Build and run the application
await builder.Build().RunAsync();
