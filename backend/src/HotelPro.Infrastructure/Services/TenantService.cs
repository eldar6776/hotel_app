using HotelPro.Core.Entities;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HotelPro.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly HotelProDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public TenantService(HotelProDbContext dbContext, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsMultiTenant => true;

    public async Task<Hotel?> ResolveByCodeAsync(string code)
    {
        var cacheKey = $"hotel_{code}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _dbContext.Hotels
                .FirstOrDefaultAsync(h => h.Code == code && h.IsActive);
        });
    }

    public async Task<Hotel?> GetCurrentHotelAsync()
    {
        var hotelId = GetCurrentHotelId();
        if (!hotelId.HasValue) return null;

        return await _dbContext.Hotels.FindAsync(hotelId.Value);
    }

    public Guid? GetCurrentHotelId()
    {
        if (_httpContextAccessor.HttpContext?.Items.TryGetValue("HotelId", out var hotelIdObj) == true)
        {
            return hotelIdObj as Guid?;
        }
        return null;
    }
}
