# Fashion E-Commerce Platform - .NET Backend

Một nền tảng thương mại điện tử thời trang được xây dựng bằng .NET với một kiến trúc sạch và module hóa.

## 📋 Mục Lục

- [Tổng Quan](#tổng-quan)
- [Cấu Trúc Dự Án](#cấu-trúc-dự-án)
- [Các Module Chính](#các-module-chính)
- [Thiết Kế Cơ Sở Dữ Liệu](#thiết-kế-cơ-sở-dữ-liệu)
- [Cài Đặt & Chạy](#cài-đặt--chạy)
- [Tài Liệu API](#tài-liệu-api)

## 🎯 Tổng Quan

Fashion E-Commerce Platform là một backend được xây dựng hoàn toàn bằng .NET 8.0 với Entity Framework Core, cung cấp các API RESTful cho nền tảng thương mại điện tử thời trang.

**Công nghệ:**

- .NET 8.0
- Entity Framework Core 8.0
- PostgreSQL / Neon
- Swagger/OpenAPI
- CORS Support

## 📁 Cấu Trúc Dự Án

```
fashion-ecommerce-platform/
├── FashionEcommerce.sln                    # Solution file
├── src/
│   ├── FashionEcommerce.API/              # ASP.NET Core Web API
│   │   ├── Controllers/                   # API Controllers
│   │   ├── Program.cs                     # Startup configuration
│   │   ├── appsettings.json              # Configuration settings
│   │   └── FashionEcommerce.API.csproj
│   │
│   ├── FashionEcommerce.Core/             # Core/Domain Models
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs             # Base entity class
│   │   │   ├── User.cs                   # User entity
│   │   │   ├── Product.cs                # Product entity
│   │   │   ├── Category.cs               # Category entity
│   │   │   ├── Inventory.cs              # Inventory entity
│   │   │   ├── Cart.cs                   # Cart & CartItem entities
│   │   │   └── Order.cs                  # Order & OrderItem entities
│   │   └── FashionEcommerce.Core.csproj
│   │
│   ├── FashionEcommerce.Data/             # Data Access Layer
│   │   ├── FashionEcommerceDbContext.cs  # DbContext
│   │   ├── Migrations/                   # EF Core Migrations
│   │   └── FashionEcommerce.Data.csproj
│   │
│   └── FashionEcommerce.Services/         # Business Logic Layer
│       ├── Interfaces/                   # Service interfaces
│       ├── Repositories/                 # Repository pattern
│       └── FashionEcommerce.Services.csproj
│
└── README.md
```

## 🏗️ Các Module Chính

### 1. **User (Người Dùng)**

Quản lý thông tin khách hàng, tài khoản và quyền hạn.

**Thông tin lưu trữ:**

- Thông tin cá nhân (Tên, Email, Số điện thoại)
- Địa chỉ giao hàng
- Vai trò (Customer, Admin, Staff)
- Thông tin đăng nhập

**Trạng thái:** Active/Inactive

---

### 2. **Category (Danh Mục)**

Phân loại sản phẩm thời trang.

**Tính năng:**

- Danh mục cha-con (Sub-categories)
- Tên danh mục, mô tả, hình ảnh
- Hỗ trợ danh mục được bật/tắt

---

### 3. **Product (Sản Phẩm)**

Quản lý sản phẩm thời trang.

**Thông tin:**

- Tên, mô tả, giá
- SKU, thương hiệu
- Màu sắc, kích cỡ, chất liệu
- Hình ảnh, đánh giá sao
- Trạng thái hoạt động

---

### 4. **Inventory (Kho Hàng)**

Theo dõi tồn kho sản phẩm.

**Chức năng:**

- Số lượng tồn kho và được đặt trước
- Vị trí kho lưu trữ
- Tính toán tự động số lượng có sẵn

---

### 5. **Cart (Giỏ Hàng)**

Quản lý giỏ hàng của người dùng.

**Thông tin:**

- Mục giỏ hàng với sản phẩm
- Số lượng, giá
- Tính tổng động

---

### 6. **Order (Đơn Hàng)**

Quản lý đơn hàng khách hàng.

**Trạng thái:**

- Pending (Chờ xử lý)
- Processing (Đang xử lý)
- Shipped (Đã gửi)
- Delivered (Đã giao)
- Cancelled (Đã hủy)
- Returned (Trả lại)

**Thông tin:**

- Số đơn hàng, ngày tạo
- Chi phí vận chuyển, thuế, giảm giá
- Địa chỉ giao hàng
- Số theo dõi vận chuyển

---

## 🗄️ Thiết Kế Cơ Sở Dữ Liệu

### Bảng chính:

#### Users

```sql
Id (PK)
FirstName
LastName
Email (UNIQUE)
PasswordHash
PhoneNumber
Address
City, State, PostalCode, Country
Role (0=Customer, 1=Admin, 2=Staff)
IsActive
LastLoginAt
CreatedAt, UpdatedAt
IsDeleted
```

#### Categories

```sql
Id (PK)
Name
Description
ImageUrl
ParentCategoryId (FK)
IsActive
CreatedAt, UpdatedAt
IsDeleted
```

#### Products

```sql
Id (PK)
Name
Description
Price (decimal)
DiscountPrice (decimal)
CategoryId (FK)
SKU (UNIQUE)
Brand
Color, Size, Material
ImageUrl
Rating (0-5)
ReviewCount
IsActive
CreatedAt, UpdatedAt
IsDeleted
```

#### Inventories

```sql
Id (PK)
ProductId (FK)
Quantity
ReservedQuantity
WarehouseId
Location
LastRestockDate
Notes
CreatedAt, UpdatedAt
IsDeleted
```

#### Carts

```sql
Id (PK)
UserId (FK) - UNIQUE
TotalPrice (decimal)
ItemCount
CreatedAt, UpdatedAt
IsDeleted
```

#### CartItems

```sql
Id (PK)
CartId (FK)
ProductId (FK)
Quantity
UnitPrice (decimal)
TotalPrice (decimal)
CreatedAt, UpdatedAt
IsDeleted
```

#### Orders

```sql
Id (PK)
UserId (FK)
OrderNumber (UNIQUE)
Status (0=Pending, 1=Processing, 2=Shipped, 3=Delivered, 4=Cancelled, 5=Returned)
SubTotal, ShippingCost, TaxAmount, DiscountAmount, TotalPrice (decimal)
ShippingAddress
City, State, PostalCode, Country
PhoneNumber
Notes
ShippedDate, DeliveredDate
TrackingNumber
CreatedAt, UpdatedAt
IsDeleted
```

#### OrderItems

```sql
Id (PK)
OrderId (FK)
ProductId (FK)
Quantity
UnitPrice (decimal)
TotalPrice (decimal)
Size, Color
CreatedAt, UpdatedAt
IsDeleted
```

### Mối Quan Hệ Chính:

- **User → Orders** (1:N)
- **User → Cart** (1:1)
- **Category → Products** (1:N)
- **Category → Categories** (1:N - Sub-categories)
- **Product → Inventories** (1:N)
- **Product → CartItems** (1:N)
- **Product → OrderItems** (1:N)
- **Cart → CartItems** (1:N)
- **Order → OrderItems** (1:N)

## 🚀 Cài Đặt & Chạy

### Yêu Cầu:

- .NET SDK 8.0 trở lên
- PostgreSQL / Neon
- Visual Studio 2022 (khuyến nghị)

### Các Bước:

1. **Clone dự án**

```bash
git clone <repository-url>
cd fashion-ecommerce-platform
```

2. **Cập nhật Connection String**

Chỉnh sửa [appsettings.json](src/FashionEcommerce.API/appsettings.json):

```json
"ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_NEON_HOST;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
}
```

3. **Khôi phục NuGet packages**

```bash
dotnet restore
```

4. **Tạo migrations và cơ sở dữ liệu**

```bash
dotnet restore
dotnet build
dotnet run --project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```

5. **Chạy ứng dụng**

```bash
dotnet run --project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```

Ứng dụng sẽ khả dụng tại: `https://localhost:5001`

## 📚 Tài Liệu API

### Swagger/OpenAPI

Sau khi chạy ứng dụng, truy cập Swagger UI:

```
https://localhost:5001/swagger/index.html
```

### Endpoints Chính:

#### Products

- `GET /api/products` - Lấy tất cả sản phẩm
- `GET /api/products/{id}` - Lấy sản phẩm theo ID
- `GET /api/products/category/{categoryId}` - Lấy sản phẩm theo danh mục
- `GET /api/products/search?searchTerm=` - Tìm kiếm sản phẩm
- `POST /api/products` - Tạo sản phẩm mới
- `PUT /api/products/{id}` - Cập nhật sản phẩm
- `DELETE /api/products/{id}` - Xóa sản phẩm

#### Categories

- `GET /api/categories` - Lấy tất cả danh mục
- `GET /api/categories/{id}` - Lấy danh mục theo ID
- `POST /api/categories` - Tạo danh mục mới

#### Users

- `POST /api/auth/register` - Đăng ký tài khoản
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/request-password-reset` - Tạo token reset mật khẩu
- `POST /api/auth/reset-password` - Đặt lại mật khẩu

#### Orders

- `GET /api/orders` - Lấy tất cả đơn hàng
- `GET /api/orders/{id}` - Lấy đơn hàng theo ID
- `GET /api/users/{userId}/orders` - Lấy đơn hàng của người dùng
- `POST /api/orders` - Tạo đơn hàng mới
- `PUT /api/orders/{id}` - Cập nhật đơn hàng

#### Cart

- `GET /api/carts/user/{userId}` - Lấy giỏ hàng
- `POST /api/carts/add` - Thêm vào giỏ hàng
- `PUT /api/carts/update` - Cập nhật giỏ hàng
- `DELETE /api/carts/remove` - Xóa khỏi giỏ hàng

## 🔐 Bảo Mật

- JWT Authentication
- CORS configuration
- Hashed passwords
- Soft delete

## 📝 Ghi Chú Phát Triển

### Tiếp Theo:

- [ ] Implement authorization & roles
- [ ] Add validation rules
- [ ] Implement DTOs (Data Transfer Objects)
- [ ] Add unit tests
- [ ] Add integration tests
- [ ] Logging & monitoring
- [ ] Payment integration
- [ ] Email notifications
- [ ] Image upload & storage

### Best Practices Đã Áp Dụng:

- ✅ Clean Architecture
- ✅ Repository Pattern
- ✅ Dependency Injection
- ✅ Entity Framework Core
- ✅ Async/Await
- ✅ Soft Delete Pattern
- ✅ Audit Trails (CreatedAt, UpdatedAt)
- ✅ JWT authentication

## 📞 Liên Hệ & Hỗ Trợ

Nếu có bất kỳ câu hỏi hoặc vấn đề, vui lòng tạo một issue trên GitHub.

---

**Version:** 1.0.0  
**Last Updated:** 2026-05-26
