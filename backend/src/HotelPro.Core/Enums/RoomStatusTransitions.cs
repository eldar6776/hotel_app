using HotelPro.Core.Enums;

namespace HotelPro.Core.Enums;

public static class RoomStatusTransitions
{
    private static readonly Dictionary<RoomStatus, HashSet<RoomStatus>> AllowedTransitions = new()
    {
        [RoomStatus.Free] = new() { RoomStatus.Reserved, RoomStatus.Occupied, RoomStatus.OutOfOrder, RoomStatus.OutOfService },
        [RoomStatus.Reserved] = new() { RoomStatus.Occupied, RoomStatus.Free, RoomStatus.OutOfOrder },
        [RoomStatus.Occupied] = new() { RoomStatus.Dirty, RoomStatus.OutOfOrder },
        [RoomStatus.Dirty] = new() { RoomStatus.Free, RoomStatus.OutOfOrder },
        [RoomStatus.OutOfOrder] = new() { RoomStatus.Free, RoomStatus.OutOfService },
        [RoomStatus.OutOfService] = new() { RoomStatus.Free },
    };

    public static bool IsValid(RoomStatus from, RoomStatus to)
        => AllowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
}
