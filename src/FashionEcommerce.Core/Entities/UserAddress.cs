using System.ComponentModel.DataAnnotations;

namespace FashionEcommerce.Core.Entities
{
    public class UserAddress : BaseEntity
    {
        public int UserId { get; set; }

        [StringLength(100)]
        public string ReceiverName { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(500)]
        public string AddressLine { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Ward { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        public bool IsDefault { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
