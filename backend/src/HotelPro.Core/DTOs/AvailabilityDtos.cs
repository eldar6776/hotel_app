namespace HotelPro.Core.DTOs;

public record AvailabilityRequest(
    Guid RoomTypeId,
    DateTime Arrival,
    DateTime Departure,
    int Quantity = 1,
    Guid? ExcludeBookingId = null
);

public record DateRange(
    DateTime Start,
    DateTime End
);

public record AvailabilityResult(
    bool IsAvailable,
    int AvailableQuantity,
    int TotalRoomsOfType,
    List<DateRange> ConflictingPeriods
);

public record LockRequest(
    Guid RoomTypeId,
    DateTime Arrival,
    DateTime Departure,
    int Quantity = 1
);

public record LockResult(
    bool Success,
    string? LockId,
    string? ErrorMessage
);
