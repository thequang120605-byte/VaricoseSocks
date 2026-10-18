namespace VaricoseSocks.Web.Models;

public class ProductViewModel
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
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int CompressionLevel { get; set; }
    public string CompressionLevelText { get; set; } = "CCL1 (15-21 mmHg)";
    public int Style { get; set; }
    public string StyleText { get; set; } = "Vớ gối (AD)";
    public int ToeType { get; set; }
    public string ToeTypeText { get; set; } = "Hở ngón";
    public string? Brand { get; set; }
    public string? Origin { get; set; }
    public string? Material { get; set; }
    public string? UsageInstructions { get; set; }
    public string? SizeGuide { get; set; }
    public bool IsFeatured { get; set; }
    public List<ProductVariantViewModel> Variants { get; set; } = new();
}

public class ProductVariantViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Size { get; set; } = "M";
    public string Color { get; set; } = "Màu Da";
    public int StockQuantity { get; set; }
    public decimal PriceAdjustment { get; set; }
}

public class ProductListViewModel
{
    public List<ProductViewModel> Products { get; set; } = new();
    public List<CategoryViewModel> Categories { get; set; } = new();
    public int? SelectedCategoryId { get; set; }
    public string? SearchTerm { get; set; }
    public int? CompressionFilter { get; set; }
    public int? StyleFilter { get; set; }
    public string? SortBy { get; set; }
}