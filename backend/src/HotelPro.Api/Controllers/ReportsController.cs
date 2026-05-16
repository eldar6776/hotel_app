using Asp.Versioning;
using HotelPro.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/reports"), Authorize]
public class ReportsController : ControllerBase
{
    private readonly ReportsService _reports;

    public ReportsController(ReportsService reports) => _reports = reports;

    [HttpGet("daily"), Authorize(Policy = "CanViewReports")]
    public async Task<ActionResult> DailyReport([FromQuery] DateTime? date)
    {
        return Ok(await _reports.GetDailyReportAsync(date ?? DateTime.UtcNow));
    }

    [HttpGet("financial"), Authorize(Policy = "CanViewReports")]
    public async Task<ActionResult> FinancialReport([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        return Ok(await _reports.GetFinancialReportAsync(from, to));
    }

    [HttpGet("guest-book"), Authorize(Policy = "CanViewReports")]
    public async Task<ActionResult> GuestRegistrationBook([FromQuery] DateTime? date)
    {
        return Ok(await _reports.GetGuestRegistrationBookAsync(date ?? DateTime.UtcNow));
    }

    [HttpGet("revenue-by-channel"), Authorize(Policy = "CanViewReports")]
    public async Task<ActionResult> RevenueByChannel([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        return Ok(await _reports.GetRevenueByChannelAsync(from, to));
    }
}
