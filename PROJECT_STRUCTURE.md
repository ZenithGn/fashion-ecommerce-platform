# Fashion E-Commerce Platform - .NET Backend

Một nền tảng thương mại điện tử thời trang được xây dựng bằng .NET với một kiến trúc sạch (Clean Architecture) và module hóa.

## 📋 Mục Lục

- [Tổng Quan](#tổng-quan)
- [Cấu Trúc Dự Án](#cấu-trúc-dự-án)
- [Các Module Chính](#các-module-chính)
- [Thiết Kế Cơ Sở Dữ Liệu](#thiết-kế-cơ-sở-dữ-liệu)
- [Cài Đặt & Chạy](#cài-đặt--chạy)
- [Tài Liệu API](#tài-liệu-api)
- [Bảo Mật & Phân Quyền (RBAC)](#bảo-mật--phân-quyền-rbac)

---

## 🎯 Tổng Quan

Fashion E-Commerce Platform là một backend được xây dựng hoàn toàn bằng **.NET 8.0** với **Entity Framework Core**, cung cấp các API RESTful bảo mật và hiệu năng cao cho hệ thống thương mại điện tử thời trang.

**Công nghệ sử dụng:**
- .NET 8.0 (Web API)
- Entity Framework Core 8.0 (ORM)
- PostgreSQL / Neon Database
- JWT Authentication & Role-Based Access Control (RBAC)
- MailKit / SMTP Email Sender
- Swagger / OpenAPI cho tài liệu API

---

## 📁 Cấu Trúc Dự Án

```
fashion-ecommerce-platform/
├── FashionEcommerce.sln                    # Solution file chính
├── src/
│   ├── FashionEcommerce.API/              # ASP.NET Core Web API (Presentation Layer)
│   │   ├── Controllers/                   # Các API Controller xử lý Request
│   │   │   ├── AuthController.cs          # Đăng ký, Đăng nhập, Reset mật khẩu
│   │   │   ├── CartController.cs          # Quản lý giỏ hàng
│   │   │   ├── CategoriesController.cs    # Quản lý danh mục sản phẩm
│   │   │   ├── DashboardController.cs     # Thống kê doanh thu, tồn kho, người dùng
│   │   │   ├── InventoriesController.cs   # Quản lý kho hàng & kiểm tra tồn kho
│   │   │   ├── OrdersController.cs        # Tạo và xử lý đơn hàng
│   │   │   ├── PermissionsController.cs   # Quản lý quyền hệ thống (Actions)
│   │   │   ├── ProductsController.cs      # Quản lý sản phẩm & biến thể
│   │   │   ├── RolesController.cs         # Quản lý phân vai trò
│   │   │   ├── ShipmentsController.cs     # Điều hành vận chuyển & lịch trình
│   │   │   ├── UserAddressesController.cs # Sổ địa chỉ giao hàng
│   │   │   └── UsersController.cs         # Quản lý người dùng hệ thống
│   │   ├── Program.cs                     # Cấu hình khởi động dịch vụ, Middleware, DB
│   │   ├── appsettings.json               # Cấu hình hệ thống (Database, JWT, Email)
│   │   └── wwwroot/                       # Static files & Web UI Dashboard nội bộ
│   │
│   ├── FashionEcommerce.Core/             # Domain Layer (Thực thể & Logic cốt lõi)
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs             # Thực thể cơ sở (Id, CreatedAt, UpdatedAt, IsDeleted)
│   │   │   ├── User.cs                   # Thông tin tài khoản người dùng
│   │   │   ├── UserAddress.cs            # Địa chỉ giao hàng của người dùng
│   │   │   ├── Role.cs                   # Vai trò hệ thống (Admin, Customer, Staff, Manager)
│   │   │   ├── Permission.cs             # Quyền chi tiết (Ví dụ: products.manage)
│   │   │   ├── RolePermission.cs         # Thực thể trung gian Many-to-Many giữa Role và Permission
│   │   │   ├── Product.cs                # Thông tin sản phẩm cơ bản
│   │   │   ├── ProductVariant.cs         # Biến thể sản phẩm (Kích thước, Màu sắc, Giá ghi đè)
│   │   │   ├── ProductImage.cs           # Hình ảnh chi tiết cho sản phẩm
│   │   │   ├── Category.cs               # Danh mục sản phẩm (Hỗ trợ cấu trúc Cha-Con)
│   │   │   ├── Inventory.cs              # Tồn kho sản phẩm, số lượng giữ trước (Reserved)
│   │   │   ├── Cart.cs                   # Giỏ hàng của người dùng (1-1)
│   │   │   ├── CartItem.cs               # Chi tiết từng sản phẩm trong giỏ hàng
│   │   │   ├── Order.cs                  # Thông tin đơn mua hàng
│   │   │   ├── OrderItem.cs              # Chi tiết các mặt hàng trong đơn mua
│   │   │   └── Shipment.cs               # Thông tin giao nhận vận đơn
│   │   │   └── ShipmentEvent.cs          # Lịch trình chi tiết quá trình vận chuyển
│   │
│   ├── FashionEcommerce.Data/             # Data Infrastructure Layer
│   │   ├── FashionEcommerceDbContext.cs  # EF Core DbContext, Fluent API & Seed Data
│   │   └── Migrations/                   # Lịch sử EF Core Migrations
│   │
│   └── FashionEcommerce.Services/         # Business Logic Layer (Ứng dụng & Dịch vụ)
│       ├── Interfaces/                   # Các Interface dịch vụ định nghĩa nghiệp vụ
│       ├── Repositories/                 # Repository Pattern dùng chung (Generic Repository)
│       ├── Products/                     # Nghiệp vụ liên quan đến Product & Search
│       ├── Email/                        # Nghiệp vụ gửi mail (SMTP)
│       └── Services/                     # Các Service triển khai nghiệp vụ cụ thể
│
└── test/
    └── FashionEcommerce.API.Tests/        # Thư mục chứa các Integration Tests
        ├── FashionEcommerceApiFactory.cs  # Khởi tạo WebApplicationFactory cho test
        ├── CheckoutIntegrationTests.cs    # Test luồng mua hàng và hủy đơn hàng
        ├── ProductsControllerTests.cs     # Test quản lý sản phẩm, hình ảnh & biến thể
        └── SearchTests.cs                 # Test tìm kiếm sản phẩm phân trang
```

---

## 🏗️ Các Module Chính

### 1. **User & Authentication (Người dùng & Xác thực)**
- **Đăng ký & Đăng nhập:** Xác thực người dùng bằng JWT Token.
- **Quản lý phân quyền (RBAC):** Người dùng được liên kết với một `Role`. Mỗi `Role` chứa danh sách các `Permission` (quyền hạn chi tiết) để kiểm soát quyền truy cập API.
- **Sổ địa chỉ (`UserAddress`):** Mỗi người dùng được thêm nhiều địa chỉ nhận hàng, có cờ đánh dấu địa chỉ mặc định (`IsDefault`).

### 2. **Product & Categories (Sản phẩm & Danh mục)**
- **Sản phẩm chính (`Product`):** Quản lý các thuộc tính chung như thương hiệu, chất liệu, rating, danh mục.
- **Biến thể (`ProductVariant`):** Quản lý chi tiết theo SKU, màu sắc, kích thước, và cho phép ghi đè giá bán riêng cho biến thể (`PriceOverride`).
- **Hình ảnh (`ProductImage`):** Hỗ trợ nhiều ảnh cho một sản phẩm, đánh dấu ảnh đại diện (`IsThumbnail`).
- **Danh mục (`Category`):** Tổ chức phân cấp nhiều tầng (Cha-Con) thông qua thuộc tính `ParentCategoryId`.

### 3. **Inventory & Stock Management (Kho hàng)**
- Kiểm tra số lượng tồn kho vật lý (`Quantity`) và số lượng đang bị giữ tạm do khách đặt hàng nhưng chưa thanh toán/giao (`ReservedQuantity`).
- Số lượng hàng thực tế có sẵn bán: `AvailableQuantity = Quantity - ReservedQuantity`.
- Cảnh báo khi kho xuống dưới ngưỡng an toàn (Low Stock Alerts).

### 4. **Cart & Checkout (Giỏ hàng & Đặt hàng)**
- Quản lý thêm/sửa/xóa sản phẩm trong giỏ hàng (`Cart` & `CartItem`). Tính toán tự động tổng tiền và số lượng item.
- Khi Checkout: Thực hiện trừ tạm vào kho (`ReservedQuantity`), chuyển thông tin giỏ hàng thành đơn hàng `Order` và các `OrderItem`, sau đó làm sạch giỏ hàng.

### 5. **Order & Shipment (Đơn hàng & Giao hàng)**
- Theo dõi vòng đời đơn hàng qua trạng thái: `Pending` -> `Processing` -> `Shipped` -> `Delivered` -> `Cancelled`.
- **Logistics (`Shipment`):** Tạo vận đơn, liên kết đơn vị vận chuyển (`CarrierName`), mã vận đơn (`TrackingNumber`), phí giao hàng.
- **Lịch trình vận đơn (`ShipmentEvent`):** Cập nhật trạng thái chi tiết theo thời gian thực (Ví dụ: `Created` -> `Packing` -> `InTransit` -> `OutForDelivery` -> `Delivered`).

---

## 🗄️ Thiết Thiết Kế Cơ Sở Dữ Liệu

### Sơ đồ các bảng quan trọng:

#### Roles (Vai trò)
- `Id` (PK), `RoleName` (Unique), `Description`

#### Permissions (Quyền hạn)
- `Id` (PK), `ActionName` (Unique), `Description`

#### RolePermissions (Bảng trung gian)
- `RoleId` (FK), `PermissionId` (FK) -> Composite PK

#### Users (Tài khoản)
- `Id` (PK), `RoleId` (FK), `FirstName`, `LastName`, `Email` (Unique), `PasswordHash`, `PhoneNumber`, `IsActive`, `LastLoginAt`

#### UserAddresses (Sổ địa chỉ)
- `Id` (PK), `UserId` (FK), `ReceiverName`, `Phone`, `AddressLine`, `Ward`, `District`, `Province`, `IsDefault`

#### Products (Sản phẩm)
- `Id` (PK), `CategoryId` (FK), `Name`, `Description`, `Price`, `DiscountPrice`, `SKU` (Unique), `Brand`, `Material`, `Rating`, `IsActive`

#### ProductVariants (Biến thể sản phẩm)
- `Id` (PK), `ProductId` (FK), `SKU` (Unique), `Color`, `Size`, `PriceOverride`

#### ProductImages (Thư viện ảnh)
- `Id` (PK), `ProductId` (FK), `ImageUrl`, `IsThumbnail`

#### Carts (Giỏ hàng)
- `Id` (PK), `UserId` (FK, Unique), `TotalPrice`, `ItemCount`

#### Orders (Đơn hàng)
- `Id` (PK), `UserId` (FK), `OrderNumber` (Unique), `Status`, `SubTotal`, `ShippingCost`, `TotalPrice`, `ShippingAddress`, `PhoneNumber`

#### Shipments (Vận chuyển)
- `Id` (PK), `OrderId` (FK, Unique), `CarrierName`, `TrackingNumber`, `Status`, `ShippingFee`, `ShippedAt`, `DeliveredAt`

#### ShipmentEvents (Sự kiện hành trình)
- `Id` (PK), `ShipmentId` (FK), `Status`, `Location`, `Note`, `OccurredAt`

---

## 🚀 Cài Đặt & Chạy

### Yêu Cầu:
- .NET SDK 8.0 trở lên
- PostgreSQL / Neon Database (hoặc Docker chạy Postgres)

### Các Bước Thực Hiện:

1. **Clone dự án & Khôi phục packages:**
```bash
git clone <repository-url>
cd fashion-ecommerce-platform
dotnet restore
```

2. **Cấu hình biến môi trường:**
Tạo file `src/FashionEcommerce.API/.env` hoặc cấu hình trong `appsettings.json`:
```text
ConnectionStrings__DefaultConnection=Host=YOUR_DB_HOST;Database=YOUR_DB_NAME;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
JwtSettings__SecretKey=YourSuperSecretKeyMustBeVeryLongAndSecureHere
```

3. **Chạy Migration cập nhật Database:**
```bash
dotnet ef database update \
  --project src/FashionEcommerce.Data/FashionEcommerce.Data.csproj \
  --startup-project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```

4. **Chạy ứng dụng:**
```bash
dotnet run --project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```
API Swagger sẽ hoạt động tại: `http://localhost:5000/swagger/index.html` (hoặc cổng https được cấu hình).

---

## 📚 Tài Liệu API

### Endpoints Chính:

#### 🔐 Auth & Phân quyền
- `POST /api/auth/register` - Đăng ký tài khoản mới (Mặc định Role: Customer)
- `POST /api/auth/login` - Đăng nhập nhận JWT Token
- `GET /api/roles` & `POST /api/roles` - Quản lý Vai trò (Admin)
- `GET /api/permissions` - Lấy danh sách các Permission hiện có (Admin)

#### 🛍️ Products (Sản phẩm & Biến thể)
- `GET /api/products` - Lấy danh sách sản phẩm (có hỗ trợ phân trang)
- `GET /api/products/{id}` - Xem chi tiết sản phẩm bao gồm danh sách biến thể & hình ảnh
- `GET /api/products/search?searchTerm=` - Tìm kiếm sản phẩm
- `POST /api/products` - Tạo sản phẩm cùng các biến thể, hình ảnh đi kèm (Admin/Manager)
- `PUT /api/products/{id}` & `DELETE /api/products/{id}` - Cập nhật/Xóa sản phẩm (Admin)

#### 📦 Shipments (Vận chuyển)
- `GET /api/shipments` - Lấy danh sách vận đơn (Lọc theo trạng thái, nhà vận chuyển)
- `GET /api/shipments/{id}/events` - Lấy toàn bộ hành trình sự kiện giao hàng
- `POST /api/shipments` - Tạo mới vận đơn giao hàng cho Order (Staff/Manager/Admin)
- `PUT /api/shipments/{id}/status` - Cập nhật trạng thái vận đơn & chèn lịch trình sự kiện giao hàng mới

#### 🛒 Cart & Checkout
- `GET /api/carts/user/{userId}` - Xem giỏ hàng của User
- `POST /api/carts/add` - Thêm hàng vào giỏ
- `POST /api/orders` - Checkout chuyển giỏ hàng thành Order

---

## 🔐 Bảo Mật & Phân Quyền (RBAC)

Hệ thống triển khai phân quyền động thông qua bảng `RolePermissions`. Một số hành động bắt buộc kiểm tra quyền:
1. **Quyền hạn chi tiết (`Permission`):**
   - `products.manage` -> Quản lý sản phẩm.
   - `orders.manage` -> Xử lý đơn hàng.
   - `roles.manage` -> Quản lý vai trò & quyền hạn hệ thống.
   - `dashboard.view` -> Xem báo cáo doanh thu quản trị.
2. **Quyền sở hữu tài nguyên (Resource Ownership):**
   - Một `Customer` chỉ được phép xem/sửa địa chỉ (`UserAddress`) hoặc đơn hàng (`Order`) của chính họ. Hệ thống so khớp `ClaimTypes.NameIdentifier` của Token với `UserId` trong database trước khi phản hồi.

**Document Version:** 2.0  
**Last Updated:** 2026-07-01  
