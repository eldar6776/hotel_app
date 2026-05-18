namespace HotelPro.Core.DTOs;

public record CreateInvoiceRequest(
    Guid BookingId,
    string? Currency = null
);

public record CreateFolioInvoiceRequest(
    Guid FolioId,
    Guid? IssuedBy = null,
    string? Notes = null
);

public record StornoInvoiceRequest(
    Guid InvoiceId,
    string Reason,
    Guid? IssuedBy = null
);

public record StornoRequest(
    string Reason
);

public record InvoiceResultDto(
    Guid Id,
    string InvoiceNumber,
    Guid FolioId,
    string GuestName,
    string RoomNumber,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    decimal SubTotal,
    decimal VatAmount,
    decimal TotalAmount,
    bool IsStorno,
    string? StornoReason,
    DateTime CreatedAt,
    List<InvoiceLineItemDto> LineItems
);

public record InvoiceLineItemDto(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    decimal VatRate,
    decimal VatAmount
);

public record InvoiceItemDto(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    decimal VatRate,
    decimal VatAmount
);

public record InvoiceDetailDto(
    Guid Id,
    string InvoiceNumber,
    DateTime IssueDate,
    string GuestName,
    string? GuestAddress,
    decimal TotalNet,
    decimal TotalVat,
    decimal TotalGross,
    string Currency,
    string Status,
    bool IsStorno,
    string? StornoReason,
    List<InvoiceItemDto> LineItems
);

public record ProformaInvoiceDto(
    Guid Id,
    Guid BookingId,
    string ProformaNumber,
    DateTime IssueDate,
    decimal TotalAmount,
    string Status,
    DateTime? ExpiryDate,
    Guid? ConvertedToInvoiceId
);

public record AdvancePaymentDto(
    Guid Id,
    Guid BookingId,
    decimal Amount,
    DateTime PaymentDate,
    string PaymentMethod,
    string? Reference,
    Guid? AppliedToInvoiceId,
    bool IsRefunded
);

public record CreateAdvancePaymentDto(
    Guid BookingId,
    decimal Amount,
    string PaymentMethod,
    string? Reference = null
);

public record ExchangeRateDto(
    Guid Id,
    string CurrencyCode,
    decimal Rate,
    bool IsLocalCurrency,
    DateTime ValidFrom,
    DateTime? ValidTo,
    string Source
);

public record PaymentAllocationRequest(
    decimal TotalAmount,
    string PaymentMethod,
    List<FolioAllocationEntry> FolioAllocations,
    string? Reference = null,
    Guid? ProcessedById = null,
    string? Notes = null
);

public record FolioAllocationEntry(
    Guid FolioId,
    decimal Amount
);

public record PaymentAllocationResultDto(
    Guid AllocationId,
    string Reference,
    decimal TotalAmount,
    List<FolioPaymentResultDto> FolioPayments
);

public record FolioPaymentResultDto(
    Guid PaymentId,
    Guid FolioId,
    decimal Amount,
    string Status
);

public record ConfirmReservationRequest(
    Guid BookingId,
    Guid? ConfirmedById = null,
    string? Notes = null
);

public record CancelReservationRequest(
    Guid BookingId,
    string Reason,
    Guid? CancelledById = null
);

public record MarkNoShowRequest(
    Guid BookingId,
    Guid? MarkedById = null,
    string? Notes = null
);

public record ReservationResultDto(
    Guid BookingId,
    string Status,
    string? CancellationReason,
    DateTime? CancelledAt,
    List<ReservationAuditEntryDto> AuditTrail
);

public record ReservationAuditEntryDto(
    string Action,
    string? PreviousValue,
    string? NewValue,
    DateTime ChangedAt
);
