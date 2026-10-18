using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Web.Services;

namespace VaricoseSocks.Web.Controllers;

public class ChatbotController : Controller
{
    private readonly IApiService _apiService;

    public ChatbotController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] ChatbotRequestModel request)
    {
        var result = await _apiService.ConsultChatbotAsync(
            request.Message ?? "",
            request.AnkleCm,
            request.CalfCm,
            request.ThighCm,
            request.IsPregnant,
            request.HasVaricose,
            request.HasSwelling
        );

        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> CalculateSize(double ankleCm, double? calfCm, double? thighCm)
    {
        var result = await _apiService.CalculateSizeAsync(ankleCm, calfCm, thighCm);
        return Json(result);
    }
}

public class ChatbotRequestModel
{
    public string? Message { get; set; }
    public double? AnkleCm { get; set; }
    public double? CalfCm { get; set; }
    public double? ThighCm { get; set; }
    public bool IsPregnant { get; set; }
    public bool HasVaricose { get; set; }
    public bool HasSwelling { get; set; }
}
