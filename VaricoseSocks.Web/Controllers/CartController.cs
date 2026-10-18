using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Web.Services;

namespace VaricoseSocks.Web.Controllers;

public class CartController : Controller
{
    private readonly IApiService _apiService;

    public CartController(IApiService apiService)
    {
        _apiService = apiService;
    }

    private string GetOrCreateSessionId()
    {
        var sessionId = HttpContext.Session.GetString("UserSessionId");
        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString("N");
            HttpContext.Session.SetString("UserSessionId", sessionId);
        }
        return sessionId;
    }

    public async Task<IActionResult> Index()
    {
        var sessionId = GetOrCreateSessionId();
        var cart = await _apiService.GetCartAsync(null, sessionId);
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, string? size, string? color, int quantity = 1)
    {
        var sessionId = GetOrCreateSessionId();
        var selectedSize = string.IsNullOrWhiteSpace(size) ? "M" : size;
        var selectedColor = string.IsNullOrWhiteSpace(color) ? "Màu Da (Beige)" : color;

        await _apiService.AddToCartAsync(productId, null, selectedSize, selectedColor, quantity, null, sessionId);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var cart = await _apiService.GetCartAsync(null, sessionId);
            return Json(new { success = true, totalQuantity = cart.TotalQuantity, subTotal = cart.SubTotal });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        await _apiService.UpdateCartItemQuantityAsync(cartItemId, quantity);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var sessionId = GetOrCreateSessionId();
            var cart = await _apiService.GetCartAsync(null, sessionId);
            return Json(new { success = true, totalQuantity = cart.TotalQuantity, subTotal = cart.SubTotal, totalAmount = cart.TotalAmount });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        await _apiService.RemoveCartItemAsync(cartItemId);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var sessionId = GetOrCreateSessionId();
            var cart = await _apiService.GetCartAsync(null, sessionId);
            return Json(new { success = true, totalQuantity = cart.TotalQuantity, subTotal = cart.SubTotal, totalAmount = cart.TotalAmount });
        }

        return RedirectToAction(nameof(Index));
    }
}
