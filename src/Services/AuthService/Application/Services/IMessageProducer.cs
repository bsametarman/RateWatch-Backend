namespace RateWatch.AuthService.Application.Services
{
    public interface IMessageProducer
    {
        Task ProduceAsync<T>(string topic, T message);
    }
}
