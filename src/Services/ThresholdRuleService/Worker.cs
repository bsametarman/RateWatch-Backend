using Confluent.Kafka;
using RateWatch.ThresholdRuleService.Domain.Entities;
using StackExchange.Redis;
using System.Text.Json;

namespace RateWatch.ThresholdRuleService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDatabase _redisDb;
    private readonly IProducer<Null, string> _kafkaProducer;
    private readonly IConsumer<Null, string> _kafkaConsumer;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _configuration = configuration;
        _redisDb = redis.GetDatabase();

        var producerConfig = new ProducerConfig { BootstrapServers = _configuration["Kafka:BootstrapServers"] };
        _kafkaProducer = new ProducerBuilder<Null, string>(producerConfig).Build();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Threshold rule service is running.");
        _kafkaConsumer.Subscribe(_configuration["Kafka:RatesUpdatedTopic"]);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Yield();

            try
            {
                var consumerResult = _kafkaConsumer.Consume(stoppingToken);
                if (consumerResult == null)
                {
                    continue;
                }

                _logger.LogInformation("New rates recieved.");
                await ProcessRateUpdate(consumerResult.Message.Value);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Something went wrong.");
            }
        }

        _kafkaConsumer.Close();
    }

    private async Task ProcessRateUpdate(string ratesUpdateMessage)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rates = JsonSerializer.Deserialize<ExchangeRates>(ratesUpdateMessage, options);

        if (rates?.conversion_rates == null)
        {
            return;
        }

        var activeAlerts = await _redisDb.HashGetAllAsync(_configuration["Redis:ActiveAlertsKey"]);
        if (activeAlerts.Length == 0)
        {
            return;
        }

        _logger.LogInformation($"Checking {activeAlerts.Length} alerts with new rates.");

        foreach (var alert in activeAlerts)
        {
            var rule = JsonSerializer.Deserialize<AlertRule>(alert.Value, options);
            if (rule != null && rule.IsTriggered)
            {
                continue;
            }

            if (rates.conversion_rates.TryGetValue(rule.TargetCurrency, out var currentRate))
            {
                bool isTriggered = false;

                if (rule.Condition == 0 && (decimal)currentRate > rule.Threshold)
                {
                    isTriggered = true;
                }
                else if (rule.Condition == 1 && (decimal)currentRate < rule.Threshold)
                {
                    isTriggered = true;
                }

                if (isTriggered)
                {
                    _logger.LogWarning($"ALERT TRIGGERED: Rule Id = {rule.Id}," +
                        $"\n{rule.BaseCurrency}/{rule.TargetCurrency} is now {currentRate}. Threshold was {rule.Threshold}");

                    await MarkAlertAsTriggeredInRedis(rule);

                    var triggeredEvent = new { rule.Id, rule.UserId, rule.BaseCurrency, rule.TargetCurrency, rule.Threshold, CurrentRate = currentRate };
                    await _kafkaProducer.ProduceAsync(_configuration["Kafka:AlertTriggeredTopic"], new Message<Null, string> { Value = JsonSerializer.Serialize(triggeredEvent) });
                }
            }

        }

    }
    private async Task MarkAlertAsTriggeredInRedis(AlertRule rule)
    {
        var updatedRule = rule with { IsTriggered = true };
        await _redisDb.HashSetAsync(_configuration["Redis:ActiveAlertsKey"], rule.Id.ToString(), JsonSerializer.Serialize(updatedRule));
    }
}
