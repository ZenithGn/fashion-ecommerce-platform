using FashionEcommerce.Services.Models.Users;
using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly FashionEcommerceDbContext _context;
        private readonly IUserService _userService;

        public UsersController(FashionEcommerceDbContext context, IUserService userService, ILogger<UsersController> logger)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<IEnumerable<UserListItemDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();

            return Ok(users.Select(user => new UserListItemDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role?.RoleName ?? string.Empty,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }));
        }

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

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            await _userService.UpdateUserAsync(user);

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
                IsActive = user.IsActive
            };
        }
    }
}