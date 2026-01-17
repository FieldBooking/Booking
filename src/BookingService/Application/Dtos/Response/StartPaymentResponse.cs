namespace BookingService.Application.Dtos.Response;

public record StartPaymentResponse(long BookingId, string GenerationId);
