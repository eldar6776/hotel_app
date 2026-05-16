using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
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

public class NightAuditJob : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NightAuditJob> _logger;
    private Timer? _timer;

    public NightAuditJob(ILogger<NightAuditJob> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("[NightAudit] Starting night audit job");

        var now = DateTime.UtcNow;
        var nextMidnight = now.Date.AddDays(1);
        var delay = nextMidnight - now;

        _timer = new Timer(async _ => await ExecuteSafeAsync(ct), null, delay, TimeSpan.FromDays(1));

        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
            await RunCatchUpAsync(ct);
        }, ct);

        return Task.CompletedTask;
    }

    private async Task RunCatchUpAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HotelProDbContext>();
            var auditService = scope.ServiceProvider.GetRequiredService<INightAuditService>();

            var lastAudit = await db.Set<NightAuditLog>()
                .OrderByDescending(n => n.AuditDate)
                .Select(n => n.AuditDate)
                .FirstOrDefaultAsync(ct);

            if (lastAudit == default)
            {
                lastAudit = DateTime.UtcNow.Date.AddDays(-1);
            }

            var currentDate = DateTime.UtcNow.Date;

            while (lastAudit < currentDate)
            {
                lastAudit = lastAudit.AddDays(1);
                _logger.LogInformation("[NightAudit] Catch-up: running audit for {Date}", lastAudit);
                await auditService.RunAuditAsync(lastAudit);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NightAudit] Catch-up failed");
        }
    }

    private async Task ExecuteSafeAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var auditService = scope.ServiceProvider.GetRequiredService<INightAuditService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<NightAuditJob>>();

            var today = DateTime.UtcNow.Date;
            var result = await auditService.RunAuditAsync(today);

            if (result.Success)
            {
                logger.LogInformation("[NightAudit] Completed: {Processed} bookings, {Charges} charges, {NoShows} no-shows",
                    result.BookingsProcessed, result.TotalStayCharges, result.NoShowsDetected);
            }
            else
            {
                logger.LogWarning("[NightAudit] Failed or skipped: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NightAudit] Error");
        }
    }

    public Task StopAsync(CancellationToken ct)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
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
