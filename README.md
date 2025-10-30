# InventoryHub - Product Inventory Management System

## REFLECTION - How Copilot Assisted in the Full-Stack Development of InventoryHub

Microsoft/GitHub Copilot played a central role in building and refining the InventoryHub application. Here’s how Copilot contributed across key areas:

### 1. Generating Integration Code
- **CRUD Operations:** Copilot provided step-by-step implementation plans for client-side CRUD pages and shared components, ensuring seamless communication with the backend API.
- **Service Layer:** It generated robust ProductService code for HTTP requests, error handling, and DTO mapping, reducing manual boilerplate.
- **Dependency Injection:** Copilot guided the correct setup of DI in Program.cs, including HttpClient configuration and service registration.

### 2. Debugging Issues
- **Build Failures:** When build errors occurred due to missing namespaces or dependencies, Copilot analyzed the error output and recommended reordering the implementation plan to ensure successful builds after each phase.
- **Error Handling:** It suggested patterns for catching and displaying errors, both in services and UI components, improving user feedback and resilience.
- **Configuration Pitfalls:** Copilot flagged issues with hardcoded ports, CORS settings, and model drift, helping avoid common integration bugs.

### 3. Structuring JSON Responses
- **DTO Consistency:** Copilot ensured that client and server models matched, enabling reliable JSON serialization/deserialization.
- **Validation Responses:** It documented how to handle `ValidationProblemDetails` and structured error messages for both client and server, making error handling predictable and user-friendly.
- **API Documentation:** Copilot summarized endpoint conventions and query parameters, clarifying how JSON responses should be shaped and consumed.

### 4. Optimizing Performance
- **Caching Strategy:** Copilot explained the registry pattern for cache key management and automatic invalidation, helping optimize server-side performance.
- **Middleware:** It highlighted the use of performance monitoring middleware and response compression, ensuring efficient request handling and fast client experiences.
- **State Management:** Copilot recommended singleton lifetimes for state services, reducing unnecessary data fetches and improving navigation speed.

## Challenges Encountered & Copilot’s Solutions

- **Build Order Dependencies:** Initial implementation plans led to build failures due to missing folders/namespaces. Copilot diagnosed the issue and restructured the plan to add prerequisites first, ensuring incremental build success.
- **Model Drift:** Maintaining duplicate models on client and server was error-prone. Copilot flagged this and recommended manual synchronization, preventing subtle bugs.
- **CORS and Port Issues:** Copilot identified the need to update multiple files when changing ports or client URLs, preventing integration failures.
- **Validation and Error Feedback:** Copilot provided patterns for handling validation errors and translating technical exceptions into user-friendly messages, improving the overall UX.

## Lessons Learned About Using Copilot Effectively

- **Iterative Guidance:** Copilot excels when given clear, incremental tasks. Breaking work into phases and checkpoints allows Copilot to provide actionable, context-aware advice.
- **Error Analysis:** Copilot’s ability to interpret build and runtime errors is invaluable for debugging, especially in multi-project solutions.
- **Documentation & Planning:** Copilot can generate not only code but also comprehensive implementation plans, prompts, and documentation, streamlining onboarding and collaboration.
- **Pattern Recognition:** Copilot quickly identifies and enforces architectural and coding patterns, ensuring consistency across the stack.
- **Limitations:** Copilot relies on explicit context—missing files or ambiguous requirements can lead to incomplete solutions. Manual review and adjustment are still necessary for edge cases and project-specific needs.

## Conclusion

Copilot proved to be a powerful assistant for full-stack development, from scaffolding integration code to debugging, optimizing, and documenting the project. Its guidance enabled rapid iteration, reduced errors, and improved code quality, making it an essential tool for both learning and professional development workflows.

---

## InventoryHub - Application Details

A modern full-stack web application built with .NET 9.0 for managing product inventory with real-time search, filtering, and pagination capabilities.

## 🚀 Features

