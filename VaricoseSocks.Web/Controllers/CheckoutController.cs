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

    public async Task<IActionResult> Index(string? voucher = null)
    {
        var sessionId = GetOrCreateSessionId();
        var cart = await _apiService.GetCartAsync(null, sessionId);

        if (!cart.Items.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        string name = string.Empty;
        string email = string.Empty;
        string phone = string.Empty;
        string address = string.Empty;
        string city = "Hồ Chí Minh";
        string district = string.Empty;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var uid))
            {
                var profile = await _apiService.GetProfileAsync(uid);
                if (profile != null)
                {
                    name = profile.FullName;
                    email = profile.Email;
                    phone = profile.Phone ?? string.Empty;
                    address = profile.Address ?? string.Empty;
                    if (!string.IsNullOrEmpty(profile.City)) city = profile.City;
                    if (!string.IsNullOrEmpty(profile.District)) district = profile.District;
                }
            }

            if (string.IsNullOrEmpty(name)) name = User.Identity.Name ?? string.Empty;
            if (string.IsNullOrEmpty(email)) email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;
            if (string.IsNullOrEmpty(phone)) phone = User.FindFirst("Phone")?.Value ?? string.Empty;
            if (string.IsNullOrEmpty(address)) address = User.FindFirst("Address")?.Value ?? string.Empty;
        }

        var model = new CheckoutViewModel
        {
            Cart = cart,
            CustomerName = name,
            CustomerEmail = email,
            CustomerPhone = phone,
            ShippingAddress = address,
            City = city,
            District = district,
            VoucherCode = voucher
        };

        if (!string.IsNullOrWhiteSpace(voucher))
        {
            var vResult = await _apiService.ValidateVoucherAsync(voucher, cart.SubTotal);
            if (vResult != null && vResult.Valid)
            {
                model.DiscountAmount = vResult.DiscountAmount;
            }
        }

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

        // Ràng buộc bắt buộc khi Đặt hàng: Phải cung cấp Số điện thoại và Địa chỉ nhận hàng
        if (string.IsNullOrWhiteSpace(model.CustomerPhone) || model.CustomerPhone.Trim().Length < 8)
        {
            ModelState.AddModelError("CustomerPhone", "Ràng buộc đặt hàng: Quý khách vui lòng cung cấp Số điện thoại liên hệ để nhân viên y tế gọi xác nhận.");
        }

        if (string.IsNullOrWhiteSpace(model.ShippingAddress) || model.ShippingAddress.Trim().Length < 5)
        {
            ModelState.AddModelError("ShippingAddress", "Ràng buộc đặt hàng: Quý khách vui lòng cung cấp Địa chỉ nhận hàng chi tiết (Số nhà, tên đường, phường xã).");
        }

        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

        // Nếu người dùng đã đăng nhập nhưng chưa có SĐT/Địa chỉ, tự động đồng bộ cập nhật vào Profile Database
        if (userId.HasValue)
        {
            try
            {
                var profile = await _apiService.GetProfileAsync(userId.Value);
                if (profile != null && (string.IsNullOrWhiteSpace(profile.Phone) || string.IsNullOrWhiteSpace(profile.Address)))
                {
                    await _apiService.UpdateProfileAsync(userId.Value, new UpdateProfileViewModel
                    {
                        FullName = model.CustomerName,
                        Phone = model.CustomerPhone,
                        Address = model.ShippingAddress,
                        City = model.City,
                        District = model.District
                    });
                }
            }
            catch { }
        }

        var order = await _apiService.CreateOrderAsync(model, sessionId, userId);
        if (order != null)
        {
            return RedirectToAction(nameof(Success), new { orderCode = order.OrderCode });
        }

        ModelState.AddModelError("", "Đã có lỗi khi tạo đơn hàng. Vui lòng thử lại.");
        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> ValidateVoucher([FromBody] VoucherRequestModel req)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
        {
            return Json(new { success = false, message = "Vui lòng nhập mã giảm giá." });
        }

        var result = await _apiService.ValidateVoucherAsync(req.Code, req.OrderAmount);
        if (result != null && result.Valid)
        {
            return Json(new
            {
                success = true,
                code = result.Code,
                name = result.Name,
                discountAmount = result.DiscountAmount,
                finalAmount = result.FinalAmount,
                message = result.Message
            });
        }

        return Json(new
        {
            success = false,
            message = result?.Message ?? "Mã giảm giá không hợp lệ hoặc không đủ điều kiện."
        });
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

public class VoucherRequestModel
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}
