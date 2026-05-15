using HotelPro.Core.Entities;

namespace HotelPro.Core.Services;

public interface ITenantService
{
    Task<Hotel?> ResolveByCodeAsync(string code);
    Task<Hotel?> GetCurrentHotelAsync();
    Guid? GetCurrentHotelId();
    bool IsMultiTenant { get; }
}
