using BookingService.Domain.Bookings;
using BookingService.Infrastructure.Persistence.Db.Entities;
using BookingService.Infrastructure.Persistence.Db.Mappers;
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
                           select id, sports_object_id, client_id, starts_at, ends_at, amount, status, created_at, updated_at
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
        return BookingMapper.ToDomain(row);
    }

    public async Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        if (booking is null) throw new ArgumentNullException(nameof(booking));

        BookingRow row = BookingMapper.ToRow(booking);

        const string sql = """
                           insert into bookings (sports_object_id, client_id, starts_at, ends_at, amount, status)
                           values (@sports_object_id, @client_id, @starts_at, @ends_at, @amount, @status)
                           returning id, sports_object_id, client_id, starts_at, ends_at, amount, status, created_at, updated_at;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, connection);

        cmd.Parameters.Add(new NpgsqlParameter<long>("sports_object_id", row.SportsObjectId));
        cmd.Parameters.Add(new NpgsqlParameter<long>("client_id", row.ClientId));
        cmd.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("starts_at", row.StartsAt));
        cmd.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("ends_at", row.EndsAt));
        cmd.Parameters.Add(new NpgsqlParameter<long>("amount", row.Amount));
        cmd.Parameters.Add(new NpgsqlParameter<string>("status", row.Status));

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Insert failed: no row returned.");

        BookingRow inserted = ReadRow(reader);
        return BookingMapper.ToDomain(inserted);
    }

    public async Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        if (booking is null) throw new ArgumentNullException(nameof(booking));

        BookingRow row = BookingMapper.ToRow(booking);

        const string sql = """
                           update bookings
                           set
                               sports_object_id = @sports_object_id,
                               client_id        = @client_id,
                               starts_at        = @starts_at,
                               ends_at          = @ends_at,
                               amount           = @amount,
                               status           = @status,
                               updated_at       = now()
                           where id = @id
                           returning id, sports_object_id, client_id, starts_at, ends_at, amount, status, created_at, updated_at;
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, connection);

        cmd.Parameters.Add(new NpgsqlParameter<long>("id", row.Id));
        cmd.Parameters.Add(new NpgsqlParameter<long>("sports_object_id", row.SportsObjectId));
        cmd.Parameters.Add(new NpgsqlParameter<long>("client_id", row.ClientId));
        cmd.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("starts_at", row.StartsAt));
        cmd.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("ends_at", row.EndsAt));
        cmd.Parameters.Add(new NpgsqlParameter<long>("amount", row.Amount));
        cmd.Parameters.Add(new NpgsqlParameter<string>("status", row.Status));

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Update failed: no row returned (booking not found?).");

        BookingRow updated = ReadRow(reader);
        return BookingMapper.ToDomain(updated);
    }


    private static BookingRow ReadRow(NpgsqlDataReader reader)
    {
        return new BookingRow
        {
            Id = reader.GetInt64(0),
            SportsObjectId = reader.GetInt64(1),
            ClientId = reader.GetInt64(2),
            StartsAt = reader.GetFieldValue<DateTimeOffset>(3),
            EndsAt = reader.GetFieldValue<DateTimeOffset>(4),
            Amount = reader.GetInt64(5),
            Status = reader.GetString(6),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(7),
            UpdatedAt = reader.GetFieldValue<DateTimeOffset>(8),
        };
    }
}