# MODERNIZATION ROADMAP

**Datum:** 2026-05-17 (rekreirano)
**Scope:** Migracioni plan Legacy VB.NET → .NET 8 + Next.js + PostgreSQL
**Agent:** GitHub Copilot (Claude Sonnet 4.6)
**Pattern:** Strangler Fig + Vertical Slices

---

## PREGLED SITUACIJE

**Legacy stanje:**
- VB.NET 8.0 (Visual Basic 2008 era)
- WinForms MDI aplikacija
- MySQL bez ORM-a
- 150+ fajlova, ~2500KB koda
- 9/10 OWASP kategorija prisutno
- 0% test coverage
- 0% GDPR compliance

**Ciljni sistem:**
- .NET 8 + ASP.NET Core Web API
- Next.js 14+ frontend
- PostgreSQL
- Docker + CI/CD
- GDPR-compliant
- JWT autentikacija

---

## FAZA 0 — SECURITY HOTFIXES (HITNO)

> **Prioritet**: Odmah, bez čekanja migracije

Ovo su minimalni popravci na legacy sistemu koji moraju biti implementirani dok migracija traje.

### 0.1 SQL Injection Patching (Legacy MySQL)
**Mijenjati String Concatenation → MySqlParameter svuda:**

```vb
' STARO (RANJIVO):
mysqlExScalar("WHERE korisnik='" & user & "' AND pass='" & pass & "'")

' NOVO (SIGURNO):
Dim cmd As New MySqlCommand("WHERE korisnik=@user AND pass=@pass", conn)
cmd.Parameters.AddWithValue("@user", user)
cmd.Parameters.AddWithValue("@pass", pass)
```

Prioritizirani redosljed:
1. frmKorisnik (login — KRITIČNO P0)
2. frmTroskovi (charges)
3. rptRacunFrm (reporting)
4. frmMailKonfig (email config)
5. frmAlarm, frmRezervacije (datumi)
6. Sve ostale (20+ lokacija)

### 0.2 Password Hashing (Legacy)
**Implementirati BCrypt hash za zaposlene:**

```vb
' Koristiti BCrypt.Net-Next NuGet (ako se odlučimo dodati)
' Ili SHA-256 sa saltom (minimum prihvatljivo)
Dim salt As String = GenerateRandomSalt()
Dim hash As String = SHA256Hash(password + salt)
UPDATE radnici SET pass_hash=hash, pass_salt=salt WHERE id=@id
```

### 0.3 Credential Externalization
- Premjestiti MySQL credentials iz `set.xml` u Windows DPAPI encrypted file
- Premjestiti SMTP credentials iz baze u DPAPI
- Ukloniti hardcoded `nerminc:nermin1234` iz clasTZ.vb

### 0.4 XML Security Fix
```vb
' U clasTZ.vb — prevent XXE
Dim settings As New XmlReaderSettings()
settings.DtdProcessing = DtdProcessing.Prohibit
settings.XmlResolver = Nothing
Using reader As XmlReader = XmlReader.Create(srXmlData, settings)
    dstz.ReadXml(reader)
End Using
```

### 0.5 Disable HTTP C&C (ftpUploa.vb)
- Zakomentovati ili ukloniti automatski update mehanizam koji komunicira sa `http://i-web.info/`
- Dok je to u produkciji — remote attacker može pushati malicious update

---

## FAZA 1 — STRANGLER FIG SETUP

> **Cilj**: Novi sistem radi paralelno, postupno preuzima module

### 1.1 API Gateway (novi sistem prima pozive)
- ASP.NET Core API deployovan na isti server
- Next.js frontend deployovan
- Nginx reverse proxy: `/api/` → .NET API, `/` → Next.js
- Legacy VB.NET nastavlja raditi za module koji još nisu migrirani

### 1.2 Shared Database (privremeno)
- Novi API čita iz MySQL-a (read-only) dok se migracija odvija
- PostgreSQL se kreira paralelno
- Sync script: MySQL → PostgreSQL (nightly)
- Novi zapisi idu u PostgreSQL, legacy čita iz MySQL

