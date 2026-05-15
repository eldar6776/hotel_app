using HotelPro.Core.Entities;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HotelPro.Infrastructure.Services;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly HotelProDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public FeatureFlagService(HotelProDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<bool> IsEnabledAsync(string featureName, Guid? hotelId = null)
    {
        var cacheKey = $"feature_{featureName}_{hotelId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var flag = await _dbContext.FeatureFlags
                .FirstOrDefaultAsync(f => f.FeatureName == featureName && f.HotelId == hotelId);

            if (flag == null)
            {
                flag = await _dbContext.FeatureFlags
                    .FirstOrDefaultAsync(f => f.FeatureName == featureName && f.HotelId == null);
            }

            if (flag == null) return false;
            if (!flag.IsEnabled) return false;
            if (flag.RolloutPercentage >= 100) return true;
            if (flag.RolloutPercentage <= 0) return false;

            return hotelId.HasValue && GetDeterministicHash(hotelId.Value) % 100 < flag.RolloutPercentage;
        });
    }

    public async Task EnableAsync(string featureName, Guid? hotelId = null)
    {
        var flag = await GetOrCreateFlagAsync(featureName, hotelId);
        flag.IsEnabled = true;
        flag.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        ClearCache(featureName, hotelId);
    }

    public async Task DisableAsync(string featureName, Guid? hotelId = null)
    {
        var flag = await GetOrCreateFlagAsync(featureName, hotelId);
        flag.IsEnabled = false;
        flag.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        ClearCache(featureName, hotelId);
    }

    public async Task SetRolloutAsync(string featureName, int percentage, Guid? hotelId = null)
    {
        var flag = await GetOrCreateFlagAsync(featureName, hotelId);
        flag.RolloutPercentage = Math.Clamp(percentage, 0, 100);
        flag.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        ClearCache(featureName, hotelId);
    }

    private async Task<FeatureFlag> GetOrCreateFlagAsync(string featureName, Guid? hotelId)
    {
        var flag = await _dbContext.FeatureFlags
            .FirstOrDefaultAsync(f => f.FeatureName == featureName && f.HotelId == hotelId);

        if (flag == null)
        {
            flag = new FeatureFlag
            {
                Id = Guid.NewGuid(),
                FeatureName = featureName,
                HotelId = hotelId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.FeatureFlags.Add(flag);
        }

        return flag;
    }

    private void ClearCache(string featureName, Guid? hotelId)
    {
        _cache.Remove($"feature_{featureName}_{hotelId}");
        _cache.Remove($"feature_{featureName}_");
    }

    private static int GetDeterministicHash(Guid guid)
    {
        var bytes = guid.ToByteArray();
        int hash = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            hash = (hash * 31) + bytes[i];
        }
        return Math.Abs(hash);
    }
}
