using RateWatch.AuthService.Application.DTOs;
using RateWatch.AuthService.Application.Responses;

namespace RateWatch.AuthService.Application.Services
{
    public interface IAuthService
    {
        Task<ApiResponse> RegisterAsync(UserForRegisterDto userForRegisterDto);
        Task<ApiDataResponse<string>> LoginAsync(UserForLoginDto userForLoginDto);
    }
}