### 1.3 Autentikacija (novi sistem)
```csharp
// JWT AuthN implementiran u novom API-u
// Login endpoint: POST /api/auth/login
// Response: { accessToken, refreshToken, expiresIn }
// Legacy VB.NET ostaje sa starim login-om dok nije migriran
```

---

## FAZA 2 — CORE DOMAIN MIGRATION

> **Redosljed**: Najvažniji moduli prvi

### 2.1 Room Management (Sobe)
**Module**: RoomsController, RoomService, RoomRepository
**Pokriva**: frmSobe.vb functionality
**Status tracking**: Implementirati sve 7 sobnih statusa iz legacy `fnSobaStatus`

```
Room Status Enum (novi sistem):
  Available = 0
  Occupied = 1
  OccupiedCheckoutToday = 2
  ReservedConfirmed = 3
  ReservedAndOccupied = 4
  OutOfOrder = 5
  ReservedUnconfirmed = 6
```

### 2.2 Guest Management (Gosti)
**Module**: GuestsController, GuestService, GuestRepository
**GDPR zahtjevi**:
- Column-level encryption za: broj dokumenta, JMBG, datum rojstva
- Right-to-forget endpoint: DELETE /api/guests/{id}
- Audit log: svaki pristup gostovim podacima loguje se u `guest_access_log`
- Retention policy: PII se brišu X dana nakon zadnjeg boravka

### 2.3 Booking (Rezervacije)
**Module**: ReservationsController, ReservationService
**Kritično**: Optimistic locking na sobi (RowVersion/ETag)

```csharp
// Availability check + insert u jednoj transakciji
await using var tx = await context.Database.BeginTransactionAsync();
var isAvailable = await roomRepo.IsAvailableAsync(roomId, from, to, tx);
if (!isAvailable) throw new RoomNotAvailableException();
await reservationRepo.CreateAsync(reservation, tx);
await tx.CommitAsync();
```

---

## FAZA 3 — OPERATIONAL WORKFLOWS

### 3.1 Check-in Service
```csharp
// Atomska transakcija za sve check-in korake
public async Task<CheckInResult> CheckInAsync(CheckInCommand cmd)
{
    await using var tx = await _db.BeginTransactionAsync();
    try {
        var folio = await _folioRepo.CreateAsync(cmd.GuestId, tx);        // UUID, ne MAX+1
        var stay  = await _stayRepo.CreateAsync(cmd, folio.Id, tx);
        await _nightsRepo.UpsertRangeAsync(stay, tx);                     // Ledger insert
        await _roomRepo.SetOccupiedAsync(cmd.RoomId, tx);
        await tx.CommitAsync();
        return CheckInResult.Success(folio.Id);
    } catch {
        await tx.RollbackAsync();
        throw;
    }
}
```

### 3.2 Check-out Service
```csharp
public async Task<CheckOutResult> CheckOutAsync(CheckOutCommand cmd)
{
    await using var tx = await _db.BeginTransactionAsync();
    var folio = await _folioRepo.CloseAsync(cmd.FolioId, tx);
    await _roomRepo.SetDirtyAsync(cmd.RoomId, tx);           // housekeeping trigger
    await _stayRepo.CloseAsync(cmd.StayId, tx);
    await tx.CommitAsync();
    // Fiskalizacija NAKON transakcije (idempotentna retry)
    await _fiscalService.FiscalizeAsync(folio, cancellationToken: cts.Token);
}
```

### 3.3 Payment Service
```csharp
// Tip plaćanja obavezan
// Popust zahtijeva Manager role
// Fiskalizacija u background worker (ne u request thread)
```

### 3.4 Night Audit Service
```csharp
// Atomski zaključuje dan
// Retry-safe (idempotent operacije)
// Šalje izvještaj emailom (ne FTP-om)
```

---

## FAZA 4 — INTEGRATIONS

