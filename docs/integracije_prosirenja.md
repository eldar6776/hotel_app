# Integracije i proširenja

## 1. OCR čitač pasoša i ličnih karata

### Problem
Recepcioner ručno prepisuje ime, prezime, broj pasoša, datum rođenja, državljanstvo — gubi se 2-3 min po gostu, greške su česte.

### Rješenje: Web kamera + JS OCR (na frontendu)

Gost prisloni pasoš pred kameru telefona ili web kameru na recepciji. MRZ (Machine Readable Zone) se očita automatski.

```
┌──────────────────────────────────────┐
│  📷  Skenirajte pasoš                │
│                                      │
│  ┌──────────────────────────────┐   │
│  │                              │   │
│  │   [kamera preko pregledača] │   │
│  │                              │   │
│  └──────────────────────────────┘   │
│                                      │
│  Očitano:                            │
│  Ime: Marko                    [✓]  │
│  Prezime: Marković            [✓]  │
│  Broj pasoša: EE123456        [✓]  │
│  Datum rođenja: 15.03.1985.   [✓]  │
│  Državljanstvo: Hrvatska      [✓]  │
│                                      │
│  [▸ Potvrdi i nastavi]             │
└──────────────────────────────────────┘
```

### MRZ format

Pasos (TD3):
```
P<HRVIMAROVIC<<MARKO<<<<<<<<<<<<<<<<<<<<<<<
EE123456<0HRV8503151M2605152<<<<<<<<<<<<<<02
```

Licna karta (TD1):
```
IDHRV123456<7<<<<<<<<<<<<<<<
8503151M2605152HRV<<<<<<<<<<<1
MAROVIC<<MARKO<<<<<<<<<<<<<<
```

### Implementacija (sa postojećim OCR skenerom)

Hotel već ima skener koji vraća MRZ tekst. Potreban je samo **parser** — 50 linija koda:

```javascript
// frontend/utils/mrzParser.js
export function parseMrz(rawText) {
  const lines = rawText.trim().split('\n');
  const join = (s, start, end) => s.substring(start, end).replace(/<+$/, '').replace(/</g, ' ').trim();

  =if (lines[0].startsWith('P<')) {
    return {
      type: 'PASSPORT',
      surname: join(lines[0], 5, 44),
      givenName: join(lines[0], 44),
      documentNumber: lines[1].substring(0, 9).replace('<', ''),
      nationality: lines[1].substring(10, 13),
      birthDate: parseMrzDate(lines[1].substring(13, 19)),
      gender: lines[1].substring(19, 20),
      expiryDate: parseMrzDate(lines[1].substring(21, 27)),
    };
  }

  if (/\d{6}<{2}/.test(lines[0])) {
    return {
      type: 'ID_CARD',
      documentNumber: lines[0].substring(5, 14).replace('<', ''),
      birthDate: parseMrzDate(lines[1].substring(0, 6)),
      gender: lines[1].substring(6, 7),
      expiryDate: parseMrzDate(lines[1].substring(7, 13)),
      nationality: lines[1].substring(14, 17),
      surname: join(lines[2], 0, 30),
      givenName: join(lines[2], 30),
    };
  }
}

// MRZ format: YYMMDD → DD.MM.YYYY
function parseMrzDate(mrz) {
  const year = parseInt(mrz.substring(0, 2));
  const month = mrz.substring(2, 4);
  const day = mrz.substring(4, 6);
  const fullYear = year > 30 ? 1900 + year : 2000 + year;
  return `${day}.${month}.${fullYear}.`;
}
```

Integracija u formu:
```html
<textarea id="mrzInput" rows="3"
  placeholder="Skenirajte pasos... (MRZ tekst se automatski parsira)"
  onchange="autoFillGuestForm(parseMrz(this.value))">
</textarea>
```

Gost ili recepcioner skenira dokument → skener upisuje MRZ tekst u polje → parser odmah popunjava ostatak forme. Samo se potvrde podaci.plementacija

**Frontend (JavaScript) — `tesseract.js` ili `Dynamsoft`:**

```javascript
// frontend/components/MrzScanner.tsx
import { MRZReader } from '@mrz-reader/core';

export function MrzScanner({ onParsed }: { onParsed: (data: GuestData) => void }) {
    const handleImage = async (image: string) => {
        const result = await MRZReader.parse(image);
        
        if (result.type === 'PASSPORT') {
            onParsed({
                firstName: result.givenNames,
                lastName: result.surname,
                documentNumber: result.documentNumber,
                birthDate: result.birthDate,        // → format za formu
                nationality: result.nationality,      // → ISO code
                gender: result.sex,
                expiryDate: result.expirationDate,
                documentType: 'PASSPORT'
            });
        }
    };

    return <WebCamera onCapture={handleImage} />;
}
```

