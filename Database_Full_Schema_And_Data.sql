-- =====================================================================================
-- DỰ ÁN: HỆ THỐNG WEBSITE BÁN VỚ Y KHOA ĐIỀU TRỊ SUY GIÃN TĨNH MẠCH (VARICOSE SOCKS)
-- CÔNG NGHỆ: .NET 9 Core / Clean Architecture / SQL Server & MySQL Compatibility
-- SINH VIÊN: Chu Văn Quốc Trung (Frontend / Full-stack)
-- =====================================================================================

-- 1. BẢNG ROLES (Vai trò quyền hạn người dùng)
CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,       -- Admin, Staff, Customer
    Description NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- 2. BẢNG USERS (Người dùng, Khách hàng, Quản trị viên)
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Address NVARCHAR(300) NULL,
    City NVARCHAR(100) NULL,
    District NVARCHAR(100) NULL,
    AvatarUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

-- 3. BẢNG USER_ROLES (Quan hệ nhiều - nhiều giữa User và Role)
CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);

-- 4. BẢNG CATEGORIES (Danh mục phân loại vớ y khoa)
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Slug NVARCHAR(150) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    ImageUrl NVARCHAR(500) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- 5. BẢNG PRODUCTS (Sản phẩm vớ y khoa và phụ kiện)
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    Name NVARCHAR(250) NOT NULL,
    Slug NVARCHAR(250) NOT NULL,
    Sku NVARCHAR(50) NOT NULL UNIQUE,
    ShortDescription NVARCHAR(500) NULL,
    Description NVARCHAR(MAX) NULL,
    Price DECIMAL(18,2) NOT NULL,
    DiscountPrice DECIMAL(18,2) NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(500) NULL,
    
    -- Thông số y khoa chuyên biệt cho vớ giãn tĩnh mạch:
    CompressionLevel INT NOT NULL DEFAULT 1,  -- 1: CCL1 (15-21 mmHg), 2: CCL2 (23-32 mmHg), 3: CCL3 (34-46 mmHg), 4: CCL4
    Style INT NOT NULL DEFAULT 1,             -- 1: Vớ gối (AD), 2: Vớ đùi (AG), 3: Quần vớ (AT), 4: Cho mẹ bầu
    ToeType INT NOT NULL DEFAULT 1,           -- 1: Hở ngón (Open Toe), 2: Kín ngón (Closed Toe)
    Material NVARCHAR(200) NULL,              -- 70% Polyamide, 30% Elastane
    Brand NVARCHAR(100) NULL,                 -- Jobst (Mỹ), Mediven (Đức), RelaxSan (Ý), Sigvaris (Thụy Sĩ)
    Origin NVARCHAR(100) NULL,                -- Quốc gia sản xuất
    UsageInstructions NVARCHAR(MAX) NULL,     -- Hướng dẫn xỏ vớ và bảo quản
    SizeGuide NVARCHAR(MAX) NULL,             -- Bảng hướng dẫn đo cB (cổ chân), cC (bắp chân)
    
    IsFeatured BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- 6. BẢNG PRODUCT_VARIANTS (Biến thể Size & Màu sắc của từng đôi vớ)
CREATE TABLE ProductVariants (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    Sku NVARCHAR(50) NOT NULL,
    Size NVARCHAR(20) NOT NULL,               -- S, M, L, XL, XXL
    Color NVARCHAR(50) NOT NULL,              -- Màu da (Beige), Màu đen (Black)
    StockQuantity INT NOT NULL DEFAULT 0,
    PriceAdjustment DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_ProductVariants_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

-- 7. BẢNG PRODUCT_IMAGES (Bộ sưu tập hình ảnh chi tiết sản phẩm)
CREATE TABLE ProductImages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    AltText NVARCHAR(200) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsPrimary BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

-- 8. BẢNG CARTS (Giỏ hàng người dùng / khách vãng lai)
CREATE TABLE Carts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    SessionId NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Carts_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- 9. BẢNG CART_ITEMS (Chi tiết các mặt hàng trong giỏ)
CREATE TABLE CartItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CartId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductVariantId INT NULL,
    Size NVARCHAR(20) NOT NULL DEFAULT 'M',
    Color NVARCHAR(50) NOT NULL DEFAULT 'Beige',
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT FK_CartItems_ProductVariants FOREIGN KEY (ProductVariantId) REFERENCES ProductVariants(Id) ON DELETE SET NULL
);

