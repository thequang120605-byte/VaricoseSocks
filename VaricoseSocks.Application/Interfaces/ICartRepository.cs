using VaricoseSocks.Domain.Entities;

namespace VaricoseSocks.Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetCartAsync(int? userId, string? sessionId);
    Task<Cart> CreateCartAsync(int? userId, string? sessionId);
    Task<CartItem> AddItemAsync(int cartId, int productId, int? variantId, string size, string color, int quantity, decimal unitPrice);
    Task<bool> UpdateItemQuantityAsync(int cartItemId, int quantity);
    Task<bool> RemoveItemAsync(int cartItemId);
    Task ClearCartAsync(int cartId);
}
