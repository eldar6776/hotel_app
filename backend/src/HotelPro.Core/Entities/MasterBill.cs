namespace HotelPro.Core.Entities;

public class MasterBill
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid PayerGuestId { get; set; }
    public decimal TotalStayCharges { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public BookingGroup Group { get; set; } = null!;
    public Guest PayerGuest { get; set; } = null!;
}
