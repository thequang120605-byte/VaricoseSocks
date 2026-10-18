-- =====================================================================================
-- DỰ ÁN: HỆ THỐNG VỚ Y KHOA ĐIỀU TRỊ SUY GIÃN TĨNH MẠCH (VARICOSE SOCKS)
-- NGUYỄN THẾ QUANG (Backend Lead) & CHU VĂN QUỐC TRUNG (Frontend / Full-stack)
-- CSDL: MySQL 8.x / MariaDB (Laragon) - UTF8MB4
-- =====================================================================================

CREATE DATABASE IF NOT EXISTS VaricoseSocksDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE VaricoseSocksDB;

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS Payments;
DROP TABLE IF EXISTS OrderDetails;
DROP TABLE IF EXISTS Orders;
DROP TABLE IF EXISTS CartItems;
DROP TABLE IF EXISTS Carts;
DROP TABLE IF EXISTS ProductImages;
DROP TABLE IF EXISTS InventoryTransactions;
DROP TABLE IF EXISTS ProductVariants;
DROP TABLE IF EXISTS Products;
DROP TABLE IF EXISTS Categories;
DROP TABLE IF EXISTS UserRoles;
DROP TABLE IF EXISTS Roles;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS ChatbotConsultations;
DROP TABLE IF EXISTS Promotions;
SET FOREIGN_KEY_CHECKS = 1;

-- 1. ROLES
CREATE TABLE Roles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. USERS
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FullName VARCHAR(150) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Phone VARCHAR(20) NULL,
    Address VARCHAR(300) NULL,
    City VARCHAR(100) NULL,
    District VARCHAR(100) NULL,
    AvatarUrl VARCHAR(500) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. USER_ROLES
CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4. CATEGORIES
CREATE TABLE Categories (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    Slug VARCHAR(150) NOT NULL UNIQUE,
    Description TEXT NULL,
    ImageUrl VARCHAR(500) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 5. PRODUCTS
CREATE TABLE Products (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CategoryId INT NOT NULL,
    Name VARCHAR(250) NOT NULL,
    Slug VARCHAR(250) NOT NULL,
    Sku VARCHAR(50) NOT NULL UNIQUE,
    ShortDescription TEXT NULL,
    Description LONGTEXT NULL,
    Price DECIMAL(18,2) NOT NULL,
    DiscountPrice DECIMAL(18,2) NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    ImageUrl VARCHAR(500) NULL,
    CompressionLevel INT NOT NULL DEFAULT 1,
    Style INT NOT NULL DEFAULT 1,
    ToeType INT NOT NULL DEFAULT 1,
    Material VARCHAR(200) NULL,
    Brand VARCHAR(100) NULL,
    Origin VARCHAR(100) NULL,
    UsageInstructions LONGTEXT NULL,
    SizeGuide LONGTEXT NULL,
    IsFeatured TINYINT(1) NOT NULL DEFAULT 0,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 6. PRODUCT_VARIANTS
CREATE TABLE ProductVariants (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    Sku VARCHAR(50) NOT NULL,
    Size VARCHAR(20) NOT NULL,
    Color VARCHAR(50) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    PriceAdjustment DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CONSTRAINT FK_ProductVariants_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 7. PRODUCT_IMAGES
CREATE TABLE ProductImages (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    ImageUrl VARCHAR(500) NOT NULL,
    AltText VARCHAR(200) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsPrimary TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 8. CARTS
CREATE TABLE Carts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NULL,
    SessionId VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Carts_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 9. CART_ITEMS
CREATE TABLE CartItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CartId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductVariantId INT NULL,
    Size VARCHAR(20) NOT NULL DEFAULT 'M',
    Color VARCHAR(50) NOT NULL DEFAULT 'Beige',
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT FK_CartItems_ProductVariants FOREIGN KEY (ProductVariantId) REFERENCES ProductVariants(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 10. ORDERS
CREATE TABLE Orders (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderCode VARCHAR(50) NOT NULL UNIQUE,
    UserId INT NULL,
    CustomerName VARCHAR(150) NOT NULL,
    CustomerEmail VARCHAR(150) NOT NULL,
    CustomerPhone VARCHAR(20) NOT NULL,
    ShippingAddress VARCHAR(300) NOT NULL,
    City VARCHAR(100) NULL,
    District VARCHAR(100) NULL,
    Ward VARCHAR(100) NULL,
    Note TEXT NULL,
    PaymentMethod INT NOT NULL DEFAULT 1,
    PaymentStatus INT NOT NULL DEFAULT 1,
    OrderStatus INT NOT NULL DEFAULT 1,
    SubTotal DECIMAL(18,2) NOT NULL,
    ShippingFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 11. ORDER_DETAILS
CREATE TABLE OrderDetails (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductVariantId INT NULL,
    ProductName VARCHAR(250) NOT NULL,
    Sku VARCHAR(50) NULL,
    Size VARCHAR(20) NOT NULL,
    Color VARCHAR(50) NOT NULL,
    CompressionLevel INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT FK_OrderDetails_ProductVariants FOREIGN KEY (ProductVariantId) REFERENCES ProductVariants(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 12. PAYMENTS
CREATE TABLE Payments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderId INT NOT NULL,
    Method INT NOT NULL,
    TransactionId VARCHAR(100) NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Status INT NOT NULL DEFAULT 1,
    PaidAt DATETIME NULL,
    ResponsePayload TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 13. PROMOTIONS
CREATE TABLE Promotions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Code VARCHAR(50) NOT NULL UNIQUE,
    Name VARCHAR(150) NOT NULL,
    Description TEXT NULL,
    DiscountType INT NOT NULL DEFAULT 1,
    DiscountValue DECIMAL(18,2) NOT NULL,
    MinOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaxDiscountAmount DECIMAL(18,2) NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    UsageLimit INT NOT NULL DEFAULT 100,
    UsedCount INT NOT NULL DEFAULT 0,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 14. INVENTORY_TRANSACTIONS
CREATE TABLE InventoryTransactions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    ProductVariantId INT NULL,
    Type INT NOT NULL DEFAULT 1,
    Quantity INT NOT NULL,
    UnitCost DECIMAL(18,2) NULL,
    Note TEXT NULL,
    PerformedBy VARCHAR(150) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Inventory_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 15. CHATBOT_CONSULTATIONS
CREATE TABLE ChatbotConsultations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SessionId VARCHAR(100) NULL,
    CustomerName VARCHAR(150) NULL,
    CustomerPhone VARCHAR(20) NULL,
    SymptomsDescription TEXT NULL,
    AnkleCircumferenceCm DOUBLE NULL,
    CalfCircumferenceCm DOUBLE NULL,
    ThighCircumferenceCm DOUBLE NULL,
    RecommendedCompression INT NULL,
    RecommendedSize VARCHAR(20) NULL,
    RecommendedStyle INT NULL,
    AdviceSummary TEXT NULL,
    RecommendedProductIdsJson TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================================================
-- SEED DATA
-- =====================================================================================

INSERT INTO Roles (Name, Description) VALUES 
('Admin', 'Quản trị viên toàn quyền hệ thống'),
('Staff', 'Nhân viên bán hàng & tư vấn y khoa'),
('Customer', 'Khách hàng mua sắm');

INSERT INTO Users (FullName, Email, PasswordHash, Phone, Address, City, District, IsActive) VALUES 
('Quản Trị Viên', 'admin@varicosesocks.vn', 'Admin@123', '0901234567', '123 Hai Bà Trưng', 'Hồ Chí Minh', 'Quận 1', 1),
('Nguyễn Thế Quang', 'quang.nguyen@varicosesocks.vn', 'Quang@123', '0912345678', '789 Điện Biên Phủ', 'Hồ Chí Minh', 'Bình Thạnh', 1),
('Chu Văn Quốc Trung', 'trung.chu@varicosesocks.vn', '123456', '0987654321', '456 Lê Văn Sỹ', 'Hồ Chí Minh', 'Quận 3', 1);

INSERT INTO UserRoles (UserId, RoleId) VALUES (1, 1), (2, 1), (3, 3);

INSERT INTO Categories (Name, Slug, Description, ImageUrl, DisplayOrder, IsActive) VALUES 
('Vớ gối y khoa (AD)', 'vo-goi-y-khoa', 'Vớ bắp chân dành cho giãn tĩnh mạch mắt cá chân và bắp chuối', '/images/products/vo-goi-jobst-ccl2.svg', 1, 1),
('Vớ đùi y khoa (AG)', 'vo-dui-y-khoa', 'Vớ đùi có viền dải silicone chống trượt, nâng đỡ toàn bộ chi dưới', '/images/products/vo-dui-mediven-ccl2.svg', 2, 1),
('Quần vớ y khoa (AT)', 'quan-vo-y-khoa', 'Dáng quần tất sang trọng nâng đỡ đến thắt lưng, chống tụt vớ', '/images/products/quan-vo-relaxsan-ccl2.svg', 3, 1),
('Quần vớ cho bà bầu (AT/M)', 'quan-vo-ba-bau', 'Thiết kế vùng bụng co giãn êm ái, chống phù nề và chuột rút thai kỳ', '/images/products/quan-vo-ba-bau-ccl1.svg', 4, 1),
('Phụ kiện hỗ trợ mang vớ', 'phu-kien-ho-tro', 'Khung xỏ vớ y tế, găng tay chống trượt bảo vệ sợi dệt', '/images/products/khung-xo-vo-mediven.svg', 5, 1);

INSERT INTO Products (CategoryId, Name, Slug, Sku, ShortDescription, Description, Price, DiscountPrice, StockQuantity, ImageUrl, CompressionLevel, Style, ToeType, Material, Brand, Origin, IsFeatured, IsActive) VALUES 
(1, 'Vớ gối y khoa Jobst Relief CCL2 (Hở ngón)', 'vo-goi-jobst-relief-ccl2-ho-ngon', 'JOBST-RELIEF-CCL2-OPEN', 'Chuẩn áp lực CCL2 (20-30 mmHg) xuất xứ Mỹ, thoáng khí, điều trị phù nề chân.', 'Vớ gối y khoa Jobst Relief xuất xứ Mỹ với độ co giãn 4 chiều, hỗ trợ van tĩnh mạch đẩy máu về tim, làm nhẹ chân tức thì.', 750000, 690000, 120, '/images/products/vo-goi-jobst-ccl2.svg', 2, 1, 1, '74% Polyamide, 26% Elastane', 'Jobst', 'Mỹ', 1, 1),
(1, 'Vớ gối y khoa Jobst UltraSheer CCL1 (Kín ngón)', 'vo-goi-jobst-ultrasheer-ccl1-kin-ngon', 'JOBST-SHEER-CCL1-CLOSED', 'Áp lực CCL1 (15-20 mmHg), bề mặt mịn đẹp như tất da thời trang.', 'Dòng vớ y khoa thẩm mỹ cao cấp, thoáng nhẹ, thích hợp phòng ngừa suy giãn tĩnh mạch cho dân văn phòng.', 820000, 780000, 85, '/images/products/vo-goi-jobst-ccl1.svg', 1, 1, 2, '60% Polyamide, 40% Elastane', 'Jobst', 'Mỹ', 1, 1),
(2, 'Vớ đùi y khoa Mediven Elegance CCL2 (Silicone chống trượt)', 'vo-dui-mediven-elegance-ccl2', 'MEDI-ELEGANCE-CCL2-THIGH', 'Chuẩn y tế Đức RAL-GZ 387, áp lực CCL2 (23-32 mmHg), hạt silicone dịu nhẹ cho da.', 'Sản phẩm vớ đùi cao cấp từ Đức, điều trị tĩnh mạch nổi to vùng đùi và sau tiêm xơ, phẫu thuật Laser nội mạch.', 1250000, 1150000, 60, '/images/products/vo-dui-mediven-ccl2.svg', 2, 2, 2, '65% Polyamide, 35% Elastane', 'Mediven', 'Đức', 1, 1),
(2, 'Vớ đùi y khoa Sigvaris Cotton CCL2 (Siêu êm cho da nhạy cảm)', 'vo-dui-sigvaris-cotton-ccl2', 'SIGVARIS-COTTON-CCL2', 'Sợi dệt Cotton Thụy Sĩ tự nhiên, áp lực CCL2 (23-32 mmHg), thấm hút mồ hôi vượt trội.', 'Dòng vớ y khoa Sigvaris Cotton cao cấp từ Thụy Sĩ, mặt trong tiếp xúc trực tiếp với da hoàn toàn bằng sợi cotton tự nhiên tạo cảm giác thoáng mát tối đa cho khí hậu nhiệt đới Việt Nam.', 1450000, 1380000, 45, '/images/products/vo-dui-sigvaris-ccl2.svg', 2, 2, 2, '14% Cotton, 52% Polyamide, 34% Elastane', 'Sigvaris', 'Thụy Sĩ', 1, 1),
(3, 'Quần vớ y khoa RelaxSan Basic CCL2 (Kín ngón)', 'quan-vo-relaxsan-basic-ccl2', 'RELAXSAN-PANTY-CCL2', 'Xuất xứ Ý, bảo vệ toàn diện hệ tĩnh mạch chi dưới và vùng chậu.', 'Quần vớ y khoa RelaxSan thiết kế ôm trọn eo và hai chân, giúp đôi chân thon gọn, không sợ tuột vớ.', 980000, 890000, 50, '/images/products/quan-vo-relaxsan-ccl2.svg', 2, 3, 2, '70% Polyamide, 30% Elastane', 'RelaxSan', 'Ý', 1, 1),
(4, 'Quần vớ y khoa cho mẹ bầu RelaxSan Maternity CCL1', 'quan-vo-ba-bau-relaxsan-ccl1', 'RELAXSAN-MATERNITY-CCL1', 'Đai nâng bụng co giãn tùy chỉnh theo thai kỳ, chống chuột rút và nặng chân.', 'Dành riêng cho mẹ mang thai, giảm sức nặng lên tĩnh mạch chậu, phòng ngừa vỡ búi gân tĩnh mạch chân.', 850000, NULL, 40, '/images/products/quan-vo-ba-bau-ccl1.svg', 1, 4, 2, '75% Polyamide, 25% Elastane', 'RelaxSan', 'Ý', 0, 1),
(5, 'Khung hỗ trợ xỏ vớ y khoa Mediven Butler', 'khung-xo-vo-y-khoa-mediven-butler', 'MEDI-BUTLER-AID', 'Khung thép sơn tĩnh điện giúp xỏ vớ dễ dàng không tốn sức.', 'Khung xỏ vớ chuyên dụng giúp người cao tuổi tự mang vớ nhanh chóng, chống móng tay làm rách sợi dệt y tế.', 650000, 590000, 90, '/images/products/khung-xo-vo-mediven.svg', 1, 1, 2, 'Thép sơn tĩnh điện', 'Mediven', 'Đức', 0, 1),
(5, 'Gel thoa dịu mát và săn chắc tĩnh mạch VenoFresh 100ml', 'gel-thoa-diu-mat-venofresh-100ml', 'VENOFRESH-GEL-100ML', 'Chiết xuất hạt dẻ ngựa và tinh dầu bạc hà, làm dịu cảm giác nặng chân, nhức mỏi ngay sau 5 phút.', 'Gel bôi VenoFresh chiết xuất từ Aescin thảo dược thiên nhiên, thẩm thấu nhanh không nhờn rít, an toàn khi kết hợp mang cùng vớ y khoa.', 320000, 280000, 150, '/images/products/gel-venofresh-soothing.svg', 1, 1, 2, 'Aescin 2%, Menthol, Hamamelis', 'VenoFresh', 'Pháp', 1, 1);

INSERT INTO ProductVariants (ProductId, Sku, Size, Color, StockQuantity, PriceAdjustment, IsActive) VALUES 
(1, 'JOBST-RELIEF-CCL2-OPEN-S-BEIGE', 'S', 'Màu Da (Beige)', 25, 0, 1),
(1, 'JOBST-RELIEF-CCL2-OPEN-M-BEIGE', 'M', 'Màu Da (Beige)', 35, 0, 1),
(1, 'JOBST-RELIEF-CCL2-OPEN-L-BEIGE', 'L', 'Màu Da (Beige)', 30, 0, 1),
(1, 'JOBST-RELIEF-CCL2-OPEN-XL-BEIGE', 'XL', 'Màu Da (Beige)', 20, 0, 1),
(1, 'JOBST-RELIEF-CCL2-OPEN-M-BLACK', 'M', 'Màu Đen (Black)', 10, 0, 1),
(2, 'JOBST-SHEER-CCL1-CLOSED-S-BEIGE', 'S', 'Màu Da (Beige)', 20, 0, 1),
(2, 'JOBST-SHEER-CCL1-CLOSED-M-BEIGE', 'M', 'Màu Da (Beige)', 35, 0, 1),
(2, 'JOBST-SHEER-CCL1-CLOSED-L-BEIGE', 'L', 'Màu Da (Beige)', 30, 0, 1),
(3, 'MEDI-ELEGANCE-CCL2-THIGH-M-BEIGE', 'M', 'Màu Da (Beige)', 20, 0, 1),
(3, 'MEDI-ELEGANCE-CCL2-THIGH-L-BEIGE', 'L', 'Màu Da (Beige)', 20, 0, 1),
(4, 'SIGVARIS-COTTON-CCL2-M-BEIGE', 'M', 'Màu Da (Beige)', 25, 0, 1),
(5, 'RELAXSAN-PANTY-CCL2-M-BEIGE', 'M', 'Màu Da (Beige)', 25, 0, 1),
(6, 'RELAXSAN-MATERNITY-CCL1-M-BEIGE', 'M', 'Màu Da (Beige)', 20, 0, 1),
(7, 'MEDI-BUTLER-AID-FREE', 'FreeSize', 'Bạc', 90, 0, 1),
(8, 'VENOFRESH-GEL-100ML-FREE', '100ml', 'Trắng', 150, 0, 1);

INSERT INTO Orders (OrderCode, UserId, CustomerName, CustomerEmail, CustomerPhone, ShippingAddress, City, District, Note, PaymentMethod, PaymentStatus, OrderStatus, SubTotal, ShippingFee, DiscountAmount, TotalAmount, CreatedAt)
VALUES ('VS-20261019-8921', 3, 'Chu Văn Quốc Trung', 'trung.chu@varicosesocks.vn', '0987654321', '456 Lê Văn Sỹ, Phường 14', 'Hồ Chí Minh', 'Quận 3', 'Gọi trước khi giao', 1, 1, 2, 690000, 30000, 0, 720000, NOW());

INSERT INTO OrderDetails (OrderId, ProductId, ProductVariantId, ProductName, Sku, Size, Color, CompressionLevel, UnitPrice, Quantity, TotalPrice)
VALUES (1, 1, 2, 'Vớ gối y khoa Jobst Relief CCL2 (Hở ngón)', 'JOBST-RELIEF-CCL2-OPEN-M-BEIGE', 'M', 'Màu Da (Beige)', 2, 690000, 1, 690000);

INSERT INTO Promotions (Code, Name, Description, DiscountType, DiscountValue, MinOrderAmount, MaxDiscountAmount, StartDate, EndDate, UsageLimit, UsedCount, IsActive) VALUES 
('VARICOSE10', 'Giảm 10% đơn hàng vớ y khoa', 'Áp dụng cho mọi đơn từ 500k', 1, 10, 500000, 100000, NOW(), DATE_ADD(NOW(), INTERVAL 30 DAY), 100, 0, 1),
('FREESHIP', 'Miễn phí vận chuyển toàn quốc', 'Đơn từ 300k', 2, 30000, 300000, NULL, NOW(), DATE_ADD(NOW(), INTERVAL 30 DAY), 200, 0, 1),
('YTE50K', 'Hỗ trợ bệnh nhân suy tĩnh mạch 50k', 'Đơn từ 700k', 2, 50000, 700000, NULL, NOW(), DATE_ADD(NOW(), INTERVAL 30 DAY), 50, 0, 1);
