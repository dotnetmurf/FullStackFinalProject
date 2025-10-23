# AI Agent Instructions for FullStackApp

This is a full-stack Blazor WebAssembly application with a minimal API backend, focusing on product inventory management. Follow these guidelines when working with this codebase.

## Architecture Overview

- **Client (`ClientApp/`)**: Blazor WebAssembly application
  - Uses service-based architecture for data access
  - Implements client-side caching (5-minute duration)
  - Root component: `App.razor`

- **Server (`ServerApp/`)**: .NET Minimal API
  - Provides product inventory endpoints
  - Implements in-memory caching
  - Includes performance monitoring and logging

## Key Workflows

### Development Setup

```powershell
# Run both server and client (preferred method)
dotnet run --project ServerApp    # Terminal 1
dotnet run --project ClientApp    # Terminal 2
```

Alternatively, use the VS Code tasks:
- "Run ServerApp" - Starts the server
- "Run ClientApp" - Starts the client
- "Run Both Server and Client" - Starts both components

### Project Structure Patterns

- Services are registered in `Program.cs` of respective projects
- Client-side services use scoped lifetime for per-session state
- API endpoints follow RESTful conventions with caching headers

## Key Integration Points

1. **Client-Server Communication**
   - Base API URL configured in `ClientApp/Program.cs`
   - HTTP client injected via DI into services
   - CORS configured on server for Blazor client

2. **Data Flow**
   - `ProductService.cs` handles client-side data retrieval and caching
   - Server provides structured product data with category information
   - Both client and server implement 5-minute cache duration

## Codebase Conventions

1. **Service Pattern**
   ```csharp
   public class ProductService
   {
       private readonly HttpClient _httpClient;
       private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
       // Services follow this pattern with internal caching
   }
   ```

2. **Error Handling**
   - Services include fallback mechanisms for network failures
   - Server endpoints include performance monitoring
   - Client-side caching provides offline resilience

## Common Tasks

- Adding new API endpoints: Extend `ServerApp/Program.cs`
- Adding new client features: Create new pages in `ClientApp/Pages/`
- Modifying data models: Update both client and server representations
- Cache configuration: Check both `ProductService.cs` and server's `Program.cs`

## Key Files

- `ServerApp/Program.cs` - API endpoints and server configuration
- `ClientApp/Program.cs` - Client DI and service configuration
- `ClientApp/Services/ProductService.cs` - Data access patterns
- `ClientApp/Pages/` - Blazor components and pages