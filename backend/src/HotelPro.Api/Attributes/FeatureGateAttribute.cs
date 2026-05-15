namespace HotelPro.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class FeatureGateAttribute : Attribute
{
    public string FeatureName { get; }
    public FeatureGateAttribute(string featureName) => FeatureName = featureName;
}
