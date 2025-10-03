using RateWatch.UserService.Application.DTOs;

namespace RateWatch.UserService.Application.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> CreateUserFromEventAsync(UserForCreationDto userForCreationDto);
        Task UpdateUserAsync(int id, UserForUpdateDto userForUpdateDto);
        Task DeleteUserAsync(int id);
    }
}
