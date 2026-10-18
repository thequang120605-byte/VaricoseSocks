using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.Application.Services;

public class ChatbotAiService : IChatbotAiService
{
    private readonly IProductRepository _productRepository;
    private readonly IChatbotRepository _chatbotRepository;

    public ChatbotAiService(IProductRepository productRepository, IChatbotRepository chatbotRepository)
    {
        _productRepository = productRepository;
        _chatbotRepository = chatbotRepository;
    }

    public async Task<ChatbotConsultResponseDto> ConsultAsync(ChatbotConsultRequestDto request)
    {
        var response = new ChatbotConsultResponseDto();
        var msg = (request.Message ?? string.Empty).ToLower();

        // 1. Phân tích triệu chứng & xác định cấp độ áp lực CCL
        if (request.IsPregnant || msg.Contains("bầu") || msg.Contains("thai kỳ") || msg.Contains("mang thai"))
        {
            response.RecommendedCompression = CompressionLevel.CCL1;
            response.RecommendedCompressionText = "CCL1 (15 - 20 mmHg)";
            response.RecommendedStyle = SockStyle.Maternity;
            response.RecommendedStyleText = "Quần vớ y khoa chuyên biệt cho bà bầu";
            response.Advice = "Trong thai kỳ, nội tiết tố thay đổi và áp lực tử cung chèn ép tĩnh mạch chậu. Bạn nên chọn dòng quần vớ bầu CCL1 với đai co giãn nhẹ vùng bụng để giảm phù nề, ngăn ngừa chuột rút và bảo vệ tĩnh mạch.";
            response.ReplyMessage = "Chào bạn! Với tình trạng đang mang thai, chuyên gia khuyến nghị bạn sử dụng Quần vớ y khoa bà bầu cấp độ nén CCL1 (15-20 mmHg). Dòng sản phẩm này có đai nâng bụng êm dịu và lực nén chuẩn y tế giúp giảm nhức mỏi chân hiệu quả.";
        }
        else if (request.HasSevereSwelling || msg.Contains("loét") || msg.Contains("huyết khối") || msg.Contains("dvt") || msg.Contains("nặng") || msg.Contains("sưng to"))
        {
            response.RecommendedCompression = CompressionLevel.CCL3;
            response.RecommendedCompressionText = "CCL3 (34 - 46 mmHg)";
            response.RecommendedStyle = SockStyle.ThighHigh;
            response.RecommendedStyleText = "Vớ đùi y khoa áp lực cao";
            response.Advice = "Bạn có biểu hiện suy tĩnh mạch giai đoạn tiến triển (CEAP C4-C6). Cần đi khám chuyên khoa mạch máu để siêu âm Doppler. Cấp độ nén CCL3 mang lại lực ép hỗ trợ mạnh nhất để lưu thông dòng máu tĩnh mạch sâu.";
            response.ReplyMessage = "Chào bạn, các dấu hiệu như phù cứng, sưng to kéo dài hoặc đổi màu da là giai đoạn suy tĩnh mạch nặng. Bạn nên tham khảo ý kiến bác sĩ mạch máu và sử dụng vớ y khoa chuẩn áp lực CCL3 để kiểm soát tuần hoàn chi dưới.";
        }
        else if (request.HasVisibleVaricoseVeins || msg.Contains("gân xanh") || msg.Contains("phù") || msg.Contains("búi tĩnh mạch") || msg.Contains("đau nhức") || msg.Contains("nổi gân"))
        {
            response.RecommendedCompression = CompressionLevel.CCL2;
            response.RecommendedCompressionText = "CCL2 (23 - 32 mmHg)";
            response.RecommendedStyle = SockStyle.KneeHigh;
            response.RecommendedStyleText = "Vớ gối y khoa hoặc Vớ đùi CCL2";
            response.Advice = "Khi tĩnh mạch đã nổi gân ngoằn ngoèo hoặc chân thường bị nặng phù vào chiều tối, cấp độ nén tiêu chuẩn điều trị y khoa là CCL2 (20-30 mmHg). Áp lực giảm dần từ 100% tại cổ chân xuống 70% tại bắp chân sẽ khép kín van tĩnh mạch.";
            response.ReplyMessage = "Chào bạn! Biểu hiện nổi gân xanh và nhức phù chân là triệu chứng điển hình của suy giãn tĩnh mạch nông (CEAP C2-C3). Lựa chọn tối ưu nhất là Vớ y khoa chuẩn áp lực CCL2 (20-30 mmHg) xuất xứ Mỹ/Đức/Ý.";
        }
        else
        {
            response.RecommendedCompression = CompressionLevel.CCL1;
            response.RecommendedCompressionText = "CCL1 (15 - 20 mmHg)";
            response.RecommendedStyle = SockStyle.KneeHigh;
            response.RecommendedStyleText = "Vớ gối y khoa phòng ngừa";
            response.Advice = "Nếu bạn chỉ cảm thấy mỏi chân sau ngày dài đứng hoặc ngồi nhiều, vớ cấp độ CCL1 (15-20 mmHg) là giải pháp phòng ngừa tuyệt vời giúp phục hồi van tĩnh mạch và đem lại cảm giác nhẹ nhõm cho đôi chân.";
            response.ReplyMessage = "Chào bạn! Để phòng ngừa mỏi chân và giãn tĩnh mạch khi đứng/ngồi lâu, bạn nên dùng dòng vớ y khoa phòng ngừa cấp độ CCL1 (15-20 mmHg), chất liệu mềm mịn, thoáng khí.";
        }

        // 2. Tính toán size tự động nếu có số đo
        if (request.AnkleCircumferenceCm.HasValue && request.AnkleCircumferenceCm.Value > 0)
        {
            var sizeResult = CalculateSize(request.AnkleCircumferenceCm.Value, request.CalfCircumferenceCm, request.ThighCircumferenceCm);
            response.RecommendedSize = sizeResult.RecommendedSize;
            response.Advice += $" Dựa trên số đo cổ chân ({request.AnkleCircumferenceCm:F1} cm), size vớ chuẩn nhất của bạn là **Size {sizeResult.RecommendedSize}** ({sizeResult.Explanation}).";
        }

        // 3. Lấy sản phẩm gợi ý từ cơ sở dữ liệu
        var products = await _productRepository.GetAllAsync(compression: response.RecommendedCompression);
        response.RecommendedProducts = products.Take(3).Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Sku = p.Sku,
            Price = p.Price,
            DiscountPrice = p.DiscountPrice,
            ImageUrl = p.ImageUrl,
            Brand = p.Brand,
            Origin = p.Origin,
            CompressionLevel = p.CompressionLevel,
            Style = p.Style,
            ToeType = p.ToeType
        }).ToList();

        // 4. Lưu lại lịch sử tư vấn
        try
        {
            var consultation = new ChatbotConsultation
            {
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                SymptomsDescription = request.Message,
                AnkleCircumferenceCm = request.AnkleCircumferenceCm,
                CalfCircumferenceCm = request.CalfCircumferenceCm,
                ThighCircumferenceCm = request.ThighCircumferenceCm,
                RecommendedCompression = response.RecommendedCompression,
                RecommendedSize = response.RecommendedSize,
                RecommendedStyle = response.RecommendedStyle,
                AdviceSummary = response.Advice,
                RecommendedProductIdsJson = string.Join(",", response.RecommendedProducts.Select(p => p.Id)),
                CreatedAt = DateTime.UtcNow
            };
            await _chatbotRepository.SaveConsultationAsync(consultation);
        }
        catch
        {
            // Bỏ qua lỗi lưu log nếu db đang khởi tạo
        }

        return response;
    }

    public SizeCalculationResultDto CalculateSize(double ankleCircumferenceCm, double? calfCircumferenceCm = null, double? thighCircumferenceCm = null)
    {
        // Chuẩn RAL-GZ 387 (Đức) cho vớ y khoa:
        string size;
        string explanation;

        if (ankleCircumferenceCm < 18)
        {
            size = "S";
            explanation = "Cổ chân nhỏ (< 18cm)";
        }
        else if (ankleCircumferenceCm <= 21)
        {
            size = "S";
            explanation = "Vòng cổ chân cB từ 18 - 21 cm";
        }
        else if (ankleCircumferenceCm <= 25)
        {
            size = "M";
            explanation = "Vòng cổ chân cB từ 21 - 25 cm";
        }
        else if (ankleCircumferenceCm <= 29)
        {
            size = "L";
            explanation = "Vòng cổ chân cB từ 25 - 29 cm";
        }
        else if (ankleCircumferenceCm <= 33)
        {
            size = "XL";
            explanation = "Vòng cổ chân cB từ 29 - 33 cm";
        }
        else
        {
            size = "XXL";
            explanation = "Vòng cổ chân cB trên 33 cm";
        }

        // Tinh chỉnh nếu bắp chân nở lớn
        if (calfCircumferenceCm.HasValue && calfCircumferenceCm.Value > 42 && (size == "S" || size == "M"))
        {
            size = size == "S" ? "M" : "L";
            explanation += " (đã điều chỉnh tăng 1 size do vòng bắp chân nở to)";
        }

        return new SizeCalculationResultDto
        {
            RecommendedSize = size,
            Explanation = explanation,
            IsAccurate = true
        };
    }
}
