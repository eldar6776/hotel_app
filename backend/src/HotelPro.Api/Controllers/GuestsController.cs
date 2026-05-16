using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/guests")]
[Authorize]
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;

    public GuestsController(IGuestService guestService)
    {
        _guestService = guestService;
    }

    [HttpGet]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<PagedResult<GuestDto>>> GetGuests(
        [FromQuery] string? search,
        [FromQuery] string? documentNumber,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new GuestFilter(search, documentNumber, page, pageSize);
        var result = await _guestService.GetGuestsAsync(filter);
        return Ok(result);
    }

    [HttpGet("search")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<List<GuestAutoSuggestDto>>> SearchGuests(
        [FromQuery] string? q,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new List<GuestAutoSuggestDto>());

        var result = await _guestService.SearchGuestsAsync(q, limit);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<GuestDto>> GetGuest(Guid id)
    {
        var guest = await _guestService.GetGuestByIdAsync(id);
        if (guest == null) return NotFound();
        return Ok(guest);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<GuestDto>> CreateGuest(CreateGuestDto dto)
    {
        try
        {
            var guest = await _guestService.CreateGuestAsync(dto);
            return CreatedAtAction(nameof(GetGuest), new { id = guest.Id, version = "2.0" }, guest);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<GuestDto>> UpdateGuest(Guid id, UpdateGuestDto dto)
    {
        try
        {
            var guest = await _guestService.UpdateGuestAsync(id, dto);
            return Ok(guest);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> DeleteGuest(Guid id)
    {
        try
        {
            await _guestService.DeleteGuestAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
