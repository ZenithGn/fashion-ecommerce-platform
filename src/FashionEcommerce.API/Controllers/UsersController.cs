using FashionEcommerce.Services.Models.Users;
using FashionEcommerce.Services.Models.Rbac;
using FashionEcommerce.Core.Entities;
using FashionEcommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get a list of all active users with search and filter parameters (Admin/Manager/Staff only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<IEnumerable<UserListItemDto>>> GetUsers(
            [FromQuery] string? search = null,
            [FromQuery] int? roleId = null,
            [FromQuery] bool? isActive = null)
        {
            var users = await _userService.GetUsersAsync(search, roleId, isActive);

            return Ok(users.Select(user => new UserListItemDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role?.RoleName ?? string.Empty,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                UpdatedAt = user.UpdatedAt
            }));
        }

        /// <summary>
        /// Get a user's details by ID (Admin/Manager/Staff only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<ActionResult<UserProfileDto>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user == null ? NotFound("User not found") : Ok(MapProfile(user));
        }

        /// <summary>
        /// Get the profile of the current logged-in user
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(MapProfile(user));
        }

        /// <summary>
        /// Update the current logged-in user's profile information
        /// </summary>
        [HttpPut("me")]
        public async Task<ActionResult<UserProfileDto>> UpdateMyProfile([FromBody] UpdateUserProfileRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest("First name and last name are required.");
            }

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

            await _userService.UpdateUserAsync(user);

            return Ok(MapProfile(user));
        }

        /// <summary>
        /// Update a user's role (Admin only)
        /// </summary>
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserProfileDto>> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
        {
            var user = await _userService.UpdateUserRoleAsync(id, request.RoleId);
            if (user == null)
                return BadRequest("User not found or role is invalid");

            return Ok(MapProfile(user));
        }

        /// <summary>
        /// Update a user's active status (Admin/Manager only)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<UserProfileDto>> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            if (User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString())
                return BadRequest("You cannot change your own active status");

            var user = request.IsActive 
                ? await _userService.UnlockUserAsync(id) 
                : await _userService.LockUserAsync(id);

            if (user == null)
                return NotFound("User not found");

            return Ok(MapProfile(user));
        }

        /// <summary>
        /// Lock a user account, preventing login (Admin/Manager only)
        /// </summary>
        [HttpPost("{id}/lock")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<UserProfileDto>> LockUser(int id)
        {
            return await SetUserActiveStatus(id, false);
        }

        /// <summary>
        /// Unlock a locked user account (Admin/Manager only)
        /// </summary>
        [HttpPost("{id}/unlock")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<UserProfileDto>> UnlockUser(int id)
        {
            return await SetUserActiveStatus(id, true);
        }

        private async Task<ActionResult<UserProfileDto>> SetUserActiveStatus(int id, bool isActive)
        {
            if (User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString())
                return BadRequest("You cannot change your own active status");

            var user = isActive 
                ? await _userService.UnlockUserAsync(id) 
                : await _userService.LockUserAsync(id);

            if (user == null)
                return NotFound("User not found");

            return Ok(MapProfile(user));
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }

        private static UserProfileDto MapProfile(User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role?.RoleName ?? string.Empty,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
