# Fashion Ecommerce Platform

Backend API for a fashion ecommerce project built with ASP.NET Core, Entity Framework Core and PostgreSQL.

## Requirements

- .NET SDK 8.x
- PostgreSQL database, for example Neon Console
- EF Core CLI tool: `dotnet tool install --global dotnet-ef --version 8.0.11`

## Local configuration

Do not commit real database passwords or JWT secrets. Create a local `.env` file or `src/FashionEcommerce.API/appsettings.Development.json` on each machine.

Example `.env`:

```env
ConnectionStrings__DefaultConnection=Host=your-neon-host;Database=neondb;Username=your-user;Password=your-password;SSL Mode=Require;Trust Server Certificate=true
JwtSettings__SecretKey=your-long-secret-key-at-least-32-characters
JwtSettings__Issuer=YourAppName
JwtSettings__Audience=YourAppUsers
EmailSettings__Username=your-email@gmail.com
EmailSettings__Password=your-gmail-app-password
```

Example `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-neon-host;Database=neondb;Username=your-user;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
  },
  "JwtSettings": {
    "SecretKey": "your-long-secret-key-at-least-32-characters",
    "Issuer": "YourAppName",
    "Audience": "YourAppUsers",
    "ExpirationMinutes": 60
  }
}
```

## Run

```bash
dotnet restore
dotnet build
dotnet run --project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```

Swagger is available at `/swagger`.

## Database migrations

Create a migration:

```bash
dotnet ef migrations add MigrationName \
  --project src/FashionEcommerce.Data/FashionEcommerce.Data.csproj \
  --startup-project src/FashionEcommerce.API/FashionEcommerce.API.csproj \
  --output-dir Migrations
```

Apply migrations:

```bash
dotnet ef database update \
  --project src/FashionEcommerce.Data/FashionEcommerce.Data.csproj \
  --startup-project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```
