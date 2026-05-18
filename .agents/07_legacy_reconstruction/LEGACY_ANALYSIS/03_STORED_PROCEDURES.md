# Stored Procedures & Functions - Complete Extraction

> Source: `legacy_code/ModuleKod.vb` (bazaProc + bazaSql), `legacy_code/bin/merona.sql`, `legacy_code/bin/frmBaza.vb`, all `*.vb` form files

---

## 1. Stored Procedures Found in SQL Dumps

### Source: `merona.sql` (lines 21772–22697)

The merona.sql dump contains **54 stored procedures** and **7 functions** persisted in the database. These represent the canonical DB-side definitions. The `Database Backup 2019-02-01 08-31-26.sql` and `HOTELVIP 20150602 0904.sql` and `novaBazaJHotel 20150602 0848.sql` dumps contain **no** stored procedures or functions — they only have tables and views.

#### 1.1 INSERT/UPDATE Procedures

| # | SP Name | Parameters | Tables Written | Business Meaning | merona.sql:line | ModuleKod.vb:line |
|---|---------|------------|---------------|-----------------|----------------|-----------------|
| 1 | `addRelGostSoba` | gostID bigint, sobaID int, checkInDate datetime, checkInRadnik int, checkOutDate datetime, checkOutRadnik int, stampanaPrijava bit, odjavljen bit, rezervacija bit, rezervacijaP bit, grupaID int, tarifaID int, PID int, popust int | relgostsoba (INSERT) | Check-in guest to room | 21807 | 566 |
| 2 | `addFolio` | sSID int, svrijemeD datetime, svrijemeO datetime, szakljucen bit | posjetafolio (INSERT) | Create new folio | 21777 | 852 |
| 3 | `addSmjenaStart` | radnikID int, start datetime | smjene (INSERT) | Start new work shift | 21822 | 897 |
| 4 | `addTroskovi` | iddzid VARCHAR(45), GSID bigint, SID int, TID int, vrijeme datetime, kolicina int, iznos DECIMAL(18,2) (merona) / VARCHAR(50) (ModuleKod), radnikID int | troskovi (INSERT) | Add expense to room | 21837 | 903 |
| 5 | `addPlacanjeSlozeno` | rbt int, nacin int, iznos double | placanjeSlozeno (INSERT) | Add complex payment | 21792 | 1192 |
| 6 | `getLogs` | dugme LONGTEXT, vrijeme datetime, opis LONGTEXT, radnik LONGTEXT, radnikid int, opis1 LONGTEXT | logs (INSERT) | Insert log entry | 22002 | 952 |
| 7 | `Unesinocenja` | RID int, DatumPp datetime, Tarifa decimal(18,2) (merona) / double (ModuleKod), SID int, PID int, opis text, Pop int, ssoba text | nocenja (DELETE+INSERT) | Delete+insert night record for month | 22512 | 1081 |
| 8 | `unesiPojedinacne` | noviSID int, ID int, stariSID int | troskovi (UPDATE SID) | Move expense to different room | 22512 | 1086 |
| 9 | `unesirelsobavrstasobatarifa` | ttarifaID int, sSobaVrstaID int | relsobavrstasobatarifa (INSERT if not exists) | Link room type to tariff | 22527 | 1091 |
| 10 | `promijeniDatumVrijeme` | DD datetime, DOD datetime, GID int | relgostsoba (UPDATE checkOutDate) | Change guest checkout date | 22467 | 1066 |
| 11 | `promijeniGosti` | prezime, ime, adresa, datumrodjenja, mjestodrzavaR, pol, drzavljanstvo, dokument, brDokument, telefon, mobitel, email, brojGosta, DID | gosti (UPDATE) | Update guest information | 22467 | 1071 |
| 12 | `PromjenaFolio` | noviSID int, PID int | posjetafolio (UPDATE SID) | Change folio room assignment | 22497 | 1076 |
| 13 | `updateNapomena` | napomena nvarchar(500) | napomena (UPDATE) where ID=1 | Update global note | 22542 | 1096 |
| 14 | `updateSobaClean` | naziv nvarchar(20), clean int | sobe (UPDATE clean) | Update room cleanliness | 22557 | 1101 |
| 15 | `updateSobaOOO` | naziv nvarchar(50), razlog nvarchar(50), ooo int | sobe (UPDATE ooo, razlog) | Set room out-of-order | 22572 | 1106 |

#### 1.2 SELECT Procedures (Reports/Queries)