-- 10. BẢNG ORDERS (Đơn đặt hàng)
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderCode NVARCHAR(50) NOT NULL UNIQUE,
    UserId INT NULL,
    CustomerName NVARCHAR(150) NOT NULL,
    CustomerEmail NVARCHAR(150) NOT NULL,
    CustomerPhone NVARCHAR(20) NOT NULL,
    ShippingAddress NVARCHAR(300) NOT NULL,
    City NVARCHAR(100) NULL,
    District NVARCHAR(100) NULL,
    Ward NVARCHAR(100) NULL,
    Note NVARCHAR(500) NULL,
    
    PaymentMethod INT NOT NULL DEFAULT 1,     -- 1: COD, 2: Chuyển khoản, 3: VNPay, 4: Momo
    PaymentStatus INT NOT NULL DEFAULT 1,     -- 1: Chờ thanh toán, 2: Đã thanh toán, 3: Thất bại, 4: Hoàn tiền
    OrderStatus INT NOT NULL DEFAULT 1,       -- 1: Chờ xác nhận, 2: Đã xác nhận, 3: Đang đóng gói, 4: Đang giao, 5: Hoàn thành, 6: Đã hủy
    
    SubTotal DECIMAL(18,2) NOT NULL,
    ShippingFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- 11. BẢNG ORDER_DETAILS (Chi tiết đơn hàng - Lưu snapshot tại thời điểm mua)
CREATE TABLE OrderDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductVariantId INT NULL,
    ProductName NVARCHAR(250) NOT NULL,
    Sku NVARCHAR(50) NULL,
    Size NVARCHAR(20) NOT NULL,
    Color NVARCHAR(50) NOT NULL,
    CompressionLevel INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT FK_OrderDetails_ProductVariants FOREIGN KEY (ProductVariantId) REFERENCES ProductVariants(Id) ON DELETE SET NULL
);

-- 12. BẢNG PAYMENTS (Lịch sử giao dịch thanh toán)
CREATE TABLE Payments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    Method INT NOT NULL,
    TransactionId NVARCHAR(100) NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Status INT NOT NULL DEFAULT 1,
    PaidAt DATETIME2 NULL,
    ResponsePayload NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
);

-- 13. BẢNG CHATBOT_CONSULTATIONS (Nhật ký tư vấn chẩn đoán & đo size vớ của Chatbot AI)
CREATE TABLE ChatbotConsultations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SessionId NVARCHAR(100) NULL,
    CustomerName NVARCHAR(150) NULL,
    CustomerPhone NVARCHAR(20) NULL,
    SymptomsDescription NVARCHAR(MAX) NULL,
    AnkleCircumferenceCm FLOAT NULL,           -- cB (vòng cổ chân nhỏ nhất)
    CalfCircumferenceCm FLOAT NULL,            -- cC (vòng bắp chân nở nhất)
    ThighCircumferenceCm FLOAT NULL,           -- cG (vòng đùi dưới mông 5cm)
    RecommendedCompression INT NULL,           -- CCL1, CCL2, CCL3
    RecommendedSize NVARCHAR(20) NULL,         -- S, M, L, XL
    RecommendedStyle INT NULL,
    AdviceSummary NVARCHAR(MAX) NULL,
    RecommendedProductIdsJson NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- =====================================================================================
-- DỮ LIỆU KHỞI TẠO (SEED DATA)
-- =====================================================================================

-- Roles
INSERT INTO Roles (Name, Description) VALUES 
(N'Admin', N'Quản trị viên toàn quyền hệ thống'),
(N'Staff', N'Nhân viên bán hàng & tư vấn y khoa'),
(N'Customer', N'Khách hàng mua sắm');

