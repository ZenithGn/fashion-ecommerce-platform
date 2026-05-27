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

### Swagger

`https://localhost:5001/swagger`
