using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionEcommerce.Core.Entities
{
    /// <summary>
    /// ProductVariant entity - represents SKU level variations
    /// </summary>
    public class ProductVariant : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string SKU { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PriceOverride { get; set; }

        // Navigation
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}