using System.Net.Http.Json;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ClientApp.Models;

namespace ClientApp.Services;

/// <summary>
/// Service for managing product-related operations with caching support
/// </summary>
public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductService> _logger;
    private const string CacheKeyPrefix = "product_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly ConcurrentDictionary<string, object> _cacheKeys = new();

    /// <summary>
    /// Initializes a new instance of the ProductService
    /// </summary>
    public ProductService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<ProductService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a paginated list of products
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>A paginated list of products</returns>
    public async Task<PaginatedList<Product>> GetProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var cacheKey = $"{CacheKeyPrefix}list_p{pageNumber}_s{pageSize}";
            if (!_cache.TryGetValue(cacheKey, out PaginatedList<Product>? products))
            {
                products = await _httpClient.GetFromJsonAsync<PaginatedList<Product>>($"api/products?page={pageNumber}&pageSize={pageSize}");
                if (products != null)
                {
                    _cacheKeys.TryAdd(cacheKey, new object());
                    _cache.Set(cacheKey, products, CacheDuration);
                }
            }
            return products ?? new PaginatedList<Product>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products list");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a specific product by ID
    /// </summary>
    /// <param name="id">The ID of the product to retrieve</param>
    /// <returns>The product if found, null otherwise</returns>
    public async Task<Product?> GetProductAsync(int id)
    {
        try
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            if (!_cache.TryGetValue(cacheKey, out Product? product))
            {
                product = await _httpClient.GetFromJsonAsync<Product>($"api/products/{id}");
                if (product != null)
                {
                    _cacheKeys.TryAdd(cacheKey, new object());
                    _cache.Set(cacheKey, product, CacheDuration);
                }
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
    /// <param name="product">The product to create</param>
    /// <returns>The created product</returns>
    /// <exception cref="ArgumentNullException">Thrown when product is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the server fails to create the product</exception>
    public async Task<Product> CreateProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/products", product);
            response.EnsureSuccessStatusCode();
            
            var createdProduct = await response.Content.ReadFromJsonAsync<Product>();
            if (createdProduct != null)
            {
                InvalidateProductCache();
                return createdProduct;
            }
            
            throw new InvalidOperationException("Failed to create product: server returned null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {Name}", product.Name);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The ID of the product to update</param>
    /// <param name="product">The updated product data</param>
    /// <returns>The updated product</returns>
    /// <exception cref="ArgumentNullException">Thrown when product is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the server fails to update the product</exception>
    public async Task<Product> UpdateProductAsync(int id, Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", product);
            response.EnsureSuccessStatusCode();
            
            var updatedProduct = await response.Content.ReadFromJsonAsync<Product>();
            if (updatedProduct != null)
            {
                InvalidateProductCache();
                return updatedProduct;
            }
            
            throw new InvalidOperationException($"Failed to update product {id}: server returned null");
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
    /// <param name="id">The ID of the product to delete</param>
    /// <exception cref="HttpRequestException">Thrown when the server fails to delete the product</exception>
    public async Task DeleteProductAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/products/{id}");
            response.EnsureSuccessStatusCode();
            InvalidateProductCache();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Invalidates the product cache
    /// </summary>
    private void InvalidateProductCache()
    {
        try
        {
            foreach (var key in _cacheKeys.Keys.ToList())
            {
                _cache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating product cache");
        }
    }
}