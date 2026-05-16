namespace HotelPro.Core.Services;

public interface INightAuditService
{
    Task<NightAuditResult> RunAuditAsync(DateTime auditDate);
}

public record NightAuditResult(
    bool Success,
    int BookingsProcessed,
    decimal TotalStayCharges,
    int NoShowsDetected,
    string? ErrorMessage
);
