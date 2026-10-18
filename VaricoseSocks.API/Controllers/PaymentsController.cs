using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Domain.Enums;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("methods")]
    public IActionResult GetPaymentMethods()
    {
        var methods = new[]
        {
            new { id = 1, code = "COD", name = "Thanh toán khi nhận hàng (Cash On Delivery)", isOnline = false },
            new { id = 2, code = "VIETQR", name = "Chuyển khoản VietQR tự động qua App Ngân hàng", isOnline = true },
            new { id = 3, code = "VNPAY", name = "Cổng thanh toán điện tử VNPay (ATM / Visa / QR)", isOnline = true },
            new { id = 4, code = "MOMO", name = "Ví điện tử MoMo", isOnline = true }
        };
        return Ok(methods);
    }

    [HttpPost("vietqr")]
    public IActionResult GenerateVietQr([FromBody] VietQrRequestDto dto)
    {
        // Tích hợp sinh URL ảnh VietQR chuẩn Ngân hàng Nhà nước
        var bankId = "970422"; // MB Bank
        var accountNo = "0901234567";
        var accountName = Uri.EscapeDataString("VARICOSE SOCKS MEDICAL");
        var memo = Uri.EscapeDataString(dto.OrderCode);
        var qrUrl = $"https://api.vietqr.io/image/{bankId}-{accountNo}-compact2.jpg?amount={dto.Amount}&addInfo={memo}&accountName={accountName}";

        return Ok(new
        {
            bankName = "Ngân hàng Quân Đội (MB Bank)",
            accountNumber = accountNo,
            accountName = "VARICOSE SOCKS MEDICAL",
            amount = dto.Amount,
            memo = dto.OrderCode,
            qrImageUrl = qrUrl
        });
    }

    [HttpPost("vnpay/create-url")]
    public IActionResult CreateVNPayUrl([FromBody] VNPayRequestDto dto)
    {
        // Demo Sandbox URL chuẩn cho VNPay Gateway
        var vnpayUrl = $"https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount={dto.Amount * 100}&vnp_Command=pay&vnp_CreateDate={DateTime.UtcNow:yyyyMMddHHmmss}&vnp_CurrCode=VND&vnp_IpAddr=127.0.0.1&vnp_Locale=vn&vnp_OrderInfo={dto.OrderCode}&vnp_OrderType=medical&vnp_ReturnUrl=http://localhost:5118/Checkout/Success?orderCode={dto.OrderCode}&vnp_TmnCode=VARICOSE&vnp_TxnRef={dto.OrderCode}";

        return Ok(new
        {
            paymentUrl = vnpayUrl,
            orderCode = dto.OrderCode,
            amount = dto.Amount
        });
    }

    [HttpPost("callback")]
    public async Task<IActionResult> PaymentCallback([FromBody] PaymentCallbackDto dto)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == dto.OrderCode);
        if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

        if (dto.Success)
        {
            order.PaymentStatus = PaymentStatus.Paid;
            order.OrderStatus = OrderStatus.Confirmed;
            order.UpdatedAt = DateTime.UtcNow;

            var payment = new Payment
            {
                OrderId = order.Id,
                Method = (PaymentMethod)dto.PaymentMethod,
                TransactionId = dto.TransactionId ?? Guid.NewGuid().ToString("N"),
                Amount = dto.Amount,
                Status = PaymentStatus.Paid,
                PaidAt = DateTime.UtcNow,
                ResponsePayload = dto.RawResponse,
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Thanh toán thành công và đã cập nhật trạng thái đơn." });
        }

        return BadRequest(new { success = false, message = "Giao dịch không thành công." });
    }
}

public class VietQrRequestDto
{
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class VNPayRequestDto
{
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PaymentCallbackDto
{
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public bool Success { get; set; }
    public string? RawResponse { get; set; }
}
