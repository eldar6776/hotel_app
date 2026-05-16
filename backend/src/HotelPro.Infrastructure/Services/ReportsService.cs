using HotelPro.Core.Enums;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.Services;

public class ReportsService
{
    private readonly HotelProDbContext _dbContext;

    public ReportsService(HotelProDbContext dbContext) => _dbContext = dbContext;

    public async Task<object> GetDailyReportAsync(DateTime date)
    {
        var d = date.Date;
        var totalRooms = await _dbContext.Rooms.CountAsync(r => r.IsActive);
        var occupied = await _dbContext.Rooms.CountAsync(r => r.Status == RoomStatus.Occupied);
        var occupancyRate = totalRooms > 0 ? Math.Round((double)occupied / totalRooms * 100, 1) : 0;

        var checkedIn = await _dbContext.Bookings.IgnoreQueryFilters()
            .Where(b => b.Status == BookingStatus.CheckedIn && b.ArrivalDate.Date <= d && b.DepartureDate.Date > d)
            .ToListAsync();

        var roomRevenue = checkedIn.Sum(b => (double)b.TotalPrice / Math.Max(1, (b.DepartureDate - b.ArrivalDate).Days));
        var adr = checkedIn.Count > 0 ? Math.Round(checkedIn.Average(b => (double)b.TotalPrice / Math.Max(1, (b.DepartureDate - b.ArrivalDate).Days)), 2) : 0;
        var revPar = totalRooms > 0 ? Math.Round(roomRevenue / totalRooms, 2) : 0;

        var todaysArrivals = await _dbContext.Bookings.IgnoreQueryFilters()
            .CountAsync(b => b.ArrivalDate.Date == d && b.Status != BookingStatus.Cancelled);
        var todaysDepartures = await _dbContext.Bookings.IgnoreQueryFilters()
            .CountAsync(b => b.DepartureDate.Date == d && b.Status == BookingStatus.CheckedIn);

        return new
        {
            Date = d.ToString("yyyy-MM-dd"),
            TotalRooms = totalRooms,
            OccupiedRooms = occupied,
            OccupancyRate = occupancyRate,
            ADR = adr,
            RevPAR = revPar,
            Arrivals = todaysArrivals,
            Departures = todaysDepartures,
            InHouse = checkedIn.Count
        };
    }

    public async Task<object> GetFinancialReportAsync(DateTime from, DateTime to)
    {
        var payments = await _dbContext.Payments
            .Where(p => p.PaymentDate >= from && p.PaymentDate <= to)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { Method = g.Key, Total = g.Sum(p => p.Amount), Count = g.Count() })
            .ToListAsync();

        var invoices = await _dbContext.Invoices
            .Where(i => i.IssueDate >= from && i.IssueDate <= to)
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key.ToString(), Total = g.Sum(i => i.TotalGross), Count = g.Count() })
            .ToListAsync();

        var totalRevenue = payments.Sum(p => p.Total);
        var totalInvoiced = invoices.Sum(i => i.Total);

        return new { From = from, To = to, TotalRevenue = totalRevenue, TotalInvoiced = totalInvoiced, Payments = payments, Invoices = invoices };
    }

    public async Task<object> GetGuestRegistrationBookAsync(DateTime date)
    {
        var guests = await _dbContext.Bookings.IgnoreQueryFilters()
            .Include(b => b.Guest)
            .Where(b => b.Status == BookingStatus.CheckedIn && b.ArrivalDate.Date <= date && b.DepartureDate.Date > date)
            .Select(b => new
            {
                b.Guest!.FirstName,
                b.Guest.LastName,
                b.Guest.DateOfBirth,
                b.Guest.Gender,
                DocumentType = b.Guest.Documents.FirstOrDefault()!.DocumentType.ToString(),
                DocumentNumber = b.Guest.Documents.FirstOrDefault()!.DocumentNumber,
                Nationality = b.Guest.NationalityCountryId.HasValue ? "" : "Unknown",
                b.ArrivalDate,
                b.DepartureDate,
                RoomNumber = b.BookingRooms.FirstOrDefault()!.Room!.RoomNumber
            })
            .ToListAsync();

        return new { Date = date, TotalGuests = guests.Count, Guests = guests };
    }

    public async Task<object> GetRevenueByChannelAsync(DateTime from, DateTime to)
    {
        var data = await _dbContext.Bookings.IgnoreQueryFilters()
            .Where(b => b.CreatedAt >= from && b.CreatedAt <= to && b.Status != BookingStatus.Cancelled)
            .GroupBy(b => b.Source)
            .Select(g => new { Channel = g.Key.ToString(), Bookings = g.Count(), Revenue = g.Sum(b => b.TotalPrice) })
            .ToListAsync();

        return new { From = from, To = to, Channels = data };
    }
}
