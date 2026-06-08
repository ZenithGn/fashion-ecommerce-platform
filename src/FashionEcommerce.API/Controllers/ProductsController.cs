using FashionEcommerce.Data;
using FashionEcommerce.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly FashionEcommerceDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(FashionEcommerceDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] int? categoryId = null, [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? size = null, [FromQuery] string? color = null, [FromQuery] string? sort = null)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0 || pageSize > 100) pageSize = 20;

                // Base query
                var query = _context.Products
                    .Where(p => !p.IsDeleted && p.IsActive);

                if (categoryId.HasValue)
                    query = query.Where(p => p.CategoryId == categoryId.Value);

                if (minPrice.HasValue)
                    query = query.Where(p => p.Price >= minPrice.Value);
                if (maxPrice.HasValue)
                    query = query.Where(p => p.Price <= maxPrice.Value);

                if (!string.IsNullOrWhiteSpace(size))
                    query = query.Where(p => p.Size == size || p.Variants.Any(v => v.Size == size));
                if (!string.IsNullOrWhiteSpace(color))
                    query = query.Where(p => p.Color == color || p.Variants.Any(v => v.Color == color));

                // Sorting
                query = sort switch
                {
                    "price_asc" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "newest" => query.OrderByDescending(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.Name),
                };

                var totalItems = await query.CountAsync();

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductListDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        BasePrice = p.Price,
                        Price = p.Variants.OrderBy(v => v.PriceOverride ?? p.Price).Select(v => v.PriceOverride ?? p.Price).FirstOrDefault(),
                        Category = p.Category != null ? new CategoryDto { Id = p.Category.Id, Name = p.Category.Name } : null,
                        ThumbnailUrl = p.Images.OrderByDescending(i => i.IsThumbnail).Select(i => i.ImageUrl).FirstOrDefault(),
                        AvailableQuantity = p.Inventories.Sum(i => (int?)i.AvailableQuantity) ?? 0,
                        VariantsSummary = p.Variants.Select(v => new VariantSummaryDto { Id = v.Id, SKU = v.SKU, Color = v.Color, Size = v.Size, PriceOverride = v.PriceOverride }).ToList()
                    })
                    .ToListAsync();

                var result = new PagedResult<ProductListDto>
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = items
                };

                // Simple ETag based on last modified data
                var etag = $"\"{result.TotalItems}-{result.Page}-{result.PageSize}\"";
                if (Request.Headers.TryGetValue("If-None-Match", out var clientEtag) && clientEtag == etag)
                    return StatusCode(304);

                Response.Headers["ETag"] = etag;
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting products: {ex.Message}");
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
                var product = await _context.Products
                    .Where(p => p.Id == id && !p.IsDeleted)
                    .Select(p => new ProductDetailDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        BasePrice = p.Price,
                        DiscountPrice = p.DiscountPrice,
                        Category = p.Category != null ? new CategoryDto { Id = p.Category.Id, Name = p.Category.Name } : null,
                        Images = p.Images.OrderByDescending(i => i.IsThumbnail).Select(i => new ImageDto { Id = i.Id, Url = i.ImageUrl, IsThumbnail = i.IsThumbnail }).ToList(),
                        Variants = p.Variants.Select(v => new VariantDetailDto
                        {
                            Id = v.Id,
                            SKU = v.SKU,
                            Color = v.Color,
                            Size = v.Size,
                            PriceOverride = v.PriceOverride,
                            Price = v.PriceOverride ?? p.Price
                        }).ToList(),
                        AvailableQuantity = p.Inventories.Sum(i => (int?)i.AvailableQuantity) ?? 0
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                    return NotFound("Product not found");

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting product: {ex.Message}");
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
                var products = await _context.Products
                    .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.IsActive)
                    .Include(p => p.Category)
                    .ToListAsync();

                if (!products.Any())
                    return NotFound("No products found for this category");

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting products by category: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Search products by name or description
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult> SearchProducts([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequest("Search term cannot be empty");

                if (page <= 0) page = 1;
                if (pageSize <= 0 || pageSize > 100) pageSize = 20;

                var baseQuery = _context.Products
                    .Where(p => (p.Name.Contains(searchTerm) || p.Description!.Contains(searchTerm))
                              && !p.IsDeleted && p.IsActive);

                var total = await baseQuery.CountAsync();

                var items = await baseQuery
                    .OrderBy(p => p.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductSearchDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        BasePrice = p.Price,
                        Price = p.Variants.OrderBy(v => v.PriceOverride ?? p.Price).Select(v => v.PriceOverride ?? p.Price).FirstOrDefault(),
                        Category = p.Category != null ? new CategoryDto { Id = p.Category.Id, Name = p.Category.Name } : null,
                        ThumbnailUrl = p.Images.OrderByDescending(i => i.IsThumbnail).Select(i => i.ImageUrl).FirstOrDefault(),
                        AvailableQuantity = p.Inventories.Sum(i => (int?)i.AvailableQuantity) ?? 0
                    })
                    .ToListAsync();

                var result = new PagedResult<ProductSearchDto>
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = total,
                    Items = items
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching products: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            try
            {
                if (product == null)
                    return BadRequest("Product cannot be null");

                product.CreatedAt = DateTime.UtcNow;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating product: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                if (id != product.Id)
                    return BadRequest("ID mismatch");

                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                    return NotFound("Product not found");

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.DiscountPrice = product.DiscountPrice;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                return Ok(existingProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound("Product not found");

                product.IsDeleted = true;
                product.UpdatedAt = DateTime.UtcNow;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
