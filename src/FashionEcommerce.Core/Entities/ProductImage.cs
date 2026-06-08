using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionEcommerce.Core.Entities
{
    /// <summary>
    /// ProductImage entity - product images
    /// </summary>
    public class ProductImage : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsThumbnail { get; set; } = false;

        // Navigation
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}