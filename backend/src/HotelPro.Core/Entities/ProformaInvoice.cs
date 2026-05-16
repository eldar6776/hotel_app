using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class ProformaInvoice
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string ProformaNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public ProformaStatus Status { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? ConvertedToInvoiceId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ProformaStatus
{
    Issued,
    PartiallyPaid,
    FullyPaid,
    Cancelled
}
