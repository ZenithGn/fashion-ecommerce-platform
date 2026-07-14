namespace FashionEcommerce.Services.Models.Rbac
{
    public sealed class PermissionDto
    {
        public int Id { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public sealed class RoleDto
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    public sealed class CreateRoleRequest
    {
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }

    public sealed class UpdateRoleRequest
    {
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }

    public sealed class CreatePermissionRequest
    {
        public string ActionName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public sealed class UpdatePermissionRequest
    {
        public string ActionName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public sealed class AssignPermissionsRequest
    {
        public List<int> PermissionIds { get; set; } = new();
    }

    public sealed class UpdateUserRoleRequest
    {
        public int RoleId { get; set; }
    }
}
