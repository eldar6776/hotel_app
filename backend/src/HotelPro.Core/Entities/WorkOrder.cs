namespace HotelPro.Core.Entities;

public class WorkOrder
{
    public Guid Id { get; set; }
    public Guid? RoomId { get; set; }
    public Guid ReportedById { get; set; }
    public Guid? AssignedToId { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }

    public Room? Room { get; set; }
    public Employee ReportedBy { get; set; } = null!;
    public Employee? AssignedTo { get; set; }
}
