using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IFolioLedgerService
{
    Task<FolioLedgerDto> GetFolioLedgerAsync(Guid folioId);
    Task<decimal> GetFolioBalanceAsync(Guid folioId);
    Task<decimal> GetTotalChargesAsync(Guid folioId);
    Task<decimal> GetTotalPaymentsAsync(Guid folioId);
    Task ReconcileFolioBalanceAsync(Guid folioId);
}

public record FolioLedgerDto(
    Guid FolioId,
    string FolioNumber,
    string Status,
    decimal NightCharges,
    decimal OtherCharges,
    decimal TotalCharges,
    decimal TotalPayments,
    decimal Balance,
    List<FolioLedgerEntryDto> Entries
);

public record FolioLedgerEntryDto(
    string Type,
    DateTime Date,
    string Description,
    decimal Debit,
    decimal Credit,
    Guid? ReferenceId
);
