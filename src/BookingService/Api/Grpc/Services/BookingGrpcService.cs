using BookingService.Application.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService;

namespace BookingService.Api.Grpc.Services;

public class BookingGrpcService : BookingGrpc.BookingGrpcBase
{
    private readonly IBookingAppService _app;

    public BookingGrpcService(IBookingAppService app)
    {
        _app = app;
    }

    public override async Task<GetBookingResponse> GetBooking(GetBookingRequest request, ServerCallContext context)
    {
        Domain.Bookings.Booking? booking = await _app.GetAsync(request.Id, context.CancellationToken);
        if (booking is null)
            throw new RpcException(new Status(StatusCode.NotFound, "Booking not found"));

        return new GetBookingResponse { Booking = ToProto(booking) };
    }

    public override async Task<CreateBookingResponse> CreateBooking(CreateBookingRequest request, ServerCallContext context)
    {
        var startsAt = request.Interval.StartsAt.ToDateTimeOffset();
        var endsAt = request.Interval.EndsAt.ToDateTimeOffset();

        // проверить что доступен объект
        try
        {
            Domain.Bookings.Booking saved = await _app.CreateAsync(request.SportsObjectId, startsAt, endsAt, request.Amount, context.CancellationToken);
            return new CreateBookingResponse { Booking = ToProto(saved) };
        }
        catch (Exception ex) when (ex is ArgumentException)
        {
            // если домен кинул ArgumentException на валидации
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<CancelBookingResponse> CancelBooking(CancelBookingRequest request, ServerCallContext context)
    {
        Domain.Bookings.Booking updated = await _app.CancelAsync(request.Id, context.CancellationToken);
        return new CancelBookingResponse { Booking = ToProto(updated) };
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