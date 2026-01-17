using BookingService.Application.Dtos;
using BookingService.Application.Dtos.Response;
using BookingService.Application.Exceptions;
using BookingService.Application.Interfaces;
using BookingService.Domain.Bookings;
using BookingService.Infrastructure.Grpc.Services;
using BookingService.Infrastructure.Kafka.Message;
using BookingService.Infrastructure.Kafka.Producer;

namespace BookingService.Application.Services;

public sealed class BookingAppService : IBookingAppService
{
    private const string IoChannel = "payments.input|payments.output";
    private readonly IPaymentRequestedProducerService _producer;
    private readonly IBookingRepository _repo;
    private readonly SportsObjectsBookingGrpcClient _sports;

    public BookingAppService(IBookingRepository repo, SportsObjectsBookingGrpcClient sports, IPaymentRequestedProducerService producer)
    {
        _repo = repo;
        _sports = sports;
        _producer = producer;
    }

    public async Task<Booking> GetAsync(long id, CancellationToken cancellationToken)
    {
        return await _repo.GetByIdAsync(id, cancellationToken) ?? throw new BookingNotFoundException(id);
    }

    public async Task<Booking> CreateAsync(long sportsObjectId, DateOnly dateOnly, TimeOnly startTime, TimeOnly endTime, CancellationToken cancellationToken)
    {
        if (startTime >= endTime)
            throw new ArgumentException("startTime must be less than endTime");

        var startsAt = new DateTimeOffset(dateOnly.Year, dateOnly.Month, dateOnly.Day, startTime.Hour, startTime.Minute, 0, TimeSpan.Zero);
        var endsAt = new DateTimeOffset(dateOnly.Year, dateOnly.Month, dateOnly.Day, endTime.Hour, endTime.Minute, 0, TimeSpan.Zero);

        int dayOfWeek = (((int)startsAt.DayOfWeek + 6) % 7) + 1;

        SportsObjectForBookingResult obj = await _sports.ObjectForBookingAsync(sportsObjectId, dayOfWeek, startTime.ToString(), endTime.ToString(), cancellationToken);

        if (obj.Status != SportsObjectBookingStatus.Ok)
        {
            throw new InvalidOperationException($"Sport object is not available: {obj.Status}");
        }

        decimal minutes = (decimal)(endsAt - startsAt).TotalMinutes;
        long amount = (long)Math.Ceiling(obj.PricePerHour * minutes / 60m);
        var booking = Booking.Create(sportsObjectId, startsAt, endsAt, amount);

        return await _repo.CreateAsync(booking, cancellationToken);
    }

    public async Task<Booking> CancelAsync(long id, CancellationToken cancellationToken)
    {
        // через ручка достыпны только при создании брони(до оплаты)
        Booking booking = await GetAsync(id, cancellationToken);
        if (booking.Status == BookingStatus.CancelledNoPayment)
        {
            return booking;
        }

        booking.RequestCancel();

        return await _repo.UpdateAsync(booking, cancellationToken);
    }

    public async Task<Booking> ApplyOrCancelPaymentForceAsync(
        long bookingId,
        string correlationId,
        string ioChannel,
        bool confirmed,
        CancellationToken cancellationToken)
    {
        return await _repo.ApplyPaymentResultAsync(bookingId, correlationId, ioChannel, confirmed, cancellationToken) ?? throw new InvalidOperationException("Booking not found or cannot apply payment result");
    }

    public async Task<StartPaymentResponse> StartPaymentAsync(
        long bookingId,
        CancellationToken cancellationToken)
    {
        string correlationId = Guid.NewGuid().ToString();

        Booking updated = await _repo.StartPaymentAsync(bookingId, correlationId, IoChannel, cancellationToken) ?? throw new InvalidOperationException("Booking not found or cannot start payment");

        await _producer.SendEventAsync(
            new PaymentRequestedEvent(
                EventType: "payment.requested",
                CorrelationId: correlationId,
                IoChannel: IoChannel,
                BookingId: updated.Id,
                SportsObjectId: updated.SportsObjectId,
                Amount: updated.Amount,
                StartsAt: updated.StartsAt,
                EndsAt: updated.EndsAt),
            cancellationToken);

        return new StartPaymentResponse(
            updated.Id,
            correlationId);
    }
}