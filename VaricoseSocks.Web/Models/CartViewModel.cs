namespace VaricoseSocks.Web.Models;

public class CartViewModel
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? SessionId { get; set; }
    public List<CartItemViewModel> Items { get; set; } = new();
    public int TotalQuantity => Items.Sum(i => i.Quantity);
    public decimal SubTotal => Items.Sum(i => i.TotalPrice);
    public decimal ShippingFee => SubTotal >= 500000 || SubTotal == 0 ? 0 : 30000;
    public decimal TotalAmount => SubTotal + ShippingFee;
}

public class CartItemViewModel
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int? ProductVariantId { get; set; }
    public string Size { get; set; } = "M";
    public string Color { get; set; } = "Màu Da";
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}
