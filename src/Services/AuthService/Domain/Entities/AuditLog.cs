namespace RateWatch.AuthService.Domain.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }
    }
}
