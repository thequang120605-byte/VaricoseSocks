using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;

    public OrdersController(IOrderRepository orderRepo, IProductRepository productRepo)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] OrderStatus? status)
    {
        var orders = await _orderRepo.GetAllAsync(status);
        return Ok(orders.Select(o => MapToDto(o)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

        return Ok(MapToDto(order));
    }

    [HttpGet("code/{orderCode}")]
    public async Task<IActionResult> GetByOrderCode(string orderCode)
    {
        var order = await _orderRepo.GetByOrderCodeAsync(orderCode);
        if (order == null) return NotFound(new { message = "Không tìm thấy mã đơn hàng." });

        return Ok(MapToDto(order));
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var orders = await _orderRepo.GetByUserIdAsync(userId);
        return Ok(orders.Select(o => MapToDto(o)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
        {
            return BadRequest(new { message = "Đơn hàng phải có ít nhất 1 sản phẩm." });
        }

        var orderCode = "VS-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);

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
            CreatedAt = DateTime.UtcNow
        };

        decimal subTotal = 0;
        foreach (var item in dto.Items)
        {
            var product = await _productRepo.GetByIdAsync(item.ProductId);
            if (product == null) continue;

            var price = product.DiscountPrice ?? product.Price;
            var lineTotal = price * item.Quantity;
            subTotal += lineTotal;

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

        order.SubTotal = subTotal;
        order.ShippingFee = subTotal >= 500000 ? 0 : 30000;
        order.DiscountAmount = 0;
        order.TotalAmount = order.SubTotal + order.ShippingFee - order.DiscountAmount;

        var created = await _orderRepo.CreateOrderAsync(order);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var success = await _orderRepo.UpdateStatusAsync(id, dto.OrderStatus, dto.PaymentStatus);
        if (!success) return NotFound(new { message = "Không tìm thấy đơn hàng để cập nhật." });

        return Ok(new { message = "Cập nhật trạng thái đơn hàng thành công." });
    }

    private static OrderDto MapToDto(Order o)
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
