using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/folios")]
[Authorize]
public class FoliosController : ControllerBase
{
    private readonly IFolioService _folioService;

    public FoliosController(IFolioService folioService)
    {
        _folioService = folioService;
    }

    [HttpGet("booking/{bookingId:guid}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<List<FolioDto>>> GetFoliosByBooking(Guid bookingId)
    {
        var folios = await _folioService.GetFoliosByBookingAsync(bookingId);
        return Ok(folios);
    }

    [HttpGet("booking/{bookingId:guid}/main")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<FolioDto>> GetMainFolio(Guid bookingId)
    {
        var folio = await _folioService.GetFolioByBookingAsync(bookingId);
        if (folio == null)
            return NotFound();
        return Ok(folio);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<FolioDto>> CreateFolio(CreateFolioDto dto)
    {
        try
        {
            var folio = await _folioService.CreateFolioAsync(dto);
            return Ok(folio);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("sub-folio")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<FolioDto>> CreateSubFolio(CreateSubFolioDto dto)
    {
        try
        {
            var folio = await _folioService.CreateSubFolioAsync(dto);
            return Ok(folio);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{folioId:guid}/charges")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<FolioChargeDto>> AddCharge(Guid folioId, CreateFolioChargeDto dto)
    {
        try
        {
            var charge = await _folioService.AddChargeAsync(folioId, dto);
            return Ok(charge);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("charges/{chargeId:guid}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> DeleteCharge(Guid chargeId)
    {
        try
        {
            await _folioService.DeleteChargeAsync(chargeId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("charges/{chargeId:guid}/storno")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<FolioChargeDto>> StornoCharge(Guid chargeId, [FromBody] StornoRequest request)
    {
        try
        {
            var storno = await _folioService.StornoChargeAsync(chargeId, request.Reason);
            return Ok(storno);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{folioId:guid}/close")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> CloseFolio(Guid folioId)
    {
        try
        {
            await _folioService.CloseFolioAsync(folioId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{folioId:guid}/balance")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<decimal>> GetBalance(Guid folioId)
    {
        var balance = await _folioService.GetFolioBalanceAsync(folioId);
        return Ok(new { folioId, balance });
    }
}

public record StornoRequest(string Reason);
