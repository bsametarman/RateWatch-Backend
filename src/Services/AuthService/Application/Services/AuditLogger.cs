
using RateWatch.AuthService.Domain.Entities;
using RateWatch.AuthService.Infrastructure.Data;

namespace RateWatch.AuthService.Application.Services
{
    public class AuditLogger : IAuditLogger
    {
        private readonly AuthContext _context;

        public AuditLogger(AuthContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int? userId, string action, string ipAddress, string? userAgent, string? details = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
