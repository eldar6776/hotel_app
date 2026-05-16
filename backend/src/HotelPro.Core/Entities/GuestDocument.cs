using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class GuestDocument
{
    public Guid Id { get; set; }
    public Guid GuestId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public string? MRZLine1 { get; set; }
    public string? MRZLine2 { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? FileUrl { get; set; }
    public string? FrontImagePath { get; set; }
    public string? BackImagePath { get; set; }
    public bool IsVerified { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guest Guest { get; set; } = null!;
}
