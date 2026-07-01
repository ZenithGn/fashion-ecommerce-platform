using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using FashionEcommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FashionEcommerce.Services.Services
{
    public sealed class UserService : IUserService
    {
        private readonly FashionEcommerceDbContext _context;

        public UserService(FashionEcommerceDbContext context)
        {
            _context = context;
        }

        public Task<User?> GetUserByIdAsync(int userId)
        {
            return _context.Users
                .Include(u => u.Role)
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            return _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.IsDeleted);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersAsync(string? search, int? roleId, bool? isActive)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim().ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(normalizedSearch) ||
                    u.LastName.ToLower().Contains(normalizedSearch) ||
                    u.Email.ToLower().Contains(normalizedSearch) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(normalizedSearch)));
            }

            if (roleId.HasValue)
                query = query.Where(u => u.RoleId == roleId.Value);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return false;
            }

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> LockUserAsync(int userId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return null;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UnlockUserAsync(int userId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return null;

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateUserRoleAsync(int userId, int roleId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return null;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);
            if (role == null) return null;

            user.RoleId = role.Id;
            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return user;
        }
    }
}