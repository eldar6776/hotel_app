# ANALYSIS 01 — CORE MODULES

**Datum:** 2026-05-17 (rekreirano)
**Scope:** FAZA 1 — core moduli: Data.vb, ModuleKod.vb, funkcije.vb, ApplicationEvents.vb, Settings.vb, konfiguracija.vb
**Agent:** GitHub Copilot (Claude Sonnet 4.6)

---

## PREGLED

| Fajl | Veličina | Risk | Ključni nalaz |
|---|---|---|---|
| Data.vb | ~225KB | 🔴 P0 | Centralni DB layer, plaintext credentials, SQL injection na više mjesta |
| ModuleKod.vb | ~230KB | 🔴 P0 | 50+ sistema procedura, globalni state, race conditions |
| funkcije.vb | ~15KB | 🟠 P1 | Utility funkcije, dio sa SQL operacijama |
| ApplicationEvents.vb | 0KB | 🟢 P3 | Prazan fajl |
| Settings.vb | ~1KB | 🟢 P3 | Framework-generated settings stub |
| konfiguracija.vb | ~3KB | 🟢 P3 | Zakomentovan kod, nema aktivne logike |

---

## DETALJNA ANALIZA

### 1. Data.vb — Centralni SQL/DB Layer
**Risk: 🔴 P0 — KRITIČNO (~225KB, najveći operativni fajl)**

Sadrži apsolutno sve SQL operacije aplikacije. Nema ORM-a, nema repozitorija — sve ide direktno kroz ADO.NET.

**a) Plaintext DB credentials iz XML config-a:**
```vb
ds1.ReadXml("C:\Program Files\IMEDIA\HotelPro\set.xml")
Dim pass As String = ds1.Tables("constr").Rows(0).Item("pass").ToString.Replace("%&rt!h23", "")
passwo = pass.Substring(0, pass.Length - 3)
```
- Lozinka "šifrovana" sufiksom koji se odbaci: trivijal reverse
- XML fajl čitljiv svim Windows korisnicima na mašini
- **CWE-261 Weak Cryptography / CWE-312 Cleartext Storage**

**b) Globalne varijable sa DB credentials:**
```vb
Public server As String
Public servDB As String
Public UIDd As String
Public passwo As String
Public ConnStr As String
```
- Credentials dostupni svugdje u aplikaciji (Public module-level)
- U memory dump-u vidljive u plaintext-u

**c) SQL Injection u kritičnim procedurama:**
```vb
' nocenjeSo - string concatenation
"WHERE sobaID='" & soba & "'"

' pripremaRcuna
"WHERE ID='" & rid & "'"

' vratiCijenunocenja
"WHERE RID=" & rid & " AND datum='" & dat & "'"
```
- Više od 20 procedura koristi string concatenation umjesto parametara
- Najkritičnije: nocenjeSo, pripremaRcuna, vratiCijenunocenja
- **OWASP A3 — CWE-89**

**d) OdjavaSobe — transakcija bez kompletnog rollback-a:**
```vb
sqlTrans = konekcija.BeginTransaction()
' ... 6+ ExecuteNonQuery poziva ...
' Nema Try/Catch/Rollback u svim granama
```
- Transaction postoji, ali nije konzistentno rollback-ovana na svim error putanjama
- Djelimičan checkout moguć ako 3. od 6 koraka failuje
- **CWE-398 Poor Code Quality / Finansijski integritet**

**e) Globalni DataSet `ds` (god object):**
```vb
Public ds As New DataSet
```
- Jedan DataSet za cijelu aplikaciju
- Sve forme čitaju/pišu u isti objekat
- Nema thread safety, nema validacije schema promijena
- **CWE-362 Race Condition**

**Ključne procedure:**
- `citajpod()` — učitava sve inicijalne podatke (rooms, settings, reports config)
- `OdjavaSobe()` — checkout workflow (kritično)
- `nocenjeSo()` — noćenja zapis
- `pripremaRcuna()` — račun priprema
- `addRelGostSoba()` — check-in
- `Unesinocenja()` — ledger zapis noćenja

---

### 2. ModuleKod.vb — System Procedures Module
**Risk: 🔴 P0 (~230KB)**

Sadrži 50+ sistemskih procedura koje koriste sve forme.

