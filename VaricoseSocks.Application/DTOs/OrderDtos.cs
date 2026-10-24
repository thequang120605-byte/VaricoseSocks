using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? Note { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodText => PaymentMethod switch
    {
        PaymentMethod.COD => "Thanh toán khi nhận hàng (COD)",
        PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng (VietQR)",
        PaymentMethod.VNPay => "Cổng VNPay",
        PaymentMethod.Momo => "Ví điện tử Momo",
        _ => "COD"
    };

    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusText => PaymentStatus switch
    {
        PaymentStatus.Pending => "Chờ thanh toán",
        PaymentStatus.Paid => "Đã thanh toán",
        PaymentStatus.Failed => "Thanh toán thất bại",
        PaymentStatus.Refunded => "Đã hoàn tiền",
        _ => "Chờ thanh toán"
    };

    public OrderStatus OrderStatus { get; set; }
    public string OrderStatusText => OrderStatus switch
    {
        OrderStatus.Pending => "Chờ xác nhận",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Processing => "Đang đóng gói",
        OrderStatus.Shipping => "Đang vận chuyển",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        _ => "Chờ xác nhận"
    };

    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<OrderDetailDto> Items { get; set; } = new();
}

public class OrderDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public CompressionLevel CompressionLevel { get; set; }
    public string CompressionText => CompressionLevel switch
    {
        CompressionLevel.CCL1 => "CCL1 (15-21 mmHg)",
        CompressionLevel.CCL2 => "CCL2 (23-32 mmHg)",
        CompressionLevel.CCL3 => "CCL3 (34-46 mmHg)",
        _ => "Chuẩn y khoa"
    };
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateOrderDto
{
    public int? UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? Note { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
    public string? VoucherCode { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public string Size { get; set; } = "M";
    public string Color { get; set; } = "Màu Da";
    public int Quantity { get; set; } = 1;
}

public class UpdateOrderStatusDto
{
    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
}
