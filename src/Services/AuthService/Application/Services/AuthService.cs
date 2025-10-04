using Microsoft.IdentityModel.Tokens;
using RateWatch.AuthService.Application.DTOs;
using RateWatch.AuthService.Domain.Entities;
using RateWatch.AuthService.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RateWatch.AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IMessageProducer _messageProducer;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration, IMessageProducer messageProducer)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _messageProducer = messageProducer;

        }

        public async Task<bool> RegisterAsync(UserForRegisterDto userForRegisterDto)
        {
            if(await _authRepository.UserExistsAsync(userForRegisterDto.Email)) { return false; }

            CreatePasswordHash(userForRegisterDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new AuthUser
            {
                Email = userForRegisterDto.Email,
                passwordHash = passwordHash,
                passwordSalt = passwordSalt,
            };

            var newUser = await _authRepository.AddUserAsync(user);

            if(newUser != null)
            {
                var userRegisteredEvent = new
                {
                    AuthUserId = newUser.Id,
                    newUser.Email,
                    Username = newUser.Email.Split('@')[0]
                };
                
                await _messageProducer.ProduceAsync("user-registered-topic", userRegisteredEvent);

                return true;
            }

            return false;
        }

        public async Task<string> LoginAsync(UserForLoginDto userForLoginDto)
        {
            var user = await _authRepository.GetUserByEmailAsync(userForLoginDto.email);
            if(user == null || !VerifyPasswordHash(userForLoginDto.password, user.passwordHash, user.passwordSalt)) {
                return "Wrong credentials.";            
            }
            return CreateToken(user);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

        private string CreateToken(AuthUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var appSettingsToken = _configuration.GetSection("AppSettings:Token").Value;
            if (string.IsNullOrEmpty(appSettingsToken))
                throw new Exception("AppSettings Token is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettingsToken));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }
    }
}
