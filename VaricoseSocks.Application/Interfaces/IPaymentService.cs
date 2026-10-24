using VaricoseSocks.Application.DTOs;

namespace VaricoseSocks.Application.Interfaces;

public interface IPaymentService
{
    IEnumerable<PaymentMethodDto> GetPaymentMethods();
    object GenerateVietQr(VietQrRequestDto dto);
    object CreateVNPayUrl(VNPayRequestDto dto);
    Task<(bool Success, string Message)> ProcessCallbackAsync(PaymentCallbackDto dto);
    string GenerateCallbackSignature(PaymentCallbackDto dto);
}
