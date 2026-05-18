using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.Services;

public class NightAuditService : INightAuditService
{
    private readonly HotelProDbContext _dbContext;
    private readonly INightLedgerService _nightLedger;
    private readonly ILogger<NightAuditService> _logger;

    public NightAuditService(
        HotelProDbContext dbContext,
        INightLedgerService nightLedger,
        ILogger<NightAuditService> logger)
    {
        _dbContext = dbContext;
        _nightLedger = nightLedger;
        _logger = logger;
    }

    public async Task<NightAuditResult> RunAuditAsync(DateTime auditDate)
    {
        var auditDateOnly = auditDate.Date;

        var existingAudit = await _dbContext.Set<NightAuditLog>()
            .FirstOrDefaultAsync(a => a.AuditDate == auditDateOnly);

        if (existingAudit != null)
        {
            _logger.LogInformation("Night audit already ran for {AuditDate}", auditDateOnly);
            return new NightAuditResult(false, 0, 0, 0, "Audit already completed for this date");
        }

        var log = new NightAuditLog
        {
            Id = Guid.NewGuid(),
            AuditDate = auditDateOnly,
            RanAt = DateTime.UtcNow,
            Status = NightAuditStatus.Success,
            BookingsProcessed = 0,
            TotalStayCharges = 0,
        };

        try
        {
            var checkedInBookings = await _dbContext.Bookings
                .IgnoreQueryFilters()
                .Include(b => b.BookingRooms)
                .Where(b => b.Status == BookingStatus.CheckedIn && b.ArrivalDate.Date <= auditDateOnly)
                .ToListAsync();

            var totalCharges = 0m;
            var processed = 0;

            var nightsGenerated = await _nightLedger.GenerateNightsForDateAsync(auditDateOnly);
            _logger.LogInformation("Night ledger generated {Count} nights for {Date}", nightsGenerated, auditDateOnly);

            if (nightsGenerated == 0)
            {
                foreach (var booking in checkedInBookings)
                {
                    var bookingNights = booking.BookingRooms.Where(br => br.Status == BookingRoomStatus.Occupied).ToList();
                    if (bookingNights.Count == 0) continue;

                    foreach (var br in bookingNights)
                    {
                        var folio = await _dbContext.Folios
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(f => f.BookingId == booking.Id && f.Status == FolioStatus.Open);

                        if (folio == null) continue;

                        var existingNight = await _dbContext.StayNights
                            .AnyAsync(n => n.FolioId == folio.Id && n.Date == auditDateOnly);

                        if (existingNight) continue;

                        _dbContext.StayNights.Add(new StayNight
                        {
                            Id = Guid.NewGuid(),
                            FolioId = folio.Id,
                            Date = auditDateOnly,
                            TariffAmount = br.PricePerNight,
                            DiscountPercent = 0,
                            Status = NightStatus.Active,
                            IsComp = false,
                        });

                        var charge = new Charge
                        {
                            Id = Guid.NewGuid(),
                            FolioId = folio.Id,
                            ChargeType = ChargeType.StayNight,
                            Description = $"Noćenje - {auditDateOnly:dd.MM.yyyy.}",
                            Quantity = 1,
                            UnitPrice = br.PricePerNight,
                            TotalPrice = br.PricePerNight,
                            VatAmount = 0,
                            ChargeDate = auditDateOnly,
                            IsTaxable = true
                        };

                        _dbContext.Charges.Add(charge);

                        folio.Balance += br.PricePerNight;
                        folio.UpdatedAt = DateTime.UtcNow;

                        totalCharges += br.PricePerNight;
                    }

                    processed++;
                }
            }
            else
            {
                foreach (var booking in checkedInBookings)
                {
                    var bookingNights = booking.BookingRooms.Where(br => br.Status == BookingRoomStatus.Occupied).ToList();
                    if (bookingNights.Count > 0) processed++;
                    totalCharges += bookingNights.Sum(br => br.PricePerNight);
                }
            }

            var confirmedBookings = await _dbContext.Bookings
                .IgnoreQueryFilters()
                .Where(b => b.Status == BookingStatus.Confirmed && b.ArrivalDate.Date < auditDateOnly)
                .ToListAsync();

            var noShows = 0;
            foreach (var booking in confirmedBookings)
            {
                booking.Status = BookingStatus.NoShow;
                booking.UpdatedAt = DateTime.UtcNow;
                noShows++;
            }

            log.BookingsProcessed = processed;
            log.TotalStayCharges = totalCharges;
            log.Status = NightAuditStatus.Success;

            var existingLock = await _dbContext.Set<DayLock>()
                .FirstOrDefaultAsync(d => d.LockedDate == auditDateOnly);

            if (existingLock == null)
            {
                var dayLock = new DayLock
                {
                    Id = Guid.NewGuid(),
                    LockedDate = auditDateOnly,
                    LockedAt = DateTime.UtcNow,
                };
                _dbContext.Set<DayLock>().Add(dayLock);
            }

            _dbContext.Set<NightAuditLog>().Add(log);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Night audit completed for {AuditDate}: {Processed} bookings, {Charges} charges, {NoShows} no-shows",
                auditDateOnly, processed, totalCharges, noShows);

            return new NightAuditResult(true, processed, totalCharges, noShows, null);
        }
        catch (Exception ex)
        {
            log.Status = NightAuditStatus.Failed;
            log.ErrorMessage = ex.Message;
            _dbContext.Set<NightAuditLog>().Add(log);
            await _dbContext.SaveChangesAsync();

            _logger.LogError(ex, "Night audit failed for {AuditDate}", auditDateOnly);
            return new NightAuditResult(false, 0, 0, 0, ex.Message);
        }
    }
}
