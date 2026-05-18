using HotelPro.Core.Enums;

namespace HotelPro.Core.DTOs;

public record StayCheckInRequest(
    Guid RoomId,
    List<StayGuestEntry> Guests,
    DateTime? CheckInDate = null,
    DateTime? CheckOutDate = null,
    Guid? BookingId = null,
    Guid? CheckedInBy = null
);

public record StayGuestEntry(
    Guid GuestId,
    GuestCategory GuestCategory = GuestCategory.Unknown,
    decimal DiscountPercent = 0,
    string? DiscountReason = null,
    int TaxOverride = 0,
    string? StayNote = null,
    List<GuestDocumentDto>? Documents = null
);

public record ReservationCheckInRequest(
    Guid BookingId,
    Guid? CheckedInBy = null
);

public record StayCheckInResponse(
    Guid RoomId,
    string RoomNumber,
    Guid FolioId,
    string FolioNumber,
    List<StayGuestResult> Guests,
    List<string> Warnings
);

public record StayGuestResult(
    Guid StayId,
    Guid GuestId,
    string GuestName,
    GuestCategory GuestCategory,
    decimal DiscountPercent,
    int NightsCreated
);

public record StayDto(
    Guid Id,
    Guid GuestId,
    string GuestName,
    Guid RoomId,
    string RoomNumber,
    Guid? FolioId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    bool IsCheckedOut,
    GuestCategory GuestCategory,
    decimal DiscountPercent,
    string? DiscountReason
);

public record StayNightDto(
    Guid Id,
    Guid FolioId,
    Guid? StayId,
    Guid RoomId,
    string RoomNumber,
    DateTime Date,
    decimal TariffAmount,
    decimal DiscountPercent,
    NightStatus Status,
    bool IsComp,
    string? Description,
    DateTime? ClosedAt
);
