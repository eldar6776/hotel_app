namespace HotelPro.Core.DTOs;

public record InvoiceDetailDto(
    Guid Id,
    string InvoiceNumber,
    DateTime IssueDate,
    string GuestName,
    string? GuestAddress,
    decimal Subtotal,
    decimal VatAmount,
    decimal TotalAmount,
    string Currency,
    string Status,
    bool IsFiscalized,
    string? FiscalCode,
    List<InvoiceItemDto> Items
);

public record InvoiceItemDto(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    decimal VatRate,
    decimal VatAmount
);

public record CreateInvoiceRequest(
    Guid BookingId,
    string? Currency
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
    string? Reference
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

public record StornoRequest(
    string Reason,
    string? Description
);
