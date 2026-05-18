using Asp.Versioning;
using HotelPro.Api.Attributes;
using HotelPro.Core.Attributes;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/channel-manager"), Authorize]
[Mock("Channel manager integrations require provider API credentials.")]
[FeatureGate("ChannelManager")]
public class ChannelManagerController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlags;

    public ChannelManagerController(IFeatureFlagService featureFlags)
    {
        _featureFlags = featureFlags;
    }

    [HttpGet("status"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> Status()
    {
        var unavailable = await GetMockUnavailableAsync("ChannelManager");
        if (unavailable != null) return unavailable;

        return Ok(new
        {
            channels = new[]
            {
                new { name = "Booking.com", connected = false, lastSync = (DateTime?)null, activeProperties = 0 },
                new { name = "Airbnb", connected = false, lastSync = (DateTime?)null, activeProperties = 0 },
                new { name = "Expedia", connected = false, lastSync = (DateTime?)null, activeProperties = 0 }
            },
            totalActiveConnections = 0
        });
    }

    [HttpPost("bookingcom/sync"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> SyncBookingCom()
    {
        var unavailable = await GetMockUnavailableAsync("ChannelManager");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "mock", message = "Booking.com sync simulated - no API key configured", reservations = 0, updatedPrices = 0 });
    }

    [HttpPost("airbnb/sync"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> SyncAirbnb()
    {
        var unavailable = await GetMockUnavailableAsync("ChannelManager");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "mock", message = "Airbnb sync simulated - no API key configured", reservations = 0, updatedPrices = 0 });
    }

    [HttpPost("availability/push"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> PushAvailability()
    {
        var unavailable = await GetMockUnavailableAsync("ChannelManager");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "mock", message = "Availability pushed to all connected channels", channels = 0 });
    }

    [HttpPost("rates/sync"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> SyncRates()
    {
        var unavailable = await GetMockUnavailableAsync("ChannelManager");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "mock", message = "Rate plans synced to all connected channels", updated = 0 });
    }

    [HttpGet("webhook/events"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> RecentEvents()
    {
        var unavailable = await GetMockUnavailableAsync("ChannelManager");
        if (unavailable != null) return unavailable;

        return Ok(new { events = Array.Empty<object>(), total = 0 });
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
