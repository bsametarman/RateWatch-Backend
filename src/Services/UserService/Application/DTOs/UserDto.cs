namespace RateWatch.UserService.Application.DTOs
{
    public record UserDto(int Id, int AuthUserId, string Email, string Username);
}
