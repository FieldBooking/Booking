namespace BookingService.Application.Dtos;

public record PaymentResultEvent(
    string EventType,
    long BookingId,
    string CorrelationId,
    string IoChannel);