namespace RateWatch.ThresholdRuleService.Domain.Entities
{
    public record AlertRule(
        int Id,
        int UserId,
        string BaseCurrency,
        string TargetCurrency,
        int Condition,
        decimal Threshold,
        bool IsTriggered
    );
}
