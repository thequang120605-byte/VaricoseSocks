# TÀI LIỆU KIẾN TRÚC & HƯỚNG DẪN DỰ ÁN VARICOSE SOCKS (.NET 9 CORE)

**Dự án:** Xây dựng Hệ thống Website & REST API bán vớ y khoa điều trị suy giãn tĩnh mạch (Web & Mobile Responsive)  
**Nhóm sinh viên thực hiện:**
- **Nguyễn Thế Quang** — Backend Lead (ASP.NET Core REST API, EF Core, SQL Server/MySQL, Auth, Payment, Inventory, Promotion, Admin API).
- **Chu Văn Quốc Trung** — Frontend Lead (Web UI MVC, Responsive Mobile, Giỏ hàng, Đặt hàng, Tra cứu đơn hàng, Admin UI, Chatbot AI).

---

## 1. GIẢI THÍCH LỖI `localhost:5046` BỊ 404 VÀ CÁCH KHẮC PHỤC

Trong ảnh chụp màn hình bạn gửi, trình duyệt báo lỗi `Không thể tìm thấy trang localhost này - HTTP ERROR 404` tại địa chỉ:
`http://localhost:5046/`

### Nguyên nhân:
1. **Cổng 5046** là cổng của **`VaricoseSocks.API`** (Backend REST API), không phải là cổng của giao diện Web mua sắm!
2. Trước đây, API chỉ có các route `/api/...` mà không có route mặc định `/`, dẫn đến khi mở trình duyệt vào `localhost:5046/` thì máy chủ báo lỗi `404 Not Found`.
3. **Giao diện mua sắm (Frontend Web UI)** nằm ở dự án **`VaricoseSocks.Web`** và chạy ở cổng:
   👉 **`http://localhost:5118`**

### Cách khắc phục đã xử lý:
1. Đã bổ sung trang **API Dashboard** trực tiếp tại `http://localhost:5046/`, khi truy cập vào sẽ có nút bấm chuyển nhanh sang Web UI và xem toàn bộ endpoint API.
2. Tạo script chạy tự động 1-click **`run_project.bat`** hoặc **`run_project.ps1`** để khởi động đồng thời cả Backend API và Frontend Web UI và tự động mở trình duyệt.

---

## 2. PHÂN CÔNG & DANH MỤC CÔNG VIỆC CỦA NGUYỄN THẾ QUANG (BACKEND LEAD)

| Hạng mục Backend | Controller & API Endpoint | Mô tả chi tiết |
|---|---|---|
| **ASP.NET Core & Architecture** | `Program.cs`, Clean Architecture | Cấu hình DI, CORS, Middleware, OpenAPI Swagger, tự động nạp dữ liệu mẫu ban đầu. |
| **SQL Server & EF Core** | `ApplicationDbContext.cs` | 15 bảng cơ sở dữ liệu quan hệ, Fluent API, cấu hình khóa ngoại, indexes. |
| **Authentication & RBAC** | `api/auth/login`, `register`, `user/{id}` | Xác thực tài khoản, mã hóa mật khẩu, phân quyền Role `Admin`, `Staff`, `Customer`. |
| **Product API** | `api/products`, `api/products/slug/{slug}` | CRUD vớ y khoa, lọc theo cấp độ nén CCL1/CCL2/CCL3, lọc kiểu dáng, biến thể size S/M/L/XL. |
| **Category API** | `api/categories` | Quản lý danh mục vớ gối, vớ đùi viền silicone, quần vớ, vớ mẹ bầu. |
| **Cart API** | `api/cart`, `api/cart/items` | Thêm vào giỏ, cập nhật số lượng, xóa mặt hàng, làm trống giỏ hàng theo SessionId hoặc UserId. |
| **Order API** | `api/orders`, `api/orders/code/{code}` | Tạo đơn hàng mới, tra cứu mã đơn `VS-yyyyMMdd-xxxx`, cập nhật trạng thái đơn hàng. |
| **Customer API** | `api/customers` | Danh sách khách hàng, lịch sử mua hàng, tổng chi tiêu cho quản trị viên. |
| **Promotion API** | `api/promotions`, `api/promotions/validate` | Quản lý mã voucher (`VARICOSE10`, `FREESHIP`), kiểm tra điều kiện áp dụng giảm giá. |
| **Inventory API** | `api/inventory/status`, `import`, `transactions` | Kiểm tra tồn kho theo size/màu, nhập kho hàng loạt từ Đức/Mỹ, nhật ký xuất nhập tồn. |
| **Payment Integration** | `api/payments/vietqr`, `vnpay/create-url`, `callback` | Tích hợp sinh mã VietQR tự động theo chuẩn Napas và cổng thanh toán VNPay Sandbox. |
| **Admin API** | `api/admin/dashboard`, `reports/revenue` | Thống kê tổng doanh thu, số lượng đơn hàng, sản phẩm sắp hết hàng, báo cáo biểu đồ. |

---

## 3. THIẾT KẾ CƠ SỞ DỮ LIỆU ĐẦY ĐỦ (15 BẢNG ĐÃ TẠO TRONG MYSQL & SQL SERVER)

File kịch bản hoàn chỉnh:
- **Cho MySQL / Laragon:** [`Database_MySQL_Import.sql`](file:///d:/Desktop/Th%E1%BB%B1c%20t%E1%BA%ADp/Database_MySQL_Import.sql) *(Đã import thành công vào Laragon MySQL)*
- **Cho SQL Server:** [`Database_Full_Schema_And_Data.sql`](file:///d:/Desktop/Th%E1%BB%B1c%20t%E1%BA%ADp/Database_Full_Schema_And_Data.sql)

```
1. Roles                    9. CartItems
2. Users                   10. Orders
3. UserRoles               11. OrderDetails
4. Categories              12. Payments
5. Products                13. Promotions
6. ProductVariants         14. InventoryTransactions
7. ProductImages           15. ChatbotConsultations
8. Carts
```

---

## 4. HƯỚNG DẪN CHẠY GIAO DIỆN (CHỈ 1 THAO TÁC)

### Cách nhanh nhất:
Vào thư mục `d:\Desktop\Thực tập` và nhấp đúp file:
👉 **`run_project.bat`**

Hệ thống sẽ:
1. Bật Backend API tại `http://localhost:5046`
2. Bật Giao diện Web UI tại `http://localhost:5118`
3. Tự động mở Google Chrome vào trang web mua sắm `http://localhost:5118`!

### Hoặc chạy qua PowerShell / Command Prompt:
```powershell
# Chạy Frontend Web (Xem giao diện người dùng):
dotnet run --project VaricoseSocks.Web

# Chạy Backend API:
dotnet run --project VaricoseSocks.API
```
