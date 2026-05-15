using HotelPro.Core.Enums;
using HotelPro.Core.Interfaces;

namespace HotelPro.Core.Entities;

public class Booking : IHaveHotelId
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public Guid GuestId { get; set; }
    public Guid? GroupId { get; set; }
    public BookingSource Source { get; set; }
    public BookingType Type { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime ArrivalDate { get; set; }
    public DateTime DepartureDate { get; set; }
    public int AdultCount { get; set; }
    public int ChildCount { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal ExchangeRateTotal { get; set; }
    public string Currency { get; set; } = "EUR";
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guest Guest { get; set; } = null!;
    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
    public ICollection<BookingHistory> Histories { get; set; } = new List<BookingHistory>();
}
