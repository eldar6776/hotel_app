using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IProformaService
{
    Task<ProformaInvoiceDto> CreateProformaAsync(Guid bookingId);
    Task<ProformaInvoiceDto?> GetProformaAsync(Guid bookingId);
    Task<InvoiceDetailDto> ConvertToInvoiceAsync(Guid proformaId);
}

public interface IAdvancePaymentService
{
    Task<AdvancePaymentDto> AddAdvancePaymentAsync(CreateAdvancePaymentDto dto);
    Task<List<AdvancePaymentDto>> GetAdvancePaymentsAsync(Guid bookingId);
    Task RefundAdvancePaymentAsync(Guid paymentId);
}
