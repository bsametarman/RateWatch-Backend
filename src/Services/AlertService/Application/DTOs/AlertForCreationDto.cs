namespace RateWatch.AlertService.Application.DTOs
{
    public record AlertForCreationDto(
        int UserId,
        string BaseCurrency,
        string TargetCurrency,
        string Condition,
        decimal Threshold
    );
}
