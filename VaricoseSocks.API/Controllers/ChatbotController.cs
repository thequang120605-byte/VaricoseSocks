using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;

namespace VaricoseSocks.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotAiService _chatbotService;

    public ChatbotController(IChatbotAiService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    [HttpPost("consult")]
    public async Task<IActionResult> Consult([FromBody] ChatbotConsultRequestDto request)
    {
        var result = await _chatbotService.ConsultAsync(request);
        return Ok(result);
    }

    [HttpGet("calculate-size")]
    public IActionResult CalculateSize([FromQuery] double ankleCm, [FromQuery] double? calfCm, [FromQuery] double? thighCm)
    {
        if (ankleCm <= 0)
        {
            return BadRequest(new { message = "Số đo vòng cổ chân cB phải lớn hơn 0." });
        }

        var result = _chatbotService.CalculateSize(ankleCm, calfCm, thighCm);
        return Ok(result);
    }
}
