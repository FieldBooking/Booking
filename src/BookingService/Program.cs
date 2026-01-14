using BookingService.Domain.Bookings;
using Npgsql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

WebApplication app = builder.Build();

const string connString = "fdg"; // вынести в extension позже

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString); // вынести в extension позже
dataSourceBuilder.MapEnum<BookingStatus>("booking_status"); // вынести в extension позже

app.MapGet(
    "/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();