using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionEcommerce.Core.Entities
{
    /// <summary>
    /// Voucher entity - represents discount codes that can be applied to orders.
    /// </summary>
    public class Voucher : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public VoucherDiscountType DiscountType { get; set; } = VoucherDiscountType.FixedAmount;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinimumOrderAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaximumDiscountAmount { get; set; }

        public int? UsageLimit { get; set; }

        public int UsedCount { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public enum VoucherDiscountType
    {
        FixedAmount = 0,
        Percentage = 1
    }
}
