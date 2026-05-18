using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IReservationPolicyService
{
    Task<ReservationResultDto> ConfirmReservationAsync(ConfirmReservationRequest request);
    Task<ReservationResultDto> CancelReservationAsync(CancelReservationRequest request);
    Task<ReservationResultDto> MarkNoShowAsync(MarkNoShowRequest request);
    Task<ReservationResultDto> GetReservationStatusAsync(Guid bookingId);
}
