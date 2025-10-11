using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using RateWatch.DashboardService.Domain.Entities;
using StackExchange.Redis;
using System.Net.Sockets;
using System.Security.Claims;

namespace RateWatch.DashboardService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IElasticClient _elasticClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IConnectionMultiplexer redis, IElasticClient elasticClient, IConfiguration configuration, ILogger<DashboardController> logger)
        {
            _configuration = configuration;
            _redis = redis;
            _elasticClient = elasticClient;
            _logger = logger;
        }

        [HttpGet("latest-rates")]
        public async Task<IActionResult> GetLatestRates()
        {
            var db = _redis.GetDatabase();
            var redisKey = _configuration["Redis:LatestRatesKey"];
            var hashEntries = await db.HashGetAllAsync(redisKey);

            if(hashEntries.Length == 0)
            {
                return NotFound("Rates are not available yet.");
            }

            var result = hashEntries.ToDictionary(
                entry => entry.Name.ToString(),
                entry => double.Parse(entry.Value.ToString())
            );

            return Ok(result);
        }

        [HttpGet("historical-rates/{currencyPair}")]
        public async Task<IActionResult> GetHistoricalRates(string currencyPair, [FromQuery] int hours = 24)
        {
            var currencies = currencyPair.ToUpper().Split('-');
            if(currencies.Length != 2)
            {
                return BadRequest("Currency format is like EUR-USD");
            }

            var baseCurrency = currencies[0].ToUpper();
            var targetCurrency = currencies[1].ToUpper();

            var searchResponse = await _elasticClient.SearchAsync<ExchangeRateDocument>(s => s
                .Index(_configuration["Elasticsearch:DefaultIndex"])
                .Size(1500) // last 1 records
                .Sort(ss => ss.Descending("timeStamp"))
                .Query(q => q
                .DateRange(
                        r => r
                        .Field("timeStamp")
                        .GreaterThanOrEquals(DateMath.Now.Subtract(new DateMathTime($"{hours}h")))
                        .LessThanOrEquals(DateMath.Now)
                    )
                )
            );

            if(!searchResponse.IsValid)
            {
                _logger.LogError("Elasticsearch query failed: {DebugInfo}", searchResponse.DebugInformation);
                return StatusCode(500, "Failed to retrieve data.");
            }

            var historicalData = searchResponse.Documents
                .Select(doc =>
                {
                    var rates = doc.Rates.ConversionRates;

                    rates.TryGetValue(baseCurrency, out var baseRateInUsd);
                    rates.TryGetValue(targetCurrency, out var targetRateInUsd);

                    if (baseRateInUsd == 0 || targetRateInUsd == 0)
                    {
                        return null;
                    }

                    var crossRate = targetRateInUsd / baseRateInUsd;

                    return new
                    {
                        doc.TimeStamp,
                        Rate = Math.Round(crossRate, 6)
                    };
                })
                .Where(x => x.Rate > 0)
                .OrderBy(x => x.TimeStamp)
                .ToList();

            return Ok(historicalData);
        }

        [HttpGet("historical-rates-daily/{currencyPair}")]
        public async Task<IActionResult> GetHistoricalRatesDaily(string currencyPair, [FromQuery] int days = 7)
        {
            var currencies = currencyPair.ToUpper().Split('-');
            if (currencies.Length != 2)
            {
                return BadRequest("Currency format is like EUR-USD");
            }

            var baseCurrency = currencies[0].ToUpper();
            var targetCurrency = currencies[1].ToUpper();

            var searchResponse = await _elasticClient.SearchAsync<ExchangeRateDocument>(s => s
                .Index(_configuration["Elasticsearch:DefaultIndex"])
                .Size(0)
                .Query(q => q
                    .DateRange(r => r
                        .Field(f => f.TimeStamp)
                        .GreaterThanOrEquals(DateMath.Now.Subtract(new DateMathTime($"{days}d")))
                    )
                )
                .Aggregations(aggs => aggs
                    .DateHistogram("rates_by_day", dh => dh
                        .Field(f => f.TimeStamp)
                        .CalendarInterval(DateInterval.Day)
                        .MinimumDocumentCount(1)
                        .Aggregations(subAggs => subAggs
                            .TopHits("latest_rate_per_day", th => th
                                .Size(1)
                                .Sort(sort => sort.Descending(f => f.TimeStamp))
                            )
                        )
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                _logger.LogError("Elasticsearch query failed: {DebugInfo}", searchResponse.DebugInformation);
                return StatusCode(500, "Failed to retrieve data.");
            }

            var dailyBuckets = searchResponse.Aggregations
                                     .DateHistogram("rates_by_day")
                                     ?.Buckets;

            if (dailyBuckets == null)
            {
                return Ok(new List<object>());
            }

            var historicalData = dailyBuckets.Select(bucket =>
                {
                    var latestHit = bucket.TopHits("latest_rate_per_day")?.Documents<ExchangeRateDocument>().FirstOrDefault();

                    if (latestHit == null)
                    {
                        return null;
                    }

                    var rates = latestHit.Rates.ConversionRates;

                    rates.TryGetValue(baseCurrency, out var baseRateInUsd);
                    rates.TryGetValue(targetCurrency, out var targetRateInUsd);

                    if (baseRateInUsd == 0 || targetRateInUsd == 0)
                    {
                        return null;
                    }

                    var crossRate = targetRateInUsd / baseRateInUsd;

                    return new
                    {
                        TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds((long)bucket.Key).UtcDateTime,
                        Rate = Math.Round(crossRate, 6)
                    };
                })
                .Where(x => x.Rate > 0)
                .OrderBy(x => x.TimeStamp)
                .ToList();

            return Ok(historicalData);
        }
    }
}