| # | SP Name | Parameters | Tables Read | Business Meaning | merona.sql:line | ModuleKod.vb:line |
|---|---------|------------|-------------|-----------------|----------------|-----------------|
| 16 | `getBrojGostiju` | (none) | sobe, sobavrsta, relgostsoba | Count guests per room | 21852 | 909 |
| 17 | `getDokumenti` | (none) | gostdokument | List document types | 21867 | 915 |
| 18 | `getGlavniImena` | (none) | relgostsoba, gosti, sobe | Get current guest names with rooms | 21882 | 921 |
| 19 | `getGlavniPodaci` | IDd int | relgostsoba, gosti, sobe | Get guest-room by ID | 21897 | 926 |
| 20 | `getGlavniTrosakIme` | idd int | relgostsoba, sobe, troskovi, troskovivrste | Get expenses for guest-room | 21912 | 931 |
| 21 | `getGosti` | (none) | relgostsoba, gosti, sobe | Get all current guests | 21927 | 936 |
| 22 | `getGostiKucice` | br int | gosti | Get guest by ID | 21942 | 942 |
| 23 | `getGostisvi` | od1t datetime, do1 datetime, tip int | relgostsoba, sobe, sobaTarifa, radnici, gosti, gostDokument | Get guests by date range | 21957 | 947 |
| 24 | `getIzvjestajDnevni` | datumOD datetime, datumDO datetime | placanjeslozeno, printracuni, placanjenacin | Daily payment report by method | 21972 | 873 |
| 25 | `getIzvjestajDnevniPlacanje` | d1atumOD datetime, d1atumDO datetime | printracuni, printracunidetalji | Daily payment report with receipts | 21987 | 861 |
| 26 | `getNeodjavljeneSobe1` | (none) | sobe, relgostsoba | Rooms with guests not checked out | 22017 | 957 |
| 27 | `getNeplacene` | (none) | relgostsoba, neplaceni, gosti, sobe, troskovi, neplaceniplacanja, sobatarifa | Get unpaid balances | 22032 | 963 |
| 28 | `getNeplaceneSUM` | (none) | neplaceni, neplaceniplacanja | Sum of all unpaid | 22047 | 969 |
| 29 | `getOdjavaCombo` | (none) | relgostsoba, gosti, sobe | Checkout dropdown data | 22062 | 974 |
| 30 | `getPlacanjaPID` | PID int | placanje, placanjenacin | Get payments by PID | 22077 | 979 |
| 31 | `getPlacanjeNocenja` | PID int | placanjedetalji | Sum accommodation payments | 22092 | 984 |
| 32 | `getPosjete` | GID int | relgostsoba, sobe, sobatarifa | Get guest visits | 22107 | 989 |
| 33 | `getPrintDetalji` | RBR int | printracunidetalji | Receipt detail lines | 22122 | 994 |
| 34 | `getPrintFooter` | brojRAC int | printracunifooter | Receipt footer | 22137 | 1000 |
| 35 | `getPrintHeader` | (none) | printracuni | Receipt headers | 22152 | 1005 |
| 36 | `getRezervacijePrijava` | Startt datetime | rezervacije, gosti, sobavrsta | Reservations for check-in | 22167 | 1010 |
| 37 | `getRezrervacijePrikazi` | ddatOD datetime, ddatDO datetime | rezervacije, gosti, sobavrsta, rezervacijeGrupe, rezervacijeIzvor, rezervacijeTip | Reservations display (active) | 22182 | 1198 |
| 38 | `getRezrervacijePrikaziOD` | ddatOD datetime, ddatDO datetime | (same as above) | Reservations for specific date | 22197 | 1204 |
| 39 | `getRezrervacijePrikaziPot` | ddatOD datetime, ddatDO datetime | (same as above) | Confirmed reservations | 22212 | 1210 |
| 40 | `getRezrervacijePrikaziSto` | ddatOD datetime, ddatDO datetime | (same as above) | Cancelled reservations | 22227 | 1215 |
| 41 | `getSobaPodaci` | sobaNaziv VARCHAR(50) | sobe, sobavrsta | Room details by name | 22242 | 1015 |
| 42 | `getSobeSadrzaji` | sobaNaziv VARCHAR(50) | sobasadrzaji, relsobavrstasadrzaj, sobe | Room amenities | 22257 | 1020 |
| 43 | `getSobeShema` | datumP datetime, datumK datetime | sobe, sobavrsta | Room schema with status (calls fnSobaStatus) | 22272 | 1025 |
| 44 | `getStatistika` | (none) | (calls fnBrojNocenja, fnBrojZauzetihSoba, fnBrojSoba) | Hotel statistics | 22302 | 1030 |
| 45 | `getTarifaSve` | (none) | sobatarifa, relsobavrstasobatarifa | All tariffs | 22317 | 1035 |
| 46 | `getTelefonski` | datumPt datetime, datumKt datetime, sobaLokal integer | telpozivi | Phone calls by room | 22332 | 1040 |
| 47 | `getTelefonskiSve` | datumPt datetime, datumKt datetime | telpozivi | All phone calls by date | 22347 | 1045 |
| 48 | `getTroskoviSoba` | SobaID int | troskovi | Expenses for room (ID, iznos, zaklj=0) | 22362 | 1051 |
| 49 | `getTroskoveLista` | brojSobe int | troskovi, troskovivrste | Expense details for room | 22347 | 1226 |
| 50 | `gosti1` | id int | relgostsoba, sobe, sobatarifa, radnici | Guest visit history | 22377 | 1056 |
| 51 | `podaciGostiSobe` | brojSobe VARCHAR(50) | gosti, relgostsoba, sobe, sobavrsta, relsobavrstasobatarifa, sobatarifa | Full guest-room data | 22437 | 1061 |
| 52 | `placanje1` | DS datetime, DK datetime | printracuni, printracunidetalji | Payment sums by method/date | 22392 | 891 |
| 53 | `placanje2` | DS datetime, DK datetime | placanjeslozeno, printracuni, placanjenacin | Payment report (slozeno) | 22407 | 879 |
| 54 | `placanje3` | DS datetime, DK datetime | printracuniavans | Advance payment report | 22422 | 885 |
| 55 | `vratiGostSoba` | sid int | relgostsoba, gosti | Guests by room ID | 22587 | 1111 |
| 56 | `vratiMaxDatum` | sidd int | nocenja | Max accommodation date | 22602 | 1116 |
| 57 | `vratiRIDNocenja` | SIDd int | nocenja, relgostsoba | Distinct RIDs for room nights | 22617 | 1121 |
| 58 | `vratiTarifePoVrsta` | svid int | sobatarifa, relsobavrstasobatarifa | Tariffs by room type | 22632 | 1126 |
| 59 | `vratiTrenutnoRezervisane` | Dayy DATE | rezervacije, sobavrsta | Currently reserved room types | 22647 | 1131 |
| 60 | `vratiTrenutnoSlobodne` | (none) | sobe, sobavrsta | Free room types count | 22662 | 1136 |
| 61 | `vratiTrenutnoZauzete` | (none) | sobavrsta, sobe, relgostsoba | Occupied room types count | 22677 | 1141 |
| 62 | `vratiTrosakSoba` | SID int | troskovi | Sum expenses for room | 22692 | 1146 |

