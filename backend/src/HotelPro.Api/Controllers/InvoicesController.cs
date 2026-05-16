using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController, ApiVersion("2.0"), Route("api/v{version:apiVersion}/invoices"), Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceGenerator _invoiceGenerator;

    public InvoicesController(IInvoiceGenerator invoiceGenerator) => _invoiceGenerator = invoiceGenerator;

    [HttpPost]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<InvoiceDetailDto>> GenerateInvoice(CreateInvoiceRequest req)
    {
        var invoice = await _invoiceGenerator.GenerateInvoiceAsync(req.BookingId, req.Currency);
        return Ok(invoice);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<InvoiceDetailDto>> GetInvoice(Guid id)
    {
        var inv = await _invoiceGenerator.GetInvoiceAsync(id);
        return inv == null ? NotFound() : Ok(inv);
    }

    [HttpGet("{id:guid}/pdf")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult> GetPdf(Guid id)
    {
        var pdf = await _invoiceGenerator.GenerateInvoicePdfAsync(id);
        return File(pdf, "application/pdf", $"invoice-{id}.pdf");
    }

    [HttpPost("{id:guid}/storno")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<InvoiceDetailDto>> Storno(Guid id, StornoRequest req)
    {
        var result = await _invoiceGenerator.StornoInvoiceAsync(id, req.Reason, "");
        return Ok(result);
    }
}
