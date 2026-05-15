namespace HotelPro.Core.Entities;

public class Shift
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime ShiftDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string ShiftType { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;
}
