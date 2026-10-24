using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Domain.Enums;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var totalProducts = await _context.Products.AsNoTracking().CountAsync();
        var totalCustomers = await _context.Users.AsNoTracking().CountAsync();
        var totalOrders = await _context.Orders.AsNoTracking().CountAsync();
        var pendingOrders = await _context.Orders.AsNoTracking().CountAsync(o => o.OrderStatus == OrderStatus.Pending);
        
        // Exclude Cancelled orders from total revenue
        var totalRevenue = await _context.Orders
            .AsNoTracking()
            .Where(o => o.OrderStatus != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount);

        var recentOrdersRaw = await _context.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new
            {
                o.Id,
                o.OrderCode,
                o.CustomerName,
                o.CustomerPhone,
                o.TotalAmount,
                o.OrderStatus,
                o.PaymentStatus,
                o.CreatedAt
            })
            .ToListAsync();

        var recentOrders = recentOrdersRaw.Select(o => new
        {
            o.Id,
            o.OrderCode,
            o.CustomerName,
            o.CustomerPhone,
            o.TotalAmount,
            o.OrderStatus,
            OrderStatusName = o.OrderStatus switch
            {
                OrderStatus.Pending => "Chờ xác nhận",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Processing => "Đang xử lý",
                OrderStatus.Shipping => "Đang giao",
                OrderStatus.Completed => "Hoàn thành",
                OrderStatus.Cancelled => "Đã hủy",
                _ => "Đang xử lý"
            },
            o.PaymentStatus,
            o.CreatedAt
        }).ToList();

        var lowStock = await _context.Products
            .AsNoTracking()
            .Where(p => p.StockQuantity <= 50)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Sku,
                p.StockQuantity,
                p.Price
            })
            .ToListAsync();

        return Ok(new
        {
            totalRevenue,
            totalOrders,
            pendingOrders,
            totalProducts,
            totalCustomers,
            recentOrders,
            lowStockProducts = lowStock
        });
    }

    [HttpGet("reports/revenue")]
    public async Task<IActionResult> GetRevenueReport()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.OrderStatus != OrderStatus.Cancelled)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new
            {
                Month = $"{g.Key.Month}/{g.Key.Year}",
                TotalOrders = g.Count(),
                Revenue = g.Sum(o => o.TotalAmount)
            })
            .ToListAsync();

        return Ok(orders);
    }
}
