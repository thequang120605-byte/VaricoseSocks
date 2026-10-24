using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Web.Models;
using VaricoseSocks.Web.Services;

namespace VaricoseSocks.Web.Controllers;

public class CheckoutController : Controller
{
    private readonly IApiService _apiService;

    public CheckoutController(IApiService apiService)
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

        if (!cart.Items.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        string name = User.Identity?.IsAuthenticated == true ? (User.Identity.Name ?? "Chu Văn Quốc Trung") : "Chu Văn Quốc Trung";
        string email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "trung.chu@varicosesocks.vn";
        string phone = User.FindFirst("Phone")?.Value ?? "0987654321";
        string address = User.FindFirst("Address")?.Value ?? "456 Lê Văn Sỹ, Phường 14";

        var model = new CheckoutViewModel
        {
            Cart = cart,
            CustomerName = name,
            CustomerEmail = email,
            CustomerPhone = phone,
            ShippingAddress = address,
            City = "Hồ Chí Minh",
            District = "Quận 3"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(CheckoutViewModel model)
    {
        var sessionId = GetOrCreateSessionId();
        var cart = await _apiService.GetCartAsync(null, sessionId);
        model.Cart = cart;

        if (!cart.Items.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

        var order = await _apiService.CreateOrderAsync(model, sessionId, userId);
        if (order != null)
        {
            return RedirectToAction(nameof(Success), new { orderCode = order.OrderCode });
        }

        ModelState.AddModelError("", "Đã có lỗi khi tạo đơn hàng. Vui lòng thử lại.");
        return View("Index", model);
    }

    public async Task<IActionResult> Success(string orderCode)
    {
        var order = await _apiService.GetOrderByCodeAsync(orderCode);
        if (order == null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(order);
    }
}
