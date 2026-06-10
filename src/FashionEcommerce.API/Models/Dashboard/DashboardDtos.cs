namespace FashionEcommerce.API.Models.Dashboard
{
    public sealed class DashboardSummaryDto
    {
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
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
    }

    public sealed class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Label { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
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

    public sealed class DashboardOverviewDto
    {
        public DashboardSummaryDto Summary { get; set; } = new();
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<StockAlertDto> StockAlerts { get; set; } = new();
    }
}