using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface IExchangeRateService
{
    Task<List<ExchangeRateDto>> GetCurrentRatesAsync();
    Task UpdateRateAsync(string currencyCode, decimal rate);
    Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
}
