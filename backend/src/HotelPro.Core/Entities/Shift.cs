using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class Shift
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime ShiftDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ShiftType ShiftType { get; set; }
    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;
}
