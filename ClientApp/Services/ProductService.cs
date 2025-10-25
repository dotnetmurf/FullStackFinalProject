using System.Net.Http.Json;
using System.Net;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ClientApp.Models;

namespace ClientApp.Services;

/// <summary>
/// Service for managing product-related operations with caching support
/// </summary>
public class ProductService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductService> _logger;
    private const string CacheKeyPrefix = "product_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly ConcurrentDictionary<string, object> _cacheKeys = new();
    private static readonly int MaxRetries = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);
    private bool _disposed;

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
    /// Retrieves a list of all unique categories
    /// </summary>
    /// <returns>A list of unique product categories</returns>
    /// <exception cref="HttpRequestException">Thrown when the server request fails</exception>
    public async Task<List<Category>> GetCategoriesAsync()
    {
        ThrowIfDisposed();
        var retryCount = 0;
        
        while (true)
        {
            try
            {
                const string cacheKey = $"{CacheKeyPrefix}categories";
                if (_cache.TryGetValue(cacheKey, out List<Category>? categories))
                {
                    return categories!;
                }

                var response = await _httpClient.GetAsync("api/product/categories");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new List<Category>();
                }
                
                response.EnsureSuccessStatusCode();
                categories = await response.Content.ReadFromJsonAsync<List<Category>>();
                
                if (categories != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(CacheDuration)
                        .RegisterPostEvictionCallback((key, value, reason, state) =>
                        {
                            _cacheKeys.TryRemove(key.ToString()!, out _);
                        });

                    _cacheKeys.TryAdd(cacheKey, new object());
                    _cache.Set(cacheKey, categories, cacheEntryOptions);
                    return categories;
                }

                throw new InvalidOperationException("Server returned null response for categories list");
            }
            catch (Exception ex) when (retryCount < MaxRetries && 
                (ex is HttpRequestException || ex is TaskCanceledException))
            {
                retryCount++;
                _logger.LogWarning(ex, "Retry {Count} of {Max} for GetCategories", retryCount, MaxRetries);
                await Task.Delay(RetryDelay * retryCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories list");
                throw;
            }
        }
    }

    /// <summary>
    /// Retrieves a paginated list of products
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>A paginated list of products</returns>
    /// <exception cref="HttpRequestException">Thrown when the server request fails</exception>
    public async Task<PaginatedList<Product>> GetProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        ThrowIfDisposed();
        var retryCount = 0;
        
        while (true)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}list_p{pageNumber}_s{pageSize}";
                if (_cache.TryGetValue(cacheKey, out PaginatedList<Product>? products))
                {
                    return products!;
                }

                var response = await _httpClient.GetAsync($"api/product?page={pageNumber}&pageSize={pageSize}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new PaginatedList<Product>();
                }
                
                response.EnsureSuccessStatusCode();
                products = await response.Content.ReadFromJsonAsync<PaginatedList<Product>>();
                
                if (products != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(CacheDuration)
                        .RegisterPostEvictionCallback((key, value, reason, state) =>
                        {
                            _cacheKeys.TryRemove(key.ToString()!, out _);
                        });

                    _cacheKeys.TryAdd(cacheKey, new object());
                    _cache.Set(cacheKey, products, cacheEntryOptions);
                    return products;
                }

                throw new InvalidOperationException("Server returned null response for products list");
            }
            catch (Exception ex) when (retryCount < MaxRetries && 
                (ex is HttpRequestException || ex is TaskCanceledException))
            {
                retryCount++;
                _logger.LogWarning(ex, "Retry {Count} of {Max} for GetProducts", retryCount, MaxRetries);
                await Task.Delay(RetryDelay * retryCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products list. Page: {Page}, Size: {Size}", 
                    pageNumber, pageSize);
                throw;
            }
        }
    }

    /// <summary>
    /// Retrieves a specific product by ID
    /// </summary>
    /// <param name="id">The ID of the product to retrieve</param>
    /// <returns>The product if found, null if not found</returns>
    /// <exception cref="HttpRequestException">Thrown when the server request fails</exception>
    public async Task<Product?> GetProductAsync(int id)
    {
        ThrowIfDisposed();
        var retryCount = 0;

        while (true)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}{id}";
                if (_cache.TryGetValue(cacheKey, out Product? product))
                {
                    return product;
                }

                var response = await _httpClient.GetAsync($"api/product/{id}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();
                product = await response.Content.ReadFromJsonAsync<Product>();

                if (product != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(CacheDuration)
                        .RegisterPostEvictionCallback((key, value, reason, state) =>
                        {
                            _cacheKeys.TryRemove(key.ToString()!, out _);
                        });

                    _cacheKeys.TryAdd(cacheKey, new object());
                    _cache.Set(cacheKey, product, cacheEntryOptions);
                    return product;
                }

                throw new InvalidOperationException($"Server returned null response for product {id}");
            }
            catch (Exception ex) when (retryCount < MaxRetries && 
                (ex is HttpRequestException || ex is TaskCanceledException))
            {
                retryCount++;
                _logger.LogWarning(ex, "Retry {Count} of {Max} for GetProduct {Id}", 
                    retryCount, MaxRetries, id);
                await Task.Delay(RetryDelay * retryCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {Id}", id);
                throw;
            }
        }
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="product">The product to create</param>
    /// <returns>The created product</returns>
    /// <exception cref="ArgumentNullException">Thrown when product is null</exception>
    /// <exception cref="HttpRequestException">Thrown when the server request fails</exception>
    /// <exception cref="ValidationException">Thrown when the server returns validation errors</exception>
    public async Task<Product> CreateProductAsync(Product product)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(product);

        var retryCount = 0;
        
        while (true)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/product", product);
                
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var validationErrors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();
                    if (validationErrors?.Any() == true)
                    {
                        throw new ValidationException(
                            string.Join(Environment.NewLine, validationErrors.SelectMany(e => e.Value)));
                    }
                }
                
                response.EnsureSuccessStatusCode();
                var createdProduct = await response.Content.ReadFromJsonAsync<Product>();
                
                if (createdProduct != null)
                {
                    InvalidateProductCache();
                    return createdProduct;
                }
                
                throw new InvalidOperationException("Failed to create product: server returned null");
            }
            catch (Exception ex) when (retryCount < MaxRetries && 
                (ex is HttpRequestException || ex is TaskCanceledException))
            {
                retryCount++;
                _logger.LogWarning(ex, "Retry {Count} of {Max} for CreateProduct", 
                    retryCount, MaxRetries);
                await Task.Delay(RetryDelay * retryCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {Name}", product.Name);
                throw;
            }
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The ID of the product to update</param>
    /// <param name="product">The updated product data</param>
    /// <returns>The updated product</returns>
    /// <exception cref="ArgumentNullException">Thrown when product is null</exception>
    /// <exception cref="HttpRequestException">Thrown when the server request fails</exception>
    /// <exception cref="ValidationException">Thrown when the server returns validation errors</exception>
    public async Task<Product> UpdateProductAsync(int id, Product product)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(product);
        var retryCount = 0;
        
        while (true)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/product/{id}", product);
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found");
                }
                
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var validationErrors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();
                    if (validationErrors?.Any() == true)
                    {
                        throw new ValidationException(
                            string.Join(Environment.NewLine, validationErrors.SelectMany(e => e.Value)));
                    }
                }
                
                response.EnsureSuccessStatusCode();
                var updatedProduct = await response.Content.ReadFromJsonAsync<Product>();
                
                if (updatedProduct != null)
                {
                    InvalidateProductCache();
                    return updatedProduct;
                }
                
                throw new InvalidOperationException($"Failed to update product {id}: server returned null");
            }
            catch (Exception ex) when (retryCount < MaxRetries && 
                (ex is HttpRequestException || ex is TaskCanceledException))
            {
                retryCount++;
                _logger.LogWarning(ex, "Retry {Count} of {Max} for UpdateProduct {Id}", 
                    retryCount, MaxRetries, id);
                await Task.Delay(RetryDelay * retryCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Id}", id);
                throw;
            }
        }
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">The ID of the product to delete</param>
    /// <exception cref="HttpRequestException">Thrown when the server request fails</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the product is not found</exception>
    public async Task DeleteProductAsync(int id)
    {
        ThrowIfDisposed();
        var retryCount = 0;
        
        while (true)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/product/{id}");
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException($"Product with ID {id} not found");
                }
                
                response.EnsureSuccessStatusCode();
                InvalidateProductCache();
                return;
            }
            catch (Exception ex) when (retryCount < MaxRetries && 
                (ex is HttpRequestException || ex is TaskCanceledException))
            {
                retryCount++;
                _logger.LogWarning(ex, "Retry {Count} of {Max} for DeleteProduct {Id}", 
                    retryCount, MaxRetries, id);
                await Task.Delay(RetryDelay * retryCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Id}", id);
                throw;
            }
        }
    }

    /// <summary>
    /// Invalidates specific product cache entries
    /// </summary>
    /// <param name="productId">Optional product ID to invalidate specific product cache</param>
    private void InvalidateProductCache(int? productId = null)
    {
        try
        {
            if (productId.HasValue)
            {
                var key = $"{CacheKeyPrefix}{productId}";
                _cache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
            }
            else
            {
                // Only invalidate list caches for single item operations
                foreach (var key in _cacheKeys.Keys.Where(k => k.Contains("list_")).ToList())
                {
                    _cache.Remove(key);
                    _cacheKeys.TryRemove(key, out _);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating product cache");
        }
    }

    /// <summary>
    /// Throws an ObjectDisposedException if the service is disposed
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ProductService));
        }
    }

    /// <summary>
    /// Disposes of the service resources
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                InvalidateProductCache();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing ProductService");
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}