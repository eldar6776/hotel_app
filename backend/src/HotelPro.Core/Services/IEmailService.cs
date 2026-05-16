namespace HotelPro.Core.Services;

public interface IEmailService
{
    Task<EmailSendResult> SendConfirmationAsync(Guid bookingId);
    Task<EmailSendResult> SendCancellationAsync(Guid bookingId);
}

public record EmailSendResult(bool Success, string? ErrorMessage = null);
