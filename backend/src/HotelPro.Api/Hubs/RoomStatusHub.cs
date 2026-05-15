using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HotelPro.Api.Hubs;

[Authorize]
public class RoomStatusHub : Hub
{
    public async Task JoinHotelGroup(string hotelCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"hotel_{hotelCode}");
    }

    public async Task LeaveHotelGroup(string hotelCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"hotel_{hotelCode}");
    }

    public override async Task OnConnectedAsync()
    {
        var hotelId = Context.User?.FindFirst("hotelId")?.Value;
        if (hotelId != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"hotel_{hotelId}");
        await base.OnConnectedAsync();
    }
}
