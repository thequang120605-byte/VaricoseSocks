using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VaricoseSocks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FullName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Size = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompressionLevel = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 9, 30, 2, 15, 30, 627, DateTimeKind.Utc).AddTicks(8869), "Tất thoải mái cho sử dụng hàng ngày", true, "Tất hàng ngày" },
                    { 2, new DateTime(2026, 9, 30, 2, 15, 30, 627, DateTimeKind.Utc).AddTicks(9058), "Tất hỗ trợ nén, tốt cho tuần hoàn và giảm sưng", true, "Tất nén" },
                    { 3, new DateTime(2026, 9, 30, 2, 15, 30, 627, DateTimeKind.Utc).AddTicks(9060), "Tất hiệu suất cao dành cho hoạt động thể thao", true, "Tất thể thao" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Color", "CompressionLevel", "CreatedAt", "Description", "DiscountPrice", "ImageUrl", "IsActive", "Name", "Price", "Size", "StockQuantity", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "Đen", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(5580), "Tất cotton mềm, nhiều màu", null, "", true, "Tất cotton cổ điển", 59000m, "M", 150, null },
                    { 2, 1, "Trắng", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(5765), "Tất cổ thấp thoáng khí cho thể thao", null, "", true, "Tất cổ thể thao", 69900m, "L", 120, null },
                    { 3, 2, "Xám", "15-20 mmHg", new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(5768), "Tất nén nhẹ dùng hàng ngày, phù hợp khi đi máy bay hoặc đứng nhiều", null, "", true, "Tất nén nhẹ 15-20 mmHg", 125000m, "M", 80, null },
                    { 4, 3, "Đen", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(5979), "Tất thoát ẩm cho người chạy bộ", 74900m, "", true, "Tất chạy bộ hiệu suất", 89900m, "M", 200, null },
                    { 5, 3, "Xanh đậm", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(5982), "Đệm thêm cho tập gym và luyện tập", null, "", true, "Tất đệm cho tập luyện", 95000m, "L", 140, null },
                    { 6, 1, "Beige", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(5984), "Tất ẩn cho giày casual", null, "", true, "Tất không lộ (No-Show)", 49900m, "S", 180, null },
                    { 7, 1, "Xám đậm", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(5987), "Tất giữ nhiệt ấm cho mùa lạnh", 90000m, "", true, "Tất giữ nhiệt mùa đông", 110000m, "XL", 60, null },
                    { 8, 2, "Đen", "20-30 mmHg", new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(5989), "Tất nén mạnh hơn cho mục đích y tế", null, "", true, "Tất y tế 20-30 mmHg", 187500m, "M", 50, null },
                    { 9, 3, "Trắng", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(5991), "Tất cổ thấp cho các môn thể thao trên sân", null, "", true, "Tất cổ thấp cho thể thao", 72500m, "M", 130, null },
                    { 10, 1, "Nâu", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6009), "Tất thân thiện môi trường từ sợi tre, mềm và thoáng", 54900m, "", true, "Tất tre (Bamboo)", 64900m, "M", 90, null },
                    { 11, 1, "Xanh", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6012), "Tất kẻ sọc dùng hằng ngày", null, "", true, "Tất kẻ sọc", 54900m, "M", 110, null },
                    { 12, 1, "Xám", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6014), "Tất cổ dài cổ điển bằng cotton", null, "", true, "Tất cổ dài cotton", 57500m, "L", 160, null },
                    { 13, 2, "Beige", "15-20 mmHg", new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6016), "Tất nén phân đoạn thoải mái cho cả ngày", 120000m, "", true, "Tất nén phân đoạn 15-20 mmHg", 140000m, "M", 95, null },
                    { 14, 2, "Đen", "20-30 mmHg", new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6018), "Hỗ trợ phục hồi sau tập luyện", null, "", true, "Tất nén thể thao 20-30 mmHg", 165000m, "L", 70, null },
                    { 15, 3, "Xanh lá", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6021), "Tất bền cho chạy địa hình", null, "", true, "Tất chạy địa hình", 109900m, "M", 85, null },
                    { 16, 3, "Trắng", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6023), "Tất nhẹ cho đạp xe", null, "", true, "Tất xe đạp thoáng khí", 82500m, "S", 120, null },
                    { 17, 1, "Nhiều màu", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6025), "Bộ 3 tất cổ thấp giá tốt", 99900m, "", true, "Bộ 3 tất cổ thấp", 129900m, "M", 200, null },
                    { 18, 1, "Đen", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6037), "Tất mỏng cho mặc cùng vest", null, "", true, "Tất công sở", 64900m, "L", 75, null },
                    { 19, 2, "Da", "15-20 mmHg", new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6039), "Giảm sưng khi đi máy bay", null, "", true, "Tất du lịch 15-20 mmHg", 139900m, "M", 100, null },
                    { 20, 2, "Xanh đậm", "20-30 mmHg", new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6042), "Dùng khi đứng nhiều giờ", 179900m, "", true, "Tất chống mỏi 20-30 mmHg", 199900m, "L", 60, null },
                    { 21, 3, "Hồng", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6044), "Tất có đế chống trượt cho yoga và pilates", null, "", true, "Tất yoga chống trượt", 99900m, "S", 110, null },
                    { 22, 3, "Nâu", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6046), "Tất len merino ấm cho leo núi", null, "", true, "Tất merino cho leo núi", 155000m, "L", 45, null },
                    { 23, 1, "Nhiều màu", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6048), "Tất nhiều màu dành cho trẻ em", null, "", true, "Tất trẻ em", 42500m, "S", 220, null },
                    { 24, 1, "Kem", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6050), "Tất ấm có đế chống trượt cho ở nhà", null, "", true, "Tất bệt trong nhà", 77500m, "M", 95, null },
                    { 25, 2, "Xám", "15-20 mmHg", new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6053), "Hỗ trợ nhẹ cho phụ nữ mang thai", null, "", true, "Tất hỗ trợ cho bà bầu 15-20 mmHg", 149900m, "M", 40, null },
                    { 26, 3, "Đỏ", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6068), "Tất có đệm cho trượt tuyết", null, "", true, "Tất trượt tuyết", 169900m, "L", 30, null },
                    { 27, 3, "Đen", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6070), "Tất bền cho bài tập CrossFit", null, "", true, "Tất CrossFit", 92500m, "M", 70, null },
                    { 28, 1, "Trắng", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6072), "Không đường may, thoải mái cho ngón chân", 52500m, "", true, "Tất không đường may (Seamless)", 62500m, "M", 85, null },
                    { 29, 2, "Đen", "20-30 mmHg", new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6075), "Tất nén hỗ trợ phục hồi cơ bắp", null, "", true, "Tất hồi phục 20-30 mmHg", 175000m, "M", 55, null },
                    { 30, 1, "Tự nhiên", null, new DateTime(2026, 9, 30, 2, 15, 30, 628, DateTimeKind.Utc).AddTicks(6077), "Tất cotton hữu cơ, nguồn gốc bền vững", 69900m, "", true, "Tất cotton hữu cơ", 79900m, "M", 100, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
