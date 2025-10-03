using RateWatch.UserService.Application.DTOs;
using RateWatch.UserService.Domain.Entities;
using RateWatch.UserService.Domain.Interfaces;

namespace RateWatch.UserService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<UserDto> CreateUserFromEventAsync(UserForCreationDto userForCreationDto)
        {
            var user = new User
            {
                AuthUserId = userForCreationDto.AuthUserId,
                Email = userForCreationDto.Email,
                Username = userForCreationDto.Username
            };
            var newUser = await _userRepository.AddUserAsync(user);
            return new UserDto(newUser.Id, newUser.AuthUserId, newUser.Email, newUser.Username);
        }

        public async Task DeleteUserAsync(int id)
        {
            await _userRepository.DeleteUserAsync(id);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(u => new UserDto(u.Id, u.AuthUserId, u.Email, u.Username));
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            return user != null ? new UserDto(user.Id, user.AuthUserId, user.Email, user.Username) : null;
        }

        public async Task UpdateUserAsync(int id, UserForUpdateDto userForUpdateDto)
        {
            var userToUpdate = await _userRepository.GetUserByIdAsync(id);
            if (userToUpdate != null)
            {
                userToUpdate.Username = userForUpdateDto.Username;
                await _userRepository.UpdateUserAsync(userToUpdate);
            }
            else
            {
                throw new Exception("User not found!");
            }
        }
    }
}
