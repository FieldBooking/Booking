using BookingService.Domain.Bookings;
using BookingService.Infrastructure.Persistence.Db.Entities;

namespace BookingService.Infrastructure.Persistence.Db.Mappers;

public static class BookingMapper
{
    public static Booking ToDomain(BookingRow row)
    {
        if (row is null) throw new ArgumentNullException(nameof(row));

        if (!Enum.TryParse<BookingStatus>(row.Status, ignoreCase: false, out var status))
            throw new InvalidOperationException($"Unknown booking status value: '{row.Status}'.");

        return Booking.Build(
            id: row.Id,
            sportsObjectId: row.SportsObjectId,
            startsAt: row.StartsAt,
            endsAt: row.EndsAt,
            amount: row.Amount,
            status: status
        );
    }

    public static BookingRow ToRow(Booking booking)
    {
        if (booking is null) throw new ArgumentNullException(nameof(booking));

        return new BookingRow
        {
            Id = booking.Id,
            SportsObjectId = booking.SportsObjectId,
            StartsAt = booking.StartsAt,
            EndsAt = booking.EndsAt,
            Amount = booking.Amount,
            Status = booking.Status.ToString(),
        };
    }
}