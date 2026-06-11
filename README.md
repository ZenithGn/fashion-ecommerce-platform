# Fashion E-Commerce Platform

Dự án là hệ thống backend xây dựng trên nền tảng .NET 8, sử dụng PostgreSQL và Entity Framework Core. Hệ thống cung cấp các chức năng thương mại điện tử cốt lõi như quản lý sản phẩm, giỏ hàng, đặt hàng và xác thực người dùng.

## Cấu trúc dự án

Dự án được thiết kế theo kiến trúc N-Tier phân lớp để dễ dàng bảo trì và mở rộng:

- **src/FashionEcommerce.Core**: Chứa các thực thể (Entities), là đại diện cho các bảng trong cơ sở dữ liệu như User, Product, Order, v.v.
- **src/FashionEcommerce.Data**: Tầng giao tiếp với cơ sở dữ liệu, chứa `FashionEcommerceDbContext` và các file Migrations của Entity Framework Core.
- **src/FashionEcommerce.Services**: Tầng chứa logic nghiệp vụ (Business Logic), bao gồm các đối tượng truyền tải dữ liệu (Models/DTOs), Interface và Services thực thi (Email Service, Order Service...).
- **src/FashionEcommerce.API**: Tầng giao tiếp HTTP (Controllers), làm nhiệm vụ tiếp nhận HTTP request từ phía client, phân giải JWT token và khởi chạy ứng dụng.
- **test/FashionEcommerce.API.Tests**: Chứa các bài kiểm tra tự động (Integration Tests).

## Hướng dẫn cài đặt và chạy dự án

### 1. Yêu cầu hệ thống

- .NET 8 SDK
- Hệ quản trị cơ sở dữ liệu PostgreSQL (hoặc dùng dịch vụ cloud như Neon)

### 2. Cấu hình bảo mật và API Keys

Toàn bộ các thông tin nhạy cảm bao gồm chuỗi kết nối (Connection Strings), JWT Secret, và thông tin tài khoản SMTP gửi Mail đều được lưu trữ bảo mật qua file `.env`. Hệ thống sẽ tự động đọc từ `.env` để ghi đè cấu hình trong `appsettings.json`.

**Bước 1:** Di chuyển vào thư mục dự án API.
**Bước 2:** Copy nội dung từ file mẫu `src/FashionEcommerce.API/.env.example` và tạo một file mới có tên là `.env` tại cùng thư mục `src/FashionEcommerce.API`.
**Bước 3:** Mở file `.env` lên và thay thế bằng các thông tin thật của bạn:
- `ConnectionStrings__DefaultConnection`: Chuỗi kết nối đến cơ sở dữ liệu PostgreSQL.
- `JwtSettings__SecretKey`: Chuỗi bí mật dùng để mã hóa và giải mã JWT token.
- `EmailSettings__FromEmail`, `EmailSettings__Username`, `EmailSettings__Password`: Thông tin SMTP (Ví dụ: Gmail và App Password) để gửi email.

*Lưu ý: File `.env` đã được thiết lập bỏ qua trong `.gitignore` nên sẽ không bị vô tình đẩy lên GitHub.*

### 3. Build và khởi chạy

Thực hiện các lệnh sau tại thư mục gốc của dự án:

Khôi phục thư viện và biên dịch (Build) dự án:
```bash
dotnet restore
dotnet build
```

Khởi chạy ứng dụng:
```bash
dotnet run --project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```

Sau khi ứng dụng khởi chạy thành công, giao diện Swagger UI để test API sẽ có sẵn tại:
`https://localhost:5001/swagger` (hoặc cổng HTTP/HTTPS tương ứng được hiển thị trên Terminal).

### 4. Chạy kiểm thử (Testing)

Để chạy toàn bộ các bài kiểm thử tự động, hãy chạy lệnh sau ở thư mục gốc:
```bash
dotnet test
```
