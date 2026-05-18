using Asp.Versioning;
using HotelPro.Core.DTOs;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelPro.Api.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentAllocationService _paymentAllocation;

    public PaymentsController(IPaymentAllocationService paymentAllocation)
    {
        _paymentAllocation = paymentAllocation;
    }

    [HttpPost("allocate")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<PaymentAllocationResultDto>> AllocatePayment(PaymentAllocationRequest request)
    {
        try
        {
            var result = await _paymentAllocation.AllocatePaymentAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("reference/{reference}")]
    [Authorize(Policy = "CanManageBookings")]
    public async Task<ActionResult<PaymentAllocationResultDto>> GetByReference(string reference)
    {
        try
        {
            var result = await _paymentAllocation.GetAllocationsForPaymentAsync(reference);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
