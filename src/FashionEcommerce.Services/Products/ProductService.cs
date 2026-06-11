using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly FashionEcommerceDbContext _context;

        public ProductService(FashionEcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ProductListDto>> GetProductsAsync(ProductQueryParameters parameters)
        {
            var page = parameters.Page <= 0 ? 1 : parameters.Page;
            var pageSize = parameters.PageSize <= 0 || parameters.PageSize > 100 ? 20 : parameters.PageSize;

            var query = _context.Products
                .Where(p => !p.IsDeleted && p.IsActive);

            if (parameters.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == parameters.CategoryId.Value);

            if (parameters.MinPrice.HasValue)
                query = query.Where(p => p.Price >= parameters.MinPrice.Value);

            if (parameters.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= parameters.MaxPrice.Value);

            if (!string.IsNullOrWhiteSpace(parameters.Size))
                query = query.Where(p => p.Size == parameters.Size || p.Variants.Any(v => v.Size == parameters.Size));

            if (!string.IsNullOrWhiteSpace(parameters.Color))
                query = query.Where(p => p.Color == parameters.Color || p.Variants.Any(v => v.Color == parameters.Color));

            query = ApplyProductSorting(query, parameters.Sort, parameters.SortBy, parameters.SortDirection);

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
                    AvailableQuantity = p.Inventories.Sum(i => (int?)(i.Quantity - i.ReservedQuantity)) ?? 0,
                    VariantsSummary = p.Variants.Select(v => new VariantSummaryDto
                    {
                        Id = v.Id,
                        SKU = v.SKU,
                        Color = v.Color,
                        Size = v.Size,
                        PriceOverride = v.PriceOverride
                    }).ToList()
                })
                .ToListAsync();

            return new PagedResult<ProductListDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        public async Task<ProductDetailDto?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Where(p => p.Id == productId && !p.IsDeleted)
                .Select(p => ToProductDetailDto(p))
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.IsActive)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<ProductServiceResult<PagedResult<ProductSearchDto>>> SearchProductsAsync(SearchProductQueryParameters parameters)
        {
            var page = parameters.Page <= 0 ? 1 : parameters.Page;
            var pageSize = parameters.PageSize <= 0 || parameters.PageSize > 100 ? 20 : parameters.PageSize;
            var searchTerm = parameters.SearchTerm?.Trim();

            var hasAnySearchCriteria =
                !string.IsNullOrWhiteSpace(searchTerm) ||
                parameters.CategoryId.HasValue ||
                parameters.MinPrice.HasValue ||
                parameters.MaxPrice.HasValue ||
                !string.IsNullOrWhiteSpace(parameters.Size) ||
                !string.IsNullOrWhiteSpace(parameters.Color) ||
                !string.IsNullOrWhiteSpace(parameters.Brand);

            if (!hasAnySearchCriteria)
                return ProductServiceResult<PagedResult<ProductSearchDto>>.Failure(
                    ProductServiceError.Validation,
                    "At least one search condition is required.");

            var query = _context.Products
                .Where(p => !p.IsDeleted && p.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var keyword = $"%{searchTerm}%";
                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, keyword) ||
                    (p.Description != null && EF.Functions.ILike(p.Description, keyword)));
            }

            if (parameters.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == parameters.CategoryId.Value);

            if (parameters.MinPrice.HasValue)
                query = query.Where(p => p.Price >= parameters.MinPrice.Value);

            if (parameters.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= parameters.MaxPrice.Value);

            if (!string.IsNullOrWhiteSpace(parameters.Size))
                query = query.Where(p => p.Size == parameters.Size || p.Variants.Any(v => v.Size == parameters.Size));

            if (!string.IsNullOrWhiteSpace(parameters.Color))
                query = query.Where(p => p.Color == parameters.Color || p.Variants.Any(v => v.Color == parameters.Color));

            if (!string.IsNullOrWhiteSpace(parameters.Brand))
                query = query.Where(p => p.Brand != null && EF.Functions.ILike(p.Brand, $"%{parameters.Brand.Trim()}%"));

            query = ApplyProductSorting(query, parameters.Sort, parameters.SortBy, parameters.SortDirection);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductSearchDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    BasePrice = p.Price,
                    Price = p.Variants.OrderBy(v => v.PriceOverride ?? p.Price).Select(v => v.PriceOverride ?? p.Price).FirstOrDefault(),
                    DiscountPrice = p.DiscountPrice,
                    Brand = p.Brand,
                    Color = p.Color,
                    Size = p.Size,
                    Category = p.Category != null ? new CategoryDto { Id = p.Category.Id, Name = p.Category.Name } : null,
                    ThumbnailUrl = p.Images.OrderByDescending(i => i.IsThumbnail).Select(i => i.ImageUrl).FirstOrDefault(),
                    AvailableQuantity = p.Inventories.Sum(i => (int?)(i.Quantity - i.ReservedQuantity)) ?? 0
                })
                .ToListAsync();

            var result = new PagedResult<ProductSearchDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Items = items
            };

            return ProductServiceResult<PagedResult<ProductSearchDto>>.Success(result);
        }

        public async Task<ProductServiceResult<ProductDetailDto>> CreateProductAsync(CreateProductDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ProductServiceResult<ProductDetailDto>.Failure(ProductServiceError.Validation, "Name is required");

            if (dto.BasePrice <= 0)
                return ProductServiceResult<ProductDetailDto>.Failure(ProductServiceError.Validation, "BasePrice must be greater than 0");

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && !c.IsDeleted);
            if (!categoryExists)
                return ProductServiceResult<ProductDetailDto>.Failure(ProductServiceError.Validation, "CategoryId is invalid");

            var skusInPayload = dto.Variants?
                .Where(v => !string.IsNullOrWhiteSpace(v.SKU))
                .Select(v => v.SKU)
                .ToList() ?? new List<string>();

            var duplicateInPayload = skusInPayload
                .GroupBy(s => s)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .FirstOrDefault();

            if (duplicateInPayload != null)
                return ProductServiceResult<ProductDetailDto>.Failure(ProductServiceError.Conflict, $"Duplicate SKU in request payload: {duplicateInPayload}");

            if (skusInPayload.Any())
            {
                var conflict = await _context.ProductVariants.AnyAsync(v => skusInPayload.Contains(v.SKU));
                if (conflict)
                    return ProductServiceResult<ProductDetailDto>.Failure(ProductServiceError.Conflict, "One or more SKUs already exist");
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.BasePrice,
                DiscountPrice = dto.DiscountPrice,
                CategoryId = dto.CategoryId,
                SKU = dto.SKU,
                Brand = dto.Brand,
                Color = dto.Color,
                Size = dto.Size,
                Material = dto.Material,
                ImageUrl = dto.ImageUrl,
                IsActive = dto.IsActive ?? true,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Images != null)
            {
                foreach (var img in dto.Images)
                {
                    product.Images.Add(new ProductImage { ImageUrl = img.ImageUrl, IsThumbnail = img.IsThumbnail });
                }
            }

            if (dto.Variants != null)
            {
                foreach (var v in dto.Variants)
                {
                    product.Variants.Add(new ProductVariant
                    {
                        SKU = v.SKU,
                        Color = v.Color,
                        Size = v.Size,
                        PriceOverride = v.PriceOverride
                    });
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var created = await GetProductByIdAsync(product.Id);
            return ProductServiceResult<ProductDetailDto>.Success(created!);
        }

        public async Task<ProductServiceResult<Product>> UpdateProductAsync(int id, Product product)
        {
            if (id != product.Id)
                return ProductServiceResult<Product>.Failure(ProductServiceError.Validation, "ID mismatch");

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return ProductServiceResult<Product>.Failure(ProductServiceError.NotFound, "Product not found");

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.DiscountPrice = product.DiscountPrice;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            return ProductServiceResult<Product>.Success(existingProduct);
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return false;

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return true;
        }

        private static IQueryable<Product> ApplyProductSorting(
            IQueryable<Product> query,
            string? legacySort,
            string? sortBy,
            string? sortDirection)
        {
            if (!string.IsNullOrWhiteSpace(legacySort))
            {
                return legacySort switch
                {
                    "price_asc" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "size_asc" => query.OrderBy(p => p.Size ?? p.Variants.OrderBy(v => v.Size).Select(v => v.Size).FirstOrDefault()),
                    "size_desc" => query.OrderByDescending(p => p.Size ?? p.Variants.OrderByDescending(v => v.Size).Select(v => v.Size).FirstOrDefault()),
                    "color_asc" => query.OrderBy(p => p.Color ?? p.Variants.OrderBy(v => v.Color).Select(v => v.Color).FirstOrDefault()),
                    "color_desc" => query.OrderByDescending(p => p.Color ?? p.Variants.OrderByDescending(v => v.Color).Select(v => v.Color).FirstOrDefault()),
                    "newest" => query.OrderByDescending(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.Name),
                };
            }

            var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();
            var isDescending = string.Equals(sortDirection?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);

            return normalizedSortBy switch
            {
                "price" => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "size" => isDescending
                    ? query.OrderByDescending(p => p.Size ?? p.Variants.OrderByDescending(v => v.Size).Select(v => v.Size).FirstOrDefault())
                    : query.OrderBy(p => p.Size ?? p.Variants.OrderBy(v => v.Size).Select(v => v.Size).FirstOrDefault()),
                "color" => isDescending
                    ? query.OrderByDescending(p => p.Color ?? p.Variants.OrderByDescending(v => v.Color).Select(v => v.Color).FirstOrDefault())
                    : query.OrderBy(p => p.Color ?? p.Variants.OrderBy(v => v.Color).Select(v => v.Color).FirstOrDefault()),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                _ => query.OrderBy(p => p.Name),
            };
        }

        private static ProductDetailDto ToProductDetailDto(Product p)
        {
            return new ProductDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                BasePrice = p.Price,
                DiscountPrice = p.DiscountPrice,
                Category = p.Category != null ? new CategoryDto { Id = p.Category.Id, Name = p.Category.Name } : null,
                Images = p.Images.OrderByDescending(i => i.IsThumbnail).Select(i => new ImageDto
                {
                    Id = i.Id,
                    Url = i.ImageUrl,
                    IsThumbnail = i.IsThumbnail
                }).ToList(),
                Variants = p.Variants.Select(v => new VariantDetailDto
                {
                    Id = v.Id,
                    SKU = v.SKU,
                    Color = v.Color,
                    Size = v.Size,
                    PriceOverride = v.PriceOverride,
                    Price = v.PriceOverride ?? p.Price
                }).ToList(),
                AvailableQuantity = p.Inventories.Sum(i => (int?)(i.Quantity - i.ReservedQuantity)) ?? 0
            };
        }
    }
}
