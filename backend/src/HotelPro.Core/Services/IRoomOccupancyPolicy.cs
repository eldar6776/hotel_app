using HotelPro.Core.DTOs;
using HotelPro.Core.Enums;

namespace HotelPro.Core.Services;

public interface IRoomOccupancyPolicy
{
    Task<RoomStatusDetailDto> GetRoomStatusAsync(Guid roomId, DateTime date);
    Task<IReadOnlyDictionary<Guid, RoomStatusDetailDto>> GetRoomStatusForAllRoomsAsync(DateTime date);
}
