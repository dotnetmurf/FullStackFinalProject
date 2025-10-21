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
            if (_cachedProducts != null && DateTime.Now - _lastFetchTime < _cacheDuration)
            {
                return _cachedProducts;
            }
            try
            {
                var products = await _httpClient.GetFromJsonAsync<Product[]>("http://localhost:5132/api/productlist");
                _cachedProducts = products;
                _lastFetchTime = DateTime.Now;
                return products;
            }
            catch
            {
                return null;
            }
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }
    }
}
