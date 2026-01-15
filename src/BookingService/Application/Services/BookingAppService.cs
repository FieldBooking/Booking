using BookingService.Application.Dtos;
using BookingService.Application.Interfaces;
using BookingService.Domain.Bookings;
using BookingService.Infrastructure.Persistence.Repositories;

namespace BookingService.Application.Services;

public sealed class BookingAppService : IBookingAppService
{
    private readonly IBookingRepository _repo;
    private readonly ISportsObjectsClient _sports;

    public BookingAppService(IBookingRepository repo, ISportsObjectsClient sports)
    {
        _repo = repo;
        _sports = sports;
    }

    public Task<Booking?> GetAsync(long id, CancellationToken ct)
    {
        return _repo.GetByIdAsync(id, ct);
    }

    public async Task<Booking> CreateAsync(long sportsObjectId, DateTimeOffset startsAt, DateTimeOffset endsAt, long amount, CancellationToken ct)
    {
        SportsObjectForBookingResult obj = await _sports.ObjectForBookingAsync(sportsObjectId, startsAt, endsAt, ct);

        if (obj.Status != SportsObjectBookingStatus.Ok)
            throw new InvalidOperationException($"Sport object is not available: {obj.Status}");

        var booking = Booking.Create(sportsObjectId, startsAt, endsAt, amount);
        return await _repo.CreateAsync(booking, ct);
    }

    public async Task<Booking> CancelAsync(long id, CancellationToken ct)
    {
        Booking? booking = await _repo.GetByIdAsync(id, ct);
        if (booking is null)
            throw new NullReferenceException();

        if (booking.Status != BookingStatus.Created && booking.Status != BookingStatus.CancelledNoPayment)
            throw new InvalidOperationException($"{booking.Status}");

        booking.RequestCancel();

        return await _repo.UpdateAsync(booking, ct);
    }
}