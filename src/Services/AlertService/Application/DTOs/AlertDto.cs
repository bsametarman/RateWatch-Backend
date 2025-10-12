namespace RateWatch.AlertService.Application.DTOs
{
    public record AlertDto(
        int Id,
        int UserId,
        string BaseCurrency,
        string TargetCurrency,
        string Condition,
        decimal Threshold,
        bool IsTriggered,
        bool IsActive
    );
}
