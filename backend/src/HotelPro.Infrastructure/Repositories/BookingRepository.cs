using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Interfaces;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly HotelProDbContext _dbContext;

    public BookingRepository(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking?> GetByIdWithRoomsAsync(Guid id)
    {
        return await _dbContext.Bookings
            .Include(b => b.BookingRooms)
            .ThenInclude(br => br.RoomType)
            .Include(b => b.BookingRooms)
            .ThenInclude(br => br.Room)
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Booking>> GetAllAsync(
        List<BookingStatus>? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? guestId = null,
        Guid? roomId = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _dbContext.Bookings
            .Include(b => b.BookingRooms)
            .ThenInclude(br => br.RoomType)
            .Include(b => b.BookingRooms)
            .ThenInclude(br => br.Room)
            .Include(b => b.Guest)
            .AsQueryable();

        query = ApplyFilters(query, status, fromDate, toDate, guestId, roomId);

        return await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(
        List<BookingStatus>? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? guestId = null,
        Guid? roomId = null)
    {
        var query = _dbContext.Bookings.AsQueryable();
        query = ApplyFilters(query, status, fromDate, toDate, guestId, roomId);
        return await query.CountAsync();
    }

    public async Task AddAsync(Booking booking)
    {
        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Booking booking)
    {
        _dbContext.Bookings.Update(booking);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Booking booking)
    {
        _dbContext.Bookings.Remove(booking);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbContext.Bookings.AnyAsync(b => b.Id == id);
    }

    private static IQueryable<Booking> ApplyFilters(
        IQueryable<Booking> query,
        List<BookingStatus>? status,
        DateTime? fromDate,
        DateTime? toDate,
        Guid? guestId,
        Guid? roomId)
    {
        if (status != null && status.Count > 0)
        {
            query = query.Where(b => status.Contains(b.Status));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.ArrivalDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.DepartureDate <= toDate.Value);
        }

        if (guestId.HasValue)
        {
            query = query.Where(b => b.GuestId == guestId.Value);
        }

        if (roomId.HasValue)
        {
            query = query.Where(b => b.BookingRooms.Any(br => br.RoomId == roomId.Value));
        }

        return query;
    }

    public async Task<int> CountConflictingBookingsAsync(
        Guid roomTypeId,
        DateTime arrival,
        DateTime departure,
        Guid? excludeBookingId = null)
    {
        var query = _dbContext.BookingRooms
            .IgnoreQueryFilters()
            .Include(br => br.Booking)
            .Where(br =>
                br.RoomTypeId == roomTypeId &&
                br.Status != BookingRoomStatus.Released &&
                br.Booking.Status != BookingStatus.Cancelled &&
                br.Booking.ArrivalDate < departure &&
                br.Booking.DepartureDate > arrival);

        if (excludeBookingId.HasValue)
        {
            query = query.Where(br => br.BookingId != excludeBookingId.Value);
        }

        return await query.CountAsync();
    }

    public async Task<List<BookingRoom>> GetConflictingBookingsAsync(
        Guid roomTypeId,
        DateTime arrival,
        DateTime departure,
        Guid? excludeBookingId = null)
    {
        var query = _dbContext.BookingRooms
            .IgnoreQueryFilters()
            .Include(br => br.Booking)
            .Where(br =>
                br.RoomTypeId == roomTypeId &&
                br.Status != BookingRoomStatus.Released &&
                br.Booking.Status != BookingStatus.Cancelled &&
                br.Booking.ArrivalDate < departure &&
                br.Booking.DepartureDate > arrival);

        if (excludeBookingId.HasValue)
        {
            query = query.Where(br => br.BookingId != excludeBookingId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task AcquireRoomTypeLockAsync(
        Guid roomTypeId,
        DateTime arrival,
        DateTime departure,
        CancellationToken ct = default)
    {
        var sql = """
            SELECT br."Id"
            FROM "booking_rooms" br
            INNER JOIN "bookings" b ON br."BookingId" = b."Id"
            WHERE br."RoomTypeId" = {0}
              AND br."Status" NOT IN ('Released')
              AND b."Status" != 'Cancelled'
              AND b."ArrivalDate" < {1}
              AND b."DepartureDate" > {2}
            FOR UPDATE NOWAIT
            """;

        await _dbContext.Database.ExecuteSqlRawAsync(sql, roomTypeId, departure, arrival, ct);
    }

    public async Task AddBookingInTransactionAsync(Booking booking, CancellationToken ct = default)
    {
        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync(ct);
    }
}
