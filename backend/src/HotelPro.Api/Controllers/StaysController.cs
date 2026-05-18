using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/stays")]
[Authorize]
public class StaysController : ControllerBase
{
    private readonly IStayLifecycleService _stayLifecycle;
    private readonly ICheckOutWorkflowService _checkOutWorkflow;
    private readonly IConfigurationService _config;
    private readonly ILogger<StaysController> _logger;

    public StaysController(
        IStayLifecycleService stayLifecycle,
        ICheckOutWorkflowService checkOutWorkflow,
        IConfigurationService config,
        ILogger<StaysController> logger)
    {
        _stayLifecycle = stayLifecycle;
        _checkOutWorkflow = checkOutWorkflow;
        _config = config;
        _logger = logger;
    }

    [HttpPost("check-in")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<StayCheckInResponse>> CheckIn(StayCheckInRequest request)
    {
        try
        {
            var result = await _stayLifecycle.CheckInAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("check-out/full")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<CheckOutWorkflowResponse>> FullCheckOut([FromBody] FullCheckOutRequest request)
    {
        try
        {
            var result = await _checkOutWorkflow.FullCheckOutAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("check-out/partial")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<PartialCheckOutResponse>> PartialCheckOut([FromBody] PartialCheckOutRequest request)
    {
        try
        {
            var result = await _checkOutWorkflow.PartialCheckOutAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
