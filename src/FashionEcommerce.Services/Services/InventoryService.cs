using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.Services.Services
{
    /// <summary>
    /// Inventory service implementation - handles inventory checks, reservations and updates
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly FashionEcommerceDbContext _context;

        public InventoryService(FashionEcommerceDbContext context)
        {
            _context = context;
        }

        public Task<Inventory?> GetInventoryByIdAsync(int inventoryId)
        {
            return _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId && !i.IsDeleted);
        }

        public Task<Inventory?> GetInventoryByProductIdAsync(int productId)
        {
            return _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && !i.IsDeleted);
        }

        public async Task<int> GetAvailableQuantityAsync(int productId)
        {
            var inventory = await GetInventoryByProductIdAsync(productId);
            if (inventory == null) return 0;
            return Math.Max(0, inventory.Quantity - inventory.ReservedQuantity);
        }

        public async Task<IEnumerable<Inventory>> GetAllInventoriesAsync()
        {
            return await _context.Inventories
                .Where(i => !i.IsDeleted)
                .Include(i => i.Product)
                .ToListAsync();
        }

        public async Task<bool> CheckAvailabilityAsync(int productId, int quantity)
        {
            if (quantity <= 0) return false;
            var available = await GetAvailableQuantityAsync(productId);
            return available >= quantity;
        }

        /// <summary>
        /// Update inventory quantity (sets quantity to provided value).
        /// Also ensures reserved quantity does not exceed total quantity.
        /// </summary>
        public async Task<Inventory> UpdateInventoryAsync(int productId, int quantity)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && !i.IsDeleted);

            if (inventory == null)
            {
                // create new inventory record if not exists
                inventory = new Inventory
                {
                    ProductId = productId,
                    Quantity = Math.Max(0, quantity),
                    ReservedQuantity = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Inventories.Add(inventory);
            }
            else
            {
                inventory.Quantity = Math.Max(0, quantity);
                // Ensure reserved doesn't exceed quantity
                if (inventory.ReservedQuantity > inventory.Quantity)
                {
                    inventory.ReservedQuantity = inventory.Quantity;
                }
                inventory.UpdatedAt = DateTime.UtcNow;
                _context.Inventories.Update(inventory);
            }

            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<Inventory> CreateOrUpdateInventoryAsync(Inventory inventory)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));

            var existing = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventory.Id || (i.ProductId == inventory.ProductId && i.WarehouseId == inventory.WarehouseId) );

            if (existing == null)
            {
                inventory.CreatedAt = DateTime.UtcNow;
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();
                return inventory;
            }

            existing.Quantity = inventory.Quantity;
            existing.ReservedQuantity = Math.Min(inventory.ReservedQuantity, inventory.Quantity);
            existing.Location = inventory.Location;
            existing.Notes = inventory.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.Inventories.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteInventoryAsync(int inventoryId)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null) return false;
            inventory.IsDeleted = true;
            inventory.UpdatedAt = DateTime.UtcNow;
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Reserve inventory for a product (increase ReservedQuantity) if available.
        /// </summary>
        public async Task<bool> ReserveInventoryAsync(int productId, int quantity)
        {
            if (quantity <= 0) return false;

            // Use a transaction to avoid race conditions
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == productId && !i.IsDeleted);

                if (inventory == null) return false;

                var available = inventory.Quantity - inventory.ReservedQuantity;
                if (available < quantity) return false;

                inventory.ReservedQuantity += quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Release a previously reserved quantity (decrease ReservedQuantity).
        /// </summary>
        public async Task<bool> ReleaseReservationAsync(int productId, int quantity)
        {
            if (quantity <= 0) return false;

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && !i.IsDeleted);

            if (inventory == null) return false;

            inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - quantity);
            inventory.UpdatedAt = DateTime.UtcNow;
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
