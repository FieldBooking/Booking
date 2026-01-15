using BookingService.Application.Exceptions;
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
        if (request.Interval is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "interval is required"));

        var startsAt = request.Interval.StartsAt.ToDateTimeOffset();
        var endsAt = request.Interval.EndsAt.ToDateTimeOffset();

        try
        {
            Domain.Bookings.Booking saved = await _app.CreateAsync(
                request.SportsObjectId,
                startsAt,
                endsAt,
                context.CancellationToken);

            return new CreateBookingResponse { Booking = ToProto(saved) };
        }
        catch (SlotBusyException)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "slot is busy"));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<CancelBookingResponse> CancelBooking(CancelBookingRequest request, ServerCallContext context)
    {
        try
        {
            Domain.Bookings.Booking updated = await _app.CancelAsync(request.Id, context.CancellationToken);
            return new CancelBookingResponse { Booking = ToProto(updated) };
        }
        catch (BookingNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Booking not found"));
        }
        catch (InvalidBookingStateException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<StartPaymentResponse> StartPayment(StartPaymentRequest request, ServerCallContext context)
    {
        try
        {
            Domain.Bookings.Booking updated = await _app.StartPaymentAsync(request.Id, context.CancellationToken);
            return new StartPaymentResponse
            {
                Booking = ToProto(updated),
            };
        }
        catch (BookingNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Booking not found"));
        }
        catch (InvalidBookingStateException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
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