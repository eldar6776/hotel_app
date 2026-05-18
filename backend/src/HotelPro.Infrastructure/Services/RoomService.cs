using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.Services;

public class RoomService : IRoomService
{
    private readonly HotelProDbContext _dbContext;
    private readonly IRoomStatusBroadcaster _broadcaster;
    private readonly IRoomOccupancyPolicy _occupancyPolicy;

    public RoomService(
        HotelProDbContext dbContext,
        IRoomStatusBroadcaster broadcaster,
        IRoomOccupancyPolicy occupancyPolicy)
    {
        _dbContext = dbContext;
        _broadcaster = broadcaster;
        _occupancyPolicy = occupancyPolicy;
    }

    public async Task<PagedResult<RoomDto>> GetRoomsAsync(RoomFilter filter)
    {
        var query = _dbContext.Rooms
            .Include(r => r.Building)
            .Include(r => r.RoomType)
            .AsQueryable();

        if (filter.IncludeInactive)
        {
            query = query.IgnoreQueryFilters();
        }

        if (filter.BuildingId.HasValue)
        {
            query = query.Where(r => r.BuildingId == filter.BuildingId.Value);
        }

        if (filter.RoomTypeId.HasValue)
        {
            query = query.Where(r => r.RoomTypeId == filter.RoomTypeId.Value);
        }

        if (filter.Floor.HasValue)
        {
            query = query.Where(r => r.Floor == filter.Floor.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(r => r.RoomNumber.Contains(filter.Search));
        }

        var rooms = await query
            .OrderBy(r => r.Building.Name)
            .ThenBy(r => r.Floor)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();

        var statusMap = await _occupancyPolicy.GetRoomStatusForAllRoomsAsync(DateTime.UtcNow);
        var mapped = rooms
            .Select(r => MapToDto(r, statusMap.TryGetValue(r.Id, out var status) ? status.Status : r.Status))
            .ToList();

        if (filter.Status != null && filter.Status.Count > 0)
        {
            mapped = mapped
                .Where(r => Enum.TryParse<RoomStatus>(r.Status, out var parsed) && filter.Status.Contains(parsed))
                .ToList();
        }

        var totalCount = mapped.Count;
        var items = mapped
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return new PagedResult<RoomDto>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<RoomDto?> GetRoomByIdAsync(Guid id)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.Building)
            .Include(r => r.RoomType)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return null;

        var status = await _occupancyPolicy.GetRoomStatusAsync(room.Id, DateTime.UtcNow);
        return MapToDto(room, status.Status);
    }

    public async Task<RoomStatusDetailDto?> GetRoomStatusAsync(Guid id, DateTime? date = null)
    {
        var exists = await _dbContext.Rooms
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id == id);

        if (!exists) return null;

