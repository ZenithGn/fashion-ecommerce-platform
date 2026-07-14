using FashionEcommerce.Services.Models.Users;
using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserAddressesController : ControllerBase
    {
        private readonly FashionEcommerceDbContext _context;

        public UserAddressesController(FashionEcommerceDbContext context, ILogger<UserAddressesController> logger)
        {
            _context = context;
        }

        /// <summary>
        /// Get all shipping addresses of the current logged-in user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserAddressDto>>> GetMyAddresses()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var addresses = await _context.UserAddresses
                .Where(address => address.UserId == userId.Value && !address.IsDeleted)
                .OrderByDescending(address => address.IsDefault)
                .ThenByDescending(address => address.CreatedAt)
                .ToListAsync();

            return Ok(addresses.Select(MapAddress));
        }

        /// <summary>
        /// Get details of a specific shipping address by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserAddressDto>> GetAddressById(int id)
        {
            var address = await GetOwnedAddressAsync(id);
            if (address == null)
            {
                return NotFound("Address not found");
            }

            return Ok(MapAddress(address));
        }

        /// <summary>
        /// Create a new shipping address for the logged-in user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserAddressDto>> CreateAddress([FromBody] CreateUserAddressRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (!IsValidRequest(request, out var errorMessage))
            {
                return BadRequest(errorMessage);
            }

            var hasAddresses = await _context.UserAddresses.AnyAsync(address => address.UserId == userId.Value && !address.IsDeleted);

            var address = new UserAddress
            {
                UserId = userId.Value,
                ReceiverName = request.ReceiverName.Trim(),
                Phone = request.Phone.Trim(),
                AddressLine = request.AddressLine.Trim(),
                Ward = string.IsNullOrWhiteSpace(request.Ward) ? null : request.Ward.Trim(),
                District = string.IsNullOrWhiteSpace(request.District) ? null : request.District.Trim(),
                Province = string.IsNullOrWhiteSpace(request.Province) ? null : request.Province.Trim(),
                IsDefault = request.IsDefault || !hasAddresses,
                CreatedAt = DateTime.UtcNow
            };

            if (address.IsDefault)
            {
                await ClearDefaultAddressAsync(userId.Value);
            }

            _context.UserAddresses.Add(address);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAddressById), new { id = address.Id }, MapAddress(address));
        }

        /// <summary>
        /// Update an existing shipping address
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserAddressDto>> UpdateAddress(int id, [FromBody] UpdateUserAddressRequest request)
        {
            var address = await GetOwnedAddressAsync(id);
            if (address == null)
            {
                return NotFound("Address not found");
            }

            if (!IsValidRequest(request, out var errorMessage))
            {
                return BadRequest(errorMessage);
            }

            address.ReceiverName = request.ReceiverName.Trim();
            address.Phone = request.Phone.Trim();
            address.AddressLine = request.AddressLine.Trim();
            address.Ward = string.IsNullOrWhiteSpace(request.Ward) ? null : request.Ward.Trim();
            address.District = string.IsNullOrWhiteSpace(request.District) ? null : request.District.Trim();
            address.Province = string.IsNullOrWhiteSpace(request.Province) ? null : request.Province.Trim();
            address.IsDefault = request.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;

            if (address.IsDefault)
            {
                await ClearDefaultAddressAsync(address.UserId, address.Id);
            }

            await _context.SaveChangesAsync();

            return Ok(MapAddress(address));
        }

        /// <summary>
        /// Delete (soft delete) a shipping address by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var address = await GetOwnedAddressAsync(id);
            if (address == null)
            {
                return NotFound("Address not found");
            }

            var wasDefault = address.IsDefault;
            address.IsDeleted = true;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (wasDefault)
            {
                var nextDefault = await _context.UserAddresses
                    .Where(item => item.UserId == address.UserId && !item.IsDeleted)
                    .OrderByDescending(item => item.CreatedAt)
                    .FirstOrDefaultAsync();

                if (nextDefault != null)
                {
                    nextDefault.IsDefault = true;
                    nextDefault.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Set a specific shipping address as the default address
        /// </summary>
        [HttpPut("{id}/set-default")]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            var address = await GetOwnedAddressAsync(id);
            if (address == null)
            {
                return NotFound("Address not found");
            }

            await ClearDefaultAddressAsync(address.UserId, address.Id);
            address.IsDefault = true;
            address.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(MapAddress(address));
        }

        private async Task<UserAddress?> GetOwnedAddressAsync(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return null;
            }

            return await _context.UserAddresses
                .FirstOrDefaultAsync(address => address.Id == id && address.UserId == userId.Value && !address.IsDeleted);
        }

        private async Task ClearDefaultAddressAsync(int userId, int? excludeAddressId = null)
        {
            var defaultAddresses = await _context.UserAddresses
                .Where(address => address.UserId == userId && !address.IsDeleted && address.IsDefault && (!excludeAddressId.HasValue || address.Id != excludeAddressId.Value))
                .ToListAsync();

            foreach (var defaultAddress in defaultAddresses)
            {
                defaultAddress.IsDefault = false;
                defaultAddress.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static bool IsValidRequest(CreateUserAddressRequest request, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(request.ReceiverName) ||
                string.IsNullOrWhiteSpace(request.Phone) ||
                string.IsNullOrWhiteSpace(request.AddressLine))
            {
                errorMessage = "Receiver name, phone and address line are required.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }

        private static UserAddressDto MapAddress(UserAddress address)
        {
            return new UserAddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                ReceiverName = address.ReceiverName,
                Phone = address.Phone,
                AddressLine = address.AddressLine,
                Ward = address.Ward,
                District = address.District,
                Province = address.Province,
                IsDefault = address.IsDefault
            };
        }
    }
}