**Note:** `getIzvjestajDnevniPlacanjeN` is in ModuleKod.vb (line 867) but **NOT** in merona.sql.

#### Difference: merona.sql vs ModuleKod.vb

| SP Name | In merona.sql | In ModuleKod bazaProc | Notes |
|---------|:------------:|:---------------------:|-------|
| `addRelGostSoba` | YES (fewer params: 14 vs 18) | YES | ModuleKod adds: nap text, usl text, taksa int, stat int |
| `addTroskovi` | YES (iznos VARCHAR(50)) | YES (iznos DECIMAL(18,2)) | Type mismatch — merona has VARCHAR(50) |
| `Unesinocenja` | YES (Tarifa decimal(18,2)) | YES (Tarifa double) | Type difference |
| `getIzvjestajDnevniPlacanjeN` | **NO** | YES | Not in database dump |
| `getTroskoveLista` | YES | YES | — |

---

## 2. Stored Procedures Created Dynamically in VB.NET Code

All SPs listed in Section 1 are also created dynamically in `ModuleKod.vb:bazaProc()` (lines 844–1234). The application calls `bazaProc()` at startup to DROP and recreate every stored procedure and function.

### 2.1 `bazaProc()` — ModuleKod.vb (lines 844–1234)

Creates **54 stored procedures** + **5 (+1 test) functions** via `DROP IF EXISTS + CREATE PROCEDURE/FUNCTION` pattern.

### 2.2 `bazaSql()` — ModuleKod.vb (lines 1235–2161) — SQL Server Legacy ALTER PROCEDURE

These are **ALTER PROCEDURE** statements (SQL Server syntax with `@param` notation). They were used when the system ran on MS SQL Server and are **legacy/migration** code. They modify procedures that already exist in the MySQL version:

