namespace HotelPro.Core.Entities;

public class OutstandingBalance
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public Guid FolioId { get; set; }
    public decimal Balance { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsOverdue { get; set; }

    public Folio Folio { get; set; } = null!;
}
