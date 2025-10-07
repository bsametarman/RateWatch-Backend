using Microsoft.AspNetCore.Mvc;
using Nest;
using RateWatch.DashboardService.Domain.Entities;
using StackExchange.Redis;

namespace RateWatch.DashboardService.API.Controllers
{
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

            var targetCurrency = currencies[1];

            var searchResponse = await _elasticClient.SearchAsync<ExchangeRateDocument>(s => s
                .Index(_configuration["Elasticsearch:DefaultIndex"])
                .Size(1) // last 1 records
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
                .Select(doc => new
                {
                    doc.TimeStamp,
                    Rate = doc.Rates.ConversionRates.TryGetValue(targetCurrency, out var rate) ? rate : 0
                })
                .Where(x => x.Rate > 0)
                .OrderBy(x => x.TimeStamp)
                .ToList();

            return Ok(historicalData);
        }
    }
}
