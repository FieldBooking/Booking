using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Kafka.Consumer;

public class KafkaConsumerHostedService : BackgroundService
{
    private readonly IKafkaConsumerService _consumer;
    private readonly KafkaOptions _options;

    public KafkaConsumerHostedService(
        IKafkaConsumerService consumer,
        IOptions<KafkaOptions> options)
    {
        _consumer = consumer;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await _consumer.ConsumeLoopAsync(_options.PaymentResultTopic, stoppingToken);
    }
}