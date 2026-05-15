using HotelPro.Core.Enums;

namespace HotelPro.Core.DTOs;

public record RoomDto(
    Guid Id,
    string RoomNumber,
    int Floor,
    Guid BuildingId,
    string BuildingName,
    Guid RoomTypeId,
    string RoomTypeName,
    string Status,
    int BaseCapacity,
    int MaxCapacity,
    decimal? BasePrice,
    string? Notes
);

public record CreateRoomDto(
    string RoomNumber,
    int Floor,
    Guid BuildingId,
    Guid RoomTypeId,
    decimal? BasePrice,
    string? Notes
);

public record UpdateRoomDto(
    string? RoomNumber,
    int? Floor,
    Guid? BuildingId,
    Guid? RoomTypeId,
    RoomStatus? Status,
    decimal? BasePrice,
    string? Notes
);

public record RoomFilter(
    List<RoomStatus>? Status = null,
    Guid? BuildingId = null,
    Guid? RoomTypeId = null,
    int? Floor = null,
    string? Search = null,
    bool IncludeInactive = false,
    int Page = 1,
    int PageSize = 20
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record RoomTypeDto(
    Guid Id,
    string Name,
    string Code,
    int BaseCapacity,
    int MaxCapacity,
    decimal DefaultPrice,
    string? Description,
    bool IsActive
);

public record CreateRoomTypeDto(
    string Name,
    string Code,
    int BaseCapacity,
    int MaxCapacity,
    decimal DefaultPrice,
    string? Description
);

public record UpdateRoomTypeDto(
    string? Name,
    string? Code,
    int? BaseCapacity,
    int? MaxCapacity,
    decimal? DefaultPrice,
    string? Description,
    bool? IsActive
);

public record BuildingDto(
    Guid Id,
    string Name,
    string Code,
    string? Address,
    string? City,
    bool IsActive
);

public record CreateBuildingDto(
    string Name,
    string Code,
    string? Address,
    string? City
);

public record UpdateBuildingDto(
    string? Name,
    string? Code,
    string? Address,
    string? City,
    bool? IsActive
);

public record TariffDto(
    Guid Id,
    string Name,
    Guid? RoomTypeId,
    string? RoomTypeName,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    decimal BasePrice,
    string Currency,
    bool IsActive
);

public record CreateTariffDto(
    string Name,
    Guid? RoomTypeId,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    decimal BasePrice,
    string Currency
);

public record UpdateTariffDto(
    string? Name,
    Guid? RoomTypeId,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    decimal? BasePrice,
    string? Currency,
    bool? IsActive
);

public record AmenityDto(
    Guid Id,
    string Name,
    string? Icon,
    bool IsActive
);

public record CreateAmenityDto(
    string Name,
    string? Icon
);

public record UpdateAmenityDto(
    string? Name,
    string? Icon,
    bool? IsActive
);

public record RoomOutOfOrderDto(
    Guid Id,
    Guid RoomId,
    string RoomNumber,
    string Reason,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    string Status,
    DateTime CreatedAt,
    string? ResolutionNotes,
    DateTime? ResolvedAt
);

public record CreateOooDto(
    string Reason,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate
);

public record ResolveOooDto(
    string? ResolutionNotes
);
