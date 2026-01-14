using BookingService.Domain.Bookings;

namespace BookingService.Infrastructure.Persistence.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
    
}