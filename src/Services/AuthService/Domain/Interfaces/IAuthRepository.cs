using RateWatch.AuthService.Domain.Entities;

namespace RateWatch.AuthService.Domain.Interfaces
{
    public interface IAuthRepository
    {
        Task<AuthUser> AddUserAsync(AuthUser user);
        Task<AuthUser?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
    }
}
