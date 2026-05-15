using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class HousekeepingLog
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid EmployeeId { get; set; }
    public HousekeepingAction Action { get; set; }
    public HousekeepingStatus Status { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public bool IsVerified { get; set; }
    public Guid? VerifiedById { get; set; }

    public Room Room { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public Employee? VerifiedBy { get; set; }
}
