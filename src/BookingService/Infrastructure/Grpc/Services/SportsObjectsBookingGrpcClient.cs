using BookingService.Application.Dtos;
using BookingService.Application.Interfaces;
using SportsObjectsService;

namespace BookingService.Infrastructure.Grpc.Services;

public class SportsObjectsBookingGrpcClient : ISportsObjectsClient
{
    private readonly SportsObjectsBookingService.SportsObjectsBookingServiceClient _client;

    public SportsObjectsBookingGrpcClient(SportsObjectsBookingService.SportsObjectsBookingServiceClient client)
    {
        _client = client;
    }

    public async Task<SportsObjectForBookingResult> ObjectForBookingAsync(
        long sportObjectId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        CancellationToken cancellationToken = default)
    {
        int dayOfWeek = startsAt.LocalDateTime.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)startsAt.LocalDateTime.DayOfWeek;
        string startTime = startsAt.ToString("HH:mm");
        string endTime = endsAt.ToString("HH:mm");

        ObjectForBookingResponse? response = await _client.ObjectForBookingAsync(
            new ObjectForBookingRequest
            {
                SportObjectId = sportObjectId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
            },
            cancellationToken: cancellationToken);
        SportsObjectBookingStatus status = response.ObjectStatus switch
        {
            Status.Unspecified => SportsObjectBookingStatus.Unspecified,
            Status.Ok => SportsObjectBookingStatus.Ok,
            Status.NotFound => SportsObjectBookingStatus.NotFound,
            Status.Inactive => SportsObjectBookingStatus.Inactive,
            Status.OutOfSchedule => SportsObjectBookingStatus.OutOfSchedule,
            _ => SportsObjectBookingStatus.OutOfSchedule,
        };
        return new SportsObjectForBookingResult(
            status,
            sportObjectId,
            (decimal)response.PricePerHour);
    }
}