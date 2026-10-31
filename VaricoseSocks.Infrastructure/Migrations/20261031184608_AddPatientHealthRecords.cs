using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VaricoseSocks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientHealthRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientHealthRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AnkleCircumferenceCm = table.Column<double>(type: "double", nullable: false),
                    CalfCircumferenceCm = table.Column<double>(type: "double", nullable: true),
                    ThighCircumferenceCm = table.Column<double>(type: "double", nullable: true),
                    LegLengthCm = table.Column<double>(type: "double", nullable: true),
                    CeapStage = table.Column<int>(type: "int", nullable: false),
                    Symptoms = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiagnosisNote = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DoctorName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrescribedCompression = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecommendedSize = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MeasurementDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReplacementDueDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReminderSent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientHealthRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientHealthRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PatientHealthRecords_UserId",
                table: "PatientHealthRecords",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientHealthRecords");
        }
    }
}
