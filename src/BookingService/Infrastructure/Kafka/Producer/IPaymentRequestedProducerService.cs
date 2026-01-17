using BookingService.Infrastructure.Kafka.Message;

namespace BookingService.Infrastructure.Kafka.Producer;

public interface IPaymentRequestedProducerService
{
    Task SendEventAsync(PaymentRequestedEvent message, CancellationToken cancellationToken);
}