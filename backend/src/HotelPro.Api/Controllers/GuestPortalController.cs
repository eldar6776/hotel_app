using Asp.Versioning;
using HotelPro.Api.Attributes;
using HotelPro.Core.Attributes;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/guest-portal")]
[Mock("Guest portal requires guest-scoped authentication and booking validation.")]
[FeatureGate("GuestPortal")]
public class GuestPortalController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlags;

    public GuestPortalController(IFeatureFlagService featureFlags)
    {
        _featureFlags = featureFlags;
    }

    [HttpPost("online-checkin")]
    public async Task<ActionResult> OnlineCheckIn([FromBody] OnlineCheckInRequest req)
    {
        var unavailable = await GetMockUnavailableAsync("GuestPortal");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "completed", message = "Online check-in processed", roomNumber = req.RoomPreference ?? "Auto-assigned" });
    }

    [HttpPost("online-checkout")]
    public async Task<ActionResult> OnlineCheckOut([FromBody] OnlineCheckOutRequest req)
    {
        var unavailable = await GetMockUnavailableAsync("GuestPortal");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "completed", message = "Online check-out processed", totalAmount = 350m });
    }

    [HttpGet("digital-key/{bookingId:guid}")]
    public async Task<ActionResult> DigitalKey(Guid bookingId)
    {
        var unavailable = await GetMockUnavailableAsync("GuestPortal");
        if (unavailable != null) return unavailable;

        return Ok(new { bookingId, status = "active", keyUrl = $"https://hotelpro.app/key/{Guid.NewGuid():N}"[..12], validUntil = DateTime.UtcNow.AddDays(3) });
    }

    [HttpGet("room-service-menu")]
    public async Task<ActionResult> RoomServiceMenu()
    {
        var unavailable = await GetMockUnavailableAsync("GuestPortal");
        if (unavailable != null) return unavailable;

        return Ok(new
        {
            categories = new[]
            {
                new { name = "Restoran", items = new[] { new { name = "Pizza", price = 12m }, new { name = "Pasta", price = 10m } } },
                new { name = "Bar", items = new[] { new { name = "Coca-Cola", price = 3m }, new { name = "Pivo", price = 4m } } }
            }
        });
    }

    [HttpPost("order")]
    public async Task<ActionResult> PlaceOrder([FromBody] object order)
    {
        var unavailable = await GetMockUnavailableAsync("GuestPortal");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "received", orderId = Guid.NewGuid(), estimatedDelivery = "30min" });
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

public record OnlineCheckInRequest(Guid BookingId, string? RoomPreference, string? Notes);
public record OnlineCheckOutRequest(Guid BookingId);
