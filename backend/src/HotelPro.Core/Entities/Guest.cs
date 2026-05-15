namespace HotelPro.Core.Entities;

public class Guest
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public Guid? CountryId { get; set; }
    public string? IdDocumentType { get; set; }
    public string? IdDocumentNumber { get; set; }
    public string? Nationality { get; set; }
    public bool IsCompany { get; set; }
    public string? CompanyName { get; set; }
    public string? VatNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Country? Country { get; set; }
    public ICollection<GuestDocument> Documents { get; set; } = new List<GuestDocument>();
}
