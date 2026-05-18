# Reports (Izvjestaji) System - Legacy Analysis

> Source files:
> - `legacy_code/frmIzvjestaji.vb` (408 lines) — Tree-view reports with Crystal Reports
> - `legacy_code/frmIzvjestajiDnevni.vb` (820 lines) — Daily financial reports with date-range filters
> - Cross-reference: `LEGACY_ANALYSIS/02_MODULEKOD_FUNCTIONS.md` (stored procedure definitions)

---

## 1. Report Types and Business Purpose

### frmIzvjestaji (Crystal Reports via TreeView)

| # | Report Name | Tree Node Key | Crystal Report Class | Business Purpose | Load Method |
|---|-------------|---------------|---------------------|------------------|-------------|
| 1 | Housekeeper Morning Report 1 | `sobariceJutarnji` (Tag="1") | `sobaricaJutarnji` | Room status overview for housekeeping — shows VAC/OCC/CO-UNK/R/R-OCC/OOO per room | `izvjestajSobaricaJutarnji()` |
| 2 | Housekeeper Morning Report 2 | `sobariceJutarnji1` (Tag="") | `sobaricaJutarnji1` | Alternate room status layout — uses 6 status columns (statusNaziv0..5) per row | `izvjestajSobaricaJutarnji1()` |
| 3 | Restaurant Breakfast | `restoranDorucak` (Tag="2") | `restoranDorucak` | Guest count for breakfast planning | `izvjestajRestoranDorucak()` |
| 4 | Hotel Statistics | `statistika` | `HotelStatistika` | Hotel occupancy/nights/rooms statistics | `izvjestajStatistika()` |

**Tree hierarchy** (defined in `InitializeComponent`):
```
I Z V J E S T A J I
├── HOTEL
│   └── Statistika
├── RECEPCIJA
│   ├── Izvjestaj nocnog recepcionera 1
│   └── Izvjestaj nocnog recepcionera 2
└── RESTORAN
    └── Izvjestaj za dorucak
```
Evidence: `legacy_code/frmIzvjestaji.vb:34-42`

### frmIzvjestajiDnevni (Daily Financial Reports)

| # | Report Name | Trigger | Display Form | Business Purpose |
|---|-------------|---------|-------------|------------------|
| 1 | Daily Payment Report | `ucitajNaplatu()` → `getIzvjestajDnevni` SP | `IzvjestajNaplata.xml` schema | Payment totals by method for date range |
| 2 | Daily Payment Detail | `ucitajDnevni()` → `getIzvjestajDnevniPlacanje`/`getIzvjestajDnevniPlacanjeN` SPs | `DnevniPlacanje.xml` | Individual receipts with expense breakdown, filtered by payment method |
| 3 | Advance Payments (Avans) | `ucitajDnevniAvans()` inline SQL | Merged into `DnevniPlacanje` | Advance receipts in date range (non-storno only) |
| 4 | Payment Summary by Method | `ucitajUkupni()` → merges `placanje1` + `placanje2` + `placanje3` | `DnevniSUB.xml` schema | Total per payment method with grand total |
| 5 | Simple Payments | `ucitajJednostavno()` → `placanje1` SP | Merged into summary | Payment totals by method (simple) |
| 6 | Complex Payments | `ucitajSlozeno()` → `placanje2` SP | Merged into summary | Payment totals by method (complex/grouped) |
| 7 | Advance Payment Summary | `ucitajAvansno()` → `placanje3` SP | Merged into summary | Advance payment totals by method |
| 8 | Tourism/Overnight Statistics | `ucitajnocenjaSTAT()` → `getGostiTurizam1` SP | `NocenjeSum.xml` schema → `frmReportTuristicki` | Guest arrivals/departures with night count calculation |
| 9 | Daily Operating Report | `btnDnevni_Click` → `dnevniIzvjestaj()` | `rptDnevniIzvje` form | Single-day P&L: old/new nights, expenses, collected amounts |
| 10 | Period Operating Report | `btnPeriod_Click` → iterates `dnevniIzvjestaj()` per day | `rptDnevniSve` form | Multi-day P&L summary |

Evidence: `legacy_code/frmIzvjestajiDnevni.vb` (all methods enumerated above)

---

## 2. SQL Inventory

### 2.1 Stored Procedure Calls — frmIzvjestaji.vb

