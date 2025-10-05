namespace RateWatch.AlertService.Application.DTOs
{
    public record AlertForUpdateDto(
        string BaseCurrency,
        string TargetCurrency,
        string Condition,
        decimal Threshold,
        bool IsActive
    );
}
