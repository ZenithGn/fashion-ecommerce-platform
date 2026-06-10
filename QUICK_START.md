# 🚀 Quick Start Guide - Fashion E-Commerce Platform

## Tổng Quan Nhanh

Dự án này là một backend hoàn chỉnh cho nền tảng thương mại điện tử thời trang được xây dựng bằng .NET 8 với cấu trúc sạch và organized.

## 📦 Cấu Trúc Rõ Ràng

```
src/
├── FashionEcommerce.API/              # 🌐 Web API (ASP.NET Core)
│   ├── Controllers/                   # API Controllers
│   ├── Program.cs                     # Startup config
│   └── appsettings.json              # Settings
│
├── FashionEcommerce.Core/             # 📦 Domain Models (Entities)
│   └── Entities/                      # Business entities
│
├── FashionEcommerce.Data/             # 💾 Data Access (EF Core)
│   └── FashionEcommerceDbContext.cs   # Database context
│
└── FashionEcommerce.Services/         # 🔧 Business Logic
    ├── Interfaces/                    # Service interfaces
    └── Repositories/                  # Repository pattern
```

## 🎯 5 Bước Để Bắt Đầu

### 1️⃣ Chuẩn Bị Môi Trường

```bash
# Đảm bảo cài đặt .NET 8 SDK
dotnet --version

# Khôi phục dependencies
dotnet restore
```

### 2️⃣ Cấu Hình Cơ Sở Dữ Liệu

Project hiện đang dùng PostgreSQL trên Neon. File `src/FashionEcommerce.API/appsettings.json` đã trỏ sẵn tới Neon, nên để chạy local chỉ cần giữ cấu hình hiện có hoặc thay bằng connection string Neon của bạn:

```json
"ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_NEON_HOST;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;"
}
```

### 3️⃣ Tạo Database

```bash
dotnet restore
dotnet build

# App sẽ tự migrate database khi khởi động
dotnet run --project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```

### 4️⃣ Chạy Ứng Dụng

```bash
dotnet run --project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```

### 5️⃣ Kiểm Tra API

- 🔌 **API Base**: `https://localhost:5001`
- 📚 **Swagger UI**: `https://localhost:5001/swagger`

## 📋 Các Module Chính

### 1. **User (Người Dùng)**

- ✅ Đăng ký / Đăng nhập
- ✅ Thông tin cá nhân
- ✅ Địa chỉ giao hàng
- ✅ Vai trò (Customer, Admin, Staff)

### 2. **Category (Danh Mục)**

- ✅ Danh mục chính
- ✅ Danh mục phụ (Sub-categories)
- ✅ Hình ảnh & mô tả

### 3. **Product (Sản Phẩm)**

- ✅ Thông tin sản phẩm đầy đủ
- ✅ Giá & giảm giá
- ✅ Đánh giá sao
- ✅ Tìm kiếm

### 4. **Inventory (Kho Hàng)**

- ✅ Theo dõi tồn kho
- ✅ Dự trữ sản phẩm
- ✅ Tính sẵn có tự động

### 5. **Cart (Giỏ Hàng)**

- ✅ Thêm/xóa sản phẩm
- ✅ Cập nhật số lượng
- ✅ Tính tổng tự động

### 6. **Order (Đơn Hàng)**

- ✅ Tạo đơn hàng
- ✅ Theo dõi trạng thái
- ✅ Chi tiết đơn hàng

## 🔗 API Endpoints Chính

```
GET    /api/products                      # Lấy tất cả sản phẩm
GET    /api/products/{id}                 # Lấy sản phẩm
GET    /api/products/category/{categoryId}# Sản phẩm theo danh mục
GET    /api/products/search?searchTerm=   # Tìm kiếm
POST   /api/products                      # Tạo sản phẩm
PUT    /api/products/{id}                 # Cập nhật sản phẩm
DELETE /api/products/{id}                 # Xóa sản phẩm

GET    /api/categories                    # Lấy danh mục
GET    /api/categories/{id}               # Lấy danh mục
POST   /api/categories                    # Tạo danh mục
PUT    /api/categories/{id}               # Cập nhật danh mục
DELETE /api/categories/{id}               # Xóa danh mục

GET    /api/inventories                   # Lấy tồn kho
GET    /api/inventories/product/{id}      # Tồn kho sản phẩm
GET    /api/inventories/{id}/available    # Số lượng sẵn có
POST   /api/inventories/{id}/reserve      # Dự trữ
POST   /api/inventories/{id}/release      # Giải phóng dự trữ

GET    /api/orders                        # Lấy đơn hàng
GET    /api/orders/{id}                   # Chi tiết đơn hàng
POST   /api/orders                        # Tạo đơn hàng
PUT    /api/orders/{id}                   # Cập nhật đơn hàng

POST   /api/auth/register                 # Đăng ký tài khoản
POST   /api/auth/login                    # Đăng nhập
POST   /api/auth/request-password-reset   # Xin token reset mật khẩu
POST   /api/auth/reset-password          # Đổi mật khẩu bằng token
```

## 🗄️ Thiết Kế Database

### Các Bảng Chính

