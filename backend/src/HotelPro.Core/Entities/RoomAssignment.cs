namespace HotelPro.Core.Entities;

public class RoomAssignment
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid? GuestId { get; set; }
    public DateTime ArrivalDate { get; set; }
    public DateTime DepartureDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public Room Room { get; set; } = null!;
    public Guest? Guest { get; set; }
}
