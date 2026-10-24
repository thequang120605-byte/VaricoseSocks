using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Web.Models;
using VaricoseSocks.Web.Services;

namespace VaricoseSocks.Web.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class AdminController : Controller
{
    private readonly IApiService _apiService;

    public AdminController(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _apiService.GetProductsAsync();
        var orders = await _apiService.GetAllOrdersAsync();

        var model = new AdminDashboardViewModel
        {
            TotalProducts = products.Count,
            TotalOrders = orders.Count,
            TotalRevenue = orders.Where(o => o.OrderStatus != 6).Sum(o => o.TotalAmount),
            PendingOrdersCount = orders.Count(o => o.OrderStatus == 1),
            RecentOrders = orders.Take(5).ToList(),
            LowStockProducts = products.Where(p => p.StockQuantity <= 50).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Products()
    {
        var products = await _apiService.GetProductsAsync();
        return View(products);
    }

    public async Task<IActionResult> CreateProduct()
    {
        var categories = await _apiService.GetCategoriesAsync();
        var model = new AdminProductEditViewModel
        {
            Categories = categories,
            StockQuantity = 50,
            Price = 500000
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(AdminProductEditViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name) || model.Price <= 0 || model.CategoryId <= 0)
        {
            model.Categories = await _apiService.GetCategoriesAsync();
            ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin bắt buộc.");
            return View(model);
        }

        await _apiService.CreateProductAsync(model);
        return RedirectToAction(nameof(Products));
    }

    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await _apiService.GetProductByIdAsync(id);
        if (product == null) return NotFound();

        var categories = await _apiService.GetCategoriesAsync();
        var model = new AdminProductEditViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            ImageUrl = product.ImageUrl,
            CompressionLevel = product.CompressionLevel,
            Style = product.Style,
            ToeType = product.ToeType,
            Brand = product.Brand,
            Origin = product.Origin,
            Material = product.Material,
            IsFeatured = product.IsFeatured,
            Categories = categories
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(int id, AdminProductEditViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name) || model.Price <= 0 || model.CategoryId <= 0)
        {
            model.Categories = await _apiService.GetCategoriesAsync();
            ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin bắt buộc.");
            return View(model);
        }

        await _apiService.UpdateProductAsync(id, model);
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _apiService.DeleteProductAsync(id);
        return RedirectToAction(nameof(Products));
    }

    public async Task<IActionResult> Orders(int? status)
    {
        var orders = await _apiService.GetAllOrdersAsync(status);
        ViewBag.SelectedStatus = status;
        return View(orders);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, int status)
    {
        await _apiService.UpdateOrderStatusAsync(orderId, status);
        return RedirectToAction(nameof(Orders));
    }
}