**Backend — verifikacija i validacija:**

```csharp
[HttpPost("api/guest/scan-document")]
public async Task<IActionResult> ScanDocument([FromBody] MrzResult scan)
{
    // 1. Validacija checksum-a (MRZ ima kontrolne znamenke)
    if (!MrzValidator.Validate(scan.RawMrz))
        return BadRequest("Neispravan MRZ");

    // 2. Izracun godina (provjera punoljetnosti)
    var age = DateTime.Today.Year - scan.BirthDate.Year;
    if (age < 18) return BadRequest("Osoba je maloljetna");

    // 3. Provjera isteka dokumenta
    if (scan.ExpiryDate < DateTime.Today)
        return BadRequest("Dokument je istekao");

    // 4. Auto-pretraga: da li gost vec postoji u bazi?
    var existing = await _guestRepo.FindByDocumentAsync(
        scan.DocumentNumber, scan.DocumentType);

    if (existing != null)
    {
        return Ok(new { guest = existing, isNew = false });
    }

    return Ok(new { guest = MapToGuest(scan), isNew = true });
}
```

### Biblioteke

| Biblioteka | Platforma | MRZ podrška | Cijena |
|-----------|-----------|-------------|--------|
| `tesseract.js` | Frontend (JS) | Osnovna | Besplatno |
| `Dynamsoft` | Frontend (JS) | Napredna + kamera | $999/mj |
| `tess4j` | Backend (Java/.NET) | Potpuna | Besplatno |
| `Google ML Kit` | Mobilna app | Odlična | Besplatno |

---

### Smart Room � autentifikacija

Tri opcije za dostavu pristupa gostu (bira se najjeftiniji/najefikasniji za dati hotel):
1. QR kod na papiru (�tampa na recepciji)
2. QR kod na WhatsApp poruci
3. NFC na telefonu (samo Android)

Sve tri opcije su podr�ane. Backend je isti � razlikuje se samo frontend tok.

### Smart Room � PWA
Samo privremeni pristup kroz web browser (PWA). Nije potrebna native aplikacija (iOS/Android).

### Smart Room � prioritet funkcionalnosti
Prva verzija: Klima, DND, Racun i express check-out, Prijava kvara.
Druga verzija: TV, Svjetlo/zavjese, Room service, Minibar pregled.

## 2. Plugin sistem za eksterne API-je

### Problem
Svaki hotel ima različite integracije (različiti TZ servisi, različiti channel manageri). Kod se ne smije mijenjati za svakog klijenta.

### Rješenje: Plugin arhitektura (isti pattern kao hardware driveri)

```
backend/plugins/
├── TouristRegistration/       ← pluginovi za zakonske prijave
│   ├── PrijavaBa.cs           ← prijava.ba API
│   ├── EstranacBa.cs          ← eStranac.ba API
│   └── MockTz.cs              ← za testiranje
│
├── ChannelManagers/           ← pluginovi za distribuciju
│   ├── Channex.cs             ← Channex API
│   ├── BookingCom.cs          ← Booking.com
│   ├── Airbnb.cs              ← Airbnb API
│   └── MockChannel.cs
│
├── PaymentGateways/           ← pluginovi za plaćanje
│   ├── Stripe.cs
│   ├── Monri.cs               ← lokalni BH provider
│   └── MockPayment.cs
│
└── PluginLoader.cs            ← učitava DLL-ove runtime
```

### Plugin interfejsi

```csharp
// Contracts/IPlugin.cs
public interface IPlugin
{
    string Id { get; }              // "prijava.ba", "channex", "stripe"
    string Name { get; }
    Version Version { get; }
    Task<bool> TestConnectionAsync();
}

// Tourist registration
public interface ITouristRegistrationPlugin : IPlugin
{
    Task<TzResult> RegisterGuestAsync(GuestRegistration guest);
    Task<TzResult> UpdateGuestAsync(GuestRegistration guest);
    Task<List<Country>> GetCountriesAsync();
    Task<List<DocumentType>> GetDocumentTypesAsync();
    Task<List<TzSubject>> GetSubjectsAsync();
}

// Channel Manager
public interface IChannelManagerPlugin : IPlugin
{
    Task SyncAvailabilityAsync(DateRange period, List<RoomAvailability> rooms);
    Task<BookingPushResult> PushBookingAsync(ExternalBooking booking);
    Task<List<ExternalBooking>> PullBookingsAsync(DateRange period);
    Task SyncRatesAsync(List<RatePlan> rates);
}
```

### Konfiguracija po hotelu

