using Microsoft.EntityFrameworkCore;
using RateWatch.AlertService.Domain.Entities;

namespace RateWatch.AlertService.Infrastructure.Data
{
    public class AlertContext : DbContext
    {
        public AlertContext(DbContextOptions<AlertContext> options) : base(options)
        {
        }
        public DbSet<Alert> Alerts { get; set; }
    }
}
