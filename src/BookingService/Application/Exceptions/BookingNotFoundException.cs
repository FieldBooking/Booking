namespace BookingService.Application.Exceptions;

public class BookingNotFoundException : Exception
{
    public BookingNotFoundException(long bookingId)
        : base($"Booking not found: {bookingId}")
    {
        BookingId = bookingId;
    }

    public long BookingId { get; }
}