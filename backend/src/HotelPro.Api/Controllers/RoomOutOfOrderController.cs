using System.Security.Claims;
using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/rooms/{roomId:guid}/ooo")]
[Authorize]
public class RoomOutOfOrderController : ControllerBase
{
    private readonly HotelProDbContext _dbContext;

    public RoomOutOfOrderController(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Reception,Housekeeping")]
    public async Task<ActionResult<IEnumerable<RoomOutOfOrderDto>>> GetOooEntries(Guid roomId)
    {
        var room = await _dbContext.Rooms.FindAsync(roomId);
        if (room == null)
            return NotFound("Room not found.");

        var entries = await _dbContext.RoomOutOfOrders
            .Where(o => o.RoomId == roomId)
            .OrderByDescending(o => o.StartDate)
            .Select(o => new RoomOutOfOrderDto(
                o.Id,
                o.RoomId,
                _dbContext.Rooms.Where(r => r.Id == o.RoomId).Select(r => r.RoomNumber).FirstOrDefault() ?? "Unknown",
                o.Reason,
                o.Description,
                o.StartDate,
                o.EndDate,
                o.Status,
                o.CreatedAt,
                o.ResolutionNotes,
                o.ResolvedAt
            ))
            .ToListAsync();

        return Ok(entries);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<RoomOutOfOrderDto>> CreateOoo(Guid roomId, CreateOooDto dto)
    {
        var room = await _dbContext.Rooms.FindAsync(roomId);
        if (room == null)
            return NotFound("Room not found.");

        if (room.Status == RoomStatus.Occupied)
            return BadRequest(new { error = "Cannot set an occupied room out of order." });

        if (dto.EndDate.HasValue)
        {
            var overlappingBookings = await _dbContext.BookingRooms
                .IgnoreQueryFilters()
                .Where(br => br.RoomId == roomId
                    && br.Status != BookingRoomStatus.Released
                    && br.Booking.Status != BookingStatus.Cancelled
                    && br.Booking.ArrivalDate < dto.EndDate.Value
                    && br.Booking.DepartureDate > dto.StartDate)
                .AnyAsync();

            if (overlappingBookings)
                return BadRequest(new { error = "Room has active reservations during this period." });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        var ooo = new RoomOutOfOrder
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            Reason = dto.Reason,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = "Active",
            CreatedById = userId != null ? Guid.Parse(userId) : Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RoomOutOfOrders.Add(ooo);

        room.Status = RoomStatus.OutOfOrder;

        await _dbContext.SaveChangesAsync();

        var roomNumber = await _dbContext.Rooms.Where(r => r.Id == roomId).Select(r => r.RoomNumber).FirstAsync();

        return CreatedAtAction(nameof(GetOooEntries), new { roomId, version = "2.0" },
            new RoomOutOfOrderDto(ooo.Id, ooo.RoomId, roomNumber, ooo.Reason, ooo.Description, ooo.StartDate, ooo.EndDate, ooo.Status, ooo.CreatedAt, ooo.ResolutionNotes, ooo.ResolvedAt));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<RoomOutOfOrderDto>> UpdateOoo(Guid roomId, Guid id, CreateOooDto dto)
    {
        var ooo = await _dbContext.RoomOutOfOrders
            .FirstOrDefaultAsync(o => o.Id == id && o.RoomId == roomId);

        if (ooo == null)
            return NotFound("OOO entry not found.");

        ooo.Reason = dto.Reason;
        ooo.Description = dto.Description;
        ooo.StartDate = dto.StartDate;
        ooo.EndDate = dto.EndDate;

        await _dbContext.SaveChangesAsync();

        var roomNumber = await _dbContext.Rooms.Where(r => r.Id == roomId).Select(r => r.RoomNumber).FirstAsync();

        return Ok(new RoomOutOfOrderDto(ooo.Id, ooo.RoomId, roomNumber, ooo.Reason, ooo.Description, ooo.StartDate, ooo.EndDate, ooo.Status, ooo.CreatedAt, ooo.ResolutionNotes, ooo.ResolvedAt));
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> ResolveOoo(Guid roomId, Guid id, [FromBody] ResolveOooDto dto)
    {
        var ooo = await _dbContext.RoomOutOfOrders
            .FirstOrDefaultAsync(o => o.Id == id && o.RoomId == roomId);

        if (ooo == null)
            return NotFound("OOO entry not found.");

        if (ooo.Status != "Active")
            return BadRequest(new { error = "OOO entry is already resolved." });

        ooo.Status = "Resolved";
        ooo.ResolvedAt = DateTime.UtcNow;
        ooo.ResolutionNotes = dto.ResolutionNotes;

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        if (userId != null && Guid.TryParse(userId, out var uid))
            ooo.ResolvedById = uid;

        var room = await _dbContext.Rooms.FindAsync(roomId);
        if (room != null && room.Status == RoomStatus.OutOfOrder)
            room.Status = RoomStatus.Free;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