- **Product Management**: Full CRUD operations for products
- **Smart Search**: Real-time search by product name
- **Category Filtering**: Filter products by category
- **Pagination**: Efficient data loading with customizable page sizes
- **Performance Monitoring**: Built-in request timing and logging
- **Response Caching**: Intelligent caching with automatic invalidation
- **Response Compression**: Brotli and Gzip compression for optimal performance
- **Validation**: Comprehensive client and server-side validation
- **Error Handling**: User-friendly error messages with retry capabilities
- **Swagger Documentation**: Interactive API documentation in development mode

## 🏗️ Architecture

### Technology Stack

**Backend (ServerApp)**
- ASP.NET Core 9.0 Minimal API
- Entity Framework Core (In-Memory Database)
- Memory Caching with custom `CacheService`
- Swagger/OpenAPI for API documentation

**Frontend (ClientApp)**
- Blazor WebAssembly
- Bootstrap 5 for responsive UI
- Component-based architecture
- Centralized state management

### Project Structure

```
FullStackApp/
├── ServerApp/              # Backend API
│   ├── Endpoints/          # API endpoint definitions
│   ├── Services/           # Business logic & utilities
│   ├── Middleware/         # Custom middleware (performance monitoring)
│   ├── Data/               # Database context & initialization
│   └── Models/             # DTOs & domain models
│
├── ClientApp/              # Frontend application
│   ├── Pages/              # Routable Razor components
│   ├── Shared/             # Reusable UI components
│   ├── Services/           # API clients & state management
│   ├── Layout/             # Application shell & navigation
│   └── Models/             # Client-side DTOs
│
└── FullStackSolution.sln   # Solution file
```

## 🔧 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A code editor (Visual Studio 2022, VS Code, or JetBrains Rider)
- PowerShell (for Windows) or Bash (for Linux/macOS)

## 📦 Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/dotnetmurf/FullStackFinalProject.git
cd FullStackFinalProject/FullStackApp
```

### 2. Restore Dependencies

```powershell
dotnet restore FullStackSolution.sln
```

### 3. Build the Solution

```powershell
dotnet build FullStackSolution.sln
```

## 🚀 Running the Application

### Option 1: Visual Studio

1. Open `FullStackSolution.sln` in Visual Studio
2. Right-click the solution → **Set Startup Projects**
3. Select **Multiple startup projects**
4. Set both `ServerApp` and `ClientApp` to **Start**
5. Press **F5** or click **Start**

### Option 2: Command Line (Two Terminals)

**Terminal 1 - Backend:**
```powershell
cd ServerApp
dotnet run
```
The server will start at `http://localhost:5132`

**Terminal 2 - Frontend:**
```powershell
cd ClientApp
dotnet run
```
The client will start at `http://localhost:5019`

### Option 3: Single Command (Sequential)

```powershell
dotnet run --project ServerApp; dotnet run --project ClientApp
```

## 🌐 Accessing the Application

- **Frontend Application**: http://localhost:5019
- **Backend API**: http://localhost:5132
- **Swagger Documentation**: http://localhost:5132 (Development mode only)

## 📚 API Documentation

### Endpoints

#### Products

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get paginated products with optional search and category filter |
| GET | `/api/product/{id}` | Get a single product by ID |
| POST | `/api/product` | Create a new product |
| PUT | `/api/product/{id}` | Update an existing product |
| DELETE | `/api/product/{id}` | Delete a product |
| POST | `/api/products/refresh` | Refresh sample data |

