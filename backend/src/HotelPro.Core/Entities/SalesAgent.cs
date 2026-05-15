namespace HotelPro.Core.Entities;

public class SalesAgent
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Guid? PartnerId { get; set; }
    public decimal? CommissionPercent { get; set; }
    public bool IsActive { get; set; }

    public Partner? Partner { get; set; }
}
