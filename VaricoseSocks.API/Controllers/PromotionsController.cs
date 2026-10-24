using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Application.DTOs;
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
        var list = await _context.Promotions
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var now = DateTime.UtcNow;
        var list = await _context.Promotions
            .AsNoTracking()
            .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now && p.UsedCount < p.UsageLimit)
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponDto dto)
    {
        var now = DateTime.UtcNow;
        var promo = await _context.Promotions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code.ToUpper() == dto.Code.Trim().ToUpper() && p.IsActive);

        if (promo == null)
            return BadRequest(new CouponResultDto { Valid = false, Message = "Mã giảm giá không tồn tại hoặc đã bị vô hiệu hóa." });

        if (promo.StartDate > now || promo.EndDate < now)
            return BadRequest(new CouponResultDto { Valid = false, Message = "Mã giảm giá chưa đến đợt áp dụng hoặc đã hết thời hạn." });

        if (promo.UsedCount >= promo.UsageLimit)
            return BadRequest(new CouponResultDto { Valid = false, Message = "Mã giảm giá đã đạt giới hạn số lượt sử dụng tối đa." });

        if (dto.OrderAmount < promo.MinOrderAmount)
            return BadRequest(new CouponResultDto { Valid = false, Message = $"Mã giảm giá yêu cầu đơn hàng tối thiểu từ {promo.MinOrderAmount:N0} ₫." });

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

        return Ok(new CouponResultDto
        {
            Valid = true,
            Code = promo.Code,
            Name = promo.Name,
            DiscountAmount = discount,
            FinalAmount = Math.Max(0, dto.OrderAmount - discount),
            Message = $"Áp dụng mã {promo.Code} thành công! Bạn được giảm {discount:N0} ₫."
        });
    }

    [Authorize(Roles = "Admin")]
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
