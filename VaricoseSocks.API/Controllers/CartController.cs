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
                StockQuantity = ci.ProductVariant != null ? ci.ProductVariant.StockQuantity : ci.Product.StockQuantity,
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

        if (!product.IsActive)
        {
            return BadRequest(new { message = $"Sản phẩm '{product.Name}' hiện đang tạm ngừng kinh doanh." });
        }

        // Xác định biến thể và kiểm tra tồn kho tương ứng
        Domain.Entities.ProductVariant? variant = null;
        if (dto.ProductVariantId.HasValue)
        {
            variant = product.Variants.FirstOrDefault(v => v.Id == dto.ProductVariantId.Value);
        }
        else if (product.Variants.Any())
        {
            variant = product.Variants.FirstOrDefault(v => v.Size == dto.Size && (string.IsNullOrEmpty(dto.Color) || v.Color == dto.Color))
                   ?? product.Variants.FirstOrDefault(v => v.Size == dto.Size);
            if (variant != null)
            {
                dto.ProductVariantId = variant.Id;
            }
        }

        int availableStock = variant != null ? variant.StockQuantity : product.StockQuantity;
        if (availableStock <= 0)
        {
            return BadRequest(new { message = $"Sản phẩm '{product.Name}' (Size {dto.Size}) hiện đã hết hàng trong kho." });
        }

        var cart = await _cartRepo.GetCartAsync(dto.UserId, dto.SessionId);
        int currentInCart = 0;
        if (cart != null)
        {
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId && ci.Size == dto.Size && (string.IsNullOrEmpty(dto.Color) || ci.Color == dto.Color));
            if (existingItem != null)
            {
                currentInCart = existingItem.Quantity;
            }
        }
        else
        {
            cart = await _cartRepo.CreateCartAsync(dto.UserId, dto.SessionId);
        }

        if (currentInCart + dto.Quantity > availableStock)
        {
            return BadRequest(new { message = $"Kho chỉ còn {availableStock} sản phẩm. Giỏ hàng của bạn đã có {currentInCart}, không thể thêm tiếp {dto.Quantity} sản phẩm nữa." });
        }

        var price = product.DiscountPrice ?? product.Price;
        if (variant != null)
        {
            price += variant.PriceAdjustment;
        }

        var item = await _cartRepo.AddItemAsync(cart.Id, dto.ProductId, dto.ProductVariantId, dto.Size, dto.Color, dto.Quantity, price);

        return Ok(new { message = "Đã thêm sản phẩm vào giỏ hàng thành công.", cartItemId = item.Id });
    }

    [HttpPut("items/{id:int}")]
    public async Task<IActionResult> UpdateItemQuantity(int id, [FromBody] UpdateCartItemDto dto)
    {
        if (dto.Quantity <= 0)
        {
            var delSuccess = await _cartRepo.RemoveItemAsync(id);
            return Ok(new { message = "Đã xóa sản phẩm khỏi giỏ hàng." });
        }

        var cartItem = await _cartRepo.GetCartItemAsync(id);
        if (cartItem == null) return NotFound(new { message = "Không tìm thấy mặt hàng trong giỏ." });

        int availableStock = cartItem.ProductVariant != null ? cartItem.ProductVariant.StockQuantity : cartItem.Product.StockQuantity;
        if (dto.Quantity > availableStock)
        {
            return BadRequest(new { message = $"Số lượng cập nhật ({dto.Quantity}) vượt quá tồn kho khả dụng (kho hiện chỉ còn {availableStock} sản phẩm)." });
        }

        var success = await _cartRepo.UpdateItemQuantityAsync(id, dto.Quantity);
        if (!success) return NotFound(new { message = "Không tìm thấy mặt hàng trong giỏ." });

        return Ok(new { message = "Cập nhật số lượng thành công." });
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCartBySession([FromQuery] int? userId, [FromQuery] string? sessionId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId.HasValue) userId = currentUserId.Value;

        var cart = await _cartRepo.GetCartAsync(userId, sessionId);
        if (cart != null)
        {
            await _cartRepo.ClearCartAsync(cart.Id);
        }
        return Ok(new { message = "Đã làm trống giỏ hàng thành công." });
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
