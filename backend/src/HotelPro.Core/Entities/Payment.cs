using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid FolioId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Reference { get; set; }
    public Guid? ProcessedById { get; set; }
    public string? Notes { get; set; }

    public Folio Folio { get; set; } = null!;
    public PaymentMethod? PaymentMethodEntity { get; set; }
    public Employee? ProcessedBy { get; set; }
}
