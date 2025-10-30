# InventoryHub - Product Inventory Management System

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

