using HotelPro.Core.DTOs;
using HotelPro.Core.Enums;

namespace HotelPro.Core.Services;

public interface IRoomService
{
    Task<PagedResult<RoomDto>> GetRoomsAsync(RoomFilter filter);
    Task<RoomDto?> GetRoomByIdAsync(Guid id);
    Task<RoomStatusDetailDto?> GetRoomStatusAsync(Guid id, DateTime? date = null);
    Task<RoomDto> CreateRoomAsync(CreateRoomDto dto);
    Task<RoomDto> UpdateRoomAsync(Guid id, UpdateRoomDto dto);
    Task UpdateRoomStatusAsync(Guid id, RoomStatus newStatus);
    Task DeleteRoomAsync(Guid id);
}
