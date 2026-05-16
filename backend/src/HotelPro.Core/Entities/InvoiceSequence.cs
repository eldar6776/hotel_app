namespace HotelPro.Core.Entities;

public class InvoiceSequence
{
    public Guid Id { get; set; }
    public string Prefix { get; set; } = "INV";
    public int LastNumber { get; set; }
    public int Year { get; set; }
}
