using System.ComponentModel.DataAnnotations;

namespace VaricoseSocks.Web.Models;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên của bạn")]
    [Display(Name = "Họ và tên người nhận")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email nhận hóa đơn")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    [Display(Name = "Email")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
    [Display(Name = "Địa chỉ chi tiết (Số nhà, tên đường)")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Display(Name = "Tỉnh / Thành phố")]
    public string? City { get; set; } = "Hồ Chí Minh";

    [Display(Name = "Quận / Huyện")]
    public string? District { get; set; }

    [Display(Name = "Ghi chú giao hàng")]
    public string? Note { get; set; }

    [Display(Name = "Phương thức thanh toán")]
    public int PaymentMethod { get; set; } = 1; // 1: COD, 2: Chuyển khoản VietQR

    public CartViewModel Cart { get; set; } = new();
}

public class OrderDetailViewModel
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Note { get; set; }
    public string PaymentMethodText { get; set; } = "COD";
    public string PaymentStatusText { get; set; } = "Chờ thanh toán";
    public string OrderStatusText { get; set; } = "Chờ xác nhận";
    public int OrderStatus { get; set; } = 1;
    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class OrderItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string CompressionText { get; set; } = "CCL1";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderTrackingViewModel
{
    public string? SearchQuery { get; set; }
    public OrderDetailViewModel? Order { get; set; }
    public bool HasSearched { get; set; }
    public string? ErrorMessage { get; set; }
}
