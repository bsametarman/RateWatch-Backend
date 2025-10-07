using Confluent.Kafka;
using Nest;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RateWatch.AnalyticsService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IElasticClient _elasticClient;
    private readonly IConsumer<Null, string> _kafkaConsumer;

    public Worker(ILogger<Worker> logger, IElasticClient elasticClient, IConfiguration configuration)
    {
        _logger = logger;
        _elasticClient = elasticClient;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:Bootstrapservers"],
            GroupId = configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
        _kafkaConsumer.Subscribe(configuration["Kafka:RatesUpdatedTopic"]);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Analytics service is working.");

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

                await ProcessResult(consumerResult.Message.Value);
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
    }

    private async Task ProcessResult(string message)
    {
        var rateDocument = JsonSerializer.Deserialize<JsonElement>(message);
        var rateDocumentWithTimeStamp = new
        {
            timeStamp = DateTime.UtcNow,
            rates = rateDocument
        };

        var indexResponse = await _elasticClient.IndexDocumentAsync(rateDocumentWithTimeStamp);

        if (!indexResponse.IsValid)
        {
            _logger.LogError("Failed to index document to Elasticsearch: {Reason}", indexResponse.DebugInformation);
        }
        else
        {
            _logger.LogInformation("Document successfully added to Elasticsearch. Id: {Id}", indexResponse.Id);
        }

    }
}
