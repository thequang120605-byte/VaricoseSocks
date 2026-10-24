using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;

namespace VaricoseSocks.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartRepository _cartRepo;
    private readonly IProductRepository _productRepo;

    public CartController(ICartRepository cartRepo, IProductRepository productRepo)
    {
        _cartRepo = cartRepo;
        _productRepo = productRepo;
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart([FromQuery] int? userId, [FromQuery] string? sessionId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId.HasValue)
        {
            userId = currentUserId.Value;
        }

        if (!userId.HasValue && string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(new { message = "Cần cung cấp userId hoặc sessionId." });
        }

        var cart = await _cartRepo.GetCartAsync(userId, sessionId);
        if (cart == null)
        {
            return Ok(new CartDto { UserId = userId, SessionId = sessionId });
        }

        var dto = new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            SessionId = cart.SessionId,
            Items = cart.CartItems.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                CartId = ci.CartId,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                ProductImageUrl = ci.Product.ImageUrl,
                ProductVariantId = ci.ProductVariantId,
                Size = ci.Size,
                Color = ci.Color,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        if (dto.Quantity <= 0)
        {
            return BadRequest(new { message = "Số lượng sản phẩm thêm vào giỏ phải lớn hơn 0." });
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId.HasValue)
        {
            dto.UserId = currentUserId.Value;
        }

        var product = await _productRepo.GetByIdAsync(dto.ProductId);
        if (product == null) return NotFound(new { message = "Sản phẩm không tồn tại." });

        var cart = await _cartRepo.GetCartAsync(dto.UserId, dto.SessionId);
        if (cart == null)
        {
            cart = await _cartRepo.CreateCartAsync(dto.UserId, dto.SessionId);
        }

        var price = product.DiscountPrice ?? product.Price;
        var item = await _cartRepo.AddItemAsync(cart.Id, dto.ProductId, dto.ProductVariantId, dto.Size, dto.Color, dto.Quantity, price);

        return Ok(new { message = "Đã thêm sản phẩm vào giỏ hàng thành công.", cartItemId = item.Id });
    }

    [HttpPut("items/{id:int}")]
    public async Task<IActionResult> UpdateItemQuantity(int id, [FromBody] UpdateCartItemDto dto)
    {
        if (dto.Quantity < 0)
        {
            return BadRequest(new { message = "Số lượng sản phẩm không thể là số âm." });
        }

        var success = await _cartRepo.UpdateItemQuantityAsync(id, dto.Quantity);
        if (!success) return NotFound(new { message = "Không tìm thấy mặt hàng trong giỏ." });

        return Ok(new { message = "Cập nhật số lượng thành công." });
    }

    [HttpDelete("items/{id:int}")]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var success = await _cartRepo.RemoveItemAsync(id);
        if (!success) return NotFound(new { message = "Không tìm thấy mặt hàng trong giỏ." });

        return Ok(new { message = "Đã xóa sản phẩm khỏi giỏ hàng." });
    }

    [HttpDelete("{cartId:int}/clear")]
    public async Task<IActionResult> Clear(int cartId)
    {
        await _cartRepo.ClearCartAsync(cartId);
        return Ok(new { message = "Đã làm trống giỏ hàng." });
    }
}
