using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace HotelPro.Infrastructure.Services;

public class ProformaService : IProformaService
{
    private readonly HotelProDbContext _dbContext;

    public ProformaService(HotelProDbContext dbContext) => _dbContext = dbContext;

    public async Task<ProformaInvoiceDto> CreateProformaAsync(Guid bookingId)
    {
        var booking = await _dbContext.Bookings.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new InvalidOperationException("Booking not found");

        var proforma = new ProformaInvoice
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            ProformaNumber = $"PRO-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            IssueDate = DateTime.UtcNow,
            TotalAmount = booking.TotalPrice,
            Status = ProformaStatus.Issued,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Set<ProformaInvoice>().Add(proforma);
        await _dbContext.SaveChangesAsync();

        return MapToDto(proforma);
    }

    public async Task<ProformaInvoiceDto?> GetProformaAsync(Guid bookingId)
    {
        var p = await _dbContext.Set<ProformaInvoice>()
            .FirstOrDefaultAsync(x => x.BookingId == bookingId && x.Status == ProformaStatus.Issued);
        return p != null ? MapToDto(p) : null;
    }

    public async Task<InvoiceDetailDto> ConvertToInvoiceAsync(Guid proformaId)
    {
        var p = await _dbContext.Set<ProformaInvoice>().FindAsync(proformaId)
            ?? throw new InvalidOperationException("Proforma not found");

        var booking = await _dbContext.Bookings.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == p.BookingId)
            ?? throw new InvalidOperationException("Booking not found");

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = $"INV-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            FolioId = Guid.Empty,
            GuestId = booking.GuestId,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            TotalNet = p.TotalAmount,
            TotalVat = 0,
            TotalGross = p.TotalAmount,
            Status = InvoiceStatus.Sent,
            CreatedAt = DateTime.UtcNow
        };

        p.Status = ProformaStatus.FullyPaid;
        p.ConvertedToInvoiceId = invoice.Id;

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        return new InvoiceDetailDto(invoice.Id, invoice.InvoiceNumber, invoice.IssueDate,
            $"{booking.Guest?.FirstName} {booking.Guest?.LastName}", booking.Guest?.Address,
            invoice.TotalNet, invoice.TotalVat, invoice.TotalGross,
            "EUR", invoice.Status.ToString(), false, null, new());
    }

    private static ProformaInvoiceDto MapToDto(ProformaInvoice p) => new(
        p.Id, p.BookingId, p.ProformaNumber, p.IssueDate, p.TotalAmount,
        p.Status.ToString(), p.ExpiryDate, p.ConvertedToInvoiceId);
}

public class AdvancePaymentService : IAdvancePaymentService
{
    private readonly HotelProDbContext _dbContext;

    public AdvancePaymentService(HotelProDbContext dbContext) => _dbContext = dbContext;