| # | SP Name | Line | SQL Server Params | Notes |
|---|---------|------|-------------------|-------|
| 1 | `getNeplaceneSUM` | 1354 | (none) | SQL Server version |
| 2 | `getLogs` | 1453 | @dugme ntext, @vrijeme datetime, @opis text, @radnik text, @radnikid int, @opis1 text | SQL Server types differ |
| 3 | `Unesinocenja` | 1469 | @RID int, @DatumP datetime, @Tarifa decimal(18,2), @SID int, @PID int, @opis text, @Pop int | No `soba` param vs MySQL |
| 4 | `getSobeShema` | 1520 | @datumP datetime, @datumK datetime | Uses `getdate()` instead of `DATE_FORMAT(NOW(),...)` |
| 5 | `getPrintFooter` | 1778 | @brojRAC int | Adds `nocenja` column |
| 6 | `getPrintHeader` | 1804, 2051 | (none) | Multiple ALTER versions, adds `rad,printime` |
| 7 | `getPlacanjenocenja` | 1815 | @PID int | Note lowercase 'n' in name vs bazaProc `getPlacanjeNocenja` |
| 8 | `getPrintHeaderAvans` | 1856 | (none) | **Not in bazaProc** — advance receipt header |
| 9 | `getGlavniPodaci` | 1867 | @ID integer | Uses `+` concat instead of MySQL `concat()` |
| 10 | `getPrintDetalji` | 1900 | @BrojRac int | — |
| 11 | `placanje3` | 2121 | @DS datetime, @DK datetime | Uses `tipplacanja` column (different from bazaProc) |
| 12 | `vratiAvansDnevniPlacanje` | 2132 | @datumOD DateTime, @datumDO DateTime | **Not in bazaProc** — advance daily payment report |

### 2.3 `frmBaza.vb` (line 853) — `resetbr`

```sql
DROP PROCEDURE IF EXISTS `resetbr`;
CREATE PROCEDURE `resetbr` ()
BEGIN
  truncate table placanje;
  truncate table placanjedetalji;
  truncate table placanjeslozeno;
  truncate table predracuni;
  truncate table predracunidet;
  truncate table printracspec;
  truncate table printracuni;
  truncate table printracuniavans;
  truncate table printracunidetalji;
  truncate table printracunifooter;
END
```
**Business meaning:** Reset all billing/payment tables (year-end data clear). **Not in `bazaProc()`** — created separately.

### 2.4 `getgostisvi` — ModuleKod.vb line 1878 (SQL Server Table-Valued Function)

```sql
CREATE FUNCTION getgostisvi (@od1 datetime, @do1 datetime)
returns table as return
SELECT relgostsoba.checkInDate, relgostsoba.checkOutDate, gosti.ime, gosti.prezime,
       sobe.naziv AS Soba, radnici.ime AS checkInRadn, radnici_1.ime AS checkOutRadn,
       sobatarifa.naziv AS Tarifa, relgostsoba.popust, relgostsoba.PopustRazlog,
       relgostsoba.ID, gosti.adresa, gosti.datumRodjenja, gosti.telefon, gosti.mobitel,
       gosti.email, gosti.brDokument, gostdokument.naziv, gosti.drzavljanstvo,
       gosti.pol, relgostsoba.gostID
FROM relgostsoba INNER JOIN sobe ON relgostsoba.sobaID = sobe.ID
     INNER JOIN sobatarifa ON relgostsoba.tarifaID = sobatarifa.ID
     INNER JOIN radnici ON relgostsoba.checkInRadnik = radnici.ID
     INNER JOIN radnici AS radnici_1 ON relgostsoba.checkOutRadnik = radnici_1.ID
     INNER JOIN gosti ON relgostsoba.gostID = gosti.ID
     INNER JOIN gostdokument ON gosti.dokument = gostdokument.ID
WHERE (relgostsoba.checkInDate > @od1) AND (relgostsoba.checkInDate < @do1)
```
**Note:** This is **SQL Server** table-valued function syntax (`returns table as return SELECT...`). Not compatible with MySQL. Not called from any VB form. **Legacy only.**

### 2.5 `frmGlavni.vb` — sp_rename operations (lines 1398–1503)

These use the `sp_rename` system stored procedure to temporarily rename billing tables for data migration. Not application SPs — they are **system SP calls**:

