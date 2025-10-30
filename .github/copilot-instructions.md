# InventoryHub Full-Stack Application - AI Agent Instructions

## Architecture Overview

This is a .NET 9.0 full-stack product inventory management system with:
- **ServerApp**: ASP.NET Core Minimal API backend (`http://localhost:5132`)
- **ClientApp**: Blazor WebAssembly frontend (`http://localhost:5019`)
- **Database**: In-memory EF Core (no persistent storage - resets on restart)
- **Solution**: Multi-project solution managed via `FullStackSolution.sln`

## Critical Configuration Points

### Inter-App Communication
- Client `HttpClient` base address is **hardcoded** in `ClientApp/Program.cs` (`http://localhost:5132`)
- Server CORS allows specific origins in `ServerApp/Program.cs` (`http://localhost:5019`, `https://localhost:7253`)
- **When changing ports**: Update BOTH locations + `launchSettings.json` files

### Service Registration Pattern
- **ClientApp**: Scoped services EXCEPT `ProductsStateService` (Singleton for state persistence across navigation)
- **ServerApp**: Singleton `CacheService`, scoped DbContext, transient for most services

## Key Architectural Patterns

### 1. Cache Management (`ServerApp/Services/CacheService.cs`)
- **Registry pattern**: Tracks all cache keys in `ConcurrentBag<string>` for bulk invalidation
- **Durations**: 5 min absolute, 2 min sliding expiration
- **Invalidation**: `InvalidateProductCaches()` called after any mutation (POST/PUT/DELETE)
- Cache keys format: `products_page{N}_size{N}_search{term}_cat{id}`

### 2. Error Handling Flow
```
ProductService throws ProductServiceException with StatusCode
  ↓
ErrorHandlerService.HandleException() translates to UserError
  ↓
Component displays via ErrorAlert or ToastService
```
- Never catch/handle HTTP errors in Razor components - let service throw
- Use `ErrorHandlerService.HandleException(ex, "context string")` for user-facing errors
- ValidationException includes JSON-serialized error details

### 3. State Management (`ClientApp/Services/ProductsStateService.cs`)
- **Singleton lifetime** preserves: PageSize, PageNumber, SearchTerm, CategoryId
- Resets page to 1 when filters change (search/category/pageSize)
- `OnChange` event notifies components to re-fetch data

### 4. Validation Strategy
- **Server**: DataAnnotations on models + `ValidationService.TryValidate<T>()`
- **Client**: `<EditForm>` with `<DataAnnotationsValidator>` - models mirror server
- Validation errors return `ValidationProblemDetails` with `Dictionary<string, List<string>>`

## Development Workflow

### Running the Application
```powershell
# Option 1: Run solution (starts both apps)
dotnet run --project ServerApp; dotnet run --project ClientApp

# Option 2: Visual Studio - Set multiple startup projects

# Option 3: Run individually
cd ServerApp; dotnet run  # Terminal 1
cd ClientApp; dotnet run  # Terminal 2
```

### Build/Test Commands
```powershell
# Build entire solution
dotnet build FullStackSolution.sln

# Build specific project
dotnet build ServerApp/ServerApp.csproj

# Clean and rebuild
dotnet clean; dotnet build
```

### Database Reset
Database seeds automatically on startup via `DbInitializerService.InitializeAsync()`. To refresh:
1. Restart ServerApp, OR
2. Call `POST /api/products/refresh` endpoint (requires `SeedingService.GetSampleProducts()`)

## API Conventions

### Endpoint Patterns (`ServerApp/Endpoints/ProductEndpoints.cs`)
- All extracted to static extension methods: `app.MapProductEndpoints()`
- Naming: `GetProducts`, `GetProductById`, `CreateProduct`, `UpdateProduct`, `DeleteProduct`
- All endpoints include `.WithName()` and `.WithOpenApi()` for Swagger

### Request/Response Models
- DTOs live in `Models/` folders (both projects have separate but matching models)
- Request models: `CreateProductRequest`, `UpdateProductRequest`
- Response models: `Product`, `PaginatedList<T>`, `ValidationProblemDetails`, `ErrorResponse`

### Pagination
- Uses `PaginatedList<T>` with `Items`, `TotalCount`, `PageNumber`, `PageSize`, `HasPreviousPage`, `HasNextPage`
- Default: pageSize=10 (server), pageSize=12 (client)
- Max pageSize: 100 (enforced server-side)

## Component Patterns

### Razor Component Structure
```csharp
@page "/route"
@inject ServiceName Service
@implements IDisposable  // If subscribing to events

// OnInitializedAsync: Load data, subscribe to events
// IDisposable.Dispose: Unsubscribe from events (ProductsStateService.OnChange)
```

### Reusable Components (`ClientApp/Shared/`)
- `ProductForm`: Handles create/edit with `IsEditMode` parameter
- `ProductCard`: Display product with action buttons
- `ErrorAlert`: Shows `UserError` with retry button
- `ToastContainer`: Global success notifications

## Performance & Monitoring

### Performance Middleware (`ServerApp/Middleware/PerformanceMiddleware.cs`)
- Wraps ALL requests with `Stopwatch` timing
- Logs: `{Method} {Path} responded with {StatusCode} in {ElapsedMs} ms`
- Registered via `.UsePerformanceMonitoring()` - must be after CORS, before endpoints

### Response Compression
- Enabled for HTTPS
- Brotli (preferred), Gzip (fallback)
- Level: `CompressionLevel.Fastest`

### Swagger/OpenAPI
- Available in Development mode ONLY at root URL (`/`)
- XML documentation enabled (requires `<GenerateDocumentationFile>true</GenerateDocumentationFile>`)
- Suppress warning 1591 (missing XML comments) via `<NoWarn>1591</NoWarn>`

## Common Pitfalls

1. **Models Drift**: Client/Server have duplicate models - keep in sync manually
2. **State Service Lifetime**: ProductsStateService MUST be Singleton (not Scoped)
3. **Cache Invalidation**: Always call `CacheService.InvalidateProductCaches()` after mutations
4. **CORS Issues**: Add new client URLs to `ServerApp/Program.cs` CORS policy
5. **Port Conflicts**: Check both launchSettings.json files if apps won't start
6. **Validation Mismatch**: Client-side validation mirrors server - update both

## File Organization

```
ServerApp/
  ├── Endpoints/       # Minimal API endpoint definitions (static extension methods)
  ├── Services/        # Business logic (CacheService, ValidationService, etc.)
  ├── Middleware/      # Custom middleware (PerformanceMiddleware)
  ├── Data/            # EF Core context + DbInitializer
  └── Models/          # Request/Response DTOs, validation attributes

ClientApp/
  ├── Pages/           # Routable Razor components (@page directive)
  ├── Shared/          # Reusable components (no @page)
  ├── Services/        # API clients, state management, error handling
  ├── Layout/          # App shell (MainLayout, NavMenu)
  └── Models/          # Client-side DTOs (mirror ServerApp models)
```

## Testing Notes

- No unit tests currently in solution
- Manual testing via Swagger UI (`/` when ServerApp running)
- Test pagination: `?pageNumber=1&pageSize=5`
- Test search: `?searchTerm=laptop`
- Test filtering: `?categoryId=1`
