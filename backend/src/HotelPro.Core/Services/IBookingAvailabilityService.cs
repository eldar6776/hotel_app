using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IBookingAvailabilityService
{
    Task<AvailabilityResult> CheckAvailabilityAsync(AvailabilityRequest request);
    Task<LockResult> LockRoomsAsync(LockRequest request);
    Task<LockResult> ExecuteWithLockAsync(LockRequest request, Func<Task<BookingResult>> action);
    Task ReleaseRoomLockAsync(string lockId);
}

public record BookingResult(Guid BookingId);
