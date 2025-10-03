namespace RateWatch.UserService.Application.DTOs
{
    public record UserForCreationDto(int AuthUserId, string Email, string Username);
}
