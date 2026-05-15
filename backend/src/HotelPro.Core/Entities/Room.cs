using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class Room
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public Guid BuildingId { get; set; }
    public Guid RoomTypeId { get; set; }
    public RoomStatus Status { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public decimal? BasePrice { get; set; }
    public int SortOrder { get; set; }

    public Building Building { get; set; } = null!;
    public RoomType RoomType { get; set; } = null!;
}
