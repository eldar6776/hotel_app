using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/availability")]
[Authorize]
public class AvailabilityController : ControllerBase
{
    private readonly IBookingAvailabilityService _availabilityService;

    public AvailabilityController(IBookingAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpGet]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<AvailabilityResult>> CheckAvailability(
        [FromQuery] Guid roomTypeId,
        [FromQuery] DateTime arrival,
        [FromQuery] DateTime departure,
        [FromQuery] int quantity = 1,
        [FromQuery] Guid? excludeBookingId = null)
    {
        if (arrival >= departure)
            return BadRequest(new { error = "Arrival date must be before departure date." });

        var request = new AvailabilityRequest(
            RoomTypeId: roomTypeId,
            Arrival: arrival,
            Departure: departure,
            Quantity: quantity,
            ExcludeBookingId: excludeBookingId);

        var result = await _availabilityService.CheckAvailabilityAsync(request);
        return Ok(result);
    }

    [HttpPost("check-and-lock")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<LockResult>> CheckAndLock([FromBody] LockRequest request)
    {
        if (request.Arrival >= request.Departure)
            return BadRequest(new { error = "Arrival date must be before departure date." });

        var result = await _availabilityService.LockRoomsAsync(request);

        if (!result.Success)
            return Conflict(new { error = result.ErrorMessage });

        return Ok(result);
    }

    [HttpDelete("lock/{lockId}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> ReleaseLock(string lockId)
    {
        await _availabilityService.ReleaseRoomLockAsync(lockId);
        return NoContent();
    }
}
