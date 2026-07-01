using FashionEcommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using FashionEcommerce.Services.Categories;
using FashionEcommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
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
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
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
                var category = await _categoryService.GetCategoryByIdAsync(id);

                if (category == null)
                    return NotFound("Category not found");

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category");
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
                var subcategories = await _categoryService.GetSubCategoriesAsync(id);
                return Ok(subcategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subcategories");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Category cannot be null");

                var result = await _categoryService.CreateCategoryAsync(dto);
                if (!result.Succeeded)
                    return ToErrorResponse(result);

                return CreatedAtAction(nameof(GetCategoryById), new { id = result.Data!.Id }, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            try
            {
                var result = await _categoryService.UpdateCategoryAsync(id, dto);
                if (!result.Succeeded)
                    return ToErrorResponse(result);

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var deleted = await _categoryService.DeleteCategoryAsync(id);
                if (!deleted)
                    return NotFound("Category not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return StatusCode(500, "Internal server error");
            }
        }

        private ActionResult ToErrorResponse<T>(CategoryServiceResult<T> result)
        {
            return result.Error switch
            {
                CategoryServiceError.NotFound => NotFound(result.ErrorMessage),
                CategoryServiceError.Validation => BadRequest(result.ErrorMessage),
                _ => StatusCode(500, result.ErrorMessage ?? "Internal server error")
            };
        }
    }
}
