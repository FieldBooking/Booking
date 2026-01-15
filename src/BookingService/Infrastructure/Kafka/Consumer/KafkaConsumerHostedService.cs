using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Kafka.Consumer;

public class KafkaConsumerHostedService : BackgroundService
{
    private readonly IKafkaConsumerService _consumer;
    private readonly ILogger<KafkaConsumerHostedService> _logger;
    private readonly KafkaOptions _options;

    public KafkaConsumerHostedService(
        IKafkaConsumerService consumer,
        IOptions<KafkaOptions> options,
        ILogger<KafkaConsumerHostedService> logger)
    {
        _consumer = consumer;
        _options = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KafkaConsumerHostedService started");
        return _consumer.ConsumeLoopAsync(_options.PaymentResultTopic, stoppingToken);
    }
}