namespace BookingService.Domain.Bookings;

public enum BookingStatus
{
    Created,
    PaymentInProgress,
    CancelRequestedDuringPayment,
    CancelledNoPayment,
    Paid,
}