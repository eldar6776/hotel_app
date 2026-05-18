using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface ICheckOutWorkflowService
{
    Task<CheckOutWorkflowResponse> FullCheckOutAsync(FullCheckOutRequest request);
    Task<PartialCheckOutResponse> PartialCheckOutAsync(PartialCheckOutRequest request);
}

public record FullCheckOutRequest(
    Guid RoomId,
    Guid? CheckedOutBy = null,
    bool CreateUnpaidRecords = true
);

public record PartialCheckOutRequest(
    Guid StayId,
    Guid? CheckedOutBy = null
);

public record CheckOutWorkflowResponse(
    Guid RoomId,
    string RoomNumber,
    Guid FolioId,
    string FolioNumber,
    int GuestsCheckedOut,
    int NightsClosed,
    int ExpensesLocked,
    bool FolioClosed,
    bool HasUnpaidBalance,
    decimal OutstandingAmount
);

public record PartialCheckOutResponse(
    Guid StayId,
    Guid GuestId,
    string GuestName,
    Guid FolioId,
    int NightsClosedForGuest,
    int NightsCreatedForRemaining,
    bool FolioStillOpen,
    int RemainingGuests
);
