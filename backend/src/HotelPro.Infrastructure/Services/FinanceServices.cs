using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

    public ExchangeRateService(HotelProDbContext dbContext) => _dbContext = dbContext;

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
        var rates = await GetCurrentRatesAsync();
        var fromRate = rates.FirstOrDefault(r => r.CurrencyCode == fromCurrency)?.Rate ?? 1m;
        var toRate = rates.FirstOrDefault(r => r.CurrencyCode == toCurrency)?.Rate ?? 1m;
        return amount * (toRate / fromRate);
    }
}
