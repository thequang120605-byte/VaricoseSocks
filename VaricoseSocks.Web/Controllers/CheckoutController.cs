using Microsoft.AspNetCore.Authorization;
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

    [Authorize]
    public async Task<IActionResult> Index(string? voucher = null)
    {
        var sessionId = GetOrCreateSessionId();
        var cart = await _apiService.GetCartAsync(null, sessionId);

        if (!cart.Items.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        // Ràng buộc tồn kho: Nếu có sản phẩm vượt quá tồn kho khả dụng, yêu cầu người dùng điều chỉnh
        var overflowItem = cart.Items.FirstOrDefault(i => i.Quantity > i.StockQuantity);
        if (overflowItem != null)
        {
            TempData["ErrorMessage"] = $"Sản phẩm '{overflowItem.ProductName}' (Size {overflowItem.Size}) chỉ còn {overflowItem.StockQuantity} sản phẩm trong kho (bạn đang đặt {overflowItem.Quantity}). Vui lòng điều chỉnh lại số lượng giỏ hàng.";
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

    [Authorize]
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

        var (order, errorMessage) = await _apiService.CreateOrderAsync(model, sessionId, userId);
        if (order != null)
        {
            // Làm trống giỏ hàng và đặt lại badge giỏ hàng về 0 sau khi xác nhận đặt hàng thành công
            await _apiService.ClearCartAsync(userId, sessionId);
            HttpContext.Session.SetInt32("CartCount", 0);

            if (model.PaymentMethod == 2) // 2: Chuyển khoản ngân hàng VietQR -> Chuyển sang Cổng Quét Mã Thanh Toán 15 phút
            {
                return RedirectToAction(nameof(Payment), new { orderCode = order.OrderCode });
            }

            return RedirectToAction(nameof(Success), new { orderCode = order.OrderCode });
        }

        ModelState.AddModelError("", errorMessage ?? "Đã có lỗi khi tạo đơn hàng. Vui lòng thử lại.");
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

    [AllowAnonymous]
    public async Task<IActionResult> Payment(string orderCode)
    {
        var order = await _apiService.GetOrderByCodeAsync(orderCode);
        if (order == null)
        {
            return RedirectToAction("Index", "Home");
        }

        if (order.PaymentStatusText.Contains("Đã thanh toán", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Success), new { orderCode = order.OrderCode });
        }

        return View(order);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CheckPaymentStatus(string orderCode)
    {
        var order = await _apiService.GetOrderByCodeAsync(orderCode);
        if (order == null)
        {
            return Json(new { success = false, isPaid = false, message = "Không tìm thấy đơn hàng." });
        }

        bool isPaid = order.PaymentStatusText.Contains("Đã thanh toán", StringComparison.OrdinalIgnoreCase);
        return Json(new
        {
            success = true,
            orderCode = order.OrderCode,
            isPaid = isPaid,
            paymentStatus = order.PaymentStatusText,
            orderStatus = order.OrderStatusText
        });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SimulateBankTransfer([FromBody] SimulateTransferRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.OrderCode))
        {
            return Json(new { success = false, message = "Mã đơn hàng không hợp lệ." });
        }

        var order = await _apiService.GetOrderByCodeAsync(req.OrderCode);
        if (order == null)
        {
            return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
        }

        var success = await _apiService.ConfirmVietQrPaymentAsync(order.OrderCode, order.TotalAmount);
        return Json(new { success, message = success ? "Khớp lệnh Vietcombank thành công!" : "Lỗi khi xử lý giao dịch." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmPayment(string orderCode)
    {
        var order = await _apiService.GetOrderByCodeAsync(orderCode);
        if (order == null)
        {
            return RedirectToAction("Index", "Home");
        }

        await _apiService.ConfirmVietQrPaymentAsync(orderCode, order.TotalAmount);
        return RedirectToAction(nameof(Success), new { orderCode = order.OrderCode, paid = true });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Success(string orderCode, bool? paid = null)
    {
        var order = await _apiService.GetOrderByCodeAsync(orderCode);
        if (order == null)
        {
            return RedirectToAction("Index", "Home");
        }

        if (paid == true)
        {
            ViewBag.JustPaid = true;
        }

        return View(order);
    }
}

public class VoucherRequestModel
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}

public class SimulateTransferRequest
{
    public string OrderCode { get; set; } = string.Empty;
}
