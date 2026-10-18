using VaricoseSocks.Application.DTOs;

namespace VaricoseSocks.Application.Interfaces;

public interface IChatbotAiService
{
    Task<ChatbotConsultResponseDto> ConsultAsync(ChatbotConsultRequestDto request);
    SizeCalculationResultDto CalculateSize(double ankleCircumferenceCm, double? calfCircumferenceCm = null, double? thighCircumferenceCm = null);
}
