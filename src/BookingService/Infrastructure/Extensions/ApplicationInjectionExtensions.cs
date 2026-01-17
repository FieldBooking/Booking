using BookingService.Application.Interfaces;
using BookingService.Application.Services;
using BookingService.Infrastructure.Grpc.Services;
using BookingService.Infrastructure.HostedServices;
using Microsoft.Extensions.Options;
using SportsObjectsService;

namespace BookingService.Infrastructure.Extensions;

public static class ApplicationInjectionExtensions
{
    public static IServiceCollection AddApplicationInjection(this IServiceCollection services, IConfiguration config)
    {
        services.AddMigration();
        services.AddScoped<IBookingAppService, BookingAppService>();
        services.AddRepository(config);
        services.AddHostedService<MigrationBackgroundService>();
        services.AddKafka(config);

        services.Configure<Options.ClientOptions>(config.GetSection("ClientOptions"));
        services.AddGrpcClient<SportsObjectsBookingService.SportsObjectsBookingServiceClient>(
            (sp, o) =>
            {
                IOptions<Options.ClientOptions> options = sp.GetRequiredService<IOptions<Options.ClientOptions>>();
                o.Address = options.Value.ConnectionUrlObject;
            });

        services.AddScoped<SportsObjectsBookingGrpcClient>();

        return services;
    }
}