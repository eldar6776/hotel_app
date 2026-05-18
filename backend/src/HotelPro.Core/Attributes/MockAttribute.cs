namespace HotelPro.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class MockAttribute : Attribute
{
    public string? Reason { get; }

    public MockAttribute(string? reason = null)
    {
        Reason = reason;
    }
}
