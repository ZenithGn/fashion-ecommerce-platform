using System.ComponentModel.DataAnnotations;

namespace FashionEcommerce.Core.Entities
{
    public class Permission : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string ActionName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