| Line | SP Name | Parameters | Data Table | Report |
|------|---------|-----------|------------|--------|
| 113 | `getBrojGostiju` | (none) | `RestoranDorucak` | Restaurant Breakfast |
| 144-163 | `getSobeShema` | `@datumP`=Today, `@datumK`=Today | `SobaricaShema` / `SobaricaShema1` | Housekeeper 1 & 2 |
| 210 | `getStatistika` | (none) | `izvjestajStatistika` | Hotel Statistics |

### 2.2 Stored Procedure Calls — frmIzvjestajiDnevni.vb

| Line | SP Name | Parameters | Data Table | Report |
|------|---------|-----------|------------|--------|
| ~20 | `getIzvjestajDnevni` | `datumOD`, `datumDO` | `IzvjestajNaplata` | Daily Payment |
| ~100 | `getIzvjestajDnevniPlacanje` | `d1atumOD`, `d1atumDO` | `DnevniPlacanje` | Daily Payment Detail |
| ~100 | `getIzvjestajDnevniPlacanjeN` | `pl`, `d1atumOD`, `d1atumDO` | `DnevniPlacanje` | Daily Payment (filtered by method) |
| ~400 | `placanje1` | `DS`, `DK` | (local DataTable) | Simple Payment |
| ~425 | `placanje2` | `DS`, `DK` | (local DataTable) | Complex Payment |
| ~450 | `placanje3` | `DS`, `DK` | (local DataTable) | Advance Payment |
| ~270 | `getGostiTurizam1` | `@SD`, `@KD` | `NocenjeSTAT` | Tourism Statistics |
| ~560 | `vratinocenjaPrije` | `@da`, `@date` | (scalar) | Daily P&L — previous night rate |
| ~590 | `vratiTroskoveSve` | `@datum` | `dtPomocna` | Daily P&L — all expenses |
| ~625 | `vratinocenjaDanas` | `@datum` | (scalar) | Daily P&L — today's rate |
| ~657 | `vratiUplaceneDanas` | `@datum` | (scalar) | Daily P&L — collected today |
| ~686 | `vratiNaplaceno` | (none) | (scalar) | Daily P&L — total collected |

### 2.3 Inline SQL — frmIzvjestajiDnevni.vb

| Approx Line | Context | SQL Statement | Purpose |
|-------------|---------|---------------|---------|
| ~101 | `ucitajDnevni()` — expense detail | `SELECT trosak, sum(kol) AS kolicina, sum(CONVERT(REPLACE(REPLACE(ukupno, '.', ''), ',', '.'), DECIMAL(10,2))) AS ukupno FROM printracunidetalji JOIN printracuni ON printracuni.brojracuna=printracunidetalji.brojracuna WHERE dat BETWEEN '{dod}' AND '{ddo}' AND storno=0 GROUP BY trosak` | Expense totals by type for date range. **SQL INJECTION**: dates string-concatenated. |
| ~107 | `ucitajDnevni()` — expense detail filtered | Same as above + `AND nacinid={cmbNacin.SelectedValue}` | Same, filtered by payment method. **SQL INJECTION**: both date and method concatenated. |
| ~157 | `ucitajDnevniAvans()` | `SELECT BrojRacuna AS broj, datu AS datum, 'Avans' AS naziv, Ime AS ImePrezime, DrugoIme AS NazivOstalo, ukupno AS iznos, TipPlacanja AS nacin, @datumOD AS datumOD, @datumDO AS datumDO, storno FROM printracuniavans WHERE datu BETWEEN @datumOD AND @datumDO AND storno=0` | Advance receipts. Parameterized — safe. |
| ~472 | `ucitajN()` | `SELECT placanjenacin.ID, placanjenacin.nacin FROM placanjenacin WHERE id<>4 AND id<>5 AND id<>6` | Load payment methods for filter dropdown (excludes IDs 4,5,6). Hardcoded filter. |
| ~507 | `ucitajUkupni()` | `SELECT p.ID, p.nacin FROM placanjenacin p WHERE p.id<>5` | Load payment methods for summary (excludes ID 5). Hardcoded filter. |

### 2.4 Stored Procedures Defined in ModuleKod.vb (Referenced by Reports)

From `02_MODULEKOD_FUNCTIONS.md`, these SPs are created in `bazaProc()` (ModuleKod.vb lines 844-1234):

