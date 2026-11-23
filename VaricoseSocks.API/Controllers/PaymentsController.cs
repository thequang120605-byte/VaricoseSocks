using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;

namespace VaricoseSocks.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("methods")]
    public IActionResult GetPaymentMethods()
    {
        var methods = _paymentService.GetPaymentMethods();
        return Ok(methods);
    }

    [HttpPost("vietqr")]
    public IActionResult GenerateVietQr([FromBody] VietQrRequestDto dto)
    {
        var result = _paymentService.GenerateVietQr(dto);
        return Ok(result);
    }

    [HttpPost("vnpay/create-url")]
    public IActionResult CreateVNPayUrl([FromBody] VNPayRequestDto dto)
    {
        var result = _paymentService.CreateVNPayUrl(dto);
        return Ok(result);
    }

    [HttpPost("compute-signature")]
    public IActionResult ComputeSignature([FromBody] PaymentCallbackDto dto)
    {
        var signature = _paymentService.GenerateCallbackSignature(dto);
        return Ok(new { signature });
    }

    [HttpPost("callback")]
    public async Task<IActionResult> PaymentCallback([FromBody] PaymentCallbackDto dto)
    {
        var result = await _paymentService.ProcessCallbackAsync(dto);
        if (!result.Success)
        {
            return BadRequest(new { success = false, message = result.Message });
        }

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("confirm-vietqr")]
    public async Task<IActionResult> ConfirmVietQr([FromBody] VietQrRequestDto dto)
    {
        var callback = new PaymentCallbackDto
        {
            OrderCode = dto.OrderCode,
            Amount = dto.Amount,
            PaymentMethod = 2, // VietQR
            TransactionId = "VIETQR-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            Success = true,
            RawResponse = "User confirmed Vietcombank transfer on UI"
        };
        callback.Signature = _paymentService.GenerateCallbackSignature(callback);
        var result = await _paymentService.ProcessCallbackAsync(callback);
        return Ok(new { success = result.Success, message = result.Message });
    }
}
