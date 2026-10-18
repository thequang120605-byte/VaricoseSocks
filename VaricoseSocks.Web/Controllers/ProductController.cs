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

    public async Task<IActionResult> Index(int? categoryId, string? searchTerm, int? compression, int? style, string? sortBy)
    {
        var categories = await _apiService.GetCategoriesAsync();
        var products = await _apiService.GetProductsAsync(categoryId, searchTerm, compression, style);

        // Sorting
        if (sortBy == "price_asc")
            products = products.OrderBy(p => p.DiscountPrice ?? p.Price).ToList();
        else if (sortBy == "price_desc")
            products = products.OrderByDescending(p => p.DiscountPrice ?? p.Price).ToList();
        else if (sortBy == "name")
            products = products.OrderBy(p => p.Name).ToList();

        var viewModel = new ProductListViewModel
        {
            Categories = categories,
            Products = products,
            SelectedCategoryId = categoryId,
            SearchTerm = searchTerm,
            CompressionFilter = compression,
            StyleFilter = style,
            SortBy = sortBy
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

        // Get related products
        var all = await _apiService.GetProductsAsync(product.CategoryId);
        ViewBag.RelatedProducts = all.Where(p => p.Id != product.Id).Take(4).ToList();

        return View(product);
    }
}