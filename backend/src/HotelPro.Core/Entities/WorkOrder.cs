using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class WorkOrder
{
    public Guid Id { get; set; }
    public Guid? RoomId { get; set; }
    public Guid ReportedById { get; set; }
    public Guid? AssignedToId { get; set; }
    public WorkOrderPriority Priority { get; set; }
    public WorkOrderCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public WorkOrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }

    public Room? Room { get; set; }
    public Employee ReportedBy { get; set; } = null!;
    public Employee? AssignedTo { get; set; }
}
