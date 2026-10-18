using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.Application.DTOs;

public class ChatbotConsultRequestDto
{
    public string? SessionId { get; set; }
    public string? Message { get; set; }
    public double? AnkleCircumferenceCm { get; set; }   // Vòng cổ chân cB (cm)
    public double? CalfCircumferenceCm { get; set; }    // Vòng bắp chân cC (cm)
    public double? ThighCircumferenceCm { get; set; }   // Vòng đùi cG (cm)
    public bool IsPregnant { get; set; }
    public bool HasVisibleVaricoseVeins { get; set; }   // Có nổi búi gân xanh rõ rệt
    public bool HasSevereSwelling { get; set; }         // Phù chân nặng vào cuối ngày
}

public class ChatbotConsultResponseDto
{
    public string ReplyMessage { get; set; } = string.Empty;
    public CompressionLevel? RecommendedCompression { get; set; }
    public string? RecommendedCompressionText { get; set; }
    public string? RecommendedSize { get; set; }
    public SockStyle? RecommendedStyle { get; set; }
    public string? RecommendedStyleText { get; set; }
    public string? Advice { get; set; }
    public List<ProductDto> RecommendedProducts { get; set; } = new();
}

public class SizeCalculationResultDto
{
    public string RecommendedSize { get; set; } = "M";
    public string Explanation { get; set; } = string.Empty;
    public bool IsAccurate { get; set; }
}
