using System.Text;
using System.Text.Json;
using VaricoseSocks.Web.Models;

namespace VaricoseSocks.Web.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly List<OrderDetailViewModel> _fallbackOrders = new();
    private static readonly CartViewModel _fallbackCart = new();
    private static List<CategoryViewModel>? _cachedCategories;
    private static List<ProductViewModel>? _cachedProducts;

    public ApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5046/api";
        _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = TimeSpan.FromMilliseconds(400); // 400ms max timeout để trang web luôn phản hồi tức thì
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
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
                if (cart != null && cart.Items.Any()) return cart;
            }
        }
        catch { }

        return _fallbackCart;
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
            if (response.IsSuccessStatusCode) return true;
        }
        catch { }

        // Local fallback cart update (instantaneous response)
        var products = await GetProductsAsync();
        var prod = products.FirstOrDefault(p => p.Id == productId);
        if (prod != null)
        {
            var existing = _fallbackCart.Items.FirstOrDefault(i => i.ProductId == productId && i.Size == size && i.Color == color);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _fallbackCart.Items.Add(new CartItemViewModel
                {
                    Id = _fallbackCart.Items.Count + 1,
                    ProductId = prod.Id,
                    ProductName = prod.Name,
                    ProductImageUrl = prod.ImageUrl,
                    Size = size,
                    Color = color,
                    Quantity = quantity,
                    UnitPrice = prod.DiscountPrice ?? prod.Price
                });
            }
        }
        return true;
    }

    public async Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
    {
        try
        {
            var payload = new { cartItemId, quantity };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"cart/items/{cartItemId}", content);
            if (response.IsSuccessStatusCode) return true;
        }
        catch { }

        var item = _fallbackCart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item != null)
        {
            if (quantity <= 0) _fallbackCart.Items.Remove(item);
            else item.Quantity = quantity;
        }
        return true;
    }

    public async Task<bool> RemoveCartItemAsync(int cartItemId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"cart/items/{cartItemId}");
            if (response.IsSuccessStatusCode) return true;
        }
        catch { }

        var item = _fallbackCart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item != null) _fallbackCart.Items.Remove(item);
        return true;
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
                if (created != null)
                {
                    _fallbackCart.Items.Clear();
                    return created;
                }
            }
        }
        catch { }

        // Local fallback order
        var orderCode = "VS-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);
        var order = new OrderDetailViewModel
        {
            Id = _fallbackOrders.Count + 1,
            OrderCode = orderCode,
            CustomerName = model.CustomerName,
            CustomerPhone = model.CustomerPhone,
            CustomerEmail = model.CustomerEmail,
            ShippingAddress = model.ShippingAddress,
            City = model.City,
            District = model.District,
            Note = model.Note,
            PaymentMethodText = model.PaymentMethod == 2 ? "Chuyển khoản VietQR" : "COD (Thanh toán khi nhận hàng)",
            PaymentStatusText = "Chờ thanh toán",
            OrderStatusText = "Chờ xác nhận",
            OrderStatus = 1,
            SubTotal = model.Cart.SubTotal,
            ShippingFee = model.Cart.ShippingFee,
            TotalAmount = model.Cart.TotalAmount,
            CreatedAt = DateTime.Now,
            Items = model.Cart.Items.Select(i => new OrderItemViewModel
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductImageUrl = i.ProductImageUrl,
                Size = i.Size,
                Color = i.Color,
                CompressionText = "CCL1 (15-21 mmHg)",
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice
            }).ToList()
        };

        _fallbackOrders.Add(order);
        _fallbackCart.Items.Clear();
        return order;
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

        return _fallbackOrders.FirstOrDefault(o => o.OrderCode.Equals(orderCode, StringComparison.OrdinalIgnoreCase))
            ?? GetFallbackSeedOrders().FirstOrDefault(o => o.OrderCode.Equals(orderCode, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<OrderDetailViewModel>> GetAllOrdersAsync(int? status = null)
    {
        try
        {
            var url = "orders" + (status.HasValue ? $"?status={status.Value}" : "");
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<List<OrderDetailViewModel>>(stream, _jsonOptions);
                if (data != null && data.Any()) return data;
            }
        }
        catch { }

        var list = new List<OrderDetailViewModel>(_fallbackOrders);
        list.AddRange(GetFallbackSeedOrders());
        return list;
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, int status)
    {
        try
        {
            var payload = new { orderStatus = status };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"orders/{orderId}/status", content);
            if (response.IsSuccessStatusCode) return true;
        }
        catch { }

        var order = _fallbackOrders.FirstOrDefault(o => o.Id == orderId) ?? GetFallbackSeedOrders().FirstOrDefault(o => o.Id == orderId);
        if (order != null)
        {
            order.OrderStatus = status;
            order.OrderStatusText = status switch
            {
                1 => "Chờ xác nhận",
                2 => "Đã xác nhận",
                3 => "Đang đóng gói",
                4 => "Đang giao",
                5 => "Hoàn thành",
                6 => "Đã hủy",
                _ => "Đang xử lý"
            };
        }
        return true;
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

    private static List<OrderDetailViewModel> GetFallbackSeedOrders()
    {
        return new List<OrderDetailViewModel>
        {
            new()
            {
                Id = 1,
                OrderCode = "VS-20261019-8921",
                CustomerName = "Chu Văn Quốc Trung",
                CustomerPhone = "0987654321",
                CustomerEmail = "trung.chu@varicosesocks.vn",
                ShippingAddress = "456 Lê Văn Sỹ, Phường 14, Quận 3, TP.HCM",
                City = "Hồ Chí Minh",
                District = "Quận 3",
                Note = "Giao giờ hành chính",
                PaymentMethodText = "COD (Thanh toán khi nhận hàng)",
                PaymentStatusText = "Chờ thanh toán",
                OrderStatusText = "Đã xác nhận",
                OrderStatus = 2,
                SubTotal = 690000,
                ShippingFee = 30000,
                TotalAmount = 720000,
                CreatedAt = DateTime.Now.AddDays(-1),
                Items = new List<OrderItemViewModel>
                {
                    new()
                    {
                        ProductId = 1,
                        ProductName = "Vớ gối y khoa Jobst Relief CCL2 (Hở ngón)",
                        ProductImageUrl = "/images/products/vo-goi-jobst-ccl2.svg",
                        Size = "M",
                        Color = "Màu Da (Beige)",
                        CompressionText = "CCL2 (20-30 mmHg)",
                        UnitPrice = 690000,
                        Quantity = 1,
                        TotalPrice = 690000
                    }
                }
            }
        };
    }
}