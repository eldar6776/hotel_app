using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IConfigurationService
{
    Task<IReadOnlyList<HotelConfigDto>> GetAllAsync(bool includeSecrets = false);
    Task<IReadOnlyList<HotelConfigDto>> GetByCategoryAsync(string category, bool includeSecrets = false);
    Task<HotelConfigDto?> GetByKeyAsync(string key, bool includeSecrets = false);
    Task<string?> GetValueAsync(string key);
    Task<bool> IsEnabledAsync(string key);
    Task<HotelConfigDto> UpdateAsync(string key, UpdateHotelConfigDto dto);
    Task<IReadOnlyDictionary<string, object?>> GetPublicSettingsAsync();
    Task EnsureDefaultsAsync();
}
