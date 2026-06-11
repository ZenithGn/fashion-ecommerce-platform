using FashionEcommerce.Core.Entities;
using System.Collections.Generic;

namespace FashionEcommerce.Services.Models
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal Price { get; set; }
        public CategoryDto? Category { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int AvailableQuantity { get; set; }
        public List<VariantSummaryDto> VariantsSummary { get; set; } = new List<VariantSummaryDto>();
    }

    public class VariantSummaryDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Size { get; set; }
        public decimal? PriceOverride { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }

    public class ProductSearchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal Price { get; set; }
        public CategoryDto? Category { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int AvailableQuantity { get; set; }
    }

    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public CategoryDto? Category { get; set; }
        public List<ImageDto> Images { get; set; } = new List<ImageDto>();
        public List<VariantDetailDto> Variants { get; set; } = new List<VariantDetailDto>();
        public int AvailableQuantity { get; set; }
    }

    public class VariantDetailDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Size { get; set; }
        public decimal Price { get; set; }
        public decimal? PriceOverride { get; set; }
    }

    public class ImageDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsThumbnail { get; set; }
    }

    // DTOs for creating products
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int CategoryId { get; set; }
        public string? SKU { get; set; }
        public string? Brand { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
        public List<CreateProductVariantDto>? Variants { get; set; }
        public List<CreateProductImageDto>? Images { get; set; }
    }

    public class CreateProductVariantDto
    {
        public string SKU { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Size { get; set; }
        public decimal? PriceOverride { get; set; }
    }

    public class CreateProductImageDto
    {
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsThumbnail { get; set; }
    }
}
