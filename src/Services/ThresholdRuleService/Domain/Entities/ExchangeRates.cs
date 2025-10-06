namespace RateWatch.ThresholdRuleService.Domain.Entities
{
    public record ExchangeRates(
        Dictionary<string, double> conversion_rates
    );
}
