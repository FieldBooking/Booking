using BookingService.Api.Grpc;
using BookingService.Api.Grpc.Services;
using BookingService.Infrastructure.Extensions;
using BookingService.Infrastructure.ModelOptions;
using BookingService.Options;
using Microsoft.AspNetCore.Server.Kestrel.Core;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PostgresConnect>(builder.Configuration.GetSection("Postgres"));
builder.Services.AddApplicationInjection(builder.Configuration);

builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(
    builder.Configuration.GetSection("GrpcServer").Get<GrpcServerOptions>()?.ServerPort ?? 7200,
    listenOptions => listenOptions.Protocols = HttpProtocols.Http2));

builder.Services.AddScoped<ErrorHandlingInterceptor>();
builder.Services.AddGrpc(options => options.Interceptors.Add<ErrorHandlingInterceptor>());
WebApplication app = builder.Build();

app.MapGrpcService<BookingGrpcService>();
app.Run();