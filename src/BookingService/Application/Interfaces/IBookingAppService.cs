using BookingService.Domain.Bookings;

namespace BookingService.Application.Interfaces;

public interface IBookingAppService
{
    Task<Booking?> GetAsync(long id, CancellationToken cancellationToken);

    Task<Booking> CreateAsync(long sportsObjectId, DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken cancellationToken);

    Task<Booking> CancelAsync(long id, CancellationToken cancellationToken);

    Task<Booking> StartPaymentAsync(long bookingId, CancellationToken cancellationToken);
}