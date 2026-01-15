using BookingService.Application.Dtos;

namespace BookingService.Application.Interfaces;

public interface ISportsObjectsClient
{
    Task<SportsObjectForBookingResult> ObjectForBookingAsync(
        long sportObjectId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        CancellationToken cancellationToken = default);
}