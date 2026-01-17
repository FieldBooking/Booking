using BookingService.Domain.Bookings;

namespace BookingService.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken);

    Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken);

    Task<Booking?> StartPaymentAsync(long bookingId, string correlationId, string ioChannel, CancellationToken cancellationToken);

    Task<Booking?> ApplyPaymentResultAsync(
        long bookingId,
        string correlationId,
        string ioChannel,
        bool confirmed,
        CancellationToken cancellationToken);
}