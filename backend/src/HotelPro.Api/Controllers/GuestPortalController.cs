using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/guest-portal")]
public class GuestPortalController : ControllerBase
{
    [HttpPost("online-checkin")]
    public ActionResult OnlineCheckIn([FromBody] OnlineCheckInRequest req)
    {
        return Ok(new { status = "completed", message = "Online check-in processed", roomNumber = req.RoomPreference ?? "Auto-assigned" });
    }

    [HttpPost("online-checkout")]
    public ActionResult OnlineCheckOut([FromBody] OnlineCheckOutRequest req)
    {
        return Ok(new { status = "completed", message = "Online check-out processed", totalAmount = 350m });
    }

    [HttpGet("digital-key/{bookingId:guid}")]
    public ActionResult DigitalKey(Guid bookingId)
    {
        return Ok(new { bookingId, status = "active", keyUrl = $"https://hotelpro.app/key/{Guid.NewGuid():N}"[..12], validUntil = DateTime.UtcNow.AddDays(3) });
    }

    [HttpGet("room-service-menu")]
    public ActionResult RoomServiceMenu()
    {
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
    public ActionResult PlaceOrder([FromBody] object order)
    {
        return Ok(new { status = "received", orderId = Guid.NewGuid(), estimatedDelivery = "30min" });
    }
}

public record OnlineCheckInRequest(Guid BookingId, string? RoomPreference, string? Notes);
public record OnlineCheckOutRequest(Guid BookingId);