| SP Name | Line (ModuleKod) | Parameters | Purpose | Used By Report |
|---------|-------------------|-----------|---------|---------------|
| `getIzvjestajDnevniPlacanje` | 861 | `d1atumOD`, `d1atumDO` | Daily payment report | frmIzvjestajiDnevni.ucitajDnevni |
| `getIzvjestajDnevniPlacanjeN` | 867 | `d1atumOD`, `d1atumDO`, `pl` | Daily payment by method | frmIzvjestajiDnevni.ucitajDnevni |
| `getIzvjestajDnevni` | 873 | `datumOD`, `datumDO` | Daily report (payment sums) | frmIzvjestajiDnevni.ucitajNaplatu |
| `placanje2` | 879 | `DS`, `DK` | Complex payment report | frmIzvjestajiDnevni.ucitajSlozeno |
| `placanje3` | 885 | `DS`, `DK` | Advance payment report | frmIzvjestajiDnevni.ucitajAvansno |
| `placanje1` | 891 | `DS`, `DK` | Simple payment sum by method | frmIzvjestajiDnevni.ucitajJednostavno |
| `getBrojGostiju` | 909 | (none) | Count guests per room | frmIzvjestaji.izvjestajRestoranDorucak |
| `getStatistika` | 1030 | (none) | Hotel statistics (nights, occupied, total) | frmIzvjestaji.izvjestajStatistika |
| `getSobeShema` | 1025 | `datumP`, `datumK` | Room status for date range | frmIzvjestaji.izvjestajSobaricaJutarnji(1) |

### 2.5 Stored Procedures NOT Defined in ModuleKod.vb

These are called in frmIzvjestajiDnevni.vb but have no CREATE PROCEDURE in ModuleKod.vb. They must exist in the database already or be defined elsewhere:

| SP Name | Parameters | Called In | Purpose |
|---------|-----------|-----------|---------|
| `getGostiTurizam1` | `@SD`, `@KD` | `ucitajnocenjaSTAT()` | Tourism guest arrival/departure data |
| `vratinocenjaPrije` | `@da`, `@date` | `vratiCijenunocenjaPrije()` | Previous night accommodation revenue |
| `vratiTroskoveSve` | `@datum` | `vratiTroskove()` | All expenses for a date |
| `vratinocenjaDanas` | `@datum` | `vratiCijenunocenjaDanas()` | Today's accommodation revenue |
| `vratiUplaceneDanas` | `@datum` | `vratiUplaceniDnevni()` | Today's total payments collected |
| `vratiNaplaceno` | (none) | `vratiNaplacene()` | Grand total of all collected amounts |

---

## 3. Report Data Sources

### 3.1 Database Tables Used Directly

| Table | Used In | Purpose |
|-------|---------|---------|
| `printracunidetalji` | `ucitajDnevni()` inline SQL | Receipt line items (trosak, kol, ukupno, nacinid) |
| `printracuni` | `ucitajDnevni()` inline SQL JOIN | Receipt headers (brojracuna, dat, storno) |
| `printracuniavans` | `ucitajDnevniAvans()` inline SQL | Advance receipts (BrojRacuna, datu, Ime, DrugoIme, ukupno, TipPlacanja, storno) |
| `placanjenacin` | `ucitajN()`, `ucitajUkupni()` inline SQL | Payment method reference (ID, nacin) |

### 3.2 Data Tables Created In-Memory

| Data Table Name | Form | Source | Columns Created Programmatically |
|----------------|------|--------|--------------------------------|
| `RestoranDorucak` | frmIzvjestaji | SP `getBrojGostiju` | (from SP result) |
| `SobaricaShema` | frmIzvjestaji | SP `getSobeShema` | + `statusNaziv` (VAC/OCC/CO-UNK/R/R-OCC/OOO) |
| `SobaricaShema1` | frmIzvjestaji | SP `getSobeShema` | + `statusNaziv0`..`statusNaziv5` (X per status) |
| `izvjestajStatistika` | frmIzvjestaji | SP `getStatistika` | (from SP result) |
| `IzvjestajNaplata` | frmIzvjestajiDnevni | SP `getIzvjestajDnevni` | (from SP result) |
| `DnevniPlacanje` | frmIzvjestajiDnevni | SP `getIzvjestajDnevniPlacanje(N)` + avans rows | (from SP result + merged avans) |
| `dettrosk` | frmIzvjestajiDnevni | Inline SQL (expense aggregation) | `trosak`, `kolicina`, `ukupno` |
| `NocenjeSTAT` | frmIzvjestajiDnevni | SP `getGostiTurizam1` | (from SP result) |
| `NocenjeSUM` | frmIzvjestajiDnevni | Computed from NocenjeSTAT | `ID`, `naziv`, `DatumDolaska`, `DatumOdlaska`, `BrojDolazaka`, `Brojnocenja`, `ImePrezime`, `DatumStart`, `DatumKraj` |
| `dtNacini` | frmIzvjestajiDnevni | Inline SQL + merged sums | `ID`, `nacin`, `Ukupno` |
| `trenutniDnevni` | frmIzvjestajiDnevni | Multiple SP calls | `ExtraStari`, `ExtraDanasnji`, `AvansJucer`, `AvansDanasnji`, `AvansUkupno`, `Staranocenja`, `Novanocenja`, `NaplacenoDanas`, `NaplacenoUkupno`, `Datum` |

