using FashionEcommerce.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionEcommerce.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(FashionEcommerceDbContext))]
    [Migration("20260701210000_EnsureAdminPasswordHash")]
    public partial class EnsureAdminPasswordHash : Migration
    {
        private const string AdminPasswordHash = "AQAAAAIAAYagAAAAEKeCR865UafnojgPG7COFHAkUKhEdzz8s0aNown6Eqa0Owaxy1uyy6bXG0UmmU18jg==";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                UPDATE "Users"
                SET "PasswordHash" = '{AdminPasswordHash}',
                    "IsActive" = TRUE,
                    "UpdatedAt" = NOW()
                WHERE "Id" = 1 AND "Email" = 'admin@fashionecommerce.com';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
