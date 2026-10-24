using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryStatusDto>> GetInventoryStatusAsync()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Variants)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new InventoryStatusDto
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                StockQuantity = p.StockQuantity,
                Status = p.StockQuantity <= 0 ? "Hết hàng" : p.StockQuantity <= 50 ? "Sắp hết hàng" : "Còn hàng",
                Variants = p.Variants.Select(v => new InventoryVariantStatusDto
                {
                    Id = v.Id,
                    Sku = v.Sku,
                    Size = v.Size,
                    Color = v.Color,
                    StockQuantity = v.StockQuantity
                }).ToList()
            })
            .ToListAsync();

        return products;
    }

    public async Task<(bool Success, string Message, int CurrentStock)> ImportStockAsync(InventoryImportDto dto)
    {
        if (dto.Quantity <= 0)
        {
            return (false, "Số lượng nhập kho phải lớn hơn 0.", 0);
        }

        var product = await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

        if (product == null)
        {
            return (false, "Không tìm thấy sản phẩm trong hệ thống.", 0);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            product.StockQuantity += dto.Quantity;

            if (dto.ProductVariantId.HasValue)
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == dto.ProductVariantId.Value);
                if (variant != null)
                {
                    variant.StockQuantity += dto.Quantity;
                }
            }

            var inventoryRecord = new InventoryTransaction
            {
                ProductId = dto.ProductId,
                ProductVariantId = dto.ProductVariantId,
                Type = InventoryTransactionType.Import,
                Quantity = dto.Quantity,
                UnitCost = dto.UnitCost,
                Note = dto.Note ?? "Nhập hàng lưu kho từ nhà sản xuất",
                PerformedBy = dto.PerformedBy ?? "Quản Trị Viên Kho",
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryTransactions.Add(inventoryRecord);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Nhập kho thành công.", product.StockQuantity);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, "Lỗi xảy ra khi lưu kho: " + ex.Message, 0);
        }
    }

    public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync()
    {
        var list = await _context.InventoryTransactions
            .AsNoTracking()
            .Include(t => t.Product)
            .OrderByDescending(t => t.CreatedAt)
            .Take(100)
            .Select(t => new InventoryTransactionDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = t.Product.Name,
                Type = (int)t.Type,
                TypeName = t.Type == InventoryTransactionType.Import ? "Nhập kho" : "Xuất kho",
                Quantity = t.Quantity,
                UnitCost = t.UnitCost,
                Note = t.Note,
                PerformedBy = t.PerformedBy,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return list;
    }
}
