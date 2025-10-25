using System.Net.Http.Json;
using ClientApp.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ClientApp.Services;

/// <summary>
/// Service for managing product-related operations
/// </summary>
public interface IProductService
{
    Task<PaginatedList<Product>> GetProductsAsync(int pageNumber = 1, int pageSize = 10);
    Task<Product?> GetProductAsync(int id);
    Task<Product> CreateProductAsync(ProductRequest request);
    Task<Product> UpdateProductAsync(int id, ProductRequest request);
    Task DeleteProductAsync(int id);
}

/// <summary>
/// Implementation of the product service
/// </summary>
public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductService> _logger;
    private readonly IMemoryCache _cache;
    private const string ProductsCacheKey = "products";
    private const string ProductCacheKeyPrefix = "product_";

    public ProductService(HttpClient httpClient, ILogger<ProductService> logger, IMemoryCache cache)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// Retrieves a paginated list of products
    /// </summary>
    public async Task<PaginatedList<Product>> GetProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            string cacheKey = $"{ProductsCacheKey}_page{pageNumber}_size{pageSize}";
            
            if (_cache.TryGetValue(cacheKey, out PaginatedList<Product>? cachedProducts))
            {
                return cachedProducts!;
            }

            var response = await _httpClient.GetFromJsonAsync<PaginatedList<Product>>($"api/products?pageNumber={pageNumber}&pageSize={pageSize}");
            if (response == null)
            {
                throw new Exception("Failed to retrieve products");
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            _cache.Set(cacheKey, response, cacheOptions);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a single product by ID
    /// </summary>
    public async Task<Product?> GetProductAsync(int id)
    {
        try
        {
            string cacheKey = $"{ProductCacheKeyPrefix}{id}";
            
            if (_cache.TryGetValue(cacheKey, out Product? cachedProduct))
            {
                return cachedProduct;
            }

            var product = await _httpClient.GetFromJsonAsync<Product>($"api/product/{id}");
            if (product != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                _cache.Set(cacheKey, product, cacheOptions);
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    public async Task<Product> CreateProductAsync(ProductRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/product", request);
            response.EnsureSuccessStatusCode();
            
            var product = await response.Content.ReadFromJsonAsync<Product>();
            if (product == null)
            {
                throw new Exception("Failed to create product");
            }

            _cache.Remove(ProductsCacheKey);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    public async Task<Product> UpdateProductAsync(int id, ProductRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/product/{id}", request);
            response.EnsureSuccessStatusCode();
            
            var product = await response.Content.ReadFromJsonAsync<Product>();
            if (product == null)
            {
                throw new Exception("Failed to update product");
            }

            string cacheKey = $"{ProductCacheKeyPrefix}{id}";
            _cache.Remove(cacheKey);
            _cache.Remove(ProductsCacheKey);
            
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    public async Task DeleteProductAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/product/{id}");
            response.EnsureSuccessStatusCode();

            string cacheKey = $"{ProductCacheKeyPrefix}{id}";
            _cache.Remove(cacheKey);
            _cache.Remove(ProductsCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            throw;
        }
    }
}