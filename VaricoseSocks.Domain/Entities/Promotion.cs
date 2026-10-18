namespace VaricoseSocks.Domain.Entities;

public enum DiscountType
{
    Percentage = 1, // Giảm theo % (Vd: 10%)
    FixedAmount = 2 // Giảm số tiền cố định (Vd: 50.000₫)
}

public class Promotion
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;       // Vd: VARICOSE10, FREESHIP50
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; set; }             // Giá trị giảm (% hoặc VNĐ)
    public decimal MinOrderAmount { get; set; } = 0;       // Đơn hàng tối thiểu
    public decimal? MaxDiscountAmount { get; set; }        // Giới hạn giảm tối đa
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    public int UsageLimit { get; set; } = 100;             // Số lần tối đa có thể dùng
    public int UsedCount { get; set; } = 0;                // Số lần đã dùng
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
