using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.Services.Services
{
    public class CartService : ICartService
    {
        private readonly FashionEcommerceDbContext _context;
        private readonly IInventoryService _inventoryService;

        public CartService(FashionEcommerceDbContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public Task<Cart?> GetCartByUserIdAsync(int userId)
        {
            return _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
        }

        public async Task<Cart> AddToCartAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted && p.IsActive);
            if (product == null) throw new KeyNotFoundException("Product not found.");

            // Check inventory availability
            var available = await _inventoryService.GetAvailableQuantityAsync(productId);
            if (available < quantity) throw new InvalidOperationException($"Insufficient stock. Available: {available}");

            // Get or create cart
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    TotalPrice = 0m,
                    ItemCount = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Check if item exists
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && !i.IsDeleted);
            var unitPrice = product.DiscountPrice ?? product.Price;

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + quantity;
                if (available < newQuantity) throw new InvalidOperationException($"Insufficient stock for updated quantity. Available: {available}");

                existingItem.Quantity = newQuantity;
                existingItem.UnitPrice = unitPrice;
                existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice * quantity,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            // Recalculate totals
            await _context.SaveChangesAsync();
            await RecalculateCartAsync(cart.Id);

            return await GetCartByUserIdAsync(userId) ?? cart;
        }

        public async Task<Cart> UpdateCartItemAsync(int cartId, int productId, int quantity)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId && !c.IsDeleted);

            if (cart == null) throw new KeyNotFoundException("Cart not found.");

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId && !i.IsDeleted);
            if (item == null) throw new KeyNotFoundException("Cart item not found.");

            if (quantity <= 0)
            {
                // remove item
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                await RecalculateCartAsync(cart.Id);
                return await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == cartId && !c.IsDeleted) ?? cart;
            }

            var available = await _inventoryService.GetAvailableQuantityAsync(productId);
            if (available < quantity) throw new InvalidOperationException($"Insufficient stock. Available: {available}");

            var unitPrice = item.Product?.DiscountPrice ?? item.Product?.Price ?? item.UnitPrice;
            item.Quantity = quantity;
            item.UnitPrice = unitPrice;
            item.TotalPrice = unitPrice * quantity;
            item.UpdatedAt = DateTime.UtcNow;
            _context.CartItems.Update(item);

            await _context.SaveChangesAsync();
            await RecalculateCartAsync(cart.Id);

            return await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId && !c.IsDeleted) ?? cart;
        }

        public async Task<Cart> RemoveFromCartAsync(int cartId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId && !c.IsDeleted);

            if (cart == null) throw new KeyNotFoundException("Cart not found.");

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId && !i.IsDeleted);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                await RecalculateCartAsync(cart.Id);
            }

            return await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId && !c.IsDeleted) ?? cart;
        }

        public async Task<bool> ClearCartAsync(int cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId && !c.IsDeleted);

            if (cart == null) return false;

            if (cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
            }

            cart.TotalPrice = 0m;
            cart.ItemCount = 0;
            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateCartTotalAsync(int cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId && !c.IsDeleted);

            if (cart == null) return 0m;

            return cart.Items.Sum(i => i.TotalPrice);
        }

        private async Task RecalculateCartAsync(int cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId && !c.IsDeleted);

            if (cart == null) return;

            cart.TotalPrice = cart.Items.Sum(i => i.TotalPrice);
            cart.ItemCount = cart.Items.Sum(i => i.Quantity);
            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }
    }
}
