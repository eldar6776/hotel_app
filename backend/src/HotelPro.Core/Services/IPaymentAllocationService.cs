using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IPaymentAllocationService
{
    Task<PaymentAllocationResultDto> AllocatePaymentAsync(PaymentAllocationRequest request);
    Task<PaymentAllocationResultDto> GetAllocationAsync(Guid allocationId);
    Task<PaymentAllocationResultDto> GetAllocationsForPaymentAsync(string paymentReference);
}
