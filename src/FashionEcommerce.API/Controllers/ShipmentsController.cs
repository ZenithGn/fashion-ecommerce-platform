using FashionEcommerce.Core.Entities;
using FashionEcommerce.Services.Interfaces;
using FashionEcommerce.Services.Models.Shipping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShipmentsController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        private readonly ILogger<ShipmentsController> _logger;

        public ShipmentsController(
            IShipmentService shipmentService,
            ILogger<ShipmentsController> logger)
        {
            _shipmentService = shipmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of all active shipments filtered by status, carrier, and date range (Admin/Manager/Staff only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<IEnumerable<ShipmentDto>>> GetShipments(
            [FromQuery] ShipmentStatus? status = null,
            [FromQuery] string? carrier = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var shipments = await _shipmentService.GetShipmentsAsync(status, carrier, from, to);
                return Ok(shipments.Select(MapShipment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shipments");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get details of a specific shipment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ShipmentDto>> GetShipment(int id)
        {
            try
            {
                var shipment = await _shipmentService.GetShipmentByIdAsync(id);
                if (shipment == null)
                    return NotFound("Shipment not found");

                if (!CanAccessShipment(shipment))
                    return Forbid();

                return Ok(MapShipment(shipment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shipment {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get shipment details associated with a specific Order ID
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<ShipmentDto>> GetShipmentByOrder(int orderId)
        {
            try
            {
                var shipment = await _shipmentService.GetShipmentByOrderIdAsync(orderId);
                if (shipment == null)
                    return NotFound("Shipment not found");

                if (!CanAccessShipment(shipment))
                    return Forbid();

                return Ok(MapShipment(shipment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shipment by order {OrderId}", orderId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new shipment and generate an initial tracking event (Admin/Manager/Staff only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<ShipmentDto>> CreateShipment([FromBody] CreateShipmentRequest request)
        {
            if (request == null)
                return BadRequest("Request is required");

            if (request.OrderId <= 0)
                return BadRequest("OrderId is required");

            if (string.IsNullOrWhiteSpace(request.CarrierName))
                return BadRequest("Carrier name is required");

            try
            {
                var shipment = await _shipmentService.CreateShipmentAsync(
                    request.OrderId,
                    request.CarrierName,
                    request.TrackingNumber,
                    request.ShippingFee,
                    request.EstimatedDeliveryDate,
                    request.Notes
                );

                var created = await _shipmentService.GetShipmentByIdAsync(shipment.Id);
                return CreatedAtAction(nameof(GetShipment), new { id = shipment.Id }, MapShipment(created!));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shipment");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update shipping details (carrier, tracking, estimated date) for a shipment (Admin/Manager/Staff only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<ShipmentDto>> UpdateShipment(int id, [FromBody] UpdateShipmentRequest request)
        {
            if (request == null)
                return BadRequest("Request is required");

            if (string.IsNullOrWhiteSpace(request.CarrierName))
                return BadRequest("Carrier name is required");

            try
            {
                var shipment = await _shipmentService.UpdateShipmentAsync(
                    id,
                    request.CarrierName,
                    request.TrackingNumber,
                    request.ShippingFee,
                    request.EstimatedDeliveryDate,
                    request.Notes
                );

                var updated = await _shipmentService.GetShipmentByIdAsync(id);
                return Ok(MapShipment(updated!));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipment {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update status of a shipment and synchronize with order status (Admin/Manager/Staff only)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<ShipmentDto>> UpdateShipmentStatus(int id, [FromBody] UpdateShipmentStatusRequest request)
        {
            if (request == null)
                return BadRequest("Request is required");

            try
            {
                var shipment = await _shipmentService.UpdateShipmentStatusAsync(
                    id,
                    request.Status,
                    request.Location,
                    request.Note,
                    request.OccurredAt
                );

                var updated = await _shipmentService.GetShipmentByIdAsync(id);
                return Ok(MapShipment(updated!));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for shipment {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Add a new shipment timeline event (Admin/Manager/Staff only)
        /// </summary>
        [HttpPost("{id}/events")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<ShipmentDto>> AddShipmentEvent(int id, [FromBody] CreateShipmentEventRequest request)
        {
            if (request == null)
                return BadRequest("Request is required");

            try
            {
                await _shipmentService.UpdateShipmentStatusAsync(
                    id,
                    request.Status,
                    request.Location,
                    request.Note,
                    request.OccurredAt
                );

                var updated = await _shipmentService.GetShipmentByIdAsync(id);
                return Ok(MapShipment(updated!));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding event to shipment {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get the history of tracking events for a specific shipment
        /// </summary>
        [HttpGet("{id}/events")]
        public async Task<ActionResult<IEnumerable<ShipmentEventDto>>> GetShipmentEvents(int id)
        {
            try
            {
                var shipment = await _shipmentService.GetShipmentByIdAsync(id);
                if (shipment == null)
                    return NotFound("Shipment not found");

                if (!CanAccessShipment(shipment))
                    return Forbid();

                var events = await _shipmentService.GetShipmentEventsAsync(id);
                return Ok(events.Select(MapEvent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for shipment {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private bool CanAccessShipment(Shipment shipment)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Staff"))
                return true;

            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) && shipment.Order?.UserId == userId;
        }

        private static ShipmentDto MapShipment(Shipment shipment)
        {
            return new ShipmentDto
            {
                Id = shipment.Id,
                OrderId = shipment.OrderId,
                OrderNumber = shipment.Order?.OrderNumber,
                CarrierName = shipment.CarrierName,
                TrackingNumber = shipment.TrackingNumber,
                Status = shipment.Status,
                ShippingFee = shipment.ShippingFee,
                EstimatedDeliveryDate = shipment.EstimatedDeliveryDate,
                ShippedAt = shipment.ShippedAt,
                DeliveredAt = shipment.DeliveredAt,
                Notes = shipment.Notes,
                CreatedAt = shipment.CreatedAt,
                Events = shipment.Events.OrderByDescending(e => e.OccurredAt).Select(MapEvent).ToList()
            };
        }

        private static ShipmentEventDto MapEvent(ShipmentEvent shipmentEvent)
        {
            return new ShipmentEventDto
            {
                Id = shipmentEvent.Id,
                ShipmentId = shipmentEvent.ShipmentId,
                Status = shipmentEvent.Status,
                Location = shipmentEvent.Location,
                Note = shipmentEvent.Note,
                OccurredAt = shipmentEvent.OccurredAt
            };
        }
    }
}
