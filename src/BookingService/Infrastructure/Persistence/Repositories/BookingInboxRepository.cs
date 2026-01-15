using BookingService.Application.Interfaces;
using Npgsql;

namespace BookingService.Infrastructure.Persistence.Repositories;

public class BookingInboxRepository : IBookingInboxRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public BookingInboxRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task InsertAsync(string eventType, long bookingId, string correlationId, string ioChannel, CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into booking_inbox (event_type, booking_id, correlation_id, io_channel)
                           values (@etype, @bid , @corr, @io )
                           on conflict (event_type,correlation_id, io_channel) do nothing;
                           """;

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.Add(new NpgsqlParameter<string>("etype", eventType));
        cmd.Parameters.Add(new NpgsqlParameter<long>("bid", bookingId));
        cmd.Parameters.Add(new NpgsqlParameter<string>("corr", correlationId));
        cmd.Parameters.Add(new NpgsqlParameter<string>("io", ioChannel));
    }
}