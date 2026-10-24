using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Domain.Enums;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, OrderDto? Order)> CreateOrderAsync(CreateOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
        {
            return (false, "Đơn hàng phải có ít nhất 1 sản phẩm.", null);
        }

        // Validate quantities
        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
            {
                return (false, "Số lượng sản phẩm đặt mua phải lớn hơn 0.", null);
            }
        }

        // Consolidate duplicate order lines by ProductId, ProductVariantId, Size, Color
        var items = dto.Items
            .GroupBy(i => new
            {
                i.ProductId,
                i.ProductVariantId,
                Size = (i.Size ?? "").Trim(),
                Color = (i.Color ?? "").Trim()
            })
            .Select(g => new CreateOrderItemDto
            {
                ProductId = g.Key.ProductId,
                ProductVariantId = g.Key.ProductVariantId,
                Size = g.Key.Size,
                Color = g.Key.Color,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToList();

        // Batch lookup all products referenced in items
        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Variants)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // Validate all products exist, are active, and have sufficient stock
        foreach (var item in items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                return (false, $"Sản phẩm ID {item.ProductId} không tồn tại hoặc đã ngừng kinh doanh.", null);
            }

            if (!product.IsActive)
            {
                return (false, $"Sản phẩm '{product.Name}' hiện đang tạm ngừng kinh doanh.", null);
            }

            if (product.StockQuantity < item.Quantity)
            {
                return (false, $"Sản phẩm '{product.Name}' chỉ còn {product.StockQuantity} sản phẩm trong kho (tổng đặt {item.Quantity}).", null);
            }

            if (item.ProductVariantId.HasValue)
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == item.ProductVariantId.Value);
                if (variant == null)
                {
                    return (false, $"Biến thể sản phẩm không hợp lệ cho '{product.Name}'.", null);
                }
                if (variant.StockQuantity < item.Quantity)
                {
                    return (false, $"Size {variant.Size} - Màu {variant.Color} của sản phẩm '{product.Name}' chỉ còn {variant.StockQuantity} sản phẩm trong kho.", null);
                }
            }
        }

        // Compute subtotal
        decimal subTotal = 0;
        foreach (var item in items)
        {
            var product = products[item.ProductId];
            var price = product.DiscountPrice ?? product.Price;
            if (item.ProductVariantId.HasValue)
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == item.ProductVariantId.Value);
                if (variant != null)
                {
                    price += variant.PriceAdjustment;
                }
            }
            subTotal += price * item.Quantity;
        }

        // Process promotion / voucher if provided
        decimal discountAmount = 0;
        Promotion? appliedPromo = null;
        if (!string.IsNullOrWhiteSpace(dto.VoucherCode))
        {
            var now = DateTime.UtcNow;
            var promoCode = dto.VoucherCode.Trim().ToUpper();
            appliedPromo = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code.ToUpper() == promoCode && p.IsActive);

            if (appliedPromo != null && appliedPromo.StartDate <= now && appliedPromo.EndDate >= now && appliedPromo.UsedCount < appliedPromo.UsageLimit)
            {
                if (subTotal >= appliedPromo.MinOrderAmount)
                {
                    if (appliedPromo.DiscountType == DiscountType.Percentage)
                    {
                        discountAmount = subTotal * (appliedPromo.DiscountValue / 100m);
                        if (appliedPromo.MaxDiscountAmount.HasValue && discountAmount > appliedPromo.MaxDiscountAmount.Value)
                        {
                            discountAmount = appliedPromo.MaxDiscountAmount.Value;
                        }
                    }
                    else
                    {
                        discountAmount = appliedPromo.DiscountValue;
                    }
                }
            }
        }

        decimal shippingFee = subTotal >= 500000 ? 0 : 30000;
        decimal totalAmount = Math.Max(0, subTotal + shippingFee - discountAmount);

        var orderCode = "VS-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Random.Shared.Next(1000, 9999);

        var order = new Order
        {
            OrderCode = orderCode,
            UserId = dto.UserId,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            ShippingAddress = dto.ShippingAddress,
            City = dto.City,
            District = dto.District,
            Ward = dto.Ward,
            Note = dto.Note,
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            OrderStatus = OrderStatus.Pending,
            SubTotal = subTotal,
            ShippingFee = shippingFee,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow
        };

        // Atomically deduct stock and save order within a database transaction
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in items)
            {
                var product = products[item.ProductId];
                var price = product.DiscountPrice ?? product.Price;

                ProductVariant? variant = null;
                if (item.ProductVariantId.HasValue)
                {
                    variant = product.Variants.FirstOrDefault(v => v.Id == item.ProductVariantId.Value);
                    if (variant != null)
                    {
                        price += variant.PriceAdjustment;
                    }
                }

                var lineTotal = price * item.Quantity;

                // Concurrency-safe atomic deduction with WHERE StockQuantity >= Quantity
                var productRows = await _context.Products
                    .Where(p => p.Id == item.ProductId && p.StockQuantity >= item.Quantity)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity - item.Quantity));

                if (productRows == 0)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Sản phẩm '{product.Name}' đã hết hàng hoặc không đủ tồn kho do có đơn hàng khác vừa mua đồng thời.", null);
                }

                if (item.ProductVariantId.HasValue)
                {
                    var variantRows = await _context.ProductVariants
                        .Where(v => v.Id == item.ProductVariantId.Value && v.StockQuantity >= item.Quantity)
                        .ExecuteUpdateAsync(s => s.SetProperty(v => v.StockQuantity, v => v.StockQuantity - item.Quantity));

                    if (variantRows == 0)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"Biến thể (Size {item.Size} - Màu {item.Color}) của sản phẩm '{product.Name}' không đủ tồn kho.", null);
                    }
                }

                // Record Inventory Transaction
                _context.InventoryTransactions.Add(new InventoryTransaction
                {
                    ProductId = product.Id,
                    ProductVariantId = item.ProductVariantId,
                    Type = InventoryTransactionType.Export,
                    Quantity = item.Quantity,
                    UnitCost = price,
                    Note = $"Xuất kho bán hàng đơn #{orderCode}",
                    PerformedBy = "Hệ thống Bán Hàng Tự Động",
                    CreatedAt = DateTime.UtcNow
                });

                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = product.Id,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = product.Name,
                    Sku = product.Sku,
                    Size = item.Size,
                    Color = item.Color,
                    CompressionLevel = product.CompressionLevel,
                    UnitPrice = price,
                    Quantity = item.Quantity,
                    TotalPrice = lineTotal
                });
            }

            if (appliedPromo != null)
            {
                appliedPromo.UsedCount += 1;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Đặt hàng thành công.", MapToDto(order));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, "Lỗi xảy ra trong quá trình xử lý đơn hàng: " + ex.Message, null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, PaymentStatus? newPaymentStatus = null)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return (false, "Không tìm thấy đơn hàng cần cập nhật.");
        }

        // Validate state transitions
        if (order.OrderStatus == OrderStatus.Completed && newStatus == OrderStatus.Cancelled)
        {
            return (false, "Không thể hủy đơn hàng đã hoàn tất thành công.");
        }

        if (order.OrderStatus == OrderStatus.Cancelled && newStatus != OrderStatus.Cancelled)
        {
            return (false, "Đơn hàng đã hủy không thể kích hoạt lại.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // If cancelling a non-cancelled order, restock items
            if (newStatus == OrderStatus.Cancelled && order.OrderStatus != OrderStatus.Cancelled)
            {
                foreach (var detail in order.OrderDetails)
                {
                    await _context.Products
                        .Where(p => p.Id == detail.ProductId)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity + detail.Quantity));

                    if (detail.ProductVariantId.HasValue)
                    {
                        await _context.ProductVariants
                            .Where(v => v.Id == detail.ProductVariantId.Value)
                            .ExecuteUpdateAsync(s => s.SetProperty(v => v.StockQuantity, v => v.StockQuantity + detail.Quantity));
                    }

                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = detail.ProductId,
                        ProductVariantId = detail.ProductVariantId,
                        Type = InventoryTransactionType.Import,
                        Quantity = detail.Quantity,
                        UnitCost = detail.UnitPrice,
                        Note = $"Hoàn kho do hủy đơn #{order.OrderCode}",
                        PerformedBy = "Quản Trị Viên",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            order.OrderStatus = newStatus;
            if (newPaymentStatus.HasValue)
            {
                order.PaymentStatus = newPaymentStatus.Value;
            }
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Cập nhật trạng thái đơn hàng thành công.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, "Lỗi khi cập nhật trạng thái đơn hàng: " + ex.Message);
        }
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersAsync(OrderStatus? status = null)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.OrderStatus == status.Value);
        }

        var list = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return list.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto?> GetOrderByCodeAsync(string orderCode)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

        return order == null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
    {
        var list = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return list.Select(MapToDto);
    }

    public static OrderDto MapToDto(Order o)
    {
        return new OrderDto
        {
            Id = o.Id,
            OrderCode = o.OrderCode,
            UserId = o.UserId,
            CustomerName = o.CustomerName,
            CustomerEmail = o.CustomerEmail,
            CustomerPhone = o.CustomerPhone,
            ShippingAddress = o.ShippingAddress,
            City = o.City,
            District = o.District,
            Ward = o.Ward,
            Note = o.Note,
            PaymentMethod = o.PaymentMethod,
            PaymentStatus = o.PaymentStatus,
            OrderStatus = o.OrderStatus,
            SubTotal = o.SubTotal,
            ShippingFee = o.ShippingFee,
            DiscountAmount = o.DiscountAmount,
            TotalAmount = o.TotalAmount,
            CreatedAt = o.CreatedAt,
            Items = o.OrderDetails.Select(d => new OrderDetailDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = d.ProductName,
                ProductImageUrl = d.Product?.ImageUrl,
                Size = d.Size,
                Color = d.Color,
                CompressionLevel = d.CompressionLevel,
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                TotalPrice = d.TotalPrice
            }).ToList()
        };
    }
}