### 3.3 XML Schema Files Written

| File | Form | Purpose |
|------|------|---------|
| `restoranDorucak.xml` | frmIzvjestaji | Breakfast report schema |
| `sobaricaShema.xml` | frmIzvjestaji | Housekeeper report 1 schema |
| `sobaricaShema1.xml` | frmIzvjestaji | Housekeeper report 2 schema |
| `izvjestajStatistika.xml` | frmIzvjestaji | Statistics report schema |
| `IzvjestajNaplata.xml` | frmIzvjestajiDnevni | Daily payment schema |
| `DnevniPlacanje.xml` | frmIzvjestajiDnevni | Daily payment detail data dump |
| `NocenjeSum.xml` | frmIzvjestajiDnevni | Tourism overnight schema |
| `DnevniSUB.xml` | frmIzvjestajiDnevni | Payment method summary schema |
| `trenutniDnevni.xml` | frmIzvjestajiDnevni | Daily P&L schema |
| `dettrosk.xml` | frmIzvjestajiDnevni | (commented out) Expense detail |

---

## 4. Business Rules for Reports

### 4.1 Date Range Rules (frmIzvjestajiDnevni)

| Context | Rule | Evidence |
|---------|------|----------|
| Daily payment report | Date range from `DateTimePicker1` + `txtVrijemeOD` (start) to `DateTimePicker2` + `txtVrijemeDO` (end) | `ucitajNaplatu()`, `ucitajDnevni()` |
| Default date range | `DateTimePicker1` = Today, `DateTimePicker2` = Tomorrow + current time | `frmIzvjestajiDnevni_Load` |
| Tourism report | Date range from `DateTimePicker1.Value.ToShortDateString` to `DateTimePicker2.Value.ToShortDateString` (date only, no time) | `ucitajnocenjaSTAT()` |
| Period report | Iterates day-by-day from start to end date | `btnPeriod_Click` |

### 4.2 Time-of-Day Rule (Daily P&L)

| Condition | Behavior | Evidence |
|-----------|----------|----------|
| `Now.Hour >= 12` | Uses current day (`danasnji`) for accommodation calculation, current day for daily collection | `dnevniIzvjestaj()` lines ~530-540 |
| `Now.Hour < 12` | Uses previous day (`jucerasnji`) for accommodation calculation, previous day for collection | `dnevniIzvjestaj()` lines ~530-540 |

**Business meaning**: After noon, the day's rates are considered active. Before noon, the previous day's rates apply.

### 4.3 Payment Method Filtering

| Rule | Evidence |
|------|----------|
| Payment method dropdown excludes IDs 4, 5, 6 | `ucitajN()` SQL: `WHERE id<>4 AND id<>5 AND id<>6` |
| Summary report excludes ID 5 only | `ucitajUkupni()` SQL: `WHERE p.id<>5` |
| Value `0` = "-- sve --" (all methods) | `ucitajN()` appends row with ID=0 |
| When method > 0: uses `getIzvjestajDnevniPlacanjeN` (filtered) | `ucitajDnevni()` |
| When method = 0: uses `getIzvjestajDnevniPlacanje` (all) | `ucitajDnevni()` |

### 4.4 Advance Payment (Avans) Handling

| Rule | Evidence |
|------|----------|
| Storno advances: negate amount (prefix "-") | `ucitajDnevni()`: if `storno<>0` and no "-" in field, prepend "-" to iznos |
| Only non-storno avans rows included | `ucitajDnevniAvans()`: `WHERE storno=0` |
| Avans rows merged into DnevniPlacanje after SP data | `ucitajDnevni()` loop: `ImportRow` for each dtA row |

