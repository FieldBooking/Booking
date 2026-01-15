namespace BookingService.Application.Dtos;

public record PaymentRequestedEvent(
    string EventType,
    string CorrelationId,
    string IoChannel,
    long BookingId,
    long SportsObjectId,
    long Amount,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);