        return await _occupancyPolicy.GetRoomStatusAsync(id, date ?? DateTime.UtcNow);
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomDto dto)
    {
        if (dto.Floor < 0)
            throw new InvalidOperationException("Floor cannot be negative.");

        if (dto.BasePrice.HasValue && dto.BasePrice.Value <= 0)
            throw new InvalidOperationException("Base price must be greater than zero.");

        var buildingExists = await _dbContext.Buildings.AnyAsync(b => b.Id == dto.BuildingId && b.IsActive);
        if (!buildingExists)
            throw new InvalidOperationException($"Building with ID {dto.BuildingId} not found.");

        var roomTypeExists = await _dbContext.RoomTypes.AnyAsync(rt => rt.Id == dto.RoomTypeId && rt.IsActive);
        if (!roomTypeExists)
            throw new InvalidOperationException($"RoomType with ID {dto.RoomTypeId} not found.");

        var duplicateExists = await _dbContext.Rooms.AnyAsync(r =>
            r.RoomNumber == dto.RoomNumber && r.BuildingId == dto.BuildingId && r.IsActive);
        if (duplicateExists)
            throw new InvalidOperationException($"Room number '{dto.RoomNumber}' already exists in this building.");

        var room = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = dto.RoomNumber,
            Floor = dto.Floor,
            BuildingId = dto.BuildingId,
            RoomTypeId = dto.RoomTypeId,
            Status = RoomStatus.Free,
            BasePrice = dto.BasePrice,
            Notes = dto.Notes,
            IsActive = true
        };

        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        return await GetRoomByIdAsync(room.Id)
            ?? throw new InvalidOperationException($"Room with ID {room.Id} not found after creation.");
    }

    public async Task<RoomDto> UpdateRoomAsync(Guid id, UpdateRoomDto dto)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.Building)
            .Include(r => r.RoomType)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
            throw new InvalidOperationException($"Room with ID {id} not found.");

        if (dto.BuildingId.HasValue)
        {
            var buildingExists = await _dbContext.Buildings.AnyAsync(b => b.Id == dto.BuildingId.Value && b.IsActive);
            if (!buildingExists)
                throw new InvalidOperationException($"Building with ID {dto.BuildingId} not found.");

            room.BuildingId = dto.BuildingId.Value;
        }

        if (dto.RoomTypeId.HasValue)
        {
            var roomTypeExists = await _dbContext.RoomTypes.AnyAsync(rt => rt.Id == dto.RoomTypeId.Value && rt.IsActive);
            if (!roomTypeExists)
                throw new InvalidOperationException($"RoomType with ID {dto.RoomTypeId} not found.");

            room.RoomTypeId = dto.RoomTypeId.Value;
        }

        if (dto.RoomNumber != null)
        {
            var duplicateExists = await _dbContext.Rooms.AnyAsync(r =>
                r.RoomNumber == dto.RoomNumber && r.BuildingId == room.BuildingId && r.Id != id && r.IsActive);
            if (duplicateExists)
                throw new InvalidOperationException($"Room number '{dto.RoomNumber}' already exists in this building.");

            room.RoomNumber = dto.RoomNumber;
        }

        if (dto.Floor.HasValue)
        {
            if (dto.Floor.Value < 0)
                throw new InvalidOperationException("Floor cannot be negative.");
            room.Floor = dto.Floor.Value;
        }

        if (dto.BasePrice.HasValue)
        {
            if (dto.BasePrice.Value <= 0)
                throw new InvalidOperationException("Base price must be greater than zero.");
            room.BasePrice = dto.BasePrice.Value;
        }

        room.Notes = dto.Notes ?? room.Notes;

        await _dbContext.SaveChangesAsync();

        return await GetRoomByIdAsync(room.Id)
            ?? throw new InvalidOperationException($"Room with ID {room.Id} not found after update.");
    }

    public async Task UpdateRoomStatusAsync(Guid id, RoomStatus newStatus)
    {
        var room = await _dbContext.Rooms
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
            throw new InvalidOperationException($"Room with ID {id} not found.");

        if (!RoomStatusTransitions.IsValid(room.Status, newStatus))
            throw new InvalidOperationException($"Invalid status transition from {room.Status} to {newStatus}.");

        var oldStatus = room.Status;
        room.Status = newStatus;
        await _dbContext.SaveChangesAsync();

        var building = await _dbContext.Buildings.FindAsync(room.BuildingId);
        var hotelId = building?.Id;

        await _broadcaster.BroadcastStatusChangeAsync(room.Id, room.RoomNumber, oldStatus, newStatus, hotelId);
    }

    public async Task DeleteRoomAsync(Guid id)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.Building)
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
            throw new InvalidOperationException($"Room with ID {id} not found.");

        if (room.Status == RoomStatus.Occupied || room.Status == RoomStatus.Reserved)
            throw new InvalidOperationException("Cannot delete a room that is occupied or has active reservations.");

        room.IsActive = false;
        await _dbContext.SaveChangesAsync();
    }

    private static RoomDto MapToDto(Room room, RoomStatus computedStatus)
    {
        return new RoomDto(
            room.Id,
            room.RoomNumber,
            room.Floor,
            room.BuildingId,
            room.Building.Name,
            room.RoomTypeId,
            room.RoomType.Name,
            computedStatus.ToString(),
            room.RoomType.BaseCapacity,
            room.RoomType.MaxCapacity,
            room.BasePrice,
            room.Notes
        );
    }
}
