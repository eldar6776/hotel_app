using HotelPro.Core.Enums;

namespace HotelPro.Core.Services;

public interface IRoomStatusBroadcaster
{
    Task BroadcastStatusChangeAsync(Guid roomId, string roomNumber, RoomStatus oldStatus, RoomStatus newStatus, Guid? hotelId);
}
