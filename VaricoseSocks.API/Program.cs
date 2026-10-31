using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Application.Services;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Infrastructure.Data;
using VaricoseSocks.Infrastructure.Repositories;
using VaricoseSocks.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Cấu hình CORS để Web UI và Mobile Client kết nối mượt mà
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Cấu hình kết nối Database (Hỗ trợ MySQL Laragon và SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connectionString != null && connectionString.Contains("Port=3306"))
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
    else
    {
        options.UseSqlServer(connectionString ?? "Server=localhost;Database=VaricoseSocksDB;Trusted_Connection=True;TrustServerCertificate=True;");
    }
});

// Cấu hình Password Hasher (PBKDF2 HMAC-SHA256)
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Cấu hình JWT Service & Authentication
builder.Services.AddScoped<IJwtService, JwtService>();

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["JwtSettings:Secret"]
    ?? "VaricoseSocks_Medical_Secure_Secret_Key_2026_Minimum_32_Bytes_Long_JWT_Secret!";

if (builder.Environment.IsProduction())
{
    var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
    if (string.IsNullOrWhiteSpace(envSecret) || envSecret.Contains("Default", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("BẢO MẬT: Trong môi trường Production, khóa bí mật JWT bắt buộc phải được thiết lập qua biến môi trường 'JWT_SECRET' và không được sử dụng giá trị mặc định!");
    }
    jwtSecret = envSecret;
}

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["JwtSettings:Issuer"] ?? "VaricoseSocks.API";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["JwtSettings:Audience"] ?? "VaricoseSocks.Clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Đăng ký Dependency Injection cho toàn bộ Repositories và Application Services
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatbotRepository, ChatbotRepository>();

builder.Services.AddScoped<IChatbotAiService, ChatbotAiService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IHealthRecordService, HealthRecordService>();

var app = builder.Build();

// Tự động khởi tạo và nạp dữ liệu mẫu ban đầu nếu kết nối DB thành công
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning("Khởi tạo DB / Migration: " + ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Trang chủ API hiển thị trực tiếp khi truy cập localhost:5046
app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='utf-8'/>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
    <title>Varicose Socks Medical REST API</title>
    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css'/>
    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css'>
</head>
<body class='bg-light py-5'>
    <div class='container' style='max-width: 860px;'>
        <div class='card border-0 shadow-lg rounded-4 p-4 p-md-5 bg-white'>
            <div class='d-flex align-items-center mb-4'>
                <div class='bg-primary text-white rounded-circle p-3 me-3'>
                    <i class='bi bi-heart-pulse-fill fs-2'></i>
                </div>
                <div>
                    <h2 class='fw-bold text-primary mb-1'>VARICOSE SOCKS MEDICAL - BACKEND REST API</h2>
                    <p class='text-muted mb-0'>Hệ thống API Vớ Y Khoa & Thương Mại Điện Tử .NET 9 Core</p>
                    <small class='badge bg-success-subtle text-success border border-success'>Trạng thái: Máy chủ API đang hoạt động ổn định (Cổng 5046) - Bảo mật JWT & PBKDF2</small>
                </div>
            </div>

            <div class='alert alert-warning border-warning p-3 rounded-3 mb-4'>
                <h6 class='fw-bold mb-1'><i class='bi bi-info-circle-fill me-1'></i> Bạn đang truy cập cổng của Backend API (5046)!</h6>
                <p class='mb-2 small'>Để xem và trải nghiệm <strong>Giao diện mua sắm website (Frontend Web UI)</strong> của bạn Chu Văn Quốc Trung, hãy bấm nút dưới đây:</p>
                <a href='http://localhost:5118' target='_blank' class='btn btn-warning fw-bold text-dark px-4'>
                    <i class='bi bi-browser-chrome me-2'></i> Mở Giao Diện Web UI (http://localhost:5118)
                </a>
            </div>

            <h5 class='fw-bold text-dark mb-3'><i class='bi bi-gear-wide-connected text-primary me-2'></i>Danh sách Endpoint API (Phụ trách: Nguyễn Thế Quang - Backend Lead)</h5>
            <div class='row row-cols-1 row-cols-md-2 g-2 small'>
                <div class='col'><a href='/api/products' target='_blank' class='list-group-item list-group-item-action p-2 border rounded'><span class='badge bg-primary me-2'>GET</span> /api/products (Danh sách vớ y khoa)</a></div>
                <div class='col'><a href='/api/categories' target='_blank' class='list-group-item list-group-item-action p-2 border rounded'><span class='badge bg-primary me-2'>GET</span> /api/categories (Danh mục vớ)</a></div>
                <div class='col'><a href='/api/promotions/active' target='_blank' class='list-group-item list-group-item-action p-2 border rounded'><span class='badge bg-primary me-2'>GET</span> /api/promotions/active (Voucher/Giảm giá)</a></div>
                <div class='col'><a href='/api/inventory/status' target='_blank' class='list-group-item list-group-item-action p-2 border rounded'><span class='badge bg-primary me-2'>GET</span> /api/inventory/status (Quản lý tồn kho)</a></div>
                <div class='col'><a href='/api/customers' target='_blank' class='list-group-item list-group-item-action p-2 border rounded'><span class='badge bg-primary me-2'>GET</span> /api/customers (Quản lý khách hàng)</a></div>
                <div class='col'><a href='/api/orders' target='_blank' class='list-group-item list-group-item-action p-2 border rounded'><span class='badge bg-primary me-2'>GET</span> /api/orders (Đơn đặt hàng)</a></div>
                <div class='col'><a href='/api/payments/methods' target='_blank' class='list-group-item list-group-item-action p-2 border rounded'><span class='badge bg-primary me-2'>GET</span> /api/payments/methods (Thanh toán VietQR/VNPay)</a></div>
                <div class='col'><a href='/api/admin/dashboard' target='_blank' class='list-group-item list-group-item-action p-2 border rounded'><span class='badge bg-primary me-2'>GET</span> /api/admin/dashboard (Thống kê quản trị)</a></div>
                <div class='col'><a href='/api/chatbot/calculate-size?ankleCm=22' target='_blank' class='list-group-item list-group-item-action p-2 border rounded'><span class='badge bg-primary me-2'>GET</span> /api/chatbot/calculate-size (Tính size AI)</a></div>
                <div class='col'><a href='/openapi/v1.json' target='_blank' class='list-group-item list-group-item-action p-2 border rounded'><span class='badge bg-secondary me-2'>JSON</span> /openapi/v1.json (OpenAPI Docs)</a></div>
            </div>

            <div class='text-center mt-4 pt-3 border-top text-muted small'>
                &copy; 2026 VaricoseSocks Medical - ASP.NET Core 9 / Clean Architecture
            </div>
        </div>
    </div>
</body>
</html>
", "text/html"));

// Định tuyến request tới các Controller
app.MapControllers();

app.Run();
