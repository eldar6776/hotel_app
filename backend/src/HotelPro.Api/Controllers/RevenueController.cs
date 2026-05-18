using Asp.Versioning;
using HotelPro.Api.Attributes;
using HotelPro.Core.Attributes;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/revenue"), Authorize]
[Mock("Revenue management currently returns static pricing suggestions.")]
[FeatureGate("RevenueManagement")]
public class RevenueController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlags;

    public RevenueController(IFeatureFlagService featureFlags)
    {
        _featureFlags = featureFlags;
    }

    [HttpGet("pricing/suggestions"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> PricingSuggestions([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var unavailable = await GetMockUnavailableAsync("RevenueManagement");
        if (unavailable != null) return unavailable;

        return Ok(new
        {
            from = from ?? DateTime.UtcNow,
            to = to ?? DateTime.UtcNow.AddDays(30),
            suggestions = new[]
            {
                new { date = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"), recommendedPrice = 120m, currentPrice = 100m, demandLevel = "High", occupancyForecast = 85 },
                new { date = DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd"), recommendedPrice = 95m, currentPrice = 100m, demandLevel = "Medium", occupancyForecast = 60 }
            }
        });
    }

    [HttpGet("forecast"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> OccupancyForecast([FromQuery] int days = 30)
    {
        var unavailable = await GetMockUnavailableAsync("RevenueManagement");
        if (unavailable != null) return unavailable;

        return Ok(new
        {
            forecastDate = DateTime.UtcNow,
            days,
            data = Enumerable.Range(0, days).Select(d => new
            {
                date = DateTime.UtcNow.AddDays(d).ToString("yyyy-MM-dd"),
                predictedOccupancy = 50 + Random.Shared.Next(30),
                predictedAdr = 80 + Random.Shared.Next(40)
            })
        });
    }

    [HttpGet("competitor-analysis"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> CompetitorAnalysis()
    {
        var unavailable = await GetMockUnavailableAsync("RevenueManagement");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "not_configured", message = "Competitor analysis requires external API configuration" });
    }

    [HttpPost("seasonal-rules"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> CreateSeasonalRule([FromBody] object rule)
    {
        var unavailable = await GetMockUnavailableAsync("RevenueManagement");
        if (unavailable != null) return unavailable;

        return Ok(new { status = "saved", message = "Seasonal rule created" });
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