| Line | Action | Purpose |
|------|--------|---------|
| 1398 | `sp_rename 'placanje', 'placanje_'` | Temp rename payment table |
| 1400 | `sp_rename 'placanjeDetalji', 'placanjeDetalji_'` | — |
| 1402 | `sp_rename 'predracuni', 'predracuni_'` | — |
| 1404 | `sp_rename 'predracunidet', 'predracunidet_'` | — |
| 1406 | `sp_rename 'placanjeSlozeno', 'placanjeSlozeno_'` | — |
| 1408 | `sp_rename 'printracspec', 'printracspec_'` | — |
| 1410 | `sp_rename 'printracuni', 'printracuni_'` | — |
| 1412 | `sp_rename 'printracuniavans', 'printracuniavans_'` | — |
| 1414 | `sp_rename 'printracunidetalji', 'printracunidetalji_'` | — |
| 1416 | `sp_rename 'printracunidetaljiavans', 'printracunidetaljiavans_'` | — |
| 1418 | `sp_rename 'printracunifooter', 'printracuniFooter_'` | — |
| 1420 | `sp_rename 'neplaceni', 'neplaceni_'` | — |
| 1422 | `sp_rename 'neplaceniplacanja', 'neplaceniplacanja_'` | — |
| 1424 | `sp_rename 'Avans', 'Avans_'` | — |
| 1477–1503 | Reverse all renames (remove `_` suffix) | Restore original names |

---

## 3. Functions Found in SQL Dumps

### Source: merona.sql (lines 21672–21770)

| # | Function Name | Parameters | Return Type | Body Summary | Business Meaning | merona.sql:line | ModuleKod.vb:line |
|---|---------------|------------|-------------|-------------|-----------------|----------------|-----------------|
| 1 | `fnBrojGostiju` | soID int | int(11) | `SELECT COUNT(*) FROM relgostsoba WHERE sobaID=soID AND odjavljen=0 AND rezervacija=0` | Count guests in room | 21672 | 1151 |
| 2 | `fnBrojNocenja` | (none) | int(11) | `SELECT COUNT(sobaID) FROM relgostsoba WHERE odjavljen=0 AND rezervacija=0` | Count occupied rooms | 21687 | 1156 |
| 3 | `fnBrojSoba` | (none) | int(11) | `SELECT COUNT(ID) FROM sobe` | Count total rooms | 21702 | 1161 |
| 4 | `fnBrojZauzetihSoba` | (none) | int(11) | `SELECT COUNT(DISTINCT sobaID) FROM relgostsoba WHERE odjavljen=0 AND rezervacija=0` | Count distinct occupied rooms | 21717 | 1166 |
| 5 | `fnSobaStatus` | SoID int, datumP datetime, datumK datetime, tod datetime | int(11) | Complex status logic: 0=free, 1=occupied, 2=checkout today, 3=reserved confirmed, 4=reserved+occupied, 5=OOO, 6=reserved unconfirmed | Room status determination | 21732 | 1177 |
| 6 | `fntest` | soID int | int(11) | `SET brojGostiju=0; IF brojgostiju=0 THEN SET brojgostiju=2; return brojgostiju` | Test function (returns 2) | 21747 | 1182 |
| 7 | `unesisobatarifa` | nazivv nvarchar(50), naziv2 nvarchar(30), usl nvarchar(255) | int(11) | Check if tariff exists by name; if not, INSERT and return @@Identity | Insert tariff if not exists | 21762 | 1187 |

---

## 4. Functions Created Dynamically in VB.NET Code

All 7 functions from Section 3 are also created dynamically in `ModuleKod.vb:bazaProc()` (lines 1151–1190).

Additionally, `bazaSql()` contains one SQL Server legacy function:

| # | Function Name | Line | Syntax | Notes |
|---|---------------|------|--------|-------|
| 1 | `getgostisvi` | 1878 | SQL Server `returns table as return SELECT...` | TVF, not MySQL compatible, not called from any form |

---

## 5. Cross-Reference: SP/Functions Called by Forms/Modules

