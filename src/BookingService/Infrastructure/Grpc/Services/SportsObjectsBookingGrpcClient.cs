using BookingService.Application.Dtos;
using SportsObjectsService;

namespace BookingService.Infrastructure.Grpc.Services;

public class SportsObjectsBookingGrpcClient
{
    private readonly SportsObjectsBookingService.SportsObjectsBookingServiceClient _client;

    public SportsObjectsBookingGrpcClient(SportsObjectsBookingService.SportsObjectsBookingServiceClient client)
    {
        _client = client;
    }

    public async Task<SportsObjectForBookingResult> ObjectForBookingAsync(
        long sportObjectId,
        int dayOfWeek,
        string startsAt,
        string endsAt,
        CancellationToken cancellationToken)
    {
        ObjectForBookingResponse? response = await _client.ObjectForBookingAsync(
            new ObjectForBookingRequest
            {
                SportObjectId = sportObjectId,
                DayOfWeek = dayOfWeek,
                StartTime = startsAt,
                EndTime = endsAt,
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
        return new SportsObjectForBookingResult(status, sportObjectId, (decimal)response.PricePerHour);
    }
}