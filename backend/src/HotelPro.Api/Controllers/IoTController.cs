using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/iot"), Authorize]
public class IoTController : ControllerBase
{
    [HttpGet("status"), Authorize(Roles = "Admin,Manager")]
    public ActionResult Status()
    {
        return Ok(new
        {
            mqttBroker = "disconnected",
            devices = new[]
            {
                new { type = "SmartLock", count = 0, online = 0 },
                new { type = "Thermostat", count = 0, online = 0 },
                new { type = "WindowSensor", count = 0, online = 0 },
                new { type = "OccupancySensor", count = 0, online = 0 }
            }
        });
    }

    [HttpGet("rooms/{roomId:guid}/environment"), Authorize(Roles = "Admin,Manager,Reception")]
    public ActionResult RoomEnvironment(Guid roomId)
    {
        return Ok(new { roomId, temperature = 22.5, humidity = 45, occupancy = false, windowOpen = false, lastUpdated = DateTime.UtcNow });
    }

    [HttpPost("rooms/{roomId:guid}/lock"), Authorize(Roles = "Admin,Manager,Reception")]
    public ActionResult ControlLock(Guid roomId, [FromBody] LockCommand cmd)
    {
        return Ok(new { roomId, action = cmd.Action, status = "mock_success", message = $"Lock {cmd.Action} simulated for room" });
    }

    [HttpGet("energy"), Authorize(Roles = "Admin,Manager")]
    public ActionResult EnergyConsumption([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(new { from = from ?? DateTime.UtcNow.AddDays(-7), to = to ?? DateTime.UtcNow, totalKwh = 0, rooms = Array.Empty<object>() });
    }
}

public record LockCommand(string Action);
