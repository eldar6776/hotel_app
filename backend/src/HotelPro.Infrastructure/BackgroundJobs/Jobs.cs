using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HotelPro.Core.Enums;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.BackgroundJobs;

public class NoShowDetectionJob : ScheduledJobBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    protected override string JobName => "NoShowDetection";
    protected override TimeSpan Interval => TimeSpan.FromHours(1);

    public NoShowDetectionJob(ILogger<NoShowDetectionJob> logger, IServiceScopeFactory scopeFactory) : base(logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteJobAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HotelProDbContext>();

        var cutoff = DateTime.UtcNow.Date.AddHours(6);
        var noShows = await db.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed && b.ArrivalDate < cutoff)
            .ToListAsync(ct);

        foreach (var booking in noShows)
        {
            booking.Status = BookingStatus.NoShow;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.CancellationReason = "Auto-detected no-show";
        }

        if (noShows.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}

public class NightAuditJob : ScheduledJobBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    protected override string JobName => "NightAudit";
    protected override TimeSpan Interval => TimeSpan.FromDays(1);

    public NightAuditJob(ILogger<NightAuditJob> logger, IServiceScopeFactory scopeFactory) : base(logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteJobAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HotelProDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<NightAuditJob>>();

        var today = DateTime.UtcNow.Date;
        var checkedInBookings = await db.Bookings
            .Include(b => b.BookingRooms)
            .Where(b => b.Status == BookingStatus.CheckedIn && b.DepartureDate > today)
            .ToListAsync(ct);

        logger.LogInformation("[{JobName}] Processing {Count} active bookings for night audit", JobName, checkedInBookings.Count);

        int stayNightsCreated = 0;
        int chargesCreated = 0;

        foreach (var booking in checkedInBookings)
        {
            var folio = await db.Folios
                .FirstOrDefaultAsync(f => f.BookingId == booking.Id && f.Status == FolioStatus.Open, ct);

            if (folio == null) continue;

            var existingStayNight = await db.StayNights
                .AnyAsync(s => s.FolioId == folio.Id && s.Date == today, ct);

            if (existingStayNight) continue;

            var isComplementary = booking.BookingTypeId.HasValue
                && await db.BookingTypes.AnyAsync(bt => bt.Id == booking.BookingTypeId && bt.Code == "COMP", ct);

            foreach (var bookingRoom in booking.BookingRooms)
            {
                var stayNight = new HotelPro.Core.Entities.StayNight
                {
                    Id = Guid.NewGuid(),
                    FolioId = folio.Id,
                    Date = today,
                    RoomPrice = bookingRoom.PricePerNight,
                    IsComp = isComplementary,
                    Notes = $"Night audit auto-charge for room {bookingRoom.RoomId}"
                };
                db.StayNights.Add(stayNight);
                stayNightsCreated++;

                if (!stayNight.IsComp)
                {
                    var charge = new HotelPro.Core.Entities.Charge
                    {
                        Id = Guid.NewGuid(),
                        FolioId = folio.Id,
                        Description = $"Night audit - room charge ({today:yyyy-MM-dd})",
                        Quantity = 1,
                        UnitPrice = bookingRoom.PricePerNight,
                        TotalPrice = bookingRoom.PricePerNight,
                        VatAmount = 0,
                        ChargeDate = today,
                        IsTaxable = true
                    };
                    db.Charges.Add(charge);
                    chargesCreated++;

                    folio.Balance += bookingRoom.PricePerNight;
                }
            }
        }

        if (stayNightsCreated > 0 || chargesCreated > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("[{JobName}] Created {StayNights} stay night records and {Charges} charges",
                JobName, stayNightsCreated, chargesCreated);
        }
    }
}

public class DailyReportJob : ScheduledJobBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    protected override string JobName => "DailyReport";
    protected override TimeSpan Interval => TimeSpan.FromDays(1);

    public DailyReportJob(ILogger<DailyReportJob> logger, IServiceScopeFactory scopeFactory) : base(logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteJobAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HotelProDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DailyReportJob>>();

        var totalRooms = await db.Rooms.CountAsync(ct);
        var occupiedRooms = await db.Rooms.CountAsync(r => r.Status == RoomStatus.Occupied, ct);
        var occupancyRate = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0;

        logger.LogInformation("[{JobName}] Daily report: {OccupancyRate:F1}% occupancy ({Occupied}/{Total} rooms)",
            JobName, occupancyRate, occupiedRooms, totalRooms);
    }
}

public class BackupTriggerJob : ScheduledJobBase
{
    protected override string JobName => "BackupTrigger";
    protected override TimeSpan Interval => TimeSpan.FromDays(1);

    public BackupTriggerJob(ILogger<BackupTriggerJob> logger) : base(logger) { }

    protected override Task ExecuteJobAsync(CancellationToken ct)
    {
        // Backup is handled by Docker service (prodrigestivill/postgres-backup-local)
        // This job serves as a placeholder for future custom backup logic
        return Task.CompletedTask;
    }
}

public class IoTDeviceCheckJob : ScheduledJobBase
{
    protected override string JobName => "IoTDeviceCheck";
    protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

    public IoTDeviceCheckJob(ILogger<IoTDeviceCheckJob> logger) : base(logger) { }

    protected override Task ExecuteJobAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}

public class DndExpiryJob : ScheduledJobBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    protected override string JobName => "DndExpiry";
    protected override TimeSpan Interval => TimeSpan.FromMinutes(15);

    public DndExpiryJob(ILogger<DndExpiryJob> logger, IServiceScopeFactory scopeFactory) : base(logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteJobAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HotelProDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DndExpiryJob>>();

        var expiredDnd = DateTime.UtcNow.AddHours(-24);
        var dndRooms = await db.Rooms
            .Where(r => r.Status == RoomStatus.OutOfOrder && r.Notes != null && r.Notes.Contains("DND"))
            .ToListAsync(ct);

        foreach (var room in dndRooms)
        {
            room.Status = RoomStatus.Dirty;
            room.Notes = room.Notes?.Replace("DND", "").Trim();
            logger.LogInformation("[{JobName}] Room {RoomNumber} DND expired, status changed to Dirty", JobName, room.RoomNumber);
        }

        if (dndRooms.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}

public class SessionCleanupJob : ScheduledJobBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    protected override string JobName => "SessionCleanup";
    protected override TimeSpan Interval => TimeSpan.FromHours(1);

    public SessionCleanupJob(ILogger<SessionCleanupJob> logger, IServiceScopeFactory scopeFactory) : base(logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteJobAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HotelProDbContext>();

        var cutoff = DateTime.UtcNow.AddDays(-30);
        var oldLogs = await db.AccessLogs.Where(a => a.Timestamp < cutoff).ToListAsync(ct);
        if (oldLogs.Count > 0)
        {
            db.AccessLogs.RemoveRange(oldLogs);
            await db.SaveChangesAsync(ct);
        }

        var expiredTokens = await db.RefreshTokens
            .Where(rt => rt.ExpiresAt < cutoff || (rt.IsRevoked && rt.CreatedAt < cutoff))
            .ToListAsync(ct);
        if (expiredTokens.Count > 0)
        {
            db.RefreshTokens.RemoveRange(expiredTokens);
            await db.SaveChangesAsync(ct);
        }
    }
}
