using Asp.Versioning;
using HotelPro.Api.Attributes;
using HotelPro.Core.Attributes;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/iot"), Authorize]
[Mock("IoT endpoints require MQTT broker and device configuration.")]
[FeatureGate("IoT")]
public class IoTController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlags;

    public IoTController(IFeatureFlagService featureFlags)
    {
        _featureFlags = featureFlags;
    }

    [HttpGet("status"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Status()
    {
        var unavailable = await GetMockUnavailableAsync("IoT");
        if (unavailable != null) return unavailable;

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
    public async Task<ActionResult> RoomEnvironment(Guid roomId)
    {
        var unavailable = await GetMockUnavailableAsync("IoT");
        if (unavailable != null) return unavailable;

        return Ok(new { roomId, temperature = 22.5, humidity = 45, occupancy = false, windowOpen = false, lastUpdated = DateTime.UtcNow });
    }

    [HttpPost("rooms/{roomId:guid}/lock"), Authorize(Roles = "Admin,Manager,Reception")]
    public async Task<ActionResult> ControlLock(Guid roomId, [FromBody] LockCommand cmd)
    {
        var unavailable = await GetMockUnavailableAsync("IoT");
        if (unavailable != null) return unavailable;

        return Ok(new { roomId, action = cmd.Action, status = "mock_success", message = $"Lock {cmd.Action} simulated for room" });
    }

    [HttpGet("energy"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> EnergyConsumption([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var unavailable = await GetMockUnavailableAsync("IoT");
        if (unavailable != null) return unavailable;

        return Ok(new { from = from ?? DateTime.UtcNow.AddDays(-7), to = to ?? DateTime.UtcNow, totalKwh = 0, rooms = Array.Empty<object>() });
    }

    private async Task<ActionResult?> GetMockUnavailableAsync(string featureName)
    {
        if (!await _featureFlags.IsEnabledAsync(featureName))
        {
            return Ok(new { status = "not_configured", message = "Configure in Admin > Settings" });
        }

        return Ok(new { status = "missing_api_key", message = "Enter API key in Admin > Settings" });
    }
}

public record LockCommand(string Action);
