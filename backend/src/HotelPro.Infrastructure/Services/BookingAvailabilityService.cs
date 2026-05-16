using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Interfaces;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HotelPro.Infrastructure.Services;

public class BookingAvailabilityService : IBookingAvailabilityService
{
    private readonly HotelProDbContext _dbContext;
    private readonly IBookingRepository _bookingRepository;
    private readonly IFeatureFlagService _featureFlags;

    public BookingAvailabilityService(
        HotelProDbContext dbContext,
        IBookingRepository bookingRepository,
        IFeatureFlagService featureFlags)
    {
        _dbContext = dbContext;
        _bookingRepository = bookingRepository;
        _featureFlags = featureFlags;
    }

    public async Task<AvailabilityResult> CheckAvailabilityAsync(AvailabilityRequest request)
    {
        var totalRooms = await _dbContext.Rooms
            .CountAsync(r => r.RoomTypeId == request.RoomTypeId && r.IsActive);

        var conflictingCount = await _bookingRepository.CountConflictingBookingsAsync(
            request.RoomTypeId,
            request.Arrival,
            request.Departure,
            request.ExcludeBookingId);

        var allowOverbooking = await _featureFlags.IsEnabledAsync("AllowOverbooking");
        var available = allowOverbooking
            ? totalRooms
            : totalRooms - conflictingCount;

        var isAvailable = allowOverbooking || available >= request.Quantity;

        var conflictingPeriods = await GetConflictingPeriodsAsync(
            request.RoomTypeId,
            request.Arrival,
            request.Departure,
            request.ExcludeBookingId);

        return new AvailabilityResult(
            IsAvailable: isAvailable,
            AvailableQuantity: Math.Max(0, available),
            TotalRoomsOfType: totalRooms,
            ConflictingPeriods: conflictingPeriods
        );
    }

    public async Task<LockResult> LockRoomsAsync(LockRequest request)
    {
        if (_dbContext.Database.IsRelational())
        {
            return await LockRoomsWithPostgresAsync(request);
        }

        return await LockRoomsInMemoryAsync(request);
    }

    private async Task<LockResult> LockRoomsWithPostgresAsync(LockRequest request)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            var lockTimeoutSql = "SET LOCAL lock_timeout = '5s'";
            await _dbContext.Database.ExecuteSqlRawAsync(lockTimeoutSql);

            try
            {
                await _bookingRepository.AcquireRoomTypeLockAsync(
                    request.RoomTypeId,
                    request.Arrival,
                    request.Departure);
            }
            catch (PostgresException ex) when (ex.SqlState == "55P03")
            {
                await transaction.RollbackAsync();
                return new LockResult(false, null,
                    "Soba je trenutno zakljucana od strane drugog korisnika. Pokusajte ponovo.");
            }
            catch (PostgresException ex) when (ex.SqlState == "57014")
            {
                await transaction.RollbackAsync();
                return new LockResult(false, null,
                    "Vremensko ogranicenje za zakljucavanje sobe je isteklo (5s). Pokusajte ponovo.");
            }

            var availability = await CheckAvailabilityAsync(new AvailabilityRequest(
                request.RoomTypeId,
                request.Arrival,
                request.Departure,
                request.Quantity));

            if (!availability.IsAvailable)
            {
                await transaction.RollbackAsync();
                return new LockResult(false, null,
                    $"Nema dovoljno soba. Dostupno: {availability.AvailableQuantity}, Trazeno: {request.Quantity}");
            }

            await transaction.CommitAsync();
            return new LockResult(true, $"lock-{Guid.NewGuid():N}", null);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<LockResult> LockRoomsInMemoryAsync(LockRequest request)
    {
        var availability = await CheckAvailabilityAsync(new AvailabilityRequest(
            request.RoomTypeId,
            request.Arrival,
            request.Departure,
            request.Quantity));

        if (!availability.IsAvailable)
        {
            return new LockResult(false, null,
                $"Nema dovoljno soba. Dostupno: {availability.AvailableQuantity}, Trazeno: {request.Quantity}");
        }

        return new LockResult(true, $"lock-{Guid.NewGuid():N}", null);
    }

    public Task ReleaseRoomLockAsync(string lockId)
    {
        return Task.CompletedTask;
    }

    private async Task<List<DateRange>> GetConflictingPeriodsAsync(
        Guid roomTypeId,
        DateTime arrival,
        DateTime departure,
        Guid? excludeBookingId)
    {
        var conflicts = await _bookingRepository.GetConflictingBookingsAsync(
            roomTypeId, arrival, departure, excludeBookingId);

        return conflicts
            .Where(br => br.Booking != null)
            .Select(br => new DateRange(br.Booking!.ArrivalDate, br.Booking!.DepartureDate))
            .ToList();
    }
}
