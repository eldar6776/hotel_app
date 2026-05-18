using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.Services;

public class RoomOccupancyPolicy : IRoomOccupancyPolicy
{
    private readonly HotelProDbContext _dbContext;

    public RoomOccupancyPolicy(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomStatusDetailDto> GetRoomStatusAsync(Guid roomId, DateTime date)
    {
        var room = await _dbContext.Rooms
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == roomId);

        if (room == null)
        {
            throw new InvalidOperationException($"Room with ID {roomId} not found.");
        }

        var status = await ComputeStatusAsync(room, date.Date);
        return status;
    }

    public async Task<IReadOnlyDictionary<Guid, RoomStatusDetailDto>> GetRoomStatusForAllRoomsAsync(DateTime date)
    {
        var rooms = await _dbContext.Rooms
            .IgnoreQueryFilters()
            .Where(x => x.IsActive)
            .OrderBy(x => x.RoomNumber)
            .ToListAsync();

        var result = new Dictionary<Guid, RoomStatusDetailDto>();
        foreach (var room in rooms)
        {
            result[room.Id] = await ComputeStatusAsync(room, date.Date);
        }

        return result;
    }

    private async Task<RoomStatusDetailDto> ComputeStatusAsync(Room room, DateTime date)
    {
        if (await HasActiveOutOfOrderAsync(room.Id, date))
        {
            return Detail(room, RoomStatus.OutOfOrder, RoomStatus.OutOfOrder, "Active out-of-order entry", false);
        }

        var baseStatus = await ComputeBaseStatusAsync(room.Id, date);

        if (room.Status == RoomStatus.Dirty)
        {
            return Detail(room, RoomStatus.Dirty, baseStatus, "Room clean flag is dirty/not ready", true);
        }

        if (room.Status == RoomStatus.OutOfService)
        {
            return Detail(room, RoomStatus.OutOfService, baseStatus, "Room is manually marked out of service", true);
        }

        return Detail(room, baseStatus, baseStatus, GetReason(baseStatus), false);
    }

    private async Task<bool> HasActiveOutOfOrderAsync(Guid roomId, DateTime date)
    {
        return await _dbContext.RoomOutOfOrders.AnyAsync(x =>
            x.RoomId == roomId &&
            x.Status == "Active" &&
            x.StartDate.Date <= date &&
            (!x.EndDate.HasValue || x.EndDate.Value.Date >= date));
    }

    private async Task<RoomStatus> ComputeBaseStatusAsync(Guid roomId, DateTime date)
    {
        var bookingRooms = await _dbContext.BookingRooms
            .Include(x => x.Booking)
            .Where(x => x.RoomId == roomId)
            .Where(x => x.Booking.ArrivalDate.Date <= date && x.Booking.DepartureDate.Date >= date)
            .Where(x => x.Booking.Status != BookingStatus.Cancelled && x.Booking.Status != BookingStatus.NoShow)
            .ToListAsync();

        var activeStays = bookingRooms
            .Where(x => x.Status == BookingRoomStatus.Occupied || x.Booking.Status == BookingStatus.CheckedIn)
            .ToList();

        var confirmedReservations = bookingRooms
            .Where(x => x.Booking.Status == BookingStatus.Confirmed && x.Status != BookingRoomStatus.Released)
            .ToList();

        var unconfirmedReservations = bookingRooms
            .Where(x => x.Booking.Status == BookingStatus.Pending && x.Status != BookingRoomStatus.Released)
            .ToList();

        if (activeStays.Count > 0 && confirmedReservations.Count > 0)
        {
            return RoomStatus.OccupiedReserved;
        }

        if (activeStays.Count > 0)
        {
            return activeStays.Any(x => x.Booking.DepartureDate.Date <= date)
                ? RoomStatus.Departing
                : RoomStatus.Occupied;
        }

        if (confirmedReservations.Count > 0)
        {
            return RoomStatus.ReservedConfirmed;
        }

        if (unconfirmedReservations.Count > 0)
        {
            return RoomStatus.ReservedUnconfirmed;
        }

        return RoomStatus.Free;
    }

    private static RoomStatusDetailDto Detail(Room room, RoomStatus status, RoomStatus baseStatus, string reason, bool isOverride)
    {
        return new RoomStatusDetailDto(room.Id, room.RoomNumber, status, baseStatus, reason, isOverride);
    }

    private static string GetReason(RoomStatus status)
    {
        return status switch
        {
            RoomStatus.Free => "No active guests or reservations",
            RoomStatus.Occupied => "Active checked-in guest",
            RoomStatus.Departing => "Active guest has checkout today or earlier",
            RoomStatus.ReservedConfirmed => "Confirmed reservation for selected date",
            RoomStatus.OccupiedReserved => "Active guest and confirmed reservation for selected date",
            RoomStatus.ReservedUnconfirmed => "Unconfirmed reservation for selected date",
            RoomStatus.OutOfOrder => "Room is out of order",
            RoomStatus.Dirty => "Room is not ready",
            RoomStatus.OutOfService => "Room is out of service",
            _ => "Computed room status"
        };
    }
}
