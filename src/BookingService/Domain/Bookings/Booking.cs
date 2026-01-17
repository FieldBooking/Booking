using BookingService.Application.Exceptions;

namespace BookingService.Domain.Bookings;

public class Booking
{
    private Booking() { }

    private Booking(long sportsObjectId, DateTimeOffset startsAt, DateTimeOffset endsAt, long amount)
    {
        if (endsAt <= startsAt) throw new ArgumentException("Invalid time for start time and end time");
        if (amount < 0) throw new ArgumentException("Amount must be non-negative.");

        SportsObjectId = sportsObjectId;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Amount = amount;

        Status = BookingStatus.Created;
    }

    public long Id { get; private set; }

    public long SportsObjectId { get; private set; }

    public BookingStatus Status { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public long Amount { get; private set; }

    public static Booking Build(
        long id,
        long sportsObjectId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        long amount,
        BookingStatus status)
    {
        if (endsAt <= startsAt) throw new ArgumentException("Invalid time for start time and end time");
        if (amount < 0) throw new ArgumentException("Amount must be non-negative.");

        ValidateSlot(startsAt, endsAt);

        return new Booking
        {
            Id = id,
            SportsObjectId = sportsObjectId,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Amount = amount,
            Status = status,
        };
    }

    public static Booking Create(long sportsObjectId, DateTimeOffset startsAt, DateTimeOffset endsAt, long amount)
    {
        return new Booking(sportsObjectId, startsAt, endsAt, amount);
    }

    public void MarkPaymentInProgress()
    {
        if (Status != BookingStatus.Created) throw new InvalidOperationException($"Cannot start payment from status {Status}.");
        Status = BookingStatus.PaymentInProgress;
    }

    public void RequestCancel()
    {
        switch (Status)
        {
            case BookingStatus.CancelledNoPayment:
                break;
            case BookingStatus.Paid:
                throw new InvalidBookingStateException(Id, Status);
            case BookingStatus.Created:
                Status = BookingStatus.CancelledNoPayment;
                break;
            case BookingStatus.PaymentInProgress:
                Status = BookingStatus.CancelRequestedDuringPayment;
                break; // это спецом чтобы не было гонки брони(типо один отменил, но оплата все еще висит, а другой уже пытается забронить)
            case BookingStatus.CancelRequestedDuringPayment:
                throw new InvalidBookingStateException(Id, Status);
            default:
                throw new ArgumentOutOfRangeException($"Cannot request cancel from status {Status}.");
        }
    }

    public void MarkPaid()
    {
        if (Status is not (BookingStatus.PaymentInProgress or BookingStatus.CancelRequestedDuringPayment))
            throw new InvalidOperationException($"Cannot mark paid from status {Status}.");
        Status = BookingStatus.Paid;
    }

    public void MarkPaymentFailed()
    {
        if (Status is not (BookingStatus.PaymentInProgress or BookingStatus.CancelRequestedDuringPayment))
            throw new InvalidOperationException($"Cannot mark payment failed from status {Status}.");

        Status = Status == BookingStatus.CancelRequestedDuringPayment
            ? BookingStatus.CancelledNoPayment
            : BookingStatus.Created;
    }

    private static void ValidateSlot(DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        if (startsAt == default) throw new ArgumentException("StartsAt must be set.");
        if (endsAt == default) throw new ArgumentException("EndsAt must be set.");
        if (endsAt <= startsAt) throw new ArgumentException("EndsAt must be after StartsAt.");
    }
}