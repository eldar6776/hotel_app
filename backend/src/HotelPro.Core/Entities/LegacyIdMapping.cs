namespace HotelPro.Core.Entities;

public class LegacyIdMapping
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string LegacyTableName { get; set; } = string.Empty;
    public int LegacyId { get; set; }
    public Guid NewId { get; set; }
    public DateTime MigratedAt { get; set; }
}
