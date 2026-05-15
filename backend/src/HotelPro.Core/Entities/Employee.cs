using HotelPro.Core.Enums;

namespace HotelPro.Core.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public EmployeeRole Role { get; set; }
    public string? PinHash { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;
    public bool CanLogin { get; set; } = true;
    public string? AssignedBuildingIds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public ICollection<HousekeepingLog> HousekeepingLogs { get; set; } = new List<HousekeepingLog>();
    public ICollection<WorkOrder> ReportedWorkOrders { get; set; } = new List<WorkOrder>();
    public ICollection<WorkOrder> AssignedWorkOrders { get; set; } = new List<WorkOrder>();
    public ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();
}
