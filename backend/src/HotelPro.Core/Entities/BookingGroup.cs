using HotelPro.Core.Enums;
using HotelPro.Core.Interfaces;

namespace HotelPro.Core.Entities;

public class BookingGroup : IHaveHotelId
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid ContactPersonId { get; set; }
    public DateTime Arrival { get; set; }
    public DateTime Departure { get; set; }
    public int BlockedRoomCount { get; set; }
    public int ConfirmedRoomCount { get; set; }
    public Guid? RatePlanId { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public GroupStatus Status { get; set; }
    public bool UseMasterBill { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guest ContactPerson { get; set; } = null!;
    public ICollection<GroupBooking> GroupBookings { get; set; } = new List<GroupBooking>();
    public MasterBill? MasterBill { get; set; }
}
