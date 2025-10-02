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

            if(!result)
            {
                return BadRequest("Email already exists.");
            }

            return Ok("User registered successfully!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var token = await _authService.LoginAsync(userForLoginDto);

            if(string.IsNullOrEmpty(token))
            {
                return Unauthorized("Invalid credentials");
            }

            return Ok(new {Token = token});
        }
    }
}
