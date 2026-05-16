using HotelPro.Core.Entities;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HotelPro.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly HotelProDbContext _dbContext;
    private readonly EmailConfiguration _emailConfig;
    private readonly ILogger<EmailService> _logger;
    private readonly Func<ISmtpClient> _smtpClientFactory;

    public EmailService(
        HotelProDbContext dbContext,
        IOptions<EmailConfiguration> emailConfig,
        ILogger<EmailService> logger,
        Func<ISmtpClient> smtpClientFactory)
    {
        _dbContext = dbContext;
        _emailConfig = emailConfig.Value;
        _logger = logger;
        _smtpClientFactory = smtpClientFactory;
    }

    public async Task<EmailSendResult> SendConfirmationAsync(Guid bookingId)
    {
        return await SendEmailForBookingAsync(bookingId, "confirmation");
    }

    public async Task<EmailSendResult> SendCancellationAsync(Guid bookingId)
    {
        return await SendEmailForBookingAsync(bookingId, "cancellation");
    }

    private async Task<EmailSendResult> SendEmailForBookingAsync(Guid bookingId, string type)
    {
        var booking = await _dbContext.Bookings
            .IgnoreQueryFilters()
            .Include(b => b.Guest)
            .Include(b => b.BookingRooms)
            .ThenInclude(br => br.RoomType)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return new EmailSendResult(false, $"Booking {bookingId} not found.");
        }

        if (booking.Guest == null || string.IsNullOrWhiteSpace(booking.Guest.Email))
        {
            return new EmailSendResult(false, "Guest email is not available.");
        }

        var templateName = type == "confirmation" ? "BookingConfirmation.html" : "BookingCancellation.html";
        var subject = type == "confirmation"
            ? $"Potvrda rezervacije - {booking.Id.ToString().Substring(0, 8).ToUpper()}"
            : $"Obavijest o otkazivanju - {booking.Id.ToString().Substring(0, 8).ToUpper()}";

        var guestName = $"{booking.Guest.FirstName} {booking.Guest.LastName}".Trim();

        var emailLog = new EmailLog
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            Recipient = booking.Guest.Email,
            Subject = subject,
            Body = string.Empty,
            IsHtml = true,
            Status = EmailStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var body = BuildEmailBody(templateName, booking);
            emailLog.Body = body;
        }
        catch (Exception ex)
        {
            emailLog.Body = $"Error building template: {ex.Message}";
            emailLog.Status = EmailStatus.Failed;
            emailLog.ErrorMessage = ex.Message;
        }

        _dbContext.EmailLogs.Add(emailLog);
        await _dbContext.SaveChangesAsync();

        if (emailLog.Status == EmailStatus.Failed)
        {
            return new EmailSendResult(false, emailLog.ErrorMessage);
        }

        return await SendWithRetryAsync(emailLog);
    }

    private async Task<EmailSendResult> SendWithRetryAsync(EmailLog emailLog)
    {
        for (int attempt = 1; attempt <= _emailConfig.MaxRetries; attempt++)
        {
            try
            {
                await SendEmailAsync(emailLog.Recipient, emailLog.Subject, emailLog.Body, emailLog.IsHtml);

                emailLog.Status = EmailStatus.Sent;
                emailLog.SentAt = DateTime.UtcNow;
                emailLog.RetryCount = attempt;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Email sent successfully to {Recipient} on attempt {Attempt}", emailLog.Recipient, attempt);
                return new EmailSendResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Email send attempt {Attempt} failed for {Recipient}", attempt, emailLog.Recipient);

                emailLog.RetryCount = attempt;
                emailLog.ErrorMessage = ex.Message;

                if (attempt < _emailConfig.MaxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_emailConfig.RetryDelaySeconds));
                }
            }
        }

        emailLog.Status = EmailStatus.Failed;
        emailLog.ErrorMessage ??= $"Failed after {_emailConfig.MaxRetries} attempts.";
        await _dbContext.SaveChangesAsync();

        _logger.LogError("Email failed after {MaxRetries} attempts for {Recipient}", _emailConfig.MaxRetries, emailLog.Recipient);
        return new EmailSendResult(false, emailLog.ErrorMessage);
    }

    private async Task SendEmailAsync(string to, string subject, string body, bool isHtml)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromAddress));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = isHtml ? body : null,
            TextBody = isHtml ? StripHtml(body) : body
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = _smtpClientFactory();
        await client.ConnectAsync(_emailConfig.SmtpHost, _emailConfig.SmtpPort, _emailConfig.UseTls);

        if (!string.IsNullOrEmpty(_emailConfig.SmtpUsername))
        {
            await client.AuthenticateAsync(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private static string BuildEmailBody(string templateName, Booking booking)
    {
        var template = LoadTemplate(templateName);

        var guestName = $"{booking.Guest?.FirstName} {booking.Guest?.LastName}".Trim();
        var roomType = booking.BookingRooms.FirstOrDefault()?.RoomType?.Name ?? "N/A";
        var nights = Math.Max(1, (booking.DepartureDate - booking.ArrivalDate).Days);
        var bookingNumber = booking.Id.ToString().Substring(0, 8).ToUpper();

        return template
            .Replace("{{GuestName}}", guestName)
            .Replace("{{HotelName}}", "HotelPRO")
            .Replace("{{RoomType}}", roomType)
            .Replace("{{Arrival}}", booking.ArrivalDate.ToString("dd.MM.yyyy."))
            .Replace("{{Departure}}", booking.DepartureDate.ToString("dd.MM.yyyy."))
            .Replace("{{Nights}}", nights.ToString())
            .Replace("{{TotalPrice}}", $"{booking.TotalPrice:F2} {booking.Currency}")
            .Replace("{{BookingNumber}}", bookingNumber)
            .Replace("{{Status}}", booking.Status.ToString());
    }

    private static string LoadTemplate(string templateName)
    {
        var assembly = typeof(EmailService).Assembly;
        var resourceName = $"HotelPro.Infrastructure.Email.Templates.{templateName}";

        var names = assembly.GetManifestResourceNames();
        var matchedName = names.FirstOrDefault(n => n.EndsWith(templateName, StringComparison.OrdinalIgnoreCase));

        if (matchedName == null)
        {
            throw new InvalidOperationException($"Email template '{templateName}' not found. Available: {string.Join(", ", names)}");
        }

        using var stream = assembly.GetManifestResourceStream(matchedName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Email template '{templateName}' stream is null.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string StripHtml(string html)
    {
        var stripped = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", string.Empty);
        return System.Text.RegularExpressions.Regex.Replace(stripped, @"\s+", " ").Trim();
    }
}
