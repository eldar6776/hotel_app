namespace HotelPro.Core.DTOs;

public record GuestDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? Gender,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? PostalCode,
    Guid? CountryId,
    string? CountryName,
    int? NationalityCountryId,
    bool IsCompany,
    string? CompanyName,
    string? VatNumber,
    bool GdprConsentGiven,
    DateTime? GdprConsentDate,
    string? GdprConsentVersion,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<GuestDocumentDetailDto> Documents
);

public record GuestDocumentDetailDto(
    Guid Id,
    Guid GuestId,
    string DocumentType,
    string DocumentNumber,
    string IssuingCountry,
    string? MRZLine1,
    string? MRZLine2,
    DateTime? IssueDate,
    DateTime? ExpiryDate,
    string? FrontImagePath,
    string? BackImagePath,
    bool IsVerified,
    DateTime CreatedAt
);

public record CreateGuestDto(
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? Gender,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? PostalCode,
    Guid? CountryId,
    int? NationalityCountryId,
    bool IsCompany,
    string? CompanyName,
    string? VatNumber,
    bool GdprConsentGiven,
    string? GdprConsentVersion,
    string? Notes
);

public record UpdateGuestDto(
    string? FirstName,
    string? LastName,
    DateTime? DateOfBirth,
    string? Gender,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? PostalCode,
    Guid? CountryId,
    int? NationalityCountryId,
    bool? IsCompany,
    string? CompanyName,
    string? VatNumber,
    bool? GdprConsentGiven,
    string? GdprConsentVersion,
    string? Notes
);

public record GuestAutoSuggestDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Phone
);

public record GuestFilter(
    string? Search,
    string? DocumentNumber,
    int Page = 1,
    int PageSize = 20
);

public record AdvancedGuestFilter(
    string? FirstName,
    string? LastName,
    string? Phone,
    string? Email,
    string? DocumentNumber,
    Guid? CountryId,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20
);

public record GuestProfileDto(
    GuestDto Guest,
    List<GuestStaySummaryDto> StayHistory,
    List<GuestBookingSummaryDto> BookingHistory,
    int TotalStays,
    decimal TotalSpent
);

public record GuestStaySummaryDto(
    Guid BookingId,
    DateTime CheckedInAt,
    DateTime? CheckedOutAt,
    string RoomNumber,
    int Nights
);

public record GuestBookingSummaryDto(
    Guid BookingId,
    DateTime Arrival,
    DateTime Departure,
    string Status,
    int Nights,
    decimal TotalPrice,
    string RoomTypeName
);
