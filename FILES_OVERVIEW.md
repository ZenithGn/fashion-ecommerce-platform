# 📑 Fashion E-Commerce Platform - Files Overview

## 📂 Project Structure Summary

```
fashion-ecommerce-platform/
│
├── 📄 Documentation Files
│   ├── README.md                      (Original README)
│   ├── PROJECT_STRUCTURE.md           ✨ UPDATED - Detailed architecture & endpoints
│   ├── QUICK_START.md                 (Getting started guide)
│   ├── DATABASE_SCHEMA.md             ✨ UPDATED - Database design & ERD
│   └── FILES_OVERVIEW.md              ✨ UPDATED - This file
│
├── 📦 Solution & Projects
│   ├── FashionEcommerce.sln           (Solution file)
│   │
│   └── src/
│       │
│       ├── 🟦 FashionEcommerce.API/
│       │   ├── FashionEcommerce.API.csproj
│       │   ├── Program.cs             (Main startup & DI configuration)
│       │   ├── appsettings.json       (Configuration)
│       │   ├── .env.example           (Environment template)
│       │   │
│       │   ├── Controllers/
│       │   │   ├── AuthController.cs          ✨ Registration, login & password reset
│       │   │   ├── CartController.cs          ✨ Cart management (add, update, delete)
│       │   │   ├── CategoriesController.cs    ✨ Categories management
│       │   │   ├── DashboardController.cs     ✨ Analytical analytics, revenues, counts
│       │   │   ├── InventoriesController.cs   ✨ Stock levels and reservations
│       │   │   ├── OrdersController.cs        ✨ Checkout & Order processing
│       │   │   ├── PermissionsController.cs   ✨ Access permissions management
│       │   │   ├── ProductsController.cs      ✨ Products catalog & variants
│       │   │   ├── RolesController.cs         ✨ Security roles management
│       │   │   ├── ShipmentsController.cs     ✨ Courier and tracking logs
│       │   │   ├── UserAddressesController.cs ✨ Shipping address book
│       │   │   └── UsersController.cs         ✨ Member account management
│       │   │
│       │   └── wwwroot/
│       │       ├── index.html                 (Dashboard Admin GUI)
│       │       ├── app.js                     (Dashboard Frontend Logic)
│       │       └── styles.css                 (Dashboard styling)
│       │
│       ├── 📦 FashionEcommerce.Core/
│       │   ├── FashionEcommerce.Core.csproj
│       │   │
│       │   └── Entities/
│       │       ├── BaseEntity.cs              (Audit & Soft-delete columns)
│       │       ├── User.cs                    (User account info)
│       │       ├── UserAddress.cs             ✨ Multiple delivery address records
│       │       ├── Role.cs                    ✨ Security Roles definitions
│       │       ├── Permission.cs              ✨ Action privileges tags
│       │       ├── RolePermission.cs          ✨ Junction table for Roles & Permissions
│       │       ├── Product.cs                 (Product metadata)
│       │       ├── ProductVariant.cs          ✨ Variant options (Size, Color, PriceOverrides)
│       │       ├── ProductImage.cs            ✨ Product photo library
│       │       ├── Category.cs                (Product hierarchical categories)
│       │       ├── Inventory.cs               (Warehouse stock & reservations)
│       │       ├── Cart.cs                    (Shopping cart headers)
│       │       ├── CartItem.cs                (Shopping cart lines)
│       │       ├── Order.cs                   (Order headers)
│       │       ├── OrderItem.cs               (Order lines)
│       │       ├── Shipment.cs                ✨ Delivery transit record
│       │       └── ShipmentEvent.cs           ✨ Delivery step tracker
│       │
│       ├── 💾 FashionEcommerce.Data/
│       │   ├── FashionEcommerce.Data.csproj
│       │   ├── FashionEcommerceDbContext.cs   (EF Core DB config, relationships, seeds)
│       │   │
│       │   └── Migrations/
│       │       └── (Auto-generated C# migration logs)
│       │
│       └── 🔧 FashionEcommerce.Services/
│           ├── FashionEcommerce.Services.csproj
│           │
│           ├── Interfaces/
│           │   └── IServiceInterfaces.cs      (Service abstractions)
│           │
│           ├── Repositories/
│           │   └── Repository.cs              (Generic DB operations implementation)
│           │
│           ├── Email/
│           │   └── SmtpEmailSender.cs         ✨ System email communications
│           │
│           └── Services/
│               ├── ProductService.cs          ✨ Product catalog processing
│               ├── ShipmentService.cs         ✨ Shipping & tracking operations
│               └── (Other implementation modules)
│
└── 🧪 Test Project
    └── test/FashionEcommerce.API.Tests/
        ├── FashionEcommerceApiFactory.cs      (Test server orchestrator)
        ├── TestDataSeeder.cs                  (Mock database seeds)
        ├── SearchTests.cs                     (Faceted search checks)
        ├── ProductsControllerTests.cs         (Product variations test)
        └── CheckoutIntegrationTests.cs        (End-to-end checkout & cancel tests)
```

---

## 📄 File Descriptions

### **Documentation**

#### README.md
- General repository setup information and configuration commands.

#### PROJECT_STRUCTURE.md
- Comprehensive system architecture details.
- Vietnamese documentation of core modules, databases, installation, and REST API routes.

#### QUICK_START.md
- Step-by-step setup guides, database configurations, and app launches.

#### DATABASE_SCHEMA.md
- Text-based Entity Relationship Diagram (ERD).
- SQL Column constraints, design patterns, workflows, and sample query plans.

#### FILES_OVERVIEW.md
- This file, compiling file mappings and checklist statuses.

---

### **ASP.NET Core Web API Project (`FashionEcommerce.API`)**

