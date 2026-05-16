using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/reception")]
[Authorize]
public class ReceptionController : ControllerBase
{
    private readonly ICheckInService _checkInService;
    private readonly ICheckOutService _checkOutService;

    public ReceptionController(ICheckInService checkInService, ICheckOutService checkOutService)
    {
        _checkInService = checkInService;
        _checkOutService = checkOutService;
    }

    [HttpPost("check-in")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<CheckInResponse>> CheckIn(CheckInRequest request)
    {
        try
        {
            var result = await _checkInService.CheckInAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("check-out")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<CheckOutResponse>> CheckOut(CheckOutRequest request)
    {
        try
        {
            var result = await _checkOutService.CheckOutAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
