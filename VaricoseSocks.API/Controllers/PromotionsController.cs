using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PromotionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _context.Promotions.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return Ok(list);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var now = DateTime.UtcNow;
        var list = await _context.Promotions
            .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now && p.UsedCount < p.UsageLimit)
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponDto dto)
    {
        var now = DateTime.UtcNow;
        var promo = await _context.Promotions
            .FirstOrDefaultAsync(p => p.Code.ToUpper() == dto.Code.Trim().ToUpper() && p.IsActive);

        if (promo == null)
            return BadRequest(new { valid = false, message = "Mã giảm giá không tồn tại hoặc đã hết hạn." });

        if (promo.StartDate > now || promo.EndDate < now)
            return BadRequest(new { valid = false, message = "Mã giảm giá chưa đến đợt áp dụng hoặc đã hết thời hạn." });

        if (promo.UsedCount >= promo.UsageLimit)
            return BadRequest(new { valid = false, message = "Mã giảm giá đã đạt giới hạn số lượt sử dụng." });

        if (dto.OrderAmount < promo.MinOrderAmount)
            return BadRequest(new { valid = false, message = $"Mã giảm giá yêu cầu đơn hàng tối thiểu từ {promo.MinOrderAmount:N0} ₫." });

        decimal discount = 0;
        if (promo.DiscountType == DiscountType.Percentage)
        {
            discount = dto.OrderAmount * (promo.DiscountValue / 100m);
            if (promo.MaxDiscountAmount.HasValue && discount > promo.MaxDiscountAmount.Value)
            {
                discount = promo.MaxDiscountAmount.Value;
            }
        }
        else
        {
            discount = promo.DiscountValue;
        }

        return Ok(new
        {
            valid = true,
            code = promo.Code,
            name = promo.Name,
            discountAmount = discount,
            finalAmount = Math.Max(0, dto.OrderAmount - discount),
            message = $"Áp dụng mã {promo.Code} thành công! Bạn được giảm {discount:N0} ₫."
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Promotion promo)
    {
        if (string.IsNullOrWhiteSpace(promo.Code)) return BadRequest(new { message = "Mã voucher không được để trống." });

        promo.Code = promo.Code.ToUpper().Trim();
        _context.Promotions.Add(promo);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = promo.Id }, promo);
    }
}

public class ValidateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}
