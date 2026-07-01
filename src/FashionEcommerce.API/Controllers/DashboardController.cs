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
    [Authorize(Roles = "Admin,Manager,Staff")]
    public class DashboardController : ControllerBase
    {
        private const int RecentOrderLimit = 10;
        private const int StockAlertThreshold = 10;
        private const int DefaultChartDays = 30;

        private readonly FashionEcommerceDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(FashionEcommerceDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get full dashboard overview with summary, revenue chart, recent orders, top products, and stock alerts.
        /// </summary>
        [HttpGet("overview")]
        public async Task<ActionResult<DashboardOverviewDto>> GetOverview(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string groupBy = "day")
        {
            try
            {
                var range = NormalizeDateRange(from, to);

                var summaryTask = GetSummaryAsync(range.From, range.To);
                var revenueChartTask = GetRevenueChartAsync(range.From, range.To, groupBy);
                var recentOrdersTask = GetRecentOrdersAsync(range.From, range.To);
                var stockAlertsTask = GetStockAlertsAsync();
                var topProductsTask = GetTopProductsAsync(range.From, range.To);

                await Task.WhenAll(summaryTask, revenueChartTask, recentOrdersTask, stockAlertsTask, topProductsTask);

                return Ok(new DashboardOverviewDto
                {
                    Summary = summaryTask.Result,
                    RevenueChart = revenueChartTask.Result,
                    RecentOrders = recentOrdersTask.Result,
                    StockAlerts = stockAlertsTask.Result,
                    TopProducts = topProductsTask.Result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard overview");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get dashboard summary metrics with optional date range filters for order and revenue metrics.
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var range = NormalizeDateRange(from, to);
                var summary = await GetSummaryAsync(range.From, range.To);
                return Ok(summary);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get revenue chart points grouped by day or month.
        /// </summary>
        [HttpGet("revenue-chart")]
        public async Task<ActionResult<RevenueChartDto>> GetRevenueChart(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string groupBy = "day")
        {
            try
            {
                var range = NormalizeDateRange(from, to);
                var chart = await GetRevenueChartAsync(range.From, range.To, groupBy);
                return Ok(chart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue chart");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get the best-selling products in a date range.
        /// </summary>
        [HttpGet("top-products")]
        public async Task<ActionResult<IEnumerable<TopProductDto>>> GetTopProducts(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var range = NormalizeDateRange(from, to);
                var products = await GetTopProductsAsync(range.From, range.To);
                return Ok(products);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top products");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get the latest orders for the dashboard.
        /// </summary>
        [HttpGet("recent-orders")]
        public async Task<ActionResult<IEnumerable<RecentOrderDto>>> GetRecentOrders(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var range = NormalizeDateRange(from, to);
                var recentOrders = await GetRecentOrdersAsync(range.From, range.To);
                return Ok(recentOrders);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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

        private async Task<DashboardSummaryDto> GetSummaryAsync(DateTime from, DateTime to)
        {
            var orderQuery = GetOrdersInRange(from, to);
            var revenueOrderQuery = orderQuery.Where(IsRevenueStatusExpression());

            var totalUsersTask = _context.Users.AsNoTracking().CountAsync(u => !u.IsDeleted);
            var totalCustomersTask = _context.Users.AsNoTracking().CountAsync(u => !u.IsDeleted && u.Role != null && u.Role.RoleName == "Customer");
            var totalManagersTask = _context.Users.AsNoTracking().CountAsync(u => !u.IsDeleted && u.Role != null && u.Role.RoleName == "Manager");
            var totalStaffTask = _context.Users.AsNoTracking().CountAsync(u => !u.IsDeleted && u.Role != null && u.Role.RoleName == "Staff");
            var totalProductsTask = _context.Products.AsNoTracking().CountAsync(p => !p.IsDeleted);
            var activeProductsTask = _context.Products.AsNoTracking().CountAsync(p => !p.IsDeleted && p.IsActive);
            var totalCategoriesTask = _context.Categories.AsNoTracking().CountAsync(c => !c.IsDeleted);
            var totalInventoriesTask = _context.Inventories.AsNoTracking().CountAsync(i => !i.IsDeleted);
            var lowStockProductsTask = _context.Inventories.AsNoTracking().CountAsync(i => !i.IsDeleted && i.AvailableQuantity <= StockAlertThreshold);
            var totalOrdersTask = orderQuery.CountAsync();
            var pendingOrdersTask = orderQuery.CountAsync(o => o.Status == OrderStatus.Pending);
            var processingOrdersTask = orderQuery.CountAsync(o => o.Status == OrderStatus.Processing);
            var completedOrdersTask = orderQuery.CountAsync(o => o.Status == OrderStatus.Delivered);
            var cancelledOrdersTask = orderQuery.CountAsync(o => o.Status == OrderStatus.Cancelled);
            var totalRevenueTask = revenueOrderQuery.SumAsync(o => (decimal?)o.TotalPrice);
            var pendingRevenueTask = orderQuery
                .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing)
                .SumAsync(o => (decimal?)o.TotalPrice);

            await Task.WhenAll(
                totalUsersTask,
                totalCustomersTask,
                totalManagersTask,
                totalStaffTask,
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

            var totalRevenue = totalRevenueTask.Result ?? 0;
            var completedOrders = completedOrdersTask.Result;

            return new DashboardSummaryDto
            {
                From = from,
                To = to,
                TotalUsers = totalUsersTask.Result,
                TotalCustomers = totalCustomersTask.Result,
                TotalManagers = totalManagersTask.Result,
                TotalStaff = totalStaffTask.Result,
                TotalProducts = totalProductsTask.Result,
                ActiveProducts = activeProductsTask.Result,
                TotalCategories = totalCategoriesTask.Result,
                TotalInventories = totalInventoriesTask.Result,
                LowStockProducts = lowStockProductsTask.Result,
                TotalOrders = totalOrdersTask.Result,
                PendingOrders = pendingOrdersTask.Result,
                ProcessingOrders = processingOrdersTask.Result,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrdersTask.Result,
                TotalRevenue = totalRevenue,
                PendingRevenue = pendingRevenueTask.Result ?? 0,
                AverageOrderValue = completedOrders == 0 ? 0 : totalRevenue / completedOrders
            };
        }

        private async Task<RevenueChartDto> GetRevenueChartAsync(DateTime from, DateTime to, string groupBy)
        {
            var normalizedGroupBy = NormalizeGroupBy(groupBy);
            var orders = await GetOrdersInRange(from, to)
                .Where(IsRevenueStatusExpression())
                .Select(o => new { o.CreatedAt, o.TotalPrice })
                .ToListAsync();

            var points = normalizedGroupBy == "month"
                ? orders
                    .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => CreateRevenuePoint(g.Key.Year, g.Key.Month, null, $"{g.Key.Year}-{g.Key.Month:00}", g.Count(), g.Sum(o => o.TotalPrice)))
                    .ToList()
                : orders
                    .GroupBy(o => o.CreatedAt.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => CreateRevenuePoint(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.ToString("yyyy-MM-dd"), g.Count(), g.Sum(o => o.TotalPrice)))
                    .ToList();

            return new RevenueChartDto
            {
                From = from,
                To = to,
                GroupBy = normalizedGroupBy,
                TotalOrders = points.Sum(p => p.Orders),
                TotalRevenue = points.Sum(p => p.Revenue),
                Points = points
            };
        }

        private async Task<List<RecentOrderDto>> GetRecentOrdersAsync(DateTime from, DateTime to)
        {
            return await GetOrdersInRange(from, to)
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

        private async Task<List<TopProductDto>> GetTopProductsAsync(DateTime from, DateTime to)
        {
            return await _context.OrderItems
                .AsNoTracking()
                .Where(oi => !oi.IsDeleted
                    && oi.Order != null
                    && !oi.Order.IsDeleted
                    && oi.Order.CreatedAt >= from
                    && oi.Order.CreatedAt <= to
                    && oi.Order.Status == OrderStatus.Delivered)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .Join(_context.Products.AsNoTracking().Include(p => p.Category),
                    sales => sales.ProductId,
                    product => product.Id,
                    (sales, product) => new TopProductDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        SKU = product.SKU,
                        CategoryName = product.Category != null ? product.Category.Name : null,
                        CurrentPrice = product.Price,
                        TotalSold = sales.TotalSold,
                        Revenue = sales.Revenue,
                        AvailableQuantity = product.Inventories.Sum(i => (int?)(i.Quantity - i.ReservedQuantity)) ?? 0
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

        private IQueryable<Order> GetOrdersInRange(DateTime from, DateTime to)
        {
            return _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.CreatedAt >= from && o.CreatedAt <= to);
        }

        private static System.Linq.Expressions.Expression<Func<Order, bool>> IsRevenueStatusExpression()
        {
            return o => o.Status == OrderStatus.Delivered;
        }

        private static (DateTime From, DateTime To) NormalizeDateRange(DateTime? from, DateTime? to)
        {
            var normalizedTo = (to ?? DateTime.UtcNow).ToUniversalTime();
            var normalizedFrom = (from ?? normalizedTo.AddDays(-DefaultChartDays)).ToUniversalTime();

            normalizedFrom = normalizedFrom.Date;
            normalizedTo = normalizedTo.Date.AddDays(1).AddTicks(-1);

            if (normalizedFrom > normalizedTo)
                throw new ArgumentException("From date must be earlier than or equal to To date");

            return (normalizedFrom, normalizedTo);
        }

        private static string NormalizeGroupBy(string groupBy)
        {
            var normalized = groupBy.Trim().ToLowerInvariant();
            return normalized switch
            {
                "day" => "day",
                "month" => "month",
                _ => throw new ArgumentException("groupBy must be either 'day' or 'month'")
            };
        }

        private static RevenueChartPointDto CreateRevenuePoint(int year, int? month, int? day, string label, int orders, decimal revenue)
        {
            return new RevenueChartPointDto
            {
                Year = year,
                Month = month,
                Day = day,
                Label = label,
                Orders = orders,
                Revenue = revenue,
                AverageOrderValue = orders == 0 ? 0 : revenue / orders
            };
        }
    }
}
