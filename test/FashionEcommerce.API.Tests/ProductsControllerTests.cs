using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FashionEcommerce.API.Controllers;
using FashionEcommerce.Services.Models;

namespace FashionEcommerce.API.Tests
{
    public class ProductsControllerTests : IClassFixture<FashionEcommerceApiFactory>
    {
        private readonly FashionEcommerceApiFactory _factory;

        public ProductsControllerTests(FashionEcommerceApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateProduct_Success_With_Variants_And_Images()
        {
            var client = _factory.CreateClient();
            var dto = new CreateProductDto
            {
                Name = "New Product",
                BasePrice = 123m,
                CategoryId = 1,
                Variants = new System.Collections.Generic.List<CreateProductVariantDto>
                {
                    new CreateProductVariantDto { SKU = "NEW-1", Size = "M" }
                },
                Images = new System.Collections.Generic.List<CreateProductImageDto>
                {
                    new CreateProductImageDto { ImageUrl = "http://example.com/new.jpg", IsThumbnail = true }
                }
            };

            var resp = await client.PostAsJsonAsync("/api/products", dto);
            resp.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await resp.Content.ReadFromJsonAsync<ProductDetailDto>();
            created.Should().NotBeNull();
            created!.Name.Should().Be("New Product");
            created.Variants.Should().HaveCount(1);
            created.Images.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateProduct_InvalidCategory_Returns_BadRequest()
        {
            var client = _factory.CreateClient();
            var dto = new CreateProductDto
            {
                Name = "Bad Cat",
                BasePrice = 10m,
                CategoryId = 9999
            };

            var resp = await client.PostAsJsonAsync("/api/products", dto);
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateProduct_DuplicateSKUInPayload_Returns_Conflict()
        {
            var client = _factory.CreateClient();
            var dto = new CreateProductDto
            {
                Name = "Dup SKU",
                BasePrice = 10m,
                CategoryId = 1,
                Variants = new System.Collections.Generic.List<CreateProductVariantDto>
                {
                    new CreateProductVariantDto { SKU = "DUP-1" },
                    new CreateProductVariantDto { SKU = "DUP-1" }
                }
            };

            var resp = await client.PostAsJsonAsync("/api/products", dto);
            resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task GetProductById_Returns_ProductDetail_Success()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/products/100");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await resp.Content.ReadFromJsonAsync<ProductDetailDto>();
            dto.Should().NotBeNull();
            dto!.Id.Should().Be(100);
            dto.Variants.Should().NotBeEmpty();
            dto.Images.Should().NotBeEmpty();
            dto.AvailableQuantity.Should().Be(8); // 10 - 2

            // price resolution: variant price override used per variant
            dto.Variants[0].PriceOverride.Should().Be(120m);
            dto.Variants[0].Price.Should().Be(120m);
        }

        [Fact]
        public async Task GetProductById_Returns_404_For_Missing()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/products/9999");
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetProductById_ProductWithoutImage_Returns_EmptyImages()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/products/101");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await resp.Content.ReadFromJsonAsync<ProductDetailDto>();
            dto.Should().NotBeNull();
            dto!.Images.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProductById_ProductWithoutVariants_Returns_EmptyVariants_And_BasePrice()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/products/102");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var dto = await resp.Content.ReadFromJsonAsync<ProductDetailDto>();
            dto.Should().NotBeNull();
            dto!.Variants.Should().BeEmpty();
            // No variants: variants list empty and price should be base price for any variant returns
            dto.BasePrice.Should().Be(75m);
        }
    }
}