using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IBookingAvailabilityService
{
    Task<AvailabilityResult> CheckAvailabilityAsync(AvailabilityRequest request);
    Task<LockResult> LockRoomsAsync(LockRequest request);
    Task ReleaseRoomLockAsync(string lockId);
}
