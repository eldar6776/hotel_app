using HotelPro.Core.DTOs;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.Services;

public class FolioLedgerService : IFolioLedgerService
{
    private readonly HotelProDbContext _dbContext;
    private readonly ILogger<FolioLedgerService> _logger;

    public FolioLedgerService(
        HotelProDbContext dbContext,
        ILogger<FolioLedgerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<FolioLedgerDto> GetFolioLedgerAsync(Guid folioId)
    {
        var folio = await _dbContext.Folios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == folioId);

        if (folio == null)
            throw new InvalidOperationException($"Folio {folioId} not found.");

        var nightCharges = await _dbContext.StayNights
            .Where(n => n.FolioId == folioId && n.Status == NightStatus.Active)
            .SumAsync(n => n.TariffAmount);

        var otherCharges = await _dbContext.Charges
            .Where(c => c.FolioId == folioId && c.ChargeType != ChargeType.StayNight)
            .SumAsync(c => c.TotalPrice);

        var totalPayments = await _dbContext.Payments
            .Where(p => p.FolioId == folioId)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var totalCharges = nightCharges + otherCharges;
        var balance = totalCharges - totalPayments;

        var entries = new List<FolioLedgerEntryDto>();

        var nights = await _dbContext.StayNights
            .Where(n => n.FolioId == folioId)
            .OrderBy(n => n.Date)
            .ToListAsync();

        foreach (var night in nights)
        {
            entries.Add(new FolioLedgerEntryDto(
                Type: "Night",
                Date: night.Date,
                Description: night.Description ?? $"Night {night.Date:yyyy-MM-dd}",
                Debit: night.Status == NightStatus.Active ? night.TariffAmount : 0,
                Credit: 0,
                ReferenceId: night.Id
            ));
        }

        var charges = await _dbContext.Charges
            .Where(c => c.FolioId == folioId && c.ChargeType != ChargeType.StayNight)
            .OrderBy(c => c.ChargeDate)
            .ToListAsync();

        foreach (var charge in charges)
        {
            entries.Add(new FolioLedgerEntryDto(
                Type: "Charge",
                Date: charge.ChargeDate,
                Description: charge.Description,
                Debit: charge.TotalPrice > 0 ? charge.TotalPrice : 0,
                Credit: charge.TotalPrice < 0 ? Math.Abs(charge.TotalPrice) : 0,
                ReferenceId: charge.Id
            ));
        }

        var payments = await _dbContext.Payments
            .Where(p => p.FolioId == folioId)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync();

        foreach (var payment in payments)
        {
            entries.Add(new FolioLedgerEntryDto(
                Type: "Payment",
                Date: payment.PaymentDate,
                Description: payment.Notes ?? $"Payment",
                Debit: 0,
                Credit: payment.Amount,
                ReferenceId: payment.Id
            ));
        }

        entries = entries.OrderBy(e => e.Date).ToList();

        return new FolioLedgerDto(
            folio.Id,
            folio.FolioNumber,
            folio.Status.ToString(),
            nightCharges,
            otherCharges,
            totalCharges,
            totalPayments,
            balance,
            entries
        );
    }

    public async Task<decimal> GetFolioBalanceAsync(Guid folioId)
    {
        var totalCharges = await GetTotalChargesAsync(folioId);
        var totalPayments = await GetTotalPaymentsAsync(folioId);
        return totalCharges - totalPayments;
    }

    public async Task<decimal> GetTotalChargesAsync(Guid folioId)
    {
        var nightCharges = await _dbContext.StayNights
            .Where(n => n.FolioId == folioId && n.Status == NightStatus.Active)
            .SumAsync(n => n.TariffAmount);

        var otherCharges = await _dbContext.Charges
            .Where(c => c.FolioId == folioId && c.ChargeType != ChargeType.StayNight)
            .SumAsync(c => c.TotalPrice);

        return nightCharges + otherCharges;
    }

    public async Task<decimal> GetTotalPaymentsAsync(Guid folioId)
    {
        return await _dbContext.Payments
            .Where(p => p.FolioId == folioId)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
    }

    public async Task ReconcileFolioBalanceAsync(Guid folioId)
    {
        var computedBalance = await GetFolioBalanceAsync(folioId);

        var folio = await _dbContext.Folios.FindAsync(folioId);
        if (folio == null)
            throw new InvalidOperationException($"Folio {folioId} not found.");

        if (folio.Balance != computedBalance)
        {
            _logger.LogInformation(
                "Reconciling folio {FolioId}: stored balance {StoredBalance} -> computed balance {ComputedBalance}",
                folioId, folio.Balance, computedBalance);

            folio.Balance = computedBalance;
            folio.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
}
