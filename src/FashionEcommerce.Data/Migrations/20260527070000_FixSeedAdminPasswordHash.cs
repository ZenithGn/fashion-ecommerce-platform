using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionEcommerce.Data.Migrations
{
    [DbContext(typeof(FashionEcommerceDbContext))]
    [Migration("20260527070000_FixSeedAdminPasswordHash")]
    public partial class FixSeedAdminPasswordHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"PasswordHash\" = 'AQAAAAIAAYagAAAAEOzwK0SYOoUJDmcnSlEZ1qfQ9N5os+cLuin70oW59QSlIfeFMFeYyEKIzzha7FyTHw==' WHERE \"Id\" = 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"PasswordHash\" = 'hashed_password_here' WHERE \"Id\" = 1;");
        }
    }
}