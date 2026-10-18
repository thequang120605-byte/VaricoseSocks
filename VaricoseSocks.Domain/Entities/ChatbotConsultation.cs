using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.Domain.Entities;

public class ChatbotConsultation
{
    public int Id { get; set; }
    public string? SessionId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? SymptomsDescription { get; set; }        // Ví dụ: Đau mỏi chân chiều tối, nổi gân xanh
    public double? AnkleCircumferenceCm { get; set; }      // Chu vi cổ chân (cB)
    public double? CalfCircumferenceCm { get; set; }       // Chu vi bắp chân (cC)
    public double? ThighCircumferenceCm { get; set; }      // Chu vi đùi (cG)
    public CompressionLevel? RecommendedCompression { get; set; }
    public string? RecommendedSize { get; set; }           // S, M, L, XL
    public SockStyle? RecommendedStyle { get; set; }
    public string? AdviceSummary { get; set; }             // Lời khuyên của AI
    public string? RecommendedProductIdsJson { get; set; } // Danh sách ID sản phẩm gợi ý
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
