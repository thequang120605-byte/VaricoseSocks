using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var passwordHasher = new PasswordHasher<User>();

        // 1. Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            var adminRole = new Role { Name = "Admin", Description = "Quản trị viên toàn quyền hệ thống" };
            var staffRole = new Role { Name = "Staff", Description = "Nhân viên bán hàng và hỗ trợ khách hàng" };
            var customerRole = new Role { Name = "Customer", Description = "Khách hàng mua sắm" };

            await context.Roles.AddRangeAsync(adminRole, staffRole, customerRole);
            await context.SaveChangesAsync();
        }

        // 2. Seed Users
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                FullName = "Quản Trị Viên",
                Email = "admin@varicosesocks.vn",
                Phone = "0901234567",
                Address = "123 Hai Bà Trưng, Phường Bến Nghé, Quận 1, TP.HCM",
                City = "Hồ Chí Minh",
                District = "Quận 1",
                IsActive = true
            };
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");

            var customerUser = new User
            {
                FullName = "Chu Văn Quốc Trung",
                Email = "trung.chu@varicosesocks.vn",
                Phone = "0987654321",
                Address = "456 Lê Văn Sỹ, Phường 14, Quận 3, TP.HCM",
                City = "Hồ Chí Minh",
                District = "Quận 3",
                IsActive = true
            };
            customerUser.PasswordHash = passwordHasher.HashPassword(customerUser, "123456");

            await context.Users.AddRangeAsync(adminUser, customerUser);
            await context.SaveChangesAsync();

            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var customerRole = await context.Roles.FirstAsync(r => r.Name == "Customer");

            await context.UserRoles.AddRangeAsync(
                new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id },
                new UserRole { UserId = customerUser.Id, RoleId = customerRole.Id }
            );
            await context.SaveChangesAsync();
        }
        else
        {
            // Upgrade legacy plain-text or dummy hashes to standard PBKDF2 hash
            var existingUsers = await context.Users.ToListAsync();
            foreach (var u in existingUsers)
            {
                if (u.PasswordHash.Contains("...") || u.PasswordHash == "123456" || u.PasswordHash == "Admin@123" || !u.PasswordHash.StartsWith("AQAAAA"))
                {
                    var defaultPwd = u.Email.Contains("admin", StringComparison.OrdinalIgnoreCase) ? "Admin@123" : "123456";
                    u.PasswordHash = passwordHasher.HashPassword(u, defaultPwd);
                }
            }
            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }
        }

        // 3. Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Vớ gối y khoa (AD)", Slug = "vo-goi-y-khoa", Description = "Dành cho người suy giãn tĩnh mạch bắp chân, mắt cá, phù chân nhẹ đến trung bình.", ImageUrl = "/images/products/vo-goi-jobst-ccl2.svg", DisplayOrder = 1 },
                new() { Name = "Vớ đùi y khoa (AG)", Slug = "vo-dui-y-khoa", Description = "Dành cho người giãn tĩnh mạch nổi cao trên đùi, sau phẫu thuật can thiệp tĩnh mạch.", ImageUrl = "/images/products/vo-dui-mediven-ccl2.svg", DisplayOrder = 2 },
                new() { Name = "Quần vớ y khoa (AT)", Slug = "quan-vo-y-khoa", Description = "Nâng đỡ toàn bộ đôi chân đến eo, không bị tụt, dáng thẩm mỹ cao cấp.", ImageUrl = "/images/products/quan-vo-relaxsan-ccl2.svg", DisplayOrder = 3 },
                new() { Name = "Quần vớ cho bà bầu (AT/M)", Slug = "quan-vo-ba-bau", Description = "Thiết kế vùng bụng co giãn nhẹ nhàng, ngừa chuột rút và phù chân khi mang thai.", ImageUrl = "/images/products/quan-vo-ba-bau-ccl1.svg", DisplayOrder = 4 },
                new() { Name = "Phụ kiện hỗ trợ mang vớ", Slug = "phu-kien-ho-tro", Description = "Khung xỏ vớ, găng tay cao su y tế giúp mang và bảo quản vớ dễ dàng không rách.", ImageUrl = "/images/products/khung-xo-vo-mediven.svg", DisplayOrder = 5 }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // 4. Seed Products
        if (!await context.Products.AnyAsync())
        {
            var catGoi = await context.Categories.FirstAsync(c => c.Slug == "vo-goi-y-khoa");
            var catDui = await context.Categories.FirstAsync(c => c.Slug == "vo-dui-y-khoa");
            var catQuan = await context.Categories.FirstAsync(c => c.Slug == "quan-vo-y-khoa");
            var catBau = await context.Categories.FirstAsync(c => c.Slug == "quan-vo-ba-bau");
            var catPhuKien = await context.Categories.FirstAsync(c => c.Slug == "phu-kien-ho-tro");

            var products = new List<Product>
            {
                new()
                {
                    CategoryId = catGoi.Id,
                    Name = "Vớ gối y khoa Jobst Relief CCL2 (Hở ngón)",
                    Slug = "vo-goi-jobst-relief-ccl2-ho-ngon",
                    Sku = "JOBST-RELIEF-CCL2-OPEN",
                    ShortDescription = "Chuẩn áp lực CCL2 (20-30 mmHg) xuất xứ Mỹ, thoáng khí, điều trị phù nề và giãn tĩnh mạch chân hiệu quả.",
                    Description = "Vớ gối y khoa Jobst Relief được sản xuất tại Mỹ, thiết kế hở ngón giúp dễ thoát khí và dễ theo dõi tuần hoàn ngón chân. Sợi dệt 3D siêu bền, co giãn 4 chiều mang lại sự êm ái suốt cả ngày dài. Khuyến nghị cho người đứng/ngồi lâu, giáo viên, bác sĩ, nhân viên văn phòng.",
                    Price = 750000,
                    DiscountPrice = 690000,
                    StockQuantity = 120,
                    ImageUrl = "/images/products/vo-goi-jobst-ccl2.svg",
                    CompressionLevel = CompressionLevel.CCL2,
                    Style = SockStyle.KneeHigh,
                    ToeType = ToeType.OpenToe,
                    Brand = "Jobst",
                    Origin = "Mỹ (USA)",
                    Material = "74% Polyamide, 26% Elastane",
                    UsageInstructions = "Mang vào buổi sáng ngay sau khi thức dậy khi chân chưa bị phù. Tháo ra trước khi đi ngủ. Giặt tay với xà phòng dịu nhẹ.",
                    SizeGuide = "Đo chu vi cổ chân nhỏ nhất (cB) và bắp chân nở nhất (cC): Size S (cB: 18-21cm), M (cB: 21-25cm), L (cB: 25-29cm), XL (cB: 29-33cm).",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catGoi.Id,
                    Name = "Vớ gối y khoa Jobst UltraSheer CCL1 (Kín ngón)",
                    Slug = "vo-goi-jobst-ultrasheer-ccl1-kin-ngon",
                    Sku = "JOBST-SHEER-CCL1-CLOSED",
                    ShortDescription = "Áp lực CCL1 (15-20 mmHg), chất liệu siêu mịn trong suốt như tất da thời trang, phòng ngừa mỏi chân và gân xanh.",
                    Description = "Dòng vớ thẩm mỹ cao cấp nhất của Jobst, bề mặt mỏng mịn như vớ thời trang nhưng tích hợp chuẩn nén y khoa. Giúp đôi chân thon gọn và ngăn ngừa các triệu chứng ban đầu của suy giãn tĩnh mạch.",
                    Price = 820000,
                    DiscountPrice = 780000,
                    StockQuantity = 85,
                    ImageUrl = "/images/products/vo-goi-jobst-ccl1.svg",
                    CompressionLevel = CompressionLevel.CCL1,
                    Style = SockStyle.KneeHigh,
                    ToeType = ToeType.ClosedToe,
                    Brand = "Jobst",
                    Origin = "Mỹ (USA)",
                    Material = "60% Polyamide, 40% Elastane",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catDui.Id,
                    Name = "Vớ đùi y khoa Mediven Elegance CCL2 (Có dải hạt silicone chống trượt)",
                    Slug = "vo-dui-mediven-elegance-ccl2",
                    Sku = "MEDI-ELEGANCE-CCL2-THIGH",
                    ShortDescription = "Chuẩn y tế Đức RAL-GZ 387, áp lực CCL2 (23-32 mmHg), viền silicone cao cấp không gây kích ứng da.",
                    Description = "Mediven Elegance là thương hiệu vớ y khoa hàng đầu từ Đức. Sản phẩm vớ đùi được trang bị dải hạt silicone mềm mại giúp giữ vớ chắc chắn trên đùi mà không bị tuột khi vận động. Hỗ trợ điều trị suy tĩnh mạch sâu, huyết khối tĩnh mạch chi dưới.",
                    Price = 1250000,
                    DiscountPrice = 1150000,
                    StockQuantity = 60,
                    ImageUrl = "/images/products/vo-dui-mediven-ccl2.svg",
                    CompressionLevel = CompressionLevel.CCL2,
                    Style = SockStyle.ThighHigh,
                    ToeType = ToeType.ClosedToe,
                    Brand = "Mediven",
                    Origin = "Đức (Germany)",
                    Material = "65% Polyamide, 35% Elastane",
                    SizeGuide = "Đo cổ chân cB, bắp chân cC và vòng đùi dưới nếp lằn mông 5cm (cG).",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catDui.Id,
                    Name = "Vớ đùi y khoa Sigvaris Cotton CCL2 (Siêu êm cho da nhạy cảm)",
                    Slug = "vo-dui-sigvaris-cotton-ccl2",
                    Sku = "SIGVARIS-COTTON-CCL2",
                    ShortDescription = "Sợi dệt Cotton Thụy Sĩ tự nhiên, áp lực CCL2 (23-32 mmHg), thấm hút mồ hôi vượt trội, dành riêng cho da dễ ngứa.",
                    Description = "Dòng vớ y khoa Sigvaris Cotton cao cấp từ Thụy Sĩ, mặt trong tiếp xúc trực tiếp với da hoàn toàn bằng sợi cotton tự nhiên tạo cảm giác thoáng mát tối đa cho khí hậu nhiệt đới Việt Nam.",
                    Price = 1450000,
                    DiscountPrice = 1380000,
                    StockQuantity = 45,
                    ImageUrl = "/images/products/vo-dui-sigvaris-ccl2.svg",
                    CompressionLevel = CompressionLevel.CCL2,
                    Style = SockStyle.ThighHigh,
                    ToeType = ToeType.ClosedToe,
                    Brand = "Sigvaris",
                    Origin = "Thụy Sĩ (Switzerland)",
                    Material = "14% Cotton, 52% Polyamide, 34% Elastane",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catQuan.Id,
                    Name = "Quần vớ y khoa RelaxSan Basic CCL2 (Kín ngón)",
                    Slug = "quan-vo-relaxsan-basic-ccl2",
                    Sku = "RELAXSAN-PANTY-CCL2",
                    ShortDescription = "Sản xuất 100% tại Ý, áp lực CCL2 (23-32 mmHg), bảo vệ toàn diện từ ngón chân đến thắt lưng.",
                    Description = "Quần vớ RelaxSan mang lại sự bảo bọc toàn diện cho toàn bộ hệ tĩnh mạch chi dưới và vùng chậu. Chất liệu vi sợi mềm thoáng, độ bền cao, thích hợp cho người có nhu cầu điều trị kết hợp định hình chân thon thả.",
                    Price = 980000,
                    DiscountPrice = 890000,
                    StockQuantity = 50,
                    ImageUrl = "/images/products/quan-vo-relaxsan-ccl2.svg",
                    CompressionLevel = CompressionLevel.CCL2,
                    Style = SockStyle.Pantyhose,
                    ToeType = ToeType.ClosedToe,
                    Brand = "RelaxSan",
                    Origin = "Ý (Italy)",
                    Material = "70% Polyamide, 30% Elastane",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catBau.Id,
                    Name = "Quần vớ y khoa cho mẹ bầu RelaxSan Maternity CCL1",
                    Slug = "quan-vo-ba-bau-relaxsan-ccl1",
                    Sku = "RELAXSAN-MATERNITY-CCL1",
                    ShortDescription = "Thiết kế đai bụng điều chỉnh linh hoạt theo từng tháng thai kỳ, ngăn ngừa chuột rút và phù chân khi mang thai.",
                    Description = "Thai kỳ làm tăng áp lực ổ bụng và lượng máu tuần hoàn, rất dễ gây phù nề và giãn búi tĩnh mạch chân. Quần vớ bầu RelaxSan với cạp nâng đỡ êm dịu, lực ép giảm dần từ cổ chân giúp máu lưu thông về tim tốt hơn, giảm đau nhức tức thì.",
                    Price = 850000,
                    DiscountPrice = null,
                    StockQuantity = 40,
                    ImageUrl = "/images/products/quan-vo-ba-bau-ccl1.svg",
                    CompressionLevel = CompressionLevel.CCL1,
                    Style = SockStyle.Maternity,
                    ToeType = ToeType.ClosedToe,
                    Brand = "RelaxSan",
                    Origin = "Ý (Italy)",
                    Material = "75% Polyamide, 25% Elastane",
                    IsFeatured = false,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catPhuKien.Id,
                    Name = "Khung hỗ trợ xỏ vớ y khoa chuyên dụng Mediven Butler",
                    Slug = "khung-xo-vo-y-khoa-mediven-butler",
                    Sku = "MEDI-BUTLER-AID",
                    ShortDescription = "Dụng cụ hỗ trợ xỏ vớ dễ dàng trong 30 giây, đặc biệt phù hợp cho người lớn tuổi hoặc người khó cúi lưng.",
                    Description = "Do vớ y khoa có lực nén cao nên việc mang bằng tay không có thể tốn nhiều sức. Khung xỏ Mediven Butler bằng thép sơn tĩnh điện chống trầy, thiết kế tay cầm tiện dụng giúp kéo vớ lên cẳng chân nhẹ nhàng, không sợ móng tay làm xước rách vớ.",
                    Price = 650000,
                    DiscountPrice = 590000,
                    StockQuantity = 90,
                    ImageUrl = "/images/products/khung-xo-vo-mediven.svg",
                    CompressionLevel = CompressionLevel.CCL1,
                    Style = SockStyle.KneeHigh,
                    ToeType = ToeType.ClosedToe,
                    Brand = "Mediven",
                    Origin = "Đức (Germany)",
                    Material = "Thép phủ sơn tĩnh điện y tế",
                    IsFeatured = false,
                    IsActive = true
                },
                new()
                {
                    CategoryId = catPhuKien.Id,
                    Name = "Gel thoa dịu mát và săn chắc tĩnh mạch VenoFresh 100ml",
                    Slug = "gel-thoa-diu-mat-venofresh-100ml",
                    Sku = "VENOFRESH-GEL-100ML",
                    ShortDescription = "Chiết xuất hạt dẻ ngựa và tinh dầu bạc hà, làm dịu cảm giác nặng chân, nhức mỏi ngay sau 5 phút.",
                    Description = "Gel bôi VenoFresh chiết xuất từ Aescin thảo dược thiên nhiên, thẩm thấu nhanh không nhờn rít, an toàn khi kết hợp mang cùng vớ y khoa, giúp tăng độ đàn hồi thành mạch và xua tan căng thẳng bàn chân.",
                    Price = 320000,
                    DiscountPrice = 280000,
                    StockQuantity = 150,
                    ImageUrl = "/images/products/gel-venofresh-soothing.svg",
                    CompressionLevel = CompressionLevel.CCL1,
                    Style = SockStyle.KneeHigh,
                    ToeType = ToeType.ClosedToe,
                    Brand = "VenoFresh",
                    Origin = "Pháp (France)",
                    Material = "Aescin 2%, Menthol, Hamamelis",
                    IsFeatured = true,
                    IsActive = true
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // Seed Variants for each product
            var variants = new List<ProductVariant>();
            foreach (var p in products)
            {
                if (p.CategoryId == catPhuKien.Id)
                {
                    variants.Add(new ProductVariant { ProductId = p.Id, Sku = $"{p.Sku}-FREE", Size = "FreeSize", Color = "Bạc", StockQuantity = 90 });
                }
                else
                {
                    variants.Add(new ProductVariant { ProductId = p.Id, Sku = $"{p.Sku}-S-BEIGE", Size = "S", Color = "Màu Da (Beige)", StockQuantity = 25 });
                    variants.Add(new ProductVariant { ProductId = p.Id, Sku = $"{p.Sku}-M-BEIGE", Size = "M", Color = "Màu Da (Beige)", StockQuantity = 35 });
                    variants.Add(new ProductVariant { ProductId = p.Id, Sku = $"{p.Sku}-L-BEIGE", Size = "L", Color = "Màu Da (Beige)", StockQuantity = 30 });
                    variants.Add(new ProductVariant { ProductId = p.Id, Sku = $"{p.Sku}-XL-BEIGE", Size = "XL", Color = "Màu Da (Beige)", StockQuantity = 20 });
                    variants.Add(new ProductVariant { ProductId = p.Id, Sku = $"{p.Sku}-M-BLACK", Size = "M", Color = "Màu Đen (Black)", StockQuantity = 20 });
                    variants.Add(new ProductVariant { ProductId = p.Id, Sku = $"{p.Sku}-L-BLACK", Size = "L", Color = "Màu Đen (Black)", StockQuantity = 20 });
                }
            }
            await context.ProductVariants.AddRangeAsync(variants);
            await context.SaveChangesAsync();
        }

        // 5. Seed a demo Cart and Order if empty
        if (!await context.Orders.AnyAsync())
        {
            var customer = await context.Users.FirstOrDefaultAsync(u => u.Email == "trung.chu@varicosesocks.vn");
            var sampleProduct = await context.Products.FirstAsync();

            var order = new Order
            {
                OrderCode = "VS-20261019-8921",
                UserId = customer?.Id,
                CustomerName = customer?.FullName ?? "Chu Văn Quốc Trung",
                CustomerEmail = customer?.Email ?? "trung.chu@varicosesocks.vn",
                CustomerPhone = customer?.Phone ?? "0987654321",
                ShippingAddress = customer?.Address ?? "456 Lê Văn Sỹ, Phường 14, Quận 3, TP.HCM",
                City = "Hồ Chí Minh",
                District = "Quận 3",
                Note = "Giao hàng giờ hành chính, gọi trước khi giao 15 phút.",
                PaymentMethod = PaymentMethod.COD,
                PaymentStatus = PaymentStatus.Pending,
                OrderStatus = OrderStatus.Confirmed,
                SubTotal = 690000,
                ShippingFee = 30000,
                DiscountAmount = 0,
                TotalAmount = 720000,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            var orderDetail = new OrderDetail
            {
                OrderId = order.Id,
                ProductId = sampleProduct.Id,
                ProductName = sampleProduct.Name,
                Sku = sampleProduct.Sku,
                Size = "M",
                Color = "Màu Da (Beige)",
                CompressionLevel = sampleProduct.CompressionLevel,
                UnitPrice = 690000,
                Quantity = 1,
                TotalPrice = 690000
            };

            await context.OrderDetails.AddAsync(orderDetail);
            await context.SaveChangesAsync();
        }

        // 6. Seed Promotions
        if (!await context.Promotions.AnyAsync())
        {
            await context.Promotions.AddRangeAsync(
                new Promotion { Code = "VARICOSE10", Name = "Giảm 10% đơn hàng vớ y khoa", DiscountType = DiscountType.Percentage, DiscountValue = 10, MinOrderAmount = 500000, MaxDiscountAmount = 100000 },
                new Promotion { Code = "FREESHIP", Name = "Miễn phí vận chuyển toàn quốc", DiscountType = DiscountType.FixedAmount, DiscountValue = 30000, MinOrderAmount = 300000 },
                new Promotion { Code = "YTE50K", Name = "Hỗ trợ điều trị suy giãn tĩnh mạch 50k", DiscountType = DiscountType.FixedAmount, DiscountValue = 50000, MinOrderAmount = 700000 }
            );
            await context.SaveChangesAsync();
        }
    }
}
