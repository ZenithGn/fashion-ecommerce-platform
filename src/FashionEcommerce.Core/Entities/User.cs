using System.ComponentModel.DataAnnotations;

namespace FashionEcommerce.Core.Entities
{
    /// <summary>
    /// User entity - represents customers, staff and admins.
    /// </summary>
    public class User : BaseEntity
    {
        public int RoleId { get; set; } = 2;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual Cart? Cart { get; set; }
    }
}
