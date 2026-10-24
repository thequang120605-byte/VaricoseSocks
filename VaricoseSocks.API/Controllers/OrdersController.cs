using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    private bool IsAdminOrStaff()
    {
        return User.IsInRole("Admin") || User.IsInRole("Staff");
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] OrderStatus? status)
    {
        var orders = await _orderService.GetOrdersAsync(status);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

        // Quyền sở hữu: Nếu đã đăng nhập và không phải Admin/Staff, chỉ được xem đơn hàng của chính mình
        var currentUserId = GetCurrentUserId();
        if (currentUserId.HasValue && !IsAdminOrStaff())
        {
            if (order.UserId.HasValue && order.UserId.Value != currentUserId.Value)
            {
                return Forbid();
            }
        }

        return Ok(order);
    }

    [HttpGet("code/{orderCode}")]
    public async Task<IActionResult> GetByOrderCode(string orderCode)
    {
        var order = await _orderService.GetOrderByCodeAsync(orderCode);
        if (order == null) return NotFound(new { message = "Không tìm thấy mã đơn hàng." });

        return Ok(order);
    }

    [Authorize]
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        // Quyền sở hữu: Chỉ cho phép Admin/Staff hoặc chính khách hàng đó xem lịch sử đơn
        var currentUserId = GetCurrentUserId();
        if (!IsAdminOrStaff() && (!currentUserId.HasValue || currentUserId.Value != userId))
        {
            return Forbid();
        }

        var orders = await _orderService.GetOrdersByUserIdAsync(userId);
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        // Gán chính xác UserId từ token nếu người dùng đã đăng nhập, ngăn giả mạo UserId khác
        var currentUserId = GetCurrentUserId();
        if (currentUserId.HasValue)
        {
            dto.UserId = currentUserId.Value;
        }

        var result = await _orderService.CreateOrderAsync(dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Order!.Id }, result.Order);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, dto.OrderStatus, dto.PaymentStatus);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }
}
