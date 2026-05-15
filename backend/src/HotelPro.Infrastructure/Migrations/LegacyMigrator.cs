using Microsoft.Extensions.Logging;
using HotelPro.Core.Entities;
using HotelPro.Infrastructure.Data;
using System.Globalization;
using MySqlConnector;

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
        var count = 0;
        idMapping["Zgrade"] = new Dictionary<int, Guid>();

        await using var conn = new MySqlConnection(_legacyConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new MySqlCommand("SELECT ID, Naziv, Oznaka, Adresa, Grad FROM Zgrade WHERE Aktivan = 1", conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            try
            {
                var legacyId = reader.GetInt32("ID");
                var newId = Guid.NewGuid();
                idMapping["Zgrade"][legacyId] = newId;

                _targetDbContext.Buildings.Add(new Building
                {
                    Id = newId,
                    Name = reader.GetString("Naziv"),
                    Code = reader.GetString("Oznaka"),
                    Address = reader.IsDBNull(reader.GetOrdinal("Adresa")) ? null : reader.GetString("Adresa"),
                    City = reader.IsDBNull(reader.GetOrdinal("Grad")) ? null : reader.GetString("Grad"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                _targetDbContext.LegacyIdMappings.Add(new LegacyIdMapping
                {
                    Id = Guid.NewGuid(),
                    EntityType = "Building",
                    LegacyTableName = "Zgrade",
                    LegacyId = legacyId,
                    NewId = newId,
                    MigratedAt = DateTime.UtcNow
                });

                count++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate building row");
            }
        }

        if (count > 0) await _targetDbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Migrated {Count} buildings", count);
        return count;
    }

    private async Task<int> MigrateRoomTypesAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating room types...");
        var count = 0;
        idMapping["VrsteSoba"] = new Dictionary<int, Guid>();

        await using var conn = new MySqlConnection(_legacyConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new MySqlCommand("SELECT ID, Naziv, Oznaka, Kapacitet, MaxKapacitet, Cijena, Opis FROM VrsteSoba WHERE Aktivan = 1", conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            try
            {
                var legacyId = reader.GetInt32("ID");
                var newId = Guid.NewGuid();
                idMapping["VrsteSoba"][legacyId] = newId;

                _targetDbContext.RoomTypes.Add(new RoomType
                {
                    Id = newId,
                    Name = reader.GetString("Naziv"),
                    Code = reader.GetString("Oznaka"),
                    BaseCapacity = reader.GetInt32("Kapacitet"),
                    MaxCapacity = reader.GetInt32("MaxKapacitet"),
                    DefaultPrice = reader.GetDecimal("Cijena"),
                    Description = reader.IsDBNull(reader.GetOrdinal("Opis")) ? null : reader.GetString("Opis"),
                    IsActive = true,
                    SortOrder = count + 1
                });

                _targetDbContext.LegacyIdMappings.Add(new LegacyIdMapping
                {
                    Id = Guid.NewGuid(),
                    EntityType = "RoomType",
                    LegacyTableName = "VrsteSoba",
                    LegacyId = legacyId,
                    NewId = newId,
                    MigratedAt = DateTime.UtcNow
                });

                count++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate room type row");
            }
        }

        if (count > 0) await _targetDbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Migrated {Count} room types", count);
        return count;
    }

    private async Task<int> MigrateRoomsAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating rooms...");
        var count = 0;
        idMapping["Sobe"] = new Dictionary<int, Guid>();

        if (!idMapping.TryGetValue("Zgrade", out var buildingMap) || !idMapping.TryGetValue("VrsteSoba", out var roomTypeMap))
        {
            _logger.LogWarning("Skipping rooms — buildings or room types not migrated yet");
            return 0;
        }

        await using var conn = new MySqlConnection(_legacyConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new MySqlCommand("SELECT ID, BrojSobe, Sprat, ZgradaID, VrstaSobeID, Status, Cijena FROM Sobe WHERE Aktivan = 1", conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            try
            {
                var legacyId = reader.GetInt32("ID");
                var buildingLegacyId = reader.GetInt32("ZgradaID");
                var roomTypeLegacyId = reader.GetInt32("VrstaSobeID");

                if (!buildingMap.TryGetValue(buildingLegacyId, out var buildingId)) continue;
                if (!roomTypeMap.TryGetValue(roomTypeLegacyId, out var roomTypeId)) continue;

                var newId = Guid.NewGuid();
                idMapping["Sobe"][legacyId] = newId;

                _targetDbContext.Rooms.Add(new Room
                {
                    Id = newId,
                    RoomNumber = reader.GetString("BrojSobe"),
                    Floor = reader.GetInt32("Sprat"),
                    BuildingId = buildingId,
                    RoomTypeId = roomTypeId,
                    Status = ParseLegacyRoomStatus(reader.GetString("Status")),
                    BasePrice = reader.IsDBNull(reader.GetOrdinal("Cijena")) ? null : reader.GetDecimal("Cijena"),
                    IsActive = true,
                    SortOrder = count + 1
                });

                _targetDbContext.LegacyIdMappings.Add(new LegacyIdMapping
                {
                    Id = Guid.NewGuid(),
                    EntityType = "Room",
                    LegacyTableName = "Sobe",
                    LegacyId = legacyId,
                    NewId = newId,
                    MigratedAt = DateTime.UtcNow
                });

                count++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate room row");
            }
        }

        if (count > 0) await _targetDbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Migrated {Count} rooms", count);
        return count;
    }

    private async Task<int> MigrateCountriesAsync(Dictionary<string, Dictionary<int, Guid>> idMapping, CancellationToken ct)
    {
        _logger.LogInformation("Migrating countries...");
        var count = 0;
        idMapping["Drzave"] = new Dictionary<int, Guid>();

        await using var conn = new MySqlConnection(_legacyConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new MySqlCommand("SELECT ID, Oznaka, Naziv, Nacionalnost, PozivniBroj, Valuta FROM Drzave WHERE Aktivan = 1", conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            try
            {
                var legacyId = reader.GetInt32("ID");
                var newId = Guid.NewGuid();
                idMapping["Drzave"][legacyId] = newId;

                _targetDbContext.Countries.Add(new Country
                {
                    Id = newId,
                    Code = reader.GetString("Oznaka"),
                    Name = reader.GetString("Naziv"),
                    Nationality = reader.IsDBNull(reader.GetOrdinal("Nacionalnost")) ? "" : reader.GetString("Nacionalnost"),
                    PhoneCode = reader.IsDBNull(reader.GetOrdinal("PozivniBroj")) ? "" : reader.GetString("PozivniBroj"),
                    CurrencyCode = reader.IsDBNull(reader.GetOrdinal("Valuta")) ? "EUR" : reader.GetString("Valuta")
                });

                _targetDbContext.LegacyIdMappings.Add(new LegacyIdMapping
                {
                    Id = Guid.NewGuid(),
                    EntityType = "Country",
                    LegacyTableName = "Drzave",
                    LegacyId = legacyId,
                    NewId = newId,
                    MigratedAt = DateTime.UtcNow
                });

                count++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate country row");
            }
        }

        if (count > 0) await _targetDbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Migrated {Count} countries", count);
        return count;
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

    private static HotelPro.Core.Enums.RoomStatus ParseLegacyRoomStatus(string status)
    {
        return status?.ToLowerInvariant() switch
        {
            "slobodna" or "free" => HotelPro.Core.Enums.RoomStatus.Free,
            "zauzeta" or "occupied" => HotelPro.Core.Enums.RoomStatus.Occupied,
            "rezervisana" or "reserved" => HotelPro.Core.Enums.RoomStatus.Reserved,
            "prljava" or "dirty" => HotelPro.Core.Enums.RoomStatus.Dirty,
            "van funkcije" or "ooo" => HotelPro.Core.Enums.RoomStatus.OutOfOrder,
            _ => HotelPro.Core.Enums.RoomStatus.Free
        };
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
