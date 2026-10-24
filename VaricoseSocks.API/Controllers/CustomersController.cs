using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.API.Controllers;

[Authorize(Roles = "Admin,Staff")]
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Orders)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Phone,
                u.Address,
                u.City,
                u.District,
                u.IsActive,
                u.CreatedAt,
                TotalOrders = u.Orders.Count,
                TotalSpent = u.Orders.Sum(o => o.TotalAmount),
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Orders)
                .ThenInclude(o => o.OrderDetails)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound(new { message = "Không tìm thấy khách hàng." });

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.Address,
            user.City,
            user.District,
            user.IsActive,
            user.CreatedAt,
            Orders = user.Orders.OrderByDescending(o => o.CreatedAt).Select(o => new
            {
                o.Id,
                o.OrderCode,
                o.TotalAmount,
                o.OrderStatus,
                o.PaymentStatus,
                o.CreatedAt,
                ItemCount = o.OrderDetails.Count
            }).ToList()
        });
    }
}
