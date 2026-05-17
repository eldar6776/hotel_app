# ANALYSIS 04 — REPORTING FORMS

**Datum:** 2026-05-17 (rekreirano)
**Scope:** FAZA 4 — sve reporting forme iz legacy_app/Radna
**Agent:** GitHub Copilot (Claude Sonnet 4.6)

---

## PREGLED

| Fajl | Veličina | Risk | Ključni nalaz |
|---|---|---|---|
| rptRacunFrm.vb | ~22KB | 🔴 P0 | Path traversal u CreateStream, culture pollution |
| rptDnevniIzvje.vb | ~3KB | 🔴 P0 | CR credentials u kodu |
| rptPlacanjeGrupno.vb | ~5KB | 🟢 P3 | Auto-generated wrapper |
| rptPlacanjePojedinacno1.vb | ~1KB | 🟡 P2 | DataSet bez bounds check |
| rptPrijavaBoravka.vb | ~5KB | 🟠 P1 | Parcijalno parameterizovano |
| rptRezervacije.vb | ~6KB | 🟢 P3 | Auto-generated wrapper |
| rptTuristicka.vb | ~5KB | 🟢 P3 | Auto-generated wrapper |
| GostiListing.vb | ~6KB | 🟢 P3 | Auto-generated wrapper |
| HotelStatistika.vb | ~5KB | 🟢 P3 | Auto-generated wrapper |
| rptDnevniIzvjestaj.vb | ~1KB | 🟢 P3 | Auto-generated stub |

---

## DETALJNA ANALIZA

### 1. rptRacunFrm.vb — Invoice Report Form
**Risk: 🔴 P0**

Ovo je centralna forma za štampanje računa. Koristi RDLC/ReportViewer (WinForms), ne Crystal Reports.

**Kritični problemi:**

**a) Path Traversal u CreateStream (stream export):**
```vb
Dim stream As Stream = New FileStream("..\..\" + name + "." + fileNameExtension, FileMode.Create)
```
- `name` i `fileNameExtension` dolaze iz ReportViewer callback-a
- Relativna putanja bez validacije → napadač može pisati na `..\..\..\..\Windows\system32\`
- **OWASP A1 — Broken Access Control / CWE-22 Path Traversal**

**b) Thread-local culture pollution:**
```vb
System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol = ds.Tables("setings").Rows(0).Item("valuta").ToString
```
- Globally mutira CultureInfo na trenutnom threadu
- Nije resetovano u `Finally` bloku → u multi-thread scenariju kontaminira andere threadove
- **CWE-362 Race Condition**

**c) SQL Injection u printpredr funkciji:**
```vb
' Iz session analize - potvrđeno
WHERE p.id='" & br & "'"
```
- `br` je string parametar koji se konkatenira direktno u SQL
- **OWASP A3 — Injection / CWE-89**
- **Ovo je P0 — faktura se može izvući za bilo koji ID ubacivanjem ' OR '1'='1**

**d) Stream leak na exception:**
```vb
Private m_streams As IList(Of Stream)
' Nema Dispose() na exception path
```
- Streams liste se ne čiste na grešci → memory/file descriptor leak

**Migracioni zahtjev:**
- `InvoiceGenerator` u novom sistemu mora koristiti QuestPDF + parametrizovane query-je
- Export putanje moraju biti u temp folderu sa validacijom ekstenzije

---

### 2. rptDnevniIzvje.vb — Daily Report (Crystal Reports)
**Risk: 🔴 P0**

**Kritični problem — hardcoded DB credentials:**
```vb
Public Sub pokreni(ByVal br As Integer)
    Dim crTable As CrystalDecisions.CrystalReports.Engine.Table
    Dim crTableLogonInfo As CrystalDecisions.Shared.TableLogOnInfo
    Dim ConnInfo As New CrystalDecisions.Shared.ConnectionInfo()

    ConnInfo.ServerName = server
    ConnInfo.DatabaseName = servDB
    ConnInfo.UserID = UIDd
    ConnInfo.Password = passwo
