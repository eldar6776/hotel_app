using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IInvoiceWorkflowService
{
    Task<InvoiceResultDto> CreateInvoiceAsync(CreateFolioInvoiceRequest request);
    Task<InvoiceResultDto> StornoInvoiceAsync(StornoInvoiceRequest request);
    Task<InvoiceResultDto> GetInvoiceAsync(Guid invoiceId);
    Task<IEnumerable<InvoiceResultDto>> GetInvoicesForFolioAsync(Guid folioId);
}