-- Users
INSERT INTO Users (FullName, Email, PasswordHash, Phone, Address, City, District, IsActive) VALUES 
(N'Quản Trị Viên', N'admin@varicosesocks.vn', N'Admin@123', N'0901234567', N'123 Hai Bà Trưng', N'Hồ Chí Minh', N'Quận 1', 1),
(N'Chu Văn Quốc Trung', N'trung.chu@varicosesocks.vn', N'123456', N'0987654321', N'456 Lê Văn Sỹ', N'Hồ Chí Minh', N'Quận 3', 1);

-- UserRoles
INSERT INTO UserRoles (UserId, RoleId) VALUES (1, 1), (2, 3);

-- Categories
INSERT INTO Categories (Name, Slug, Description, ImageUrl, DisplayOrder, IsActive) VALUES 
(N'Vớ gối y khoa (AD)', N'vo-goi-y-khoa', N'Vớ bắp chân dành cho giãn tĩnh mạch mắt cá chân và bắp chuối', N'/images/products/vo-goi-jobst-ccl2.svg', 1, 1),
(N'Vớ đùi y khoa (AG)', N'vo-dui-y-khoa', N'Vớ đùi có viền dải silicone chống trượt, nâng đỡ toàn bộ chi dưới', N'/images/products/vo-dui-mediven-ccl2.svg', 2, 1),
(N'Quần vớ y khoa (AT)', N'quan-vo-y-khoa', N'Dáng quần tất sang trọng nâng đỡ đến thắt lưng, chống tụt vớ', N'/images/products/quan-vo-relaxsan-ccl2.svg', 3, 1),
(N'Quần vớ cho bà bầu (AT/M)', N'quan-vo-ba-bau', N'Thiết kế vùng bụng co giãn êm ái, chống phù nề và chuột rút thai kỳ', N'/images/products/quan-vo-ba-bau-ccl1.svg', 4, 1),
(N'Phụ kiện hỗ trợ mang vớ', N'phu-kien-ho-tro', N'Khung xỏ vớ y tế, găng tay chống trượt bảo vệ sợi dệt', N'/images/products/khung-xo-vo-mediven.svg', 5, 1);

