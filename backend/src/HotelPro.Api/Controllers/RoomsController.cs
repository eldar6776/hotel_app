using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/rooms")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Reception,Housekeeping")]
    public async Task<ActionResult<PagedResult<RoomDto>>> GetRooms(
        [FromQuery] string? status,
        [FromQuery] Guid? buildingId,
        [FromQuery] Guid? roomTypeId,
        [FromQuery] int? floor,
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        List<RoomStatus>? statuses = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            statuses = status.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Enum.TryParse<RoomStatus>(s.Trim(), true, out var parsed) ? parsed : (RoomStatus?)null)
                .Where(s => s.HasValue)
                .Select(s => s!.Value)
                .ToList();
        }

        var filter = new RoomFilter(
            Status: statuses,
            BuildingId: buildingId,
            RoomTypeId: roomTypeId,
            Floor: floor,
            Search: search,
            IncludeInactive: includeInactive,
            Page: page,
            PageSize: pageSize
        );

        var result = await _roomService.GetRoomsAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Reception,Housekeeping")]
    public async Task<ActionResult<RoomDto>> GetRoom(Guid id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
            return NotFound();

        return Ok(room);
    }

    [HttpGet("{id:guid}/status")]
    [Authorize(Roles = "Admin,Manager,Reception,Housekeeping")]
    public async Task<ActionResult<RoomStatusDetailDto>> GetRoomStatus(Guid id, [FromQuery] DateTime? date)
    {
        var status = await _roomService.GetRoomStatusAsync(id, date);
        if (status == null)
            return NotFound();

        return Ok(status);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<RoomDto>> CreateRoom(CreateRoomDto dto)
    {
        try
        {
            var room = await _roomService.CreateRoomAsync(dto);
            return CreatedAtAction(nameof(GetRoom), new { id = room.Id, version = "2.0" }, room);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<RoomDto>> UpdateRoom(Guid id, UpdateRoomDto dto)
    {
        try
        {
            var room = await _roomService.UpdateRoomAsync(id, dto);
            return Ok(room);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Manager,Reception,Housekeeping")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] RoomStatus newStatus)
    {
        try
        {
            await _roomService.UpdateRoomStatusAsync(id, newStatus);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteRoom(Guid id)
    {
        try
        {
            await _roomService.DeleteRoomAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
