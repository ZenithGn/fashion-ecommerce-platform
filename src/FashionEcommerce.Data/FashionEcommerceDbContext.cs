using FashionEcommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.Data
{
    /// <summary>
    /// EF Core DbContext for Fashion Ecommerce database
    /// </summary>
    public class FashionEcommerceDbContext : DbContext
    {
        public FashionEcommerceDbContext(DbContextOptions<FashionEcommerceDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Inventory> Inventories { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.HasMany(e => e.Orders)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Cart)
                    .WithOne(c => c.User)
                    .HasForeignKey<Cart>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasMany(e => e.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.SubCategories)
                    .WithOne(e => e.ParentCategory)
                    .HasForeignKey(e => e.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DiscountPrice).HasColumnType("decimal(10,2)");
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.HasMany(e => e.Inventories)
                    .WithOne(i => i.Product)
                    .HasForeignKey(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Inventory entity
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).IsRequired();
                entity.HasIndex(new[] { "ProductId", "WarehouseId" }).IsUnique();
            });

            // Configure Cart entity
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)");
                entity.HasMany(e => e.Items)
                    .WithOne(ci => ci.Cart)
                    .HasForeignKey(ci => ci.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CartItem entity
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)");
                entity.HasIndex(new[] { "CartId", "ProductId" }).IsUnique();
            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.Property(e => e.SubTotal).HasColumnType("decimal(10,2)");
                entity.Property(e => e.ShippingCost).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)");
                entity.HasMany(e => e.Items)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)");
            });

            // Add seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Nam", Description = "Thời trang nam", IsActive = true },
                new Category { Id = 2, Name = "Nữ", Description = "Thời trang nữ", IsActive = true },
                new Category { Id = 3, Name = "Trẻ em", Description = "Quần áo trẻ em", IsActive = true }
            );

            // Seed Sample User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@fashionecommerce.com",
                    PasswordHash = "hashed_password_here", // In real app, this should be properly hashed
                    PhoneNumber = "0123456789",
                    Country = "Vietnam",
                    Role = UserRole.Admin,
                    IsActive = true
                }
            );

            // Seed Sample Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Áo thun nam cơ bản",
                    Description = "Áo thun thoáng mát, chất liệu cotton 100%",
                    Price = 299000,
                    CategoryId = 1,
                    SKU = "TSM001",
                    Brand = "FashionStore",
                    Color = "Trắng",
                    Size = "M",
                    Material = "Cotton",
                    Rating = 4.5m,
                    IsActive = true
                },
                new Product
                {
                    Id = 2,
                    Name = "Áo sơ mi nữ thanh lịch",
                    Description = "Áo sơ mi dài tay phù hợp công sở",
                    Price = 499000,
                    CategoryId = 2,
                    SKU = "ASN001",
                    Brand = "FashionStore",
                    Color = "Xanh",
                    Size = "S",
                    Material = "Polyester",
                    Rating = 4.8m,
                    IsActive = true
                },
                new Product
                {
                    Id = 3,
                    Name = "Quần jeans nam classic",
                    Description = "Quần jeans bền, thoải mái",
                    Price = 599000,
                    CategoryId = 1,
                    SKU = "QJN001",
                    Brand = "FashionStore",
                    Color = "Xanh đậm",
                    Size = "32",
                    Material = "Denim",
                    Rating = 4.6m,
                    IsActive = true
                }
            );

            // Seed Inventory
            modelBuilder.Entity<Inventory>().HasData(
                new Inventory
                {
                    Id = 1,
                    ProductId = 1,
                    Quantity = 100,
                    ReservedQuantity = 5,
                    WarehouseId = 1,
                    Location = "Kho chính - Tầng 1"
                },
                new Inventory
                {
                    Id = 2,
                    ProductId = 2,
                    Quantity = 50,
                    ReservedQuantity = 2,
                    WarehouseId = 1,
                    Location = "Kho chính - Tầng 2"
                },
                new Inventory
                {
                    Id = 3,
                    ProductId = 3,
                    Quantity = 75,
                    ReservedQuantity = 3,
                    WarehouseId = 1,
                    Location = "Kho chính - Tầng 1"
                }
            );

            // Seed Sample Cart
            modelBuilder.Entity<Cart>().HasData(
                new Cart
                {
                    Id = 1,
                    UserId = 1,
                    TotalPrice = 0,
                    ItemCount = 0
                }
            );
        }
    }
}
