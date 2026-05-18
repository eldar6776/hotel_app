namespace HotelPro.Core.DTOs;

public record HotelConfigDto(
    Guid Id,
    string Key,
    string? Value,
    string Category,
    string? Description,
    bool IsSecret,
    bool IsEnabled,
    DateTime UpdatedAt
);

public record UpdateHotelConfigDto(
    string? Value,
    bool? IsEnabled,
    string? Description
);

public record HotelConfigTestResultDto(
    string Status,
    string Message
);
