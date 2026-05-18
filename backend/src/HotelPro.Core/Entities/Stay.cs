using HotelPro.Core.Enums;
namespace HotelPro.Core.Entities;

public class Stay
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public Guid GuestId { get; set; }
    public Guid RoomId { get; set; }
    public Guid? FolioId { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? BookingRoomId { get; set; }
    public Guid? TariffId { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public Guid? CheckedInBy { get; set; }
    public Guid? CheckedOutBy { get; set; }
    public DateTime? CheckedOutAt { get; set; }

    public bool IsCheckedOut { get; set; }
    public bool IsRegistrationPrinted { get; set; }
    public bool IsReservationLink { get; set; }
    public bool IsFromConfirmedReservation { get; set; }
    public bool IsAccommodationPaid { get; set; }

    public GuestCategory GuestCategory { get; set; }
    public decimal DiscountPercent { get; set; }
    public string? DiscountReason { get; set; }
    public int TaxOverride { get; set; }
    public string? StayNote { get; set; }
    public string? ServiceNote { get; set; }
    public string? PaymentNote { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Guest Guest { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public Folio? Folio { get; set; }
    public Booking? Booking { get; set; }
    public BookingRoom? BookingRoom { get; set; }
    public Employee? CheckedInByEmployee { get; set; }
    public Employee? CheckedOutByEmployee { get; set; }
}
