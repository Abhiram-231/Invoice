using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxRatesAndTaxSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaxType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, defaultValue: "GST")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0.00m),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCompound = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsInclusive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ApplicationLevel = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, defaultValue: "Item")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, defaultValue: "Active")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RowVersion = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxRates_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TaxSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    IsTaxEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    DefaultTaxCalculation = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, defaultValue: "Exclusive")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PricesIncludeTax = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DefaultTaxRateId = table.Column<int>(type: "int", nullable: true),
                    TaxRegistrationNumber = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaxNumberLabel = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, defaultValue: "GSTIN")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnableMultipleTaxes = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    State = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RowVersion = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_CreatedAtUtc",
                table: "TaxRates",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_Priority",
                table: "TaxRates",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_TenantId_Code",
                table: "TaxRates",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_TenantId_Status",
                table: "TaxRates",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_TenantId_TaxType",
                table: "TaxRates",
                columns: new[] { "TenantId", "TaxType" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxSettings_TenantId",
                table: "TaxSettings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxRates");

            migrationBuilder.DropTable(
                name: "TaxSettings");
        }
    }
}
