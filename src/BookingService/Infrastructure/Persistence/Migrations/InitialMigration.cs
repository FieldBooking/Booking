using FluentMigrator;

namespace BookingService.Infrastructure.Persistence.Migrations;

[Migration(1, "Initial booking schema")]
public class InitialMigration : Migration
{
    public override void Up()
    {
        Execute.Sql($"""

                     create type booking_status as enum (
                        'Created',
                         'PaymentInProgress',
                         'CancelRequestedDuringPayment',
                         'CancelledNoPayment',
                         'Paid'
                     );

                     create table if not exists booking
                     (
                         
                         
                         id bigint generated always as identity primary key,

                         sports_object_id bigint not null,

                         starts_at timestamptz not null,
                         ends_at   timestamptz not null,
                     

                         amount bigint not null,
                         status booking_status  not null,

                         created_at timestamptz not null default now(),
                         updated_at timestamptz not null default now(),
                     )
                     """);
    }

    public override void Down()
    {
        Execute.Sql($"""
                    drop table if exists booking
                    """);
    }
}