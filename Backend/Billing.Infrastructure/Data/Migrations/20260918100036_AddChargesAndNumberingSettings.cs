using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChargesAndNumberingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChargeConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChargeType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CalculationType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinInvoiceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxChargeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsTaxable = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    TaxCategory = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, defaultValue: "Active")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RowVersion = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargeConfigurations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NumberingSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Prefix = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, defaultValue: "INV-")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Suffix = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tokens = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, defaultValue: "{YEAR}-")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SequenceLength = table.Column<int>(type: "int", nullable: false, defaultValue: 4),
                    NextNumber = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    ResetPolicy = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastResetDateUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, defaultValue: "Active")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RowVersion = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberingSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NumberingSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeConfigurations_CreatedAtUtc",
                table: "ChargeConfigurations",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeConfigurations_TenantId_Code",
                table: "ChargeConfigurations",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChargeConfigurations_TenantId_Status",
                table: "ChargeConfigurations",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_NumberingSettings_TenantId_DocumentType",
                table: "NumberingSettings",
                columns: new[] { "TenantId", "DocumentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumberingSettings_TenantId_Status",
                table: "NumberingSettings",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChargeConfigurations");

            migrationBuilder.DropTable(
                name: "NumberingSettings");
        }
    }
}
