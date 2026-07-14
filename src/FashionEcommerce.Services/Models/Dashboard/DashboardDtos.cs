namespace FashionEcommerce.Services.Models.Dashboard
{
    public sealed class DashboardSummaryDto
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalManagers { get; set; }
        public int TotalStaff { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalInventories { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public sealed class RevenueChartPointDto
    {
        public int Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public string Label { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public sealed class RevenueChartDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string GroupBy { get; set; } = "day";
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public List<RevenueChartPointDto> Points { get; set; } = new();
    }

    public sealed class RecentOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string? CategoryName { get; set; }
        public decimal CurrentPrice { get; set; }
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
        public int AvailableQuantity { get; set; }
    }

    public sealed class StockAlertDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string? CategoryName { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string? Location { get; set; }
    }


    public sealed class AdminDashboardDto
    {
        public DashboardSummaryDto Summary { get; set; } = new();
        public RevenueStatisticsDto RevenueStatistics { get; set; } = new();
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<StockAlertDto> StockAlerts { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
        public List<RevenueByPeriodDto> RevenueTrend { get; set; } = new();
    }

    public sealed class RevenueStatisticsDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GroupBy { get; set; } = "month";
        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ReturnedOrders { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
        public decimal CancelledRevenue { get; set; }
        public decimal ReturnedRevenue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<RevenueByPeriodDto> RevenueByPeriod { get; set; } = new();
        public List<OrderStatusRevenueDto> RevenueByStatus { get; set; } = new();
    }

    public sealed class RevenueByPeriodDto
    {
        public string Period { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public sealed class OrderStatusRevenueDto
    {
        public string Status { get; set; } = string.Empty;
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
    }

    public sealed class DashboardOverviewDto
    {
        public DashboardSummaryDto Summary { get; set; } = new();
        public RevenueChartDto RevenueChart { get; set; } = new();
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<StockAlertDto> StockAlerts { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
    }
}
