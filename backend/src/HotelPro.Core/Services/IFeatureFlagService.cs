namespace HotelPro.Core.Services;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName, Guid? hotelId = null);
    Task EnableAsync(string featureName, Guid? hotelId = null);
    Task DisableAsync(string featureName, Guid? hotelId = null);
    Task SetRolloutAsync(string featureName, int percentage, Guid? hotelId = null);
}
