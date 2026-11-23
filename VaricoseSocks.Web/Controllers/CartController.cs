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
        HttpContext.Session.SetInt32("CartCount", cart.TotalQuantity);
        return View(cart);
    }

    [HttpGet]
    public async Task<IActionResult> GetCount()
    {
        var sessionId = GetOrCreateSessionId();
        var cart = await _apiService.GetCartAsync(null, sessionId);
        HttpContext.Session.SetInt32("CartCount", cart.TotalQuantity);
        return Json(new { count = cart.TotalQuantity });
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, string? size, string? color, int quantity = 1)
    {
        var sessionId = GetOrCreateSessionId();
        var selectedSize = string.IsNullOrWhiteSpace(size) ? "M" : size;
        var selectedColor = string.IsNullOrWhiteSpace(color) ? "Màu Da (Beige)" : color;

        var (success, errorMsg) = await _apiService.AddToCartAsync(productId, null, selectedSize, selectedColor, quantity, null, sessionId);
        var cart = await _apiService.GetCartAsync(null, sessionId);
        HttpContext.Session.SetInt32("CartCount", cart.TotalQuantity);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            if (!success)
            {
                return Json(new { success = false, message = errorMsg ?? "Không đủ tồn kho khả dụng." });
            }
            return Json(new { success = true, totalQuantity = cart.TotalQuantity, subTotal = cart.SubTotal });
        }

        if (!success)
        {
            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var (success, errorMsg) = await _apiService.UpdateCartItemQuantityAsync(cartItemId, quantity);
        var sessionId = GetOrCreateSessionId();
        var cart = await _apiService.GetCartAsync(null, sessionId);
        HttpContext.Session.SetInt32("CartCount", cart.TotalQuantity);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            if (!success)
            {
                return Json(new { success = false, message = errorMsg ?? "Số lượng vượt quá tồn kho khả dụng." });
            }
            return Json(new { success = true, totalQuantity = cart.TotalQuantity, subTotal = cart.SubTotal, totalAmount = cart.TotalAmount });
        }

        if (!success)
        {
            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        await _apiService.RemoveCartItemAsync(cartItemId);
        var sessionId = GetOrCreateSessionId();
        var cart = await _apiService.GetCartAsync(null, sessionId);
        HttpContext.Session.SetInt32("CartCount", cart.TotalQuantity);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, totalQuantity = cart.TotalQuantity, subTotal = cart.SubTotal, totalAmount = cart.TotalAmount });
        }

        return RedirectToAction(nameof(Index));
    }
}
