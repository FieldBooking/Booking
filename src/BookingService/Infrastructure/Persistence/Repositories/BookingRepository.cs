using BookingService.Application.Exceptions;
using BookingService.Application.Interfaces;
using BookingService.Domain.Bookings;
using BookingService.Infrastructure.Persistence.Db.Entities;
using Npgsql;

namespace BookingService.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public BookingRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Booking?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           select id, sports_object_id, starts_at, ends_at, amount, status, created_at, updated_at
                           from bookings
                           where id = @id
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, connection);

        cmd.Parameters.Add(new NpgsqlParameter<long>("id", id));

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        BookingRow row = ReadRow(reader);
        return ToDomain(row);
    }

    public async Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(booking);

        const string sql = """
                           insert into bookings (sports_object_id, starts_at, ends_at, amount, status)
                           values (@sports_object_id, @starts_at, @ends_at, @amount, @status)
                           returning id, sports_object_id, starts_at, ends_at, amount, status, created_at, updated_at;
                           """;
        try
        {
            await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = new NpgsqlCommand(sql, connection);

            cmd.Parameters.Add(new NpgsqlParameter<long>("sports_object_id", booking.SportsObjectId));
            cmd.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("starts_at", booking.StartsAt));
            cmd.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("ends_at", booking.EndsAt));
            cmd.Parameters.Add(new NpgsqlParameter<long>("amount", booking.Amount));
            cmd.Parameters.Add(new NpgsqlParameter<BookingStatus>("status", BookingStatus.Created));

            await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new InvalidOperationException("Insert failed: no row returned.");

            BookingRow inserted = ReadRow(reader);
            return ToDomain(inserted);
        }
        catch (PostgresException ex) when (ex.SqlState == "23P01")
        {
            throw new SlotBusyException(
                booking.SportsObjectId,
                booking.StartsAt,
                booking.EndsAt,
                ex);
        }
    }

    public async Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(booking);

        const string sql = """
                           update bookings
                           set
                               sports_object_id = @sports_object_id,
                               starts_at        = @starts_at,
                               ends_at          = @ends_at,
                               amount           = @amount,
                               status           = @status,
                               updated_at       = now()
                           where id = @id
                           returning id, sports_object_id, starts_at, ends_at, amount, status, created_at, updated_at;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, connection);

        cmd.Parameters.Add(new NpgsqlParameter<long>("id", booking.Id));
        cmd.Parameters.Add(new NpgsqlParameter<long>("sports_object_id", booking.SportsObjectId));
        cmd.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("starts_at", booking.StartsAt));
        cmd.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("ends_at", booking.EndsAt));
        cmd.Parameters.Add(new NpgsqlParameter<long>("amount", booking.Amount));
        cmd.Parameters.Add(new NpgsqlParameter<BookingStatus>("status", booking.Status));

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Update failed: no row returned (booking not found?).");

        BookingRow updated = ReadRow(reader);
        return ToDomain(updated);
    }

    public async Task<Booking?> StartPaymentAsync(long bookingId, string correlationId, string ioChannel, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           update bookings
                           set
                             status = @status,
                             payment_correlation_id = @corr,
                             payment_io_channel = @io,
                             updated_at = now()
                           where id = @id and status = 'created'::booking_status
                           returning id, sports_object_id, starts_at, ends_at, amount, status, created_at, updated_at;
                           """;

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.Add(new NpgsqlParameter<long>("id", bookingId));
        cmd.Parameters.Add(new NpgsqlParameter<BookingStatus>("status", BookingStatus.PaymentInProgress));
        cmd.Parameters.Add(new NpgsqlParameter<string>("corr", correlationId));
        cmd.Parameters.Add(new NpgsqlParameter<string>("io", ioChannel));

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        BookingRow row = ReadRow(reader);
        return ToDomain(row);
    }

    public async Task<Booking?> ApplyPaymentResultAsync(
        long bookingId,
        string correlationId,
        string ioChannel,
        bool confirmed,
        CancellationToken cancellationToken = default)
    {
        BookingStatus newStatus = confirmed ? BookingStatus.Paid : BookingStatus.CancelledNoPayment;

        const string sql = """
                           update bookings
                           set
                             status = @new_status,
                             updated_at = now()
                           where id = @id
                             and payment_correlation_id = @corr
                             and payment_io_channel = @io
                             and status in ('payment_in_progress'::booking_status, 'cancel_requested_during_payment'::booking_status)
                           returning id, sports_object_id, starts_at, ends_at, amount, status, created_at, updated_at;
                           """;

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.Add(new NpgsqlParameter<long>("id", bookingId));
        cmd.Parameters.Add(new NpgsqlParameter<string>("corr", correlationId));
        cmd.Parameters.Add(new NpgsqlParameter<string>("io", ioChannel));
        cmd.Parameters.Add(new NpgsqlParameter<BookingStatus>("new_status", newStatus));

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        BookingRow row = ReadRow(reader);
        return ToDomain(row);
    }

    private static BookingRow ReadRow(NpgsqlDataReader reader)
    {
        return new BookingRow
        {
            Id = reader.GetInt64(0),
            SportsObjectId = reader.GetInt64(1),
            StartsAt = reader.GetFieldValue<DateTimeOffset>(2),
            EndsAt = reader.GetFieldValue<DateTimeOffset>(3),
            Amount = reader.GetInt64(4),
            Status = reader.GetFieldValue<BookingStatus>(5),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(6),
            UpdatedAt = reader.GetFieldValue<DateTimeOffset>(7),
        };
    }

    private static Booking ToDomain(BookingRow row)
    {
        return Booking.Build(
            row.Id,
            row.SportsObjectId,
            row.StartsAt,
            row.EndsAt,
            row.Amount,
            row.Status);
    }
}