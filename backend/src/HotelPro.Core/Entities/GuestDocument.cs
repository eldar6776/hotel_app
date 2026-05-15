namespace HotelPro.Core.Entities;

public class GuestDocument
{
    public Guid Id { get; set; }
    public Guid GuestId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? FileUrl { get; set; }
    public bool IsVerified { get; set; }
    public string? Notes { get; set; }

    public Guest Guest { get; set; } = null!;
}
