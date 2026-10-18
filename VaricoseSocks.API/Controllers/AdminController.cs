using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.API.Controllers;

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
        var totalProducts = await _context.Products.CountAsync();
        var totalCustomers = await _context.Users.CountAsync();
        var totalOrders = await _context.Orders.CountAsync();
        var pendingOrders = await _context.Orders.CountAsync(o => (int)o.OrderStatus == 1);
        var totalRevenue = await _context.Orders.Where(o => (int)o.OrderStatus != 6).SumAsync(o => o.TotalAmount);

        var recentOrders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new
            {
                o.Id,
                o.OrderCode,
                o.CustomerName,
                o.CustomerPhone,
                o.TotalAmount,
                o.OrderStatus,
                o.CreatedAt
            })
            .ToListAsync();

        var lowStock = await _context.Products
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
            .Where(o => (int)o.OrderStatus != 6)
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
