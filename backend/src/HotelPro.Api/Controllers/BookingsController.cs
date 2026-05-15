using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<PagedResult<BookingDto>>> GetBookings(
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] Guid? guestId,
        [FromQuery] Guid? roomId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        List<BookingStatus>? statuses = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            statuses = status.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Enum.TryParse<BookingStatus>(s.Trim(), true, out var parsed) ? parsed : (BookingStatus?)null)
                .Where(s => s.HasValue)
                .Select(s => s!.Value)
                .ToList();
        }

        var filter = new BookingFilter(
            Status: statuses,
            FromDate: fromDate,
            ToDate: toDate,
            GuestId: guestId,
            RoomId: roomId,
            Page: page,
            PageSize: pageSize
        );

        var result = await _bookingService.GetBookingsAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<BookingDto>> GetBooking(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
            return NotFound();

        return Ok(booking);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<BookingDto>> CreateBooking(CreateBookingDto dto)
    {
        try
        {
            var booking = await _bookingService.CreateBookingAsync(dto);
            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id, version = "2.0" }, booking);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<BookingDto>> UpdateBooking(Guid id, UpdateBookingDto dto)
    {
        try
        {
            var booking = await _bookingService.UpdateBookingAsync(id, dto);
            return Ok(booking);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] BookingStatus newStatus)
    {
        try
        {
            await _bookingService.UpdateBookingStatusAsync(id, newStatus);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> DeleteBooking(Guid id)
    {
        try
        {
            await _bookingService.DeleteBookingAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
