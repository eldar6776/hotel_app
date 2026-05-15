using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class BookingRoom
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid RoomTypeId { get; set; }
    public Guid RatePlanId { get; set; }
    public decimal PricePerNight { get; set; }
    public BookingRoomStatus Status { get; set; }

    public Booking Booking { get; set; } = null!;
    public Room? Room { get; set; }
    public RoomType RoomType { get; set; } = null!;
}
