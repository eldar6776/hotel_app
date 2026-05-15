using System.Text.Json;

namespace HotelPro.Core.Services;

public class TranslationService : ITranslationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private static readonly string[] SupportedLanguages = { "hr", "en", "de", "it" };

    public TranslationService()
    {
        LoadTranslations();
    }

    private void LoadTranslations()
    {
        foreach (var lang in SupportedLanguages)
        {
            _translations[lang] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        LoadLanguage("hr", new Dictionary<string, string>
        {
            ["common.save"] = "Spremi",
            ["common.cancel"] = "Odustani",
            ["common.delete"] = "Izbrisi",
            ["common.search"] = "Pretraga",
            ["common.edit"] = "Uredi",
            ["common.add"] = "Dodaj",
            ["common.close"] = "Zatvori",
            ["common.yes"] = "Da",
            ["common.no"] = "Ne",
            ["common.loading"] = "Ucitavanje",
            ["booking.create"] = "Nova rezervacija",
            ["booking.status.confirmed"] = "Potvrdjeno",
            ["booking.status.checkedin"] = "Prijavljen",
            ["booking.status.checkedout"] = "Odjavljen",
            ["booking.status.cancelled"] = "Otkazano",
            ["booking.status.noshow"] = "Nije dosao",
            ["booking.status.provisional"] = "Provizorno",
            ["room.status.free"] = "Slobodna",
            ["room.status.occupied"] = "Zauzeta",
            ["room.status.reserved"] = "Rezervisana",
            ["room.status.dirty"] = "Prijava",
            ["room.status.ooo"] = "Van funkcije",
            ["room.status.outofservice"] = "Van usluge",
            ["folio.balance"] = "Stanje racuna",
            ["folio.payment"] = "Uplata",
            ["folio.charge"] = "Trosak",
            ["error.notfound"] = "Nije pronadjeno",
            ["error.validation"] = "Greska pri validaciji",
            ["error.unauthorized"] = "Neautorizirani pristup",
            ["error.server"] = "Interna greska servera",
            ["auth.login"] = "Prijava",
            ["auth.logout"] = "Odjava",
            ["auth.unauthorized"] = "Neautorizirani pristup",
            ["guest.create"] = "Novi gost",
            ["guest.search"] = "Pretraga gostiju",
            ["housekeeping.clean"] = "Ciscenje",
            ["housekeeping.inspected"] = "Inspekcija",
            ["housekeeping.repaired"] = "Popravljeno"
        });

        LoadLanguage("en", new Dictionary<string, string>
        {
            ["common.save"] = "Save",
            ["common.cancel"] = "Cancel",
            ["common.delete"] = "Delete",
            ["common.search"] = "Search",
            ["common.edit"] = "Edit",
            ["common.add"] = "Add",
            ["common.close"] = "Close",
            ["common.yes"] = "Yes",
            ["common.no"] = "No",
            ["common.loading"] = "Loading",
            ["booking.create"] = "New Booking",
            ["booking.status.confirmed"] = "Confirmed",
            ["booking.status.checkedin"] = "Checked In",
            ["booking.status.checkedout"] = "Checked Out",
            ["booking.status.cancelled"] = "Cancelled",
            ["booking.status.noshow"] = "No Show",
            ["booking.status.pending"] = "Pending",
            ["room.status.free"] = "Free",
            ["room.status.occupied"] = "Occupied",
            ["room.status.reserved"] = "Reserved",
            ["room.status.dirty"] = "Dirty",
            ["room.status.ooo"] = "Out of Order",
            ["room.status.outofservice"] = "Out of Service",
            ["folio.balance"] = "Account Balance",
            ["folio.payment"] = "Payment",
            ["folio.charge"] = "Charge",
            ["error.notfound"] = "Not Found",
            ["error.validation"] = "Validation Error",
            ["error.unauthorized"] = "Unauthorized Access",
            ["error.server"] = "Internal Server Error",
            ["auth.login"] = "Login",
            ["auth.logout"] = "Logout",
            ["auth.unauthorized"] = "Unauthorized Access",
            ["guest.create"] = "New Guest",
            ["guest.search"] = "Search Guests",
            ["housekeeping.clean"] = "Cleaning",
            ["housekeeping.inspected"] = "Inspected",
            ["housekeeping.repaired"] = "Repaired"
        });

        LoadLanguage("de", new Dictionary<string, string>
        {
            ["common.save"] = "Speichern",
            ["common.cancel"] = "Abbrechen",
            ["common.delete"] = "Loschen",
            ["common.search"] = "Suche",
            ["common.edit"] = "Bearbeiten",
            ["common.add"] = "Hinzufugen",
            ["common.close"] = "SchlieBen",
            ["common.yes"] = "Ja",
            ["common.no"] = "Nein",
            ["booking.create"] = "Neue Buchung",
            ["booking.status.confirmed"] = "Bestatigt",
            ["booking.status.checkedin"] = "Eingecheckt",
            ["booking.status.checkedout"] = "Ausgecheckt",
            ["booking.status.cancelled"] = "Storniert",
            ["booking.status.noshow"] = "Nicht erschienen",
            ["room.status.free"] = "Frei",
            ["room.status.occupied"] = "Belegt",
            ["room.status.reserved"] = "Reserviert",
            ["room.status.dirty"] = "Schmutzig",
            ["room.status.ooo"] = "AuBer Betrieb",
            ["folio.balance"] = "Kontostand",
            ["folio.payment"] = "Zahlung",
            ["error.notfound"] = "Nicht gefunden",
            ["auth.login"] = "Anmeldung",
            ["auth.logout"] = "Abmeldung"
        });

        LoadLanguage("it", new Dictionary<string, string>
        {
            ["common.save"] = "Salva",
            ["common.cancel"] = "Annulla",
            ["common.delete"] = "Elimina",
            ["common.search"] = "Cerca",
            ["common.edit"] = "Modifica",
            ["common.add"] = "Aggiungi",
            ["common.close"] = "Chiudi",
            ["common.yes"] = "Si",
            ["common.no"] = "No",
            ["booking.create"] = "Nuova Prenotazione",
            ["booking.status.confirmed"] = "Confermato",
            ["booking.status.checkedin"] = "Check-in",
            ["booking.status.checkedout"] = "Check-out",
            ["booking.status.cancelled"] = "Annullato",
            ["booking.status.noshow"] = "No Show",
            ["room.status.free"] = "Libera",
            ["room.status.occupied"] = "Occupata",
            ["room.status.reserved"] = "Prenotata",
            ["room.status.dirty"] = "Da pulire",
            ["room.status.ooo"] = "Fuori servizio",
            ["folio.balance"] = "Saldo conto",
            ["folio.payment"] = "Pagamento",
            ["error.notfound"] = "Non trovato",
            ["auth.login"] = "Accesso",
            ["auth.logout"] = "Uscita"
        });
    }

    private void LoadLanguage(string lang, Dictionary<string, string> translations)
    {
        if (_translations.ContainsKey(lang))
        {
            foreach (var kvp in translations)
            {
                _translations[lang][kvp.Key] = kvp.Value;
            }
        }
    }

    public string Translate(string key, string language)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;

        var lang = IsLanguageSupported(language) ? language : "en";

        if (_translations.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var value))
            return value;

        if (lang != "en" && _translations.TryGetValue("en", out var enDict) && enDict.TryGetValue(key, out var enValue))
            return enValue;

        if (lang != "hr" && _translations.TryGetValue("hr", out var hrDict) && hrDict.TryGetValue(key, out var hrValue))
            return hrValue;

        return $"[!{key}]";
    }

    public Task<string> TranslateAsync(string key, string language, CancellationToken ct = default) =>
        Task.FromResult(Translate(key, language));

    public bool IsLanguageSupported(string language) =>
        SupportedLanguages.Contains(language?.ToLowerInvariant());

    public IEnumerable<string> GetSupportedLanguages() => SupportedLanguages;
}
