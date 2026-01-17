using BookingService.Application.Interfaces;
using BookingService.Application.Services;
using BookingService.Infrastructure.HostedServices;

namespace BookingService.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IBookingAppService, BookingAppService>();
    }

    public static IServiceCollection AddMigrationServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService<MigrationBackgroundService>();
        return serviceCollection;
    }
}
