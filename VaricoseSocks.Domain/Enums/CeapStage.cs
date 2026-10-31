using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.Domain.Enums;

public enum CeapStage
{
    C0 = 0, // Không thấy hoặc không sờ thấy dấu hiệu bệnh tĩnh mạch (Mỏi nhẹ)
    C1 = 1, // Giãn mao mạch hoặc tĩnh mạch dạng lưới (Mạng nhện, < 3mm)
    C2 = 2, // Giãn tĩnh mạch nông ngoằn ngoèo (> 3mm)
    C3 = 3, // Phù chi dưới nhưng chưa có biến đổi da
    C4 = 4, // Loạn dưỡng da: xơ mỡ da, sạm da, chàm tĩnh mạch
    C5 = 5, // Biến đổi da có vết loét tĩnh mạch đã liền sẹo
    C6 = 6  // Vết loét tĩnh mạch đang tiến triển / hở miệng
}
