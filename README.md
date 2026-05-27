## README

Project này là backend .NET 8 cho fashion e-commerce, hiện chạy với PostgreSQL trên Neon.

### Chạy local

```bash
dotnet restore
dotnet build
dotnet run --project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```

### Cấu hình DB

`src/FashionEcommerce.API/appsettings.json` đã có connection string Neon. Nếu muốn dùng DB riêng, tạo `src/FashionEcommerce.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_NEON_HOST;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### API auth

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/request-password-reset`
- `POST /api/auth/reset-password`

### Reset mật khẩu qua email thật

Để gửi mail thật bằng Gmail, cần cấu hình `EmailSettings` với:

- `SmtpHost`: `smtp.gmail.com`
- `SmtpPort`: `587`
- `Username` / `FromEmail`: Gmail gửi mail
- `Password`: Gmail App Password, không dùng mật khẩu đăng nhập thường

Mẫu request reset cho email của bạn:

```json
{
  "email": "khangblue1101@gmail.com"
}
```

Sau khi gọi `POST /api/auth/request-password-reset`, token sẽ được gửi vào inbox email này.

### Swagger

`https://localhost:5001/swagger`
