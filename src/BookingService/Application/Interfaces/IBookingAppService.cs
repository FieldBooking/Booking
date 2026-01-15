using BookingService.Domain.Bookings;

namespace BookingService.Application.Interfaces;

public interface IBookingAppService
{
    Task<Booking?> GetAsync(long id, CancellationToken ct);

    Task<Booking> CreateAsync(long sportsObjectId, DateTimeOffset startsAt, DateTimeOffset endsAt, long amount, CancellationToken ct);

    Task<Booking> CancelAsync(long id, CancellationToken ct);
}