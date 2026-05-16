namespace HotelPro.Core.Entities;

public class PhoneExtension
{
    public string Extension { get; set; } = string.Empty;
    public Guid? RoomId { get; set; }
    public string? Description { get; set; }

    public Room? Room { get; set; }
}
