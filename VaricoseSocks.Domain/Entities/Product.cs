using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    
    // Medical Compression Sock specific attributes
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.CCL1;
    public SockStyle Style { get; set; } = SockStyle.KneeHigh;
    public ToeType ToeType { get; set; } = ToeType.ClosedToe;
    public string? Material { get; set; }                  // Vd: 70% Polyamide, 30% Elastane
    public string? Brand { get; set; }                     // Jobst, Mediven, RelaxSan, Sigvaris
    public string? Origin { get; set; }                    // Đức, Ý, Thụy Sĩ, Mỹ
    public string? UsageInstructions { get; set; }         // Hướng dẫn mang và bảo quản y tế
    public string? SizeGuide { get; set; }                 // Bảng hướng dẫn đo cB (cổ chân), cC (bắp chân)

    public bool IsFeatured { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}