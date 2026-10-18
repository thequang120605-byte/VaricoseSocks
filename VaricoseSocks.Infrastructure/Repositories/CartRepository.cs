using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetCartAsync(int? userId, string? sessionId)
    {
        var query = _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
            .AsQueryable();

        if (userId.HasValue && userId.Value > 0)
        {
            return await query.FirstOrDefaultAsync(c => c.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            return await query.FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }

        return null;
    }

    public async Task<Cart> CreateCartAsync(int? userId, string? sessionId)
    {
        var cart = new Cart
        {
            UserId = userId,
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<CartItem> AddItemAsync(int cartId, int productId, int? variantId, string size, string color, int quantity, decimal unitPrice)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId && ci.Size == size && ci.Color == color);

        if (existing != null)
        {
            existing.Quantity += quantity;
            await _context.SaveChangesAsync();
            return existing;
        }

        var item = new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            ProductVariantId = variantId,
            Size = size,
            Color = color,
            Quantity = quantity,
            UnitPrice = unitPrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> UpdateItemQuantityAsync(int cartItemId, int quantity)
    {
        var item = await _context.CartItems.FindAsync(cartItemId);
        if (item == null) return false;

        if (quantity <= 0)
        {
            _context.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveItemAsync(int cartItemId)
    {
        var item = await _context.CartItems.FindAsync(cartItemId);
        if (item == null) return false;

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ClearCartAsync(int cartId)
    {
        var items = await _context.CartItems.Where(ci => ci.CartId == cartId).ToListAsync();
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}
