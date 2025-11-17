namespace RateWatch.AuthService.Application.Services
{
    public interface IAuditLogger
    {
        Task LogAsync(int? userId, string action, string ipAddress, string? userAgent, string? details = null);
    }
}