| SP/Function Name | Called From | Evidence |
|------------------|------------|----------|
| `addFolio` | frmPrijava1 | `frmPrijava1.vb:723` |
| `addPlacanjeSlozeno` | frmPlacanje | `frmPlacanje.vb:4996` |
| `addRelGostSoba` | frmPrijava1 | `frmPrijava1.vb:635` |
| `addSmjenaStart` | funkcije.vb | `funkcije.vb:421` |
| `addTroskovi` | frmTroskovi, frmTroskoviNoc | `frmTroskovi.vb:172`, `frmTroskoviNoc.vb:52` |
| `fnBrojGostiju` | frmIzvjestaji | `frmIzvjestaji.vb:140` |
| `fnSobaStatus` | (called via `getSobeShema`) | ModuleKod.vb:1025 |
| `fnBrojNocenja` | (called via `getStatistika`) | ModuleKod.vb:1030 |
| `fnBrojSoba` | (called via `getStatistika`) | ModuleKod.vb:1030 |
| `fnBrojZauzetihSoba` | (called via `getStatistika`) | ModuleKod.vb:1030 |
| `getBrojGostiju` | frmIzvjestaji | `frmIzvjestaji.vb:140` |
| `getDokumenti` | frmPrijavaGostiUnos, frmPrijavaGostiKucice | `frmPrijavaGostiUnos.vb:705`, `frmPrijavaGostiKucice.vb:548` |
| `getGlavniImena` | funkcije.vb | `funkcije.vb:560` |
| `getGlavniPodaci` | funkcije.vb | `funkcije.vb:581` |
| `getGlavniTrosakIme` | frmPlacanje | `frmPlacanje.vb:210` |
| `getGosti` | frmTroskovi, frmTroskoviNoc | `frmTroskovi.vb:28`, `frmTroskoviNoc.vb:94` |
| `getGostiKucice` | frmPrijavaGostiKucice | `frmPrijavaGostiKucice.vb:640` |
| `getGostisvi` | frmIzvjestajiDnevni | `frmIzvjestajiDnevni.vb:215` (as `getGostiTurizam1`) |
| `getIzvjestajDnevni` | frmIzvjestajiDnevni | `frmIzvjestajiDnevni.vb:32` |
| `getIzvjestajDnevniPlacanje` | frmIzvjestajiDnevni, frmZurnal1 | `frmIzvjestajiDnevni.vb:107`, `frmZurnal1.vb:541,623` |
| `getLogs` | funkcije.vb | `funkcije.vb:467` |
| `getNeplacene` | frmRacuni | `frmRacuni.vb:1007` |
| `getNeplaceneSUM` | frmRacuni | `frmRacuni.vb:1053` |
| `getOdjavaCombo` | frmOdjava1 | `frmOdjava1.vb:34` |
| `getPlacanjaPID` | frmOdjava1 | `frmOdjava1.vb:489` |
| `getPlacanjeNocenja` | frmOdjava1, Data.vb | `frmOdjava1.vb:203`, `Data.vb:443` |
| `getPosjete` | frmPrijavaGostiUnos | `frmPrijavaGostiUnos.vb:1215` |
| `getPrintDetalji` | frmRacuni | `frmRacuni.vb:109` |
| `getPrintFooter` | frmRacuni | `frmRacuni.vb:807` |
| `getPrintHeader` | frmRacuni | `frmRacuni.vb:20` |
| `getRezrervacijePrikazi` | frmRezervacije, frmZurnal1, frmRezervacijePregled | `frmRezervacije.vb:74`, `frmZurnal1.vb:50`, `frmRezervacijePregled.vb:33,710` |
| `getRezrervacijePrikaziPot` | frmRezervacijePregled | `frmRezervacijePregled.vb:35,712` |
| `getRezrervacijePrikaziSto` | frmRezervacijePregled | `frmRezervacijePregled.vb:37,715` |
| `getRezrervacijePrikaziOD` | frmRezervacijePregled | `frmRezervacijePregled.vb:40,721` |
| `getSobaPodaci` | frmSobaInfo | `frmSobaInfo.vb:38` |
| `getSobeSadrzaji` | frmSobaInfo | `frmSobaInfo.vb:496` |
| `getSobeShema` | frmSobe, frmIzvjestaji | `frmSobe.vb:65`, `frmIzvjestaji.vb:192,347` |
| `getStatistika` | frmIzvjestaji | `frmIzvjestaji.vb:254` |
| `getTarifaSve` | frmSobaInfo | `frmSobaInfo.vb:730` |
| `getTelefonski` | frmTelefon | `frmTelefon.vb:123` |
| `getTelefonskiSve` | frmTelefon | `frmTelefon.vb:118` |
| `getTroskoviSoba` | frmOdjava1 | `frmOdjava1.vb:932` |
| `getTroskoveLista` | frmSobaInfo, frmPlacanje | `frmSobaInfo.vb:1162`, `frmPlacanje.vb:5980` |
| `gosti1` | frmGosti | `frmGosti.vb:1381` |
| `placanje1` | frmIzvjestajiDnevni | `frmIzvjestajiDnevni.vb:400` |
| `placanje2` | frmIzvjestajiDnevni | `frmIzvjestajiDnevni.vb:432` |
| `placanje3` | frmIzvjestajiDnevni | `frmIzvjestajiDnevni.vb:463` |
| `podaciGostiSobe` | funkcije.vb | `funkcije.vb:390` |
| `promijeniDatumVrijeme` | frmSobaInfoPromjena | `frmSobaInfoPromjena.vb:38` |
| `promijeniGosti` | frmPrijavaGostiUnos, frmPrijavaGostiKucice | `frmPrijavaGostiUnos.vb:893`, `frmPrijavaGostiKucice.vb:812` |
| `PromjenaFolio` | frmSobaInfo | `frmSobaInfo.vb:1507` |
| `Unesinocenja` | frmRacuni, frmSobaInfo, frmPrijava1, frmOdjava1, frmRezervacijePrebaci | `frmRacuni.vb:1416`, `frmSobaInfo.vb:803`, `frmPrijava1.vb:819`, `frmOdjava1.vb:686`, `frmRezervacijePrebaci.vb:793` |
| `unesiPojedinacne` | frmSobaInfo | `frmSobaInfo.vb:1243` |
| `unesirelsobavrstasobatarifa` | frmTarife | `frmTarife.vb:36,71,106,141,177,212,247,282,316,350,384,418,452,486,520,554` |
| `unesisobatarifa` (function) | frmTarife | `frmTarife.vb:598` |
| `updateSobaClean` | frmSobaInfo, frmPrijava1 | `frmSobaInfo.vb:231,282`, `frmPrijava1.vb:504` |
| `updateSobaOOO` | frmSobaInfo | `frmSobaInfo.vb:180` |
| `vratiGostSoba` | frmOdjava1 | `frmOdjava1.vb:144` |
| `vratiMaxDatum` | frmPrijava1 | `frmPrijava1.vb:535` |
| `vratiRIDNocenja` | funkcije.vb | `funkcije.vb:638` |
| `vratiTarifePoVrsta` | frmPrijava1, frmRezervacije_unos, frmRezervacijeNove | `frmPrijava1.vb:854`, `frmRezervacije_unos.vb:522`, `frmRezervacijeNove.vb:498` |
| `vratiTrenutnoRezervisane` | frmPrijava1 | `frmPrijava1.vb:941` |
| `vratiTrenutnoSlobodne` | frmPrijava1 | `frmPrijava1.vb:907` |
| `vratiTrenutnoZauzete` | frmPrijava1 | `frmPrijava1.vb:984` |
| `vratiTrosakSoba` | frmOdjava1 | `frmOdjava1.vb:238` |
| `resetbr` | frmBaza | `frmBaza.vb:853` |

