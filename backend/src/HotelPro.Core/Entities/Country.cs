namespace HotelPro.Core.Entities;

public class Country
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string PhoneCode { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;

    public ICollection<Guest> Guests { get; set; } = new List<Guest>();
    public ICollection<Partner> Partners { get; set; } = new List<Partner>();
}
