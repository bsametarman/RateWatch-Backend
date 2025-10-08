namespace RateWatch.AuthService.Domain.Entities
{
    public class AuthUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public byte[] passwordHash { get; set; }
        public byte[] passwordSalt { get; set; }
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
