using FashionEcommerce.Core.Entities;

<<<<<<<< HEAD:src/FashionEcommerce.Services/Products/ProductDtos.cs
namespace FashionEcommerce.Services.Products
========
namespace FashionEcommerce.Services.Models
>>>>>>>> 04def7b23c69eb3e9586986d28602e92a2febef4:src/FashionEcommerce.Services/Models/ProductsDtos.cs
{
    public class ProductQueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Sort { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }

    public class SearchProductQueryParameters
    {
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Brand { get; set; }
        public string? Sort { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }

    public class ProductServiceResult<T>
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public ProductServiceError Error { get; set; } = ProductServiceError.None;
        public T? Data { get; set; }

        public static ProductServiceResult<T> Success(T data) => new() { Succeeded = true, Data = data };

        public static ProductServiceResult<T> Failure(ProductServiceError error, string message) =>
            new() { Succeeded = false, Error = error, ErrorMessage = message };
    }

    public enum ProductServiceError
    {
        None,
        Validation,
        NotFound,
        Conflict
    }

    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal Price { get; set; }
        public CategoryDto? Category { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int AvailableQuantity { get; set; }
        public List<VariantSummaryDto> VariantsSummary { get; set; } = new();
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
        public List<T> Items { get; set; } = new();
    }

    public class ProductSearchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string? Brand { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
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
        public List<ImageDto> Images { get; set; } = new();
        public List<VariantDetailDto> Variants { get; set; } = new();
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
