using RateWatch.AlertService.Application.DTOs;
using RateWatch.AlertService.Domain.Entities;
using RateWatch.AlertService.Domain.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace RateWatch.AlertService.Application.Services
{
    public class AlertService : IAlertService
    {
        private readonly IAlertRepository _alertRepository;
        private readonly IDatabase _redisDb;
        private const string ActivateAlertsRedisKey = "active_alerts";

        public AlertService(IAlertRepository alertRepository, IConnectionMultiplexer redis)
        {
            _alertRepository = alertRepository;
            _redisDb = redis.GetDatabase();
        }

        public async Task<AlertDto> AddAlertAsync(AlertForCreationDto alertForCreationDto)
        {
            var alert = new Alert
            {
                BaseCurrency = alertForCreationDto.BaseCurrency.ToUpper(),
                Condition = Enum.Parse<AlertCondition>(alertForCreationDto.Condition, true),
                TargetCurrency = alertForCreationDto.TargetCurrency.ToUpper(),
                Treshold = alertForCreationDto.Threshold,
                UserId = alertForCreationDto.UserId,
            };

            var newAlert = await _alertRepository.AddAlertAsync(alert);

            var alertJson = JsonSerializer.Serialize(newAlert);
            await _redisDb.HashSetAsync(ActivateAlertsRedisKey, newAlert.Id.ToString(), alertJson);

            return new AlertDto(newAlert.Id, newAlert.UserId, newAlert.BaseCurrency, newAlert.TargetCurrency, newAlert.Condition.ToString(), newAlert.Treshold, newAlert.IsTriggered, newAlert.IsActive);
        }

        public async Task DeleteAlertAsync(int id)
        {
            await _alertRepository.DeleteAlertAsync(id);

            await _redisDb.HashDeleteAsync(ActivateAlertsRedisKey, id.ToString());
        }

        public async Task<AlertDto?> GetAlertByIdAsync(int id)
        {
            var alert = await _alertRepository.GetAlertByIdAsync(id);
            if (alert != null)
            {
                return new AlertDto(alert.Id, alert.UserId, alert.BaseCurrency.ToUpper(), alert.TargetCurrency.ToUpper(), alert.Condition.ToString(), alert.Treshold, alert.IsTriggered, alert.IsActive);
            }
            return null;
        }

        public async Task<List<AlertDto>> GetAlertsByUserIdAsync(int userId)
        {
            var alerts = await _alertRepository.GetAlertsByUserIdAsync(userId);
            Console.WriteLine("service");
            Console.WriteLine(alerts);
            if (alerts == null || !alerts.Any())
            {
                return new List<AlertDto>();
            }

            var alertDtos = alerts.Select(alert => new AlertDto(
                alert.Id,
                alert.UserId,
                alert.BaseCurrency.ToUpper(),
                alert.TargetCurrency.ToUpper(),
                alert.Condition.ToString(),
                alert.Treshold,
                alert.IsTriggered,
                alert.IsActive
            )).ToList();

            return alertDtos;
        }

        public async Task UpdateAlertAsync(int id, AlertForUpdateDto alertForUpdateDto)
        {
            var alert = await _alertRepository.GetAlertByIdAsync(id);
            if (alert != null)
            {
                alert.TargetCurrency = alertForUpdateDto.TargetCurrency.ToUpper();
                alert.BaseCurrency = alertForUpdateDto.BaseCurrency.ToUpper();
                alert.Treshold = alertForUpdateDto.Threshold; ;
                alert.Condition = Enum.Parse<AlertCondition>(alertForUpdateDto.Condition, true);
                alert.IsActive = alertForUpdateDto.IsActive;
                alert.IsTriggered = false;

                await _alertRepository.UpdateAlertAsync(alert);

                var alertJson = JsonSerializer.Serialize(alert);
                await _redisDb.HashSetAsync(ActivateAlertsRedisKey, alert.Id.ToString(), alertJson);
            }
        }
    }
}
