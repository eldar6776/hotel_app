using Asp.Versioning;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly HotelProDbContext _db;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(HotelProDbContext db, ILogger<DashboardController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("kpi")]
    public async Task<ActionResult> GetKpi()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var totalRooms = await _db.Rooms.CountAsync(r => r.IsActive);
        var freeRooms = await _db.Rooms.CountAsync(r => r.IsActive && r.Status == RoomStatus.Free);
        var occupiedRooms = await _db.Rooms.CountAsync(r => r.IsActive && r.Status == RoomStatus.Occupied);
        var dirtyRooms = await _db.Rooms.CountAsync(r => r.IsActive && r.Status == RoomStatus.Dirty);
        var oooRooms = await _db.Rooms.CountAsync(r => r.IsActive && r.Status == RoomStatus.OutOfOrder);

        var todayCheckIns = await _db.Bookings.CountAsync(b =>
            b.Status == BookingStatus.Confirmed &&
            b.ArrivalDate.Date == today);

        var todayCheckOuts = await _db.Bookings.CountAsync(b =>
            b.Status == BookingStatus.CheckedIn &&
            b.DepartureDate.Date <= today);

        var openFolios = await _db.Folios.CountAsync(f => f.Status == FolioStatus.Open);

        var occupancyPercent = totalRooms > 0 ? Math.Round((double)occupiedRooms / totalRooms * 100, 1) : 0;

        var todayRevenue = await _db.Bookings
            .Where(b => b.ArrivalDate.Date <= today && b.DepartureDate.Date > today)
            .SumAsync(b => b.TotalPrice);

        var adr = occupiedRooms > 0 ? Math.Round((double)todayRevenue / occupiedRooms, 2) : 0;
        var revpar = totalRooms > 0 ? Math.Round((double)todayRevenue / totalRooms, 2) : 0;

        return Ok(new
        {
            occupancyPercent,
            adr,
            revpar,
            todayCheckIns,
            todayCheckOuts,
            openFolios,
            freeRooms,
            totalRooms,
            occupiedRooms,
            dirtyRooms,
            oooRooms
        });
    }

    [HttpGet("occupancy-trend")]
    public async Task<ActionResult> GetOccupancyTrend([FromQuery] int days = 30)
    {
        if (days < 1) days = 1;
        if (days > 365) days = 365;

        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-days + 1);

        var totalRooms = await _db.Rooms.CountAsync(r => r.IsActive);

        var bookings = await _db.Bookings
            .Where(b => b.ArrivalDate < today.AddDays(1) && b.DepartureDate > startDate.AddDays(-1))
            .Select(b => new { b.ArrivalDate, b.DepartureDate, b.TotalPrice })
            .ToListAsync();

        var trend = new List<object>();

        for (var date = startDate; date <= today; date = date.AddDays(1))
        {
            var occupiedOnDate = bookings.Count(b =>
                b.ArrivalDate.Date <= date && b.DepartureDate.Date > date);

            var revenueOnDate = bookings
                .Where(b => b.ArrivalDate.Date <= date && b.DepartureDate.Date > date)
                .Sum(b => (double)(b.TotalPrice / Math.Max(1, (b.DepartureDate - b.ArrivalDate).Days)));

            var occupancy = totalRooms > 0
                ? Math.Round((double)occupiedOnDate / totalRooms * 100, 1)
                : 0;

            trend.Add(new
            {
                date = date.ToString("yyyy-MM-dd"),
                occupancy,
                revenue = Math.Round(revenueOnDate, 2)
            });
        }

        return Ok(trend);
    }

    [HttpGet("recent-bookings")]
    public async Task<ActionResult> GetRecentBookings([FromQuery] int limit = 10)
    {
        if (limit < 1) limit = 1;
        if (limit > 50) limit = 50;

        var bookings = await _db.Bookings
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .OrderByDescending(b => b.CreatedAt)
            .Take(limit)
            .ToListAsync();

        var result = bookings.Select(b => new
        {
            id = b.Id.ToString(),
            bookingNumber = $"BK-{b.Id.ToString()[..8].ToUpper()}",
            guestName = $"{b.Guest.FirstName} {b.Guest.LastName}",
            roomNumber = b.BookingRooms.FirstOrDefault(br => br.Room != null)?.Room?.RoomNumber ?? "N/A",
            arrivalDate = b.ArrivalDate.ToString("yyyy-MM-dd"),
            departureDate = b.DepartureDate.ToString("yyyy-MM-dd"),
            status = b.Status.ToString(),
            amount = b.TotalPrice
        });

        return Ok(result);
    }

    [HttpGet("upcoming-checkins")]
    public async Task<ActionResult> GetUpcomingCheckIns([FromQuery] int days = 7)
    {
        if (days < 1) days = 1;
        if (days > 30) days = 30;

        var today = DateTime.UtcNow.Date;
        var endDate = today.AddDays(days);

        var bookings = await _db.Bookings
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .Where(b => b.Status == BookingStatus.Confirmed &&
                        b.ArrivalDate.Date >= today &&
                        b.ArrivalDate.Date < endDate)
            .OrderBy(b => b.ArrivalDate)
            .ToListAsync();

        var result = bookings.Select(b => new
        {
            id = b.Id.ToString(),
            guestName = $"{b.Guest.FirstName} {b.Guest.LastName}",
            roomNumber = b.BookingRooms.FirstOrDefault(br => br.Room != null)?.Room?.RoomNumber ?? "N/A",
            arrivalDate = b.ArrivalDate.ToString("yyyy-MM-dd"),
            nights = (b.DepartureDate - b.ArrivalDate).Days
        });

        return Ok(result);
    }
}
