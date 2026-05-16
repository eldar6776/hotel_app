using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IGuestService
{
    Task<PagedResult<GuestDto>> GetGuestsAsync(GuestFilter filter);
    Task<GuestDto?> GetGuestByIdAsync(Guid id);
    Task<GuestDto> CreateGuestAsync(CreateGuestDto dto);
    Task<GuestDto> UpdateGuestAsync(Guid id, UpdateGuestDto dto);
    Task DeleteGuestAsync(Guid id);
    Task<List<GuestAutoSuggestDto>> SearchGuestsAsync(string query, int limit = 10);
}
