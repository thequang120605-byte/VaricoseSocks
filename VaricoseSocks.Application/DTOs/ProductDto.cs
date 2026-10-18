using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public CompressionLevel CompressionLevel { get; set; }
    public string CompressionLevelText => CompressionLevel switch
    {
        CompressionLevel.CCL1 => "CCL1 (15-21 mmHg)",
        CompressionLevel.CCL2 => "CCL2 (23-32 mmHg)",
        CompressionLevel.CCL3 => "CCL3 (34-46 mmHg)",
        CompressionLevel.CCL4 => "CCL4 (>49 mmHg)",
        _ => "Tiêu chuẩn"
    };
    public SockStyle Style { get; set; }
    public string StyleText => Style switch
    {
        SockStyle.KneeHigh => "Vớ gối (AD)",
        SockStyle.ThighHigh => "Vớ đùi (AG)",
        SockStyle.Pantyhose => "Quần vớ (AT)",
        SockStyle.Maternity => "Vớ bà bầu",
        _ => "Khác"
    };
    public ToeType ToeType { get; set; }
    public string ToeTypeText => ToeType == ToeType.OpenToe ? "Hở ngón" : "Kín ngón";
    public string? Brand { get; set; }
    public string? Origin { get; set; }
    public string? Material { get; set; }
    public string? UsageInstructions { get; set; }
    public string? SizeGuide { get; set; }
    public bool IsFeatured { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<ProductVariantDto> Variants { get; set; } = new();
}

public class ProductVariantDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal PriceAdjustment { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.CCL1;
    public SockStyle Style { get; set; } = SockStyle.KneeHigh;
    public ToeType ToeType { get; set; } = ToeType.ClosedToe;
    public string? Brand { get; set; }
    public string? Origin { get; set; }
    public string? Material { get; set; }
    public string? Sku { get; set; }
    public bool IsFeatured { get; set; } = false;
}