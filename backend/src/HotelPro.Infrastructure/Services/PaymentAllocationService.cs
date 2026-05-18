using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.Services;

public class PaymentAllocationService : IPaymentAllocationService
{
    private readonly HotelProDbContext _dbContext;
    private readonly IFolioLedgerService _folioLedgerService;
    private readonly ILogger<PaymentAllocationService> _logger;

    public PaymentAllocationService(
        HotelProDbContext dbContext,
        IFolioLedgerService folioLedgerService,
        ILogger<PaymentAllocationService> logger)
    {
        _dbContext = dbContext;
        _folioLedgerService = folioLedgerService;
        _logger = logger;
    }

    public async Task<PaymentAllocationResultDto> AllocatePaymentAsync(PaymentAllocationRequest request)
    {
        if (request.FolioAllocations.Count == 0)
            throw new InvalidOperationException("At least one folio allocation is required.");

        var totalAllocated = request.FolioAllocations.Sum(f => f.Amount);
        if (totalAllocated > request.TotalAmount)
            throw new InvalidOperationException(
                $"Total allocation ({totalAllocated}) exceeds payment amount ({request.TotalAmount}).");

        if (totalAllocated <= 0)
            throw new InvalidOperationException("Total allocation must be greater than zero.");

        var folioIds = request.FolioAllocations.Select(f => f.FolioId).Distinct().ToList();
        var folios = await _dbContext.Folios
            .Where(f => folioIds.Contains(f.Id))
            .ToListAsync();

        var missingFolioIds = folioIds.Except(folios.Select(f => f.Id)).ToList();
        if (missingFolioIds.Any())
            throw new InvalidOperationException($"Folios not found: {string.Join(", ", missingFolioIds)}");

        var closedFolios = folios.Where(f => f.Status == FolioStatus.Closed).ToList();
        if (closedFolios.Any())
            throw new InvalidOperationException(
                $"Cannot allocate payment to closed folios: {string.Join(", ", closedFolios.Select(f => f.FolioNumber))}");

        var reference = request.Reference ?? $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        var results = new List<FolioPaymentResultDto>();

        foreach (var allocation in request.FolioAllocations)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                FolioId = allocation.FolioId,
                PaymentMethod = request.PaymentMethod,
                Amount = allocation.Amount,
                Status = PaymentStatus.Paid,
                PaymentDate = DateTime.UtcNow,
                Reference = reference,
                ProcessedById = request.ProcessedById,
                Notes = request.Notes
            };

            _dbContext.Payments.Add(payment);
            results.Add(new FolioPaymentResultDto(
                payment.Id, allocation.FolioId, allocation.Amount, PaymentStatus.Paid.ToString()));
        }

        await _dbContext.SaveChangesAsync();

        foreach (var allocation in request.FolioAllocations)
        {
            try
            {
                await _folioLedgerService.ReconcileFolioBalanceAsync(allocation.FolioId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to reconcile folio {FolioId} after payment allocation", allocation.FolioId);
            }
        }

        _logger.LogInformation(
            "Payment {Reference} allocated: {TotalAmount} across {Count} folios",
            reference, request.TotalAmount, request.FolioAllocations.Count);

        return new PaymentAllocationResultDto(
            Guid.NewGuid(),
            reference,
            request.TotalAmount,
            results
        );
    }

    public async Task<PaymentAllocationResultDto> GetAllocationAsync(Guid allocationId)
    {
        throw new NotImplementedException("Allocation lookup by ID is not yet supported. Use GetAllocationsForPaymentAsync.");
    }

    public async Task<PaymentAllocationResultDto> GetAllocationsForPaymentAsync(string paymentReference)
    {
        var payments = await _dbContext.Payments
            .Where(p => p.Reference == paymentReference)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync();

        if (!payments.Any())
            throw new InvalidOperationException($"No payments found for reference {paymentReference}");

        var results = payments.Select(p => new FolioPaymentResultDto(
            p.Id, p.FolioId, p.Amount, p.Status.ToString()
        )).ToList();

        return new PaymentAllocationResultDto(
            Guid.NewGuid(),
            paymentReference,
            payments.Sum(p => p.Amount),
            results
        );
    }
}