```
- `server`, `servDB`, `UIDd`, `passwo` su globalne varijable učitane iz XML config-a
- Crystal Reports zahtijeva DB credentials direktno u kodu za re-auth
- Credentials vidljive u memory dump-u procesa
- **OWASP A2 — Cryptographic Failures / CWE-312 Cleartext Storage**

**Dodatni problem:**
```vb
padiva.Value = br  ' br dolazi iz pozivajuće forme, nema range/type check
```
- Parametar `br` proslijeđen bez validacije tipa/raspona

**Migracioni zahtjev:**
- Novi reporting sistem (QuestPDF/SSRS) mora koristiti connection iz DI containera
- Nikada ne prolaziti raw DB credentials u report layer

---

### 3. rptPlacanjeGrupno.vb — Group Payment Report
**Risk: 🟢 P3**

Auto-generated Crystal Reports wrapper. Samo `SetDataSource` poziv.
Nema vlastite poslovne logike. Rizik dolazi iz parent CR .rpt fajla.

---

### 4. rptPlacanjePojedinacno1.vb — Individual Payment Report
**Risk: 🟡 P2**

```vb
rpt.SetDataSource(ds.Tables("printGore"))
rpt.OpenSubreport("rptPrintReportSub.rpt").SetDataSource(ds.Tables("printSredina"))
```
- `ds.Tables("printGore")` — KeyNotFoundException ako tabela nedostaje (no null check)
- Subreport path je hardcoded string — ne postoji validacija da .rpt fajl postoji

---

### 5. rptPrijavaBoravka.vb — Guest Arrival/Registration Report
**Risk: 🟠 P1**

**Dobra praksa (JEDINI slučaj parameterizacije u reporting layer-u):**
```vb
komanda.Parameters.Add("@datOD", MySqlDbType.DateTime).Value = dtpOD.Value.Date
komanda.Parameters.Add("@datDO", MySqlDbType.DateTime).Value = dtpDO.Value.Date
```
✅ DateTime parametri su ispravno parameterizovani.

**Problem — hardcoded export putanja:**
```vb
dtEvidencija.WriteXmlSchema("printEvidencija.xml")
```
- XML shema se piše u working directory bez provjere dozvola
- Može overwriteati postojeći fajl

**Pословni kontekst:**
- Ovo je forma za prijavu boravka stranca (turistička evidencija)
- Podaci uključuju ime, prezime, broj pasoša, datum ulaska/izlaska, državljanstvo
- GDPR osjetljivi podaci u XML export-u bez enkripcije

---

### 6-10. Ostale reporting forme
**Risk: 🟢 P3**

`rptRezervacije.vb`, `rptTuristicka.vb`, `GostiListing.vb`, `HotelStatistika.vb`, `rptDnevniIzvjestaj.vb` su auto-generated Crystal Reports wrapper-i ili stub-ovi. Nema vlastite poslovne logike. Rizik je indirektan kroz .rpt definicije i CR runtime.

---

## SAŽETAK NALAZA

| Prioritet | Fajl | Problem | OWASP | CWE |
|---|---|---|---|---|
| 🔴 P0 | rptRacunFrm.vb | Path traversal u CreateStream | A1 | CWE-22 |
| 🔴 P0 | rptRacunFrm.vb | SQL Injection u printpredr | A3 | CWE-89 |
| 🔴 P0 | rptDnevniIzvje.vb | DB credentials u Crystal Reports | A2 | CWE-312 |
| 🟠 P1 | rptPrijavaBoravka.vb | GDPR XML export bez enkripcije | A2 | CWE-311 |
| 🟡 P2 | rptPlacanjePojedinacno1.vb | DataSet bez bounds check | A5 | CWE-129 |

## MIGRACIONI ZAHTJEVI

1. Crystal Reports → QuestPDF (nema DB credential passing u report layer)
2. Sve SQL u reporting mora biti parametrizovano i kroz servisni layer
3. Export u temp direktorij sa validacijom putanje i ekstenzije
4. GDPR export mora biti enkriptovan ili zaštićen lozinkom
5. Kreirati `ReportingService` sa DI-injected connection, ne direktan DB pristup iz report forme
