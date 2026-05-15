namespace HotelPro.Core.Enums;

public enum BookingStatus
{
    Provisional,
    Confirmed,
    CheckedIn,
    CheckedOut,
    Cancelled,
    NoShow
}

public enum PaymentStatus
{
    Unpaid,
    Partial,
    Paid,
    Refunded
}

public enum BookingRoomStatus
{
    Pending,
    CheckedIn,
    CheckedOut,
    Cancelled
}

public enum BookingHistoryAction
{
    Created,
    Modified,
    Cancelled,
    CheckedIn,
    CheckedOut,
    NoShow
}

public enum RoomAssignmentStatus
{
    Tentative,
    Confirmed,
    CheckedIn,
    CheckedOut
}

public enum FolioStatus
{
    Open,
    Closed,
    Archived
}

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    Cancelled
}

public enum EmployeeRole
{
    Admin,
    Manager,
    Reception,
    Housekeeping,
    Maintenance,
    Accountant
}

public enum ShiftType
{
    Morning,
    Afternoon,
    Night
}

public enum HousekeepingAction
{
    Cleaned,
    Inspected,
    Repaired,
    Restocked,
    TurnDown
}

public enum HousekeepingStatus
{
    Pending,
    InProgress,
    Completed,
    Verified
}

public enum WorkOrderPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum WorkOrderCategory
{
    Plumbing,
    Electrical,
    HVAC,
    Furniture,
    Other
}

public enum WorkOrderStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public enum AccessAction
{
    Login,
    Logout,
    AccessGranted,
    AccessDenied,
    PinVerified
}
