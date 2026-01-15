namespace BookingService.Application.Exceptions;

public class SlotBusyException : Exception
{
    public SlotBusyException(long sportsObjectId, DateTimeOffset startsAt, DateTimeOffset endsAt, Exception? inner = null)
        : base($"Slot is busy for sportObjectId={sportsObjectId} [{startsAt:o}..{endsAt:o}]", inner)
    {
        SportsObjectId = sportsObjectId;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public long SportsObjectId { get; }

    public DateTimeOffset StartsAt { get; }

    public DateTimeOffset EndsAt { get; }
}