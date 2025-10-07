using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace RateWatch.AnalyticsService.Domain.Entities
{
    public class ExchangeRateData
    {
        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("time_last_update_unix")]
        public long TimeLastUpdateUnix { get; set; }

        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; }

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, double> ConversionRates { get; set; }
    }
}
