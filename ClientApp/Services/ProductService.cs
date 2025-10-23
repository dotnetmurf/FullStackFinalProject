using System.Net.Http.Json;

namespace ClientApp.Services
{
    /// <summary>
    /// Service responsible for managing product data retrieval and caching
    /// </summary>
    /// <remarks>
    /// This service implements a caching strategy with a 5-minute duration
    /// and includes fallback mechanisms for network failures
    /// </remarks>
    public class ProductService
    {
        /// <summary>
        /// HTTP client for making API requests
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Cache storage for product data
        /// </summary>
        private Product[]? _cachedProducts;

        /// <summary>
        /// Timestamp of the last successful API fetch
        /// </summary>
        private DateTime _lastFetchTime;

        /// <summary>
        /// Duration for which cached data is considered valid (5 minutes)
        /// </summary>
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Initializes a new instance of the ProductService
        /// </summary>
        /// <param name="httpClient">The HTTP client used for API requests</param>
        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Retrieves a list of products with caching support
        /// </summary>
        /// <returns>
        /// An array of products if successful, or null if the request fails and no cache is available
        /// </returns>
        /// <remarks>
        /// This method implements the following features:
        /// - 5-minute client-side caching
        /// - 10-second timeout for API requests
        /// - Fallback to cached data on network errors
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when HttpClient is null</exception>
        public async Task<Product[]?> GetProductsAsync()
        {
            // Optimized: Check cache first to avoid unnecessary async operations
            if (_cachedProducts != null && DateTime.Now - _lastFetchTime < _cacheDuration)
            {
                return _cachedProducts;
            }

            try
            {
                // Optimized: Use configurable timeout and cancellation
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var products = await _httpClient.GetFromJsonAsync<Product[]>("http://localhost:5132/api/productlist", cts.Token);
                
                // Cache successful response
                if (products != null)
                {
                    _cachedProducts = products;
                    _lastFetchTime = DateTime.Now;
                }
                
                return products;
            }
            catch (TaskCanceledException)
            {
                // Return cached data if available on timeout
                return _cachedProducts;
            }
            catch (HttpRequestException)
            {
                // Return cached data if available on network error
                return _cachedProducts;
            }
        }
    }

    /// <summary>
    /// Represents a product in the inventory system
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Unique identifier for the product
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the product
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Price of the product in the system's currency
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Current stock level of the product
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// Category classification of the product
        /// </summary>
        public Category Category { get; set; } = new();
    }

    /// <summary>
    /// Represents a product category in the inventory system
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Unique identifier for the category
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the category
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
