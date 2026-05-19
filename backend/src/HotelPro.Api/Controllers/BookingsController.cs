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
    private readonly IEmailService _emailService;
    private readonly IReservationPolicyService _reservationPolicy;

    public BookingsController(
        IBookingService bookingService,
        IEmailService emailService,
        IReservationPolicyService reservationPolicy)
    {
        _bookingService = bookingService;
        _emailService = emailService;
        _reservationPolicy = reservationPolicy;
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
            FromDate: NormalizeQueryDate(fromDate),
            ToDate: NormalizeQueryDate(toDate),
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
            if (newStatus == BookingStatus.Confirmed)
            {
                await _reservationPolicy.ConfirmReservationAsync(
                    new ConfirmReservationRequest(id));
            }
            else if (newStatus == BookingStatus.Cancelled)
            {
                await _reservationPolicy.CancelReservationAsync(
                    new CancelReservationRequest(id, "Manual cancellation"));
            }
            else if (newStatus == BookingStatus.NoShow)
            {
                await _reservationPolicy.MarkNoShowAsync(
                    new MarkNoShowRequest(id));
            }
            else
            {
                await _bookingService.UpdateBookingStatusAsync(id, newStatus);
            }

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

    [HttpPost("{id:guid}/email/confirmation")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> SendConfirmationEmail(Guid id)
    {
        var result = await _emailService.SendConfirmationAsync(id);
        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Confirmation email sent." });
    }

    [HttpPost("{id:guid}/email/cancellation")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> SendCancellationEmail(Guid id)
    {
        var result = await _emailService.SendCancellationAsync(id);
        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new { message = "Cancellation email sent." });
    }

    private static DateTime? NormalizeQueryDate(DateTime? value)
    {
        if (!value.HasValue) return null;

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }
}
