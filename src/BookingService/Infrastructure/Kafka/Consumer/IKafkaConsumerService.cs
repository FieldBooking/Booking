namespace BookingService.Infrastructure.Kafka.Consumer;

public interface IKafkaConsumerService
{
    Task ConsumeLoopAsync(string topic, CancellationToken cancellationToken);
}