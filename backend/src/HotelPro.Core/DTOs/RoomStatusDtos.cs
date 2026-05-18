using HotelPro.Core.Enums;

namespace HotelPro.Core.DTOs;

public record RoomStatusDetailDto(
    Guid RoomId,
    string RoomNumber,
    RoomStatus Status,
    RoomStatus BaseStatus,
    string Reason,
    bool IsOverride
);