---

## 6. Missing/Incomplete Procedures

The following SPs are **referenced in VB.NET code** but **NOT defined in `bazaProc()`** (ModuleKod.vb lines 844–1234) and **NOT found in merona.sql**:

| # | SP Name | Called From | Evidence | Likely Source |
|---|---------|------------|----------|---------------|
| 1 | `odjaviGosta` | frmOdjava1 | `frmOdjava1.vb:759` | **Missing** — not in any creation code |
| 2 | `getGostiTurizam1` | frmIzvjestajiDnevni | `frmIzvjestajiDnevni.vb:215` | **Missing** — not defined anywhere |
| 3 | `getJedinicnaCijena` | funkcije.vb | `funkcije.vb:181,299` | **Missing** — not defined anywhere |
| 4 | `getAvansUplata` | frmPlacanje | `frmPlacanje.vb:4651` | **Missing** — not defined anywhere |
| 5 | `getAvansUplataOdjava` | frmOdjava1 | `frmOdjava1.vb:271` | **Missing** — not defined anywhere |
| 6 | `getneplaceniDetalji` | frmRacuni | `frmRacuni.vb:1084` (note lowercase 'n') | **Missing** — not defined anywhere |
| 7 | `promijeniTrosak` | frmRacuni | `frmRacuni.vb:1231` | **Missing** — not defined anywhere |
| 8 | `promijeniRelGost` | frmRacuni | `frmRacuni.vb:1260` | **Missing** — not defined anywhere |
| 9 | `vratiPrijasnji` | frmOdjava1 | `frmOdjava1.vb:655` | **Missing** — not defined anywhere |
| 10 | `unesinocenjaDatumOdj` | frmOdjava1 | `frmOdjava1.vb:792` | **Missing** — not defined anywhere |
| 11 | `vratinocenjaPrije` | frmIzvjestajiDnevni, frmZurnal | `frmIzvjestajiDnevni.vb:631`, `frmZurnal.vb:111` | **Missing** — not defined anywhere |
| 12 | `vratiTroskoveSve` | frmIzvjestajiDnevni, frmZurnal | `frmIzvjestajiDnevni.vb:665`, `frmZurnal.vb:145` | **Missing** — not defined anywhere |
| 13 | `vratinocenjaDanas` | frmIzvjestajiDnevni, frmZurnal | `frmIzvjestajiDnevni.vb:702`, `frmZurnal.vb:182` | **Missing** — not defined anywhere |
| 14 | `vratiUplaceneDanas` | frmIzvjestajiDnevni, frmZurnal | `frmIzvjestajiDnevni.vb:733`, `frmZurnal.vb:213` | **Missing** — not defined everywhere |
| 15 | `vratiNaplaceno` | frmIzvjestajiDnevni, frmZurnal | `frmIzvjestajiDnevni.vb:764`, `frmZurnal.vb:244` | **Missing** — not defined anywhere |
| 16 | `pPercentil` | frmpostavke | `frmpostavke.vb:469` | **Missing** — not defined anywhere |
| 17 | `getPrijavaBoravka` | rptPrijavaBoravka | `rptPrijavaBoravka.vb:29` | **Missing** — not defined anywhere |
| 18 | `getVratiZauzete` | frmRezervacijePrebaci | `frmRezervacijePrebaci.vb:240` | **Missing** — variant of `vratiTrenutnoZauzete`? |
| 19 | `Cijenanocenja` | frmPlacanje (commented), Data.vb (commented) | `frmPlacanje.vb:1024`, `Data.vb:355` | **Commented out** — not active |
| 20 | `addGosti` | frmPrijavaGostiUnos (commented) | `frmPrijavaGostiUnos.vb:788` | **Commented out** — not active |
| 21 | `getPlacanjaSlozena` | frmPlacanjeSlozeno (commented) | `frmPlacanjeSlozeno.vb:126` | **Commented out** — not active |
| 22 | `vratiTrenutni` | frmSobaInfo (commented) | `frmSobaInfo.vb:838` | **Commented out** — not active |
| 23 | `getPrintHeaderAvans` | bazaSql only (ALTER PROCEDURE) | `ModuleKod.vb:1856` | **SQL Server ALTER only**, no MySQL CREATE |
| 24 | `vratiAvansDnevniPlacanje` | bazaSql only (ALTER PROCEDURE) | `ModuleKod.vb:2132` | **SQL Server ALTER only**, no MySQL CREATE |
| 25 | `getGostiTurizam1` | frmIzvjestajiDnevni | `frmIzvjestajiDnevni.vb:215` | **Missing** — not defined anywhere; likely similar to `getGostisvi` with `pl` filter |

