using System.Text.RegularExpressions;
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
        var rawMsg = request.Message ?? string.Empty;
        var msg = rawMsg.ToLower().Trim();

        // Tự động phân tích số đo vòng cổ chân (cB) từ nội dung tin nhắn nếu request chưa có
        if (!request.AnkleCircumferenceCm.HasValue || request.AnkleCircumferenceCm.Value <= 0)
        {
            var match = Regex.Match(msg, @"(\d+(\.\d+)?)\s*(cm)?");
            if (match.Success && double.TryParse(match.Groups[1].Value, out var parsedCm) && parsedCm >= 15 && parsedCm <= 45)
            {
                request.AnkleCircumferenceCm = parsedCm;
            }
        }

        // 1. Nhận diện các ý định hỗ trợ mua hàng đặc thù (Voucher, Đổi trả, Tra cứu đơn)
        if (msg.Contains("mã") || msg.Contains("voucher") || msg.Contains("giảm giá") || msg.Contains("khuyến mãi") || msg.Contains("ưu đãi"))
        {
            response.RecommendedCompression = CompressionLevel.CCL2;
            response.RecommendedCompressionText = "Áp dụng cho mọi cấp độ vớ";
            response.RecommendedSize = "Tất cả size";
            response.RecommendedStyleText = "Vớ y khoa chính hãng";
            response.ReplyMessage = "Chào bạn! Varicose Socks đang áp dụng chương trình ưu đãi đặc quyền cho khách hàng: Nhập mã voucher **VARICOSE10** để được **giảm ngay 10%** tổng giá trị đơn hàng và nhận **Miễn phí vận chuyển toàn quốc**. Bạn có thể bấm 'Sao chép' và chọn các sản phẩm gợi ý bên dưới để thêm nhanh vào giỏ hàng!";
            response.Advice = "Mã voucher VARICOSE10 áp dụng cho tất cả đơn hàng vớ gối, vớ đùi, quần vớ và phụ kiện y khoa.";
        }
        else if (msg.Contains("đổi trả") || msg.Contains("bảo hành") || msg.Contains("chính sách"))
        {
            response.RecommendedCompression = CompressionLevel.CCL2;
            response.RecommendedCompressionText = "Chuẩn RAL-GZ 387";
            response.ReplyMessage = "Varicose Socks cam kết chính sách y tế vượt trội:\n- **Đổi size miễn phí trong 30 ngày** nếu vớ không vừa chân hoặc bệnh nhân cảm thấy quá chật/quá lỏng.\n- **Bảo hành 6 tháng** đối với độ đàn hồi và lực nén chuẩn y tế theo chứng nhận RAL-GZ 387 của CHLB Đức.\n- Đổi trả tận nhà hoàn toàn miễn phí trên toàn quốc.";
            response.Advice = "Bạn chỉ cần giữ lại bao bì và hóa đơn, đội ngũ y tế sẽ hỗ trợ đổi size ngay trong 24 giờ.";
        }
        else if (msg.Contains("tra cứu") || msg.Contains("đơn hàng") || msg.Contains("vận chuyển") || msg.Contains("giao hàng"))
        {
            response.ReplyMessage = "Bạn có thể theo dõi hành trình đơn hàng trực tiếp bằng cách truy cập trang [Tra cứu đơn hàng](/Order/Track) và nhập Mã đơn hàng cùng Số điện thoại đã đặt. Đơn hàng nội thành giao hỏa tốc 2-4 giờ, ngoại thành và các tỉnh từ 1-3 ngày làm việc.";
            response.Advice = "Mọi đơn hàng đều được đóng gói bảo mật và cho phép kiểm tra hàng trước khi thanh toán.";
        }
        // 2. Phân tích triệu chứng lâm sàng & xác định cấp độ áp lực CCL
        else if (request.IsPregnant || msg.Contains("bầu") || msg.Contains("thai kỳ") || msg.Contains("mang thai"))
        {
            response.RecommendedCompression = CompressionLevel.CCL1;
            response.RecommendedCompressionText = "CCL1 (15 - 20 mmHg)";
            response.RecommendedStyle = SockStyle.Maternity;
            response.RecommendedStyleText = "Quần vớ y khoa cho bà bầu";
            response.Advice = "Trong thai kỳ, nội tiết tố thay đổi và áp lực tử cung chèn ép tĩnh mạch chậu. Dòng quần vớ bầu CCL1 với đai co giãn nâng bụng sẽ giúp ngăn ngừa chuột rút và giảm phù nề hiệu quả.";
            response.ReplyMessage = "Chào bạn! Với tình trạng thai kỳ, bác sĩ khuyến nghị sử dụng **Quần vớ y khoa bà bầu cấp độ CCL1 (15-20 mmHg)**. Thiết kế đai nâng bụng êm dịu kết hợp lực nén vi sợi y học giúp đôi chân mẹ bầu luôn nhẹ nhàng, khỏe khoắn.";
        }
        else if (request.HasSevereSwelling || msg.Contains("loét") || msg.Contains("huyết khối") || msg.Contains("dvt") || msg.Contains("sưng to") || msg.Contains("c5") || msg.Contains("c6"))
        {
            response.RecommendedCompression = CompressionLevel.CCL3;
            response.RecommendedCompressionText = "CCL3 (34 - 46 mmHg)";
            response.RecommendedStyle = SockStyle.ThighHigh;
            response.RecommendedStyleText = "Vớ đùi y khoa áp lực cao (CCL3)";
            response.Advice = "Dấu hiệu suy tĩnh mạch tiến triển (CEAP C4-C6). Cần khám siêu âm Doppler mạch máu. Cấp độ CCL3 đem lại lực ép mạnh hỗ trợ dòng máu hồi lưu tĩnh mạch sâu.";
            response.ReplyMessage = "Chào bạn! Các biểu hiện phù cứng, sưng to kéo dài hoặc đổi màu da là giai đoạn suy tĩnh mạch tiến triển. Bác sĩ khuyến nghị bạn sử dụng **Vớ đùi y khoa chuẩn áp lực cao CCL3 (34-46 mmHg)** để hỗ trợ phục hồi lưu thông tĩnh mạch.";
        }
        else if (request.HasVisibleVaricoseVeins || msg.Contains("gân xanh") || msg.Contains("phù") || msg.Contains("búi tĩnh mạch") || msg.Contains("đau nhức") || msg.Contains("nổi gân") || msg.Contains("c2") || msg.Contains("c3"))
        {
            response.RecommendedCompression = CompressionLevel.CCL2;
            response.RecommendedCompressionText = "CCL2 (23 - 32 mmHg)";
            response.RecommendedStyle = SockStyle.KneeHigh;
            response.RecommendedStyleText = "Vớ gối hoặc Vớ đùi CCL2";
            response.Advice = "Khi tĩnh mạch đã nổi gân ngoằn ngoèo hoặc chân phù nặng vào buổi chiều, cấp độ điều trị chuẩn y khoa là CCL2 (20-30 mmHg). Lực nén giảm dần khép kín van tĩnh mạch.";
            response.ReplyMessage = "Chào bạn! Biểu hiện nổi gân xanh và nhức phù chân là triệu chứng điển hình của suy giãn tĩnh mạch nông (CEAP C2-C3). Lựa chọn tối ưu là **Vớ y khoa chuẩn áp lực điều trị CCL2 (23-32 mmHg)** nhập khẩu chính hãng.";
        }
        else
        {
            response.RecommendedCompression = CompressionLevel.CCL1;
            response.RecommendedCompressionText = "CCL1 (15 - 20 mmHg)";
            response.RecommendedStyle = SockStyle.KneeHigh;
            response.RecommendedStyleText = "Vớ gối y khoa phòng ngừa (CCL1)";
            response.Advice = "Phòng ngừa mỏi chân khi đứng nhiều hoặc ngồi văn phòng trên 2 tiếng liên tục. Lực nén CCL1 (15-20 mmHg) êm ái, thoáng khí giúp van tĩnh mạch thư giãn.";
            response.ReplyMessage = "Chào bạn! Để phòng ngừa mỏi chân và tĩnh mạch mạng nhện khi đứng hoặc ngồi lâu, bác sĩ khuyên dùng **Vớ gối phòng ngừa CCL1 (15-20 mmHg)** từ sợi dệt y tế mềm mịn, thoáng mát.";
        }

        // 3. Tính toán size vớ y khoa tự động nếu có số đo
        if (request.AnkleCircumferenceCm.HasValue && request.AnkleCircumferenceCm.Value > 0)
        {
            var sizeResult = CalculateSize(request.AnkleCircumferenceCm.Value, request.CalfCircumferenceCm, request.ThighCircumferenceCm);
            response.RecommendedSize = sizeResult.RecommendedSize;
            response.Advice += $" Dựa trên số đo vòng cổ chân ({request.AnkleCircumferenceCm:F1} cm), size vớ chuẩn nhất của bạn là **Size {sizeResult.RecommendedSize}** ({sizeResult.Explanation}).";
        }

        // 4. Lấy sản phẩm gợi ý từ cơ sở dữ liệu (ưu tiên theo cấp độ nén hoặc danh mục)
        var products = await _productRepository.GetAllAsync(compression: response.RecommendedCompression);
        if (!products.Any())
        {
            products = await _productRepository.GetAllAsync();
        }

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

        // 5. Lưu lại lịch sử tư vấn
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
