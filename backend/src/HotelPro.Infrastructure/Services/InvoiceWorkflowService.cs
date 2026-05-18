using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.Services;

public class InvoiceWorkflowService : IInvoiceWorkflowService
{
    private readonly HotelProDbContext _dbContext;
    private readonly IFolioLedgerService _folioLedgerService;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<InvoiceWorkflowService> _logger;

    public InvoiceWorkflowService(
        HotelProDbContext dbContext,
        IFolioLedgerService folioLedgerService,
        IConfigurationService configurationService,
        ILogger<InvoiceWorkflowService> logger)
    {
        _dbContext = dbContext;
        _folioLedgerService = folioLedgerService;
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<InvoiceResultDto> CreateInvoiceAsync(CreateFolioInvoiceRequest request)
    {
        var folio = await _dbContext.Folios
            .IgnoreQueryFilters()
            .Include(f => f.Guest)
            .Include(f => f.StayNights)
            .Include(f => f.Charges)
            .FirstOrDefaultAsync(f => f.Id == request.FolioId);

        if (folio == null)
            throw new InvalidOperationException($"Folio {request.FolioId} not found.");

        var vatRate = await GetVatRateAsync();
        var ledger = await _folioLedgerService.GetFolioLedgerAsync(request.FolioId);

        var lineItems = new List<InvoiceLineItemDto>();
        var subTotal = 0m;

        var activeStays = await _dbContext.Stays
            .Include(s => s.Room)
            .Where(s => s.FolioId == request.FolioId)
            .ToListAsync();

        var periodFrom = activeStays.Any() ? activeStays.Min(s => s.CheckInDate) : DateTime.UtcNow;
        var periodTo = activeStays.Any() ? activeStays.Max(s => s.CheckedOutAt ?? s.CheckOutDate) : DateTime.UtcNow;
        var roomNumber = activeStays.FirstOrDefault()?.Room?.RoomNumber ?? "";
        var guestName = folio.Guest != null
            ? $"{folio.Guest.FirstName} {folio.Guest.LastName}".Trim()
            : "";

        if (ledger.NightCharges > 0)
        {
            var nightCount = await _dbContext.StayNights
                .CountAsync(n => n.FolioId == request.FolioId && n.Status == NightStatus.Active);

            var nightTotal = ledger.NightCharges;
            var nightVat = nightTotal * vatRate / (100 + vatRate);
            var nightNet = nightTotal - nightVat;

            lineItems.Add(new InvoiceLineItemDto(
                Description: $"Accommodation ({nightCount} nights)",
                Quantity: nightCount,
                UnitPrice: nightCount > 0 ? nightNet / nightCount : 0,
                TotalPrice: nightNet,
                VatRate: vatRate,
                VatAmount: nightVat
            ));
            subTotal += nightNet;
        }

        foreach (var charge in folio.Charges.Where(c => c.ChargeType != ChargeType.StayNight))
        {
            var chargeVat = charge.TotalPrice * vatRate / (100 + vatRate);
            var chargeNet = charge.TotalPrice - chargeVat;

            lineItems.Add(new InvoiceLineItemDto(
                Description: charge.Description,
                Quantity: charge.Quantity,
                UnitPrice: charge.UnitPrice,
                TotalPrice: chargeNet,
                VatRate: vatRate,
                VatAmount: chargeVat
            ));
            subTotal += chargeNet;
        }

        var totalVat = lineItems.Sum(li => li.VatAmount);
        var totalAmount = subTotal + totalVat;

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            FolioId = request.FolioId,
            InvoiceNumber = invoiceNumber,
            Status = InvoiceStatus.Draft,
            TotalNet = subTotal,
            TotalVat = totalVat,
            TotalGross = totalAmount,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Invoices.Add(invoice);

        foreach (var li in lineItems)
        {
            _dbContext.InvoiceItems.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                NetAmount = li.TotalPrice,
                VatAmount = li.VatAmount,
                GrossAmount = li.TotalPrice + li.VatAmount,
                VatPercent = li.VatRate
            });
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Invoice {InvoiceNumber} created for folio {FolioId}, total {TotalAmount}",
            invoiceNumber, request.FolioId, totalAmount);

