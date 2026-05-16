namespace HotelPro.Core.Entities;

public class EmailLog
{
    public Guid Id { get; set; }
    public Guid? BookingId { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
    public EmailStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }

    public Booking? Booking { get; set; }
}

public enum EmailStatus
{
    Pending,
    Sent,
    Failed
}
