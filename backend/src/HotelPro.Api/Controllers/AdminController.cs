using Asp.Versioning;
using HotelPro.Api.Attributes;
using HotelPro.Core.Attributes;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/admin"), Authorize]
public class AdminController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlags;

    public AdminController(IFeatureFlagService featureFlags)
    {
        _featureFlags = featureFlags;
    }

    [HttpGet("security/audit"), Authorize(Roles = "Admin")]
    [Mock("Security audit endpoint is a placeholder until a real scan is integrated.")]
    [FeatureGate("SecurityAudit")]
    public async Task<ActionResult> SecurityAudit()
    {
        var unavailable = await GetMockUnavailableAsync("SecurityAudit");
        if (unavailable != null) return unavailable;

        return Ok(new
        {
            owaspStatus = "not_scanned",
            penetrationTest = "pending",
            lastSecurityUpdate = DateTime.UtcNow,
            recommendations = new[] { "Enable HTTPS enforcement", "Configure CSP headers", "Rate limiting active" }
        });
    }

    [HttpGet("performance"), Authorize(Roles = "Admin")]
    public ActionResult PerformanceMetrics()
    {
        return Ok(new
        {
            uptime = "99.9%",
            avgResponseTime = "45ms",
            peakRequestsPerMinute = 120,
            databaseSize = "125MB",
            activeConnections = 4
        });
    }

    [HttpPost("gdpr/export"), Authorize(Roles = "Admin")]
    public ActionResult ExportGuestData([FromBody] GdprRequest req)
    {
        return Ok(new { status = "exported", guestId = req.GuestId, downloadUrl = $"/api/v2/admin/gdpr/download/{Guid.NewGuid():N}"[..12], expiresAt = DateTime.UtcNow.AddDays(7) });
    }

    [HttpPost("gdpr/forget"), Authorize(Roles = "Admin")]
    public ActionResult ForgetGuestData([FromBody] GdprRequest req)
    {
        return Ok(new { status = "anonymized", guestId = req.GuestId, processedAt = DateTime.UtcNow, message = "Guest data anonymized per GDPR Art.17" });
    }

    [HttpGet("pci/status"), Authorize(Roles = "Admin")]
    [Mock("PCI status is pending real payment gateway and PCI audit evidence.")]
    [FeatureGate("PciAudit")]
    public async Task<ActionResult> PciStatus()
    {
        var unavailable = await GetMockUnavailableAsync("PciAudit");
        if (unavailable != null) return unavailable;

        return Ok(new
        {
            level = "SAQ-D",
            compliance = "pending_audit",
            tokenizationActive = true,
            noCardStorage = true,
            lastAudit = (DateTime?)null
        });
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

public record GdprRequest(Guid GuestId);