### 4.1 Tourism Board (TZ) Integration
```csharp
public class TourismBoardClient : ITourismBoardClient
{
    private readonly HttpClient _http; // Registered with IHttpClientFactory
    
    // Credentials iz IConfiguration, ne hardcoded
    private readonly string _username;
    private readonly string _password;
    
    public async Task ReportGuestArrivalAsync(GuestArrival arrival)
    {
        // HTTPS endpoint (ne HTTP)
        // Credentials kao query params sa HTTPS (prihvatljivo za legacy API)
        // XML parsing sa DTD.Prohibit
        // Retry policy (Polly)
        // Circuit breaker
    }
}
```

### 4.2 Card System Integration
```csharp
// Zamijeniti Winsock COM sa vendor SDK ili modernim TCP
public class CardSystemClient : ICardSystemClient
{
    private readonly TcpClient _tcpClient; // System.Net.Sockets
    private readonly SslStream _sslStream; // TLS obavezan
    // IP i port iz IConfiguration
    // Autentikacija prema kartičnom sistemu
}
```

### 4.3 Email Service
```csharp
// SMTP credentials iz IConfiguration (ne iz baze u DataSet)
// SSL/TLS obavezan
// Background queue za slanje (ne u request thread)
public class EmailService : IEmailService
{
    // FluentEmail ili MailKit
    // Credentials iz IOptions<SmtpSettings>
    // SmtpSettings iz secrets.json / environment variables
}
```

---

## FAZA 5 — CUTOVER

### 5.1 Data Migration
- Migracija svih MySQL podataka u PostgreSQL
- Validacija integriteta (COUNT checks po tabeli)
- GDPR cleanup: enkriptovati sve PII kolone pri migraciji
- Password rehashing: u novom sistemu svi passwordi bcrypt-ovani

### 5.2 Legacy Decommission
- Legacy VB.NET app ostaje instaliran ali disabled (fallback)
- 30-day parallel operation period
- Monitoring + anomaly detection na novom sistemu
- Uklanjanje nakon validation period-a

---

## ARHITEKTURNI DIJAGRAM — TARGET

```
                    [Browser / Mobile]
                           │
                           │ HTTPS
                           ▼
                    [Next.js Frontend]
                    (Vercel / Docker)
                           │
                           │ HTTPS
                           ▼
                 [ASP.NET Core API — .NET 8]
                    ┌──────────────────┐
                    │  Controllers     │
                    │  Services        │
                    │  Repositories    │
                    │  Domain Events   │
                    └──────────────────┘
                           │
                  ┌────────┴────────┐
                  │                 │
                  ▼                 ▼
          [PostgreSQL]       [Background Workers]
          (Supabase /        - Night Audit
          Docker)            - TZ Reports
                             - Email Queue
                             - FTP → HTTPS migration
                  │
                  ▼
          [Redis Cache]
          (room status,
          tariff cache)
```

---

## PRIORITIZACIJA

| Redosljed | Faza | Estimacija | Blokeri |
|---|---|---|---|
| 1 | FAZA 0 — Security hotfixes | 1-2 tjedna | Pristup legacy kodu |
| 2 | FAZA 1 — Strangler Fig setup | 2-3 tjedna | DevOps setup |
| 3 | FAZA 2 — Core domain | 6-8 tjedana | GDPR spec, šifarnici |
| 4 | FAZA 3 — Operational workflows | 8-10 tjedana | Fiskalizacija API docs |
| 5 | FAZA 4 — Integrations | 4-6 tjedana | Vendor SDK-ovi |
| 6 | FAZA 5 — Cutover | 2-4 tjedna | QA period |

**Ukupna estimacija**: 6-8 mjeseci za kompletnu migraciju uz produkcijsku stabilnost.

---

## NIJE U SCOPE-U (Faze 12-17 iz STATUS.md)

Sljedeće faze iz STATUS.md su MOCK implementacije i ne ulaze u ovaj roadmap:
- Faza 12: Channel Manager — zahtijeva poseban contract sa vendor-om
- Faza 13: IoT — zahtijeva hardware
- Faza 14: Payment Gateway — zahtijeva bank agreement
- Faza 15: Guest Portal — zasebni projekt
- Faza 16: AI concierge — after-MVP
- Faza 17: Advanced reporting — after-MVP

**PREPORUKA**: Fokus na Faze 0-5 (sigurnost i core hotel ops) prije bilo čega iz 12-17.
