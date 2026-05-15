namespace HotelPro.Core.Entities;

public class BookingRoom
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid RoomId { get; set; }
    public Guid? GuestId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsMainGuest { get; set; }
    public string Status { get; set; } = string.Empty;

    public Booking Booking { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public Guest? Guest { get; set; }
}
