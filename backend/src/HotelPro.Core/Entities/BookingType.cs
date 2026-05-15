namespace HotelPro.Core.Entities;

public class BookingType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Color { get; set; }
    public bool IsActive { get; set; }
}
