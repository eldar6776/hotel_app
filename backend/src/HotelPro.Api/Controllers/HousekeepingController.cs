using Asp.Versioning;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/housekeeping"), Authorize]
public class HousekeepingController : ControllerBase
{
    private readonly HotelProDbContext _dbContext;

    public HousekeepingController(HotelProDbContext dbContext) => _dbContext = dbContext;

    [HttpGet("rooms"), Authorize(Roles = "Admin,Manager,Housekeeping")]
    public async Task<ActionResult> GetDirtyRooms()
    {
        var rooms = await _dbContext.Rooms
            .Where(r => r.Status == RoomStatus.Dirty && r.IsActive)
            .Select(r => new { r.Id, r.RoomNumber, r.Floor, Building = r.Building!.Name })
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();

        return Ok(rooms);
    }

    [HttpPost("rooms/{roomId:guid}/clean"), Authorize(Roles = "Admin,Manager,Housekeeping")]
    public async Task<ActionResult> MarkRoomClean(Guid roomId, [FromBody] CleanRoomRequest req)
    {
        var room = await _dbContext.Rooms.FindAsync(roomId);
        if (room == null) return NotFound();

        if (room.Status != RoomStatus.Dirty)
            return BadRequest(new { error = "Room is not dirty" });

        room.Status = RoomStatus.Free;
        room.Notes = req.Notes ?? room.Notes;

        _dbContext.HousekeepingLogs.Add(new HousekeepingLog
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            EmployeeId = Guid.Empty,
            Action = HousekeepingAction.Cleaned,
            Status = HousekeepingStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            Notes = req.Notes
        });

        await _dbContext.SaveChangesAsync();
        return Ok(new { message = $"Room {room.RoomNumber} marked clean" });
    }

    [HttpPost("rooms/{roomId:guid}/inspect"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> InspectRoom(Guid roomId, [FromBody] CleanRoomRequest req)
    {
        var room = await _dbContext.Rooms.FindAsync(roomId);
        if (room == null) return NotFound();

        _dbContext.HousekeepingLogs.Add(new HousekeepingLog
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            EmployeeId = Guid.Empty,
            Action = HousekeepingAction.Inspected,
            Status = req.Passed ? HousekeepingStatus.Verified : HousekeepingStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            Notes = req.Notes
        });

        await _dbContext.SaveChangesAsync();
        return Ok(new { message = $"Room {room.RoomNumber} inspected" });
    }

    [HttpGet("rooms/{roomId:guid}/logs"), Authorize(Roles = "Admin,Manager,Housekeeping")]
    public async Task<ActionResult> GetRoomLogs(Guid roomId, [FromQuery] int limit = 20)
    {
        var logs = await _dbContext.HousekeepingLogs
            .Where(l => l.RoomId == roomId)
            .OrderByDescending(l => l.CompletedAt)
            .Take(limit)
            .Select(l => new
            {
                l.Id, l.RoomId, l.Action, l.Status, PerformedAt = l.CompletedAt, l.Notes
            })
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("work-orders"), Authorize(Roles = "Admin,Manager,Housekeeping")]
    public async Task<ActionResult> GetWorkOrders([FromQuery] string? status)
    {
        var query = _dbContext.WorkOrders.AsQueryable();
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<WorkOrderStatus>(status, true, out var s))
            query = query.Where(w => w.Status == s);

        var orders = await query.OrderByDescending(w => w.CreatedAt).Take(50).ToListAsync();
        return Ok(orders);
    }

    [HttpPost("work-orders"), Authorize(Roles = "Admin,Manager,Housekeeping")]
    public async Task<ActionResult> CreateWorkOrder([FromBody] CreateWorkOrderRequest req)
    {
        var wo = new WorkOrder
        {
            Id = Guid.NewGuid(),
            RoomId = req.RoomId,
            Category = Enum.TryParse<WorkOrderCategory>(req.Category, true, out var c) ? c : WorkOrderCategory.Other,
            Priority = Enum.TryParse<WorkOrderPriority>(req.Priority, true, out var p) ? p : WorkOrderPriority.Medium,
            Status = WorkOrderStatus.Open,
            Description = req.Description,
            ReportedById = Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.WorkOrders.Add(wo);
        await _dbContext.SaveChangesAsync();
        return Ok(wo);
    }
}

public record CleanRoomRequest(string? Notes, bool Passed = true);
public record CreateWorkOrderRequest(Guid RoomId, string Category, string Priority, string Description);
