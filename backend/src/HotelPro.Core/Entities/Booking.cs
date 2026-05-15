namespace HotelPro.Core.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public Guid GuestId { get; set; }
    public Guid? BookingTypeId { get; set; }
    public Guid? BookingSourceId { get; set; }
    public Guid? PartnerId { get; set; }
    public Guid? SalesAgentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ArrivalDate { get; set; }
    public DateTime DepartureDate { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public string PaymentStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? ConfirmationCode { get; set; }
    public Guid? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public Guest Guest { get; set; } = null!;
    public BookingType? BookingType { get; set; }
    public BookingSource? BookingSource { get; set; }
    public Partner? Partner { get; set; }
    public SalesAgent? SalesAgent { get; set; }
    public Employee? CreatedBy { get; set; }
    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
    public ICollection<BookingHistory> Histories { get; set; } = new List<BookingHistory>();
}
