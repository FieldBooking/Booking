namespace BookingService.Application.Dtos;

public record SportsObjectForBookingResult(
    SportsObjectBookingStatus Status,
    long SportObjectId,
    decimal PricePerHour);