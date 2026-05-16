namespace HotelPro.Core.Entities;

public class AdvancePayment
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public Guid? AppliedToInvoiceId { get; set; }
    public bool IsRefunded { get; set; }
    public DateTime CreatedAt { get; set; }
}
