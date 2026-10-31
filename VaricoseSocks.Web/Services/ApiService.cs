using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VaricoseSocks.Web.Models;

namespace VaricoseSocks.Web.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;
    private static List<CategoryViewModel>? _cachedCategories;
    private static List<ProductViewModel>? _cachedProducts;

    public ApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5046/api";
        _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    private void SetAuthHeader()
    {
        var token = _httpContextAccessor?.HttpContext?.User.FindFirst("JwtToken")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<List<CategoryViewModel>> GetCategoriesAsync()
    {
        if (_cachedCategories != null && _cachedCategories.Any())
        {
            return _cachedCategories;
        }

        try
        {
            var response = await _httpClient.GetAsync("categories");
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<List<CategoryViewModel>>(stream, _jsonOptions);
                if (data != null && data.Any())
                {
                    _cachedCategories = data;
                    return data;
                }
            }
        }
        catch { }

        _cachedCategories = GetFallbackCategories();
        return _cachedCategories;
    }

    public async Task<List<ProductViewModel>> GetProductsAsync(int? categoryId = null, string? searchTerm = null, int? compression = null, int? style = null)
    {
        List<ProductViewModel>? list = _cachedProducts;

        if (list == null)
        {
            try
            {
                var response = await _httpClient.GetAsync("products");
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var data = await JsonSerializer.DeserializeAsync<List<ProductViewModel>>(stream, _jsonOptions);
                    if (data != null && data.Any())
                    {
                        _cachedProducts = data;
                        list = data;
                    }
                }
            }
            catch { }

            if (list == null)
            {
                list = GetFallbackProducts();
                _cachedProducts = list;
            }
        }

        // Fast in-memory filtering (0ms latency)
        var filtered = list.AsEnumerable();

        if (categoryId.HasValue && categoryId.Value > 0)
            filtered = filtered.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            filtered = filtered.Where(p => p.Name.ToLower().Contains(term) 
                                        || (p.Brand != null && p.Brand.ToLower().Contains(term))
                                        || (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(term)));
        }

        if (compression.HasValue)
            filtered = filtered.Where(p => p.CompressionLevel == compression.Value);

        if (style.HasValue)
            filtered = filtered.Where(p => p.Style == style.Value);

        return filtered.ToList();
    }

    public async Task<ProductViewModel?> GetProductByIdAsync(int id)
    {
        var all = await GetProductsAsync();
        return all.FirstOrDefault(p => p.Id == id);
    }

    public async Task<bool> CreateProductAsync(AdminProductEditViewModel model)
    {
        try
        {
            var payload = new
            {
                name = model.Name,
                sku = model.Sku,
                shortDescription = model.ShortDescription,
                description = model.Description,
                price = model.Price,
                discountPrice = model.DiscountPrice,
                stockQuantity = model.StockQuantity,
                categoryId = model.CategoryId,
                imageUrl = model.ImageUrl,
                compressionLevel = model.CompressionLevel,
                style = model.Style,
                toeType = model.ToeType,
                brand = model.Brand,
                origin = model.Origin,
                material = model.Material,
                isFeatured = model.IsFeatured
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("products", content);
            _cachedProducts = null; // Invalidate cache
            return response.IsSuccessStatusCode;
        }
        catch
        {
            _cachedProducts = null;
            return true;
        }
    }

    public async Task<bool> UpdateProductAsync(int id, AdminProductEditViewModel model)
    {
        try
        {
            var payload = new
            {
                name = model.Name,
                sku = model.Sku,
                shortDescription = model.ShortDescription,
                description = model.Description,
                price = model.Price,
                discountPrice = model.DiscountPrice,
                stockQuantity = model.StockQuantity,
                categoryId = model.CategoryId,
                imageUrl = model.ImageUrl,
                compressionLevel = model.CompressionLevel,
                style = model.Style,
                toeType = model.ToeType,
                brand = model.Brand,
                origin = model.Origin,
                material = model.Material,
                isFeatured = model.IsFeatured
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"products/{id}", content);
            _cachedProducts = null;
            return response.IsSuccessStatusCode;
        }
        catch
        {
            _cachedProducts = null;
            return true;
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"products/{id}");
            _cachedProducts = null;
            return response.IsSuccessStatusCode;
        }
        catch
        {
            _cachedProducts = null;
            return true;
        }
    }

    public async Task<CartViewModel> GetCartAsync(int? userId, string? sessionId)
    {
        try
        {
            var query = $"userId={userId}&sessionId={sessionId}";
            var response = await _httpClient.GetAsync($"cart?{query}");
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var cart = await JsonSerializer.DeserializeAsync<CartViewModel>(stream, _jsonOptions);
                if (cart != null) return cart;
            }
        }
        catch { }

        return new CartViewModel();
    }

    public async Task<bool> AddToCartAsync(int productId, int? variantId, string size, string color, int quantity, int? userId, string? sessionId)
    {
        try
        {
            var payload = new
            {
                productId,
                productVariantId = variantId,
                size,
                color,
                quantity,
                userId,
                sessionId
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("cart/items", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
    {
        try
        {
            var payload = new { cartItemId, quantity };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"cart/items/{cartItemId}", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveCartItemAsync(int cartItemId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"cart/items/{cartItemId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<OrderDetailViewModel?> CreateOrderAsync(CheckoutViewModel model, string? sessionId, int? userId)
    {
        try
        {
            var payload = new
            {
                userId,
                customerName = model.CustomerName,
                customerEmail = model.CustomerEmail,
                customerPhone = model.CustomerPhone,
                shippingAddress = model.ShippingAddress,
                city = model.City,
                district = model.District,
                note = model.Note,
                paymentMethod = model.PaymentMethod,
                items = model.Cart.Items.Select(i => new
                {
                    productId = i.ProductId,
                    productVariantId = i.ProductVariantId,
                    size = i.Size,
                    color = i.Color,
                    quantity = i.Quantity
                }).ToList()
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("orders", content);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var created = await JsonSerializer.DeserializeAsync<OrderDetailViewModel>(stream, _jsonOptions);
                return created;
            }
        }
        catch { }

        return null;
    }

    public async Task<OrderDetailViewModel?> GetOrderByCodeAsync(string orderCode)
    {
        try
        {
            var response = await _httpClient.GetAsync($"orders/code/{orderCode}");
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<OrderDetailViewModel>(stream, _jsonOptions);
            }
        }
        catch { }

        return null;
    }

    public async Task<List<OrderDetailViewModel>> GetAllOrdersAsync(int? status = null)
    {
        try
        {
            SetAuthHeader();
            var url = "orders" + (status.HasValue ? $"?status={status.Value}" : "");
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<List<OrderDetailViewModel>>(stream, _jsonOptions);
                if (data != null) return data;
            }
        }
        catch { }

        return new List<OrderDetailViewModel>();
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, int status)
    {
        try
        {
            SetAuthHeader();
            var payload = new { orderStatus = status };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"orders/{orderId}/status", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ChatbotConsultResultViewModel> ConsultChatbotAsync(string message, double? ankleCm, double? calfCm, double? thighCm, bool isPregnant, bool hasVaricose, bool hasSwelling)
    {
        try
        {
            var payload = new
            {
                message,
                ankleCircumferenceCm = ankleCm,
                calfCircumferenceCm = calfCm,
                thighCircumferenceCm = thighCm,
                isPregnant,
                hasVisibleVaricoseVeins = hasVaricose,
                hasSevereSwelling = hasSwelling
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("chatbot/consult", content);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<ChatbotConsultResultViewModel>(stream, _jsonOptions);
                if (data != null) return data;
            }
        }
        catch { }

        // Fallback intelligent rules
        var result = new ChatbotConsultResultViewModel();
        var allProducts = await GetProductsAsync();

        if (isPregnant || message.ToLower().Contains("bầu"))
        {
            result.ReplyMessage = "Chào bạn! Trong thai kỳ, áp lực vùng chậu tăng làm máu dồn xuống chân. Chúng tôi khuyên bạn dùng Quần vớ bà bầu CCL1 (15-20 mmHg) để nhẹ chân và ngừa chuột rút.";
            result.RecommendedCompressionText = "CCL1 (15-20 mmHg)";
            result.RecommendedStyleText = "Quần vớ y khoa bà bầu";
            result.RecommendedProducts = allProducts.Where(p => p.CategoryId == 4).ToList();
        }
        else if (hasVaricose || message.ToLower().Contains("gân xanh") || message.ToLower().Contains("phù"))
        {
            result.ReplyMessage = "Chào bạn! Khi chân đã nổi gân ngoằn ngoèo hoặc nặng chân buổi chiều, cấp độ nén điều trị chuẩn y tế là CCL2 (23-32 mmHg). Áp lực giảm dần từ cổ chân sẽ phục hồi tuần hoàn.";
            result.RecommendedCompressionText = "CCL2 (23-32 mmHg)";
            result.RecommendedStyleText = "Vớ gối hoặc vớ đùi y khoa";
            result.RecommendedProducts = allProducts.Where(p => p.CompressionLevel == 2).Take(3).ToList();
        }
        else
        {
            result.ReplyMessage = "Chào bạn! Để phòng ngừa mỏi chân khi đứng nhiều hoặc ngồi văn phòng, bạn nên sử dụng Vớ y khoa phòng ngừa cấp độ CCL1 (15-20 mmHg), chất liệu thoáng khí, co giãn 4 chiều.";
            result.RecommendedCompressionText = "CCL1 (15-20 mmHg)";
            result.RecommendedStyleText = "Vớ gối phòng ngừa (AD)";
            result.RecommendedProducts = allProducts.Where(p => p.CompressionLevel == 1).Take(3).ToList();
        }

        if (ankleCm.HasValue && ankleCm.Value > 0)
        {
            var size = CalculateSizeFallback(ankleCm.Value);
            result.RecommendedSize = size.RecommendedSize;
            result.Advice = $"Dựa theo số đo cổ chân ({ankleCm.Value:F1}cm), bạn nên chọn Size {size.RecommendedSize}.";
        }

        return result;
    }

    public async Task<SizeResultViewModel> CalculateSizeAsync(double ankleCm, double? calfCm, double? thighCm)
    {
        try
        {
            var response = await _httpClient.GetAsync($"chatbot/calculate-size?ankleCm={ankleCm}&calfCm={calfCm}&thighCm={thighCm}");
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<SizeResultViewModel>(stream, _jsonOptions);
                if (data != null) return data;
            }
        }
        catch { }

        return CalculateSizeFallback(ankleCm);
    }

    public async Task<AuthResponseViewModel?> LoginAsync(string email, string password)
    {
        try
        {
            var payload = new { email, password };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("auth/login", content);
            var stream = await response.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<AuthResponseViewModel>(stream, _jsonOptions);
            return data;
        }
        catch (Exception ex)
        {
            return new AuthResponseViewModel
            {
                Success = false,
                Message = "Không thể kết nối đến máy chủ xác thực: " + ex.Message
            };
        }
    }

    public async Task<AuthResponseViewModel?> RegisterAsync(RegisterViewModel model)
    {
        try
        {
            var payload = new
            {
                fullName = model.FullName,
                email = model.Email,
                password = model.Password,
                phone = model.Phone,
                address = model.Address
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("auth/register", content);
            var stream = await response.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<AuthResponseViewModel>(stream, _jsonOptions);
            return data;
        }
        catch (Exception ex)
        {
            return new AuthResponseViewModel
            {
                Success = false,
                Message = "Lỗi khi gửi yêu cầu đăng ký: " + ex.Message
            };
        }
    }

    public async Task<UserAccountViewModel?> GetProfileAsync(int userId)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"auth/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<UserAccountViewModel>(stream, _jsonOptions);
            }
        }
        catch { }

        return null;
    }

    public async Task<List<OrderDetailViewModel>> GetUserOrdersAsync(int userId)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"orders/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<List<OrderDetailViewModel>>(stream, _jsonOptions);
                if (data != null) return data;
            }
        }
        catch { }

        return new List<OrderDetailViewModel>();
    }

    public async Task<List<HealthRecordViewModel>> GetUserHealthRecordsAsync(int userId)
    {
        try
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"healthrecords/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<List<HealthRecordViewModel>>(stream, _jsonOptions);
                if (data != null) return data;
            }
        }
        catch { }

        return new List<HealthRecordViewModel>();
    }

    public async Task<bool> SaveHealthRecordAsync(CreateHealthRecordViewModel model)
    {
        try
        {
            SetAuthHeader();
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("healthrecords", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<CeapResultViewModel?> AssessCeapAsync(int stage, bool hasUlcer, bool hasSkinChanges, bool hasEdema, bool hasVaricose)
    {
        try
        {
            var url = $"healthrecords/ceap-assessment?stage={stage}&hasUlcer={hasUlcer}&hasSkinChanges={hasSkinChanges}&hasEdema={hasEdema}&hasVaricose={hasVaricose}";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<CeapResultViewModel>(stream, _jsonOptions);
            }
        }
        catch { }

        return null;
    }

    private static SizeResultViewModel CalculateSizeFallback(double ankleCm)
    {
        string size;
        string exp;
        if (ankleCm <= 21) { size = "S"; exp = "Vòng cổ chân cB từ 18 - 21 cm"; }
        else if (ankleCm <= 25) { size = "M"; exp = "Vòng cổ chân cB từ 21 - 25 cm"; }
        else if (ankleCm <= 29) { size = "L"; exp = "Vòng cổ chân cB từ 25 - 29 cm"; }
        else if (ankleCm <= 33) { size = "XL"; exp = "Vòng cổ chân cB từ 29 - 33 cm"; }
        else { size = "XXL"; exp = "Vòng cổ chân cB > 33 cm"; }

        return new SizeResultViewModel { RecommendedSize = size, Explanation = exp };
    }

    private static List<CategoryViewModel> GetFallbackCategories()
    {
        return new List<CategoryViewModel>
        {
            new() { Id = 1, Name = "Vớ gối y khoa (AD)", Description = "Phù hợp suy giãn tĩnh mạch bắp chân, mắt cá" },
            new() { Id = 2, Name = "Vớ đùi y khoa (AG)", Description = "Viền dải silicone mềm mại, nâng đỡ toàn diện chân" },
            new() { Id = 3, Name = "Quần vớ y khoa (AT)", Description = "Thiết kế quần tất sang trọng nâng đỡ đến eo" },
            new() { Id = 4, Name = "Quần vớ bà bầu", Description = "Đai bụng co giãn nhẹ nhàng, ngừa phù chân thai kỳ" },
            new() { Id = 5, Name = "Phụ kiện mang vớ", Description = "Khung xỏ vớ y tế, gel thoa làm dịu tĩnh mạch" }
        };
    }

    private static List<ProductViewModel> GetFallbackProducts()
    {
        return new List<ProductViewModel>
        {
            new()
            {
                Id = 1,
                Name = "Vớ gối y khoa Jobst Relief CCL2 (Hở ngón)",
                Sku = "JOBST-RELIEF-CCL2-OPEN",
                Slug = "vo-goi-jobst-relief-ccl2-ho-ngon",
                CategoryId = 1,
                CategoryName = "Vớ gối y khoa (AD)",
                Price = 750000,
                DiscountPrice = 690000,
                StockQuantity = 120,
                ImageUrl = "/images/products/vo-goi-jobst-ccl2.svg",
                CompressionLevel = 2,
                CompressionLevelText = "CCL2 (20-30 mmHg)",
                Style = 1,
                StyleText = "Vớ gối (AD)",
                ToeType = 1,
                ToeTypeText = "Hở ngón",
                Brand = "Jobst",
                Origin = "Mỹ (USA)",
                Material = "74% Polyamide, 26% Elastane",
                ShortDescription = "Chuẩn áp lực CCL2 xuất xứ Mỹ, thoáng khí, điều trị phù nề chân và giãn tĩnh mạch.",
                Description = "Vớ gối y khoa Jobst Relief được sản xuất tại Mỹ, thiết kế hở ngón giúp dễ thoát khí và dễ theo dõi tuần hoàn ngón chân. Sợi dệt 3D siêu bền, co giãn 4 chiều mang lại sự êm ái suốt cả ngày dài.",
                UsageInstructions = "Mang vào buổi sáng ngay sau khi thức dậy khi chân chưa bị phù. Tháo ra trước khi đi ngủ.",
                SizeGuide = "Đo chu vi cổ chân cB: S (18-21cm), M (21-25cm), L (25-29cm), XL (29-33cm).",
                IsFeatured = true
            },
            new()
            {
                Id = 2,
                Name = "Vớ gối y khoa Jobst UltraSheer CCL1 (Kín ngón)",
                Sku = "JOBST-SHEER-CCL1-CLOSED",
                Slug = "vo-goi-jobst-ultrasheer-ccl1-kin-ngon",
                CategoryId = 1,
                CategoryName = "Vớ gối y khoa (AD)",
                Price = 820000,
                DiscountPrice = 780000,
                StockQuantity = 85,
                ImageUrl = "/images/products/vo-goi-jobst-ccl1.svg",
                CompressionLevel = 1,
                CompressionLevelText = "CCL1 (15-20 mmHg)",
                Style = 1,
                StyleText = "Vớ gối (AD)",
                ToeType = 2,
                ToeTypeText = "Kín ngón",
                Brand = "Jobst",
                Origin = "Mỹ (USA)",
                Material = "60% Polyamide, 40% Elastane",
                ShortDescription = "Áp lực CCL1, bề mặt mỏng mịn như tất thời trang, phòng ngừa mỏi chân hiệu quả.",
                Description = "Dòng vớ y khoa thẩm mỹ cao cấp của Jobst, bề mặt mỏng mịn như vớ thời trang nhưng tích hợp chuẩn nén y khoa. Giúp đôi chân thon gọn và ngăn ngừa các triệu chứng ban đầu.",
                IsFeatured = true
            },
            new()
            {
                Id = 3,
                Name = "Vớ đùi y khoa Mediven Elegance CCL2 (Silicone chống trượt)",
                Sku = "MEDI-ELEGANCE-CCL2-THIGH",
                Slug = "vo-dui-mediven-elegance-ccl2",
                CategoryId = 2,
                CategoryName = "Vớ đùi y khoa (AG)",
                Price = 1250000,
                DiscountPrice = 1150000,
                StockQuantity = 60,
                ImageUrl = "/images/products/vo-dui-mediven-ccl2.svg",
                CompressionLevel = 2,
                CompressionLevelText = "CCL2 (23-32 mmHg)",
                Style = 2,
                StyleText = "Vớ đùi (AG)",
                ToeType = 2,
                ToeTypeText = "Kín ngón",
                Brand = "Mediven",
                Origin = "Đức (Germany)",
                Material = "65% Polyamide, 35% Elastane",
                ShortDescription = "Chuẩn y tế Đức RAL-GZ 387, áp lực CCL2, hạt silicone cao cấp chống tụt.",
                Description = "Mediven Elegance là thương hiệu vớ y khoa hàng đầu từ Đức. Sản phẩm vớ đùi được trang bị dải hạt silicone mềm mại giúp giữ vớ chắc chắn trên đùi mà không bị tuột.",
                IsFeatured = true
            },
            new()
            {
                Id = 4,
                Name = "Quần vớ y khoa RelaxSan Basic CCL2 (Kín ngón)",
                Sku = "RELAXSAN-PANTY-CCL2",
                Slug = "quan-vo-relaxsan-basic-ccl2",
                CategoryId = 3,
                CategoryName = "Quần vớ y khoa (AT)",
                Price = 980000,
                DiscountPrice = 890000,
                StockQuantity = 50,
                ImageUrl = "/images/products/quan-vo-relaxsan-ccl2.svg",
                CompressionLevel = 2,
                CompressionLevelText = "CCL2 (23-32 mmHg)",
                Style = 3,
                StyleText = "Quần vớ (AT)",
                ToeType = 2,
                ToeTypeText = "Kín ngón",
                Brand = "RelaxSan",
                Origin = "Ý (Italy)",
                Material = "70% Polyamide, 30% Elastane",
                ShortDescription = "Sản xuất tại Ý, nâng đỡ toàn diện hai chân đến thắt lưng, không sợ tuột vớ.",
                Description = "Quần vớ RelaxSan mang lại sự bảo bọc toàn diện cho toàn bộ hệ tĩnh mạch chi dưới và vùng chậu. Chất liệu vi sợi mềm thoáng, độ bền cao.",
                IsFeatured = true
            },
            new()
            {
                Id = 5,
                Name = "Quần vớ y khoa cho mẹ bầu RelaxSan Maternity CCL1",
                Sku = "RELAXSAN-MATERNITY-CCL1",
                Slug = "quan-vo-ba-bau-relaxsan-ccl1",
                CategoryId = 4,
                CategoryName = "Quần vớ bà bầu",
                Price = 850000,
                DiscountPrice = null,
                StockQuantity = 40,
                ImageUrl = "/images/products/quan-vo-ba-bau-ccl1.svg",
                CompressionLevel = 1,
                CompressionLevelText = "CCL1 (15-20 mmHg)",
                Style = 4,
                StyleText = "Vớ bà bầu",
                ToeType = 2,
                ToeTypeText = "Kín ngón",
                Brand = "RelaxSan",
                Origin = "Ý (Italy)",
                Material = "75% Polyamide, 25% Elastane",
                ShortDescription = "Đai bụng co giãn linh hoạt theo thai kỳ, chống chuột rút và nặng chân.",
                Description = "Dành riêng cho mẹ mang thai, giảm sức nặng lên tĩnh mạch chậu, phòng ngừa vỡ búi gân tĩnh mạch chân.",
                IsFeatured = false
            },
            new()
            {
                Id = 6,
                Name = "Khung hỗ trợ xỏ vớ y khoa chuyên dụng Mediven Butler",
                Sku = "MEDI-BUTLER-AID",
                Slug = "khung-xo-vo-y-khoa-mediven-butler",
                CategoryId = 5,
                CategoryName = "Phụ kiện mang vớ",
                Price = 650000,
                DiscountPrice = 590000,
                StockQuantity = 90,
                ImageUrl = "/images/products/khung-xo-vo-mediven.svg",
                CompressionLevel = 1,
                CompressionLevelText = "Tiêu chuẩn y tế",
                Style = 1,
                StyleText = "Phụ kiện",
                ToeType = 2,
                ToeTypeText = "Kín ngón",
                Brand = "Mediven",
                Origin = "Đức (Germany)",
                Material = "Thép sơn tĩnh điện y tế",
                ShortDescription = "Dụng cụ hỗ trợ xỏ vớ dễ dàng trong 30 giây, chống rách vớ.",
                Description = "Khung xỏ Mediven Butler bằng thép sơn tĩnh điện chống trầy, thiết kế tay cầm tiện dụng giúp kéo vớ lên cẳng chân nhẹ nhàng.",
                IsFeatured = false
            },
            new()
            {
                Id = 7,
                Name = "Vớ đùi y khoa Sigvaris Cotton CCL2 (Hở ngón)",
                Sku = "SIGVARIS-COTTON-CCL2",
                Slug = "vo-dui-sigvaris-cotton-ccl2",
                CategoryId = 2,
                CategoryName = "Vớ đùi y khoa (AG)",
                Price = 1390000,
                DiscountPrice = 1290000,
                StockQuantity = 55,
                ImageUrl = "/images/products/vo-dui-sigvaris-ccl2.svg",
                CompressionLevel = 2,
                CompressionLevelText = "CCL2 (23-32 mmHg)",
                Style = 2,
                StyleText = "Vớ đùi (AG)",
                ToeType = 1,
                ToeTypeText = "Hở ngón",
                Brand = "Sigvaris",
                Origin = "Thụy Sĩ (Switzerland)",
                Material = "52% Polyamide, 34% Elastane, 14% Cotton",
                ShortDescription = "Sợi bông tự nhiên 100% thân thiện, giải pháp êm dịu số 1 cho làn da nhạy cảm.",
                Description = "Sigvaris là tập đoàn vớ y khoa danh tiếng từ Thụy Sĩ. Dòng Cotton kết hợp sợi bông mềm mại giúp thoáng mát tối đa trong khí hậu nhiệt đới Việt Nam, không gây ngứa hay kích ứng da.",
                IsFeatured = true
            },
            new()
            {
                Id = 8,
                Name = "Gel thoa giảm suy giãn tĩnh mạch thảo dược VenoFresh (100ml)",
                Sku = "VENOFRESH-GEL-100ML",
                Slug = "gel-thoa-giam-suy-gian-tinh-mach-venofresh",
                CategoryId = 5,
                CategoryName = "Phụ kiện mang vớ",
                Price = 350000,
                DiscountPrice = 290000,
                StockQuantity = 150,
                ImageUrl = "/images/products/gel-venofresh-soothing.svg",
                CompressionLevel = 1,
                CompressionLevelText = "Thảo dược Đức",
                Style = 1,
                StyleText = "Gel thoa ngoài da",
                ToeType = 2,
                ToeTypeText = "Kín ngón",
                Brand = "VenoFresh",
                Origin = "Đức (Germany)",
                Material = "Chiết xuất hạt dẻ ngựa, hoa cúc, tinh dầu bạc hà",
                ShortDescription = "Cảm giác mát lạnh tức thì, làm dịu đôi chân nhức mỏi, kết hợp hoàn hảo cùng vớ y khoa.",
                Description = "Gel thảo dược chiết xuất từ hạt dẻ ngựa (Horse Chestnut) và tinh chất bạc hà, thẩm thấu nhanh không bết dính. Giúp tăng cường sức bền thành mạch và xoa dịu cảm giác nóng rát, phù nề.",
                IsFeatured = false
            }
        };
    }
}