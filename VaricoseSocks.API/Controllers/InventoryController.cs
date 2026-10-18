using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InventoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetInventoryStatus()
    {
        var products = await _context.Products
            .Include(p => p.Variants)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Sku,
                p.StockQuantity,
                Status = p.StockQuantity <= 0 ? "Hết hàng" : p.StockQuantity <= 50 ? "Sắp hết hàng" : "Còn hàng",
                Variants = p.Variants.Select(v => new
                {
                    v.Id,
                    v.Sku,
                    v.Size,
                    v.Color,
                    v.StockQuantity
                }).ToList()
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportStock([FromBody] InventoryImportDto dto)
    {
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null) return NotFound(new { message = "Không tìm thấy sản phẩm." });

        product.StockQuantity += dto.Quantity;

        if (dto.ProductVariantId.HasValue)
        {
            var variant = await _context.ProductVariants.FindAsync(dto.ProductVariantId.Value);
            if (variant != null) variant.StockQuantity += dto.Quantity;
        }

        var transaction = new InventoryTransaction
        {
            ProductId = dto.ProductId,
            ProductVariantId = dto.ProductVariantId,
            Type = InventoryTransactionType.Import,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost,
            Note = dto.Note ?? "Nhập hàng từ nhà sản xuất",
            PerformedBy = dto.PerformedBy ?? "Nguyễn Thế Quang (Backend Lead)",
            CreatedAt = DateTime.UtcNow
        };

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Nhập kho thành công.", currentStock = product.StockQuantity });
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var list = await _context.InventoryTransactions
            .Include(t => t.Product)
            .OrderByDescending(t => t.CreatedAt)
            .Take(50)
            .Select(t => new
            {
                t.Id,
                t.ProductId,
                ProductName = t.Product.Name,
                t.Type,
                TypeName = t.Type == InventoryTransactionType.Import ? "Nhập kho" : "Xuất kho",
                t.Quantity,
                t.UnitCost,
                t.Note,
                t.PerformedBy,
                t.CreatedAt
            })
            .ToListAsync();

        return Ok(list);
    }
}

public class InventoryImportDto
{
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Note { get; set; }
    public string? PerformedBy { get; set; }
}
