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
    }
}