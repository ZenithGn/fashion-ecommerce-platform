using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using Microsoft.AspNetCore.Identity;

namespace FashionEcommerce.API.Tests
{
    public static class TestDataSeeder
    {
        public static void Seed(FashionEcommerceDbContext db)
        {
            SeedRolesAndUsers(db);

            if (db.Products.Any(p => p.Id == 100)) return;

            var cat = db.Categories.Find(1);
            if (cat == null)
            {
                cat = new Category { Id = 1, Name = "TestCat", IsActive = true };
                db.Categories.Add(cat);
                db.SaveChanges();
            }

            var p1 = new Product
            {
                Id = 100,
                Name = "Product With Variant And Image",
                Description = "Test",
                Price = 100m,
                CategoryId = 1,
                SKU = "P100",
                IsActive = true
            };

            var img = new ProductImage { Id = 10, ProductId = 100, ImageUrl = "http://example.com/img1.jpg", IsThumbnail = true };
            var variant = new ProductVariant { Id = 1000, ProductId = 100, SKU = "P100-S1", Color = "Red", Size = "M", PriceOverride = 120m };
            var inv = new Inventory { Id = 500, ProductId = 100, Quantity = 10, ReservedQuantity = 2, WarehouseId = 1 };

            var p2 = new Product
            {
                Id = 101,
                Name = "Product Without Image",
                Description = "No images",
                Price = 50m,
                CategoryId = 1,
                SKU = "P101",
                IsActive = true
            };

            var p3 = new Product
            {
                Id = 102,
                Name = "Product Without Variants",
                Description = "No variants",
                Price = 75m,
                CategoryId = 1,
                SKU = "P102",
                IsActive = true
            };

            var inv3 = new Inventory { Id = 502, ProductId = 102, Quantity = 5, ReservedQuantity = 0, WarehouseId = 1 };

            db.Products.AddRange(p1, p2, p3);
            db.ProductImages.Add(img);
            db.ProductVariants.Add(variant);
            db.Inventories.AddRange(inv, inv3);

            db.SaveChanges();
        }

        private static void SeedRolesAndUsers(FashionEcommerceDbContext db)
        {
            if (!db.Users.Any(u => u.Email == "admin@example.com"))
            {
                var admin = new User
                {
                    Id = 900,
                    RoleId = 1,
                    FirstName = "Test",
                    LastName = "Admin",
                    Email = "admin@example.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, "Password123!");
                db.Users.Add(admin);
            }

            db.SaveChanges();
        }
    }
}
