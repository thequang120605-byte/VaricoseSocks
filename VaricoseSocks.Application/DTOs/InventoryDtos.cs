namespace VaricoseSocks.Application.DTOs;

public class InventoryImportDto
{
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Note { get; set; }
    public string? PerformedBy { get; set; }
}

public class InventoryStatusDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<InventoryVariantStatusDto> Variants { get; set; } = new();
}

public class InventoryVariantStatusDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}

public class InventoryTransactionDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Note { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
