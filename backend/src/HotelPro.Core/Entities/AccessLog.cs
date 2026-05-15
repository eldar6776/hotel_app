namespace HotelPro.Core.Entities;

public class AccessLog
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }

    public Employee? Employee { get; set; }
}