-- Products
INSERT INTO Products (CategoryId, Name, Slug, Sku, ShortDescription, Description, Price, DiscountPrice, StockQuantity, ImageUrl, CompressionLevel, Style, ToeType, Material, Brand, Origin, IsFeatured, IsActive) VALUES 
(1, N'Vớ gối y khoa Jobst Relief CCL2 (Hở ngón)', N'vo-goi-jobst-relief-ccl2-ho-ngon', N'JOBST-RELIEF-CCL2-OPEN', N'Chuẩn áp lực CCL2 (20-30 mmHg) xuất xứ Mỹ, thoáng khí, điều trị phù nề chân.', N'Vớ gối y khoa Jobst Relief xuất xứ Mỹ với độ co giãn 4 chiều, hỗ trợ van tĩnh mạch đẩy máu về tim, làm nhẹ chân tức thì.', 750000, 690000, 120, N'/images/products/vo-goi-jobst-ccl2.svg', 2, 1, 1, N'74% Polyamide, 26% Elastane', N'Jobst', N'Mỹ', 1, 1),
(1, N'Vớ gối y khoa Jobst UltraSheer CCL1 (Kín ngón)', N'vo-goi-jobst-ultrasheer-ccl1-kin-ngon', N'JOBST-SHEER-CCL1-CLOSED', N'Áp lực CCL1 (15-20 mmHg), bề mặt mịn đẹp như tất da thời trang.', N'Dòng vớ y khoa thẩm mỹ cao cấp, thoáng nhẹ, thích hợp phòng ngừa suy giãn tĩnh mạch cho dân văn phòng.', 820000, 780000, 85, N'/images/products/vo-goi-jobst-ccl1.svg', 1, 1, 2, N'60% Polyamide, 40% Elastane', N'Jobst', N'Mỹ', 1, 1),
(2, N'Vớ đùi y khoa Mediven Elegance CCL2 (Silicone chống trượt)', N'vo-dui-mediven-elegance-ccl2', N'MEDI-ELEGANCE-CCL2-THIGH', N'Chuẩn y tế Đức RAL-GZ 387, áp lực CCL2 (23-32 mmHg), hạt silicone dịu nhẹ cho da.', N'Sản phẩm vớ đùi cao cấp từ Đức, điều trị tĩnh mạch nổi to vùng đùi và sau tiêm xơ, phẫu thuật Laser nội mạch.', 1250000, 1150000, 60, N'/images/products/vo-dui-mediven-ccl2.svg', 2, 2, 2, N'65% Polyamide, 35% Elastane', N'Mediven', N'Đức', 1, 1),
(2, N'Vớ đùi y khoa Sigvaris Cotton CCL2 (Siêu êm cho da nhạy cảm)', N'vo-dui-sigvaris-cotton-ccl2', N'SIGVARIS-COTTON-CCL2', N'Sợi dệt Cotton Thụy Sĩ tự nhiên, áp lực CCL2 (23-32 mmHg), thấm hút mồ hôi vượt trội.', N'Dòng vớ y khoa Sigvaris Cotton cao cấp từ Thụy Sĩ, mặt trong tiếp xúc trực tiếp với da hoàn toàn bằng sợi cotton tự nhiên tạo cảm giác thoáng mát tối đa cho khí hậu nhiệt đới Việt Nam.', 1450000, 1380000, 45, N'/images/products/vo-dui-sigvaris-ccl2.svg', 2, 2, 2, N'14% Cotton, 52% Polyamide, 34% Elastane', N'Sigvaris', N'Thụy Sĩ', 1, 1),
(3, N'Quần vớ y khoa RelaxSan Basic CCL2 (Kín ngón)', N'quan-vo-relaxsan-basic-ccl2', N'RELAXSAN-PANTY-CCL2', N'Xuất xứ Ý, bảo vệ toàn diện hệ tĩnh mạch chi dưới và vùng chậu.', N'Quần vớ y khoa RelaxSan thiết kế ôm trọn eo và hai chân, giúp đôi chân thon gọn, không sợ tuột vớ.', 980000, 890000, 50, N'/images/products/quan-vo-relaxsan-ccl2.svg', 2, 3, 2, N'70% Polyamide, 30% Elastane', N'RelaxSan', N'Ý', 1, 1),
(4, N'Quần vớ y khoa cho mẹ bầu RelaxSan Maternity CCL1', N'quan-vo-ba-bau-relaxsan-ccl1', N'RELAXSAN-MATERNITY-CCL1', N'Đai nâng bụng co giãn tùy chỉnh theo thai kỳ, chống chuột rút và nặng chân.', N'Dành riêng cho mẹ mang thai, giảm sức nặng lên tĩnh mạch chậu, phòng ngừa vỡ búi gân tĩnh mạch chân.', 850000, NULL, 40, N'/images/products/quan-vo-ba-bau-ccl1.svg', 1, 4, 2, N'75% Polyamide, 25% Elastane', N'RelaxSan', N'Ý', 0, 1),
(5, N'Khung hỗ trợ xỏ vớ y khoa Mediven Butler', N'khung-xo-vo-y-khoa-mediven-butler', N'MEDI-BUTLER-AID', N'Khung thép sơn tĩnh điện giúp xỏ vớ dễ dàng không tốn sức.', N'Khung xỏ vớ chuyên dụng giúp người cao tuổi tự mang vớ nhanh chóng, chống móng tay làm rách sợi dệt y tế.', 650000, 590000, 90, N'/images/products/khung-xo-vo-mediven.svg', 1, 1, 2, N'Thép sơn tĩnh điện', N'Mediven', N'Đức', 0, 1),
(5, N'Gel thoa dịu mát và săn chắc tĩnh mạch VenoFresh 100ml', N'gel-thoa-diu-mat-venofresh-100ml', N'VENOFRESH-GEL-100ML', N'Chiết xuất hạt dẻ ngựa và tinh dầu bạc hà, làm dịu cảm giác nặng chân, nhức mỏi ngay sau 5 phút.', N'Gel bôi VenoFresh chiết xuất từ Aescin thảo dược thiên nhiên, thẩm thấu nhanh không nhờn rít, an toàn khi kết hợp mang cùng vớ y khoa.', 320000, 280000, 150, N'/images/products/gel-venofresh-soothing.svg', 1, 1, 2, N'Aescin 2%, Menthol, Hamamelis', N'VenoFresh', N'Pháp', 1, 1);

