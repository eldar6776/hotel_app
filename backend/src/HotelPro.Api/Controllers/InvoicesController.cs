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
    private readonly IInvoiceWorkflowService _invoiceWorkflow;

    public InvoicesController(
        IInvoiceGenerator invoiceGenerator,
        IInvoiceWorkflowService invoiceWorkflow)
    {
        _invoiceGenerator = invoiceGenerator;
        _invoiceWorkflow = invoiceWorkflow;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<InvoiceDetailDto>> GenerateInvoice(CreateInvoiceRequest req)
    {
        var invoice = await _invoiceGenerator.GenerateInvoiceAsync(req.BookingId, req.Currency);
        return Ok(invoice);
    }

    [HttpPost("folio")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<InvoiceResultDto>> CreateFolioInvoice(CreateFolioInvoiceRequest request)
    {
        try
        {
            var result = await _invoiceWorkflow.CreateInvoiceAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<InvoiceDetailDto>> GetInvoice(Guid id)
    {
        var inv = await _invoiceGenerator.GetInvoiceAsync(id);
        return inv == null ? NotFound() : Ok(inv);
    }

    [HttpGet("folio/{folioId:guid}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<IEnumerable<InvoiceResultDto>>> GetInvoicesForFolio(Guid folioId)
    {
        var invoices = await _invoiceWorkflow.GetInvoicesForFolioAsync(folioId);
        return Ok(invoices);
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

    [HttpPost("{id:guid}/storno-workflow")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<InvoiceResultDto>> StornoWorkflow(Guid id, [FromBody] StornoInvoiceWorkflowRequest req)
    {
        try
        {
            var result = await _invoiceWorkflow.StornoInvoiceAsync(new StornoInvoiceRequest(id, req.Reason));
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record StornoInvoiceWorkflowRequest(string Reason);
