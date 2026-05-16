using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/groups")]
[Authorize]
public class BookingGroupsController : ControllerBase
{
    private readonly IBookingGroupService _groupService;

    public BookingGroupsController(IBookingGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpGet]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<PagedResult<BookingGroupDto>>> GetGroups(
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new BookingGroupFilter(
            Status: status,
            FromDate: fromDate,
            ToDate: toDate,
            Page: page,
            PageSize: pageSize
        );

        var result = await _groupService.GetGroupsAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<BookingGroupDto>> GetGroup(Guid id)
    {
        var group = await _groupService.GetGroupByIdAsync(id);
        if (group == null)
            return NotFound();

        return Ok(group);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<BookingGroupDto>> CreateGroup(CreateBookingGroupDto dto)
    {
        try
        {
            var group = await _groupService.CreateGroupAsync(dto);
            return CreatedAtAction(nameof(GetGroup), new { id = group.Id, version = "2.0" }, group);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<BookingGroupDto>> UpdateGroup(Guid id, UpdateBookingGroupDto dto)
    {
        try
        {
            var group = await _groupService.UpdateGroupAsync(id, dto);
            return Ok(group);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/release")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> ReleaseGroup(Guid id)
    {
        try
        {
            var released = await _groupService.ReleaseGroupAsync(id);
            return Ok(new { message = $"Released {released} bookings.", releasedCount = released });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}/master-bill")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<MasterBillDto>> GetMasterBill(Guid id)
    {
        try
        {
            var bill = await _groupService.GetMasterBillAsync(id);
            return Ok(bill);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
