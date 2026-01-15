using BookingService.Application.Dtos;

namespace BookingService.Infrastructure.Kafka.Producer;

public interface IPaymentRequestedProducerService
{
    Task SendEventAsync(PaymentRequestedEvent requestedEvent, CancellationToken cancellationToken);
}