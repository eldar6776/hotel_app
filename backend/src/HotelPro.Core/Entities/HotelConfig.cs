using HotelPro.Core.Interfaces;

namespace HotelPro.Core.Entities;

public class HotelConfig : IHaveHotelId
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSecret { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
