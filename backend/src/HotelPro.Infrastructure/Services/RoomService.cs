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

    public RoomService(HotelProDbContext dbContext, IRoomStatusBroadcaster broadcaster)
    {
        _dbContext = dbContext;
        _broadcaster = broadcaster;
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

        if (filter.Status != null && filter.Status.Count > 0)
        {
            query = query.Where(r => filter.Status.Contains(r.Status));
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

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(r => r.Building.Name)
            .ThenBy(r => r.Floor)
            .ThenBy(r => r.RoomNumber)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new RoomDto(
                r.Id,
                r.RoomNumber,
                r.Floor,
                r.BuildingId,
                r.Building.Name,
                r.RoomTypeId,
                r.RoomType.Name,
                r.Status.ToString(),
                r.RoomType.BaseCapacity,
                r.RoomType.MaxCapacity,
                r.BasePrice,
                r.Notes
            ))
            .ToListAsync();

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

        return new RoomDto(
            room.Id,
            room.RoomNumber,
            room.Floor,
            room.BuildingId,
            room.Building.Name,
            room.RoomTypeId,
            room.RoomType.Name,
            room.Status.ToString(),
            room.RoomType.BaseCapacity,
            room.RoomType.MaxCapacity,
            room.BasePrice,
            room.Notes
        );
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomDto dto)
    {
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
            room.Floor = dto.Floor.Value;

        if (dto.Status.HasValue)
            room.Status = dto.Status.Value;

        if (dto.BasePrice.HasValue)
            room.BasePrice = dto.BasePrice.Value;

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
}
