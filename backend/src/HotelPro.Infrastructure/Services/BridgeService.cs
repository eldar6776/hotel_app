using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.Services;

public class BridgeService
{
    private readonly ILogger<BridgeService> _logger;
    private readonly List<BridgeQueueItem> _queue = new();
    private int _retryId;

    public BridgeService(ILogger<BridgeService> logger) => _logger = logger;

    public BridgeStatus GetStatus()
    {
        return new BridgeStatus(
            FiscalPrinterConnected: false,
            RfidEncoderConnected: false,
            PabxConnected: false,
            QueueLength: _queue.Count(i => i.Status == "pending"),
            HardwareMode: "Mock"
        );
    }

    public string Enqueue(string device, string command, object payload)
    {
        var id = Interlocked.Increment(ref _retryId);
        _queue.Add(new BridgeQueueItem
        {
            Id = id,
            Device = device,
            Command = command,
            Payload = System.Text.Json.JsonSerializer.Serialize(payload),
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0,
            MaxRetries = 10
        });

        _logger.LogInformation("Bridge queued: {Device}/{Command} id={Id}", device, command, id);

        // Simulate immediate success in mock mode
        var item = _queue.First(i => i.Id == id);
        item.Status = "completed";
        item.CompletedAt = DateTime.UtcNow;

        return $"job-{id}";
    }

    public List<BridgeQueueItem> GetQueue() => _queue.OrderByDescending(i => i.CreatedAt).Take(100).ToList();

    public FiscalResult FiscalizeInvoice(FiscalInvoiceData data)
    {
        var fiscalCode = $"JIR-{Guid.NewGuid().ToString()[..8].ToUpper()}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        _logger.LogInformation("Fiscalized invoice {Invoice}, code: {FiscalCode}", data.InvoiceNumber, fiscalCode);
        return new FiscalResult(true, fiscalCode, null);
    }

    public RfidEncodeResult EncodeRfidCard(RfidEncodeData data)
    {
        _logger.LogInformation("Encoded RFID for room {Room}, code {Code}", data.RoomNumber, data.CardCode);
        return new RfidEncodeResult(true, $"RFID-{Guid.NewGuid():N}"[..12]);
    }

    public PabxCdrResult ImportCdr(DateTime from, DateTime to)
    {
        _logger.LogInformation("PABX CDR import: {From} - {To}", from, to);
        return new PabxCdrResult(0, 0, "Mock mode - no CDR data");
    }
}

public record BridgeStatus(
    bool FiscalPrinterConnected,
    bool RfidEncoderConnected,
    bool PabxConnected,
    int QueueLength,
    string HardwareMode
);

public class BridgeQueueItem
{
    public int Id { get; set; }
    public string Device { get; set; } = "";
    public string Command { get; set; } = "";
    public string Payload { get; set; } = "";
    public string Status { get; set; } = "pending";
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public record FiscalInvoiceData(string InvoiceNumber, decimal TotalAmount, string Currency, string GuestName, DateTime InvoiceDate);
public record FiscalResult(bool Success, string? FiscalCode, string? ErrorMessage);
public record RfidEncodeData(string CardCode, string RoomNumber, DateTime ValidFrom, DateTime ValidTo);
public record RfidEncodeResult(bool Success, string? CardId);
public record PabxCdrResult(int TotalCalls, decimal TotalCharges, string Status);
