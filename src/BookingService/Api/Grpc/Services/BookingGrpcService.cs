using BookingService.Infrastructure.Persistence.Repositories;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService;

namespace BookingService.Api.Grpc.Services;

public class BookingGrpcService : BookingGrpc.BookingGrpcBase
{
    private readonly IBookingRepository _repo;

    public BookingGrpcService(IBookingRepository repo)
    {
        _repo = repo;
    }

    public override async Task<GetBookingResponse> GetBooking(GetBookingRequest request, ServerCallContext context)
    {
        Domain.Bookings.Booking? booking = await _repo.GetByIdAsync(request.Id, context.CancellationToken);
        if (booking is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Booking not found"));
        }

        return new GetBookingResponse
        {
            Booking = ToProto(booking),
        };
    }

    public override async Task<CreateBookingResponse> CreateBooking(CreateBookingRequest request, ServerCallContext context)
    {
        var startsAt = request.Interval.StartsAt.ToDateTimeOffset();
        var endsAt = request.Interval.EndsAt.ToDateTimeOffset();

        // проверить что доступен объект
        var booking = Domain.Bookings.Booking.Create(
            request.SportsObjectId,
            startsAt,
            endsAt,
            request.Amount);

        Domain.Bookings.Booking saved = await _repo.CreateAsync(booking, context.CancellationToken);

        return new CreateBookingResponse
        {
            Booking = ToProto(saved),
        };
    }

    public override async Task<CancelBookingResponse> CancelBooking(CancelBookingRequest request, ServerCallContext context)
    {
        Domain.Bookings.Booking? booking = await _repo.GetByIdAsync(request.Id, context.CancellationToken);

        if (booking is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Booking not found"));
        }

        booking.RequestCancel();
        Domain.Bookings.Booking updated = await _repo.UpdateAsync(booking, context.CancellationToken);
        return new CancelBookingResponse
        {
            Booking = ToProto(updated),
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