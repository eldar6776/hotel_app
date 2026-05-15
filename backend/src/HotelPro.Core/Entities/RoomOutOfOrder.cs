namespace HotelPro.Core.Entities;

public class RoomOutOfOrder
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public Guid CreatedById { get; set; }
    public Guid? ResolvedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }

    public Room Room { get; set; } = null!;
}
