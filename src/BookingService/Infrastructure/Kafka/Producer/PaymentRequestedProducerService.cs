using BookingService.Infrastructure.Kafka.Message;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BookingService.Infrastructure.Kafka.Producer;

public class PaymentRequestedProducerService : IPaymentRequestedProducerService
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly KafkaOptions _options;
    private readonly IProducer<string, string> _producer;

    public PaymentRequestedProducerService(IOptions<KafkaOptions> options)
    {
        _options = options.Value;
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            EnableIdempotence = true,
        };
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task SendEventAsync(PaymentRequestedEvent message, CancellationToken cancellationToken)
    {
        string payload = JsonSerializer.Serialize(message, _jsonOptions);

        var kafkaMessage = new Message<string, string>
        {
            Key = message.CorrelationId,
            Value = payload,
        };

        await _producer.ProduceAsync(
            _options.PaymentRequestTopic,
            kafkaMessage,
            cancellationToken);
    }
}