```json
{
  "Hotel": {
    "Id": "uuid-hotel-1",
    "Plugins": {
      "TouristRegistration": {
        "Plugin": "PrijavaBa",
        "Settings": {
          "ApiUrl": "https://prijava.ba/api",
          "Username": "hotel1_user",
          "Password": "***",
          "SubjektId": "12345"
        }
      },
      "ChannelManager": {
        "Plugin": "Channex",
        "Settings": {
          "ApiKey": "channex_key_123",
          "PropertyId": "prop_456"
        }
      },
      "PaymentGateway": {
        "Plugin": "Monri"
      }
    }
  }
}
```

---

## 3. eStranac.ba / Prijava.ba — zakonski obavezne prijave

### Šta je zakonski obavezno

U BiH, svaki hotel je dužan prijaviti boravak stranih državljana u roku od 24h. Postoje dva sistema:

- **eStranac.ba** — državni sistem Ministarstva sigurnosti BiH (Služba za poslove sa strancima)
- **Prijava.ba** — sistem Turističke zajednice KS (javni API)

### eStranac.ba integracija

Legacy kod (`clasTZ.vb`) već komunicira sa sličnim API-jem preko HTTP GET + XML. Novi plugin:

```csharp
// plugins/TouristRegistration/EstranacBa.cs
public class EstranacBa : ITouristRegistrationPlugin
{
    private readonly HttpClient _http;
    private string _baseUrl;
    private string _username;
    private string _password;

    public async Task<TzResult> RegisterGuestAsync(GuestRegistration guest)
    {
        // Prijava.ba API (iz legacy koda)
        var url = $"{_baseUrl}?user={_username}&pass={_password}" +
                  $"&subjekt={_subjektId}&res=xml" +
                  $"&akcija=prijava" +
                  $"&ime={UrlEncode(guest.FirstName)}" +
                  $"&prezime={UrlEncode(guest.LastName)}" +
                  $"&datum_rodjenja={guest.BirthDate:dd.MM.yyyy}" +
                  $"&drzavljanstvo={guest.Nationality}" +
                  $"&br_dokumenta={guest.DocumentNumber}" +
                  $"&vrsta_dokumenta={guest.DocumentType}" +
                  $"&datum_do={guest.DocumentExpiry:dd.MM.yyyy}" +
                  $"&datum_prijave={guest.CheckIn:dd.MM.yyyy}" +
                  $"&datum_odjave={guest.CheckOut:dd.MM.yyyy}";

        var xml = await _http.GetStringAsync(url);
        var doc = XDocument.Parse(xml);

        return new TzResult
        {
            Success = doc.Root.Element("status")?.Value == "ok",
            RemoteId = doc.Root.Element("id")?.Value,
            Error = doc.Root.Element("message")?.Value
        };
    }
}
```

### Automatizacija prijave

Prijava se ne radi ručno. Sistem je automatski okida:

```
Check-in gosta
    ↓
Da li je gost stranac? (drzavljanstvo ≠ BiH)
    ↓ DA
Pokreni TZ plugin (eStranac.ba / Prijava.ba)
    ↓
Uspjeh? → sačuvaj tid (remote ID) u relgostsoba
Greška? → loguj + notifikacija recepciji
```

```csharp
// Backend servis
public class TouristRegistrationService
{
    public async Task RegisterIfRequiredAsync(RoomAssignment assignment)
    {
        var guest = await _guestRepo.GetByIdAsync(assignment.GuestId);
        if (guest.Nationality == "BIH") return; // samo stranci

        var plugin = _pluginLoader.Get<ITouristRegistrationPlugin>();
        if (plugin == null) return;

        try
        {
            var result = await plugin.RegisterGuestAsync(new GuestRegistration
            {
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                Nationality = guest.Nationality,
                DocumentNumber = guest.DocumentNumber,
                CheckIn = assignment.CheckInDate,
                CheckOut = assignment.CheckOutDate
            });

            if (result.Success)
            {
                assignment.TzId = result.RemoteId;
                await _assignmentRepo.UpdateAsync(assignment);
            }
            else
            {
                await _notificationService.WarnReceptionAsync(
                    $"TZ prijava nije uspjela: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            await _notificationService.WarnReceptionAsync(
                $"TZ servis nedostupan: {ex.Message}");
        }
    }
}
```

### TZ API plugin — default

Ako hotel nema konfigurisan TZ plugin, koristi se interni API koji legacy `clasTZ.vb` već koristi:

```
URL: {setings.sobekuc}?user={user}&pass={pass}&subjekt={subjekt}&res=xml
Akcije: prijava, promjena, drzave, pi, status, subjekti
```

---

## 4. Channel Manager — Channex integracija