    public async Task<AdvancePaymentDto> AddAdvancePaymentAsync(CreateAdvancePaymentDto dto)
    {
        var payment = new AdvancePayment
        {
            Id = Guid.NewGuid(),
            BookingId = dto.BookingId,
            Amount = dto.Amount,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = dto.PaymentMethod,
            Reference = dto.Reference,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<AdvancePayment>().Add(payment);
        await _dbContext.SaveChangesAsync();
        return MapToDto(payment);
    }

    public async Task<List<AdvancePaymentDto>> GetAdvancePaymentsAsync(Guid bookingId)
    {
        return await _dbContext.Set<AdvancePayment>()
            .Where(p => p.BookingId == bookingId)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task RefundAdvancePaymentAsync(Guid paymentId)
    {
        var payment = await _dbContext.Set<AdvancePayment>().FindAsync(paymentId)
            ?? throw new InvalidOperationException("Payment not found");
        payment.IsRefunded = true;
        await _dbContext.SaveChangesAsync();
    }

    private static AdvancePaymentDto MapToDto(AdvancePayment p) => new(
        p.Id, p.BookingId, p.Amount, p.PaymentDate, p.PaymentMethod, p.Reference, p.AppliedToInvoiceId, p.IsRefunded);
}

public class ExchangeRateService : IExchangeRateService
{
    private readonly HotelProDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfigurationService _config;
    private readonly ILogger<ExchangeRateService> _logger;

    public ExchangeRateService(
        HotelProDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        IConfigurationService config,
        ILogger<ExchangeRateService> logger)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<List<ExchangeRateDto>> GetCurrentRatesAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbContext.Set<ExchangeRate>()
            .Where(r => r.ValidFrom <= now && (r.ValidTo == null || r.ValidTo >= now))
            .Select(r => new ExchangeRateDto(r.Id, r.CurrencyCode, r.Rate, r.IsLocalCurrency, r.ValidFrom, r.ValidTo, r.Source))
            .ToListAsync();
    }

    public async Task UpdateRateAsync(string currencyCode, decimal rate)
    {
        var existing = await _dbContext.Set<ExchangeRate>()
            .FirstOrDefaultAsync(r => r.CurrencyCode == currencyCode && r.ValidTo == null);

        if (existing != null)
        {
            existing.ValidTo = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        _dbContext.Set<ExchangeRate>().Add(new ExchangeRate
        {
            Id = Guid.NewGuid(),
            CurrencyCode = currencyCode,
            Rate = rate,
            ValidFrom = DateTime.UtcNow,
            Source = "Manual",
            UpdatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        if (fromCurrency == toCurrency) return amount;

        var rates = await GetCurrentRatesAsync();
        var fromRate = rates.FirstOrDefault(r => r.CurrencyCode == fromCurrency)?.Rate ?? 1m;
        var toRate = rates.FirstOrDefault(r => r.CurrencyCode == toCurrency)?.Rate ?? 1m;
        return amount * (toRate / fromRate);
    }

    public async Task<int> SyncRatesFromExternalApiAsync(string? baseCurrency = null)
    {
        var @base = baseCurrency ?? await _config.GetValueAsync("CurrencyCode") ?? "EUR";
        var provider = await _config.GetValueAsync("ExchangeRate_Provider") ?? "Frankfurter";

        try
        {
            var client = _httpClientFactory.CreateClient("ExchangeRates");
            Dictionary<string, decimal> rates;
            string source;

            switch (provider.ToLowerInvariant())
            {
                case "openexchangerates":
                    rates = await FetchOpenExchangeRates(client, @base);
                    source = "OpenExchangeRates";
                    break;
                case "fixer":
                    rates = await FetchFixer(client, @base);
                    source = "Fixer";
                    break;
                case "exchangerateapi":
                    rates = await FetchExchangeRateAPI(client, @base);
                    source = "ExchangeRateAPI";
                    break;
                case "currencylayer":
                    rates = await FetchCurrencyLayer(client, @base);
                    source = "CurrencyLayer";
                    break;
                case "frankfurter":
                default:
                    rates = await FetchFrankfurter(client, @base);
                    source = "Frankfurter";
                    break;
            }

            if (rates.Count == 0) return 0;

            return await PersistRates(rates, @base, source);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync exchange rates from {Provider}", provider);
            return 0;
        }
    }

    private async Task<Dictionary<string, decimal>> FetchFrankfurter(HttpClient client, string @base)
    {
        var response = await client.GetAsync($"https://api.frankfurter.app/latest?base={@base}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<FrankfurterResponse>();
        return json?.Rates ?? new();
    }

    private async Task<Dictionary<string, decimal>> FetchOpenExchangeRates(HttpClient client, string @base)
    {
        var apiKey = await _config.GetValueAsync("ExchangeRate_OpenExchangeRates_ApiKey");
        if (string.IsNullOrEmpty(apiKey)) { _logger.LogWarning("OpenExchangeRates API key not configured"); return new(); }
        var response = await client.GetAsync($"https://openexchangerates.org/api/latest.json?app_id={apiKey}&base={@base}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<GenericRatesResponse>();
        return json?.Rates ?? new();
    }

    private async Task<Dictionary<string, decimal>> FetchFixer(HttpClient client, string @base)
    {
        var apiKey = await _config.GetValueAsync("ExchangeRate_Fixer_ApiKey");
        if (string.IsNullOrEmpty(apiKey)) { _logger.LogWarning("Fixer API key not configured"); return new(); }
        var response = await client.GetAsync($"https://api.fixer.io/latest?base={@base}&access_key={apiKey}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<GenericRatesResponse>();
        return json?.Rates ?? new();
    }

    private async Task<Dictionary<string, decimal>> FetchExchangeRateAPI(HttpClient client, string @base)
    {
        var apiKey = await _config.GetValueAsync("ExchangeRate_ExchangeRateAPI_ApiKey");
        if (string.IsNullOrEmpty(apiKey)) { _logger.LogWarning("ExchangeRateAPI key not configured"); return new(); }
        var response = await client.GetAsync($"https://v6.exchangerate-api.com/v6/{apiKey}/latest/{@base}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<GenericRatesResponse>();
        return json?.Rates ?? new();
    }

    private async Task<Dictionary<string, decimal>> FetchCurrencyLayer(HttpClient client, string @base)
    {
        var apiKey = await _config.GetValueAsync("ExchangeRate_CurrencyLayer_ApiKey");
        if (string.IsNullOrEmpty(apiKey)) { _logger.LogWarning("CurrencyLayer API key not configured"); return new(); }
        var response = await client.GetAsync($"http://apilayer.net/api/live?access_key={apiKey}&source={@base}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<CurrencyLayerResponse>();
        if (json?.Quotes == null) return new();
        var result = new Dictionary<string, decimal>();
        foreach (var kvp in json.Quotes)
        {
            var code = kvp.Key.Length > 3 ? kvp.Key[3..] : kvp.Key;
            result[code] = kvp.Value;
        }
        return result;
    }

    private async Task<int> PersistRates(Dictionary<string, decimal> rates, string baseCurrency, string source)
    {
        var now = DateTime.UtcNow;

        var existing = await _dbContext.Set<ExchangeRate>()
            .Where(r => r.ValidTo == null)
            .ToListAsync();

        foreach (var ex in existing)
        {
            ex.ValidTo = now;
            ex.UpdatedAt = now;
        }

        _dbContext.Set<ExchangeRate>().Add(new ExchangeRate
        {
            Id = Guid.NewGuid(),
            CurrencyCode = baseCurrency,
            Rate = 1.0m,
            IsLocalCurrency = true,
            ValidFrom = now,
            Source = source,
            UpdatedAt = now
        });

        foreach (var kvp in rates)
        {
            _dbContext.Set<ExchangeRate>().Add(new ExchangeRate
            {
                Id = Guid.NewGuid(),
                CurrencyCode = kvp.Key,
                Rate = kvp.Value,
                IsLocalCurrency = false,
                ValidFrom = now,
                Source = source,
                UpdatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Synced {Count} exchange rates from {Source} (base: {Base})", rates.Count + 1, source, baseCurrency);
        return rates.Count + 1;
    }

    private record FrankfurterResponse(Dictionary<string, decimal> Rates, string @Base, string Date);
    private record GenericRatesResponse(Dictionary<string, decimal> Rates, string @Base);
    private record CurrencyLayerResponse(Dictionary<string, decimal> Quotes);
}
