namespace HotelPro.Core.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal VatAmount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? SupplierName { get; set; }
    public string? InvoiceNumber { get; set; }
    public Guid? ApprovedById { get; set; }
    public string? Notes { get; set; }

    public Employee? ApprovedBy { get; set; }
}
