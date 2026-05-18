using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IStayLifecycleService
{
    Task<StayCheckInResponse> CheckInAsync(StayCheckInRequest request);
    Task<StayCheckInResponse> CheckInFromReservationAsync(ReservationCheckInRequest request);
    Task<IEnumerable<StayDto>> GetActiveStaysForRoomAsync(Guid roomId);
    Task<StayDto> GetStayAsync(Guid stayId);
}
