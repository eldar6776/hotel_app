namespace HotelPro.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RateLimitPolicyAttribute : Attribute
{
    public string PolicyName { get; }
    public RateLimitPolicyAttribute(string policyName) => PolicyName = policyName;
}
