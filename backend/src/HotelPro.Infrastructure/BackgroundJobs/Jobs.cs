using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            .Where(b => b.Status == "Confirmed" && b.ArrivalDate < cutoff)
            .ToListAsync(ct);

        foreach (var booking in noShows)
        {
            booking.Status = "NoShow";
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
            .Where(b => b.Status == "CheckedIn" && b.DepartureDate >= today)
            .ToListAsync(ct);

        logger.LogInformation("[{JobName}] Processing {Count} active bookings for night audit", JobName, checkedInBookings.Count);
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

        var totalRooms = await db.Rooms.CountAsync(ct);
        var occupiedRooms = await db.Rooms.CountAsync(r => r.Status == HotelPro.Core.Enums.RoomStatus.Occupied, ct);
        var occupancyRate = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0;
    }
}

public class BackupTriggerJob : ScheduledJobBase
{
    protected override string JobName => "BackupTrigger";
    protected override TimeSpan Interval => TimeSpan.FromDays(1);

    public BackupTriggerJob(ILogger<BackupTriggerJob> logger) : base(logger) { }

    protected override Task ExecuteJobAsync(CancellationToken ct)
    {
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
    }
}