### Key Observations on Missing SPs

1. **15 actively called SPs have no definition** (`odjaviGosta`, `getGostiTurizam1`, `getJedinicnaCijena`, `getAvansUplata`, `getAvansUplataOdjava`, `getneplaceniDetalji`, `promijeniTrosak`, `promijeniRelGost`, `vratiPrijasnji`, `unesinocenjaDatumOdj`, `vratinocenjaPrije`, `vratiTroskoveSve`, `vratinocenjaDanas`, `vratiUplaceneDanas`, `vratiNaplaceno`, `pPercentil`, `getPrijavaBoravka`)
2. These were likely created manually in the database or defined in code that has been lost/removed.
3. `getPrintHeaderAvans` and `vratiAvansDnevniPlacanje` exist as SQL Server ALTER PROCEDURE in `bazaSql()` but have no MySQL CREATE PROCEDURE — they will fail on MySQL.
4. `getVratiZauzete` may be a typo/variant of `vratiTrenutnoZauzete`.
5. `Cijenanocenja`, `addGosti`, `getPlacanjaSlozena`, `vratiTrenutni` are all commented out — likely deprecated.
6. `getIzvjestajDnevniPlacanjeN` exists in `bazaProc()` but is **NOT** in merona.sql — likely added later in development.
7. `sp_rename` calls in `frmGlavni.vb` (lines 1398–1503) are SQL Server system SPs for table renaming during migration — not application SPs.
8. `sp_databases` and `sp_help` in `frmBaza.vb` and `frmpostavke.vb` are SQL Server system SPs accessed via comments.