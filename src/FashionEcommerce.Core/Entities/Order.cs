using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionEcommerce.Core.Entities
{
    /// <summary>
    /// Order entity - represents customer orders
    /// </summary>
    public class Order : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingCost { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(500)]
        public string? ShippingAddress { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        [StringLength(100)]
        public string? TrackingNumber { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public virtual Shipment? Shipment { get; set; }
    }

    /// <summary>
    /// OrderItem entity - represents individual items in an order
    /// </summary>
    public class OrderItem : BaseEntity
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [StringLength(100)]
        public string? Size { get; set; }

        [StringLength(100)]
        public string? Color { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }

    /// <summary>
    /// Order status enumeration
    /// </summary>
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4,
        Returned = 5
    }
}
