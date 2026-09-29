using VaricoseSocks.Web.Models;

namespace VaricoseSocks.Web.Services;

public interface IApiService
{
    Task<List<CategoryViewModel>> GetCategoriesAsync();
    Task<List<ProductViewModel>> GetProductsAsync(int? categoryId = null, string? searchTerm = null);
    Task<ProductViewModel?> GetProductByIdAsync(int id);
}