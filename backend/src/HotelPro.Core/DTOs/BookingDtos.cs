using HotelPro.Core.Enums;

namespace HotelPro.Core.DTOs;

public record BookingRoomDto(
    Guid Id,
    Guid BookingId,
    Guid? RoomId,
    string? RoomNumber,
    Guid RoomTypeId,
    string RoomTypeName,
    Guid RatePlanId,
    decimal PricePerNight,
    string Status
);

public record BookingDto(
    Guid Id,
    Guid HotelId,
    Guid GuestId,
    string GuestName,
    Guid? GroupId,
    string Source,
    string Type,
    string Status,
    DateTime ArrivalDate,
    DateTime DepartureDate,
    int AdultCount,
    int ChildCount,
    int Nights,
    decimal TotalPrice,
    decimal ExchangeRateTotal,
    string Currency,
    string? Notes,
    string? InternalNotes,
    string? CancellationReason,
    DateTime? CancelledAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<BookingRoomDto> Rooms
);

public record CreateBookingRoomDto(
    Guid? RoomId,
    Guid RoomTypeId,
    Guid RatePlanId,
    decimal PricePerNight
);

public record CreateBookingDto(
    Guid GuestId,
    Guid? GroupId,
    BookingSource Source,
    BookingType Type,
    DateTime ArrivalDate,
    DateTime DepartureDate,
    int AdultCount,
    int ChildCount,
    string? Notes,
    string? InternalNotes,
    List<CreateBookingRoomDto> Rooms
);

public record UpdateBookingDto(
    Guid? GuestId,
    Guid? GroupId,
    BookingSource? Source,
    BookingType? Type,
    DateTime? ArrivalDate,
    DateTime? DepartureDate,
    int? AdultCount,
    int? ChildCount,
    string? Notes,
    string? InternalNotes,
    List<CreateBookingRoomDto>? Rooms
);

public record BookingFilter(
    List<BookingStatus>? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? GuestId = null,
    Guid? RoomId = null,
    int Page = 1,
    int PageSize = 20
);

public record AssignRoomDto(Guid? RoomId);
