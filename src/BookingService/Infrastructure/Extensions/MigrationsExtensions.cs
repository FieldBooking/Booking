using BookingService.Infrastructure.ModelOptions;
using BookingService.Infrastructure.Persistence.Migrations;
using FluentMigrator.Runner;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Extensions;

public static class MigrationsExtensions
{
    public static void AddMigration(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(provider =>
                    provider.GetRequiredService<IOptions<PostgresConnect>>().Value.ToConnectionString())
                .WithMigrationsIn(typeof(InitialMigration).Assembly));
    }

    public static async Task Migrate(this IServiceProvider serviceProvider)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}