### Šta je Channex

Channex (channex.io) je API-first Channel Manager koji omogućava:
- Sinhronizaciju dostupnosti soba (availability)
- Sinhronizaciju cijena (rates)
- Primanje rezervacija iz više kanala (Booking.com, Airbnb, Expedia)
- API: REST + JSON, API key autentifikacija

### Channex plugin

```csharp
// plugins/ChannelManagers/Channex.cs
public class Channex : IChannelManagerPlugin
{
    private readonly HttpClient _http;

    public async Task SyncAvailabilityAsync(DateRange period, List<RoomAvailability> rooms)
    {
        // PUT /api/v1/properties/{id}/inventory
        var request = new InventoryRequest
        {
            PropertyId = _settings.PropertyId,
            Inventory = rooms.Select(r => new InventoryItem
            {
                RoomTypeId = MapRoomType(r.RoomType),
                Date = r.Date.ToString("yyyy-MM-dd"),
                Available = r.AvailableCount,
                MinStay = r.MinStay ?? 1
            }).ToList()
        };

        var response = await _http.PutAsJsonAsync(
            $"{_baseUrl}/api/v1/properties/{_settings.PropertyId}/inventory",
            request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<ExternalBooking>> PullBookingsAsync(DateRange period)
    {
        // GET /api/v1/properties/{id}/bookings?from=...&to=...
        var response = await _http.GetFromJsonAsync<List<ChannexBooking>>(
            $"{_baseUrl}/api/v1/properties/{_settings.PropertyId}/" +
            $"bookings?from={period.From:yyyy-MM-dd}&to={period.To:yyyy-MM-dd}");

        return response.Select(MapToExternalBooking).ToList();
    }

    private ExternalBooking MapToExternalBooking(ChannexBooking cb)
    {
        return new ExternalBooking
        {
            Source = "Booking.com",       // ili Airbnb, Expedia...
            ExternalId = cb.Id,
            GuestName = cb.GuestName,
            CheckIn = cb.CheckIn,
            CheckOut = cb.CheckOut,
            RoomType = cb.RoomTypeId,
            Adults = cb.Adults,
            Children = cb.Children,
            TotalPrice = cb.TotalAmount,
            Currency = cb.Currency,
            Status = MapStatus(cb.Status)
        };
    }
}
```

### Sinhronizacija

```
Hotel sistem                    Channex API
    │                              │
    │──→ POST check-in          ──→│
    │                              │
    │──→ POST check-out (odjava) ─→│  ──→ Booking.com (zauzeto)
    │                              │       Airbnb (zauzeto)
    │←── GET bookings (period) ────│  ──→ Nova rezervacija sa Booking.com
    │                              │
    │──→ PUT inventory             │
    │──→ PUT rates                 │
```

Triggeri:
- `booking.created` → Channex: zatvori sobu
- `booking.cancelled` → Channex: otvori sobu
- `checkin.completed` → Channex: potvrdi
- `checkout.completed` → Channex: oslobodi sobu
- `rate.updated` → Channex: novi cjenovnik

### Ostali Channel Manageri

| Platforma | API tip | Tržište |
|-----------|---------|---------|
| **Channex** | REST JSON | Globalno |
| **BookLogic** | SOAP/XML | Evropa |
| **RateGain** | REST JSON | Globalno |
| **SiteMinder** | REST XML | Globalno |
| **Octorate** | REST JSON | Balkan/Evropa |
| **Direct API (Booking.com)** | REST XML | Globalno |
| **Direct API (Airbnb)** | REST JSON | Globalno |

---

## 5. Pregled svih pluginova

| Plugin kategorija | Primjeri | Status |
|-------------------|----------|--------|
| **Hardware driveri** | LuxM.Http, Salto.Tcp, KardImedia.Tcp, Tring.Fiscal | Core (obavezno) |
| **Tourist registration** | PrijavaBa, EstranacBa, HrEVisitor | Per hotel (zakonski) |
| **Channel Manager** | Channex, BookLogic, RateGain | Per hotel (opciono) |
| **Payment Gateway** | Stripe, Monri, PayPal | Per hotel (opciono) |
| **OCR dokumenti** | Tesseract, Dynamsoft | Frontend (ugrađeno) |

---

## 6. Redoslijed implementacije

1. **OCR čitač** — može odmah (frontend biblioteka, ne zahtijeva backend promjene)
2. **TZ plugin** — legacy `clasTZ.vb` već postoji, samo treba refaktor u plugin arhitekturu
3. **Plugin sistem** — prije bilo kakve integracije sa Channex-om
4. **Channel Manager** — tek nakon stabilnih core modula (Sobe, Rezervacije, Naplata)
