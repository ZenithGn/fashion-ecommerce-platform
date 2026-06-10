using FashionEcommerce.Data;
using FashionEcommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly FashionEcommerceDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(FashionEcommerceDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => !c.IsDeleted && c.IsActive)
                    .Include(c => c.SubCategories)
                    .ToListAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting categories: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get category by id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Where(c => c.Id == id && !c.IsDeleted)
                    .Include(c => c.Products)
                    .Include(c => c.SubCategories)
                    .FirstOrDefaultAsync();

                if (category == null)
                    return NotFound("Category not found");

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting category: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get subcategories by parent category id
        /// </summary>
        [HttpGet("{id}/subcategories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetSubcategories(int id)
        {
            try
            {
                var subcategories = await _context.Categories
                    .Where(c => c.ParentCategoryId == id && !c.IsDeleted && c.IsActive)
                    .ToListAsync();

                return Ok(subcategories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting subcategories: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
        {
            try
            {
                if (category == null)
                    return BadRequest("Category cannot be null");

                if (string.IsNullOrWhiteSpace(category.Name))
                    return BadRequest("Category name is required");

                category.CreatedAt = DateTime.UtcNow;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating category: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            try
            {
                if (id != category.Id)
                    return BadRequest("ID mismatch");

                var existingCategory = await _context.Categories.FindAsync(id);
                if (existingCategory == null)
                    return NotFound("Category not found");

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.ImageUrl = category.ImageUrl;
                existingCategory.IsActive = category.IsActive;
                existingCategory.UpdatedAt = DateTime.UtcNow;

                _context.Categories.Update(existingCategory);
                await _context.SaveChangesAsync();

                return Ok(existingCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating category: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound("Category not found");

                category.IsDeleted = true;
                category.UpdatedAt = DateTime.UtcNow;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting category: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
