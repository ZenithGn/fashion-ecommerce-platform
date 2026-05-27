## Cấu hình database local

Project sử dụng PostgreSQL trên Neon. Để chạy backend ở máy cá nhân, mỗi thành viên cần tạo file cấu hình riêng:

```text
src/FashionEcommerce.API/appsettings.Development.json
Nội dung file:

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_NEON_HOST;Port=5432;Database=YOUR_DATABASE_NAME;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
Thay các giá trị sau bằng thông tin lấy từ Neon Console:

YOUR_NEON_HOST
YOUR_DATABASE_NAME
YOUR_USERNAME
YOUR_PASSWORD
Ví dụ:

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-example.ap-southeast-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=your_password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
Lưu ý: không commit file appsettings.Development.json lên GitHub vì file này chứa thông tin kết nối database.

Sau khi tạo file cấu hình, chạy backend bằng lệnh:

dotnet restore
dotnet build
dotnet run --project src/FashionEcommerce.API/FashionEcommerce.API.csproj
Nếu cần cập nhật database theo migration mới nhất:

dotnet ef database update \
  --project src/FashionEcommerce.Data/FashionEcommerce.Data.csproj \
  --startup-project src/FashionEcommerce.API/FashionEcommerce.API.csproj

Nhớ thêm dòng này vào `.gitignore`:

```gitignore
src/FashionEcommerce.API/appsettings.Development.json