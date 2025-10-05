using RateWatch.AlertService.Application.DTOs;

namespace RateWatch.AlertService.Application.Services
{
    public interface IAlertService
    {
        Task<AlertDto?> GetAlertByIdAsync(int id);
        Task<AlertDto?> GetAlertByUserIdAsync(int userId);
        Task<AlertDto> AddAlertAsync(AlertForCreationDto alertForCreationDto);
        Task DeleteAlertAsync(int id);
        Task UpdateAlertAsync(int id, AlertForUpdateDto alertForUpdateDto);
    }
}
