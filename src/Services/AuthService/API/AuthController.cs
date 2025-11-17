using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RateWatch.AuthService.Application.DTOs;
using RateWatch.AuthService.Application.Responses;
using RateWatch.AuthService.Application.Services;

namespace RateWatch.AuthService.API
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth_policy")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IValidator<UserForRegisterDto> _validator;
        private readonly IAuditLogger _auditLogger;
        public AuthController(IAuthService authService, IValidator<UserForRegisterDto> validator, IAuditLogger auditLogger)
        {
            _authService = authService;
            _validator = validator;
            _auditLogger = auditLogger;

        }

        private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        private string? GetUserAgent() => HttpContext.Request.Headers["User-Agent"].ToString();

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
                    await _auditLogger.LogAsync(null, "UserRegistrationFailure", GetIpAddress(), GetUserAgent(), $"Attempt for email: {userForRegisterDto.Email}");
                    return BadRequest(result);
                }

                await _auditLogger.LogAsync(null, "UserRegistrationSuccess", GetIpAddress(), GetUserAgent(), $"Email: {userForRegisterDto.Email}");
                return Ok(result);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var response = await _authService.LoginAsync(userForLoginDto);

            if(string.IsNullOrEmpty(response.Data))
            {
                await _auditLogger.LogAsync(null, "UserLoginFailure", GetIpAddress(), GetUserAgent(), $"Attempt for email: {userForLoginDto.email}");
                return Unauthorized(response);
            }

            await _auditLogger.LogAsync(null, "UserLoginSuccess", GetIpAddress(), GetUserAgent(), $"Email: {userForLoginDto.email}");
            return Ok(response);
        }
    }
}
