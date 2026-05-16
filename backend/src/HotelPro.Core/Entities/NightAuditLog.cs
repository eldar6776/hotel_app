using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class NightAuditLog
{
    public Guid Id { get; set; }
    public DateTime AuditDate { get; set; }
    public DateTime RanAt { get; set; }
    public NightAuditStatus Status { get; set; }
    public int BookingsProcessed { get; set; }
    public decimal TotalStayCharges { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum NightAuditStatus
{
    Success,
    Failed,
    Skipped
}
