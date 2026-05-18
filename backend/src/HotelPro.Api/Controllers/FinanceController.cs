using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}"), Authorize]
public class FinanceController : ControllerBase
{
    private readonly IProformaService _proforma;
    private readonly IAdvancePaymentService _advancePayment;
    private readonly IExchangeRateService _exchangeRate;

    public FinanceController(IProformaService p, IAdvancePaymentService a, IExchangeRateService e)
    { _proforma = p; _advancePayment = a; _exchangeRate = e; }

    [HttpPost("proforma"), Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<ProformaInvoiceDto>> CreateProforma([FromBody] CreateInvoiceRequest req)
    {
        var result = await _proforma.CreateProformaAsync(req.BookingId);
        return Ok(result);
    }

    [HttpGet("proforma/{bookingId:guid}"), Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<ProformaInvoiceDto>> GetProforma(Guid bookingId)
    {
        var result = await _proforma.GetProformaAsync(bookingId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("proforma/{id:guid}/convert"), Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<InvoiceDetailDto>> ConvertProforma(Guid id)
    {
        var result = await _proforma.ConvertToInvoiceAsync(id);
        return Ok(result);
    }

    [HttpPost("advance-payments"), Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<AdvancePaymentDto>> AddAdvancePayment(CreateAdvancePaymentDto dto)
    {
        var result = await _advancePayment.AddAdvancePaymentAsync(dto);
        return Ok(result);
    }

    [HttpGet("advance-payments/{bookingId:guid}"), Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<List<AdvancePaymentDto>>> GetAdvancePayments(Guid bookingId)
    {
        return Ok(await _advancePayment.GetAdvancePaymentsAsync(bookingId));
    }

    [HttpPost("advance-payments/{id:guid}/refund"), Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> RefundAdvance(Guid id)
    {
        await _advancePayment.RefundAdvancePaymentAsync(id);
        return NoContent();
    }

    [HttpGet("exchange-rates"), Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<List<ExchangeRateDto>>> GetRates()
    {
        return Ok(await _exchangeRate.GetCurrentRatesAsync());
    }

    [HttpPost("exchange-rates"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> UpdateRate([FromBody] ExchangeRateDto dto)
    {
        await _exchangeRate.UpdateRateAsync(dto.CurrencyCode, dto.Rate);
        return NoContent();
    }

    [HttpPost("exchange-rates/sync"), Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> SyncRates([FromQuery] string? baseCurrency = null)
    {
        var count = await _exchangeRate.SyncRatesFromExternalApiAsync(baseCurrency);
        return Ok(new { syncedCount = count, source = "Frankfurter", timestamp = DateTime.UtcNow });
    }

    [HttpGet("exchange-rates/convert"), Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> ConvertAmount(
        [FromQuery] decimal amount,
        [FromQuery] string from,
        [FromQuery] string to)
    {
        var result = await _exchangeRate.ConvertAsync(amount, from, to);
        return Ok(new { amount, fromCurrency = from, toCurrency = to, convertedAmount = result });
    }
}
