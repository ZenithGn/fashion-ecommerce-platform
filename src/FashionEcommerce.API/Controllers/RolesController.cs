using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Models.Rbac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly FashionEcommerceDbContext _context;

        public RolesController(FashionEcommerceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get a list of all active roles in the system with their associated permissions (Admin only)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .Where(r => !r.IsDeleted)
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .OrderBy(r => r.Id)
                .ToListAsync();

            return Ok(roles.Select(MapRole));
        }

        /// <summary>
        /// Get details of a specific role by ID with permissions (Admin only)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(int id)
        {
            var role = await _context.Roles
                .AsNoTracking()
                .Where(r => r.Id == id && !r.IsDeleted)
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync();

            return role == null ? NotFound("Role not found") : Ok(MapRole(role));
        }

        /// <summary>
        /// Create a new system role and assign permissions (Admin only)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleRequest request)
        {
            var validation = await ValidateRoleRequestAsync(request.RoleName, request.PermissionIds);
            if (validation != null)
                return validation;

            var normalizedName = request.RoleName.Trim();
            var exists = await _context.Roles.AnyAsync(r => !r.IsDeleted && r.RoleName.ToLower() == normalizedName.ToLower());
            if (exists)
                return Conflict("Role name already exists");

            var role = new Role
            {
                RoleName = normalizedName,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            await ReplaceRolePermissionsAsync(role.Id, request.PermissionIds);

            var created = await LoadRoleAsync(role.Id);
            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, MapRole(created!));
        }

        /// <summary>
        /// Update an existing role definition and replace its permissions (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<RoleDto>> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (role == null)
                return NotFound("Role not found");

            var validation = await ValidateRoleRequestAsync(request.RoleName, request.PermissionIds);
            if (validation != null)
                return validation;

            var normalizedName = request.RoleName.Trim();
            var duplicate = await _context.Roles.AnyAsync(r => r.Id != id && !r.IsDeleted && r.RoleName.ToLower() == normalizedName.ToLower());
            if (duplicate)
                return Conflict("Role name already exists");

            role.RoleName = normalizedName;
            role.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await ReplaceRolePermissionsAsync(role.Id, request.PermissionIds);

            var updated = await LoadRoleAsync(role.Id);
            return Ok(MapRole(updated!));
        }

        /// <summary>
        /// Soft delete a role by ID (Admin only, system roles cannot be deleted)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (role == null)
                return NotFound("Role not found");

            var isSystemRole = role.RoleName is "Admin" or "Manager" or "Staff" or "Customer";
            if (isSystemRole)
                return BadRequest("System roles cannot be deleted");

            var hasUsers = await _context.Users.AnyAsync(u => !u.IsDeleted && u.RoleId == id);
            if (hasUsers)
                return Conflict("Cannot delete a role assigned to users");

            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Directly assign a collection of permission IDs to a role (Admin only)
        /// </summary>
        [HttpPut("{id}/permissions")]
        public async Task<ActionResult<RoleDto>> AssignPermissions(int id, [FromBody] AssignPermissionsRequest request)
        {
            var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (role == null)
                return NotFound("Role not found");

            var validation = await ValidatePermissionIdsAsync(request.PermissionIds);
            if (validation != null)
                return validation;

            await ReplaceRolePermissionsAsync(id, request.PermissionIds);

            var updated = await LoadRoleAsync(id);
            return Ok(MapRole(updated!));
        }

        private async Task<ActionResult?> ValidateRoleRequestAsync(string roleName, List<int> permissionIds)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Role name is required");

            return await ValidatePermissionIdsAsync(permissionIds);
        }

        private async Task<ActionResult?> ValidatePermissionIdsAsync(List<int> permissionIds)
        {
            var distinctPermissionIds = permissionIds.Distinct().ToList();
            if (distinctPermissionIds.Count != permissionIds.Count)
                return BadRequest("Permission ids cannot contain duplicates");

            if (distinctPermissionIds.Count == 0)
                return null;

            var existingCount = await _context.Permissions.CountAsync(p => distinctPermissionIds.Contains(p.Id) && !p.IsDeleted);
            return existingCount == distinctPermissionIds.Count ? null : BadRequest("One or more permissions are invalid");
        }

        private async Task ReplaceRolePermissionsAsync(int roleId, List<int> permissionIds)
        {
            var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            var distinctPermissionIds = permissionIds.Distinct().ToList();
            foreach (var permissionId in distinctPermissionIds)
            {
                _context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
            }

            await _context.SaveChangesAsync();
        }

        private async Task<Role?> LoadRoleAsync(int roleId)
        {
            return await _context.Roles
                .AsNoTracking()
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);
        }

        private static RoleDto MapRole(Role role)
        {
            return new RoleDto
            {
                Id = role.Id,
                RoleName = role.RoleName,
                Description = role.Description,
                Permissions = role.RolePermissions
                    .Where(rp => rp.Permission != null && !rp.Permission.IsDeleted)
                    .OrderBy(rp => rp.PermissionId)
                    .Select(rp => new PermissionDto
                    {
                        Id = rp.PermissionId,
                        ActionName = rp.Permission.ActionName,
                        Description = rp.Permission.Description
                    })
                    .ToList()
            };
        }
    }
}
