# Blazor WebAssembly ClientApp Rebuild Guide

## Project Overview
InventoryHub is a full-stack product inventory management application. The ClientApp is a Blazor WebAssembly project that communicates with an ASP.NET Core Minimal API backend. This guide outlines the structured approach for rebuilding the ClientApp with proper architecture and best practices.

## Architecture Requirements

### Project Structure
```
ClientApp/
├── Components/           # Reusable components
│   ├── ProductCard
│   ├── ProductDeleteConfirm
│   ├── ProductDetails
│   ├── ProductForm
│   └── ProductGrid
├── Layout/              # Layout components
│   ├── MainLayout
│   └── NavMenu
├── Models/              # Data models
│   ├── Category
│   ├── PaginatedList
│   ├── Product
│   └── ProductRequest
├── Pages/              # Application pages
│   ├── Index          # Home page
│   ├── Products       # Product list
│   └── Product        # Product details/edit
├── Services/          # Business logic
│   └── ProductService
└── Shared/           # Shared components
    └── Components/
        ├── LoadingIndicator
        ├── StatusModal
        └── ValidationMessage
```

### API Integration Points
- Base URL: http://localhost:5118
- Endpoints:
  - GET /api/products - List all products
  - GET /api/product/{id} - Get single product
  - POST /api/product - Create product
  - PUT /api/product/{id} - Update product
  - DELETE /api/product/{id} - Delete product

### Component Dependencies
- Bootstrap 5.x for styling
- Microsoft.AspNetCore.Components.WebAssembly
- Microsoft.Extensions.Http for HttpClient
- System.Net.Http.Json for JSON handling

## Implementation Phases

### Phase 1: Core Infrastructure
1. Project Setup
   ```shell
   dotnet new blazorwasm -o ClientApp
   ```
   
2. Configure Dependencies
   - Add required NuGet packages
   - Configure HttpClient in Program.cs
   - Set up CORS configuration

3. Implement Models
   - Match server-side models
   - Add validation attributes
   - Implement interfaces as needed

4. Create Base Services
   - Implement ProductService
   - Add error handling
   - Configure caching

### Phase 2: Read Operations
1. Shared Components
   ```csharp
   // LoadingIndicator.razor
   @if (IsLoading)
   {
       <div class="loading-indicator">
           <div class="spinner-border text-primary" role="status">
               <span class="visually-hidden">Loading...</span>
           </div>
       </div>
   }

   @code {
       [Parameter] public bool IsLoading { get; set; }
   }
   ```

2. Product List Implementation
   - Create Products.razor page
   - Implement ProductGrid component
   - Add sorting and filtering
   - Test GET operations

### Phase 3: Create Operations
1. Form Components
   - Implement ProductForm
   - Add validation
   - Create success/error modals

2. Create Product Flow
   - Add create page
   - Implement POST operation
   - Add loading states
   - Test creation flow

### Phase 4: Update Operations
1. Edit Functionality
   - Add edit mode to ProductForm
   - Implement PUT operation
   - Update success/error handling
   - Test update flow

### Phase 5: Delete Operations
1. Delete Implementation
   - Create confirmation modal
   - Implement DELETE operation
   - Update product list
   - Test deletion flow

## Testing Strategy

### Unit Testing
- Test each component in isolation
- Verify form validation
- Test service methods

### Integration Testing
- Test API communication
- Verify error handling
- Test loading states

### UI Testing
- Verify component rendering
- Test user interactions
- Verify modal behaviors

## Best Practices

### Component Design
1. Single Responsibility
   ```csharp
   // Good: ProductCard only handles display
   public partial class ProductCard
   {
       [Parameter] public Product Product { get; set; }
       [Parameter] public EventCallback<Product> OnEdit { get; set; }
       [Parameter] public EventCallback<Product> OnDelete { get; set; }
   }
   ```

2. Parameter Validation
   ```csharp
   protected override void OnParametersSet()
   {
       if (Product == null)
           throw new ArgumentNullException(nameof(Product));
   }
   ```

3. Event Handling
   ```csharp
   private async Task HandleEditClick()
   {
       try
       {
           await OnEdit.InvokeAsync(Product);
       }
       catch (Exception ex)
       {
           // Handle error
       }
   }
   ```

### Service Pattern
```csharp
public interface IProductService
{
    Task<PaginatedList<Product>> GetProductsAsync();
    Task<Product> GetProductAsync(int id);
    Task<Product> CreateProductAsync(ProductRequest request);
    Task<Product> UpdateProductAsync(int id, ProductRequest request);
    Task DeleteProductAsync(int id);
}

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductService> _logger;

    public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
}
```

### Error Handling
```csharp
public async Task<Product> CreateProductAsync(ProductRequest request)
{
    try
    {
        var response = await _httpClient.PostAsJsonAsync("api/product", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>();
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Error creating product");
        throw;
    }
}
```

### Form Validation
```csharp
private EditContext _editContext;
private ValidationMessageStore _messageStore;

protected override void OnInitialized()
{
    _editContext = new EditContext(Model);
    _messageStore = new ValidationMessageStore(_editContext);
    _editContext.OnValidationRequested += HandleValidationRequested;
}
```

### State Management
```csharp
@inject IProductService ProductService

private PaginatedList<Product> _products;
private bool _isLoading;
private string _errorMessage;

protected override async Task OnInitializedAsync()
{
    try
    {
        _isLoading = true;
        _products = await ProductService.GetProductsAsync();
    }
    catch (Exception ex)
    {
        _errorMessage = ex.Message;
    }
    finally
    {
        _isLoading = false;
    }
}
```

## Implementation Order

1. Infrastructure
   - Project setup
   - Model creation
   - Service implementation

2. Read Operations
   - Product list page
   - Product details view
   - Grid component

3. Create Operations
   - Product form
   - Create page
   - Success/error handling

4. Update Operations
   - Edit functionality
   - Update form
   - Success messages

5. Delete Operations
   - Confirmation modal
   - Delete functionality
   - List updates

## Success Criteria

### Functionality
- All CRUD operations working
- Proper error handling
- Loading states implemented
- Validation working

### Performance
- Efficient API calls
- Proper caching
- Responsive UI

### User Experience
- Clear feedback
- Intuitive navigation
- Consistent styling

## References
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor)
- [Bootstrap Documentation](https://getbootstrap.com/docs/5.1)
- [HTTP Client Best Practices](https://docs.microsoft.com/aspnet/core/blazor/call-web-api)