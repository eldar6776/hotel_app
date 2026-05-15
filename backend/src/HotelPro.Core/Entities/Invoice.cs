using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class Invoice
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid FolioId { get; set; }
    public Guid? GuestId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalGross { get; set; }
    public InvoiceStatus Status { get; set; }
    public string? PdfUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public Folio Folio { get; set; } = null!;
    public Guest? Guest { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}