### 4.5 Empty Result Handling

| Rule | Evidence |
|------|----------|
| If no receipts for date range, insert placeholder row: `(0, Now, "", "Nema racuna za odabrani period", "", 0, "-", dod, ddo)` | `ucitajDnevni()` ~line 95 |

### 4.6 Room Status Mapping (frmIzvjestaji)

| fnSobaStatus Code | Label in Report 1 | Label in Report 2 | Business Meaning |
|-------------------|--------------------|--------------------|-----------------|
| 0 | VAC | statusNaziv0="X" | Vacant |
| 1 | OCC | statusNaziv1="X" | Occupied |
| 2 | CO/UNK | statusNaziv2="X" | Checked out / unkempt |
| 3 | R | statusNaziv3="X" | Reserved (confirmed) |
| 4 | R/OCC | statusNaziv4="X" | Reserved & occupied |
| 5 | OOO | statusNaziv5="X" | Out of order |

Evidence: `izvjestajSobaricaJutarnji()` lines ~170-183, `izvjestajSobaricaJutarnji1()` lines ~280-305

**Note**: Report 1 uses descriptive text labels (VAC, OCC, etc.). Report 2 uses a matrix layout with "X" marks in status columns.

### 4.7 Tourism Night Count Calculation (ucitajnocenjaSTAT)

The method computes nights (Brojnocenja) using date-overlap logic between guest stay dates and the report date range:

| Condition | Calculation | Evidence |
|-----------|------------|----------|
| Arrival within range, departure after range end | `DateDiff(DD, KD)` | Lines ~340 |
| Both dates within range | `DateDiff(SD, KD)` | Lines ~342 |
| Start within range, departure before end | `DateDiff(SD, DOd)` | Lines ~344 |
| Range fully contains stay | `DateDiff(DD, DOd)` | Lines ~346 |

Where DD = arrival date, DOd = departure date, SD = range start, KD = range end.

### 4.8 Expense Aggregation SQL (inline)

The `ucitajDnevni()` method runs inline SQL that converts `ukupno` from a string (with dots as thousands separators and commas as decimal separators) to a decimal:
```sql
sum(CONVERT(REPLACE(REPLACE(ukupno, '.', ''), ',', '.'), DECIMAL(10,2)))
```
This indicates that `printracunidetalji.ukupno` is stored as a formatted string, not a numeric type. **Evidence**: `legacy_code/frmIzvjestajiDnevni.vb` ~lines 101-107.

### 4.9 Payment Summary Calculation (ucitajUkupni)

| Step | Description |
|------|-------------|
| 1 | Load payment methods (exclude ID 5) |
| 2 | Call `placanje1` (simple), `placanje2` (complex), `placanje3` (advance) — each returns rows per method |
| 3 | For each method, sum amounts from all three result sets where method IDs match |
| 4 | Add totals row with ID=7, label "UKUPNO:" |

---

## 5. Cross-Reference

### 5.1 Forms Triggering Reports

| Form | Method | SP/SQL | Report Display | Trigger |
|------|--------|--------|---------------|---------|
| frmIzvjestaji | `izvjestajSobaricaJutarnji()` | `getSobeShema(Today, Today)` | Crystal `sobaricaJutarnji` | Form Load + TreeView select |
| frmIzvjestaji | `izvjestajSobaricaJutarnji1()` | `getSobeShema(Today, Today)` | Crystal `sobaricaJutarnji1` | Form Load + TreeView select |
| frmIzvjestaji | `izvjestajRestoranDorucak()` | `getBrojGostiju` | Crystal `restoranDorucak` | Form Load + TreeView select |
| frmIzvjestaji | `izvjestajStatistika()` | `getStatistika` | Crystal `HotelStatistika` | Form Load + TreeView select |
| frmIzvjestajiDnevni | `ucitajNaplatu()` | `getIzvjestajDnevni(datumOD, datumDO)` | Data grid + XML | Form Load |
| frmIzvjestajiDnevni | `ucitajDnevni()` | `getIzvjestajDnevniPlacanje` or `getIzvjestajDnevniPlacanjeN` + inline SQL | `DnevniIzvjestajrpt` form | Button2_Click |
| frmIzvjestajiDnevni | `ucitajUkupni()` | `placanje1` + `placanje2` + `placanje3` + inline SQL | `DataGridView1` | Date/time change events |
| frmIzvjestajiDnevni | `btnDnevni_Click` | `vratinocenjaPrije`, `vratiTroskoveSve`, `vratinocenjaDanas`, `vratiUplaceneDanas`, `vratiNaplaceno` | `rptDnevniIzvje` | Button click |
| frmIzvjestajiDnevni | `btnPeriod_Click` | Same 5 SPs, iterated per day | `rptDnevniSve` | Button click |
| frmIzvjestajiDnevni | `ucitajnocenjaSTAT()` | `getGostiTurizam1(SD, KD)` | `frmReportTuristicki` | Button3_Click |

