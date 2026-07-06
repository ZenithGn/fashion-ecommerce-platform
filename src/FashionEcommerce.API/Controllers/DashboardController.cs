using FashionEcommerce.Services.Models.Dashboard;
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
        /// Get admin dashboard with summary, revenue trend, recent orders, stock alerts, and top products.
        /// </summary>
        [HttpGet("admin")]
        public async Task<ActionResult<AdminDashboardDto>> GetAdminDashboard(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var (from, toExclusive) = NormalizeDateRange(fromDate, toDate, defaultMonths: 6);
                var toInclusive = toExclusive.AddTicks(-1);

                var summaryTask = GetSummaryAsync();
                var revenueTask = GetRevenueStatisticsAsync(from, toExclusive, "month");
                var recentOrdersTask = GetRecentOrdersAsync();
                var stockAlertsTask = GetStockAlertsAsync();
                var topProductsTask = GetTopProductsAsync(from, toExclusive, 10);

                await Task.WhenAll(summaryTask, revenueTask, recentOrdersTask, stockAlertsTask, topProductsTask);

                return Ok(new AdminDashboardDto
                {
                    Summary = summaryTask.Result,
                    RevenueStatistics = revenueTask.Result,
                    RecentOrders = recentOrdersTask.Result,
                    StockAlerts = stockAlertsTask.Result,
                    TopProducts = topProductsTask.Result,
                    RevenueTrend = revenueTask.Result.RevenueByPeriod
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get revenue statistics for a date range. groupBy supports day, month, or year.
        /// </summary>
        [HttpGet("revenue-statistics")]
        public async Task<ActionResult<RevenueStatisticsDto>> GetRevenueStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string groupBy = "month")
        {
            try
            {
                var (from, toExclusive) = NormalizeDateRange(fromDate, toDate, defaultMonths: 6);
                var normalizedGroupBy = NormalizeGroupBy(groupBy);
                var result = await GetRevenueStatisticsAsync(from, toExclusive, normalizedGroupBy);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get top-selling products for a date range.
        /// </summary>
        [HttpGet("top-products")]
        public async Task<ActionResult<IEnumerable<TopProductDto>>> GetTopProducts(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int limit = 10)
        {
            try
            {
                var (from, toExclusive) = NormalizeDateRange(fromDate, toDate, defaultMonths: 6);
                var safeLimit = limit <= 0 || limit > 100 ? 10 : limit;
                var products = await GetTopProductsAsync(from, toExclusive, safeLimit);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top products");
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


        private async Task<RevenueStatisticsDto> GetRevenueStatisticsAsync(DateTime from, DateTime toExclusive, string groupBy)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.CreatedAt >= from && o.CreatedAt < toExclusive)
                .ToListAsync();

            var paidOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            var pendingOrders = orders.Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped).ToList();
            var cancelledOrders = orders.Where(o => o.Status == OrderStatus.Cancelled).ToList();
            var returnedOrders = orders.Where(o => o.Status == OrderStatus.Returned).ToList();

            var revenueByPeriod = paidOrders
                .GroupBy(o => GetPeriodKey(o.CreatedAt, groupBy))
                .OrderBy(g => g.Key)
                .Select(g => new RevenueByPeriodDto
                {
                    Period = FormatPeriod(g.Key, groupBy),
                    Label = FormatPeriodLabel(g.Key, groupBy),
                    Orders = g.Count(),
                    Revenue = g.Sum(o => o.TotalPrice),
                    DiscountAmount = g.Sum(o => o.DiscountAmount),
                    AverageOrderValue = g.Count() == 0 ? 0 : Math.Round(g.Sum(o => o.TotalPrice) / g.Count(), 2)
                })
                .ToList();

            var revenueByStatus = orders
                .GroupBy(o => o.Status)
                .OrderBy(g => g.Key)
                .Select(g => new OrderStatusRevenueDto
                {
                    Status = g.Key.ToString(),
                    Orders = g.Count(),
                    Revenue = g.Sum(o => o.TotalPrice)
                })
                .ToList();

            var grossRevenue = paidOrders.Sum(o => o.TotalPrice);
            var paidOrderCount = paidOrders.Count;

            return new RevenueStatisticsDto
            {
                FromDate = from,
                ToDate = toExclusive.AddTicks(-1),
                GroupBy = groupBy,
                TotalOrders = orders.Count,
                PaidOrders = paidOrderCount,
                PendingOrders = pendingOrders.Count,
                CancelledOrders = cancelledOrders.Count,
                ReturnedOrders = returnedOrders.Count,
                GrossRevenue = grossRevenue,
                PendingRevenue = pendingOrders.Sum(o => o.TotalPrice),
                CancelledRevenue = cancelledOrders.Sum(o => o.TotalPrice),
                ReturnedRevenue = returnedOrders.Sum(o => o.TotalPrice),
                DiscountAmount = paidOrders.Sum(o => o.DiscountAmount),
                ShippingRevenue = paidOrders.Sum(o => o.ShippingCost),
                AverageOrderValue = paidOrderCount == 0 ? 0 : Math.Round(grossRevenue / paidOrderCount, 2),
                RevenueByPeriod = revenueByPeriod,
                RevenueByStatus = revenueByStatus
            };
        }

        private async Task<List<TopProductDto>> GetTopProductsAsync(DateTime from, DateTime toExclusive, int limit)
        {
            var items = await _context.OrderItems
                .AsNoTracking()
                .Include(i => i.Order)
                .Include(i => i.Product)
                .ThenInclude(p => p!.Category)
                .Where(i => i.Order != null &&
                    !i.IsDeleted &&
                    !i.Order.IsDeleted &&
                    i.Order.Status == OrderStatus.Delivered &&
                    i.Order.CreatedAt >= from &&
                    i.Order.CreatedAt < toExclusive)
                .Select(i => new
                {
                    i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : string.Empty,
                    SKU = i.Product != null ? i.Product.SKU : null,
                    CategoryName = i.Product != null && i.Product.Category != null ? i.Product.Category.Name : null,
                    CurrentPrice = i.Product != null ? (i.Product.DiscountPrice ?? i.Product.Price) : i.UnitPrice,
                    i.Quantity,
                    i.TotalPrice
                })
                .ToListAsync();

            var inventoryByProduct = await _context.Inventories
                .AsNoTracking()
                .Where(i => !i.IsDeleted)
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvailableQuantity = g.Sum(i => i.Quantity - i.ReservedQuantity)
                })
                .ToDictionaryAsync(i => i.ProductId, i => i.AvailableQuantity);

            return items
                .GroupBy(i => new { i.ProductId, i.ProductName, i.SKU, i.CategoryName, i.CurrentPrice })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    SKU = g.Key.SKU,
                    CategoryName = g.Key.CategoryName,
                    CurrentPrice = g.Key.CurrentPrice,
                    TotalSold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.TotalPrice),
                    AvailableQuantity = inventoryByProduct.TryGetValue(g.Key.ProductId, out var availableQuantity) ? availableQuantity : 0
                })
                .OrderByDescending(p => p.TotalSold)
                .ThenByDescending(p => p.Revenue)
                .Take(limit)
                .ToList();
        }

        private static (DateTime From, DateTime ToExclusive) NormalizeDateRange(DateTime? fromDate, DateTime? toDate, int defaultMonths)
        {
            var today = DateTime.UtcNow.Date;
            var to = toDate?.Date.AddDays(1) ?? today.AddDays(1);
            var from = fromDate?.Date ?? to.AddMonths(-defaultMonths);

            from = EnsureUtc(from);
            to = EnsureUtc(to);

            if (from >= to)
                throw new ArgumentException("fromDate must be before toDate.");

            return (from, to);
        }

        private static string NormalizeGroupBy(string groupBy)
        {
            var normalized = string.IsNullOrWhiteSpace(groupBy) ? "month" : groupBy.Trim().ToLowerInvariant();
            return normalized switch
            {
                "day" => "day",
                "month" => "month",
                "year" => "year",
                _ => throw new ArgumentException("groupBy must be day, month, or year.")
            };
        }

        private static DateTime GetPeriodKey(DateTime value, string groupBy)
        {
            return groupBy switch
            {
                "day" => new DateTime(value.Year, value.Month, value.Day),
                "month" => new DateTime(value.Year, value.Month, 1),
                "year" => new DateTime(value.Year, 1, 1),
                _ => new DateTime(value.Year, value.Month, 1)
            };
        }

        private static string FormatPeriod(DateTime value, string groupBy)
        {
            return groupBy switch
            {
                "day" => value.ToString("yyyy-MM-dd"),
                "month" => value.ToString("yyyy-MM"),
                "year" => value.ToString("yyyy"),
                _ => value.ToString("yyyy-MM")
            };
        }

        private static string FormatPeriodLabel(DateTime value, string groupBy)
        {
            return groupBy switch
            {
                "day" => value.ToString("dd/MM/yyyy"),
                "month" => value.ToString("MM/yyyy"),
                "year" => value.ToString("yyyy"),
                _ => value.ToString("MM/yyyy")
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