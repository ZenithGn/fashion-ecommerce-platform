using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionEcommerce.Core.Entities
{
    /// <summary>
    /// Marketing campaign entity - represents campaigns used for sales, branding, and promotions.
    /// </summary>
    public class MarketingCampaign : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string CampaignCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public MarketingChannel Channel { get; set; } = MarketingChannel.Other;

        public MarketingCampaignStatus Status { get; set; } = MarketingCampaignStatus.Draft;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Budget { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal ActualCost { get; set; }

        [StringLength(300)]
        public string? TargetAudience { get; set; }

        [StringLength(300)]
        public string? Goal { get; set; }

        [StringLength(500)]
        public string? LandingPageUrl { get; set; }

        public int? VoucherId { get; set; }

        public int Impressions { get; set; }

        public int Clicks { get; set; }

        public int Conversions { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Revenue { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey("VoucherId")]
        public virtual Voucher? Voucher { get; set; }
    }

    public enum MarketingChannel
    {
        Email = 0,
        SocialMedia = 1,
        Livestream = 2,
        SearchAds = 3,
        DisplayAds = 4,
        Affiliate = 5,
        Other = 6
    }

    public enum MarketingCampaignStatus
    {
        Draft = 0,
        Scheduled = 1,
        Running = 2,
        Paused = 3,
        Completed = 4,
        Cancelled = 5
    }
}
