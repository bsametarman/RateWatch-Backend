using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RateWatch.AuthService.Application.DTOs;
using RateWatch.AuthService.Application.Services;

namespace RateWatch.AuthService.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            var result = await _authService.RegisterAsync(userForRegisterDto);

            if(!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var response = await _authService.LoginAsync(userForLoginDto);

            if(string.IsNullOrEmpty(response.Data))
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }
    }
}
