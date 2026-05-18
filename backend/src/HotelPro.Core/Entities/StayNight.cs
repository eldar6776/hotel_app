using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class StayNight
{
    public Guid Id { get; set; }
    public Guid FolioId { get; set; }
    public Guid? StayId { get; set; }
    public Guid RoomId { get; set; }
    public DateTime Date { get; set; }
    public decimal TariffAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public NightStatus Status { get; set; }
    public bool IsComp { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime? ClosedAt { get; set; }

    public Folio Folio { get; set; } = null!;
    public Stay? Stay { get; set; }
    public Room Room { get; set; } = null!;
}
