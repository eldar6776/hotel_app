using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/hardware"), Authorize]
public class HardwareController : ControllerBase
{
    [HttpPost("rfid/encode"), Authorize(Roles = "Admin,Manager")]
    public ActionResult EncodeRfid([FromQuery] string code, [FromQuery] string room)
    {
        return Ok(new { status = "queued", code, room, message = $"RFID encode request queued for room {room}" });
    }

    [HttpPost("fiscal/print"), Authorize(Roles = "Admin,Manager")]
    public ActionResult FiscalPrint([FromBody] object receipt)
    {
        return Ok(new { status = "sent", fiscalCode = $"JIR-{Guid.NewGuid().ToString()[..8].ToUpper()}" });
    }

    [HttpGet("pabx/status"), Authorize(Roles = "Admin,Manager,Reception")]
    public ActionResult PabxStatus()
    {
        return Ok(new { connected = false, message = "PABX bridge not configured" });
    }

    [HttpPost("pabx/wakeup-call"), Authorize(Roles = "Admin,Manager,Reception")]
    public ActionResult SetWakeupCall([FromBody] WakeupCallRequest req)
    {
        return Ok(new { status = "scheduled", req.RoomNumber, req.Time, message = $"Wake-up call scheduled for room {req.RoomNumber} at {req.Time}" });
    }
}

public record WakeupCallRequest(string RoomNumber, string Time);
