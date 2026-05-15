namespace HotelPro.Core.Entities;

public class Partner
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PartnerType { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public Guid? CountryId { get; set; }
    public string? ContractCode { get; set; }
    public decimal? CommissionPercent { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public Country? Country { get; set; }
    public ICollection<SalesAgent> SalesAgents { get; set; } = new List<SalesAgent>();
}
