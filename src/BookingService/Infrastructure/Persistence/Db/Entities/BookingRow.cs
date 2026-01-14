namespace BookingService.Infrastructure.Persistence.Db.Entities;

public class BookingRow
{
    public long Id { get; set; }

    public long SportsObjectId { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    public long Amount { get; set; }

    public required string Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}