# Fashion E-Commerce Platform

Backend API for a fashion e-commerce system built with .NET 8, PostgreSQL, and Entity Framework Core.

## Project Structure

- `src/FashionEcommerce.Core`: domain entities.
- `src/FashionEcommerce.Data`: EF Core DbContext and migrations.
- `src/FashionEcommerce.Services`: business services, DTOs, interfaces, and email integration.
- `src/FashionEcommerce.API`: HTTP API controllers and application startup.
- `test/FashionEcommerce.API.Tests`: integration and API tests.

## Configuration

Sensitive settings should be provided through environment variables or `src/FashionEcommerce.API/.env`.
Do not commit real database passwords, JWT secrets, or SMTP credentials.

Common keys:

```text
ConnectionStrings__DefaultConnection=Host=YOUR_HOST;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
JwtSettings__SecretKey=YOUR_LONG_SECRET
EmailSettings__Username=YOUR_SMTP_USERNAME
EmailSettings__Password=YOUR_SMTP_PASSWORD
```

For local-only JSON configuration, create `src/FashionEcommerce.API/appsettings.Development.json` and keep it out of Git.

## Build And Run

```bash
dotnet restore
dotnet build
dotnet run --project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```

Swagger UI is available at `/swagger` after startup.

## Database

Apply migrations with:

```bash
dotnet ef database update \
  --project src/FashionEcommerce.Data/FashionEcommerce.Data.csproj \
  --startup-project src/FashionEcommerce.API/FashionEcommerce.API.csproj
```

## Tests

```bash
dotnet test FashionEcommerce.sln
```
