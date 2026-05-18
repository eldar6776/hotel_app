using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HotelPro.Infrastructure.Services;

public class ConfigurationService : IConfigurationService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly IReadOnlyList<DefaultConfig> Defaults =
    [
        new("Hotel", "VatRate", "17", false, true, "Primary VAT rate (%)"),
        new("Hotel", "ReducedVatRate", "0", false, true, "Reduced VAT rate (%)"),
        new("Hotel", "TouristTax", "0", false, true, "Tourist tax per night"),
        new("Hotel", "CheckInHour", "14", false, true, "Check-in hour (0-23)"),
        new("Hotel", "CheckOutHour", "12", false, true, "Check-out hour (0-23)"),
        new("Hotel", "CurrencyCode", "EUR", false, true, "Default currency code"),
        new("Hotel", "BillingMode", "PerPerson", false, true, "Billing mode: PerPerson or PerRoom"),
        new("Hotel", "HotelName", "", false, true, "Hotel name"),
        new("Hotel", "HotelCode", "", false, true, "Hotel code (for multi-tenant)"),
        new("Hotel", "HotelAddress", "", false, true, "Hotel address"),
        new("Hotel", "LateCheckoutFreeHour", "14", false, true, "Late check-out free until this hour"),
        new("Hotel", "LateCheckoutFeePercent", "50", false, true, "Late check-out fee (% of nightly rate)"),
        new("Hotel", "InfantMaxAge", "2", false, true, "Maximum age for Infant category"),
        new("Hotel", "ChildMaxAge", "11", false, true, "Maximum age for Child category"),
        new("Hotel", "SeniorMinAge", "65", false, true, "Minimum age for Senior category"),
        new("Hotel", "InfantDiscountPercent", "100", false, true, "Discount % for Infant (100 = free)"),
        new("Hotel", "ChildDiscountPercent", "50", false, true, "Discount % for Child"),
        new("Hotel", "SeniorDiscountPercent", "0", false, true, "Discount % for Senior"),
        new("Hotel", "NoShowChargeFirstNight", "true", false, true, "Charge first night on NoShow"),
        new("Hotel", "FiscalEnabled", "false", false, true, "Enable fiscal printer integration"),
        new("Hotel", "GuestRegistryEnabled", "false", false, true, "Enable foreign guest registry (MUP)"),
        new("ExchangeRate", "Provider", "Frankfurter", false, true, "Exchange rate provider: Frankfurter, OpenExchangeRates, Fixer, ExchangeRateAPI, CurrencyLayer, Manual"),
        new("ExchangeRate", "SyncInterval", "Daily", false, true, "Sync frequency: Daily, Hourly, Manual"),
        new("ExchangeRate", "SyncHour", "6", false, true, "Hour of day to sync (0-23) when Daily"),
        new("ExchangeRate", "OpenExchangeRates_ApiKey", "", true, false, "Open Exchange Rates API key"),
        new("ExchangeRate", "Fixer_ApiKey", "", true, false, "Fixer.io API key"),
        new("ExchangeRate", "ExchangeRateAPI_ApiKey", "", true, false, "ExchangeRate-API key"),
        new("ExchangeRate", "CurrencyLayer_ApiKey", "", true, false, "CurrencyLayer API key"),
        new("ExchangeRate", "SupportedCurrencies", "EUR,BAM,USD,GBP,HRK,CHF,TRY", false, true, "Comma-separated supported currency codes"),
        new("Language", "DefaultLanguage", "bs", false, true, "Default language code (bs, en, de, tr)"),
        new("Language", "SupportedLanguages", "bs,en,de,tr", false, true, "Comma-separated supported language codes"),
        new("Payment", "Stripe_ApiKey", "", true, false, "Stripe API key"),
        new("Payment", "Stripe_WebhookSecret", "", true, false, "Stripe webhook signing secret"),
        new("Payment", "Stripe_TestMode", "true", false, false, "Use Stripe test mode"),
        new("Channel", "BookingCom_ApiKey", "", true, false, "Booking.com API key"),
        new("Channel", "BookingCom_HotelCode", "", false, false, "Booking.com hotel code"),
        new("Channel", "Airbnb_ClientId", "", true, false, "Airbnb client ID"),
        new("Channel", "Airbnb_ClientSecret", "", true, false, "Airbnb client secret"),
        new("Fiscal", "DriverType", "Mock", false, false, "Fiscal driver type"),
        new("Fiscal", "SerialPort", "", false, false, "Fiscal device serial port"),
        new("Fiscal", "BaudRate", "9600", false, false, "Fiscal device baud rate"),
        new("Hardware", "Rfid_DriverType", "Mock", false, false, "RFID driver type"),
        new("Hardware", "Rfid_IpAddress", "", false, false, "RFID controller IP address"),
        new("Hardware", "Rfid_Port", "", false, false, "RFID controller port"),
        new("Pabx", "DriverType", "Mock", false, false, "PABX driver type"),
        new("Pabx", "IpAddress", "", false, false, "PABX IP address"),
        new("Pabx", "Port", "", false, false, "PABX port"),
        new("IoT", "Mqtt_BrokerUrl", "", false, false, "MQTT broker URL"),
        new("IoT", "Mqtt_Username", "", false, false, "MQTT username"),
        new("IoT", "Mqtt_Password", "", true, false, "MQTT password"),
        new("IoT", "Mqtt_Enabled", "false", false, false, "Enable MQTT integration"),
        new("Email", "Provider", "Smtp", false, true, "Email provider: Smtp, SendGrid, Brevo, None"),
        new("Email", "Smtp_Host", "", false, false, "SMTP host"),
        new("Email", "Smtp_Port", "587", false, false, "SMTP port"),
        new("Email", "Smtp_Username", "", false, false, "SMTP username"),
        new("Email", "Smtp_Password", "", true, false, "SMTP password"),
        new("Email", "Smtp_FromAddress", "", false, false, "SMTP from address"),
        new("Email", "Smtp_UseTls", "true", false, false, "Use TLS for SMTP"),
        new("Email", "SendGrid_ApiKey", "", true, false, "SendGrid API key"),
        new("Email", "Brevo_ApiKey", "", true, false, "Brevo API key"),
        new("Tourism", "Tz_ApiUrl", "", false, false, "Tourism board API URL"),
        new("Tourism", "Tz_Username", "", false, false, "Tourism board username"),
        new("Tourism", "Tz_Password", "", true, false, "Tourism board password"),
        new("Tourism", "Tz_Enabled", "false", false, false, "Enable tourism board integration"),
        new("Feature", "UseNewStayWorkflow", "true", false, true, "Use new StayLifecycleService for check-in"),
        new("Feature", "UseNewCheckOutWorkflow", "true", false, true, "Use new CheckOutWorkflowService for check-out"),
        new("Feature", "UseFolioLedger", "true", false, true, "Use FolioLedgerService for balance computation"),
        new("Feature", "UseNightLedger", "true", false, true, "Use NightLedgerService for night generation"),
        new("Feature", "UseInvoiceWorkflow", "true", false, true, "Use InvoiceWorkflowService for invoicing"),
        new("Feature", "UsePaymentAllocation", "true", false, true, "Use PaymentAllocationService for split payments"),
        new("Feature", "UseReservationPolicy", "true", false, true, "Use ReservationPolicyService for booking status changes")
    ];

    private readonly HotelProDbContext _dbContext;
    private readonly ITenantService _tenantService;
    private readonly IMemoryCache _cache;

    public ConfigurationService(
        HotelProDbContext dbContext,
        ITenantService tenantService,
        IMemoryCache cache)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _cache = cache;
    }

    public async Task<IReadOnlyList<HotelConfigDto>> GetAllAsync(bool includeSecrets = false)
    {
        await EnsureDefaultsAsync();

        var configs = await _dbContext.HotelConfigs
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Key)
            .ToListAsync();

        return configs.Select(x => MapToDto(x, includeSecrets)).ToList();
    }

    public async Task<IReadOnlyList<HotelConfigDto>> GetByCategoryAsync(string category, bool includeSecrets = false)
    {
        await EnsureDefaultsAsync();

        var configs = await _dbContext.HotelConfigs
            .Where(x => x.Category == category)
            .OrderBy(x => x.Key)
            .ToListAsync();

        return configs.Select(x => MapToDto(x, includeSecrets)).ToList();
    }

    public async Task<HotelConfigDto?> GetByKeyAsync(string key, bool includeSecrets = false)
    {
        await EnsureDefaultsAsync();

        var config = await _dbContext.HotelConfigs.FirstOrDefaultAsync(x => x.Key == key);
        return config == null ? null : MapToDto(config, includeSecrets);
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var hotelId = GetCurrentHotelId();
        var cacheKey = $"hotel_config_value_{hotelId}_{key}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            await EnsureDefaultsAsync();
            var config = await _dbContext.HotelConfigs.FirstOrDefaultAsync(x => x.Key == key);
            return config == null ? null : GetReadableValue(config);
        });
    }

    public async Task<bool> IsEnabledAsync(string key)
    {
        await EnsureDefaultsAsync();
        var config = await _dbContext.HotelConfigs.FirstOrDefaultAsync(x => x.Key == key);
        return config?.IsEnabled == true;
    }

    public async Task<HotelConfigDto> UpdateAsync(string key, UpdateHotelConfigDto dto)
    {
        await EnsureDefaultsAsync();

        var config = await _dbContext.HotelConfigs.FirstOrDefaultAsync(x => x.Key == key);
        if (config == null)
        {
            throw new KeyNotFoundException($"Configuration key '{key}' was not found.");
        }

        if (dto.Value != null)
        {
            config.Value = dto.Value;
        }

        if (dto.IsEnabled.HasValue)
        {
            config.IsEnabled = dto.IsEnabled.Value;
        }

        if (dto.Description != null)
        {
            config.Description = dto.Description;
        }

        config.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        ClearCache(config.HotelId, config.Key);

        return MapToDto(config, includeSecrets: false);
    }

    public async Task<IReadOnlyDictionary<string, object?>> GetPublicSettingsAsync()
    {
        await EnsureDefaultsAsync();

        var configs = await _dbContext.HotelConfigs
            .Where(x => !x.IsSecret && (x.Category == "Hotel" || x.Key.EndsWith("_Enabled")))
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Key)
            .ToListAsync();

        return configs.ToDictionary<HotelConfig, string, object?>(
            x => x.Key,
            x => x.IsEnabled ? GetReadableValue(x) : null);
    }

    public async Task EnsureDefaultsAsync()
    {
        var hotelId = GetCurrentHotelId();
        var existingKeys = await _dbContext.HotelConfigs
            .Select(x => x.Key)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var missing = Defaults
            .Where(x => !existingKeys.Contains(x.Key))
            .Select(x => new HotelConfig
            {
                Id = Guid.NewGuid(),
                HotelId = hotelId,
                Key = x.Key,
                Value = x.Value,
                Category = x.Category,
                Description = x.Description,
                IsSecret = x.IsSecret,
                IsEnabled = x.IsEnabled,
                CreatedAt = now,
                UpdatedAt = now
            })
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        _dbContext.HotelConfigs.AddRange(missing);
        await _dbContext.SaveChangesAsync();
    }

    private HotelConfigDto MapToDto(HotelConfig config, bool includeSecrets)
    {
        var value = config.IsSecret && !includeSecrets
            ? MaskSecret(config)
            : GetReadableValue(config);

        return new HotelConfigDto(
            config.Id,
            config.Key,
            value,
            config.Category,
            config.Description,
            config.IsSecret,
            config.IsEnabled,
            config.UpdatedAt);
    }

    private string GetReadableValue(HotelConfig config)
    {
        if (!config.IsSecret || string.IsNullOrEmpty(config.Value))
        {
            return config.Value;
        }

        return config.Value;
    }

    private string? MaskSecret(HotelConfig config)
    {
        return string.IsNullOrEmpty(config.Value) ? null : "********";
    }

    private Guid GetCurrentHotelId()
    {
        return _tenantService.GetCurrentHotelId()
            ?? throw new InvalidOperationException("Hotel tenant is required for configuration access.");
    }

    private void ClearCache(Guid hotelId, string key)
    {
        _cache.Remove($"hotel_config_value_{hotelId}_{key}");
    }

    private sealed record DefaultConfig(
        string Category,
        string Key,
        string Value,
        bool IsSecret,
        bool IsEnabled,
        string Description);
}
