using BookingService.Application.Dtos.Response;
using BookingService.Domain.Bookings;

namespace BookingService.Application.Interfaces;

public interface IBookingAppService
{
    Task<Booking> GetAsync(long id, CancellationToken cancellationToken);

    Task<Booking> CreateAsync(long sportsObjectId, DateOnly dateOnly, TimeOnly startTime, TimeOnly endTime, CancellationToken cancellationToken);

    Task<Booking> CancelAsync(long id, CancellationToken cancellationToken);

    Task<StartPaymentResponse> StartPaymentAsync(long bookingId, CancellationToken cancellationToken);

    Task<Booking> ApplyOrCancelPaymentForceAsync(long bookingId, string correlationId, string ioChannel, bool confirmed, CancellationToken cancellationToken);
}