#### Categories

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/categories` | Get all categories |

### Query Parameters (GET /api/products)

- `pageNumber` (int, default: 1): Page number for pagination
- `pageSize` (int, default: 10, max: 100): Number of items per page
- `searchTerm` (string, optional): Filter products by name
- `categoryId` (int, optional): Filter products by category

**Example:**
```
GET /api/products?pageNumber=1&pageSize=20&searchTerm=laptop&categoryId=1
```

## 🔍 Key Features Explained

### Performance Monitoring

Every API request is automatically timed and logged:
```
GET /api/products responded with 200 in 45 ms
```

### Intelligent Caching

- Cache Duration: 5 minutes absolute, 2 minutes sliding
- Automatic invalidation after create, update, or delete operations
- Cache key format: `products_page{N}_size{N}_search{term}_cat{id}`

### State Persistence

The `ProductsStateService` maintains user preferences across navigation:
- Current page number
- Selected page size
- Active search term
- Category filter

### Error Handling

Three-tier error handling approach:
1. Services throw specific exceptions (`ProductServiceException`)
2. `ErrorHandlerService` translates to user-friendly messages
3. UI components display with retry options

## 🗄️ Database

The application uses an **in-memory database** that:
- Automatically seeds with sample data on startup
- Resets when the ServerApp restarts
- Perfect for development and testing
- Can be refreshed via the "Refresh Sample Data" button

### Sample Data

The database includes:
- **36 sample products** across multiple categories
- **7 categories**: Electronics, Accessories, Gaming, Networking, Storage, Software & Photography

## 🎨 UI Components

### Reusable Components

- **ProductCard**: Display product information with action buttons
- **ProductForm**: Unified form for creating and editing products
- **ErrorAlert**: User-friendly error display with retry functionality
- **ToastContainer**: Success notification system

### Pages

- **Home**: Welcome page with navigation
- **Products**: Main inventory page with search, filter, and pagination
- **Product Details**: View individual product information
- **Create Product**: Form for adding new products
- **Edit Product**: Form for updating existing products
- **Delete Product**: Confirmation page for product deletion
- **About**: Application information

## 🔧 Configuration

### Changing Ports

If you need to change the default ports, update these files:

1. **ServerApp/Properties/launchSettings.json** - Change server port
2. **ClientApp/Properties/launchSettings.json** - Change client port
3. **ClientApp/Program.cs** - Update `HttpClient.BaseAddress`
4. **ServerApp/Program.cs** - Update CORS policy origins

### CORS Configuration

Located in `ServerApp/Program.cs`:
```csharp
policy.WithOrigins("http://localhost:5019", "https://localhost:7253")
```

## 🧪 Testing

### Manual Testing

Use Swagger UI at http://localhost:5132 to test API endpoints:
- View all available endpoints
- Test requests with different parameters
- See request/response schemas
- Execute test requests directly

### Testing Scenarios

**Pagination:**
```
/api/products?pageNumber=2&pageSize=5
```

**Search:**
```
/api/products?searchTerm=laptop
```

**Category Filter:**
```
/api/products?categoryId=1
```

**Combined:**
```
/api/products?pageNumber=1&pageSize=10&searchTerm=book&categoryId=3
```

## 🐛 Troubleshooting

### Port Already in Use

If ports 5132 or 5019 are already in use:
1. Stop the conflicting process
2. Or change ports in `launchSettings.json` files (see Configuration section)

### CORS Errors

If you see CORS errors in the browser console:
1. Verify the ServerApp is running
2. Check that the client URL is listed in `ServerApp/Program.cs` CORS policy
3. Ensure the `HttpClient.BaseAddress` in `ClientApp/Program.cs` matches the server URL

### Connection Refused

If the client can't connect to the server:
1. Ensure ServerApp is running first
2. Verify the server URL in browser: http://localhost:5132
3. Check firewall settings

## 🤝 Contributing

This is a learning project from the Microsoft Full-Stack Developer Professional Certificate program. Feel free to fork and experiment!

## 📝 License

See [LICENSE.txt](LICENSE.txt) for details.

## 👨‍💻 Development Notes

- The application uses **in-memory database** - no data persists between restarts
- **No authentication** is implemented (intentional for learning purposes)
- Models are duplicated between Client and Server - keep them synchronized
- `ProductsStateService` must be registered as **Singleton**, not Scoped
- Cache invalidation happens automatically after mutations

## 📖 Learning Resources

This project demonstrates:
- Full-stack development with .NET
- RESTful API design
- Blazor WebAssembly SPA architecture
- Entity Framework Core
- Dependency injection
- Middleware implementation
- Response caching strategies
- Error handling patterns
- Component-based UI development

---

