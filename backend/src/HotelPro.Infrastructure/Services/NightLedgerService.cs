using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.Services;

public class NightLedgerService : INightLedgerService
{
    private readonly HotelProDbContext _dbContext;
    private readonly ILogger<NightLedgerService> _logger;

    public NightLedgerService(
        HotelProDbContext dbContext,
        ILogger<NightLedgerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<StayNightDto>> GetNightsForStayAsync(Guid stayId)
    {
        var nights = await _dbContext.StayNights
            .Include(n => n.Room)
            .Where(n => n.StayId == stayId)
            .OrderBy(n => n.Date)
            .ToListAsync();

        return nights.Select(MapToDto);
    }

    public async Task<IEnumerable<StayNightDto>> GetNightsForRoomAsync(Guid roomId, DateTime? date = null)
    {
        var query = _dbContext.StayNights
            .Include(n => n.Room)
            .Where(n => n.RoomId == roomId);

        if (date.HasValue)
            query = query.Where(n => n.Date == date.Value.Date);

        var nights = await query.OrderBy(n => n.Date).ToListAsync();
        return nights.Select(MapToDto);
    }

    public async Task<IEnumerable<StayNightDto>> GetActiveNightsForFolioAsync(Guid folioId)
    {
        var nights = await _dbContext.StayNights
            .Include(n => n.Room)
            .Where(n => n.FolioId == folioId && n.Status == NightStatus.Active)
            .OrderBy(n => n.Date)
            .ToListAsync();

        return nights.Select(MapToDto);
    }

    public async Task<StayNightDto> UpdateNightTariffAsync(Guid nightId, decimal newTariff)
    {
        var night = await _dbContext.StayNights.FindAsync(nightId);
        if (night == null)
            throw new InvalidOperationException($"Night {nightId} not found.");

        if (night.Status == NightStatus.Closed)
            throw new InvalidOperationException("Cannot modify a closed night charge.");

        var oldTariff = night.TariffAmount;
        var delta = newTariff - oldTariff;

        night.TariffAmount = newTariff;
        night.Description = night.DiscountPercent > 0
            ? $"Accommodation ({night.DiscountPercent}% discount)"
            : "Accommodation";

        var folio = await _dbContext.Folios.FindAsync(night.FolioId);
        if (folio != null)
        {
            folio.Balance += delta;
            folio.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Night {NightId} tariff updated: {OldTariff} -> {NewTariff}", nightId, oldTariff, newTariff);

        night = await _dbContext.StayNights.Include(n => n.Room).FirstAsync(n => n.Id == nightId);
        return MapToDto(night);
    }

    public async Task<int> CloseNightsForStayAsync(Guid stayId, DateTime closedAt)
    {
        var nights = await _dbContext.StayNights
            .Where(n => n.StayId == stayId && n.Status == NightStatus.Active)
            .ToListAsync();

        foreach (var night in nights)
        {
            night.Status = NightStatus.Closed;
            night.ClosedAt = closedAt;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Closed {Count} nights for stay {StayId}", nights.Count, stayId);
        return nights.Count;
    }

    public async Task<int> CloseNightsForRoomAsync(Guid roomId, Guid folioId, DateTime closedAt)
    {
        var nights = await _dbContext.StayNights
            .Where(n => n.RoomId == roomId && n.FolioId == folioId && n.Status == NightStatus.Active)
            .ToListAsync();

        foreach (var night in nights)
        {
            night.Status = NightStatus.Closed;
            night.ClosedAt = closedAt;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Closed {Count} nights for room {RoomId}, folio {FolioId}", nights.Count, roomId, folioId);
        return nights.Count;
    }

    public async Task<int> GenerateNightsForDateAsync(DateTime date)
    {
        var dateOnly = date.Date;
        var activeStays = await _dbContext.Stays
            .Include(s => s.Room)
            .Where(s => !s.IsCheckedOut
                && s.CheckInDate.Date <= dateOnly
                && s.CheckOutDate.Date > dateOnly)
            .ToListAsync();

        var created = 0;
        foreach (var stay in activeStays)
        {
            var existingNight = await _dbContext.StayNights
                .AnyAsync(n => n.StayId == stay.Id && n.Date == dateOnly);

            if (existingNight)
                continue;

            if (!stay.FolioId.HasValue || stay.Room == null)
                continue;

            var tariff = stay.Room.BasePrice ?? 0m;
            if (tariff == 0) continue;

            var nightPrice = tariff;
            if (stay.DiscountPercent > 0)
                nightPrice = nightPrice * (1 - stay.DiscountPercent / 100m);

            var night = new StayNight
            {
                Id = Guid.NewGuid(),
                FolioId = stay.FolioId.Value,
                StayId = stay.Id,
                RoomId = stay.RoomId,
                Date = dateOnly,
                TariffAmount = nightPrice,
                DiscountPercent = stay.DiscountPercent,
                Status = NightStatus.Active,
                IsComp = false,
                Description = stay.DiscountPercent > 0
                    ? $"Accommodation ({stay.DiscountPercent}% discount)"
                    : "Accommodation"
            };

            _dbContext.StayNights.Add(night);

            var folio = await _dbContext.Folios.FindAsync(stay.FolioId.Value);
            if (folio != null)
            {
                folio.Balance += nightPrice;
                folio.UpdatedAt = DateTime.UtcNow;
            }

            created++;
        }

        if (created > 0)
            await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Generated {Count} night charges for date {Date}", created, dateOnly);
        return created;
    }

    public async Task<decimal> CalculateNightsTotalAsync(Guid folioId)
    {
        return await _dbContext.StayNights
            .Where(n => n.FolioId == folioId && n.Status == NightStatus.Active)
            .SumAsync(n => n.TariffAmount);
    }

    private static StayNightDto MapToDto(StayNight night)
    {
        return new StayNightDto(
            night.Id,
            night.FolioId,
            night.StayId,
            night.RoomId,
            night.Room?.RoomNumber ?? "",
            night.Date,
            night.TariffAmount,
            night.DiscountPercent,
            night.Status,
            night.IsComp,
            night.Description,
            night.ClosedAt
        );
    }
}
