using ClientApp.Models;

namespace ClientApp.Services;

/// <summary>
/// Interface for product management operations
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves a paginated list of products
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>A paginated list of products</returns>
    Task<PaginatedList<Product>> GetProductsAsync(int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Retrieves a specific product by its ID
    /// </summary>
    /// <param name="id">The ID of the product to retrieve</param>
    /// <returns>The product if found, null otherwise</returns>
    Task<Product?> GetProductAsync(int id);

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="request">The product request data</param>
    /// <returns>The created product with its assigned ID</returns>
    Task<Product> CreateProductAsync(ProductRequest request);

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The ID of the product to update</param>
    /// <param name="request">The updated product data</param>
    /// <returns>The updated product</returns>
    Task<Product> UpdateProductAsync(int id, ProductRequest request);

    /// <summary>
    /// Deletes a product by its ID
    /// </summary>
    /// <param name="id">The ID of the product to delete</param>
    Task DeleteProductAsync(int id);
}