using Microsoft.EntityFrameworkCore;
using RateWatch.AlertService.Domain.Entities;
using RateWatch.AlertService.Domain.Interfaces;
using RateWatch.AlertService.Infrastructure.Data;

namespace RateWatch.AlertService.Infrastructure.Repositories
{
    public class AlertRepository : IAlertRepository
    {
        private readonly AlertContext _context;

        public AlertRepository(AlertContext context)
        {
            _context = context;
        }

        public async Task<Alert> AddAlertAsync(Alert alert)
        {
            await _context.Alerts.AddAsync(alert);
            await _context.SaveChangesAsync();
            return alert;
        }

        public async Task DeleteAlertAsync(int id)
        {
            var alert = await GetAlertByIdAsync(id);
            if(alert != null)
            {
                _context.Remove(alert);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Alert?> GetAlertByIdAsync(int id)
        {
            return await _context.Alerts.FindAsync(id);
        }

        public async Task<Alert?> GetAlertByUserIdAsync(int userId)
        {
            return await _context.Alerts.FirstOrDefaultAsync(alert => alert.UserId == userId);
        }

        public async Task UpdateAlertAsync(Alert alert)
        {
            _context.Entry(alert).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
