namespace HotelPro.Core.Entities;

public class GroupBooking
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid BookingId { get; set; }
    public Guid RoomTypeId { get; set; }
    public DateTime CreatedAt { get; set; }

    public BookingGroup Group { get; set; } = null!;
    public Booking Booking { get; set; } = null!;
    public RoomType RoomType { get; set; } = null!;
}
