namespace VaricoseSocks.Domain.Entities;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Size { get; set; } = "M";                // S, M, L, XL, XXL
    public string Color { get; set; } = "Beige";           // Beige (Da), Black (Đen)
    public int StockQuantity { get; set; } = 0;
    public decimal PriceAdjustment { get; set; } = 0;      // Chênh lệch giá (nếu size lớn có phụ phí)
    public bool IsActive { get; set; } = true;

    // Navigation property
    public Product Product { get; set; } = null!;
}
