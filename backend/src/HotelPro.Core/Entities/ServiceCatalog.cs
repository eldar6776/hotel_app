namespace HotelPro.Core.Entities;

public class ServiceCatalog
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal? DefaultPrice { get; set; }
    public decimal VatPercent { get; set; }
    public bool IsActive { get; set; }
}
