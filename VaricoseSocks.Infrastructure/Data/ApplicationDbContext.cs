using System;
using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Domain.Entities;

namespace VaricoseSocks.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình ràng buộc giá trị tiền tệ cho Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.DiscountPrice).HasPrecision(18, 2);
            
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Unique index cho Email User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Seed data (will be added to migrations)
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Tất hàng ngày", Description = "Tất thoải mái cho sử dụng hàng ngày", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = 2, Name = "Tất nén", Description = "Tất hỗ trợ nén, tốt cho tuần hoàn và giảm sưng", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = 3, Name = "Tất thể thao", Description = "Tất hiệu suất cao dành cho hoạt động thể thao", IsActive = true, CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, CategoryId = 1, Name = "Tất cotton cổ điển", Description = "Tất cotton mềm, nhiều màu", Price = 59000m, DiscountPrice = null, StockQuantity = 150, ImageUrl = "", Size = "M", Color = "Đen", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 2, CategoryId = 1, Name = "Tất cổ thể thao", Description = "Tất cổ thấp thoáng khí cho thể thao", Price = 69900m, DiscountPrice = null, StockQuantity = 120, ImageUrl = "", Size = "L", Color = "Trắng", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 3, CategoryId = 2, Name = "Tất nén nhẹ 15-20 mmHg", Description = "Tất nén nhẹ dùng hàng ngày, phù hợp khi đi máy bay hoặc đứng nhiều", Price = 125000m, DiscountPrice = null, StockQuantity = 80, ImageUrl = "", Size = "M", Color = "Xám", CompressionLevel = "15-20 mmHg", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 4, CategoryId = 3, Name = "Tất chạy bộ hiệu suất", Description = "Tất thoát ẩm cho người chạy bộ", Price = 89900m, DiscountPrice = 74900m, StockQuantity = 200, ImageUrl = "", Size = "M", Color = "Đen", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 5, CategoryId = 3, Name = "Tất đệm cho tập luyện", Description = "Đệm thêm cho tập gym và luyện tập", Price = 95000m, DiscountPrice = null, StockQuantity = 140, ImageUrl = "", Size = "L", Color = "Xanh đậm", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 6, CategoryId = 1, Name = "Tất không lộ (No-Show)", Description = "Tất ẩn cho giày casual", Price = 49900m, DiscountPrice = null, StockQuantity = 180, ImageUrl = "", Size = "S", Color = "Beige", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 7, CategoryId = 1, Name = "Tất giữ nhiệt mùa đông", Description = "Tất giữ nhiệt ấm cho mùa lạnh", Price = 110000m, DiscountPrice = 90000m, StockQuantity = 60, ImageUrl = "", Size = "XL", Color = "Xám đậm", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 8, CategoryId = 2, Name = "Tất y tế 20-30 mmHg", Description = "Tất nén mạnh hơn cho mục đích y tế", Price = 187500m, DiscountPrice = null, StockQuantity = 50, ImageUrl = "", Size = "M", Color = "Đen", CompressionLevel = "20-30 mmHg", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 9, CategoryId = 3, Name = "Tất cổ thấp cho thể thao", Description = "Tất cổ thấp cho các môn thể thao trên sân", Price = 72500m, DiscountPrice = null, StockQuantity = 130, ImageUrl = "", Size = "M", Color = "Trắng", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 10, CategoryId = 1, Name = "Tất tre (Bamboo)", Description = "Tất thân thiện môi trường từ sợi tre, mềm và thoáng", Price = 64900m, DiscountPrice = 54900m, StockQuantity = 90, ImageUrl = "", Size = "M", Color = "Nâu", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 11, CategoryId = 1, Name = "Tất kẻ sọc", Description = "Tất kẻ sọc dùng hằng ngày", Price = 54900m, DiscountPrice = null, StockQuantity = 110, ImageUrl = "", Size = "M", Color = "Xanh", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 12, CategoryId = 1, Name = "Tất cổ dài cotton", Description = "Tất cổ dài cổ điển bằng cotton", Price = 57500m, DiscountPrice = null, StockQuantity = 160, ImageUrl = "", Size = "L", Color = "Xám", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 13, CategoryId = 2, Name = "Tất nén phân đoạn 15-20 mmHg", Description = "Tất nén phân đoạn thoải mái cho cả ngày", Price = 140000m, DiscountPrice = 120000m, StockQuantity = 95, ImageUrl = "", Size = "M", Color = "Beige", CompressionLevel = "15-20 mmHg", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 14, CategoryId = 2, Name = "Tất nén thể thao 20-30 mmHg", Description = "Hỗ trợ phục hồi sau tập luyện", Price = 165000m, DiscountPrice = null, StockQuantity = 70, ImageUrl = "", Size = "L", Color = "Đen", CompressionLevel = "20-30 mmHg", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 15, CategoryId = 3, Name = "Tất chạy địa hình", Description = "Tất bền cho chạy địa hình", Price = 109900m, DiscountPrice = null, StockQuantity = 85, ImageUrl = "", Size = "M", Color = "Xanh lá", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 16, CategoryId = 3, Name = "Tất xe đạp thoáng khí", Description = "Tất nhẹ cho đạp xe", Price = 82500m, DiscountPrice = null, StockQuantity = 120, ImageUrl = "", Size = "S", Color = "Trắng", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 17, CategoryId = 1, Name = "Bộ 3 tất cổ thấp", Description = "Bộ 3 tất cổ thấp giá tốt", Price = 129900m, DiscountPrice = 99900m, StockQuantity = 200, ImageUrl = "", Size = "M", Color = "Nhiều màu", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 18, CategoryId = 1, Name = "Tất công sở", Description = "Tất mỏng cho mặc cùng vest", Price = 64900m, DiscountPrice = null, StockQuantity = 75, ImageUrl = "", Size = "L", Color = "Đen", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 19, CategoryId = 2, Name = "Tất du lịch 15-20 mmHg", Description = "Giảm sưng khi đi máy bay", Price = 139900m, DiscountPrice = null, StockQuantity = 100, ImageUrl = "", Size = "M", Color = "Da", CompressionLevel = "15-20 mmHg", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 20, CategoryId = 2, Name = "Tất chống mỏi 20-30 mmHg", Description = "Dùng khi đứng nhiều giờ", Price = 199900m, DiscountPrice = 179900m, StockQuantity = 60, ImageUrl = "", Size = "L", Color = "Xanh đậm", CompressionLevel = "20-30 mmHg", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 21, CategoryId = 3, Name = "Tất yoga chống trượt", Description = "Tất có đế chống trượt cho yoga và pilates", Price = 99900m, DiscountPrice = null, StockQuantity = 110, ImageUrl = "", Size = "S", Color = "Hồng", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 22, CategoryId = 3, Name = "Tất merino cho leo núi", Description = "Tất len merino ấm cho leo núi", Price = 155000m, DiscountPrice = null, StockQuantity = 45, ImageUrl = "", Size = "L", Color = "Nâu", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 23, CategoryId = 1, Name = "Tất trẻ em", Description = "Tất nhiều màu dành cho trẻ em", Price = 42500m, DiscountPrice = null, StockQuantity = 220, ImageUrl = "", Size = "S", Color = "Nhiều màu", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 24, CategoryId = 1, Name = "Tất bệt trong nhà", Description = "Tất ấm có đế chống trượt cho ở nhà", Price = 77500m, DiscountPrice = null, StockQuantity = 95, ImageUrl = "", Size = "M", Color = "Kem", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 25, CategoryId = 2, Name = "Tất hỗ trợ cho bà bầu 15-20 mmHg", Description = "Hỗ trợ nhẹ cho phụ nữ mang thai", Price = 149900m, DiscountPrice = null, StockQuantity = 40, ImageUrl = "", Size = "M", Color = "Xám", CompressionLevel = "15-20 mmHg", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 26, CategoryId = 3, Name = "Tất trượt tuyết", Description = "Tất có đệm cho trượt tuyết", Price = 169900m, DiscountPrice = null, StockQuantity = 30, ImageUrl = "", Size = "L", Color = "Đỏ", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 27, CategoryId = 3, Name = "Tất CrossFit", Description = "Tất bền cho bài tập CrossFit", Price = 92500m, DiscountPrice = null, StockQuantity = 70, ImageUrl = "", Size = "M", Color = "Đen", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 28, CategoryId = 1, Name = "Tất không đường may (Seamless)", Description = "Không đường may, thoải mái cho ngón chân", Price = 62500m, DiscountPrice = 52500m, StockQuantity = 85, ImageUrl = "", Size = "M", Color = "Trắng", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 29, CategoryId = 2, Name = "Tất hồi phục 20-30 mmHg", Description = "Tất nén hỗ trợ phục hồi cơ bắp", Price = 175000m, DiscountPrice = null, StockQuantity = 55, ImageUrl = "", Size = "M", Color = "Đen", CompressionLevel = "20-30 mmHg", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 30, CategoryId = 1, Name = "Tất cotton hữu cơ", Description = "Tất cotton hữu cơ, nguồn gốc bền vững", Price = 79900m, DiscountPrice = 69900m, StockQuantity = 100, ImageUrl = "", Size = "M", Color = "Tự nhiên", CompressionLevel = null, IsActive = true, CreatedAt = DateTime.UtcNow }
        );
    }
}