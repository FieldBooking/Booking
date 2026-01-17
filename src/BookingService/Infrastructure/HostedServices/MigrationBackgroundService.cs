using BookingService.Infrastructure.Extensions;

namespace BookingService.Infrastructure.HostedServices;

public class MigrationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationBackgroundService(IServiceProvider serviceCollection)
    {
        _serviceProvider = serviceCollection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _serviceProvider.Migrate();
    }
}