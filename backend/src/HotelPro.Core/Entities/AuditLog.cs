namespace HotelPro.Core.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedProperties { get; set; }
    public Guid? ChangedById { get; set; }
    public string? ChangedByEmail { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ChangedAt { get; set; }
}
