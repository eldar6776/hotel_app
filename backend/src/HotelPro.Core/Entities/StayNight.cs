namespace HotelPro.Core.Entities;

public class StayNight
{
    public Guid Id { get; set; }
    public Guid FolioId { get; set; }
    public DateTime Date { get; set; }
    public decimal RoomPrice { get; set; }
    public bool IsComp { get; set; }
    public string? Notes { get; set; }

    public Folio Folio { get; set; } = null!;
}
