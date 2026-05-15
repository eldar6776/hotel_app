namespace HotelPro.Core.Services;

public interface ITranslationService
{
    string Translate(string key, string language);
    Task<string> TranslateAsync(string key, string language, CancellationToken ct = default);
    bool IsLanguageSupported(string language);
    IEnumerable<string> GetSupportedLanguages();
}
