namespace HotelPro.Core.Entities;

public class RoomType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int BaseCapacity { get; set; }
    public int MaxCapacity { get; set; }
    public decimal DefaultPrice { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
}
