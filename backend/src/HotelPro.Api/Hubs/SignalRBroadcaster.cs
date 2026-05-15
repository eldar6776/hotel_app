using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using Microsoft.AspNetCore.SignalR;

namespace HotelPro.Api.Hubs;

public class SignalRBroadcaster : IRoomStatusBroadcaster
{
    private readonly IHubContext<RoomStatusHub> _hubContext;

    public SignalRBroadcaster(IHubContext<RoomStatusHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastStatusChangeAsync(Guid roomId, string roomNumber, RoomStatus oldStatus, RoomStatus newStatus, Guid? hotelId)
    {
        await _hubContext.Clients
            .Group($"hotel_{hotelId}")
            .SendAsync("RoomStatusChanged", new
            {
                RoomId = roomId,
                RoomNumber = roomNumber,
                OldStatus = oldStatus.ToString(),
                NewStatus = newStatus.ToString(),
                Timestamp = DateTime.UtcNow
            });
    }
}
