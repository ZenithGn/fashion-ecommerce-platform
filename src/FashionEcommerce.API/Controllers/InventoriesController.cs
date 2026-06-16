using FashionEcommerce.Data;
using FashionEcommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoriesController : ControllerBase
    {
        private readonly FashionEcommerceDbContext _context;
        private readonly ILogger<InventoriesController> _logger;

        public InventoriesController(FashionEcommerceDbContext context, ILogger<InventoriesController> logger)
        {
            _context = context;
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
                var inventories = await _context.Inventories
                    .Where(i => !i.IsDeleted)
                    .Include(i => i.Product)
                    .ToListAsync();
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
                var inventory = await _context.Inventories
                    .Where(i => i.Id == id && !i.IsDeleted)
                    .Include(i => i.Product)
                    .FirstOrDefaultAsync();

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
                var inventory = await _context.Inventories
                    .Where(i => i.ProductId == productId && !i.IsDeleted)
                    .Include(i => i.Product)
                    .FirstOrDefaultAsync();

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
                var inventory = await _context.Inventories
                    .Where(i => i.ProductId == productId && !i.IsDeleted)
                    .FirstOrDefaultAsync();

                if (inventory == null)
                    return NotFound("Inventory not found");

                return Ok(new
                {
                    productId = productId,
                    totalQuantity = inventory.Quantity,
                    reservedQuantity = inventory.ReservedQuantity,
                    availableQuantity = inventory.AvailableQuantity
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
                if (request?.ProductId <= 0 || request.Quantity <= 0)
                    return BadRequest("Invalid product id or quantity");

                var inventory = await _context.Inventories
                    .Where(i => i.ProductId == request.ProductId && !i.IsDeleted)
                    .FirstOrDefaultAsync();

                if (inventory == null)
                    return NotFound("Inventory not found");

                bool isAvailable = inventory.AvailableQuantity >= request.Quantity;

                return Ok(new
                {
                    productId = request.ProductId,
                    requestedQuantity = request.Quantity,
                    availableQuantity = inventory.AvailableQuantity,
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
        [Authorize(Roles = "Admin,Staff")]
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

                inventory.CreatedAt = DateTime.UtcNow;
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetInventoryById), new { id = inventory.Id }, inventory);
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
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] Inventory inventory)
        {
            try
            {
                if (id != inventory.Id)
                    return BadRequest("ID mismatch");

                var existingInventory = await _context.Inventories.FindAsync(id);
                if (existingInventory == null)
                    return NotFound("Inventory not found");

                existingInventory.Quantity = inventory.Quantity;
                existingInventory.ReservedQuantity = inventory.ReservedQuantity;
                existingInventory.Location = inventory.Location;
                existingInventory.Notes = inventory.Notes;
                existingInventory.UpdatedAt = DateTime.UtcNow;

                _context.Inventories.Update(existingInventory);
                await _context.SaveChangesAsync();

                return Ok(existingInventory);
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
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ReserveInventory(int id, [FromBody] ReserveInventoryRequest request)
        {
            try
            {
                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory == null)
                    return NotFound("Inventory not found");

                if (request?.Quantity <= 0)
                    return BadRequest("Invalid quantity");

                if (inventory.AvailableQuantity < request.Quantity)
                    return BadRequest("Insufficient inventory");

                inventory.ReservedQuantity += request.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Inventory reserved successfully",
                    inventory = inventory
                });
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
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ReleaseInventory(int id, [FromBody] ReleaseInventoryRequest request)
        {
            try
            {
                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory == null)
                    return NotFound("Inventory not found");

                if (request?.Quantity <= 0)
                    return BadRequest("Invalid quantity");

                if (inventory.ReservedQuantity < request.Quantity)
                    return BadRequest("Cannot release more than reserved");

                inventory.ReservedQuantity -= request.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Inventory released successfully",
                    inventory = inventory
                });
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
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            try
            {
                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory == null)
                    return NotFound("Inventory not found");

                inventory.IsDeleted = true;
                inventory.UpdatedAt = DateTime.UtcNow;
                _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();

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
