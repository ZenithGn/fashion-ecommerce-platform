using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

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