**a) fnSobaStatus — izvedeni status sobe:**
```vb
' MySQL funkcija ekvivalent u VB
' Status se izvodi iz relgostsoba, datuma, rezervacija, OOO
```
Legacy statusi:
| Kod | Značenje |
|---|---|
| 0 | slobodna |
| 1 | zauzeta |
| 2 | zauzeta — odlazak danas |
| 3 | rezervisana potvrđena |
| 4 | rezervisana i zauzeta |
| 5 | van upotrebe (OOO) |
| 6 | rezervisana nepotvrđena |

Novi sistem implementira samo dio ovih statusa. PARTIAL coverage.

**b) Unesinocenja — ledger procedura:**
```vb
' Briše postojeće noćenje za isti RID+datum, pa upisuje novo
DELETE nocenja WHERE RID=@rid AND datum=@dat
INSERT nocenja (RID, SID, PID, tarifa, opis, popust, datum)
```
- Idempotentna operacija (safe to retry)
- Finansijski važno: noćenje je materializovani ledger event, ne izračunata vrijednost
- Novi sistem mora imati ekvivalentnu `NightAuditService.UpsertNight()`

**c) Globalni state machine bez locka:**
```vb
Public sobaStatus As Integer
Public aktivnaRezervacija As Integer
Public trenutniGost As Integer
```
- Shared globals mutiraju iz više formi
- Bez Mutex/Monitor → race condition u multi-window scenariju
- **CWE-362**

**d) PrljavaSoba — dirty room flag:**
```vb
UPDATE sobe SET clean=0 WHERE id=@sid
```
- Checkout automatski postavlja sobu kao prljavu
- Housekeeping mora označiti sobu čistom (`clean=1`) da se može ponovo prodati
- Ovo je jedini automatizirani trigger za housekeeping status

---

### 3. funkcije.vb — Utility Functions
**Risk: 🟠 P1**

Utility funkcije: formatiranje datuma/valute, helper-i za UI.

**a) greska() funkcija — exception logging:**
```vb
Public Sub greska(ByVal poruka As String, ByVal gdje As String, ByVal sql As String)
    IO.File.AppendAllText("log.txt", vbCrLf & Now & ";" & gdje & ";" & sql & ";" & poruka)
```
- SQL query se loguje u plaintext log.txt fajl
- Ako query sadrži podatke o gostima (ime, broj pasoša) → GDPR problem
- `log.txt` u working directory bez rotacije (raste neograničeno)
- **CWE-532 Information Exposure Through Log Files**

**b) Hardcoded putanje u helper funkcijama:**
```vb
Dim imagePath As String = "C:\Program Files\IMEDIA\HotelPRO\images\"
```
- **CWE-426 Untrusted Search Path**

---

### 4-6. ApplicationEvents.vb, Settings.vb, konfiguracija.vb
**Risk: 🟢 P3**

- `ApplicationEvents.vb` — prazan fajl, nikad implementiran
- `Settings.vb` — auto-generated .NET Settings stub
- `konfiguracija.vb` — sve zakomentovano, mrtav kod

---

## SAŽETAK

| Prioritet | Fajl | Problem | OWASP | CWE |
|---|---|---|---|---|
| 🔴 P0 | Data.vb | Plaintext DB credentials iz XML | A2 | CWE-312 |
| 🔴 P0 | Data.vb | SQL Injection u 20+ procedura | A3 | CWE-89 |
| 🔴 P0 | Data.vb | Nepotpun transaction rollback | — | CWE-398 |
| 🔴 P0 | ModuleKod.vb | Globalni shared state bez locka | — | CWE-362 |
| 🟠 P1 | funkcije.vb | SQL u log fajlu (GDPR) | A9 | CWE-532 |

## MIGRACIONI ZAHTJEVI

1. Sve DB credentials iz `IConfiguration` + secrets manager
2. Sve SQL operacije → parameterizovane (Dapper / EF Core)
3. Transakcije → `using var tx = await conn.BeginTransactionAsync()`
4. Globalni DataSet → scoped services sa DI
5. Log → Serilog sa GDPR-safe log filtering (no PII u logovima)
6. `fnSobaStatus` → `RoomOccupancyService.GetDerivedStatus()` sa svim 7 statusa
