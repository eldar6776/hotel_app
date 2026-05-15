namespace HotelPro.Core.Entities;

public class Charge
{
    public Guid Id { get; set; }
    public Guid FolioId { get; set; }
    public Guid? ServiceCatalogId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal VatAmount { get; set; }
    public DateTime ChargeDate { get; set; }
    public Guid? PostedById { get; set; }
    public bool IsTaxable { get; set; } = true;

    public Folio Folio { get; set; } = null!;
    public ServiceCatalog? ServiceCatalog { get; set; }
    public Employee? PostedBy { get; set; }
}
