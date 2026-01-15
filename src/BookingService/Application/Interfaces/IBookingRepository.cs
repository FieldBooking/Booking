using BookingService.Domain.Bookings;

namespace BookingService.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<Booking?> StartPaymentAsync(long bookingId, string correlationId, string ioChannel, CancellationToken cancellationToken = default);

    Task<Booking?> ApplyPaymentResultAsync(
        long bookingId,
        string correlationId,
        string ioChannel,
        bool confirmed,
        CancellationToken cancellationToken = default);
}