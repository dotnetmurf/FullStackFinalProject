using System.Net.Http.Json;

namespace ClientApp.Services
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private Product[]? _cachedProducts;
        private DateTime _lastFetchTime;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

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

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }
        public Category Category { get; set; } = new();
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
