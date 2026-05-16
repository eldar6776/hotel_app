using Asp.Versioning;
using HotelPro.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/hardware"), Authorize]
public class HardwareController : ControllerBase
{
    private readonly BridgeService _bridge;

    public HardwareController(BridgeService bridge) => _bridge = bridge;

    [HttpGet("status"), Authorize(Roles = "Admin,Manager,Reception")]
    public ActionResult GetStatus() => Ok(_bridge.GetStatus());

    [HttpGet("queue"), Authorize(Roles = "Admin,Manager")]
    public ActionResult GetQueue() => Ok(_bridge.GetQueue());

    [HttpPost("{device}/command"), Authorize(Roles = "Admin,Manager")]
    public ActionResult SendCommand(string device, [FromBody] object payload)
    {
        var id = _bridge.Enqueue(device, "command", payload);
        return Ok(new { status = "queued", jobId = id });
    }

    [HttpPost("fiscal/fiscalize"), Authorize(Roles = "Admin,Manager")]
    public ActionResult Fiscalize([FromBody] FiscalInvoiceData data)
    {
        var result = _bridge.FiscalizeInvoice(data);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("rfid/encode"), Authorize(Roles = "Admin,Manager")]
    public ActionResult EncodeRfid([FromBody] RfidEncodeData data)
    {
        var result = _bridge.EncodeRfidCard(data);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("pabx/cdr"), Authorize(Roles = "Admin,Manager")]
    public ActionResult ImportCdr([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = _bridge.ImportCdr(from ?? DateTime.UtcNow.AddDays(-1), to ?? DateTime.UtcNow);
        return Ok(result);
    }

    [HttpPost("pabx/wakeup-call"), Authorize(Roles = "Admin,Manager,Reception")]
    public ActionResult SetWakeupCall([FromBody] WakeupCallRequest req)
    {
        return Ok(new { status = "scheduled", req.RoomNumber, req.Time });
    }
}

public record WakeupCallRequest(string RoomNumber, string Time, string? Notes = null);
