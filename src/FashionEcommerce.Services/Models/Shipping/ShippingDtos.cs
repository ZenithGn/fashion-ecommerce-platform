using FashionEcommerce.Core.Entities;

namespace FashionEcommerce.Services.Models.Shipping
{
    public sealed class ShipmentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string CarrierName { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public ShipmentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public decimal ShippingFee { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ShipmentEventDto> Events { get; set; } = new();
    }

    public sealed class ShipmentEventDto
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public ShipmentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? Location { get; set; }
        public string? Note { get; set; }
        public DateTime OccurredAt { get; set; }
    }

    public sealed class CreateShipmentRequest
    {
        public int OrderId { get; set; }
        public string CarrierName { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public decimal ShippingFee { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? Notes { get; set; }
    }

    public sealed class UpdateShipmentRequest
    {
        public string CarrierName { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public decimal ShippingFee { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? Notes { get; set; }
    }

    public sealed class UpdateShipmentStatusRequest
    {
        public ShipmentStatus Status { get; set; }
        public string? Location { get; set; }
        public string? Note { get; set; }
        public DateTime? OccurredAt { get; set; }
    }

    public sealed class CreateShipmentEventRequest
    {
        public ShipmentStatus Status { get; set; }
        public string? Location { get; set; }
        public string? Note { get; set; }
        public DateTime? OccurredAt { get; set; }
    }
}