#### Program.cs
- App startup configuration, CORS policies, Swagger initialization, DbContext settings, and JWT Authentication token authentication filters.

#### appsettings.json
- Connection strings, logging limits, JWT parameters, and mail SMTP port declarations.

#### Controllers/
- **AuthController.cs:** Controls user registration, JWT generation, and password resets.
- **CartController.cs:** Handles adding, updating, and removing cart entries.
- **CategoriesController.cs:** Exposes categories and subcategories.
- **DashboardController.cs:** Serves administrative summary panels (total sales, user signups, inventory notices).
- **InventoriesController.cs:** Performs safety stock checks and locks item count temporarily.
- **OrdersController.cs:** Directs customer order creation, status checks, and cancellation requests.
- **PermissionsController.cs:** Lists operational action identifiers for RBAC admin.
- **ProductsController.cs:** Administers products, variant setups, and image libraries.
- **RolesController.cs:** Oversees system role definitions and assignment policies.
- **ShipmentsController.cs:** Tracks packages, carrier settings, and delivery transit routes.
- **UserAddressesController.cs:** Maintains the address book of customers.
- **UsersController.cs:** Handles profile editing and admin user query grids.

---

### **Domain Core Project (`FashionEcommerce.Core`)**

#### Entities/
- **BaseEntity.cs:** Parent model holding `Id`, `CreatedAt`, `UpdatedAt`, and logical `IsDeleted` fields.
- **User.cs:** Represents admin, staff, and customer accounts linked to roles and addresses.
- **UserAddress.cs:** Multi-location shipping destination cards.
- **Role.cs & Permission.cs & RolePermission.cs:** Implements Dynamic Role-Based Access Control (RBAC).
- **Product.cs & ProductVariant.cs & ProductImage.cs:** Supports flexible variations (SKU, pricing override, colors, photo catalogs).
- **Category.cs:** Handles nesting parent/child menu trees.
- **Inventory.cs:** Prevents over-selling by tracking physical vs reserved stock levels.
- **Cart.cs & CartItem.cs:** Tracks shoppers' selections before purchase.
- **Order.cs & OrderItem.cs:** Persists checkouts, tax/shipping adjustments, size/color options chosen, and tracking numbers.
- **Shipment.cs & ShipmentEvent.cs:** Tracks cargo transit timeline checkpoints.

---

### **Infrastructure Data Project (`FashionEcommerce.Data`)**

#### FashionEcommerceDbContext.cs
- Establishes DB relationship keys, unique indexes, constraints, and mock seed values (such as categories, default Admin user, default products, roles, permissions).

---

### **Business Services Project (`FashionEcommerce.Services`)**

#### Interfaces/IServiceInterfaces.cs
- Defines abstractions for `IUserService`, `IProductService`, `ICategoryService`, `IInventoryService`, `ICartService`, `IOrderService`, `IShipmentService`, `IRolePermissionService`.

#### Repositories/Repository.cs
- Implements generic methods (`GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` as soft-delete, `SaveChangesAsync`) to enforce DRY principles.

---

## 📊 Project Statistics

| Category                        | Count                     |
| ------------------------------- | ------------------------- |
| **Documentation Files**         | 5                         |
| **C# Entity Classes**           | 16                        |
| **API Controller Files**        | 12                        |
| **C# Service Files & Interfaces**| ~15                       |
| **Automated Test Files**        | 5                         |
| **Configuration / Project Files**| 6                         |
| **Static UI Panel Files**       | 3                         |
| **Total Files**                 | **~62**                   |

---

## 🎯 Module Implementation Coverage

### ✅ User & Security Module
- [x] Entity model (User, Role, Permission, RolePermission)
- [x] DbContext configuration & seed values
- [x] API endpoints (Users, Roles, Permissions, Auth)
- [x] JWT token generation & verification
- [x] Role-Based Access Control (RBAC) middleware check

### ✅ User Address Module
- [x] Entity model (UserAddress)
- [x] DbContext configuration
- [x] API endpoints (UserAddressesController)
- [x] Ownership authorization validation

### ✅ Category Module
- [x] Entity model (hierarchical Category)
- [x] DbContext configuration
- [x] API endpoints (CategoriesController)
- [x] Service business implementation

### ✅ Product Module
- [x] Entity models (Product, ProductVariant, ProductImage)
- [x] DbContext configuration & indexes
- [x] API endpoints (ProductsController)
- [x] Advanced paged search & SKU lookup logic

### ✅ Inventory Module
- [x] Entity model
- [x] DbContext configuration
- [x] API endpoints (InventoriesController)
- [x] Stock reservation & release math

### ✅ Cart Module
- [x] Entity models (Cart, CartItem)
- [x] DbContext configuration
- [x] API endpoints (CartController)
- [x] Dynamic cart calculations

### ✅ Order & Shipment Module
- [x] Entity models (Order, OrderItem, Shipment, ShipmentEvent)
- [x] DbContext configuration
- [x] API endpoints (OrdersController, ShipmentsController)
- [x] Transactional Checkout flow
- [x] Logistical carrier and tracking step update logic

---

## 🛠️ Technologies Applied
- **Target SDK:** .NET 8.0
- **Database Engine:** PostgreSQL (Neon database)
- **Object Relational Mapper:** Entity Framework Core 8.0
- **Testing Engine:** XUnit, FluentAssertions, Microsoft.NET.Test.Sdk, Microsoft.AspNetCore.Mvc.Testing
- **Security Protocols:** Microsoft.AspNetCore.Authentication.JwtBearer (JWT Tokens)
- **Email Gateway:** MailKit / SMTP Client

**Document Version:** 2.0  
**Last Updated:** 2026-07-01  
