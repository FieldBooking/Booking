using BookingService.Application.Dtos;
using BookingService.Application.Interfaces;
using System.Text.Json;

namespace BookingService.Infrastructure.Kafka.Consumer;

public class PaymentResultHandler : IKafkaMessageHandler
{
    private readonly JsonSerializerOptions _jsonOptions;

    private readonly IServiceProvider _sp;

    public PaymentResultHandler(IServiceProvider sp)
    {
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        _sp = sp;
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

        using IServiceScope scope = _sp.CreateScope();
        IBookingInboxRepository inbox = scope.ServiceProvider.GetRequiredService<IBookingInboxRepository>();
        IBookingRepository repo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        await inbox.InsertAsync(evt.EventType, evt.BookingId, evt.CorrelationId, evt.IoChannel, cancellationToken);

        await repo.ApplyPaymentResultAsync(evt.BookingId, evt.CorrelationId, evt.IoChannel, confirmed.Value, cancellationToken);
    }
}