namespace HotelPro.Core.Entities;

public class FeatureFlag
{
    public Guid Id { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int RolloutPercentage { get; set; } = 100;
    public Guid? HotelId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
