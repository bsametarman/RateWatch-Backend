using RateWatch.AuthService.Application.DTOs;

namespace RateWatch.AuthService.Application.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(UserForRegisterDto userForRegisterDto);
        Task<string> LoginAsync(UserForLoginDto userForLoginDto);
    }
}
