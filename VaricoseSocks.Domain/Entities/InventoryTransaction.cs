namespace VaricoseSocks.Domain.Entities;

public enum InventoryTransactionType
{
    Import = 1,     // Nhập kho (tăng số lượng)
    Export = 2,     // Xuất kho bán hàng (giảm số lượng)
    Adjustment = 3  // Điều chỉnh kiểm kê kho
}

public class InventoryTransaction
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public InventoryTransactionType Type { get; set; } = InventoryTransactionType.Import;
    public int Quantity { get; set; }                      // Số lượng nhập/xuất
    public decimal? UnitCost { get; set; }                 // Giá nhập vào
    public string? Note { get; set; }                      // Vd: Nhập lô hàng từ Đức / Mỹ
    public string? PerformedBy { get; set; }               // Người thực hiện (Nguyễn Thế Quang / Admin)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Product Product { get; set; } = null!;
    public ProductVariant? ProductVariant { get; set; }
}
