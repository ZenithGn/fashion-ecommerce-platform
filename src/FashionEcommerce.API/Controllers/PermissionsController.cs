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
    public class PermissionsController : ControllerBase
    {
        private readonly FashionEcommerceDbContext _context;

        public PermissionsController(FashionEcommerceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissions()
        {
            var permissions = await _context.Permissions
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.ActionName)
                .Select(p => MapPermission(p))
                .ToListAsync();

            return Ok(permissions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionDto>> GetPermission(int id)
        {
            var permission = await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            return permission == null ? NotFound("Permission not found") : Ok(MapPermission(permission));
        }

        [HttpPost]
        public async Task<ActionResult<PermissionDto>> CreatePermission([FromBody] CreatePermissionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ActionName))
                return BadRequest("Action name is required");

            var actionName = request.ActionName.Trim();
            var exists = await _context.Permissions.AnyAsync(p => !p.IsDeleted && p.ActionName.ToLower() == actionName.ToLower());
            if (exists)
                return Conflict("Permission action already exists");

            var permission = new Permission
            {
                ActionName = actionName,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPermission), new { id = permission.Id }, MapPermission(permission));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PermissionDto>> UpdatePermission(int id, [FromBody] UpdatePermissionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ActionName))
                return BadRequest("Action name is required");

            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (permission == null)
                return NotFound("Permission not found");

            var actionName = request.ActionName.Trim();
            var duplicate = await _context.Permissions.AnyAsync(p => p.Id != id && !p.IsDeleted && p.ActionName.ToLower() == actionName.ToLower());
            if (duplicate)
                return Conflict("Permission action already exists");

            permission.ActionName = actionName;
            permission.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            permission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(MapPermission(permission));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (permission == null)
                return NotFound("Permission not found");

            var isAssigned = await _context.RolePermissions.AnyAsync(rp => rp.PermissionId == id);
            if (isAssigned)
                return Conflict("Cannot delete a permission assigned to roles");

            permission.IsDeleted = true;
            permission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static PermissionDto MapPermission(Permission permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                ActionName = permission.ActionName,
                Description = permission.Description
            };
        }
    }
}