### 5.2 TreeView Node → Report Mapping (frmIzvjestaji)

| Node Name | Tag | Report Class | Data Table |
|-----------|-----|-------------|------------|
| `sobariceJutarnji` | "1" | `sobaricaJutarnji` | `SobaricaShema` |
| `sobariceJutarnji1` | "" | `sobaricaJutarnji1` | `SobaricaShema1` |
| `restoranDorucak` | "2" | `restoranDorucak` | `RestoranDorucak` |
| `statistika` | (default) | `HotelStatistika` | `izvjestajStatistika` |

Evidence: `TreeView1_AfterSelect` handler at `legacy_code/frmIzvjestaji.vb:236-253`

### 5.3 Stored Procedures Shared with ModuleKod.vb

These SPs are both **defined** in ModuleKod.bazaProc() and **called** from report forms:

| SP | ModuleKod Line | Called From |
|----|----------------|-------------|
| `getIzvjestajDnevni` | 873 | frmIzvjestajiDnevni.ucitajNaplatu |
| `getIzvjestajDnevniPlacanje` | 861 | frmIzvjestajiDnevni.ucitajDnevni |
| `getIzvjestajDnevniPlacanjeN` | 867 | frmIzvjestajiDnevni.ucitajDnevni |
| `placanje1` | 891 | frmIzvjestajiDnevni.ucitajJednostavno |
| `placanje2` | 879 | frmIzvjestajiDnevni.ucitajSlozeno |
| `placanje3` | 885 | frmIzvjestajiDnevni.ucitajAvansno |
| `getBrojGostiju` | 909 | frmIzvjestaji.izvjestajRestoranDorucak |
| `getStatistika` | 1030 | frmIzvjestaji.izvjestajStatistika |
| `getSobeShema` | 1025 | frmIzvjestaji.izvjestajSobaricaJutarnji(1) |

### 5.4 Referenced External Forms/Reports

| Form/Report Class | Referenced In | Purpose |
|-------------------|---------------|---------|
| `sobaricaJutarnji` | frmIzvjestaji | Crystal Report class for housekeeper report 1 |
| `sobaricaJutarnji1` | frmIzvjestaji | Crystal Report class for housekeeper report 2 |
| `restoranDorucak` | frmIzvjestaji | Crystal Report class for breakfast report |
| `HotelStatistika` | frmIzvjestaji | Crystal Report class for statistics |
| `DnevniIzvjestajrpt` | frmIzvjestajiDnevni.Button2_Click | Daily report display form |
| `rptDnevniIzvje` | frmIzvjestajiDnevni.btnDnevni_Click | Single-day P&L report form |
| `rptDnevniSve` | frmIzvjestajiDnevni.btnPeriod_Click | Multi-day P&L report form |
| `frmReportTuristicki` | frmIzvjestajiDnevni.Button3_Click | Tourism statistics report form |

---

## 6. Key Findings for Modern System

### 6.1 Critical Issues

| # | Issue | Severity | Evidence |
|---|-------|----------|----------|
| 1 | **SQL Injection** in `ucitajDnevni()`: date strings and `cmbNacin.SelectedValue` are concatenated directly into SQL | HIGH | `legacy_code/frmIzvjestajiDnevni.vb` ~lines 101-107 |
| 2 | **Data type inconsistency**: `printracunidetalji.ukupno` stored as formatted string (needs REPLACE/CONVERT to aggregate) | HIGH | Inline SQL in `ucitajDnevni()`: `REPLACE(REPLACE(ukupno, '.', ''), ',', '.')` |
| 3 | **6 undefined stored procedures**: `getGostiTurizam1`, `vratinocenjaPrije`, `vratiTroskoveSve`, `vratinocenjaDanas`, `vratiUplaceneDanas`, `vratiNaplaceno` have no CREATE PROCEDURE in ModuleKod.vb | MEDIUM | Called in frmIzvjestajiDnevni but not in `bazaProc()` |
| 4 | **Hardcoded payment method exclusions**: IDs 4,5,6 excluded in filter dropdown, ID 5 excluded in summary — database magic numbers | MEDIUM | `ucitajN()`, `ucitajUkupni()` |
| 5 | **XML schema files written to working directory** (no path prefix) — creates files in CWD | LOW | Multiple `.WriteXmlSchema()` calls |
| 6 | **Crystal Reports dependency** — tightly coupled to Crystal Reports viewer for frmIzvjestaji | HIGH | `CrystalDecisions.Windows.Forms.CrystalReportViewer` |

