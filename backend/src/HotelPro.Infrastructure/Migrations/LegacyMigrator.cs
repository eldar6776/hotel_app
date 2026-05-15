using Microsoft.Extensions.Logging;
using HotelPro.Core.Entities;
using HotelPro.Infrastructure.Data;
using System.Globalization;

namespace HotelPro.Infrastructure.Migrations;

public class LegacyMigrator
{
    private readonly string _legacyConnectionString;
    private readonly HotelProDbContext _targetDbContext;
    private readonly ILogger<LegacyMigrator> _logger;

    public LegacyMigrator(string legacyConnectionString, HotelProDbContext targetDbContext, ILogger<LegacyMigrator> logger)
    {
        _legacyConnectionString = legacyConnectionString;
        _targetDbContext = targetDbContext;
        _logger = logger;
    }

    public async Task<MigrationResult> RunAsync(CancellationToken ct = default)
    {
        var result = new MigrationResult();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting legacy migration...");

            var idMapping = new Dictionary<string, Dictionary<int, Guid>>();

            result.TotalRecords += await MigrateBuildingsAsync(idMapping, ct);
            result.TotalRecords += await MigrateRoomTypesAsync(idMapping, ct);
            result.TotalRecords += await MigrateRoomsAsync(idMapping, ct);
            result.TotalRecords += await MigrateCountriesAsync(idMapping, ct);
            result.TotalRecords += await MigrateGuestsAsync(idMapping, ct);
            result.TotalRecords += await MigratePartnersAsync(idMapping, ct);
            result.TotalRecords += await MigrateBookingSourcesAsync(idMapping, ct);
            result.TotalRecords += await MigrateBookingTypesAsync(idMapping, ct);
            result.TotalRecords += await MigratePaymentMethodsAsync(idMapping, ct);
            result.TotalRecords += await MigrateServiceCatalogAsync(idMapping, ct);
            result.TotalRecords += await MigrateEmployeesAsync(idMapping, ct);

            result.SuccessCount = result.TotalRecords - result.ErrorCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration failed: {Message}", ex.Message);
            result.Errors.Add($"Fatal error: {ex.Message}");
        }

        sw.Stop();
        result.Elapsed = sw.Elapsed;
        return result;
    }

    private async Task<int> MigrateBuildingsAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating buildings...");
        return 0;
    }

    private async Task<int> MigrateRoomTypesAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating room types...");
        return 0;
    }

    private async Task<int> MigrateRoomsAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating rooms...");
        return 0;
    }

    private async Task<int> MigrateCountriesAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating countries...");
        return 0;
    }

    private async Task<int> MigrateGuestsAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating guests...");
        return 0;
    }

    private async Task<int> MigratePartnersAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating partners...");
        return 0;
    }

    private async Task<int> MigrateBookingSourcesAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating booking sources...");
        return 0;
    }

    private async Task<int> MigrateBookingTypesAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating booking types...");
        return 0;
    }

    private async Task<int> MigratePaymentMethodsAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating payment methods...");
        return 0;
    }

    private async Task<int> MigrateServiceCatalogAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating service catalog...");
        return 0;
    }

    private async Task<int> MigrateEmployeesAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating employees...");
        return 0;
    }

    public static decimal ParseLegacyDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0m;
        value = value.Trim().Replace(',', '.');
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return Math.Round(result, 2);
        return 0m;
    }

    public static decimal ConvertHrkToEur(decimal hrkAmount)
    {
        const decimal fixedRate = 7.5345m;
        return Math.Round(hrkAmount / fixedRate, 2);
    }

    public class MigrationResult
    {
        public int TotalRecords { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public TimeSpan Elapsed { get; set; }

        public override string ToString()
        {
            return $"""
                === Migration Report ===
                Total: {TotalRecords} records
                Success: {SuccessCount}
                Errors: {ErrorCount}
                Elapsed: {Elapsed:hh\:mm\:ss}

                {(Errors.Count > 0 ? "Errors:\n" + string.Join("\n", Errors) : "No errors")}
                """;
        }
    }
}
