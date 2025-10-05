using Confluent.Kafka;
using StackExchange.Redis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace RateWatch.RateFetcherService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IConnectionMultiplexer _redis;
    private readonly IProducer<Null, string> _kafkaProducer;

    private readonly string _exchangeRateApiUrl;
    private readonly int _fetchInterval;
    private const string RatesUpdatedTopic = "rates-updated-topic";
    private const string LatestRatesRedisKey = "latest_rates";

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _redis = redis;

        var kafkaConfig = new ProducerConfig { BootstrapServers = _configuration["Kafka:BootstrapServers"] };
        _kafkaProducer = new ProducerBuilder<Null, string>(kafkaConfig).Build();

        var apiConfig = _configuration.GetSection("ExchangeRateApi");
        _exchangeRateApiUrl = $"{apiConfig["BaseUrl"]}{apiConfig["ApiKey"]}/latest/{apiConfig["BaseCurrency"]}";

        _fetchInterval = _configuration.GetValue<int>("WorkerSettings:FetchIntervalInSeconds", 43200);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Rate fetcher service is running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Fetching latest exchange rates at: {time}", DateTimeOffset.UtcNow);

                var client = _httpClientFactory.CreateClient();
                var response = await client.GetFromJsonAsync<ExchangeRateApiResponse>(_exchangeRateApiUrl, stoppingToken);

                if(response != null && response.result == "success")
                {
                    var db = _redis.GetDatabase();
                    var hashEntries = response.conversion_rates
                        .Select(rate => new HashEntry(rate.Key, rate.Value.ToString()))
                        .ToArray();

                    await db.HashSetAsync(LatestRatesRedisKey, hashEntries, CommandFlags.FireAndForget);
                    _logger.LogInformation("Latest rates successfully updated");

                    var kafkaMessage = JsonSerializer.Serialize(response);
                    await _kafkaProducer.ProduceAsync(RatesUpdatedTopic, new Message<Null, string> { Value = kafkaMessage }, stoppingToken);

                    _logger.LogInformation("Rates successfully published to kafka topic.");
                }
                else
                {
                    _logger.LogWarning("Failed to fetch rates!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong!");
            }

            await Task.Delay(TimeSpan.FromSeconds(_fetchInterval), stoppingToken);
        }
    }

    public record ExchangeRateApiResponse(
        string result,
        long time_last_update_unix,
        string base_code,
        Dictionary<string, double> conversion_rates
    );
}
