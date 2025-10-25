using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ClientApp;
using ClientApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient with base address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5118") });

// Add memory cache
builder.Services.AddMemoryCache();

// Register services
builder.Services.AddScoped<IProductService, ProductService>();

// Add logging
builder.Services.AddLogging();

await builder.Build().RunAsync();
