using HotelPro.Core.Enums;

namespace HotelPro.Core.Enums;

public static class RoomStatusTransitions
{
    private static readonly Dictionary<RoomStatus, HashSet<RoomStatus>> AllowedTransitions = new()
    {
        [RoomStatus.Free] = new() { RoomStatus.Reserved, RoomStatus.ReservedConfirmed, RoomStatus.ReservedUnconfirmed, RoomStatus.Occupied, RoomStatus.Dirty, RoomStatus.OutOfOrder, RoomStatus.OutOfService },
        [RoomStatus.Reserved] = new() { RoomStatus.Occupied, RoomStatus.OccupiedReserved, RoomStatus.Free, RoomStatus.OutOfOrder },
        [RoomStatus.ReservedConfirmed] = new() { RoomStatus.Occupied, RoomStatus.OccupiedReserved, RoomStatus.Free, RoomStatus.OutOfOrder },
        [RoomStatus.ReservedUnconfirmed] = new() { RoomStatus.ReservedConfirmed, RoomStatus.Free, RoomStatus.OutOfOrder },
        [RoomStatus.Occupied] = new() { RoomStatus.Departing, RoomStatus.Dirty, RoomStatus.OutOfOrder },
        [RoomStatus.Departing] = new() { RoomStatus.Dirty, RoomStatus.OutOfOrder },
        [RoomStatus.OccupiedReserved] = new() { RoomStatus.Departing, RoomStatus.Dirty, RoomStatus.OutOfOrder },
        [RoomStatus.Dirty] = new() { RoomStatus.Free, RoomStatus.OutOfOrder },
        [RoomStatus.OutOfOrder] = new() { RoomStatus.Free, RoomStatus.Dirty, RoomStatus.OutOfService },
        [RoomStatus.OutOfService] = new() { RoomStatus.Free },
    };

    public static bool IsValid(RoomStatus from, RoomStatus to)
        => AllowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
}
