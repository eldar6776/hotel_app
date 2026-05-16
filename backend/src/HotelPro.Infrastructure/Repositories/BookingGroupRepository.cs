using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Interfaces;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.Repositories;

public class BookingGroupRepository : IBookingGroupRepository
{
    private readonly HotelProDbContext _dbContext;

    public BookingGroupRepository(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BookingGroup?> GetByIdAsync(Guid id)
    {
        return await _dbContext.BookingGroups
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<BookingGroup?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbContext.BookingGroups
            .IgnoreQueryFilters()
            .Include(g => g.ContactPerson)
            .Include(g => g.GroupBookings)
            .ThenInclude(gb => gb.Booking)
            .ThenInclude(b => b.Guest)
            .Include(g => g.GroupBookings)
            .ThenInclude(gb => gb.RoomType)
            .Include(g => g.MasterBill)
            .ThenInclude(mb => mb.PayerGuest)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<PagedResult<BookingGroupDto>> GetAllAsync(BookingGroupFilter filter)
    {
        var query = _dbContext.BookingGroups
            .IgnoreQueryFilters()
            .Include(g => g.ContactPerson)
            .Include(g => g.GroupBookings)
            .ThenInclude(gb => gb.RoomType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            if (Enum.TryParse<GroupStatus>(filter.Status, true, out var status))
            {
                query = query.Where(g => g.Status == status);
            }
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(g => g.Arrival >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(g => g.Departure <= filter.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<BookingGroupDto>(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task AddAsync(BookingGroup group)
    {
        _dbContext.BookingGroups.Add(group);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(BookingGroup group)
    {
        _dbContext.BookingGroups.Update(group);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ReleaseGroupAsync(Guid groupId)
    {
        var groupBookings = await _dbContext.GroupBookings
            .Include(gb => gb.Booking)
            .Where(gb => gb.GroupId == groupId)
            .ToListAsync();

        var bookingIds = new List<Guid>();
        foreach (var gb in groupBookings)
        {
            if (gb.Booking.Status == BookingStatus.Pending)
            {
                foreach (var br in gb.Booking.BookingRooms)
                {
                    if (br.Status == BookingRoomStatus.Blocked)
                    {
                        br.Status = BookingRoomStatus.Released;
                    }
                }
                gb.Booking.Status = BookingStatus.Cancelled;
                gb.Booking.CancellationReason = "Group release date expired";
                gb.Booking.CancelledAt = DateTime.UtcNow;
                bookingIds.Add(gb.Booking.Id);
            }
        }

        await _dbContext.SaveChangesAsync();

        var group = await _dbContext.BookingGroups.FindAsync(groupId);
        if (group != null)
        {
            group.Status = GroupStatus.Released;
            group.ConfirmedRoomCount = 0;
            group.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<int> GetActiveGroupsWithReleaseDateBeforeAsync(DateTime date)
    {
        return await _dbContext.BookingGroups
            .CountAsync(g => g.Status == GroupStatus.Active
                && g.ReleaseDate.HasValue
                && g.ReleaseDate.Value <= date);
    }

    public async Task<List<BookingGroup>> GetGroupsPendingReleaseAsync(DateTime date)
    {
        return await _dbContext.BookingGroups
            .IgnoreQueryFilters()
            .Where(g => g.Status == GroupStatus.Active
                && g.ReleaseDate.HasValue
                && g.ReleaseDate.Value <= date)
            .ToListAsync();
    }

    private static BookingGroupDto MapToDto(BookingGroup g)
    {
        var bookings = g.GroupBookings.Select(gb => new BookingGroupBookingDto(
            gb.Booking.Id,
            $"{gb.Booking.Guest?.FirstName} {gb.Booking.Guest?.LastName}".Trim(),
            gb.RoomTypeId,
            gb.RoomType?.Name ?? "",
            gb.Booking.Status.ToString(),
            gb.Booking.ArrivalDate,
            gb.Booking.DepartureDate
        )).ToList();

        MasterBillDto? masterBill = null;
        if (g.MasterBill != null)
        {
            masterBill = new MasterBillDto(
                g.MasterBill.Id,
                g.MasterBill.GroupId,
                g.MasterBill.PayerGuestId,
                $"{g.MasterBill.PayerGuest?.FirstName} {g.MasterBill.PayerGuest?.LastName}".Trim(),
                g.MasterBill.TotalStayCharges,
                g.MasterBill.IsClosed,
                g.MasterBill.CreatedAt,
                g.MasterBill.UpdatedAt
            );
        }

        return new BookingGroupDto(
            g.Id,
            g.HotelId,
            g.Name,
            g.ContactPersonId,
            $"{g.ContactPerson?.FirstName} {g.ContactPerson?.LastName}".Trim(),
            g.Arrival,
            g.Departure,
            g.BlockedRoomCount,
            g.ConfirmedRoomCount,
            g.RatePlanId,
            null,
            g.DiscountPercent,
            g.ReleaseDate,
            g.Status.ToString(),
            g.UseMasterBill,
            g.CreatedAt,
            g.UpdatedAt,
            bookings,
            masterBill
        );
    }
}
