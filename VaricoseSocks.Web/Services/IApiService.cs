using VaricoseSocks.Web.Models;

namespace VaricoseSocks.Web.Services;

public interface IApiService
{
    // Products & Categories
    Task<List<CategoryViewModel>> GetCategoriesAsync();
    Task<List<ProductViewModel>> GetProductsAsync(int? categoryId = null, string? searchTerm = null, int? compression = null, int? style = null);
    Task<ProductViewModel?> GetProductByIdAsync(int id);
    Task<bool> CreateProductAsync(AdminProductEditViewModel model);
    Task<bool> UpdateProductAsync(int id, AdminProductEditViewModel model);
    Task<bool> DeleteProductAsync(int id);

    // Cart
    Task<CartViewModel> GetCartAsync(int? userId, string? sessionId);
    Task<bool> AddToCartAsync(int productId, int? variantId, string size, string color, int quantity, int? userId, string? sessionId);
    Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity);
    Task<bool> RemoveCartItemAsync(int cartItemId);

    // Orders & Checkout
    Task<OrderDetailViewModel?> CreateOrderAsync(CheckoutViewModel model, string? sessionId, int? userId);
    Task<OrderDetailViewModel?> GetOrderByCodeAsync(string orderCode);
    Task<List<OrderDetailViewModel>> GetAllOrdersAsync(int? status = null);
    Task<bool> UpdateOrderStatusAsync(int orderId, int status);

    // Chatbot AI
    Task<ChatbotConsultResultViewModel> ConsultChatbotAsync(string message, double? ankleCm, double? calfCm, double? thighCm, bool isPregnant, bool hasVaricose, bool hasSwelling);
    Task<SizeResultViewModel> CalculateSizeAsync(double ankleCm, double? calfCm, double? thighCm);

    // Authentication & Users
    Task<AuthResponseViewModel?> LoginAsync(string email, string password);
    Task<AuthResponseViewModel?> RegisterAsync(RegisterViewModel model);
    Task<UserAccountViewModel?> GetProfileAsync(int userId);
    Task<List<OrderDetailViewModel>> GetUserOrdersAsync(int userId);
}

public class ChatbotConsultResultViewModel
{
    public string ReplyMessage { get; set; } = string.Empty;
    public string? RecommendedCompressionText { get; set; }
    public string? RecommendedSize { get; set; }
    public string? RecommendedStyleText { get; set; }
    public string? Advice { get; set; }
    public List<ProductViewModel> RecommendedProducts { get; set; } = new();
}

public class SizeResultViewModel
{
    public string RecommendedSize { get; set; } = "M";
    public string Explanation { get; set; } = string.Empty;
}