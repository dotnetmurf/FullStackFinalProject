using System.Net.Http.Json;
using ClientApp.Models;

namespace ClientApp.Services;

/// <summary>
/// Service for managing product-related API communications
/// </summary>
/// <remarks>
/// Handles all CRUD operations for products with the ServerApp API.
/// Implements proper error handling and HTTP status code management.
/// Uses dependency injection for HttpClient and logging services.
/// </remarks>
public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of the ProductService
    /// </summary>
    /// <param name="httpClient">HTTP client configured with ServerApp base address</param>
    /// <param name="logger">Logger for tracking service operations</param>
    public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of products from the API
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of products</returns>
    /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
    public async Task<PaginatedList<Product>> GetProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<PaginatedList<Product>>(
                $"/api/products?pageNumber={pageNumber}&pageSize={pageSize}");
            return response ?? new PaginatedList<Product>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching products from API");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a single product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product if found, null otherwise</returns>
    /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Product>($"/api/product/{id}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId} from API", id);
            throw;
        }
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="request">Product creation request with required fields</param>
    /// <returns>Created product with assigned ID</returns>
    /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
    public async Task<Product?> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/product", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error creating product");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">Product ID to update</param>
    /// <param name="request">Product update request with modified fields</param>
    /// <returns>Updated product</returns>
    /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
    public async Task<Product?> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/product/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product ID to delete</param>
    /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
    public async Task DeleteProductAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/product/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            throw;
        }
    }
}
