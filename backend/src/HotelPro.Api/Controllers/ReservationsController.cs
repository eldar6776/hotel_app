using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationPolicyService _reservationPolicy;

    public ReservationsController(IReservationPolicyService reservationPolicy)
    {
        _reservationPolicy = reservationPolicy;
    }

    [HttpPost("confirm")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<ReservationResultDto>> Confirm([FromBody] ConfirmReservationRequest request)
    {
        try
        {
            var result = await _reservationPolicy.ConfirmReservationAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("cancel")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<ReservationResultDto>> Cancel([FromBody] CancelReservationRequest request)
    {
        try
        {
            var result = await _reservationPolicy.CancelReservationAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("noshow")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<ReservationResultDto>> MarkNoShow([FromBody] MarkNoShowRequest request)
    {
        try
        {
            var result = await _reservationPolicy.MarkNoShowAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{bookingId:guid}/status")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<ReservationResultDto>> GetStatus(Guid bookingId)
    {
        try
        {
            var result = await _reservationPolicy.GetReservationStatusAsync(bookingId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
