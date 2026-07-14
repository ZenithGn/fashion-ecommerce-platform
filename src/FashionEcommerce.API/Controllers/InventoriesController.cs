using FashionEcommerce.Core.Entities;
using FashionEcommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoriesController> _logger;

        public InventoriesController(IInventoryService inventoryService, ILogger<InventoriesController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all inventory records
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetAllInventories()
        {
            try
            {
                var inventories = await _inventoryService.GetAllInventoriesAsync();
                return Ok(inventories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting inventories: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get inventory by id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory>> GetInventoryById(int id)
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(id);
                if (inventory == null)
                    return NotFound("Inventory not found");
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting inventory: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get inventory by product id
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<Inventory>> GetInventoryByProductId(int productId)
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryByProductIdAsync(productId);
                if (inventory == null)
                    return NotFound("Inventory not found for this product");
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting inventory: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get available quantity for a product
        /// </summary>
        [HttpGet("product/{productId}/available")]
        public async Task<ActionResult<object>> GetAvailableQuantity(int productId)
        {
            try
            {
                var available = await _inventoryService.GetAvailableQuantityAsync(productId);
                return Ok(new
                {
                    productId = productId,
                    totalQuantity = (await _inventoryService.GetInventoryByProductIdAsync(productId))?.Quantity ?? 0,
                    reservedQuantity = (await _inventoryService.GetInventoryByProductIdAsync(productId))?.ReservedQuantity ?? 0,
                    availableQuantity = available
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking availability: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check if product is available in requested quantity
        /// </summary>
        [HttpPost("check-availability")]
        public async Task<ActionResult<object>> CheckAvailability([FromBody] InventoryCheckRequest request)
        {
            try
            {
                if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
                    return BadRequest("Invalid product id or quantity");

                var isAvailable = await _inventoryService.CheckAvailabilityAsync(request.ProductId, request.Quantity);
                var available = await _inventoryService.GetAvailableQuantityAsync(request.ProductId);

                return Ok(new
                {
                    productId = request.ProductId,
                    requestedQuantity = request.Quantity,
                    availableQuantity = available,
                    isAvailable = isAvailable
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking availability: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create new inventory record
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<Inventory>> CreateInventory([FromBody] Inventory inventory)
        {
            try
            {
                if (inventory == null)
                    return BadRequest("Inventory cannot be null");

                if (inventory.ProductId <= 0)
                    return BadRequest("Product ID is required");

                if (inventory.Quantity < 0)
                    return BadRequest("Quantity cannot be negative");

                var created = await _inventoryService.CreateOrUpdateInventoryAsync(inventory);
                return CreatedAtAction(nameof(GetInventoryById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating inventory: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update inventory quantity
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] Inventory inventory)
        {
            try
            {
                if (inventory == null)
                    return BadRequest("Inventory cannot be null");

                if (id != inventory.Id)
                    return BadRequest("ID mismatch");
                var updated = await _inventoryService.CreateOrUpdateInventoryAsync(inventory);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating inventory: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Reserve inventory for order
        /// </summary>
        [HttpPost("{id}/reserve")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> ReserveInventory(int id, [FromBody] ReserveInventoryRequest request)
        {
            try
            {
                if (request == null || request.Quantity <= 0)
                    return BadRequest("Invalid quantity");

                var inventory = await _inventoryService.GetInventoryByIdAsync(id);
                if (inventory == null) return NotFound("Inventory not found");

                var success = await _inventoryService.ReserveInventoryAsync(inventory.ProductId, request.Quantity);
                if (!success) return BadRequest("Insufficient inventory");

                var updated = await _inventoryService.GetInventoryByIdAsync(id);
                return Ok(new { message = "Inventory reserved successfully", inventory = updated });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reserving inventory: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Release reserved inventory
        /// </summary>
        [HttpPost("{id}/release")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> ReleaseInventory(int id, [FromBody] ReleaseInventoryRequest request)
        {
            try
            {
                if (request == null || request.Quantity <= 0)
                    return BadRequest("Invalid quantity");

                var inventory = await _inventoryService.GetInventoryByIdAsync(id);
                if (inventory == null) return NotFound("Inventory not found");

                var success = await _inventoryService.ReleaseReservationAsync(inventory.ProductId, request.Quantity);
                if (!success) return BadRequest("Failed to release reservation");

                var updated = await _inventoryService.GetInventoryByIdAsync(id);
                return Ok(new { message = "Inventory released successfully", inventory = updated });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error releasing inventory: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete inventory record
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            try
            {
                var success = await _inventoryService.DeleteInventoryAsync(id);
                if (!success) return NotFound("Inventory not found");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting inventory: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    // Request DTOs
    public class InventoryCheckRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class ReserveInventoryRequest
    {
        public int Quantity { get; set; }
    }

    public class ReleaseInventoryRequest
    {
        public int Quantity { get; set; }
    }
}
