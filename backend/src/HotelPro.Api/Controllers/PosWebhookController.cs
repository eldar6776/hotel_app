using Asp.Versioning;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/folio")]
public class PosWebhookController : ControllerBase
{
    private readonly HotelProDbContext _dbContext;
    private readonly IFolioService _folioService;

    public PosWebhookController(HotelProDbContext dbContext, IFolioService folioService)
    { _dbContext = dbContext; _folioService = folioService; }

    [HttpPost("charge-from-pos")]
    public async Task<ActionResult> ChargeFromPos([FromBody] PosChargeRequest request)
    {
        var signature = Request.Headers["X-POS-Signature"].FirstOrDefault();
        var source = Request.Headers["X-POS-Source"].FirstOrDefault() ?? "unknown";

        if (string.IsNullOrEmpty(signature))
            return Unauthorized(new { error = "Missing HMAC signature" });

        var secret = "shared-pos-secret";
        var payload = System.Text.Json.JsonSerializer.Serialize(request);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expectedSig = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

        if (signature != expectedSig)
            return Unauthorized(new { error = "Invalid signature" });

        var room = await _dbContext.Rooms
            .FirstOrDefaultAsync(r => r.RoomNumber == request.RoomNumber && r.IsActive);

        if (room == null)
            return BadRequest(new { error = $"Room {request.RoomNumber} not found" });

        if (room.Status != RoomStatus.Occupied)
            return BadRequest(new { error = "Room is not occupied" });

        var booking = await _dbContext.Bookings
            .IgnoreQueryFilters()
            .Include(b => b.BookingRooms)
            .FirstOrDefaultAsync(b => b.Status == BookingStatus.CheckedIn
                && b.BookingRooms.Any(br => br.RoomId == room.Id));

        if (booking == null)
            return BadRequest(new { error = "No active booking for this room" });

        var folio = await _dbContext.Folios
            .FirstOrDefaultAsync(f => f.BookingId == booking.Id && f.Status == FolioStatus.Open);

        if (folio == null)
            return BadRequest(new { error = "No open folio for this booking" });

        foreach (var item in request.Items)
        {
            var chargeType = item.Category?.ToUpper() switch
            {
                "RESTAURANT" => ChargeType.Restaurant,
                "BAR" => ChargeType.Bar,
                "ROOM_SERVICE" => ChargeType.Service,
                "LAUNDRY" => ChargeType.Laundry,
                _ => ChargeType.Other
            };

            await _folioService.AddChargeAsync(folio.Id, new HotelPro.Core.DTOs.CreateFolioChargeDto(
                chargeType.ToString(),
                item.Description,
                item.Quantity,
                item.Amount,
                DateTime.UtcNow,
                request.PosTransactionId
            ));
        }

        return Ok(new { message = $"Charged {request.Items.Count} items to room {request.RoomNumber}" });
    }
}

public class PosChargeRequest
{
    public string PosTransactionId { get; set; } = "";
    public string RoomNumber { get; set; } = "";
    public List<PosItem> Items { get; set; } = new();
}

public class PosItem
{
    public string Description { get; set; } = "";
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public int Quantity { get; set; } = 1;
}
