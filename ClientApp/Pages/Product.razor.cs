using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using ClientApp.Services;

namespace ClientApp.Pages;

public class ProductBase : ComponentBase
{
    [Parameter]
    public int? Id { get; set; }

    [Inject]
    protected ProductService ProductService { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    protected ILogger<Product> Logger { get; set; } = default!;

    protected Models.Product _product = new();
    protected bool _isNewProduct => Id == null;
    protected bool _loading = true;
    protected bool _isSaving;
    protected string? _error;
    protected HashSet<string> _serverErrors = new();
    protected HashSet<string> _availableCategories = new();
    protected EditContext? _editContext;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _loading = true;
            await LoadProductAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading product");
            _error = "Failed to load product. Please try again.";
        }
        finally
        {
            _loading = false;
        }
    }

    protected async Task LoadProductAsync()
    {
        if (_isNewProduct)
        {
            _product = new Models.Product
            {
                Category = new Models.Category()
            };
        }
        else
        {
            var result = await ProductService.GetProductByIdAsync(Id!.Value);
            if (result == null)
            {
                _error = "Product not found";
                return;
            }
            _product = result;
        }

        // Get available categories
        var categories = await ProductService.GetCategoriesAsync();
        _availableCategories = new HashSet<string>(categories.Select(c => c.Name));

        // Create a new EditContext
        _editContext = new EditContext(_product);
    }

    protected async Task SaveProductAsync(EditContext context)
    {
        if (!context.Validate())
        {
            return;
        }

        try
        {
            _isSaving = true;
            _serverErrors.Clear();

            // Additional server-side validation
            if (string.IsNullOrWhiteSpace(_product.Name))
            {
                _serverErrors.Add("Product name is required.");
                return;
            }

            if (_product.Price <= 0)
            {
                _serverErrors.Add("Price must be greater than zero.");
                return;
            }

            if (_product.Stock < 0)
            {
                _serverErrors.Add("Stock cannot be negative.");
                return;
            }

            if (_isNewProduct)
            {
                await ProductService.CreateProductAsync(_product);
            }
            else
            {
                await ProductService.UpdateProductAsync(Id!.Value, _product);
            }

            NavigateBack();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving product");
            _serverErrors.Add("Failed to save product. Please try again.");
        }
        finally
        {
            _isSaving = false;
        }
    }

    protected void NavigateBack()
    {
        NavigationManager.NavigateTo("/products");
    }
}