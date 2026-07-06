using FashionEcommerce.Core.Entities;

namespace FashionEcommerce.Services.Marketing
{
    public class MarketingCampaignQueryParameters
    {
        public bool IncludeInactive { get; set; } = true;
        public MarketingCampaignStatus? Status { get; set; }
        public MarketingChannel? Channel { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class MarketingCampaignDto
    {
        public int Id { get; set; }
        public string CampaignCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public MarketingChannel Channel { get; set; }
        public MarketingCampaignStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Budget { get; set; }
        public decimal ActualCost { get; set; }
        public string? TargetAudience { get; set; }
        public string? Goal { get; set; }
        public string? LandingPageUrl { get; set; }
        public int? VoucherId { get; set; }
        public string? VoucherCode { get; set; }
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public decimal Revenue { get; set; }
        public decimal ClickThroughRate { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal ReturnOnInvestment { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateMarketingCampaignDto
    {
        public string CampaignCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public MarketingChannel Channel { get; set; } = MarketingChannel.Other;
        public MarketingCampaignStatus Status { get; set; } = MarketingCampaignStatus.Draft;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public decimal Budget { get; set; }
        public decimal ActualCost { get; set; }
        public string? TargetAudience { get; set; }
        public string? Goal { get; set; }
        public string? LandingPageUrl { get; set; }
        public int? VoucherId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateMarketingCampaignDto
    {
        public string? CampaignCode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public MarketingChannel? Channel { get; set; }
        public MarketingCampaignStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Budget { get; set; }
        public decimal? ActualCost { get; set; }
        public string? TargetAudience { get; set; }
        public string? Goal { get; set; }
        public string? LandingPageUrl { get; set; }
        public int? VoucherId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateMarketingCampaignStatusDto
    {
        public MarketingCampaignStatus Status { get; set; }
    }

    public class UpdateMarketingCampaignMetricsDto
    {
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public decimal ActualCost { get; set; }
        public decimal Revenue { get; set; }
    }

    public class MarketingSummaryDto
    {
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public int RunningCampaigns { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalActualCost { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalImpressions { get; set; }
        public int TotalClicks { get; set; }
        public int TotalConversions { get; set; }
        public decimal AverageClickThroughRate { get; set; }
        public decimal AverageConversionRate { get; set; }
        public decimal OverallReturnOnInvestment { get; set; }
    }

    public class MarketingServiceResult<T>
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public MarketingServiceError Error { get; set; } = MarketingServiceError.None;
        public T? Data { get; set; }

        public static MarketingServiceResult<T> Success(T data) => new() { Succeeded = true, Data = data };

        public static MarketingServiceResult<T> Failure(MarketingServiceError error, string message) =>
            new() { Succeeded = false, Error = error, ErrorMessage = message };
    }

    public enum MarketingServiceError
    {
        None,
        Validation,
        NotFound,
        Conflict
    }
}
