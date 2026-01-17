using BookingService.Application.Interfaces;
using BookingService.Domain.Bookings;
using BookingService.Infrastructure.ModelOptions;
using BookingService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BookingService.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepository(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<PostgresConnect>(config.GetSection("Postgres"));

        services.AddSingleton<NpgsqlDataSource>(sp =>
        {
            IOptionsMonitor<PostgresConnect> pg = sp.GetRequiredService<IOptionsMonitor<PostgresConnect>>();
            var builder = new NpgsqlDataSourceBuilder(pg.CurrentValue.ToConnectionString());
            builder.MapEnum<BookingStatus>(pgName: "booking_status");
            pg.OnChange(postgresConnect => builder.ConnectionStringBuilder.ConnectionString = postgresConnect.ToConnectionString());
            return builder.Build();
        });

        services.AddScoped<IBookingRepository, BookingRepository>();
        return services;
    }
}