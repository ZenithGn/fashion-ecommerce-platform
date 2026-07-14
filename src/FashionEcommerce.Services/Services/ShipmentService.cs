using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FashionEcommerce.Services.Services
{
    public sealed class ShipmentService : IShipmentService
    {
        private readonly FashionEcommerceDbContext _context;
        private readonly IOrderService _orderService;

        public ShipmentService(FashionEcommerceDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        public async Task<IEnumerable<Shipment>> GetShipmentsAsync(ShipmentStatus? status, string? carrier, DateTime? from, DateTime? to)
        {
            var query = _context.Shipments
                .AsNoTracking()
                .Include(s => s.Order)
                .Include(s => s.Events)
                .Where(s => !s.IsDeleted);

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(carrier))
                query = query.Where(s => s.CarrierName.ToLower().Contains(carrier.Trim().ToLower()));

            if (from.HasValue)
                query = query.Where(s => s.CreatedAt >= from.Value.ToUniversalTime());

            if (to.HasValue)
                query = query.Where(s => s.CreatedAt <= to.Value.ToUniversalTime());

            return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        }

        public async Task<Shipment?> GetShipmentByIdAsync(int id)
        {
            return await _context.Shipments
                .Include(s => s.Order)
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<Shipment?> GetShipmentByOrderIdAsync(int orderId)
        {
            return await _context.Shipments
                .Include(s => s.Order)
                .Include(s => s.Events)
                .FirstOrDefaultAsync(s => s.OrderId == orderId && !s.IsDeleted);
        }

        public async Task<Shipment> CreateShipmentAsync(int orderId, string carrierName, string? trackingNumber, decimal shippingFee, DateTime? estimatedDeliveryDate, string? notes)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            var exists = await _context.Shipments.AnyAsync(s => s.OrderId == orderId && !s.IsDeleted);
            if (exists)
                throw new InvalidOperationException("Shipment already exists for this order");

            var shipment = new Shipment
            {
                OrderId = orderId,
                CarrierName = carrierName.Trim(),
                TrackingNumber = string.IsNullOrWhiteSpace(trackingNumber) ? null : trackingNumber.Trim(),
                ShippingFee = shippingFee,
                EstimatedDeliveryDate = estimatedDeliveryDate?.ToUniversalTime(),
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                Status = ShipmentStatus.Created,
                CreatedAt = DateTime.UtcNow
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();

            await AddShipmentEventAsync(shipment.Id, ShipmentStatus.Created, null, "Shipment created", DateTime.UtcNow);
            await SyncOrderFromShipmentAsync(shipment);

            return shipment;
        }

        public async Task<Shipment> UpdateShipmentAsync(int id, string carrierName, string? trackingNumber, decimal shippingFee, DateTime? estimatedDeliveryDate, string? notes)
        {
            var shipment = await _context.Shipments.Include(s => s.Order).FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found");

            shipment.CarrierName = carrierName.Trim();
            shipment.TrackingNumber = string.IsNullOrWhiteSpace(trackingNumber) ? null : trackingNumber.Trim();
            shipment.ShippingFee = shippingFee;
            shipment.EstimatedDeliveryDate = estimatedDeliveryDate?.ToUniversalTime();
            shipment.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            shipment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await SyncOrderFromShipmentAsync(shipment);

            return shipment;
        }

        public async Task<Shipment> UpdateShipmentStatusAsync(int id, ShipmentStatus status, string? location, string? note, DateTime? occurredAt)
        {
            var shipment = await _context.Shipments.Include(s => s.Order).FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found");

            shipment.Status = status;
            shipment.UpdatedAt = DateTime.UtcNow;

            if ((status == ShipmentStatus.InTransit || status == ShipmentStatus.OutForDelivery) && shipment.ShippedAt == null)
                shipment.ShippedAt = DateTime.UtcNow;

            if (status == ShipmentStatus.Delivered && shipment.DeliveredAt == null)
                shipment.DeliveredAt = DateTime.UtcNow;

            await AddShipmentEventAsync(shipment.Id, status, location, note, occurredAt ?? DateTime.UtcNow);
            await _context.SaveChangesAsync();
            await SyncOrderFromShipmentAsync(shipment);

            return shipment;
        }

        public async Task<Shipment> AddShipmentEventAsync(int shipmentId, ShipmentStatus status, string? location, string? note, DateTime? occurredAt)
        {
            var shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.Id == shipmentId && !s.IsDeleted);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found");

            var shipmentEvent = new ShipmentEvent
            {
                ShipmentId = shipmentId,
                Status = status,
                Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
                Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                OccurredAt = (occurredAt ?? DateTime.UtcNow).ToUniversalTime(),
                CreatedAt = DateTime.UtcNow
            };

            _context.ShipmentEvents.Add(shipmentEvent);
            await _context.SaveChangesAsync();
            return shipment;
        }

        public async Task<IEnumerable<ShipmentEvent>> GetShipmentEventsAsync(int shipmentId)
        {
            return await _context.ShipmentEvents
                .Where(e => e.ShipmentId == shipmentId && !e.IsDeleted)
                .OrderByDescending(e => e.OccurredAt)
                .ToListAsync();
        }

        private async Task SyncOrderFromShipmentAsync(Shipment shipment)
        {
            var orderStatus = shipment.Status switch
            {
                ShipmentStatus.Packing or ShipmentStatus.ReadyToShip => OrderStatus.Processing,
                ShipmentStatus.InTransit or ShipmentStatus.OutForDelivery => OrderStatus.Shipped,
                ShipmentStatus.Delivered => OrderStatus.Delivered,
                ShipmentStatus.Returned => OrderStatus.Returned,
                ShipmentStatus.Cancelled => OrderStatus.Cancelled,
                _ => (OrderStatus?)null
            };

            if (shipment.Order != null)
            {
                shipment.Order.TrackingNumber = shipment.TrackingNumber;
                shipment.Order.ShippingCost = shipment.ShippingFee;
                shipment.Order.UpdatedAt = DateTime.UtcNow;
                
                if (shipment.ShippedAt.HasValue)
                    shipment.Order.ShippedDate = shipment.ShippedAt;
                
                if (shipment.DeliveredAt.HasValue)
                    shipment.Order.DeliveredDate = shipment.DeliveredAt;
                
                await _context.SaveChangesAsync();
            }

            if (orderStatus.HasValue && shipment.OrderId > 0)
            {
                await _orderService.UpdateOrderStatusAsync(shipment.OrderId, orderStatus.Value);
            }
        }
    }
}
