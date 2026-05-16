namespace HotelPro.Core.DTOs;

public record BookingGroupDto(
    Guid Id,
    Guid HotelId,
    string Name,
    Guid ContactPersonId,
    string ContactPersonName,
    DateTime Arrival,
    DateTime Departure,
    int BlockedRoomCount,
    int ConfirmedRoomCount,
    Guid? RatePlanId,
    string? RatePlanName,
    decimal DiscountPercent,
    DateTime? ReleaseDate,
    string Status,
    bool UseMasterBill,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<BookingGroupBookingDto> Bookings,
    MasterBillDto? MasterBill
);

public record BookingGroupBookingDto(
    Guid BookingId,
    string GuestName,
    Guid RoomTypeId,
    string RoomTypeName,
    string BookingStatus,
    DateTime ArrivalDate,
    DateTime DepartureDate
);

public record MasterBillDto(
    Guid Id,
    Guid GroupId,
    Guid PayerGuestId,
    string PayerGuestName,
    decimal TotalStayCharges,
    bool IsClosed,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateBookingGroupDto(
    string Name,
    Guid ContactPersonId,
    DateTime Arrival,
    DateTime Departure,
    int BlockedRoomCount,
    Guid? RatePlanId,
    decimal DiscountPercent,
    DateTime? ReleaseDate,
    bool UseMasterBill,
    Guid? PayerGuestId,
    List<GroupRoomTypeDto> RoomTypes
);

public record UpdateBookingGroupDto(
    string? Name,
    Guid? ContactPersonId,
    DateTime? Arrival,
    DateTime? Departure,
    Guid? RatePlanId,
    decimal? DiscountPercent,
    DateTime? ReleaseDate,
    bool? UseMasterBill,
    Guid? PayerGuestId
);

public record GroupRoomTypeDto(
    Guid RoomTypeId,
    int Quantity
);

public record BookingGroupFilter(
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20
);

public record PagedGroupResult(
    List<BookingGroupDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
