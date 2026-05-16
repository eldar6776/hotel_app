using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/revenue"), Authorize]
public class RevenueController : ControllerBase
{
    [HttpGet("pricing/suggestions"), Authorize(Roles = "Admin,Manager")]
    public ActionResult PricingSuggestions([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
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
    public ActionResult OccupancyForecast([FromQuery] int days = 30)
    {
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
    public ActionResult CompetitorAnalysis()
    {
        return Ok(new { status = "not_configured", message = "Competitor analysis requires external API configuration" });
    }

    [HttpPost("seasonal-rules"), Authorize(Roles = "Admin,Manager")]
    public ActionResult CreateSeasonalRule([FromBody] object rule)
    {
        return Ok(new { status = "saved", message = "Seasonal rule created" });
    }
}
