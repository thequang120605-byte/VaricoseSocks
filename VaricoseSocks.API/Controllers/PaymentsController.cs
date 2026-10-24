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
}
