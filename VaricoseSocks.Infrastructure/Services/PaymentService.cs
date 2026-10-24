using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Domain.Enums;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public PaymentService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    private string GetWebhookSecret()
    {
        return Environment.GetEnvironmentVariable("PAYMENT_WEBHOOK_SECRET")
            ?? _configuration["PaymentSettings:WebhookSecret"]
            ?? "VaricoseSocks_Medical_Payment_Webhook_Secure_Secret_2026#";
    }

    public string GenerateCallbackSignature(PaymentCallbackDto dto)
    {
        var rawData = $"{dto.OrderCode}|{dto.Amount:0.00}|{dto.PaymentMethod}|{dto.TransactionId ?? ""}|{dto.Success.ToString().ToLowerInvariant()}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(GetWebhookSecret()));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public IEnumerable<PaymentMethodDto> GetPaymentMethods()
    {
        return new List<PaymentMethodDto>
        {
            new() { Id = 1, Code = "COD", Name = "Thanh toán khi nhận hàng (Cash On Delivery)", IsOnline = false },
            new() { Id = 2, Code = "VIETQR", Name = "Chuyển khoản VietQR tự động qua App Ngân hàng", IsOnline = true },
            new() { Id = 3, Code = "VNPAY", Name = "Cổng thanh toán điện tử VNPay (ATM / Visa / QR)", IsOnline = true },
            new() { Id = 4, Code = "MOMO", Name = "Ví điện tử MoMo", IsOnline = true }
        };
    }

    public object GenerateVietQr(VietQrRequestDto dto)
    {
        var bankId = "970422"; // MB Bank
        var accountNo = "0901234567";
        var accountName = Uri.EscapeDataString("VARICOSE SOCKS MEDICAL");
        var memo = Uri.EscapeDataString(dto.OrderCode);
        var qrUrl = $"https://api.vietqr.io/image/{bankId}-{accountNo}-compact2.jpg?amount={dto.Amount}&addInfo={memo}&accountName={accountName}";

        return new
        {
            bankName = "Ngân hàng Quân Đội (MB Bank)",
            accountNumber = accountNo,
            accountName = "VARICOSE SOCKS MEDICAL",
            amount = dto.Amount,
            memo = dto.OrderCode,
            qrImageUrl = qrUrl
        };
    }

    public object CreateVNPayUrl(VNPayRequestDto dto)
    {
        var vnpayUrl = $"https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount={dto.Amount * 100}&vnp_Command=pay&vnp_CreateDate={DateTime.UtcNow:yyyyMMddHHmmss}&vnp_CurrCode=VND&vnp_IpAddr=127.0.0.1&vnp_Locale=vn&vnp_OrderInfo={dto.OrderCode}&vnp_OrderType=medical&vnp_ReturnUrl=http://localhost:5118/Checkout/Success?orderCode={dto.OrderCode}&vnp_TmnCode=VARICOSE&vnp_TxnRef={dto.OrderCode}";

        return new
        {
            paymentUrl = vnpayUrl,
            orderCode = dto.OrderCode,
            amount = dto.Amount
        };
    }

    public async Task<(bool Success, string Message)> ProcessCallbackAsync(PaymentCallbackDto dto)
    {
        // 1. Signature Verification (HMAC-SHA256)
        if (string.IsNullOrWhiteSpace(dto.Signature))
        {
            return (false, "Chữ ký bảo mật (Signature) không được để trống.");
        }

        var expectedSignature = GenerateCallbackSignature(dto);
        if (!string.Equals(expectedSignature, dto.Signature, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Chữ ký bảo mật (Signature) không hợp lệ.");
        }

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == dto.OrderCode);
        if (order == null)
        {
            return (false, "Không tìm thấy đơn hàng tương ứng với mã yêu cầu.");
        }

        // 2. Amount integrity verification
        if (dto.Amount > 0 && Math.Abs(dto.Amount - order.TotalAmount) > 1.0m)
        {
            return (false, $"Số tiền thanh toán ({dto.Amount:N0} ₫) không khớp với tổng tiền đơn hàng ({order.TotalAmount:N0} ₫).");
        }

        var transactionId = !string.IsNullOrWhiteSpace(dto.TransactionId) ? dto.TransactionId : Guid.NewGuid().ToString("N");

        // 3. Idempotency check: prevent double charging / double processing
        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId && p.Status == PaymentStatus.Paid);

        if (existingPayment != null)
        {
            return (true, "Giao dịch thanh toán này đã được xác nhận trước đó (Idempotent).");
        }

        if (dto.Success)
        {
            order.PaymentStatus = PaymentStatus.Paid;
            if (order.OrderStatus == OrderStatus.Pending)
            {
                order.OrderStatus = OrderStatus.Confirmed;
            }
            order.UpdatedAt = DateTime.UtcNow;

            var payment = new Payment
            {
                OrderId = order.Id,
                Method = (PaymentMethod)dto.PaymentMethod,
                TransactionId = transactionId,
                Amount = dto.Amount > 0 ? dto.Amount : order.TotalAmount,
                Status = PaymentStatus.Paid,
                PaidAt = DateTime.UtcNow,
                ResponsePayload = dto.RawResponse,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return (true, "Thanh toán thành công và đã cập nhật trạng thái đơn hàng.");
        }
        else
        {
            order.PaymentStatus = PaymentStatus.Failed;
            order.UpdatedAt = DateTime.UtcNow;

            var failedPayment = new Payment
            {
                OrderId = order.Id,
                Method = (PaymentMethod)dto.PaymentMethod,
                TransactionId = transactionId,
                Amount = dto.Amount > 0 ? dto.Amount : order.TotalAmount,
                Status = PaymentStatus.Failed,
                PaidAt = DateTime.UtcNow,
                ResponsePayload = dto.RawResponse,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(failedPayment);
            await _context.SaveChangesAsync();

            return (false, "Giao dịch thanh toán bị từ chối hoặc thất bại.");
        }
    }
}
