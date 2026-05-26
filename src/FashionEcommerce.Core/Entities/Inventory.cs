using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionEcommerce.Core.Entities
{
    /// <summary>
    /// Inventory entity - tracks product stock levels
    /// </summary>
    public class Inventory : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int ReservedQuantity { get; set; } = 0;

        public int? WarehouseId { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public DateTime? LastRestockDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        /// <summary>
        /// Gets available quantity (total - reserved)
        /// </summary>
        [NotMapped]
        public int AvailableQuantity => Quantity - ReservedQuantity;
    }
}
