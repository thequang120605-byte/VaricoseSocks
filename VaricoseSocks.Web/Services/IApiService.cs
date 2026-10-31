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

    // Patient Health Records & CEAP Clinical Tools
    Task<List<HealthRecordViewModel>> GetUserHealthRecordsAsync(int userId);
    Task<bool> SaveHealthRecordAsync(CreateHealthRecordViewModel model);
    Task<CeapResultViewModel?> AssessCeapAsync(int stage, bool hasUlcer, bool hasSkinChanges, bool hasEdema, bool hasVaricose);
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
    public bool IsAccurate { get; set; }
}

public class HealthRecordViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? PatientName { get; set; }
    public double AnkleCircumferenceCm { get; set; }
    public double? CalfCircumferenceCm { get; set; }
    public double? ThighCircumferenceCm { get; set; }
    public double? LegLengthCm { get; set; }
    public int CeapStage { get; set; }
    public string CeapStageText { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? DiagnosisNote { get; set; }
    public string? DoctorName { get; set; }
    public string? PrescribedCompression { get; set; }
    public string? RecommendedSize { get; set; }
    public DateTime MeasurementDate { get; set; }
    public DateTime? ReplacementDueDate { get; set; }
    public int DaysUntilReplacement { get; set; }
    public bool NeedsReplacement { get; set; }
}

public class CreateHealthRecordViewModel
{
    public int? UserId { get; set; }
    public double AnkleCircumferenceCm { get; set; }
    public double? CalfCircumferenceCm { get; set; }
    public double? ThighCircumferenceCm { get; set; }
    public double? LegLengthCm { get; set; }
    public int CeapStage { get; set; } = 1;
    public string? Symptoms { get; set; }
    public string? DiagnosisNote { get; set; }
    public string? DoctorName { get; set; }
    public string? PrescribedCompression { get; set; }
    public string? RecommendedSize { get; set; }
}

public class CeapResultViewModel
{
    public int Stage { get; set; }
    public string StageCode { get; set; } = "C1";
    public string StageName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RecommendedCompression { get; set; } = string.Empty;
    public string RecommendedStyle { get; set; } = string.Empty;
    public string TreatmentAdvice { get; set; } = string.Empty;
    public bool RequiresDoctorPrescription { get; set; }
}
