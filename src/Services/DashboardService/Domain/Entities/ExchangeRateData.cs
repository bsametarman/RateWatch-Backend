using System.Text.Json.Serialization;

namespace RateWatch.DashboardService.Domain.Entities
{
    public class ExchangeRateData
    {
        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("timeLastUpdateUnix")]
        public long TimeLastUpdateUnix { get; set; }

        [JsonPropertyName("baseCode")]
        public string BaseCode { get; set; }

        [JsonPropertyName("conversionRates")]
        public Dictionary<string, double> ConversionRates { get; set; }
    }
}