-- Product Variants
INSERT INTO ProductVariants (ProductId, Sku, Size, Color, StockQuantity, PriceAdjustment, IsActive) VALUES 
(1, N'JOBST-RELIEF-CCL2-OPEN-S-BEIGE', N'S', N'Màu Da (Beige)', 25, 0, 1),
(1, N'JOBST-RELIEF-CCL2-OPEN-M-BEIGE', N'M', N'Màu Da (Beige)', 35, 0, 1),
(1, N'JOBST-RELIEF-CCL2-OPEN-L-BEIGE', N'L', N'Màu Da (Beige)', 30, 0, 1),
(1, N'JOBST-RELIEF-CCL2-OPEN-XL-BEIGE', N'XL', N'Màu Da (Beige)', 20, 0, 1),
(1, N'JOBST-RELIEF-CCL2-OPEN-M-BLACK', N'M', N'Màu Đen (Black)', 10, 0, 1),
(2, N'JOBST-SHEER-CCL1-CLOSED-S-BEIGE', N'S', N'Màu Da (Beige)', 20, 0, 1),
(2, N'JOBST-SHEER-CCL1-CLOSED-M-BEIGE', N'M', N'Màu Da (Beige)', 35, 0, 1),
(2, N'JOBST-SHEER-CCL1-CLOSED-L-BEIGE', N'L', N'Màu Da (Beige)', 30, 0, 1),
(3, N'MEDI-ELEGANCE-CCL2-THIGH-M-BEIGE', N'M', N'Màu Da (Beige)', 20, 0, 1),
(3, N'MEDI-ELEGANCE-CCL2-THIGH-L-BEIGE', N'L', N'Màu Da (Beige)', 20, 0, 1),
(4, N'SIGVARIS-COTTON-CCL2-M-BEIGE', N'M', N'Màu Da (Beige)', 25, 0, 1),
(5, N'RELAXSAN-PANTY-CCL2-M-BEIGE', N'M', N'Màu Da (Beige)', 25, 0, 1),
(6, N'RELAXSAN-MATERNITY-CCL1-M-BEIGE', N'M', N'Màu Da (Beige)', 20, 0, 1),
(7, N'MEDI-BUTLER-AID-FREE', N'FreeSize', N'Bạc', 90, 0, 1),
(8, N'VENOFRESH-GEL-100ML-FREE', N'100ml', N'Trắng', 150, 0, 1);

-- Sample Order
INSERT INTO Orders (OrderCode, UserId, CustomerName, CustomerEmail, CustomerPhone, ShippingAddress, City, District, Note, PaymentMethod, PaymentStatus, OrderStatus, SubTotal, ShippingFee, DiscountAmount, TotalAmount, CreatedAt)
VALUES (N'VS-20261019-8921', 2, N'Chu Văn Quốc Trung', N'trung.chu@varicosesocks.vn', N'0987654321', N'456 Lê Văn Sỹ, Phường 14', N'Hồ Chí Minh', N'Quận 3', N'Gọi trước khi giao', 1, 1, 2, 690000, 30000, 0, 720000, GETUTCDATE());

INSERT INTO OrderDetails (OrderId, ProductId, ProductVariantId, ProductName, Sku, Size, Color, CompressionLevel, UnitPrice, Quantity, TotalPrice)
VALUES (1, 1, 2, N'Vớ gối y khoa Jobst Relief CCL2 (Hở ngón)', N'JOBST-RELIEF-CCL2-OPEN-M-BEIGE', N'M', N'Màu Da (Beige)', 2, 690000, 1, 690000);
