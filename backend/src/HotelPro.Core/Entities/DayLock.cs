namespace HotelPro.Core.Entities;

public class DayLock
{
    public Guid Id { get; set; }
    public DateTime LockedDate { get; set; }
    public Guid? LockedById { get; set; }
    public DateTime LockedAt { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public string? UnlockReason { get; set; }
    public Guid? UnlockedById { get; set; }
}
