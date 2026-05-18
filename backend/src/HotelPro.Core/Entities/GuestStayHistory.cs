namespace HotelPro.Core.Entities;

public class GuestStayHistory
{
    public Guid Id { get; set; }
    public Guid GuestId { get; set; }
    public Guid BookingId { get; set; }
    public Guid? StayId { get; set; }
    public Guid RoomId { get; set; }
    public DateTime CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Guest Guest { get; set; } = null!;
    public Booking Booking { get; set; } = null!;
    public Stay? Stay { get; set; }
    public Room Room { get; set; } = null!;
}
