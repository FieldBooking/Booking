using BookingService.Domain.Bookings;

namespace BookingService.Application.Exceptions;

public class InvalidBookingStateException : Exception
{
    public InvalidBookingStateException(long bookingId, BookingStatus status, string? message = null) : base(message ?? $"Invalid booking state: bookingId={bookingId}, status={status}")
    {
        BookingId = bookingId;
        Status = status;
    }

    public long BookingId { get; }

    public BookingStatus Status { get; }
}