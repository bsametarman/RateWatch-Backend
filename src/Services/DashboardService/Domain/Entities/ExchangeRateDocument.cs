using System.Text.Json.Serialization;

namespace RateWatch.DashboardService.Domain.Entities
{
    public class ExchangeRateDocument
    {
        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonPropertyName("rates")]
        public ExchangeRateData Rates { get; set; }
    }
}
