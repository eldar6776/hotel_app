namespace HotelPro.Core.DTOs;

public record CheckInRequest(
    Guid BookingId,
    Guid RoomId,
    List<GuestDocumentDto>? GuestDocuments,
    string? RfidCardCode
);

public record GuestDocumentDto(
    string DocumentType,
    string DocumentNumber,
    DateTime? ExpiryDate
);

public record CheckInResponse(
    Guid BookingId,
    Guid RoomId,
    string RoomNumber,
    Guid FolioId,
    string FolioNumber,
    List<string> Warnings,
    string? RfidEncodeUrl
);

public record CheckOutRequest(
    Guid BookingId,
    string PaymentMethod,
    string? PaymentReference,
    bool LateCheckout,
    bool ApplyDiscounts
);

public record CheckOutResponse(
    Guid BookingId,
    decimal TotalAmount,
    decimal StayCharges,
    decimal FolioCharges,
    decimal LateCheckoutFee,
    decimal DiscountAmount,
    string PaymentMethod,
    string FolioNumber
);
