using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionEcommerce.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerRoleAndDashboardPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "IsDeleted", "RoleName" },
                values: new object[] { 4, new DateTime(2026, 7, 1, 7, 0, 0, DateTimeKind.Utc), "Operations manager account", false, "Manager" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ActionName", "CreatedAt", "Description", "IsDeleted" },
                values: new object[,]
                {
                    { 7, "dashboard.view", new DateTime(2026, 7, 1, 7, 0, 0, DateTimeKind.Utc), "View admin dashboard", false },
                    { 8, "inventory.manage", new DateTime(2026, 7, 1, 7, 0, 0, DateTimeKind.Utc), "Manage inventory", false },
                    { 9, "roles.manage", new DateTime(2026, 7, 1, 7, 0, 0, DateTimeKind.Utc), "Manage roles and permissions", false }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                values: new object[,]
                {
                    { 1, 7 },
                    { 1, 8 },
                    { 1, 9 },
                    { 3, 7 },
                    { 3, 8 },
                    { 4, 1 },
                    { 4, 2 },
                    { 4, 3 },
                    { 4, 4 },
                    { 4, 5 },
                    { 4, 6 },
                    { 4, 7 },
                    { 4, 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 1, 7 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 1, 8 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 1, 9 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 3, 7 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 3, 8 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 4, 1 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 4, 2 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 4, 3 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 4, 4 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 4, 5 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 4, 6 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 4, 7 });
            migrationBuilder.DeleteData(table: "RolePermissions", keyColumns: new[] { "RoleId", "PermissionId" }, keyValues: new object[] { 4, 8 });

            migrationBuilder.DeleteData(table: "Roles", keyColumn: "Id", keyValue: 4);

            migrationBuilder.DeleteData(table: "Permissions", keyColumn: "Id", keyValue: 7);
            migrationBuilder.DeleteData(table: "Permissions", keyColumn: "Id", keyValue: 8);
            migrationBuilder.DeleteData(table: "Permissions", keyColumn: "Id", keyValue: 9);
        }
    }
}
