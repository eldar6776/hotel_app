using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/channel-manager"), Authorize]
public class ChannelManagerController : ControllerBase
{
    [HttpGet("status"), Authorize(Roles = "Admin,Manager")]
    public ActionResult Status()
    {
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
    public ActionResult SyncBookingCom()
    {
        return Ok(new { status = "mock", message = "Booking.com sync simulated - no API key configured", reservations = 0, updatedPrices = 0 });
    }

    [HttpPost("airbnb/sync"), Authorize(Roles = "Admin,Manager")]
    public ActionResult SyncAirbnb()
    {
        return Ok(new { status = "mock", message = "Airbnb sync simulated - no API key configured", reservations = 0, updatedPrices = 0 });
    }

    [HttpPost("availability/push"), Authorize(Roles = "Admin,Manager")]
    public ActionResult PushAvailability()
    {
        return Ok(new { status = "mock", message = "Availability pushed to all connected channels", channels = 0 });
    }

    [HttpPost("rates/sync"), Authorize(Roles = "Admin,Manager")]
    public ActionResult SyncRates()
    {
        return Ok(new { status = "mock", message = "Rate plans synced to all connected channels", updated = 0 });
    }

    [HttpGet("webhook/events"), Authorize(Roles = "Admin,Manager")]
    public ActionResult RecentEvents()
    {
        return Ok(new { events = Array.Empty<object>(), total = 0 });
    }
}
