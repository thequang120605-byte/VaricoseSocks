namespace VaricoseSocks.Web.Models;

public class AdminDashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrdersCount { get; set; }
    public List<OrderDetailViewModel> RecentOrders { get; set; } = new();
    public List<ProductViewModel> LowStockProducts { get; set; } = new();
}

public class AdminProductEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public int CompressionLevel { get; set; } = 1;
    public int Style { get; set; } = 1;
    public int ToeType { get; set; } = 1;
    public string? Brand { get; set; }
    public string? Origin { get; set; }
    public string? Material { get; set; }
    public bool IsFeatured { get; set; }
    public List<CategoryViewModel> Categories { get; set; } = new();
}
