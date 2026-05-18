using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface INightLedgerService
{
    Task<IEnumerable<StayNightDto>> GetNightsForStayAsync(Guid stayId);
    Task<IEnumerable<StayNightDto>> GetNightsForRoomAsync(Guid roomId, DateTime? date = null);
    Task<IEnumerable<StayNightDto>> GetActiveNightsForFolioAsync(Guid folioId);
    Task<StayNightDto> UpdateNightTariffAsync(Guid nightId, decimal newTariff);
    Task<int> CloseNightsForStayAsync(Guid stayId, DateTime closedAt);
    Task<int> CloseNightsForRoomAsync(Guid roomId, Guid folioId, DateTime closedAt);
    Task<int> GenerateNightsForDateAsync(DateTime date);
    Task<decimal> CalculateNightsTotalAsync(Guid folioId);
}
