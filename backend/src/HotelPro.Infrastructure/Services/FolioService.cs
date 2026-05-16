using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.Services;

public class FolioService : IFolioService
{
    private readonly HotelProDbContext _dbContext;

    public FolioService(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FolioDto> CreateFolioAsync(CreateFolioDto dto)
    {
        var folio = new Folio
        {
            Id = Guid.NewGuid(),
            FolioNumber = $"F-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            BookingId = dto.BookingId,
            GuestId = dto.GuestId,
            Status = FolioStatus.Open,
            Balance = 0,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Folios.Add(folio);
        await _dbContext.SaveChangesAsync();

        return await MapToDtoAsync(folio);
    }

    public async Task<FolioDto> CreateSubFolioAsync(CreateSubFolioDto dto)
    {
        var existingFolios = await _dbContext.Folios
            .CountAsync(f => f.BookingId == dto.BookingId);

        var folio = new Folio
        {
            Id = Guid.NewGuid(),
            FolioNumber = $"F-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}-SUB{existingFolios + 1}",
            BookingId = dto.BookingId,
            GuestId = dto.GuestId,
            Status = FolioStatus.Open,
            Balance = 0,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Folios.Add(folio);
        await _dbContext.SaveChangesAsync();

        return await MapToDtoAsync(folio);
    }

    public async Task<FolioDto?> GetFolioByBookingAsync(Guid bookingId)
    {
        var folio = await _dbContext.Folios
            .IgnoreQueryFilters()
            .Include(f => f.Guest)
            .Include(f => f.Charges)
            .Include(f => f.StayNights)
            .FirstOrDefaultAsync(f => f.BookingId == bookingId && f.Status != FolioStatus.Closed);

        return folio != null ? await MapToDtoAsync(folio) : null;
    }

    public async Task<List<FolioDto>> GetFoliosByBookingAsync(Guid bookingId)
    {
        var folios = await _dbContext.Folios
            .IgnoreQueryFilters()
            .Include(f => f.Guest)
            .Include(f => f.Charges)
            .Include(f => f.StayNights)
            .Where(f => f.BookingId == bookingId)
            .ToListAsync();

        var dtos = new List<FolioDto>();
        foreach (var f in folios)
        {
            dtos.Add(await MapToDtoAsync(f));
        }
        return dtos;
    }

    public async Task<FolioChargeDto> AddChargeAsync(Guid folioId, CreateFolioChargeDto dto)
    {
        var folio = await _dbContext.Folios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == folioId);

        if (folio == null)
            throw new InvalidOperationException($"Folio {folioId} not found.");

        if (folio.Status == FolioStatus.Closed)
            throw new InvalidOperationException("Cannot add charge to a closed folio.");

        if (!Enum.TryParse<ChargeType>(dto.ChargeType, true, out var chargeType))
            throw new InvalidOperationException($"Invalid charge type: {dto.ChargeType}");

        var charge = new Charge
        {
            Id = Guid.NewGuid(),
            FolioId = folioId,
            ChargeType = chargeType,
            Description = dto.Description,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            TotalPrice = dto.Quantity * dto.UnitPrice,
            VatAmount = 0,
            ChargeDate = dto.ChargeDate,
            POSReference = dto.POSReference,
            IsTaxable = true
        };

        _dbContext.Charges.Add(charge);

        folio.Balance += charge.TotalPrice;
        folio.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapChargeToDto(charge);
    }

    public async Task DeleteChargeAsync(Guid chargeId)
    {
        var charge = await _dbContext.Charges
            .Include(c => c.Folio)
            .FirstOrDefaultAsync(c => c.Id == chargeId);

        if (charge == null)
            throw new InvalidOperationException($"Charge {chargeId} not found.");

        if (charge.Folio.Status == FolioStatus.Closed)
            throw new InvalidOperationException("Cannot delete charge from a closed folio. Use storno instead.");

        charge.Folio.Balance -= charge.TotalPrice;
        charge.Folio.UpdatedAt = DateTime.UtcNow;

        _dbContext.Charges.Remove(charge);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<FolioChargeDto> StornoChargeAsync(Guid chargeId, string reason)
    {
        var charge = await _dbContext.Charges
            .Include(c => c.Folio)
            .FirstOrDefaultAsync(c => c.Id == chargeId);

        if (charge == null)
            throw new InvalidOperationException($"Charge {chargeId} not found.");

        var storno = new Charge
        {
            Id = Guid.NewGuid(),
            FolioId = charge.FolioId,
            ChargeType = charge.ChargeType,
            Description = $"STORNO: {charge.Description} — {reason}",
            Quantity = charge.Quantity,
            UnitPrice = -charge.UnitPrice,
            TotalPrice = -charge.TotalPrice,
            VatAmount = -charge.VatAmount,
            ChargeDate = DateTime.UtcNow,
            IsTaxable = charge.IsTaxable
        };

        _dbContext.Charges.Add(storno);

        charge.Folio.Balance += storno.TotalPrice;
        charge.Folio.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapChargeToDto(storno);
    }

    public async Task CloseFolioAsync(Guid folioId)
    {
        var folio = await _dbContext.Folios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == folioId);

        if (folio == null)
            throw new InvalidOperationException($"Folio {folioId} not found.");

        folio.Status = FolioStatus.Closed;
        folio.ClosedAt = DateTime.UtcNow;
        folio.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<decimal> GetFolioBalanceAsync(Guid folioId)
    {
        var folio = await _dbContext.Folios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == folioId);

        return folio?.Balance ?? 0;
    }

    private async Task<FolioDto> MapToDtoAsync(Folio f)
    {
        var guestName = "";
        if (f.GuestId.HasValue)
        {
            var guest = await _dbContext.Guests.FindAsync(f.GuestId.Value);
            if (guest != null)
                guestName = $"{guest.FirstName} {guest.LastName}".Trim();
        }

        var charges = f.Charges.Select(MapChargeToDto).ToList();
        var stayNights = f.StayNights.Select(sn => new FolioStayNightDto(
            sn.Id,
            sn.FolioId,
            sn.Date,
            sn.RoomPrice,
            sn.IsComp,
            sn.Notes
        )).ToList();

        return new FolioDto(
            f.Id,
            f.FolioNumber,
            f.BookingId,
            f.GuestId,
            guestName,
            f.Status.ToString(),
            f.Balance,
            f.CreatedAt,
            f.ClosedAt,
            f.Notes,
            charges,
            stayNights
        );
    }

    private static FolioChargeDto MapChargeToDto(Charge c)
    {
        return new FolioChargeDto(
            c.Id,
            c.FolioId,
            c.ChargeType.ToString(),
            c.Description,
            c.Quantity,
            c.UnitPrice,
            c.TotalPrice,
            c.ChargeDate,
            c.POSReference
        );
    }
}
