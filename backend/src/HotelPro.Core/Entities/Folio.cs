using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class Folio
{
    public Guid Id { get; set; }
    public string FolioNumber { get; set; } = string.Empty;
    public Guid? BookingId { get; set; }
    public Guid? BookingRoomId { get; set; }
    public Guid? GuestId { get; set; }
    public FolioStatus Status { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Notes { get; set; }

    public Booking? Booking { get; set; }
    public BookingRoom? BookingRoom { get; set; }
    public Guest? Guest { get; set; }
    public ICollection<StayNight> StayNights { get; set; } = new List<StayNight>();
    public ICollection<Charge> Charges { get; set; } = new List<Charge>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
