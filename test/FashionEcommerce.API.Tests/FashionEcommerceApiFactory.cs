using FashionEcommerce.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FashionEcommerce.API.Tests
{
    public class FashionEcommerceApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"FashionEcommerceTestDb_{Guid.NewGuid()}";

        public FashionEcommerceApiFactory()
        {
            Environment.SetEnvironmentVariable("JwtSettings__SecretKey", "test-jwt-secret-key-with-at-least-32-characters");
            Environment.SetEnvironmentVariable("JwtSettings__Issuer", "FashionEcommerce.Tests");
            Environment.SetEnvironmentVariable("JwtSettings__Audience", "FashionEcommerce.Tests");
            Environment.SetEnvironmentVariable("JwtSettings__ExpirationMinutes", "60");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "test-jwt-secret-key-with-at-least-32-characters",
                    ["JwtSettings:Issuer"] = "FashionEcommerce.Tests",
                    ["JwtSettings:Audience"] = "FashionEcommerce.Tests",
                    ["JwtSettings:ExpirationMinutes"] = "60"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<FashionEcommerceDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for tests
                services.AddDbContext<FashionEcommerceDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName)
                           .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });

                // Build the service provider and seed data
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<FashionEcommerceDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    // Seed minimal data for tests
                    TestDataSeeder.Seed(db);
                }
            });
        }
    }
}
