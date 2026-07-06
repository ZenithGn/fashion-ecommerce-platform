using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FashionEcommerce.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketingCampaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketingCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Budget = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ActualCost = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    TargetAudience = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Goal = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LandingPageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VoucherId = table.Column<int>(type: "integer", nullable: true),
                    Impressions = table.Column<int>(type: "integer", nullable: false),
                    Clicks = table.Column<int>(type: "integer", nullable: false),
                    Conversions = table.Column<int>(type: "integer", nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingCampaigns_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6950));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6870));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6870));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6870));

            migrationBuilder.UpdateData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6930));

            migrationBuilder.UpdateData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6940));

            migrationBuilder.UpdateData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6940));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6830));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6830));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6830));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6830));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6830));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6830));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6910));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6920));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6920));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6750));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6760));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6760));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 50, 27, 325, DateTimeKind.Utc).AddTicks(6890));

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_CampaignCode",
                table: "MarketingCampaigns",
                column: "CampaignCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketingCampaigns_VoucherId",
                table: "MarketingCampaigns",
                column: "VoucherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketingCampaigns");

            migrationBuilder.UpdateData(
                table: "Carts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2180));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2070));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2080));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2080));

            migrationBuilder.UpdateData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2160));

            migrationBuilder.UpdateData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2160));

            migrationBuilder.UpdateData(
                table: "Inventories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2160));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2030));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2030));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2030));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2030));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2030));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2030));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2130));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2140));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2140));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(1930));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(1940));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(1940));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 6, 9, 18, 38, 425, DateTimeKind.Utc).AddTicks(2100));
        }
    }
}
