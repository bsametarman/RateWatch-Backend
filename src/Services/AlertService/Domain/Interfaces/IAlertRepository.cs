using RateWatch.AlertService.Domain.Entities;

namespace RateWatch.AlertService.Domain.Interfaces
{
    public interface IAlertRepository
    {
        Task<Alert?> GetAlertByIdAsync(int id);
        Task<Alert?> GetAlertByUserIdAsync(int userId);
        Task<Alert> AddAlertAsync(Alert alert);
        Task DeleteAlertAsync(int id);
        Task UpdateAlertAsync(Alert alert);
    }
}
