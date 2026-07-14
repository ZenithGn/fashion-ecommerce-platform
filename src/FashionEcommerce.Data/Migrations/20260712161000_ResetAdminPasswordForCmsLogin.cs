using FashionEcommerce.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionEcommerce.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(FashionEcommerceDbContext))]
    [Migration("20260712161000_ResetAdminPasswordForCmsLogin")]
    public partial class ResetAdminPasswordForCmsLogin : Migration
    {
        private const string AdminPasswordHash = "AQAAAAIAAYagAAAAEPPUhsz8wxiknCxh215CEOfa5PN4I6k2/8FNTISUcmMQLCK94QpaY5uUHbIS5TQoNQ==";

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