- **Users**: Quản lý người dùng
- **Roles**: Vai trò người dùng
- **Permissions**: Quyền truy cập
- **Categories**: Danh mục sản phẩm
- **Products**: Sản phẩm chi tiết
- **Inventories**: Tồn kho
- **Carts** & **CartItems**: Giỏ hàng
- **Orders** & **OrderItems**: Đơn hàng

### Các Mối Quan Hệ

```
User (1) ─────── (N) Order
User (1) ─────── (1) Cart
Cart (1) ─────── (N) CartItem
Category (1) ────────── (N) Product
Product (1) ──────── (N) Inventory
Product (1) ──────── (N) CartItem
Product (1) ──────── (N) OrderItem
Order (1) ─────── (N) OrderItem
```

## 💾 Dữ Liệu Mẫu

Dự án bao gồm seed data tự động:

- ✅ 3 danh mục (Nam, Nữ, Trẻ em)
- ✅ 1 tài khoản admin
- ✅ 3 sản phẩm mẫu
- ✅ Tồn kho cho các sản phẩm
- ✅ Seed roles và permissions cơ bản

## 🔒 Bảo Mật

Các tính năng bảo mật đã bao gồm:

- ✅ Soft Delete (xóa mềm)
- ✅ Audit Trail (ghi lại ngày tạo/sửa)
- ✅ CORS Configuration
- ✅ Password Hashing (sẵn sàng)
- ✅ Role-based Access (sẵn sàng)
- ✅ Authentication JWT

## 📝 File Cấu Hình

| File                   | Mục Đích                    |
| ---------------------- | --------------------------- |
| `appsettings.json`     | Connection string, settings |
| `.gitignore`           | Git ignore patterns         |
| `FashionEcommerce.sln` | Solution file               |
| `PROJECT_STRUCTURE.md` | Tài liệu chi tiết           |

## 🛠️ Công Nghệ Sử Dụng

| Công Nghệ             | Phiên Bản | Mục Đích          |
| --------------------- | --------- | ----------------- |
| .NET                  | 8.0       | Framework chính   |
| ASP.NET Core          | 8.0       | Web API           |
| Entity Framework Core | 8.0       | ORM               |
| PostgreSQL            | -         | Database          |
| Neon                  | -         | Database hosting  |
| Swagger               | 6.4.6     | API Documentation |

## 🚦 Trạng Thái Phát Triển

✅ **Hoàn Thành:**

- Database schema
- Entity models
- DbContext
- API Controllers (Products, Categories, Inventories)
- Seed data
- Swagger documentation
- ✅ Authentication & JWT

⏳ **Tiếp Theo:**

- [ ] Authorization & Roles
- [ ] Service layer implementations
- [ ] Validation rules
- [ ] Unit tests
- [ ] Integration tests
- [ ] Payment gateway
- [ ] Email notifications
- [ ] Image upload service
- [ ] Logging & monitoring

## 🎓 Kiến Trúc Sử Dụng

```
┌─────────────────────────────────────────────────────┐
│              FashionEcommerce.API                   │
│         (ASP.NET Core Controllers)                  │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│        FashionEcommerce.Services                    │
│     (Business Logic & Interfaces)                   │
└────────────────────┬────────────────────────────────┘
                     │
         ┌───────────┴───────────┐
         │                       │
    ┌────▼─────────────┐  ┌─────▼──────────────┐
    │ Repositories    │  │ Service Impl       │
    │ (Data Access)   │  │ (Business Logic)   │
    └────┬─────────────┘  └─────┬──────────────┘
         │                      │
         └───────────┬──────────┘
                     │
        ┌────────────▼──────────────┐
        │  FashionEcommerce.Data    │
        │  (DbContext & EF Core)    │
        └────────────┬──────────────┘
                     │
        ┌────────────▼──────────────┐
        │  FashionEcommerce.Core    │
        │  (Domain Entities)        │
        └────────────┬──────────────┘
                     │
        ┌────────────▼──────────────┐
        │  PostgreSQL / Neon DB     │
        │    (FashionEcommerce)     │
        └───────────────────────────┘
```

## 🆘 Gặp Vấn Đề?

### Migration errors

```bash
# Xóa migration cuối cùng
dotnet ef migrations remove

# Kiểm tra migrations
dotnet ef migrations list

# Reset database
dotnet ef database drop
dotnet ef database update
```

### Connection string issues

- Kiểm tra Neon host/username/password
- Đảm bảo connection string dùng `SSL Mode=Require`
- Nếu muốn chạy local offline, cần đổi sang PostgreSQL local

### Port already in use

```bash
# Thay đổi port trong Program.cs hoặc dùng
dotnet run --urls "https://localhost:5002"
```

## 📚 Tài Liệu Thêm

- [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) - Tài liệu chi tiết
- [Swagger UI](https://localhost:5001/swagger) - API documentation (chạy ứng dụng)

## 📞 Hỗ Trợ

Nếu cần giúp đỡ, vui lòng:

1. Kiểm tra lại các bước setup
2. Xem xét error logs
3. Tạo issue trên GitHub

---

**Happy Coding! 🎉**

Version: 1.0.0  
Created: 2026-05-26
