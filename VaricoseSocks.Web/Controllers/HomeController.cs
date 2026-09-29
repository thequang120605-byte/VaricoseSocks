using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Web.Models;
using VaricoseSocks.Web.Services;

namespace VaricoseSocks.Web.Controllers;

public class HomeController : Controller
{
    private readonly IApiService _apiService;

    public HomeController(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _apiService.GetCategoriesAsync();
        var featuredProducts = await _apiService.GetProductsAsync();
        
        var model = new ProductListViewModel
        {
            Categories = categories,
            Products = featuredProducts.Take(8).ToList()
        };
        return View(model);
    }
}