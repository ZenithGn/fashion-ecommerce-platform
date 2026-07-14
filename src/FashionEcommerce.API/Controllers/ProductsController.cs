using FashionEcommerce.Core.Entities;
using FashionEcommerce.Services.Interfaces;
using FashionEcommerce.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAllProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? size = null,
            [FromQuery] string? color = null,
            [FromQuery] string? sort = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null)
        {
            try
            {
                var result = await _productService.GetProductsAsync(new ProductQueryParameters
                {
                    Page = page,
                    PageSize = pageSize,
                    CategoryId = categoryId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Size = size,
                    Color = color,
                    Sort = sort,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                });

                var etag = $"\"{result.TotalItems}-{result.Page}-{result.PageSize}\"";
                if (Request.Headers.TryGetValue("If-None-Match", out var clientEtag) && clientEtag == etag)
                    return StatusCode(304);

                Response.Headers["ETag"] = etag;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get paged products and sort by price, size, color, name, or newest
        /// </summary>
        [HttpGet("paged-sort")]
        public async Task<ActionResult> GetPagedAndSortedProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? size = null,
            [FromQuery] string? color = null)
        {
            try
            {
                var result = await _productService.GetProductsAsync(new ProductQueryParameters
                {
                    Page = page,
                    PageSize = pageSize,
                    CategoryId = categoryId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Size = size,
                    Color = color,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged and sorted products");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get product by id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                    return NotFound("Product not found");

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = (await _productService.GetProductsByCategoryAsync(categoryId)).ToList();

                if (!products.Any())
                    return NotFound("No products found for this category");

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Advanced product search by name, description and filters
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult> SearchProducts(
            [FromQuery] string? searchTerm = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? size = null,
            [FromQuery] string? color = null,
            [FromQuery] string? brand = null,
            [FromQuery] string? sort = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null)
        {
            try
            {
                var result = await _productService.SearchProductsAsync(new SearchProductQueryParameters
                {
                    SearchTerm = searchTerm,
                    Page = page,
                    PageSize = pageSize,
                    CategoryId = categoryId,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Size = size,
                    Color = color,
                    Brand = brand,
                    Sort = sort,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                });

                if (!result.Succeeded)
                    return ToErrorResponse(result);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Product cannot be null");

                var result = await _productService.CreateProductAsync(dto);
                if (!result.Succeeded)
                    return ToErrorResponse(result);

                return CreatedAtAction(nameof(GetProductById), new { id = result.Data!.Id }, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                var result = await _productService.UpdateProductAsync(id, product);
                if (!result.Succeeded)
                    return ToErrorResponse(result);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var deleted = await _productService.DeleteProductAsync(id);
                if (!deleted)
                    return NotFound("Product not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return StatusCode(500, "Internal server error");
            }
        }

        private ActionResult ToErrorResponse<T>(ProductServiceResult<T> result)
        {
            return result.Error switch
            {
                ProductServiceError.NotFound => NotFound(result.ErrorMessage),
                ProductServiceError.Conflict => Conflict(result.ErrorMessage),
                ProductServiceError.Validation => BadRequest(result.ErrorMessage),
                _ => StatusCode(500, result.ErrorMessage ?? "Internal server error")
            };
        }
    }
}
