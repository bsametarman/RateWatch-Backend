using Confluent.Kafka;
using System.Text.Json;

namespace RateWatch.NotificationService
{
    public class NotificationWorker : BackgroundService
    {
        private readonly ILogger<NotificationWorker> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly IConsumer<Null, string> _kafkaConsumer;

        public NotificationWorker(ILogger<NotificationWorker> logger, WebSocketConnectionManager connectionManager, IConfiguration configuration)
        {
            _logger = logger;
            _connectionManager = connectionManager;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            _kafkaConsumer.Subscribe(configuration["Kafka:AlertTriggeredTopic"]);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification service is working.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Yield();

                try
                {
                    var consumeResult = _kafkaConsumer.Consume(stoppingToken);
                    
                    if (consumeResult?.Message?.Value == null)
                    {
                        continue;
                    }

                    _logger.LogInformation($"Recieved alert: {consumeResult.Message.Value}");

                    await HandleAlertTriggered(consumeResult.Message.Value);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Something went wrong.");
                }
            }
            //_kafkaConsumer.Close();
        }

        private async Task HandleAlertTriggered(string message)
        {
            try
            {
                var alert = JsonDocument.Parse(message).RootElement;

                if(alert.TryGetProperty("UserId", out var userIdElement) && userIdElement.ValueKind.Equals(JsonValueKind.Number))
                {
                    var userId = userIdElement.GetInt64().ToString();

                    var notificationMessage = new
                    {
                        type = "ALERT_TRIGGERED",
                        payload = alert.Clone()
                    };

                    await _connectionManager.SendMessageToUserAsync(userId, JsonSerializer.Serialize(notificationMessage));
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse alert event message.");
            }
        }

        public override void Dispose()
        {
            _kafkaConsumer.Dispose();
            base.Dispose();
        }
    }
}
