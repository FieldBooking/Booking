namespace BookingService.Application.Interfaces;

public interface ISportsObjectsClient
{
    Task<bool> CheckAvailabilityAsync(long sportsObjectId, DateTimeOffset startsAt, DateTimeOffset endsAt, CancellationToken ct = default);
}