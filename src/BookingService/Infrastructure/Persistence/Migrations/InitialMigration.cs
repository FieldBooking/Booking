using FluentMigrator;

namespace BookingService.Infrastructure.Persistence.Migrations;

[Migration(1, "Initial booking schema")]
public class InitialMigration : Migration
{
    public override void Up()
    {
        Execute.Sql("""
                    create extension if not exists btree_gist;

                    create type booking_status as enum (
                        'created',
                        'payment_in_progress',
                        'cancel_requested_during_payment',
                        'cancelled_no_payment',
                        'paid'
                    );

                    create table if not exists bookings
                    (
                        
                        
                        id bigint generated always as identity primary key,
                    
                        sports_object_id bigint not null,
                    
                        starts_at timestamptz not null,
                        ends_at   timestamptz not null,
                    
                    
                        amount bigint not null,
                        status booking_status  not null,
                    
                        payment_correlation_id text null,
                        payment_io_channel text null,
                    
                        created_at timestamptz not null default now(),
                        updated_at timestamptz not null default now()
                    );

                    alter table bookings
                    add constraint bookings_no_overlap
                    exclude using gist
                    (
                      sports_object_id with =,
                      tstzrange(starts_at, ends_at, '[)') with &&
                    )
                    where (status <> 'cancelled_no_payment');
                    """);
    }

    public override void Down()
    {
        Execute.Sql("""
                        drop table if exists bookings;
                        drop type if exists booking_status;
                    """);
    }
}