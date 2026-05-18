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
