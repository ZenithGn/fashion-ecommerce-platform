using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly FashionEcommerceDbContext _context;

        public CategoryService(FashionEcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted && c.IsActive)
                .Include(c => c.SubCategories)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories
                .Where(c => c.Id == categoryId && !c.IsDeleted)
                .Include(c => c.Products)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentCategoryId)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentCategoryId && !c.IsDeleted && c.IsActive)
                .ToListAsync();
        }

        public async Task<CategoryServiceResult<Category>> CreateCategoryAsync(CreateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return CategoryServiceResult<Category>.Failure(CategoryServiceError.Validation, "Category name is required");

            if (dto.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories.AnyAsync(c => c.Id == dto.ParentCategoryId.Value && !c.IsDeleted);
                if (!parentExists)
                    return CategoryServiceResult<Category>.Failure(CategoryServiceError.Validation, "Parent category is invalid");
            }

            var category = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                ParentCategoryId = dto.ParentCategoryId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CategoryServiceResult<Category>.Success(category);
        }

        public async Task<CategoryServiceResult<Category>> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return CategoryServiceResult<Category>.Failure(CategoryServiceError.Validation, "Category name is required");

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
                return CategoryServiceResult<Category>.Failure(CategoryServiceError.NotFound, "Category not found");

            if (dto.ParentCategoryId.HasValue && dto.ParentCategoryId.Value == id)
                return CategoryServiceResult<Category>.Failure(CategoryServiceError.Validation, "Category cannot be its own parent");

            if (dto.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories.AnyAsync(c => c.Id == dto.ParentCategoryId.Value && !c.IsDeleted);
                if (!parentExists)
                    return CategoryServiceResult<Category>.Failure(CategoryServiceError.Validation, "Parent category is invalid");
            }

            existingCategory.Name = dto.Name.Trim();
            existingCategory.Description = dto.Description;
            existingCategory.ImageUrl = dto.ImageUrl;
            existingCategory.ParentCategoryId = dto.ParentCategoryId;
            existingCategory.IsActive = dto.IsActive;
            existingCategory.UpdatedAt = DateTime.UtcNow;

            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();

            return CategoryServiceResult<Category>.Success(existingCategory);
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
                return false;

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return true;
        }
    }

    public class CategoryServiceResult<T>
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public CategoryServiceError Error { get; set; } = CategoryServiceError.None;
        public T? Data { get; set; }

        public static CategoryServiceResult<T> Success(T data) => new() { Succeeded = true, Data = data };

        public static CategoryServiceResult<T> Failure(CategoryServiceError error, string message) =>
            new() { Succeeded = false, Error = error, ErrorMessage = message };
    }

    public enum CategoryServiceError
    {
        None,
        Validation,
        NotFound
    }
}
