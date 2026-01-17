namespace BookingService.Infrastructure.Kafka.Message;

public record PaymentRequestedEvent
(
    string EventType,
    string CorrelationId,
    string IoChannel,
    long BookingId,
    long SportsObjectId,
    long Amount,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
