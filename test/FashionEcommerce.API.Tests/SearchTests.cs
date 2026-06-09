using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FashionEcommerce.API.Tests
{
    public class SearchTests : IClassFixture<FashionEcommerceApiFactory>
    {
        private readonly FashionEcommerceApiFactory _factory;

        public SearchTests(FashionEcommerceApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Search_Returns_PagedResults_With_Thumbnail_And_Price()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/products/search?searchTerm=Product&page=1&pageSize=10");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await resp.Content.ReadFromJsonAsync<PagedResult<ProductSearchDto>>();
            result.Should().NotBeNull();
            result!.Items.Should().NotBeEmpty();

            var first = result.Items[0];
            first.ThumbnailUrl.Should().NotBeNull();
            first.Price.Should().BeGreaterThanOrEqualTo(0);
            result.TotalItems.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Search_EmptyTerm_Returns_BadRequest()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/products/search?searchTerm=");
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}