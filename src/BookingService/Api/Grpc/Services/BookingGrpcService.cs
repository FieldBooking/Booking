using BookingService.Application.Interfaces;
using BookingService.Infrastructure.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace BookingService.Api.Grpc.Services;

public sealed class BookingGrpcService : BookingGrpc.BookingGrpcBase
{
    private readonly IBookingAppService _app;

    public BookingGrpcService(IBookingAppService app)
    {
        _app = app;
    }

    public override async Task<GetBookingResponse> GetBooking(GetBookingRequest request, ServerCallContext context)
    {
        Domain.Bookings.Booking booking = await _app.GetAsync(request.Id, context.CancellationToken);

        return new GetBookingResponse
        {
            Booking = ToProto(booking),
        };
    }

    public override async Task<CreateBookingResponse> CreateBooking(CreateBookingRequest request, ServerCallContext context)
    {
        var date = DateOnly.ParseExact(request.Date, "yyyy-MM-dd");
        var start = TimeOnly.ParseExact(request.StartTime, "hh:mm");
        var end = TimeOnly.ParseExact(request.EndTime, "hh:mm");

        Domain.Bookings.Booking saved = await _app.CreateAsync(
            request.SportsObjectId,
            date,
            start,
            end,
            context.CancellationToken);

        return new CreateBookingResponse
        {
            Booking = ToProto(saved),
        };
    }

    public override async Task<CancelBookingResponse> CancelBooking(CancelBookingRequest request, ServerCallContext context)
    {
        Domain.Bookings.Booking updated = await _app.CancelAsync(request.Id, context.CancellationToken);
        return new CancelBookingResponse
        {
            Booking = ToProto(updated),
        };
    }

    public override async Task<StartPaymentResponse> StartPayment(StartPaymentRequest request, ServerCallContext context)
    {
        Application.Dtos.Response.StartPaymentResponse res = await _app.StartPaymentAsync(request.BookingId, context.CancellationToken);

        return new StartPaymentResponse
        {
            BookingId = res.BookingId,
            CorrelationId = res.GenerationId,
        };
    }

    private static Booking ToProto(Domain.Bookings.Booking booking)
    {
        return new Booking
        {
            Id = booking.Id,
            SportsObjectId = booking.SportsObjectId,
            Amount = booking.Amount,
            Status = booking.Status switch
            {
                Domain.Bookings.BookingStatus.Created => BookingStatus.Created,
                Domain.Bookings.BookingStatus.PaymentInProgress => BookingStatus.PaymentInProgress,
                Domain.Bookings.BookingStatus.CancelRequestedDuringPayment => BookingStatus.CancelRequestedDuringPayment,
                Domain.Bookings.BookingStatus.CancelledNoPayment => BookingStatus.CancelledNoPayment,
                Domain.Bookings.BookingStatus.Paid => BookingStatus.Paid,
                _ => BookingStatus.Unspecified,
            },
            Interval = new TimeInterval
            {
                StartsAt = Timestamp.FromDateTimeOffset(booking.StartsAt),
                EndsAt = Timestamp.FromDateTimeOffset(booking.EndsAt),
            },
        };
    }
}
