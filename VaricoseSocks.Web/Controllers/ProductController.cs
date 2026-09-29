using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Web.Models;
using VaricoseSocks.Web.Services;

namespace VaricoseSocks.Web.Controllers;

public class ProductController : Controller
{
    private readonly IApiService _apiService;

    public ProductController(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index(int? categoryId, string? searchTerm)
    {
        var categories = await _apiService.GetCategoriesAsync();
        var products = await _apiService.GetProductsAsync(categoryId, searchTerm);

        var viewModel = new ProductListViewModel
        {
            Categories = categories,
            Products = products,
            SelectedCategoryId = categoryId,
            SearchTerm = searchTerm
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var product = await _apiService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }
}