        return new InvoiceResultDto(
            invoice.Id,
            invoiceNumber,
            request.FolioId,
            guestName,
            roomNumber,
            periodFrom,
            periodTo,
            subTotal,
            totalVat,
            totalAmount,
            false,
            null,
            invoice.CreatedAt,
            lineItems
        );
    }

    public async Task<InvoiceResultDto> StornoInvoiceAsync(StornoInvoiceRequest request)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice {request.InvoiceId} not found.");

        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Invoice is already cancelled.");

        invoice.Status = InvoiceStatus.Cancelled;

        var stornoInvoiceNumber = $"STN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var stornoLineItems = new List<InvoiceLineItemDto>();
        var subTotal = 0m;

        foreach (var item in invoice.Items)
        {
            var stornoItem = new InvoiceLineItemDto(
                Description: $"STORNO: {item.Description}",
                Quantity: item.Quantity,
                UnitPrice: -item.UnitPrice,
                TotalPrice: -item.NetAmount,
                VatRate: item.VatPercent,
                VatAmount: -item.VatAmount
            );
            stornoLineItems.Add(stornoItem);
            subTotal += stornoItem.TotalPrice;
        }

        var totalVat = stornoLineItems.Sum(li => li.VatAmount);
        var totalAmount = subTotal + totalVat;

        var stornoInvoice = new Invoice
        {
            Id = Guid.NewGuid(),
            FolioId = invoice.FolioId,
            InvoiceNumber = stornoInvoiceNumber,
            Status = InvoiceStatus.Cancelled,
            TotalNet = subTotal,
            TotalVat = totalVat,
            TotalGross = totalAmount,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Invoices.Add(stornoInvoice);

        foreach (var li in stornoLineItems)
        {
            _dbContext.InvoiceItems.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                InvoiceId = stornoInvoice.Id,
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                NetAmount = li.TotalPrice,
                VatAmount = li.VatAmount,
                GrossAmount = li.TotalPrice + li.VatAmount,
                VatPercent = li.VatRate
            });
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Invoice {InvoiceNumber} storned: {StornoNumber}, reason: {Reason}",
            invoice.InvoiceNumber, stornoInvoiceNumber, request.Reason);

        return new InvoiceResultDto(
            stornoInvoice.Id,
            stornoInvoiceNumber,
            stornoInvoice.FolioId,
            "",
            "",
            DateTime.UtcNow,
            DateTime.UtcNow,
            subTotal,
            totalVat,
            totalAmount,
            true,
            request.Reason,
            stornoInvoice.CreatedAt,
            stornoLineItems
        );
    }

    public async Task<InvoiceResultDto> GetInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice {invoiceId} not found.");

        var lineItems = invoice.Items.Select(ii => new InvoiceLineItemDto(
            ii.Description, ii.Quantity, ii.UnitPrice, ii.NetAmount, ii.VatPercent, ii.VatAmount
        )).ToList();

        return new InvoiceResultDto(
            invoice.Id,
            invoice.InvoiceNumber ?? "",
            invoice.FolioId,
            "",
            "",
            invoice.IssueDate,
            invoice.IssueDate,
            lineItems.Sum(li => li.TotalPrice),
            lineItems.Sum(li => li.VatAmount),
            invoice.TotalGross,
            invoice.Status == InvoiceStatus.Cancelled,
            invoice.Status == InvoiceStatus.Cancelled ? "Cancelled" : null,
            invoice.CreatedAt,
            lineItems
        );
    }

    public async Task<IEnumerable<InvoiceResultDto>> GetInvoicesForFolioAsync(Guid folioId)
    {
        var invoices = await _dbContext.Invoices
            .Include(i => i.Items)
            .Where(i => i.FolioId == folioId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var results = new List<InvoiceResultDto>();
        foreach (var invoice in invoices)
        {
            results.Add(await GetInvoiceAsync(invoice.Id));
        }
        return results;
    }

    private async Task<decimal> GetVatRateAsync()
    {
        var value = await _configurationService.GetValueAsync("vat_rate");
        if (decimal.TryParse(value, out var rate))
            return rate;
        return 17m;
    }
}
