using BookingService.Application.Dtos;
using BookingService.Application.Interfaces;
using System.Text.Json;

namespace BookingService.Infrastructure.Kafka.Consumer;

public class PaymentResultHandler : IKafkaMessageHandler
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IBookingAppService _booking;

    public PaymentResultHandler(IBookingAppService booking)
    {
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        _booking = booking;
    }

    public async Task HandleAsync(string topic, string? key, string value, CancellationToken cancellationToken)
    {
        PaymentResultEvent? evt = JsonSerializer.Deserialize<PaymentResultEvent>(value, _jsonOptions);
        if (evt is null) return;

        bool? confirmed = evt.EventType switch
        {
            "payment.confirmed" => true,
            "payment.cancelled" => false,
            _ => null,
        };
        if (confirmed is null) return;

        await _booking.ApplyOrCancelPaymentForceAsync(evt.BookingId, evt.CorrelationId, evt.IoChannel, confirmed.Value, cancellationToken);
    }
}