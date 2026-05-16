using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IFolioService
{
    Task<FolioDto> CreateFolioAsync(CreateFolioDto dto);
    Task<FolioDto> CreateSubFolioAsync(CreateSubFolioDto dto);
    Task<FolioDto?> GetFolioByBookingAsync(Guid bookingId);
    Task<List<FolioDto>> GetFoliosByBookingAsync(Guid bookingId);
    Task<FolioChargeDto> AddChargeAsync(Guid folioId, CreateFolioChargeDto dto);
    Task DeleteChargeAsync(Guid chargeId);
    Task<FolioChargeDto> StornoChargeAsync(Guid chargeId, string reason);
    Task CloseFolioAsync(Guid folioId);
    Task<decimal> GetFolioBalanceAsync(Guid folioId);
}
