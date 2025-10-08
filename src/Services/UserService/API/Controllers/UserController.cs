using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateWatch.UserService.Application.DTOs;
using RateWatch.UserService.Application.Services;
using System.Security.Claims;

namespace RateWatch.UserService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user != null)
            {
                return Ok(user);
            }
            return NotFound();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            await _userService.UpdateUserAsync(id, userForUpdateDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpPost("internal/create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> CreateUserForTestKafka(UserForCreationDto userForCreationDto)
        {
            var newUser = await _userService.CreateUserFromEventAsync(userForCreationDto);
            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, newUser);
        }
    }
}
