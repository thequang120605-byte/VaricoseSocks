namespace VaricoseSocks.Web.Models;

public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? CompressionLevel { get; set; }
}

public class ProductListViewModel
{
    public List<ProductViewModel> Products { get; set; } = new();
    public List<CategoryViewModel> Categories { get; set; } = new();
    public int? SelectedCategoryId { get; set; }
    public string? SearchTerm { get; set; }
}