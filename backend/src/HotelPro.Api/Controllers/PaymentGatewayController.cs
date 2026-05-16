using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/payments"), Authorize]
public class PaymentGatewayController : ControllerBase
{
    [HttpPost("charge"), Authorize(Policy = "CanManageBookings")]
    public ActionResult ChargeCard([FromBody] ChargeRequest req)
    {
        return Ok(new { status = "success", transactionId = $"TXN-{Guid.NewGuid():N}"[..16], amount = req.Amount, currency = req.Currency ?? "EUR", timestamp = DateTime.UtcNow });
    }

    [HttpPost("refund"), Authorize(Policy = "CanManageBookings")]
    public ActionResult Refund([FromBody] RefundRequest req)
    {
        return Ok(new { status = "success", refundId = $"REF-{Guid.NewGuid():N}"[..16], amount = req.Amount, originalTransactionId = req.TransactionId });
    }

    [HttpPost("tokenize"), Authorize(Policy = "CanManageBookings")]
    public ActionResult TokenizeCard([FromBody] TokenizeRequest req)
    {
        return Ok(new { status = "success", token = $"tok_{Guid.NewGuid():N}"[..24], last4 = req.CardNumber?[^4..] ?? "****", expiryMonth = req.ExpiryMonth, expiryYear = req.ExpiryYear });
    }

    [HttpGet("webhook/events"), Authorize(Roles = "Admin,Manager")]
    public ActionResult RecentWebhooks()
    {
        return Ok(new { events = Array.Empty<object>(), total = 0 });
    }
}

public record ChargeRequest(decimal Amount, string? Currency, string? Token, string? Description);
public record RefundRequest(string TransactionId, decimal Amount, string? Reason);
public record TokenizeRequest(string? CardNumber, int ExpiryMonth, int ExpiryYear);
