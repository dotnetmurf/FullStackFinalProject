# Phase 1: Writing Integration Code - InventoryHub

## Overview
Build the integration between a Blazor WebAssembly front-end and a Minimal API back-end for the InventoryHub inventory management system. This phase establishes seamless communication between the client and server components.

---

## Step 1: Set Up the Base Application

### 1.1 Create the Project Structure

Navigate to your root folder and execute the following commands:

```powershell
# Create application folder
mkdir FullStackApp
cd FullStackApp

# Create Client and Server applications
dotnet new blazorwasm -n ClientApp
dotnet new webapi -n ServerApp

# Create solution and add projects
dotnet new sln -n FullStackSolution
dotnet sln add ClientApp ServerApp
```

### 1.2 Configure the ServerApp Back-End

Replace the code in `ServerApp/Program.cs` with the following:

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/api/products", () =>
{
    return new[]
    {
        new { Id = 1, Name = "Laptop", Price = 1200.50, Stock = 25 },
        new { Id = 2, Name = "Headphones", Price = 50.00, Stock = 100 }
    };
});

app.Run();
```

### 1.3 Launch and Verify Applications

1. Open two terminal windows in VS Code
2. **Terminal 1 (ClientApp)**:
   ```powershell
   cd ClientApp
   dotnet run
   ```
3. **Terminal 2 (ServerApp)**:
   ```powershell
   cd ServerApp
   dotnet run
   ```
4. Test the ServerApp by navigating to: `http://localhost:[port]/api/products`
   - Verify JSON product data is returned
5. Open the ClientApp in your browser
   - Note: Data will not display yet (integration pending)

---

## Step 2: Generate Integration Code

### 2.1 Create the FetchProducts Component

In the ClientApp project, create `Pages/FetchProducts.razor` with the following starter code:

```razor
@page "/fetchproducts"
@inject HttpClient Http

<h3>Product List</h3>

<ul>
   @if (products != null)
    {
        foreach (var product in products)
        {
            <li>@product.Name - $@product.Price</li>
        }
    }
    else
    {
        <li>Loading...</li>
    }
</ul>

@code {
    private Product[]? products;
    
    protected override async Task OnInitializedAsync()
    {
        // API call logic will go here
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }
    }
}
```

### 2.2 Implement API Call Logic

Replace the `OnInitializedAsync` method with the following implementation:

```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        // Call the back-end API
        products = await Http.GetFromJsonAsync<Product[]>("http://localhost:5132/api/products");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching products: {ex.Message}");
    }
}
```

**Important**: Replace `[SERVER_PORT]` with the actual port number from your ServerApp (check the terminal output).

---

## Step 3: Refine and Test the Integration

### 3.1 Add Enhanced Error Handling

Improve the API call logic with comprehensive error handling:

```csharp
@code {
    private Product[]? products;
    private string? errorMessage;
    private bool isLoading = true;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = null;
            
            // Configure HttpClient with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            
            // Call the back-end API
            products = await Http.GetFromJsonAsync<Product[]>(
                "http://localhost:5132/api/products", 
                cts.Token);
                
            if (products == null || products.Length == 0)
            {
                errorMessage = "No products available.";
            }
        }
        catch (HttpRequestException ex)
        {
            errorMessage = $"Network error: {ex.Message}";
            Console.WriteLine($"HTTP Request Error: {ex}");
        }
        catch (TaskCanceledException)
        {
            errorMessage = "Request timed out. Please try again.";
        }
        catch (Exception ex)
        {
            errorMessage = $"Unexpected error: {ex.Message}";
            Console.WriteLine($"Error fetching products: {ex}");
        }
        finally
        {
            isLoading = false;
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }
    }
}
```

### 3.2 Update the UI to Display Error Messages

Update the HTML section of `FetchProducts.razor`:

```razor
@page "/fetchproducts"
@inject HttpClient Http

<h3>Product List</h3>

@if (isLoading)
{
    <p><em>Loading products...</em></p>
}
else if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger" role="alert">
        @errorMessage
    </div>
}
else if (products != null && products.Length > 0)
{
    <ul>
        @foreach (var product in products)
        {
            <li>
                <strong>@product.Name</strong> - $@product.Price.ToString("F2")
                <span class="badge bg-secondary">Stock: @product.Stock</span>
            </li>
        }
    </ul>
}
else
{
    <p>No products available.</p>
}
```

### 3.3 Configure CORS in ServerApp (if needed)

If you encounter CORS errors, add CORS configuration to `ServerApp/Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy => policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowBlazorClient");

app.MapGet("/api/products", () =>
{
    return new[]
    {
        new { Id = 1, Name = "Laptop", Price = 1200.50, Stock = 25 },
        new { Id = 2, Name = "Headphones", Price = 50.00, Stock = 100 }
    };
});

app.Run();
```

**Note**: Update the origins to match your ClientApp's actual URLs.

### 3.4 Test the Integration

1. Ensure both applications are running
2. Navigate to `http://localhost:[CLIENT_PORT]/fetchproducts`
3. Verify that:
   - Product data displays correctly
   - Prices are formatted properly
   - Stock information appears
   - Error messages display if the server is unavailable
4. Test error scenarios:
   - Stop the ServerApp and refresh the page
   - Verify the error message displays
   - Restart ServerApp and confirm data loads again

---

## Best Practices Implemented

✅ **Error Handling**: Comprehensive try-catch blocks for different error types  
✅ **Timeout Management**: 10-second timeout prevents hanging requests  
✅ **User Feedback**: Clear loading and error states  
✅ **Null Safety**: Proper null checks and nullable reference types  
✅ **Code Readability**: Well-structured code with clear variable names  
✅ **Type Safety**: Strongly-typed Product class  
✅ **CORS Configuration**: Secure cross-origin resource sharing  

---

## Success Criteria

- [x] Solution and projects created successfully
- [x] ServerApp returns JSON product data
- [x] ClientApp displays product list from API
- [x] Error handling prevents crashes
- [x] Loading states provide user feedback
- [x] Code follows .NET best practices

---

## Next Steps

This integration code forms the foundation for:
- **Phase 2**: Debugging and troubleshooting
- **Phase 3**: Performance optimization
- **Phase 4**: Advanced features and deployment

---

## Troubleshooting

### Issue: CORS errors in browser console
**Solution**: Implement CORS configuration in ServerApp (see Step 3.3)

### Issue: "Connection refused" error
**Solution**: Verify ServerApp is running and the port number is correct in the API URL

### Issue: Data not displaying
**Solution**: Check browser console for errors and verify the API endpoint returns data

### Issue: Timeout errors
**Solution**: Increase timeout duration or check network/server performance
