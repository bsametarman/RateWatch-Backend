namespace RateWatch.UserService.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public int AuthUserId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
