namespace BookingService.Application.Interfaces;

public interface IBookingInboxRepository
{
    Task InsertAsync(string eventType, long bookingId, string correlationId, string ioChannel, CancellationToken cancellationToken);
}