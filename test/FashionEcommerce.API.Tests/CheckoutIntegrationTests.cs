using FashionEcommerce.API.Controllers;
using FashionEcommerce.Services.Models.Auth;
using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace FashionEcommerce.API.Tests
{
    public class CheckoutIntegrationTests : IClassFixture<FashionEcommerceApiFactory>
    {
        private readonly FashionEcommerceApiFactory _factory;

        public CheckoutIntegrationTests(FashionEcommerceApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Full_Checkout_And_Cancellation_IntegrationFlow()
        {
            var client = _factory.CreateClient();

            // 1. Register a new customer
            var registerDto = new RegisterRequest
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = $"jane.doe.{Guid.NewGuid()}@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                PhoneNumber = "0987654321"
            };

            var registerResp = await client.PostAsJsonAsync("/api/auth/register", registerDto);
            registerResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var authResponse = await registerResp.Content.ReadFromJsonAsync<AuthResponse>();
            authResponse.Should().NotBeNull();
            authResponse!.Token.Should().NotBeNullOrEmpty();
            int userId = authResponse.UserId;

            // Set Authorization Header for authenticated requests
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.Token);

            // 2. Directly seed the cart in database for this registered user
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<FashionEcommerceDbContext>();

                // Retrieve the product seeded by TestDataSeeder (ProductId = 100)
                var product = db.Products.First(p => p.Id == 100);
                
                // Get the user's cart (which is automatically created during registration)
                var cart = db.Carts.First(c => c.UserId == userId);

                // Add a CartItem
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = 2,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * 2,
                    CreatedAt = DateTime.UtcNow
                };

                db.CartItems.Add(cartItem);

                cart.ItemCount = 1;
                cart.TotalPrice = cartItem.TotalPrice;
                db.Carts.Update(cart);

                await db.SaveChangesAsync();
            }

            // Verify the inventory before checkout
            int originalReserved = 0;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<FashionEcommerceDbContext>();
                var inventory = db.Inventories.First(i => i.ProductId == 100);
                originalReserved = inventory.ReservedQuantity; // From TestDataSeeder it is 2
            }

            // 3. Perform checkout (place order)
            var checkoutDto = new CreateOrderRequest
            {
                ShippingAddress = "123 Main Street",
                City = "Hanoi",
                Country = "Vietnam",
                PhoneNumber = "0123456789",
                Notes = "Giao hang gio hanh chinh"
            };

            var checkoutResp = await client.PostAsJsonAsync("/api/orders", checkoutDto);
            checkoutResp.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdOrder = await checkoutResp.Content.ReadFromJsonAsync<Order>();
            createdOrder.Should().NotBeNull();
            createdOrder!.UserId.Should().Be(userId);
            createdOrder.Status.Should().Be(OrderStatus.Pending);
            createdOrder.Items.Should().HaveCount(1);
            createdOrder.Items.First().ProductId.Should().Be(100);
            createdOrder.Items.First().Quantity.Should().Be(2);

            // 4. Verify database state after checkout
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<FashionEcommerceDbContext>();

                // Cart should be empty
                var cart = db.Carts.Include(c => c.Items).First(c => c.UserId == userId);
                cart.Items.Should().BeEmpty();
                cart.ItemCount.Should().Be(0);
                cart.TotalPrice.Should().Be(0m);

                // Inventory for Product 100 should have ReservedQuantity increased by 2
                var inventory = db.Inventories.First(i => i.ProductId == 100);
                inventory.ReservedQuantity.Should().Be(originalReserved + 2);
            }

            // 5. Cancel the order and verify inventory is released
            var cancelResp = await client.PostAsJsonAsync($"/api/orders/{createdOrder.Id}/cancel", new { });
            cancelResp.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<FashionEcommerceDbContext>();

                // Order status should be Cancelled
                var order = db.Orders.First(o => o.Id == createdOrder.Id);
                order.Status.Should().Be(OrderStatus.Cancelled);

                // Inventory reserved quantity should decrease back to original
                var inventory = db.Inventories.First(i => i.ProductId == 100);
                inventory.ReservedQuantity.Should().Be(originalReserved);
            }
        }

        [Fact]
        public async Task Checkout_EmptyCart_Returns_BadRequest()
        {
            var client = _factory.CreateClient();

            // Register a user
            var registerDto = new RegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = $"john.doe.{Guid.NewGuid()}@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                PhoneNumber = "0987654321"
            };

            var registerResp = await client.PostAsJsonAsync("/api/auth/register", registerDto);
            var authResponse = await registerResp.Content.ReadFromJsonAsync<AuthResponse>();
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);

            // Checkout immediately (cart is empty)
            var checkoutDto = new CreateOrderRequest
            {
                ShippingAddress = "123 Street",
                PhoneNumber = "0123456789"
            };

            var resp = await client.PostAsJsonAsync("/api/orders", checkoutDto);
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
