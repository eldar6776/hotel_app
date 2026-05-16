namespace HotelPro.Core.Services;

public class EmailConfiguration
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool UseTls { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 300;
}
