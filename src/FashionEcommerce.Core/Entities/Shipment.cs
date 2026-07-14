using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionEcommerce.Core.Entities
{
    public class Shipment : BaseEntity
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string CarrierName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? TrackingNumber { get; set; }

        public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingFee { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public virtual Order? Order { get; set; }
        public virtual ICollection<ShipmentEvent> Events { get; set; } = new List<ShipmentEvent>();
    }

    public class ShipmentEvent : BaseEntity
    {
        [Required]
        public int ShipmentId { get; set; }

        public ShipmentStatus Status { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        public virtual Shipment? Shipment { get; set; }
    }

    public enum ShipmentStatus
    {
        Created = 0,
        Packing = 1,
        ReadyToShip = 2,
        InTransit = 3,
        OutForDelivery = 4,
        Delivered = 5,
        FailedDelivery = 6,
        Returned = 7,
        Cancelled = 8
    }
}
