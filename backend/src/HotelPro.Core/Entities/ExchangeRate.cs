namespace HotelPro.Core.Entities;

public class ExchangeRate
{
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsLocalCurrency { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string Source { get; set; } = "Manual";
    public DateTime UpdatedAt { get; set; }
}
