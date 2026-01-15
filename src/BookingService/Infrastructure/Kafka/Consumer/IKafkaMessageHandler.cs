namespace BookingService.Infrastructure.Kafka.Consumer;

public interface IKafkaMessageHandler
{
    Task HandleAsync(string topic, string? key, string value, CancellationToken cancellationToken);
}