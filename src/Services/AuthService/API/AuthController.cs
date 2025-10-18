using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RateWatch.AuthService.Application.DTOs;
using RateWatch.AuthService.Application.Responses;
using RateWatch.AuthService.Application.Services;

namespace RateWatch.AuthService.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IValidator<UserForRegisterDto> _validator;
        public AuthController(IAuthService authService, IValidator<UserForRegisterDto> validator)
        {
            _authService = authService;
            _validator = validator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            var validationResult = await _validator.ValidateAsync(userForRegisterDto);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        f => f.Key,
                        f => f.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(ApiDataResponse<Dictionary<string, string[]>>.FailWithMessage(data: validationErrors, message: "Validation errors."));
            }
            else
            {
                var result = await _authService.RegisterAsync(userForRegisterDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
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
