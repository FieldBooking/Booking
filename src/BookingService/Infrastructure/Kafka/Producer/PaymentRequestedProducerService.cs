using BookingService.Application.Dtos;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text;
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

    public async Task SendEventAsync(PaymentRequestedEvent requestedEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestedEvent);
        string key = requestedEvent.BookingId.ToString();
        string value = JsonSerializer.Serialize(requestedEvent, _jsonOptions);

        var message = new Message<string, string>
        {
            Key = key,
            Value = value,
            Headers =
            [
                new Header("event-type", Encoding.UTF8.GetBytes(requestedEvent.EventType)),
                new Header("correlation-id", Encoding.UTF8.GetBytes(requestedEvent.CorrelationId)),
                new Header("io-channel", Encoding.UTF8.GetBytes(requestedEvent.IoChannel)),
            ],
        };

        try
        {
            await _producer.ProduceAsync(_options.PaymentRequestTopic, message, cancellationToken);
        }
        catch (ProduceException<string, string> ex)
        {
            throw new ApplicationException(ex.Message);
        }
    }
}