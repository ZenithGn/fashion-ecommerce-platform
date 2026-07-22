using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Interfaces;
using FashionEcommerce.Services.Models.Orders;
using FashionEcommerce.Services.Products;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FashionEcommerce.Services
{
    public class OrderService : IOrderService
    {
        private readonly FashionEcommerceDbContext _context;

        public OrderService(FashionEcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .Include(o => o.Shipment)
                .ThenInclude(s => s!.Events)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .Include(o => o.Shipment)
                .ThenInclude(s => s!.Events)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && !o.IsDeleted);
        }

        public async Task<PagedResult<Order>> GetUserOrdersAsync(int userId, OrderQueryParameters parameters)
        {
            var query = CreateOrderListQuery()
                .Where(o => o.UserId == userId);

            return await GetPagedOrdersAsync(query, parameters);
        }

        public async Task<PagedResult<Order>> GetAllOrdersAsync(OrderQueryParameters parameters)
        {
            return await GetPagedOrdersAsync(CreateOrderListQuery(), parameters);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            if (order.UserId <= 0)
            {
                throw new ArgumentException("UserId is invalid.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Fetch user's cart and items
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == order.UserId && !c.IsDeleted);

                if (cart == null || !cart.Items.Any())
                {
                    throw new InvalidOperationException("Giỏ hàng trống hoặc không tồn tại.");
                }

                // Initialize order details
                order.Items = new List<OrderItem>();
                decimal calculatedSubTotal = 0m;

                // 2. Process each item in the cart
                foreach (var cartItem in cart.Items)
                {
                    // Check inventory stock
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == cartItem.ProductId && !i.IsDeleted);

                    if (inventory == null)
                    {
                        throw new InvalidOperationException($"Không tìm thấy thông tin kho của sản phẩm: '{cartItem.Product?.Name ?? cartItem.ProductId.ToString()}'.");
                    }

                    int availableStock = inventory.Quantity - inventory.ReservedQuantity;
                    if (availableStock < cartItem.Quantity)
                    {
                        throw new InvalidOperationException($"Sản phẩm '{cartItem.Product?.Name ?? cartItem.ProductId.ToString()}' không đủ hàng tồn kho. Yêu cầu: {cartItem.Quantity}, Còn lại: {availableStock}");
                    }

                    // Reserve inventory stock
                    inventory.ReservedQuantity += cartItem.Quantity;
                    _context.Inventories.Update(inventory);

                    // Create OrderItem
                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        TotalPrice = cartItem.TotalPrice,
                        Size = cartItem.Product?.Size,
                        Color = cartItem.Product?.Color,
                        CreatedAt = DateTime.UtcNow
                    };
                    order.Items.Add(orderItem);

                    calculatedSubTotal += cartItem.TotalPrice;
                }

                // 3. Finalize order totals
                order.SubTotal = calculatedSubTotal;
                order.TotalPrice = order.SubTotal + order.ShippingCost + order.TaxAmount - order.DiscountAmount;
                order.OrderNumber = await GenerateUniqueOrderNumberAsync();
                order.Status = OrderStatus.Pending;
                order.CreatedAt = DateTime.UtcNow;

                // 4. Save order to database
                _context.Orders.Add(order);

                // 5. Clear items from user's cart
                _context.CartItems.RemoveRange(cart.Items);
                cart.TotalPrice = 0m;
                cart.ItemCount = 0;
                cart.UpdatedAt = DateTime.UtcNow;
                _context.Carts.Update(cart);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return order;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
            }

            var previousStatus = order.Status;
            if (previousStatus == status)
            {
                return order;
            }

            if (!IsValidStatusTransition(previousStatus, status))
            {
                throw new InvalidOperationException($"Cannot change order status from {previousStatus} to {status}.");
            }

            // Inventory lifecycle logic
            // 1. Release reserved stock if order is Cancelled
            if (status == OrderStatus.Cancelled && previousStatus != OrderStatus.Cancelled && previousStatus < OrderStatus.Shipped)
            {
                foreach (var item in order.Items)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && !i.IsDeleted);

                    if (inventory != null)
                    {
                        inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - item.Quantity);
                        _context.Inventories.Update(inventory);
                    }
                }
            }
            // 2. Reduce physical stock when Shipped or Delivered (moving out of Pending/Processing)
            else if ((status == OrderStatus.Shipped || status == OrderStatus.Delivered) && previousStatus <= OrderStatus.Processing)
            {
                foreach (var item in order.Items)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && !i.IsDeleted);

                    if (inventory != null)
                    {
                        inventory.Quantity = Math.Max(0, inventory.Quantity - item.Quantity);
                        inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - item.Quantity);
                        _context.Inventories.Update(inventory);
                    }
                }
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            if (status == OrderStatus.Shipped)
            {
                order.ShippedDate = DateTime.UtcNow;
            }
            else if (status == OrderStatus.Delivered)
            {
                order.DeliveredDate = DateTime.UtcNow;
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return order;
        }

        private IQueryable<Order> CreateOrderListQuery()
        {
            return _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .Include(o => o.Shipment)
                .Where(o => !o.IsDeleted);
        }

        private async Task<PagedResult<Order>> GetPagedOrdersAsync(IQueryable<Order> query, OrderQueryParameters parameters)
        {
            var page = parameters.Page <= 0 ? 1 : parameters.Page;
            var pageSize = parameters.PageSize <= 0 || parameters.PageSize > 100 ? 20 : parameters.PageSize;

            if (parameters.Status.HasValue)
            {
                query = query.Where(o => o.Status == parameters.Status.Value);
            }

            if (parameters.FromDate.HasValue)
            {
                var from = EnsureUtc(parameters.FromDate.Value.Date);
                query = query.Where(o => o.CreatedAt >= from);
            }

            if (parameters.ToDate.HasValue)
            {
                var toExclusive = EnsureUtc(parameters.ToDate.Value.Date.AddDays(1));
                query = query.Where(o => o.CreatedAt < toExclusive);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var search = parameters.Search.Trim().ToLower();
                query = query.Where(o =>
                    o.OrderNumber.ToLower().Contains(search) ||
                    (o.User != null && (o.User.Email.ToLower().Contains(search) ||
                        (o.User.FirstName + " " + o.User.LastName).ToLower().Contains(search))) ||
                    (o.PhoneNumber != null && o.PhoneNumber.Contains(search)));
            }

            query = ApplySorting(query, parameters.SortBy, parameters.SortDirection);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items
            };
        }

        private static IQueryable<Order> ApplySorting(IQueryable<Order> query, string? sortBy, string? sortDirection)
        {
            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "createdAt" : sortBy.Trim().ToLowerInvariant();

            return normalizedSortBy switch
            {
                "ordernumber" => descending ? query.OrderByDescending(o => o.OrderNumber) : query.OrderBy(o => o.OrderNumber),
                "status" => descending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
                "total" or "totalprice" => descending ? query.OrderByDescending(o => o.TotalPrice) : query.OrderBy(o => o.TotalPrice),
                "customer" => descending
                    ? query.OrderByDescending(o => o.User != null ? o.User.FirstName + " " + o.User.LastName : string.Empty)
                    : query.OrderBy(o => o.User != null ? o.User.FirstName + " " + o.User.LastName : string.Empty),
                _ => descending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt)
            };
        }

        private async Task<string> GenerateUniqueOrderNumberAsync()
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var number = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{RandomNumberGenerator.GetInt32(100000, 1000000)}";
                var exists = await _context.Orders.AnyAsync(o => o.OrderNumber == number);
                if (!exists)
                {
                    return number;
                }
            }

            return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];
        }

        private static bool IsValidStatusTransition(OrderStatus current, OrderStatus next)
        {
            return current switch
            {
                OrderStatus.Pending => next is OrderStatus.Processing or OrderStatus.Cancelled,
                OrderStatus.Processing => next is OrderStatus.Shipped or OrderStatus.Cancelled,
                OrderStatus.Shipped => next == OrderStatus.Delivered,
                OrderStatus.Delivered => next == OrderStatus.Returned,
                OrderStatus.Cancelled => false,
                OrderStatus.Returned => false,
                _ => false
            };
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;

            if (value.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(value, DateTimeKind.Utc);

            return value.ToUniversalTime();
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.IsDeleted)
            {
                return false;
            }

            if (order.Status >= OrderStatus.Shipped)
            {
                throw new InvalidOperationException("Không thể hủy đơn hàng đã giao hoặc đã gửi đi.");
            }

            await UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled);
            return true;
        }
    }
}
