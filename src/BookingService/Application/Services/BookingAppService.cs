using BookingService.Application.Dtos;
using BookingService.Application.Exceptions;
using BookingService.Application.Interfaces;
using BookingService.Domain.Bookings;
using BookingService.Infrastructure.Kafka.Producer;

namespace BookingService.Application.Services;

public sealed class BookingAppService : IBookingAppService
{
    private const string IoChannel = "payments.input|payments.output";
    private readonly IPaymentRequestedProducerService _producer;
    private readonly IBookingRepository _repo;
    private readonly ISportsObjectsClient _sports;

    public BookingAppService(IBookingRepository repo, ISportsObjectsClient sports, IPaymentRequestedProducerService producer)
    {
        _repo = repo;
        _sports = sports;
        _producer = producer;
    }

    public Task<Booking?> GetAsync(long id, CancellationToken cancellationToken)
    {
        return _repo.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Booking> CreateAsync(long sportsObjectId, DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken cancellationToken)
    {
        SportsObjectForBookingResult obj = await _sports.ObjectForBookingAsync(sportsObjectId, startsAt, endsAt, cancellationToken);

        if (obj.Status != SportsObjectBookingStatus.Ok)
            throw new InvalidOperationException($"Sport object is not available: {obj.Status}");

        double minutes = (endsAt - startsAt).TotalMinutes;

        long amount = (long)Math.Ceiling(
            (long)obj.PricePerHour * minutes / 60.0);
        var booking = Booking.Create(sportsObjectId, startsAt, endsAt, amount);
        return await _repo.CreateAsync(booking, cancellationToken);
    }

    public async Task<Booking> CancelAsync(long id, CancellationToken cancellationToken)
    {
        Booking? booking = await _repo.GetByIdAsync(id, cancellationToken);
        if (booking is null)
            throw new BookingNotFoundException(id);

        if (booking.Status == BookingStatus.CancelledNoPayment)
        {
            return booking;
        }

        if (booking.Status != BookingStatus.Created)
        {
            throw new InvalidBookingStateException(id, booking.Status, "Cannot cancel booking after payment has started");
        }

        booking.RequestCancel();

        return await _repo.UpdateAsync(booking, cancellationToken);
    }

    public async Task<Booking> StartPaymentAsync(long bookingId, CancellationToken cancellationToken)
    {
        string corr = Guid.NewGuid().ToString();

        Booking? updated = await _repo.StartPaymentAsync(bookingId, corr, IoChannel, cancellationToken);
        if (updated is null)
            throw new InvalidOperationException("Booking not found or cannot start payment from current status");

        await _producer.SendEventAsync(
            new PaymentRequestedEvent(
                "payment.requested",
                corr,
                IoChannel,
                updated.Id,
                updated.SportsObjectId,
                updated.Amount,
                updated.StartsAt,
                updated.EndsAt),
            cancellationToken);

        return updated;
    }
}