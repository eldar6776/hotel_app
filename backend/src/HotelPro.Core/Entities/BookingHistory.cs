using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class BookingHistory
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public BookingHistoryAction Action { get; set; }
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public Guid? ChangedById { get; set; }
    public DateTime ChangedAt { get; set; }

    public Booking Booking { get; set; } = null!;
    public Employee? ChangedBy { get; set; }
}
