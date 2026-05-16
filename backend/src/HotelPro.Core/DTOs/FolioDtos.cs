using HotelPro.Core.Enums;

namespace HotelPro.Core.DTOs;

public record FolioDto(
    Guid Id,
    string FolioNumber,
    Guid? BookingId,
    Guid? GuestId,
    string GuestName,
    string Status,
    decimal Balance,
    DateTime CreatedAt,
    DateTime? ClosedAt,
    string? Notes,
    List<FolioChargeDto> Charges,
    List<FolioStayNightDto> StayNights
);

public record FolioChargeDto(
    Guid Id,
    Guid FolioId,
    string ChargeType,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    DateTime ChargeDate,
    string? POSReference
);

public record FolioStayNightDto(
    Guid Id,
    Guid FolioId,
    DateTime Date,
    decimal RoomPrice,
    bool IsComp,
    string? Notes
);

public record CreateFolioChargeDto(
    string ChargeType,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    DateTime ChargeDate,
    string? POSReference
);

public record CreateSubFolioDto(
    Guid BookingId,
    Guid GuestId,
    string? Notes
);

public record CreateFolioDto(
    Guid BookingId,
    Guid GuestId,
    string? Notes
);
