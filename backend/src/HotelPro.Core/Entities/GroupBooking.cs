namespace HotelPro.Core.Entities;

public class GroupBooking
{
    public Guid Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public Guid BookingId { get; set; }
    public Guid MemberBookingId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Booking Booking { get; set; } = null!;
    public Booking MemberBooking { get; set; } = null!;
}
