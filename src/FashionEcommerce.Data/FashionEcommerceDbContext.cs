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
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<UserAddress> UserAddresses { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;
        public DbSet<Inventory> Inventories { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Shipment> Shipments { get; set; } = null!;
        public DbSet<ShipmentEvent> ShipmentEvents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // Configure Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ActionName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.ActionName).IsUnique();
            });

            // Configure RolePermission join table
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PermissionId });

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
                entity.Property(e => e.RoleId).HasDefaultValue(2);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Addresses)
                    .WithOne(a => a.User)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Orders)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Cart)
                    .WithOne(c => c.User)
                    .HasForeignKey<Cart>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserAddress entity
            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReceiverName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AddressLine).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Ward).HasMaxLength(100);
                entity.Property(e => e.District).HasMaxLength(100);
                entity.Property(e => e.Province).HasMaxLength(100);
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
                entity.HasMany(e => e.Variants)
                    .WithOne(v => v.Product)
                    .HasForeignKey(v => v.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Images)
                    .WithOne(img => img.Product)
                    .HasForeignKey(img => img.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ProductVariant
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PriceOverride).HasColumnType("decimal(10,2)");
                entity.HasIndex(e => e.SKU).IsUnique();
            });

            // Configure ProductImage
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).IsRequired();
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
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.HasMany(e => e.Items)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Shipment)
                    .WithOne(s => s.Order)
                    .HasForeignKey<Shipment>(s => s.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)");
            });

            // Configure Shipment entity
            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CarrierName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TrackingNumber).HasMaxLength(100);
                entity.Property(e => e.ShippingFee).HasColumnType("decimal(10,2)");
                entity.HasIndex(e => e.OrderId).IsUnique();
                entity.HasIndex(e => e.TrackingNumber);
                entity.HasMany(e => e.Events)
                    .WithOne(e => e.Shipment)
                    .HasForeignKey(e => e.ShipmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ShipmentEvent entity
            modelBuilder.Entity<ShipmentEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Location).HasMaxLength(255);
                entity.Property(e => e.Note).HasMaxLength(500);
                entity.HasIndex(e => e.ShipmentId);
            });

            // Add seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "Admin", Description = "System administrator", IsDeleted = false },
                new Role { Id = 2, RoleName = "Customer", Description = "Customer account", IsDeleted = false },
                new Role { Id = 3, RoleName = "Staff", Description = "Store staff account", IsDeleted = false },
                new Role { Id = 4, RoleName = "Manager", Description = "Operations manager account", IsDeleted = false }
            );

            // Seed Permissions
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, ActionName = "products.view", Description = "View products", IsDeleted = false },
                new Permission { Id = 2, ActionName = "products.manage", Description = "Create, update and delete products", IsDeleted = false },
                new Permission { Id = 3, ActionName = "orders.view", Description = "View orders", IsDeleted = false },
                new Permission { Id = 4, ActionName = "orders.manage", Description = "Manage orders", IsDeleted = false },
                new Permission { Id = 5, ActionName = "users.manage", Description = "Manage users and staff", IsDeleted = false },
                new Permission { Id = 6, ActionName = "reports.view", Description = "View reports", IsDeleted = false },
                new Permission { Id = 7, ActionName = "dashboard.view", Description = "View admin dashboard", IsDeleted = false },
                new Permission { Id = 8, ActionName = "inventory.manage", Description = "Manage inventory", IsDeleted = false },
                new Permission { Id = 9, ActionName = "roles.manage", Description = "Manage roles and permissions", IsDeleted = false }
            );

            // Seed Role Permissions
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RoleId = 1, PermissionId = 1 },
                new RolePermission { RoleId = 1, PermissionId = 2 },
                new RolePermission { RoleId = 1, PermissionId = 3 },
                new RolePermission { RoleId = 1, PermissionId = 4 },
                new RolePermission { RoleId = 1, PermissionId = 5 },
                new RolePermission { RoleId = 1, PermissionId = 6 },
                new RolePermission { RoleId = 1, PermissionId = 7 },
                new RolePermission { RoleId = 1, PermissionId = 8 },
                new RolePermission { RoleId = 1, PermissionId = 9 },
                new RolePermission { RoleId = 3, PermissionId = 1 },
                new RolePermission { RoleId = 3, PermissionId = 3 },
                new RolePermission { RoleId = 3, PermissionId = 4 },
                new RolePermission { RoleId = 3, PermissionId = 7 },
                new RolePermission { RoleId = 3, PermissionId = 8 },
                new RolePermission { RoleId = 4, PermissionId = 1 },
                new RolePermission { RoleId = 4, PermissionId = 2 },
                new RolePermission { RoleId = 4, PermissionId = 3 },
                new RolePermission { RoleId = 4, PermissionId = 4 },
                new RolePermission { RoleId = 4, PermissionId = 5 },
                new RolePermission { RoleId = 4, PermissionId = 6 },
                new RolePermission { RoleId = 4, PermissionId = 7 },
                new RolePermission { RoleId = 4, PermissionId = 8 },
                new RolePermission { RoleId = 2, PermissionId = 1 }
            );

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
                    PasswordHash = "AQAAAAIAAYagAAAAEOzwK0SYOoUJDmcnSlEZ1qfQ9N5os+cLuin70oW59QSlIfeFMFeYyEKIzzha7FyTHw==",
                    PhoneNumber = "0123456789",
                    RoleId = 1,
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
