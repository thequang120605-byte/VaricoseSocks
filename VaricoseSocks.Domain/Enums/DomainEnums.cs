namespace VaricoseSocks.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,       // Chờ xác nhận
    Confirmed = 2,     // Đã xác nhận
    Processing = 3,    // Đang chuẩn bị hàng
    Shipping = 4,      // Đang giao hàng
    Completed = 5,     // Giao hàng thành công / Hoàn thành
    Cancelled = 6      // Đã hủy
}

public enum PaymentStatus
{
    Pending = 1,       // Chờ thanh toán
    Paid = 2,          // Đã thanh toán
    Failed = 3,        // Thất bại
    Refunded = 4       // Đã hoàn tiền
}

public enum PaymentMethod
{
    COD = 1,           // Thanh toán khi nhận hàng (Cash on Delivery)
    BankTransfer = 2,  // Chuyển khoản ngân hàng (VietQR)
    VNPay = 3,         // Cổng thanh toán VNPay
    Momo = 4           // Ví Momo
}

public enum CompressionLevel
{
    CCL1 = 1, // Nhẹ (15 - 21 mmHg) - Phòng ngừa, mỏi chân, giãn nhẹ, thai kỳ
    CCL2 = 2, // Trung bình (23 - 32 mmHg) - Giãn tĩnh mạch rõ, phù nề, sau xơ hóa
    CCL3 = 3, // Mạnh (34 - 46 mmHg) - Suy tĩnh mạch nặng, loạn dưỡng da
    CCL4 = 4  // Rất mạnh (> 49 mmHg) - Phù bạch huyết nặng
}

public enum SockStyle
{
    KneeHigh = 1,   // Vớ gối (AD)
    ThighHigh = 2,  // Vớ đùi (AG)
    Pantyhose = 3,  // Quần vớ (AT)
    Maternity = 4   // Quần vớ dành cho bà bầu (AT/M)
}

public enum ToeType
{
    OpenToe = 1,    // Hở ngón (thoáng mát, dễ xỏ, theo dõi tuần hoàn đầu ngón chân)
    ClosedToe = 2   // Kín ngón (ấm áp, ôm chân toàn diện)
}
