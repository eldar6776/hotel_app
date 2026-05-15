namespace HotelPro.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AuditableAttribute : Attribute
{
    public bool TrackChanges { get; set; } = true;
    public string[]? ExcludeProperties { get; set; }
}
