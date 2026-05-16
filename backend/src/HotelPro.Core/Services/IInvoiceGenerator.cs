using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IInvoiceGenerator
{
    Task<InvoiceDetailDto> GenerateInvoiceAsync(Guid bookingId, string? currency = null);
    Task<byte[]> GenerateInvoicePdfAsync(Guid invoiceId);
    Task<string> GetNextInvoiceNumberAsync();
    Task<InvoiceDetailDto> StornoInvoiceAsync(Guid invoiceId, string reason, string? description);
    Task<InvoiceDetailDto?> GetInvoiceAsync(Guid invoiceId);
}
