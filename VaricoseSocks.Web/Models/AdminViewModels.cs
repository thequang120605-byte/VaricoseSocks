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

public class AdminCustomerViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class InventoryStatusViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<InventoryVariantStatusViewModel> Variants { get; set; } = new();
}

public class InventoryVariantStatusViewModel
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}

public class CouponValidationResultViewModel
{
    public bool Valid { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? Message { get; set; }
}
