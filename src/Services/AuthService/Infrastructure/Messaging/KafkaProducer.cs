using Confluent.Kafka;
using RateWatch.AuthService.Application.Services;
using System.Text.Json;

namespace RateWatch.AuthService.Infrastructure.Messaging
{
    public class KafkaProducer : IMessageProducer
    {
        private readonly IProducer<Null, string> _producer;

        public KafkaProducer(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];

            if (string.IsNullOrEmpty(bootstrapServers))
            {
                throw new InvalidOperationException("Kafka:BootstrapServers configuration is missing or empty.");
            }

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task ProduceAsync<T>(string topic, T message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = jsonMessage });
        }
    }
}
