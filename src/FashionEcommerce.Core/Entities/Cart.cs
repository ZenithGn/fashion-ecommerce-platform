using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionEcommerce.Core.Entities
{
    /// <summary>
    /// Cart entity - represents a user's shopping cart
    /// </summary>
    public class Cart : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; } = 0;

        public int ItemCount { get; set; } = 0;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }

    /// <summary>
    /// CartItem entity - represents individual items in a cart
    /// </summary>
    public class CartItem : BaseEntity
    {
        [Required]
        public int CartId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        // Navigation properties
        [ForeignKey("CartId")]
        public virtual Cart? Cart { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
