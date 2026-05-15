namespace HotelPro.Core.Entities;

public class Tariff
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? RoomTypeId { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public bool IsActive { get; set; }

    public RoomType? RoomType { get; set; }
}
