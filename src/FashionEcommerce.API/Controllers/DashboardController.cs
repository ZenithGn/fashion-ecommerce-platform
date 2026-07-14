using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Models.Dashboard;
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

                return Ok(new DashboardOverviewDto
                {
                    Summary = await GetSummaryAsync(range.From, range.ToExclusive),
                    RevenueChart = await GetRevenueChartAsync(range.From, range.ToExclusive, groupBy),
                    RecentOrders = await GetRecentOrdersAsync(range.From, range.ToExclusive),
                    StockAlerts = await GetStockAlertsAsync(),
                    TopProducts = await GetTopProductsAsync(range.From, range.ToExclusive, 10)
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
        /// Get admin dashboard with summary, revenue trend, recent orders, stock alerts, and top products.
        /// </summary>
        [HttpGet("admin")]
        public async Task<ActionResult<AdminDashboardDto>> GetAdminDashboard(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var range = NormalizeDateRange(fromDate, toDate, defaultMonths: 6);
                var revenueStatistics = await GetRevenueStatisticsAsync(range.From, range.ToExclusive, "month");

                return Ok(new AdminDashboardDto
                {
                    Summary = await GetSummaryAsync(range.From, range.ToExclusive),
                    RevenueStatistics = revenueStatistics,
                    RecentOrders = await GetRecentOrdersAsync(range.From, range.ToExclusive),
                    StockAlerts = await GetStockAlertsAsync(),
                    TopProducts = await GetTopProductsAsync(range.From, range.ToExclusive, 10),
                    RevenueTrend = revenueStatistics.RevenueByPeriod
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
                var range = NormalizeDateRange(fromDate, toDate, defaultMonths: 6);
                return Ok(await GetRevenueStatisticsAsync(range.From, range.ToExclusive, NormalizeGroupBy(groupBy)));
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
        /// Get dashboard summary metrics with optional date range filters.
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var range = NormalizeDateRange(from, to);
                return Ok(await GetSummaryAsync(range.From, range.ToExclusive));
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
        /// Get revenue chart points grouped by day, month, or year.
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
                return Ok(await GetRevenueChartAsync(range.From, range.ToExclusive, groupBy));
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
            [FromQuery] DateTime? to = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int limit = 10)
        {
            try
            {
                var range = NormalizeDateRange(from ?? fromDate, to ?? toDate, defaultMonths: 6);
                var safeLimit = limit <= 0 || limit > 100 ? 10 : limit;
                return Ok(await GetTopProductsAsync(range.From, range.ToExclusive, safeLimit));
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
                return Ok(await GetRecentOrdersAsync(range.From, range.ToExclusive));
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
                return Ok(await GetStockAlertsAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock alerts");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<DashboardSummaryDto> GetSummaryAsync(DateTime from, DateTime toExclusive)
        {
            var orderQuery = GetOrdersInRange(from, toExclusive);
            var revenueOrderQuery = orderQuery.Where(IsRevenueStatusExpression());
            var completedOrders = await orderQuery.CountAsync(o => o.Status == OrderStatus.Delivered);
            var totalRevenue = await revenueOrderQuery.SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            return new DashboardSummaryDto
            {
                From = from,
                To = toExclusive.AddTicks(-1),
                TotalUsers = await _context.Users.AsNoTracking().CountAsync(u => !u.IsDeleted),
                TotalCustomers = await _context.Users.AsNoTracking().CountAsync(u => !u.IsDeleted && u.Role != null && u.Role.RoleName == "Customer"),
                TotalManagers = await _context.Users.AsNoTracking().CountAsync(u => !u.IsDeleted && u.Role != null && u.Role.RoleName == "Manager"),
                TotalStaff = await _context.Users.AsNoTracking().CountAsync(u => !u.IsDeleted && u.Role != null && u.Role.RoleName == "Staff"),
                TotalProducts = await _context.Products.AsNoTracking().CountAsync(p => !p.IsDeleted),
                ActiveProducts = await _context.Products.AsNoTracking().CountAsync(p => !p.IsDeleted && p.IsActive),
                TotalCategories = await _context.Categories.AsNoTracking().CountAsync(c => !c.IsDeleted),
                TotalInventories = await _context.Inventories.AsNoTracking().CountAsync(i => !i.IsDeleted),
                LowStockProducts = await _context.Inventories.AsNoTracking().CountAsync(i => !i.IsDeleted && i.Quantity - i.ReservedQuantity <= StockAlertThreshold),
                TotalOrders = await orderQuery.CountAsync(),
                PendingOrders = await orderQuery.CountAsync(o => o.Status == OrderStatus.Pending),
                ProcessingOrders = await orderQuery.CountAsync(o => o.Status == OrderStatus.Processing),
                CompletedOrders = completedOrders,
                CancelledOrders = await orderQuery.CountAsync(o => o.Status == OrderStatus.Cancelled),
                TotalRevenue = totalRevenue,
                PendingRevenue = await orderQuery
                    .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped)
                    .SumAsync(o => (decimal?)o.TotalPrice) ?? 0,
                AverageOrderValue = completedOrders == 0 ? 0 : Math.Round(totalRevenue / completedOrders, 2)
            };
        }

        private async Task<RevenueStatisticsDto> GetRevenueStatisticsAsync(DateTime from, DateTime toExclusive, string groupBy)
        {
            var orders = await GetOrdersInRange(from, toExclusive).ToListAsync();
            var paidOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            var pendingOrders = orders.Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped).ToList();
            var cancelledOrders = orders.Where(o => o.Status == OrderStatus.Cancelled).ToList();
            var returnedOrders = orders.Where(o => o.Status == OrderStatus.Returned).ToList();
            var grossRevenue = paidOrders.Sum(o => o.TotalPrice);

            return new RevenueStatisticsDto
            {
                FromDate = from,
                ToDate = toExclusive.AddTicks(-1),
                GroupBy = groupBy,
                TotalOrders = orders.Count,
                PaidOrders = paidOrders.Count,
                PendingOrders = pendingOrders.Count,
                CancelledOrders = cancelledOrders.Count,
                ReturnedOrders = returnedOrders.Count,
                GrossRevenue = grossRevenue,
                PendingRevenue = pendingOrders.Sum(o => o.TotalPrice),
                CancelledRevenue = cancelledOrders.Sum(o => o.TotalPrice),
                ReturnedRevenue = returnedOrders.Sum(o => o.TotalPrice),
                DiscountAmount = paidOrders.Sum(o => o.DiscountAmount),
                ShippingRevenue = paidOrders.Sum(o => o.ShippingCost),
                AverageOrderValue = paidOrders.Count == 0 ? 0 : Math.Round(grossRevenue / paidOrders.Count, 2),
                RevenueByPeriod = paidOrders
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
                    .ToList(),
                RevenueByStatus = orders
                    .GroupBy(o => o.Status)
                    .OrderBy(g => g.Key)
                    .Select(g => new OrderStatusRevenueDto
                    {
                        Status = g.Key.ToString(),
                        Orders = g.Count(),
                        Revenue = g.Sum(o => o.TotalPrice)
                    })
                    .ToList()
            };
        }

        private async Task<RevenueChartDto> GetRevenueChartAsync(DateTime from, DateTime toExclusive, string groupBy)
        {
            var normalizedGroupBy = NormalizeGroupBy(groupBy);
            var orders = await GetOrdersInRange(from, toExclusive)
                .Where(IsRevenueStatusExpression())
                .Select(o => new { o.CreatedAt, o.TotalPrice })
                .ToListAsync();

            var points = orders
                .GroupBy(o => GetPeriodKey(o.CreatedAt, normalizedGroupBy))
                .OrderBy(g => g.Key)
                .Select(g => CreateRevenuePoint(g.Key, normalizedGroupBy, g.Count(), g.Sum(o => o.TotalPrice)))
                .ToList();

            return new RevenueChartDto
            {
                From = from,
                To = toExclusive.AddTicks(-1),
                GroupBy = normalizedGroupBy,
                TotalOrders = points.Sum(p => p.Orders),
                TotalRevenue = points.Sum(p => p.Revenue),
                Points = points
            };
        }

        private async Task<List<RecentOrderDto>> GetRecentOrdersAsync(DateTime from, DateTime toExclusive)
        {
            return await GetOrdersInRange(from, toExclusive)
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

        private async Task<List<StockAlertDto>> GetStockAlertsAsync()
        {
            return await _context.Inventories
                .AsNoTracking()
                .Where(i => !i.IsDeleted && i.Quantity - i.ReservedQuantity <= StockAlertThreshold)
                .Include(i => i.Product)
                .ThenInclude(p => p!.Category)
                .OrderBy(i => i.Quantity - i.ReservedQuantity)
                .Select(i => new StockAlertDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : string.Empty,
                    SKU = i.Product != null ? i.Product.SKU : null,
                    CategoryName = i.Product != null && i.Product.Category != null ? i.Product.Category.Name : null,
                    Quantity = i.Quantity,
                    ReservedQuantity = i.ReservedQuantity,
                    AvailableQuantity = i.Quantity - i.ReservedQuantity,
                    Location = i.Location
                })
                .ToListAsync();
        }

        private IQueryable<Order> GetOrdersInRange(DateTime from, DateTime toExclusive)
        {
            return _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.CreatedAt >= from && o.CreatedAt < toExclusive);
        }

        private static System.Linq.Expressions.Expression<Func<Order, bool>> IsRevenueStatusExpression()
        {
            return o => o.Status == OrderStatus.Delivered;
        }

        private static (DateTime From, DateTime ToExclusive) NormalizeDateRange(DateTime? fromDate, DateTime? toDate, int? defaultMonths = null)
        {
            var today = DateTime.UtcNow.Date;
            var toExclusive = EnsureUtc(toDate?.Date.AddDays(1) ?? today.AddDays(1));
            var from = EnsureUtc(fromDate?.Date ?? (defaultMonths.HasValue ? toExclusive.AddMonths(-defaultMonths.Value) : toExclusive.AddDays(-DefaultChartDays)));

            if (from >= toExclusive)
                throw new ArgumentException("fromDate must be before toDate.");

            return (from, toExclusive);
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

        private static RevenueChartPointDto CreateRevenuePoint(DateTime value, string groupBy, int orders, decimal revenue)
        {
            return new RevenueChartPointDto
            {
                Year = value.Year,
                Month = groupBy == "year" ? null : value.Month,
                Day = groupBy == "day" ? value.Day : null,
                Label = FormatPeriod(value, groupBy),
                Orders = orders,
                Revenue = revenue,
                AverageOrderValue = orders == 0 ? 0 : Math.Round(revenue / orders, 2)
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
    }
}
