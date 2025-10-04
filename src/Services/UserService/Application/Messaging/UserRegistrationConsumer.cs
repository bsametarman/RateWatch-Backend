using Confluent.Kafka;
using RateWatch.UserService.Application.DTOs;
using RateWatch.UserService.Application.Services;
using System.Text.Json;

namespace RateWatch.UserService.Application.Messaging
{
    public class UserRegistrationConsumer : BackgroundService
    {
        private readonly IConsumer<Null, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserRegistrationConsumer> _logger;

        private const string TopicName = "user-registered-topic";

        public UserRegistrationConsumer(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<UserRegistrationConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Null, string>(consumerConfig)
                .SetErrorHandler((_, err) => _logger.LogError($"Kafka error: {err.Reason}"))
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(TopicName);
            _logger.LogInformation($"Kafka Consumer subscribed to topic: {TopicName}");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Yield();

                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult is null) continue;

                    _logger.LogInformation($"Received message: {consumeResult.Message.Value}");

                    _ = HandleMessage(consumeResult.Message.Value);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer is stopping.");
                    break;
                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Consume error: {e.Error.Reason}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected error in consumer loop.");
                }
            }

            _consumer.Close();
        }

        private async Task HandleMessage(string message)
        {
            var eventData = JsonSerializer.Deserialize<UserForCreationDto>(message,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if(eventData == null)
            {
                _logger.LogError("Failed to deserialize user creation event.");
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                try
                {
                    await userService.CreateUserFromEventAsync(eventData);
                    _logger.LogInformation($"User created for AuthUserId: {eventData.AuthUserId}");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while creating user for AuthUserId: {eventData.AuthUserId}");
                }
            }

        }
    }
}
