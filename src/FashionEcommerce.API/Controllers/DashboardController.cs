using FashionEcommerce.API.Models.Dashboard;
using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class DashboardController : ControllerBase
    {
        private const int RecentOrderLimit = 10;
        private const int StockAlertThreshold = 10;

        private readonly FashionEcommerceDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(FashionEcommerceDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get full dashboard overview for admin and staff users.
        /// </summary>
        [HttpGet("overview")]
        public async Task<ActionResult<DashboardOverviewDto>> GetOverview()
        {
            try
            {
                var summaryTask = GetSummaryAsync();
                var recentOrdersTask = GetRecentOrdersAsync();
                var stockAlertsTask = GetStockAlertsAsync();

                await Task.WhenAll(summaryTask, recentOrdersTask, stockAlertsTask);

                return Ok(new DashboardOverviewDto
                {
                    Summary = summaryTask.Result,
                    RecentOrders = recentOrdersTask.Result,
                    StockAlerts = stockAlertsTask.Result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard overview");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get dashboard summary metrics.
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
        {
            try
            {
                var summary = await GetSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get the latest orders for the dashboard.
        /// </summary>
        [HttpGet("recent-orders")]
        public async Task<ActionResult<IEnumerable<RecentOrderDto>>> GetRecentOrders()
        {
            try
            {
                var recentOrders = await GetRecentOrdersAsync();
                return Ok(recentOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent orders");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get inventory alerts for low-stock products.
        /// </summary>
        [HttpGet("stock-alerts")]
        public async Task<ActionResult<IEnumerable<StockAlertDto>>> GetStockAlerts()
        {
            try
            {
                var stockAlerts = await GetStockAlertsAsync();
                return Ok(stockAlerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock alerts");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var totalUsersTask = _context.Users.AsNoTracking().CountAsync(u => !u.IsDeleted);
            var totalCustomersTask = _context.Users.AsNoTracking().CountAsync(u => !u.IsDeleted && u.RoleId == 2);
            var totalProductsTask = _context.Products.AsNoTracking().CountAsync(p => !p.IsDeleted);
            var activeProductsTask = _context.Products.AsNoTracking().CountAsync(p => !p.IsDeleted && p.IsActive);
            var totalCategoriesTask = _context.Categories.AsNoTracking().CountAsync(c => !c.IsDeleted);
            var totalInventoriesTask = _context.Inventories.AsNoTracking().CountAsync(i => !i.IsDeleted);
            var lowStockProductsTask = _context.Inventories.AsNoTracking().CountAsync(i => !i.IsDeleted && i.AvailableQuantity <= StockAlertThreshold);
            var totalOrdersTask = _context.Orders.AsNoTracking().CountAsync(o => !o.IsDeleted);
            var pendingOrdersTask = _context.Orders.AsNoTracking().CountAsync(o => !o.IsDeleted && o.Status == OrderStatus.Pending);
            var processingOrdersTask = _context.Orders.AsNoTracking().CountAsync(o => !o.IsDeleted && o.Status == OrderStatus.Processing);
            var completedOrdersTask = _context.Orders.AsNoTracking().CountAsync(o => !o.IsDeleted && (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Returned));
            var cancelledOrdersTask = _context.Orders.AsNoTracking().CountAsync(o => !o.IsDeleted && o.Status == OrderStatus.Cancelled);
            var totalRevenueTask = _context.Orders.AsNoTracking()
                .Where(o => !o.IsDeleted && (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Returned))
                .SumAsync(o => (decimal?)o.TotalPrice);
            var pendingRevenueTask = _context.Orders.AsNoTracking()
                .Where(o => !o.IsDeleted && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing))
                .SumAsync(o => (decimal?)o.TotalPrice);

            await Task.WhenAll(
                totalUsersTask,
                totalCustomersTask,
                totalProductsTask,
                activeProductsTask,
                totalCategoriesTask,
                totalInventoriesTask,
                lowStockProductsTask,
                totalOrdersTask,
                pendingOrdersTask,
                processingOrdersTask,
                completedOrdersTask,
                cancelledOrdersTask,
                totalRevenueTask,
                pendingRevenueTask);

            return new DashboardSummaryDto
            {
                TotalUsers = totalUsersTask.Result,
                TotalCustomers = totalCustomersTask.Result,
                TotalProducts = totalProductsTask.Result,
                ActiveProducts = activeProductsTask.Result,
                TotalCategories = totalCategoriesTask.Result,
                TotalInventories = totalInventoriesTask.Result,
                LowStockProducts = lowStockProductsTask.Result,
                TotalOrders = totalOrdersTask.Result,
                PendingOrders = pendingOrdersTask.Result,
                ProcessingOrders = processingOrdersTask.Result,
                CompletedOrders = completedOrdersTask.Result,
                CancelledOrders = cancelledOrdersTask.Result,
                TotalRevenue = totalRevenueTask.Result ?? 0,
                PendingRevenue = pendingRevenueTask.Result ?? 0
            };
        }

        private async Task<List<RecentOrderDto>> GetRecentOrdersAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(RecentOrderLimit)
                .Select(o => new RecentOrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User != null ? o.User.FirstName + " " + o.User.LastName : string.Empty,
                    CustomerEmail = o.User != null ? o.User.Email : string.Empty,
                    Status = o.Status.ToString(),
                    TotalPrice = o.TotalPrice,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();
        }

        private async Task<List<StockAlertDto>> GetStockAlertsAsync()
        {
            return await _context.Inventories
                .AsNoTracking()
                .Where(i => !i.IsDeleted && i.AvailableQuantity <= StockAlertThreshold)
                .Include(i => i.Product)
                .ThenInclude(p => p!.Category)
                .OrderBy(i => i.AvailableQuantity)
                .Select(i => new StockAlertDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : string.Empty,
                    SKU = i.Product != null ? i.Product.SKU : null,
                    CategoryName = i.Product != null && i.Product.Category != null ? i.Product.Category.Name : null,
                    Quantity = i.Quantity,
                    ReservedQuantity = i.ReservedQuantity,
                    AvailableQuantity = i.AvailableQuantity,
                    Location = i.Location
                })
                .ToListAsync();
        }
    }
}