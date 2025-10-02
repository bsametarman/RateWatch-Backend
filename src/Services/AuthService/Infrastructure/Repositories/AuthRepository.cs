using Microsoft.EntityFrameworkCore;
using RateWatch.AuthService.Domain.Entities;
using RateWatch.AuthService.Domain.Interfaces;
using RateWatch.AuthService.Infrastructure.Data;

namespace RateWatch.AuthService.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AuthContext _context;

        public AuthRepository(AuthContext context)
        {
            _context = context;
        }
        public async Task<AuthUser> AddUserAsync(AuthUser user)
        {
            await _context.AuthUsers.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<AuthUser?> GetUserByEmailAsync(string email)
        {
            return await _context.AuthUsers.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.AuthUsers.AnyAsync(u => u.Email == email);
        }
    }
}
