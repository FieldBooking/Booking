using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Kafka.Consumer;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IKafkaMessageHandler _handler;
    private readonly KafkaOptions _options;

    public KafkaConsumerService(
        IOptions<KafkaOptions> options,
        IKafkaMessageHandler handler)
    {
        _options = options.Value;
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.PaymentResultConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _handler = handler;
    }

    public async Task ConsumeLoopAsync(string topic, CancellationToken cancellationToken)
    {
        _consumer.Subscribe(topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                ConsumeResult<string, string>? cr = _consumer.Consume(cancellationToken);
                if (cr?.Message?.Value is null)
                    continue;

                await _handler.HandleAsync(topic, cr.Message.Key, cr.Message.Value, cancellationToken);

                _consumer.Commit(cr);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                await Task.Delay(300, cancellationToken);
            }
        }

        _consumer.Close();
    }
}