### 6.2 Architectural Concerns

| Concern | Description | Recommendation |
|---------|-------------|---------------|
| No separation of data access | SQL is inline in form code, mixed with UI logic | Extract report data to service/repository layer |
| Two different report rendering models | frmIzvjestaji uses Crystal Reports in-process; frmIzvjestajiDnevni uses separate forms and XML | Unify output format; use PDF/HTML generation |
| Monetary values as strings | `ukupno` field is stored as localized string requiring REPLACE/CONVERT | Migrate to DECIMAL column type |
| Date handling fragility | Multiple date concatenation patterns: `CDate(picker.Value.ToShortDateString & " " & time.Text)` | Use parameterized date/time values |
| No error recovery | On SQL error, forms close/dispose — user loses all state | Implement retry/partial load with error display |
| Inconsistent parameter naming | SP parameters use both `@param` and `param` styles; some use `d1atumOD` (typo workaround) | Normalize parameter naming |
| Time-of-day business rule embedded in UI | `Now.Hour >= 12` determines which date's data to use | Move to business logic layer, make configurable |

### 6.3 Report Catalog for Modern Rebuild

| Modern Report Name | Legacy Source | Data Requirements | Output |
|-------------------|---------------|-------------------|--------|
| Room Status / Housekeeping | `getSobeShema` (SP) | Room statuses for date range, per fnSobaStatus codes 0-5 | Grid/matrix |
| Guest Count / Breakfast List | `getBrojGostiju` (SP) | Guest count per room | List |
| Hotel Statistics Dashboard | `getStatistika` (SP) | Nights, rooms, occupancy stats | Summary |
| Daily Payment Report | `getIzvjestajDnevni`, `getIzvjestajDnevniPlacanje(N)` (SPs) | Payment summaries and details by date range, optionally by method | Table |
| Payment Method Summary | `placanje1`, `placanje2`, `placanine3` (SPs) + inline SQL | Merged totals per payment method | Table |
| Tourism Statistics | `getGostiTurizam1` (SP) + date overlap logic | Guest stays with night count calculation | Table |
| Daily P&L | `vratinocenjaPrije`, `vratiTroskoveSve`, `vratinocenjaDanas`, `vratiUplaceneDanas`, `vratiNaplaceno` (SPs) | Revenue/expenses/collection per day | Table |
| Period P&L | Same 5 SPs iterated | Multi-day P&L | Table |
| Expense Type Breakdown | Inline SQL on `printracunidetalji` JOIN `printracuni` | Grouped by expense type with sums | Table |
| Advance Payments | `printracuniavans` table + `placanje3` (SP) | Avans receipts with storno handling | Table |

### 6.4 Missing Stored Procedures to Create/Document

These 6 SPs are called but not defined in the codebase. Their exact SQL must be recovered from the database:

| SP Name | Parameters | Returns | Called By |
|---------|-----------|---------|-----------|
| `getGostiTurizam1` | `@SD`, `@KD` | Guest stays for tourism report | `ucitajnocenjaSTAT()` |
| `vratinocenjaPrije` | `@da` (Int16), `@date` (DateTime) | Decimal — previous night rate | `vratiCijenunocenjaPrije()` |
| `vratiTroskoveSve` | `@datum` (DateTime) | Single row — all expenses for date | `vratiTroskove()` |
| `vratinocenjaDanas` | `@datum` (DateTime) | Decimal — today's accommodation rate | `vratiCijenunocenjaDanas()` |
| `vratiUplaceneDanas` | `@datum` (DateTime) | Decimal — total collected today | `vratiUplaceniDnevni()` |
| `vratiNaplaceno` | (none) | Decimal — total collected all time | `vratiNaplacene()` |