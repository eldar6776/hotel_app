using Asp.Versioning;
using HotelPro.Api.Attributes;
using HotelPro.Core.Attributes;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/payments"), Authorize]
[Mock("Payment gateway requires Stripe or provider credentials.")]
[FeatureGate("PaymentGateway")]
public class PaymentGatewayController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlags;

    public PaymentGatewayController(IFeatureFlagService featureFlags)
    {
        _featureFlags = featureFlags;
    }

    [HttpPost("charge"), Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> ChargeCard([FromBody] ChargeRequest req)
    {
        var unavailable = await GetMockUnavailableAsync("PaymentGateway");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "success", transactionId = $"TXN-{Guid.NewGuid():N}"[..16], amount = req.Amount, currency = req.Currency ?? "EUR", timestamp = DateTime.UtcNow });
    }

    [HttpPost("refund"), Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> Refund([FromBody] RefundRequest req)
    {
        var unavailable = await GetMockUnavailableAsync("PaymentGateway");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "success", refundId = $"REF-{Guid.NewGuid():N}"[..16], amount = req.Amount, originalTransactionId = req.TransactionId });
    }

    [HttpPost("tokenize"), Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> TokenizeCard([FromBody] TokenizeRequest req)
    {
        var unavailable = await GetMockUnavailableAsync("PaymentGateway");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "success", token = $"tok_{Guid.NewGuid():N}"[..24], last4 = req.CardNumber?[^4..] ?? "****", expiryMonth = req.ExpiryMonth, expiryYear = req.ExpiryYear });
    }

    [HttpGet("webhook/events"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> RecentWebhooks()
    {
        var unavailable = await GetMockUnavailableAsync("PaymentGateway");
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

public record ChargeRequest(decimal Amount, string? Currency, string? Token, string? Description);
public record RefundRequest(string TransactionId, decimal Amount, string? Reason);
public record TokenizeRequest(string? CardNumber, int ExpiryMonth, int ExpiryYear);
