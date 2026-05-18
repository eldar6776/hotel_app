# ModuleKod.vb - Comprehensive Function Analysis

> Source: `legacy_code/ModuleKod.vb` (3145 lines)  
> This is the CORE business logic module. Contains database operations, fiscal device integration, schema migrations, and CRUD for all major entities.

---

## 1. Module-level Variables and Constants

| Name | Type | Value/Default | Line | Business Meaning |
|------|------|---------------|------|-----------------|
| `Beep` | Win32 API declare | kernel32.Beep | 2 | System beep for alerts |
| `lictex` | String | (uninit) | 3 | License text |
| `lic` | Integer | 0 | 4 | License flag/counter |
| `verB` | String | "1.1" | 5 | Database version number |
| `verP` | String | "1.1" | 6 | Program version number |
| `kardTable` | DataTable | - | 7 | Card reader event table |
| `kardst` | String | "Salto nije konektovan" | 8 | Card reader status (Salto not connected) |
| `kontr` | kard_imedia1 | (new) | 9 | Card reader controller instance |
| `kontod` | String | "" | 10 | Card reader connection code |
| `ipadr` | String | My.Settings.ipAdres | 11 | IP address from settings |
| `dozvole` | String | (uninit) | 12 | Permissions string |
| `dsk` | DataSet | (uninit) | 13 | Secondary dataset |
| `fisnsc` | Integer | 0 | 14 | Fiscal NSC counter |
| `pk` | Byte | 0 | 52 | Card check in-progress flag |
| `broji` | Integer | 0 | 53 | Card reader retry counter |
| `dtalarm` | DataTable | (new) | 410 | Alarm data storage |
| `fw` | IO.FileSystemWatcher | WithEvents | 3040 | Fiscal device file watcher |
| `brrac` | Integer | 0 | 3041 | Current bill/receipt number |

### kardTable Columns (lines 18-26)

| Column | Business Meaning |
|--------|-----------------|
| `id` | Row identifier |
| `loc` | Location |
| `ulaz` | Input data |
| `izlaz` | Output data |
| `log` | Log data |
| `odg` | Response |
| `kard` | Card data |
| `sat` | Timestamp |
| `tip` | Event type (1-19, see below) |

### kardTable Tip Values (lines 29-48)

| Tip | Meaning |
|-----|---------|
| 1 | Written card response |
| 2 | Written log error |
| 3 | Written log - power-on count |
| 4 | Written log - VOC reset count |
| 5 | Written log - log entry count |
| 6 | Written log - description deleted or no log |
| 7 | Written log - read log |
| 8 | Written - programmed outputs |
| 9 | Written - programmed inputs |
| 10 | Written - device time |
| 11 | SOS alarm |
| 12 | Fire alarm |
| 13 | Minibar |
| 14 | Invalid card |
| 15 | Unauthorized housekeeping |
| 16 | Device not responding |
| 17 | Power on |
| 18 | Power off |
| 19 | Card read |

---

## 2. Function Inventory

| # | Name | Visibility | Parameters | Return | Lines | Description | Module |
|---|------|-----------|------------|--------|-------|-------------|--------|
| 1 | `New()` | Public | (none) | - | 15-51 | Module constructor. Initializes kardTable with columns and default row | P1-Hardware |
| 2 | `provjeri_kard()` | Public | (none) | - | 55-123 | Check card reader (Salto) connection, read logs, update room status (gost/sos/vatr) | P1-Hardware |
| 3 | `sobe_load()` | Public | (none) | - | 124-130 | Load all rooms with type, controller, and status into ds("sobe") | P0-Rooms |
| 4 | `loadsetg()` | Public | (none) | - | 131-153 | Load zaglavlje (header) and goststatus (guest status) into dataset | P0-Settings |
| 5 | `inportgod(tip As Byte)` | Public | tip: 0=partial, 1=full | - | 154-196 | Import yearly data from godbaza by INSERT INTO...SELECT from another DB | P0-DataMigration |
| 6 | `tabzainportsve()` | Private | (none) | String() | 197-252 | Returns array of 52 table names for full import | P0-DataMigration |
| 7 | `tabzainport()` | Private | (none) | String() | 253-294 | Returns array of 38 table names for partial import | P0-DataMigration |
| 8 | `avans_print(br As Integer, storno As Byte)` | Public | br: receipt#, storno: 0/1 | - | 295-335 | Load and save advance receipt data to XML file | P0-Billing |
| 9 | `alarm(...16 params...)` | Public | opis, opis1, odgovor, vrijeme, vrijeme1, tipalarm1, rpt, week, p,u,sr,cet,pet,subo,ned, radn, sobaid | - | 337-377 | Insert alarm record into DB | P0-Alarms |
| 10 | `alarStorno(radnik As String, id As Integer)` | Public | radnik, id | - | 378-409 | Cancel (storno) an alarm by id | P0-Alarms |
| 11 | `provjerialarm()` | Public | (none) | - | 411-439 | Load active alarms (storno=0, tip=1, time<now+10h) into dtalarm | P0-Alarms |
| 12 | `provrezstare()` | Public | (none) | - | 440-442 | Mark old reservations (checkInDate<today-1) as prijava=1 | P0-Reservations |
| 13 | `citajAlarm()` | Public | (none) | DataTable | 443-488 | Read last 500 alarms with status calculation | P0-Alarms |
| 14 | `baza()` | Public | (none) | - | 489-496 | Initialize database: create tables, views, migrations, update version | P0-Database |
| 15 | `izmjene()` | Public | (none) | - | 497-671 | Database schema migrations: 50+ ALTER TABLE, CREATE TABLE, CREATE PROCEDURE statements | P0-Database |
| 16 | `sobanaziv(sid As Integer)` | Public | sid: room ID | String | 672-679 | Return room name (naziv) by room ID | P0-Rooms |
| 17 | `checkchema(shem As String)` | Public | shem: schema name | DataTable | 680-700 | Check if database schema exists in INFORMATION_SCHEMA | P0-Database |
| 18 | `crecshem(str As String)` | Public | str: schema name | String | 701-721 | Create new database schema (CREATE DATABASE IF NOT EXISTS) | P0-Database |
| 19 | `rete()` | Private | (none) | String | 722-755 | Read and parse SQL file from d:\CreatesT.sql | P0-Database |
| 20 | `createTable()` | Public | (none) | - | 756-779 | Execute CREATE TABLE IF NOT EXISTS statements for core tables (alarm, drzave, fisc, godine, gostdokument, gosti, logcont) | P0-Database |
| 21 | `viewcr()` | Public | (none) | - | 780-843 | Create DB views: brojsoba, bzs, folio, gostipid, pid, radniksmjena, sobev, troskovisuma | P0-Database |
| 22 | `bazaProc()` | Public | (none) | - | 844-1234 | Create 40+ stored procedures and 5 functions (see SQL section) | P0-Database |
| 23 | `bazaSql(iddi As Integer)` | Public | iddi: id (returns early if 0) | - | 1235-2161 | Legacy schema migration (SQL Server style). 60+ ALTER/CREATE/PROC statements | P0-Database |
| 24 | `prikaziAvans(id As Integer)` | Public | id: receipt# | - | 2162-2204 | Load advance receipt detail and header, save to XML | P0-Billing |
| 25 | `artGet(br, tip, table)` | Public | br, tip, table | DataTable | 2205-2228 | Get expense types (troskovivrste) - params unused | P1-Expenses |
| 26 | `artProvjera(br, tip, table)` | Public | br, tip, table | Object | 2229-2253 | Verify expense types - always returns 0, incomplete implementation | P1-Expenses |
| 27 | `predracunGetZadnji()` | Public | (none) | Integer | 2255-2279 | Get next proforma number (last+1) | P0-Proforma |
| 28 | `predracunInsert(dt As DataTable)` | Public | dt: proforma data | - | 2280-2302 | INSERT proforma (predracuni) from DataTable | P0-Proforma |
| 29 | `predracunUpdate(dt As DataTable, br As Integer)` | Public | dt, br: proforma number | - | 2303-2326 | UPDATE proforma by broj | P0-Proforma |
| 30 | `predracunSeL(br As String)` | Public | br: "" for all, or number | DataTable | 2327-2355 | SELECT proformas (all or by broj) | P0-Proforma |
| 31 | `predracunBris(br As String)` | Public | br: proforma number | - | 2356-2382 | DELETE proforma by broj | P0-Proforma |
| 32 | `predracunDetInsert(dt As DataTable)` | Public | dt: detail rows | - | 2384-2412 | INSERT proforma detail lines (predracunidet) | P0-Proforma |
| 33 | `predracunDetUpdate(dt As DataTable, id As Integer)` | Public | dt, id | - | 2413-2437 | UPDATE proforma detail line | P0-Proforma |
| 34 | `predracunDetSeL(ID As String)` | Public | ID: "" for all, or BrojRacuna | DataTable | 2438-2466 | SELECT proforma details | P0-Proforma |
| 35 | `predracunDetBris(id As String)` | Public | id: row id | - | 2467-2490 | Delete proforma detail (note: SQL SELECT instead of DELETE - BUG) | P0-Proforma |
| 36 | `createTablefile()` | Public | (none) | - | 2493-2534 | Read SQL file and execute it (bazakasa.sql) | P1-Database |
| 37 | `mysqlExScalarK(queri As String)` | Public | queri | String | 2536-2554 | Execute scalar on KASA database connection | P1-Database |
| 38 | `mysqlReaderK(queri, tablename)` | Public | queri, tablename | DataTable | 2555-2572 | Execute reader on KASA database connection | P1-Database |
| 39 | `mysqlExScalarLast(queri As String)` | Public | queri | Integer | 2573-2591 | Execute INSERT and return @@Identity (last auto-increment) | P0-Database |
| 40 | `mysqlExScalar(queri As String)` | Public | queri | Boolean | 2592-2609 | Execute scalar, returns True/False. Escapes backslashes | P0-Database |
| 41 | `mysqlReader(queri, tablename)` | Public | queri, tablename | DataTable | 2610-2627 | Execute reader, returns loaded DataTable | P0-Database |
| 42 | `mysqlProcedure(queri, tablename)` | Public | queri, tablename | DataTable | 2628-2644 | Call stored procedure, return DataTable | P0-Database |
| 43 | `krrTE(put As String)` | Public | put: path | Boolean | 2647-2706 | Generate KTE receipt XML file with receipt data | P1-Fiscal |
| 44 | `FMRacunKTE(br As Integer)` | Public | br: receipt# | - | 2707-2874 | Generate full KTE fiscal receipt XML and copy to fiscal device folder | P1-Fiscal |
| 45 | `stornoKTE(tip, radnik, dt, ibobr, putod, putdo)` | Public | tip, radnik, dt, ibobr, putod, putdo | - | 2875-2897 | Generate KTE storno receipt file | P1-Fiscal |
| 46 | `upisi_radnikeTermol(id As Integer, rad As String)` | Public | id: worker code, rad: name | - | 2899-2936 | Write worker data to Termol fiscal device XML | P1-Fiscal |
| 47 | `greska(exce, mjesto)` | Private | exce, mjesto | - | 2937-2940 | Error handler (empty/no-op) | P2-Utility |
| 48 | `sifraGetdet(rac As Integer)` | Public | rac: receipt# | DataTable | 2942-2967 | Get items from sifarnik for a receipt number and year | P1-Billing |
| 49 | `FMRacunE(br As Integer)` | Public | br: receipt# | - | 2969-3039 | Generate ELN fiscal receipt file (ra.in) and copy to fiscal folder | P1-Fiscal |
| 50 | `fww(sender, e)` | Private | FileSystemEventArgs | - | 3042-3082 | File watcher event handler - reads fiscal device response file, extracts BF: number | P1-Fiscal |
| 51 | `stornoE(tip, radnik, dt, ibobr, putod, putdo)` | Public | tip, radnik, dt, ibobr, putod, putdo | - | 3083-3105 | Generate ELN storno receipt file | P1-Fiscal |
| 52 | `snimFiskal(br, vr, izn, id)` | Public | br: fiscal#, vr: time, izn: amount, id: receipt# | - | 3107-3143 | Save fiscal response data to printracuni and printracunifooter tables | P0-Fiscal |

---

## 3. SQL Operations

### 3.1 SELECT Operations

| Line | Operation | Tables | Columns | WHERE/Condition | Function | Business Purpose |
|------|-----------|--------|---------|-----------------|----------|-----------------|
| 128 | SELECT | sobe LEFT JOIN sobavrsta LEFT JOIN kontroler | id, naziv, vrstanaziv, vrsta, lokal, ooo, razlog, zgradaID, clean, tekst, idvrsta1, sprat, idkon, redulko, ipadres, port, sos, vatr, gost | ORDER BY sobe.id | sobe_load | Load all rooms with controller info |
| 134 | SELECT | zaglavlje | id, red, red1, red2, red3, idbr, pdv, nazivfirm, adresa, prijbor | WHERE id=1 | loadsetg | Load company header data |
| 144 | SELECT | goststatus | id, naziv, del, taksa | WHERE del=0 | loadsetg | Load active guest statuses |
| 305 | SELECT | printracuniavans | BrojRacuna, Poslovna, Ime, DrugoIme, PeriodOd, PeriodDo, TipPlacanja, BrojSobe, storno, folio, idgost, predracun, posjeta, racin, napo, datr, peri, usluga, kol, iznos, porezSt, porezIz, ukupno, artbr, datum, imeid, firmaid, textpr, textiz, napom, p1, p2, p3, p4, p5 | WHERE BrojRacuna={br} | avans_print | Load advance receipt data |
| 418 | SELECT | alarm | id, opis, opis1, odgovor, vrijeme, tip, chk, rpt, week, pon, uto, sri, cet, pet, sub, ned, radnik, soba, storno, vr_upis, vr_potvrde, radnStorn, radnCHK, stornovr, radnCHKvr, idd | WHERE storno=0 AND tip=1 AND vrijeme<{now+10h} | provjerialarm | Load active alarms |
| 466 | SELECT | alarm | id, opis, vrijeme, IF(tip=1, IF(storno=0,'Aktivno','Stornirano'),'Zavrseno') as status, soba, radnik, vr_upis, vr_potvrde, radnCHK as potvrdio, stornovr as storno, radnStorn as radnikStorno | ORDER BY id DESC LIMIT 500 | citajAlarm | Read recent alarms with status |
| 516 | SELECT | rezervacijeizvor | * | (no WHERE - check if empty) | izmjene | Seed source types |
| 521 | SELECT | rezervacijetip | * | (no WHERE - check if empty) | izmjene | Seed reservation types |
| 531 | SELECT | rezervacijegrupe | * | (no WHERE - check if empty) | izmjene | Seed reservation groups |
| 584 | SELECT | printracuni | SUM(INSTR(datr,'.')) | (no WHERE - check date format) | izmjene | Check if datr field has dots (date format) |
| 595 | SELECT | printracuni | brojracuna, CAST(datr AS char) as datr | (no WHERE) | izmjene | Get all receipt records for date repair |
| 610 | SELECT | printracuni | brojracuna, INSTR(datr,'0000') as dr, CAST(datr AS char) as datr | (no WHERE) | izmjene | Find receipts with invalid dates |
| 674 | SELECT | sobe | ID, naziv | WHERE id={sid} | sobanaziv | Get room name |
| 687 | SELECT | INFORMATION_SCHEMA.SCHEMATA | SCHEMA_NAME | WHERE SCHEMA_NAME LIKE '%{shem}%' | checkchema | Check if database exists |
| 2167 | SELECT | printracunidetaljiavans | BrojRacuna, Trosak, Kol, CijBezPdv, UkupnoBezPdv, Pdv, IznosPdv, Ukupno, Nacin, Valuta, OznakaValute, popust, razlogp, pop, trosakId | WHERE BrojRacuna={id} | prikaziAvans | Load advance receipt details |
| 2179 | SELECT | printracuniavans | BrojRacuna, Poslovna, Ime, DrugoIme, PeriodOd, PeriodDo, TipPlacanja, BrojSobe, storno, folio, idgost, predracun, posjeta, racin, napo, datr, peri | WHERE BrojRacuna={id} | prikaziAvans | Load advance receipt header |
| 2211 | SELECT | troskovivrste | ID, naziv | (no WHERE) | artGet | Get all expense types |
| 2236 | SELECT | troskovivrste | ID, naziv | (no WHERE) | artProvjera | Get all expense types (verify) |
| 2261 | SELECT | predracuni | broj | ORDER BY broj DESC LIMIT 1 | predracunGetZadnji | Get highest proforma number |
| 2334 | SELECT | predracuni | broj, brojpred, ime, frima, frimaid, dadtum, datumval, aktiv, ukupno, kontakt, napomnena, veza, vezaid, rabat, vr_upis, d1, d2, d3, sifra1, vrplac, nazivp, nazivid, gostid | (all or by broj) | predracunSeL | List proformas |
| 2445 | SELECT | predracunidet | convert(id,char) as id, BrojRacuna, Trosak, Kol, CijBezPdv, UkupnoBezPdv, Pdv, IznosPdv, Ukupno, Nacin, Valuta, OznakaValute, Popust, popust1, razlogp, pop, trosakId, predid, t1, trosakid1 | (all or by BrojRacuna) | predracunDetSeL | List proforma detail lines |
| 2473 | SELECT | predracunidet | predracunidet | WHERE Id={id} | predracunDetBris | **BUG: SELECT instead of DELETE** |
| 2714 | SELECT | printracunidetalji | BrojRacuna, Trosak, Kol, Pdv, Ukupno, Nacin | WHERE BrojRacuna={br} | FMRacunKTE | Load receipt items for KTE fiscal |
| 2946 | SELECT | sifarnik | id, naziv, kol, cij, ukupno, sifra, porez, racu, racun, dod, dod1, dod2, dod3, placanje | WHERE sifarnik.racu={rac} AND sifarnik.racun LIKE '%godina%' | sifraGetdet | Get items by receipt and year |
| 2989 | SELECT | printracunidetalji | id, BrojRacuna, Trosak, Kol, Pdv, Ukupno, Nacin | WHERE BrojRacuna={br} | FMRacunE | Load receipt items for ELN fiscal |
| 3111 | UPDATE | printracuni | fisrac, fisvr, fisIZN | WHERE BrojRacuna={id} | snimFiskal | Save fiscal response |
| 3121 | UPDATE | printracunifooter | nap (concat) | WHERE BrojRacuna={id} | snimFiskal | Append fiscal note to footer |

### 3.2 INSERT Operations

| Line | Table | Columns | Function | Business Purpose |
|------|-------|----------|----------|-----------------|
| 174 | (dynamic tables) | * | inportgod | Import data from godbaza |
| 356 | alarm | opis, opis1, odgovor, vrijeme, vrijeme1, tip, chk, rpt, week, pon, uto, sri, cet, pet, sub, ned, radnik, soba, storno, vr_upis | alarm | Create new alarm |
| 518 | rezervacijeizvor | id, naziv | izmjene | Seed: 'nema podataka' |
| 523-526 | rezervacijetip | id, naziv | izmjene | Seed: Nema podataka, Mail, Fax, Telefon |
| 533 | rezervacijegrupe | id, naziv, odjavljena | izmjene | Seed: Nema podataka |
| 580 | troskovivrste | naziv, tip | izmjene | Seed: 'Nocenje sa doruckom', tip=1 |
| 1934 | setings | verbaz, verpr, izmjver, rad | bazaSql | Store version info |
| 2286 | predracuni | broj, brojpred, ime, frima, frimaid, dadtum, datumval, aktiv, ukupno, kontakt, napomnena, veza, vezaid, rabat, vr_upis, d1, d2, d3, sifra1, vrplac, nazivp, nazivid, gostid | predracunInsert | Create proforma |
| 2393 | predracunidet | BrojRacuna, Trosak, Kol, CijBezPdv, UkupnoBezPdv, Pdv, IznosPdv, Ukupno, Nacin, Valuta, OznakaValute, Popust, popust1, razlogp, pop, trosakId, predid, t1, trosakid1 | predracunDetInsert | Create proforma detail line |

### 3.3 UPDATE Operations

| Line | Table | Columns | WHERE | Function | Business Purpose |
|------|-------|----------|-------|----------|-----------------|
| 441 | rezervacije | prijava=1 | WHERE prijava=0 AND checkInDate < yesterday | provrezstare | Auto-checkin old reservations |
| 389 | alarm | storno=1, radnStorn, stornovr | WHERE id={id} | alarStorno | Cancel alarm |
| 495 | setings | verbaz, verpr, izmjver, rad | ORDER BY id DESC LIMIT 1 | baza | Record version update |
| 576 | printracunidetalji | nacinid | JOIN placanjenacin SET nacinid=n.id | (payment method migration) |
| 577 | printracunidetalji | nacinid=5 | WHERE nacinid=0 | izmjene | Default payment method |
| 588 | printracuni | datr=STR_TO_DATE(datr,'%d.%m.%Y') | (date format fix) | izmjene | Fix date format |
| 602,618 | printracuni | datr | WHERE brojracuna={br} | izmjene | Fix invalid dates |
| 628 | printracuni | dat | JOIN placanje SET dat=placanje.datum | (add dat column) | izmjene |
| 2309 | predracuni | brojpred, ime, frima, frimaid, dadtum, datumval, aktiv, ukupno, kontakt, napomnena, veza, vezaid, rabat, vr_upis, d1, d2, d3, sifra1, vrplac, nazivp, nazivid, gostid | WHERE broj={br} | predracunUpdate | Update proforma |
| 2420 | predracunidet | BrojRacuna, Trosak, Kol, CijBezPdv, UkupnoBezPdv, Pdv, IznosPdv, Ukupno, Nacin, Valuta, OznakaValute, Popust, popust1, razlogp, pop, trosakId, predid, t1, trosakid1 | WHERE id={id} | predracunDetUpdate | Update proforma detail |
| 1260 | relgostsoba | pl=0 | (all rows) | bazaSql | Reset pl flag |
| 1262 | relgostsoba | pl=1 | JOIN placanje | bazaSql | Set pl=1 where payment exists |

### 3.4 DELETE Operations

| Line | Table | WHERE | Function | Business Purpose |
|------|-------|-------|----------|-----------------|
| 2362 | predracuni | WHERE broj={br} | predracunBris | Delete proforma |
| 2063 | avans | (all - DROP TABLE) | bazaSql | Remove old avans table |
| 2069 | printracunidetaljiavans | (all - DROP TABLE) | bazaSql | Remove old avans details table |

### 3.5 VIEW Creation (lines 788-843)

| Line | View Name | Definition | Purpose |
|------|-----------|-----------|---------|
| 788 | brojsoba | SELECT count(sobe.ID) AS BS FROM sobe | Total room count |
| 797 | bzs | SELECT count(DISTINCT relgostsoba.sobaID) WHERE odjavljen=0 AND rezervacija=0 | Occupied rooms count |
| 802 | folio | SELECT SID, zakljucen FROM posjetafolio WHERE zakljucen=0 | Open folios |
| 807 | gostipid | SELECT gosti.ime, gosti.prezime, checkInDate, checkOutDate, odjavljen, tarifaID, relgostsoba.ID WHERE PID=0 AND ID<>0 | Guests without payment | 
| 813 | pid | SELECT ID, SID, zakljucen FROM posjetafolio WHERE zakljucen=0 | Open folio IDs |
| 819 | radniksmjena | SELECT smjenaID, start, ime, radnik FROM radnici JOIN smjene ORDER BY start DESC LIMIT 1 | Current shift worker |
| 825 | sobev | SELECT sobe.naziv, sobe.vrsta, sobe.lokal, sobavrsta.naziv, sobavrsta.brojKreveta, sobavrsta.defaultTarifa | Room type view |
| 831 | troskovisuma | SELECT troskovivrste.naziv, SUM(troskovi.iznos) GROUP BY naziv | Expense summary by type |

### 3.6 Stored Procedures Created (lines 844-1234)

| Line | Procedure/Function Name | Parameters | Purpose |
|------|------------------------|------------|---------|
| 852 | addFolio | sSID, svrijemeD, svrijemeO, szakljucen | Insert posjetafolio record |
| 861 | getIzvjestajDnevniPlacanje | d1atumOD, d1atumDO | Daily payment report |
| 867 | getIzvjestajDnevniPlacanjeN | d1atumOD, d1atumDO, pl | Daily payment report by method |
| 873 | getIzvjestajDnevni | datumOD, datumDO | Daily report (payment sums by method) |
| 879 | placanje2 | DS, DK | Payment report by date range |
| 885 | placanje3 | DS, DK | Advance payment report |
| 891 | placanje1 | DS, DK | Payment report (sum by method) |
| 897 | addSmjenaStart | radnikID, start | Insert new shift |
| 903 | addTroskovi | iddzid, GSID, SID, TID, vrijeme, kolicina, iznos, radnikID | Insert expense |
| 909 | getBrojGostiju | (none) | Count guests per room |
| 915 | getDokumenti | (none) | List document types |
| 921 | getGlavniImena | (none) | Get main guest names with rooms |
| 926 | getGlavniPodaci | IDd | Get guest-room details by ID |
| 931 | getGlavniTrosakIme | idd | Get expenses for a guest-room |
| 936 | getGosti | (none) | Get all current guests |
| 942 | getGostiKucice | br | Get guest by ID |
| 947 | getGostisvi | od1t, do1, tip | Get guests by date range |
| 952 | getLogs | dugme, vrijeme, opis, radnik, radnikid, opis1 | Insert log entry |
| 957 | getNeodjavljeneSobe1 | (none) | Get rooms with guests not checked out |
| 963 | getNeplacene | (none) | Get unpaid balances |
| 969 | getNeplaceneSUM | (none) | Sum of unpaid balances |
| 974 | getOdjavaCombo | (none) | Get checkout combo items |
| 979 | getPlacanjaPID | PID | Get payments by PID |
| 984 | getPlacanjeNocenja | PID | Sum of accommodation payments by PID |
| 989 | getPosjete | GID | Get guest visits |
| 994 | getPrintDetalji | RBR | Get receipt details |
| 1000 | getPrintFooter | brojRAC | Get receipt footer |
| 1005 | getPrintHeader | (none) | Get receipt headers |
| 1010 | getRezervacijePrijava | Startt | Get reservations for check-in |
| 1015 | getSobaPodaci | sobaNaziv | Get room details by name |
| 1020 | getSobeSadrzaji | sobaNaziv | Get room amenities |
| 1025 | getSobeShema | datumP, datumK | Get room schema (status) for date range |
| 1030 | getStatistika | (none) | Get statistics (nights, occupied, total) |
| 1035 | getTarifaSve | (none) | Get all tariff names |
| 1040 | getTelefonski | datumPt, datumKt, sobaLokal | Get phone calls by room |
| 1045 | getTelefonskiSve | datumPt, datumKt | Get all phone calls by date |
| 1051 | getTroskoviSoba | SobaID | Get expenses for room |
| 1056 | gosti1 | id | Get guest reservation history |
| 1061 | podaciGostiSobe | brojSobe | Get guest-room data by room name |
| 1066 | promijeniDatumVrijeme | DD, DOD, GID | Update checkOutDate by guest |
| 1071 | promijeniGosti | (all guest fields) | Update guest information |
| 1076 | PromjenaFolio | noviSID, PID | Change folio SID |
| 1081 | Unesinocenja | RID, DatumPp, Tarifa, SID, PID, opis, Pop, ssoba | Insert night with delete existing for same month |
| 1086 | unesiPojedinacne | noviSID, ID, stariSID | Move expense to new room |
| 1091 | unesirelsobavrstasobatarifa | ttarifaID, sSobaVrstaID | Insert room type-tariff relation |
| 1096 | updateNapomena | napomena | Update note by ID=1 |
| 1101 | updateSobaClean | naziv, clean | Update room clean status |
| 1106 | updateSobaOOO | naziv, razlog, ooo | Update room OOO (out of order) status |
| 1111 | vratiGostSoba | sid | Get guests by room |
| 1116 | vratiMaxDatum | sidd | Get max accommodation date |
| 1121 | vratiRIDNocenja | SIDd | Get distinct RIDs for room nights |
| 1126 | vratiTarifePoVrsta | svid | Get tariffs by room type |
| 1131 | vratiTrenutnoRezervisane | Dayy | Get currently reserved room types |
| 1136 | vratiTrenutnoSlobodne | (none) | Get free room types |
| 1141 | vratiTrenutnoZauzete | (none) | Get occupied room types |
| 1146 | vratiTrosakSoba | SID | Sum expenses for room |
| 1151 | fnBrojGostiju (FUNCTION) | soID | Count guests in room |
| 1156 | fnBrojNocenja (FUNCTION) | (none) | Count total occupied rooms |
| 1161 | fnBrojSoba (FUNCTION) | (none) | Count total rooms |
| 1166 | fnBrojZauzetihSoba (FUNCTION) | (none) | Count distinct occupied rooms |
| 1177 | fnSobaStatus (FUNCTION) | SoID, datumP, datumK, tod | Get room status code (0-6, see below) |
| 1182 | fntest (FUNCTION) | soID | Test function (returns 2) |
| 1187 | unesisobatarifa (FUNCTION) | nazivv, naziv2, usl | Insert tariff if not exists, return ID |
| 1192 | addPlacanjeSlozeno | rbt, nacin, iznos | Insert complex payment |
| 1198 | getRezrervacijePrikazi | ddatOD, ddatDO | Get reservations for display |
| 1204 | getRezrervacijePrikaziOD | ddatOD, ddatDO | Get reservations for specific date |
| 1210 | getRezrervacijePrikaziPot | ddatOD, ddatDO | Get confirmed reservations |
| 1215 | getRezrervacijePrikaziSto | ddatOD, ddatDO | Get cancelled (stornirana) reservations |
| 1226 | getTroskoveLista | brojSobe | Get expenses list by room |

### 3.7 ALTER TABLE Operations in izmjene() (lines 497-671)

Over 60 ALTER TABLE statements modify the following tables: placanjedetalji, placanje, setings, rezervacije, relgostsoba, troskovi, printracunidetalji, nocenja, alarm, rezervacijegrupe, sobe, printracuni, radnici, napomena, partneri, drzave, goststatus, telefonskiimenik, troskovivrste.

Key schema additions:
- `placanjedetalji`: rid, sid, gid, soba, sobavr, sobavrid, periodod, perioddo, period, ime, usluga, popust, pdv, hotelid, idd (lines 499-500)
- `placanje`: tip, racn, racime, pdv, ctax, sobar, perio, veza, napom, napokraj, napomena, fiskalni, fiskal, fiskalizn, fiskalvr, fiskalrek, fiskalnrekvr, placnaz, uplatetex, hotelid, idd (line 502)
- `setings`: naplposo, pribora, pdv, pdvo, pdvtax, pdvtr, osig, taxa, cultur, sobegrupa, sobekuc, dijecagod, dijecapop, fiscal, valuta, racunbr, napomena, lokac, cijt (line 504)
- `rezervacije`: dateizmjena, datestorno, datepotvrda, razlogst (line 506)
- `printracuni`: dat, knj, printime, kid, exp, datstor, grupa, idkl, zatvoreno (lines 626-662)

### 3.8 CREATE TABLE Operations in createTable() (line 761)

Creates IF NOT EXISTS: alarm, drzave, fisc, godine, gostdokument, gosti (with FK), logcont

Also line 769: CREATE TABLE logcont

### 3.9 CREATE TABLE in izmjene() (lines 555-561, 578, 644, 650, 652, 668)

- kard (line 498)
- rezervacije1 (line 555)
- rezervacijasobe (line 557)
- rezervacijasobe1 (line 558)
- komercijalista (line 559)
- sobaricalog (line 561)
- tarifatxe (line 578)
- kontroler (line 582)
- napomenad (line 644)
- konta (line 650)
- export (line 652)
- placanja (line 668)

---

## 4. Business Rules

### 4.1 Card Reader Logic (lines 55-123)
- If `pk=1` (already checking), return immediately (prevents re-entry)
- If `ipadr` is empty, return (no controller configured)
- After setting `pk=1`, **immediately returns** (lines 59-60) - effectively **disabled**
- Original logic (lines 62-122) processes card reader status:
  - If connected: set `kardst = "Konekcija uspjesna"`
  - If disconnected and `kontr.po=0`: recreate controller object
  - If reading timeout (>2 iterations): log error tip=16 "uredjaj ne odgovara vise od 10 sekundi"
  - When tip=9 (inputs): parse `ulaz` string bits into room status fields:
    - Positions 0/1/2/3 → `gost` (guest presence per controller row)
    - Positions 12/9/10/11 → `sos` (SOS per controller row)
    - Positions 8/13/14/15 → NOT USED (should be vatr/fire per comments, but 8 maps to nothing in row 1)
  - Actually the mapping is: redulko 1→ulaz[0], redulko 2→ulaz[1], redulko 3→ulaz[2], redulko 4→ulaz[3] for gost; similar patterns for sos and vatr

### 4.2 Room Status from SQL fnSobaStatus (line 1177)
- **0** = Room free/available
- **1** = Room occupied
- **2** = Room occupied, guest checking out today or past checkout date
- **3** = Room reserved and confirmed
- **4** = Room reserved AND occupied
- **5** = Room out of order (OOO=1)
- **6** = Room reserved but not confirmed

### 4.3 Reservation Auto-Checkin (line 441)
- `provrezstare()`: Marks reservations with `prijava=1` where `prijava=0` AND `checkInDate < yesterday`
- Business meaning: Reservations not checked in within 1 day are auto-marked

### 4.4 Advance Receipt Storno (lines 308-314)
- If `storno=1`: negate `iznos`, `porezIz`, `ukupno` values
- Change `racin` to "STORNO AVANSNE FAKTURE:"

### 4.5 Proforma Detail Insert Validation (lines 2391-2398)
- Only insert detail line if:
  - `Trosak` (description) is not empty/whitespace
  - `ukupno` is numeric AND > 0

### 4.6 Fiscal Receipt Generation - KTE (lines 2707-2874)
- Tax rate mapping: If pdv=17% → porez=2, else → porez=4
- Item name max 18 chars, truncated, spaces→underscores
- Special chars stripped: &, <, >, ", ', /
- Discount handling: numeric discount formatted as percentage
- Price = Total/Quantity (unit price derived from line total)
- XML format: TremolFpServer Receipt command
- Additional lines: worker name + "www.imedia.ba"
- File copied to fiscal device folder from `setings.fiscal.Split("*")(3)`

### 4.7 Fiscal Receipt Generation - ELN (lines 2969-3039)
- Tax mapping: pdv=17% → "E", else → "K" (K=lower tax rate)
- Item name max 18 chars, diacritics not fully stripped (only č→c, đ→d partially)
- Special chars stripped: &, <, >, ", ', /
- Format: "S,1,______,_,__;+;{name};{price};{qty};{tax};0;{id};"
- File written to ra.in, copied to fiscal folder from `setings.fiscal.Split("*")(2)`
- Uses FileSystemWatcher on *.eln files for response

### 4.8 Fiscal Response Processing (lines 3042-3082)
- Monitors fiscal folder for .eln response files
- Parses response looking for "BF:" prefix → extracts fiscal number
- Calls `snimFiskal()` with the fiscal number

### 4.9 Fiscal Data Save (lines 3107-3143)
- UPDATE printracuni SET fisrac, fisvr, fisIZN WHERE BrojRacuna=id
- UPDATE printracunifooter SET nap=concat(nap, fiscal note) WHERE BrojRacuna=id
- Appends legal notice: "Po clanu 42 fiskalnog zakona faktura je pravljena na osnovu fiskalnih racuna: Br. {br}"

### 4.10 Data Import (lines 154-196)
- `tip=0`: Import 38 tables (partial)
- `tip=1`: Import 52 tables (full)
- Uses INSERT INTO...SELECT * FROM godbaza.table
- On failure: DROP table and CREATE TABLE...SELECT (re-import)
- Special case: `setings` table → only reads `rep` column from `hot`
- SQL injection risk: table names from arrays but not parameterized

### 4.11 Alarm System (lines 337-439)
- Insert alarm: 16 parameters including weekday flags (pon/uto/sri/cet/pet/sub/ned)
- Storno alarm: sets `storno=1`, records `radnStorn` and `stornovr`
- Check alarms: only `storno=0 AND tip=1 AND vrijeme<now+10h`
- Read alarms: status derived: tip=1 AND storno=0 → 'Aktivno', tip=1 AND storno=1 → 'Stornirano', else → 'Zavrseno'

### 4.12 KTE Storno (lines 2875-2897)
- Creates file with header "48,1,______,_,__;{date};{worker};000000;;1;"
- Each item: "52,1,______,_,__;{name};{price};"
- Footer: "53,1,______,_,__;;;" then "56,1,______,_,__;"

---

## 5. Status/Magic Values

### 5.1 Room Status (fnSobaStatus, line 1177)

| Value | Meaning | Where Used |
|-------|---------|------------|
| 0 | Room free/available | fnSobaStatus, view getSobeShema |
| 1 | Room occupied | fnSobaStatus |
| 2 | Room occupied, check-out today or overdue | fnSobaStatus |
| 3 | Room reserved and confirmed (rezervP=1) | fnSobaStatus |
| 4 | Room reserved AND occupied | fnSobaStatus |
| 5 | Room out of order (OOO) | fnSobaStatus |
| 6 | Room reserved, not confirmed (rezervP=0) | fnSobaStatus |

### 5.2 Card Reader Tip Values (lines 29-48)

| Value | Meaning |
|-------|---------|
| 0 | Default/empty row |
| 1 | Written card response |
| 2 | Written log error |
| 3 | Written log power-on count |
| 4 | Written log VOC reset count |
| 5 | Written log entry count |
| 6 | Written log deleted or no log |
| 7 | Written log read |
| 8 | Written programmed outputs |
| 9 | Written programmed inputs |
| 10 | Written device time |
| 11 | SOS alarm |
| 12 | Fire alarm |
| 13 | Minibar |
| 14 | Invalid card |
| 15 | Unauthorized housekeeping |
| 16 | Device not responding |
| 17 | Power on |
| 18 | Power off |
| 19 | Card read |

### 5.3 Alarm Tip Values

| Value | Meaning | Evidence |
|-------|---------|----------|
| 1 | Active alarm (reminder) | provjerialarm SQL WHERE tip=1, citajAlarm status calculation |

### 5.4 Alarm Storno Values

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Not cancelled | provjerialarm WHERE storno=0 |
| 1 | Cancelled | alarStorno SET storno=1 |

### 5.5 Reservation prijava Values

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Not checked in | provrezstare WHERE prijava=0 |
| 1 | Checked in (or auto-checked-in) | provrezstare SET prijava=1 |

### 5.6 Reservation stornirana Values

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Active reservation | getRezrervacijePrikazi WHERE stornirana=0 |
| 1 | Cancelled reservation | getRezrervacijePrikaziSto WHERE stornirana=1 |

### 5.7 Reservation potvrda Values

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Not confirmed | getRezrervacijePrikaziPot WHERE potvrda=1 |
| 1 | Confirmed | getRezrervacijePrikaziPot |

### 5.8 Guest odjavljen Values

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Currently checked in | Multiple views WHERE odjavljen=0 |
| 1 | Checked out | (implied) |

### 5.9 Guest rezervacija Values

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Actual stay (not reservation) | Multiple views WHERE rezervacija=0 |
| 1 | Reservation | fnSobaStatus |

### 5.10 Advance Receipt Storno

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Normal advance receipt | avans_print parameter |
| 1 | Storno advance receipt | avans_print: negates amounts, changes label |

### 5.11 Fiscal Tax Rate Mapping

| KTE System | ELN System | Meaning | Evidence |
|-----------|-----------|---------|----------|
| porez=2 | "E" | 17% VAT (standard) | FMRacunKTE line 2735, FMRacunE line 3007 |
| porez=4 | "K" | Reduced VAT | FMRacunKTE line 2738, FMRacunE line 3009 |

### 5.12 Expense Type Tip (troskovivrste)

| Value | Meaning | Evidence |
|-------|---------|----------|
| 1 | Nocenje sa doruckom (BB/accommodation) | izmjene line 580 |

### 5.13 Room Clean Status

| Value | Meaning | Evidence |
|-------|---------|----------|
| Unknown | Room cleanliness flag | updateSobaClean procedure, sobe table `clean` column |

### 5.14 Room OOO Status

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | In service | sobe.ooo, fnSobaStatus checks ooo=1 |
| 1 | Out of order (OOO) | fnSobaStatus WHERE ooo=1 → status=5 |

### 5.15 Night Record (nocenja.PrijavaOdjava)

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Accommodation charge (night) | Unesinocenja procedure INSERT PrijavaOdjava=0 |

### 5.16 Expense zaklj Status

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Open/active expense | getTroskoviSoba WHERE zaklj=0 |
| Non-zero | Closed/finalized expense | (implied) |

### 5.17 Setings Fields (from line 504)

| Field | Type | Default | Meaning |
|-------|------|---------|---------|
| naplposo | INTEGER | 0 | Naplata poslovnog (business billing) |
| pribora | VARCHAR(50) | - | Utensils/cutlery |
| pdv | DOUBLE | 0 | VAT rate |
| pdvo | DOUBLE | 0 | Second VAT rate |
| pdvtax | DOUBLE | 0 | Tax rate |
| pdvtr | DOUBLE | 0 | VAT transitional |
| osig | DOUBLE | 0 | Insurance rate |
| taxa | DOUBLE | 0 | Tourism tax |
| cultur | VARCHAR(40) | - | Currency culture |
| sobegrupa | VARCHAR(150) | - | Room groupings |
| sobekuc | VARCHAR(150) | - | Room building |
| dijecagod | DOUBLE | 0 | Children age threshold |
| dijecapop | DOUBLE | 0 | Children discount |
| fiscal | VARCHAR(150) | - | Fiscal device config (star-separated) |
| valuta | VARCHAR(20) | - | Currency code |
| racunbr | VARCHAR(50) | - | Receipt number format |
| napomena | VARCHAR(250) | - | Note |
| lokac | VARCHAR(50) | - | Location |
| cijt | INTEGER | 0 | Unknown (price type?) |

---

## 6. Database Writes

### 6.1 INSERT Operations

| Line | Table | Columns | Condition | Function | Rollback | Business Reason |
|------|-------|----------|-----------|----------|----------|----------------|
| 174 | (dynamic) | * | tip=0: 38 tables, tip=1: 52 tables | inportgod | None (catch ignores) | Annual data import |
| 356 | alarm | opis, opis1, odgovor, vrijeme, vrijeme1, tip, chk=0, rpt, week, pon-ned, radnik, soba, storno=0, vr_upis | Direct insert | alarm | None | Create reminder/alert |
| 518 | rezervacijeizvor | id=1, naziv='nema podataka' | Only if table empty | izmjene | None | Seed default data |
| 523-526 | rezervacijetip | id=1-4, naziv | Only if table empty | izmjene | None | Seed source types |
| 533 | rezervacijegrupe | id=1, naziv='Nema podataka', odjavljena=0 | Only if table empty | izmjene | None | Seed group |
| 580 | troskovivrste | naziv='Nocenje sa doruckom', tip=1 | Only if ALTER succeeded | izmjene | None | Seed accommodation expense type |
| 2286 | predracuni | All proforma fields | Direct insert | predracunInsert | None | Create proforma invoice |
| 2393 | predracunidet | All detail fields | Only if Trosak non-empty AND ukupno>0 | predracunDetInsert | None | Create proforma detail line |

### 6.2 UPDATE Operations

| Line | Table | SET | WHERE | Function | Rollback | Business Reason |
|------|-------|-----|-------|----------|----------|----------------|
| 389 | alarm | storno=1, radnStorn={radnik}, stornovr={now} | id={id} | alarStorno | None | Cancel alarm |
| 441 | rezervacije | prijava=1 | prijava=0 AND checkInDate<yesterday | provrezstare | None | Auto-checkin old reservations |
| 495 | setings | verbaz, verpr, izmjver, rad | ORDER BY id DESC LIMIT 1 | baza | None | Record version |
| 576 | printracunidetalji | nacinid=n.id | JOIN placanjenacin | izmjene | None | Migrate payment method IDs |
| 577 | printracunidetalji | nacinid=5 | nacinid=0 | izmjene | None | Default payment method |
| 588 | printracuni | datr=STR_TO_DATE(datr,'%d.%m.%Y') | (all rows with dot dates) | izmjene | None | Fix date format |
| 602,618 | printracuni | datr={previous valid date} | brojracuna={br} (invalid dates only) | izmjene | None | Fix invalid dates |
| 628 | printracuni | dat=placanje.datum | JOIN placanje ON placanje.broj=printracuni.brojracuna | izmjene | None | Populate dat from payment |
| 2309 | predracuni | All proforma fields | broj={br} | predracunUpdate | None | Update proforma |
| 2420 | predracunidet | All detail fields | id={id} | predracunDetUpdate | None | Update proforma detail |
| 3111 | printracuni | fisrac={br}, fisvr={vr}, fisIZN={izn} | BrojRacuna={id} | snimFiskal | None | Save fiscal device response |
| 3121 | printracunifooter | nap=concat(nap, fiscal_text) | BrojRacuna={id} | snimFiskal | None | Append fiscal legal note |

### 6.3 DELETE Operations

| Line | Table | WHERE | Function | Rollback | Business Reason | BUG? |
|------|-------|-------|----------|----------|----------------|------|
| 2362 | predracuni | broj={br} | predracunBris | None | Delete proforma | No |
| 2473 | predracunidet | **Uses SELECT instead of DELETE!** | predracunDetBris | None | Intended delete | **YES - BUG** |

**NOTABLE BUG** at line 2473: `predracunDetBris` executes `"select predracunidet from predracunidet WHERE Id =" & id` instead of `DELETE FROM`. The function is named "Bris" (delete) but performs a SELECT.

---

## 7. Error Messages and User Messages

| Line | Message Text | Context | Type |
|------|-------------|---------|------|
| 141 | `j.Message` (exception message) | loadsetg - header load error | MsgBox |
| 151 | `j.Message` (exception message) | loadsetg - status load error | MsgBox |
| 321 | `"Greska, avans_print!" & vbCrLf & e.Message` | avans_print SQL error | MsgBox |
| 326 | `"Greska!" & vbCrLf & "avans_print 1!"` | avans_print system error | MsgBox |
| 363 | `"Greska u konekciji sa bazom podataka!" & vbCrLf & e.Message` | alarm SQL error | MsgBox |
| 369 | `"Greska!" & vbCrLf & e.Message` | alarm system error | MsgBox |
| 397 | `"Greska u konekciji sa bazom podataka!" & vbCrLf & e.Message` | alarStorno SQL error | MsgBox |
| 401 | `"Greska!" & vbCrLf & e.Message` | alarStorno system error | MsgBox |
| 425 | `"Greska u konekciji sa bazom podataka!" & vbCrLf & e.Message` | provjerialarm SQL error | MsgBox |
| 431 | `"Greska!" & vbCrLf & e.Message` | provjerialarm system error | MsgBox |
| 473 | `"Greska u konekciji sa bazom podataka!" & vbCrLf & e.Message` | citajAlarm SQL error | MsgBox |
| 479 | `"Greska!" & vbCrLf & e.Message` | citajAlarm system error | MsgBox |
| 605 | `"prvi rekord nije ipravan datum prinracuni"` | Invalid date in printracuni | MsgBox |
| 621 | `"prvi rekord nije ipravan datum prinracuni"` | Invalid date in printracuni | MsgBox |
| 1237 | `"baza"` | bazaSql entry | MsgBox |
| 2191 | `"Greska u konekciji sa bazom podataka!-20a"` | prikaziAvans SQL error | MsgBox |
| 2196 | `"Greska!r-20a"` | prikaziAvans system error | MsgBox |
| 2217 | `"Greska u konekciji sa bazom podataka!artGet"` | artGet SQL error | MsgBox |
| 2221 | `"Greska!artGet" & vbCrLf & "Pokusajte ponovo,artGet 1!"` | artGet system error | MsgBox |
| 2242 | `"Greska u konekciji sa bazom podataka!artGet"` | artProvjera SQL error | MsgBox |
| 2246 | `"Greska!artGet" & vbCrLf & "Pokusajte ponovo,artGet 1!"` | artProvjera system error | MsgBox |
| 2268 | `"Greska u konekciji sa bazom podataka!predracunInsert"` | predracunInsert SQL error | MsgBox |
| 2272 | `"Greska!r-20a" & vbCrLf & "Pokusajte ponovo,predracunInsert 1!"` | predracunInsert system error | MsgBox |
| 2292 | `"Greska u konekciji sa bazom podataka!predracunInsert"` | predracunInsert SQL error (duplicate) | MsgBox |
| 2296 | `"Greska!r-20a" & vbCrLf & "Pokusajte ponovo,predracunInsert 1!"` | predracunUpdate system error | MsgBox |
| 2315 | `"Greska u konekciji sa bazom podataka!predracunUpdate"` | predracunUpdate SQL error | MsgBox |
| 2319 | `"Greska!r-20a" & vbCrLf & "Pokusajte ponovo,predracunUpdate 1!"` | predracunUpdate system error | MsgBox |
| 2344 | `"Greska u konekciji sa bazom podataka!predracunSeL"` | predracunSeL SQL error | MsgBox |
| 2348 | `"Greska!r-20a" & vbCrLf & "Pokusajte ponovo,predracunSeL 1!"` | predracunSeL system error | MsgBox |
| 2372 | `"Greska u konekciji sa bazom podataka!predracunInsert"` | predracunBris SQL error | MsgBox |
| 2376 | `"Greska!r-20a" & vbCrLf & "Pokusajte ponovo,predracunInsert 1!"` | predracunBris system error | MsgBox |
| 2401 | `"Greska u konekciji sa bazom podataka!predracunInsert"` | predracunDetInsert SQL error | MsgBox |
| 2406 | `"Greska!r-20a" & vbCrLf & "Pokusajte ponovo,predracunInsert 1!"` | predracunDetInsert system error | MsgBox |
| 2427 | `"Greska u konekciji sa bazom podataka!predracunDetUpdate"` | predracunDetUpdate SQL error | MsgBox |
| 2431 | `"Greska!r-20a" & vbCrLf & "Pokusajte ponovo,predracunDetUpdate 1!"` | predracunDetUpdate system error | MsgBox |
| 2454 | `"Greska u konekciji sa bazom podataka!predracunDetSeL"` | predracunDetSeL SQL error | MsgBox |
| 2459 | `"Greska!r-20a" & vbCrLf & "Pokusajte ponovo,predracunDetSeL 1!"` | predracunDetSeL system error | MsgBox |
| 2480 | `"Greska u konekciji sa bazom podataka!predracunDetBris"` | predracunDetBris SQL error | MsgBox |
| 2484 | `"Greska!r-20a" & vbCrLf & "Pokusajte ponovo,predracunDetBris 1!"` | predracunDetBris system error | MsgBox |
| 2497 | `"Nedostaje file bazakasa.sql!!!"` | Missing SQL file | MsgBox |
| 2509 | `"Cannot read file from disk. Original error: "` | File open error | MessageBox.Show |
| 2856 | `e.Message` | KTE file copy error | MsgBox Critical |
| 2934 | `e.Message` | Termol worker file copy error | MsgBox Critical |
| 2953 | `"Greska u konekciji sa bazom podataka ili je prvi racun u sistemu!"` | sifraGetdet SQL error | MsgBox |
| 2958 | `"Greska u konekciji sa bazom podataka ili je prvi racun u sistemu!"` | sifraGetdet system error | MsgBox |
| 3120 | `"Po clanu 42 fiskalnog zakona faktura je pravljena na osnovu fiskalnih racuna: Br. {br}"` | Fiscal response note appended to footer | DB UPDATE |
| 3129 | `"Greska u konekciji sa bazom podataka!-17sf"` | snimFiskal SQL error | MsgBox |
| 3134 | `"Greska!" & vbCrLf & "Pokusajte ponovo!13sf"` | snimFiskal system error | MsgBox |

---

## 8. Cross-Reference

### 8.1 Function Call Graph

```
New() → (initializes kardTable)

baza()
  ├── createTable()
  ├── createTablefile()
  ├── viewcr()
  ├── izmjene()
  │   ├── mysqlReader() [multiple times]
  │   └── mysqlExScalar() [60+ times]
  └── bazaProc()

provjeri_kard() → (disabled, but originally): kontr.citaj_logove(), kontr.Disconnect(), kontr (New)

sobe_load() → mysqlReader()

loadsetg()
  ├── frmGlavni.settingsp()
  └── mysqlReader() [x2]

inportgod(tip) → mysqlExScalar() [dynamic based on table array]

avans_print(br, storno)
  └── mysqlReader() [via mysqlCommand]

alarm(...) → mysqlExNonQuery (INSERT)

alarStorno(radnik, id) → mysqlExNonQuery (UPDATE)

provjerialarm() → mysqlCommand (SELECT)

provrezstare() → mysqlExScalar (UPDATE)

citajAlarm() → mysqlCommand (SELECT)

sobanaziv(sid) → mysqlReader()

checkchema(shem) → mysqlCommand (SELECT from INFORMATION_SCHEMA)

crecshem(str) → mysqlCommand (CREATE DATABASE)

predracunGetZadnji() → mysqlCommand (SELECT MAX)

predracunInsert(dt) → mysqlCommand (INSERT)

predracunUpdate(dt, br) → mysqlCommand (UPDATE)

predracunSeL(br) → mysqlCommand (SELECT)

predracunBris(br) → mysqlCommand (DELETE)

predracunDetInsert(dt) → mysqlCommand (INSERT) [in loop]

predracunDetUpdate(dt, id) → mysqlCommand (UPDATE) [in loop]

predracunDetSeL(ID) → mysqlCommand (SELECT)

predracunDetBris(id) → mysqlCommand (SELECT) [BUG: should be DELETE]

createTablefile() → File.ReadAllText, mysqlCommand (execute SQL file)

mysqlExScalarK(queri) → MySqlConnection.ExecuteScalar (Kasa DB)

mysqlReaderK(queri, tablename) → MySqlConnection.ExecuteReader (Kasa DB)

mysqlExScalarLast(queri) → mysqlCommand.ExecuteScalar + SELECT @@Identity

mysqlExScalar(queri) → mysqlCommand.ExecuteScalar (returns Boolean)

mysqlReader(queri, tablename) → mysqlCommand.ExecuteReader

mysqlProcedure(queri, tablename) → mysqlCommand.ExecuteReader (StoredProcedure)

krrTE(put) → XML file creation

FMRacunKTE(br)
  ├── mysqlReader() [printracunidetalji]
  └── FileCopy() to fiscal folder

stornoKTE(tip, radnik, dt, ibobr, putod, putdo) → File creation + FileCopy

upisi_radnikeTermol(id, rad) → XML file creation + FileCopy

sifraGetdet(rac) → mysqlCommand (SELECT from sifarnik)

FMRacunE(br)
  ├── mysqlReader() [printracunidetalji]
  └── FileCopy() to fiscal folder

fww(sender, e) [event handler]
  └── snimFiskal(bro, Now, 0, brrac)

stornoE(tip, radnik, dt, ibobr, putod, putdo) → File creation + FileCopy

snimFiskal(br, vr, izn, id) → mysqlCommand (UPDATE x2)
```

### 8.2 Functions Called by External Forms (inferred from naming patterns)

| Function | Likely Called By | Evidence |
|----------|-----------------|----------|
| `sobe_load()` | frmGlavni (main form) | Referenced in loadsetg |
| `loadsetg()` | frmGlavni | Calls frmGlavni.settingsp() |
| `baza()` | Application startup | Initializes entire DB |
| `provjeri_kard()` | Timer on frmGlavni | Card reader polling |
| `provjerialarm()` | Alarm check routine | frmGlavni timer |
| `provrezstare()` | frmGlavni or daily routine | Auto-checkin |
| `avans_print()` | Billing form | Receipt printing |
| `alarm()` | Alarm management form | |
| `alarStorno()` | Alarm form | Cancel button |
| `citajAlarm()` | Alarm list form | |
| `inportgod()` | Settings/admin form | Year-end import |
| `predracun*()` | Proforma form (frmPredracuni?) | Complete CRUD |
| `FMRacunKTE()` | Billing form (KTE fiscal) | |
| `FMRacunE()` | Billing form (ELN fiscal) | |
| `stornoKTE()` | Billing form (KTE storno) | |
| `stornoE()` | Billing form (ELN storno) | |
| `snimFiskal()` | fww event (fiscal response) | |
| `upisi_radnikeTermol()` | Settings form | Worker registration |
| `sifraGetdet()` | Billing form | Receipt items |

### 8.3 Function Module Classification

| Priority | Module | Functions |
|----------|--------|-----------|
| P0 | Database Core | mysqlExScalar, mysqlReader, mysqlExScalarLast, mysqlProcedure, mysqlExScalarK, mysqlReaderK, createTable, viewcr, bazaProc, izmjene, bazaSql, baza, createTablefile, checkchema, crecshem, rete |
| P0 | Room Management | sobe_load, sobanaziv |
| P0 | Reservations | provrezstare |
| P0 | Billing/Receipts | avans_print, prikaziAvans, predracunGetZadnji, predracunInsert, predracunUpdate, predracunSeL, predracunBris, predracunDetInsert, predracunDetUpdate, predracunDetSeL, predracunDetBris, snimFiskal |
| P0 | Alarms | alarm, alarStorno, provjerialarm, citajAlarm |
| P0 | Settings | loadsetg |
| P1 | Hardware/Card Reader | provjeri_kard, New() |
| P1 | Expenses | artGet, artProvjera |
| P1 | Fiscal Devices | FMRacunKTE, FMRacunE, stornoKTE, stornoE, krrTE, upisi_radnikeTermol, fww |
| P1 | Data Migration | inportgod, tabzainport, tabzainportsve |
| P2 | Utility | greska (no-op), sifraGetdet |

---

## Critical Issues Found

1. **BUG at line 2473**: `predracunDetBris` uses SELECT instead of DELETE - proforma detail rows are never actually deleted
2. **Disabled code at lines 59-60**: `provjeri_kard` sets pk=1 then immediately returns, making card reader polling non-functional
3. **SQL Injection Risk**: Multiple functions use string concatenation for SQL queries (e.g., lines 174, 2286, 2309, 2393, 2420, 2946)
4. **No Transaction Management**: No BEGIN/COMMIT/ROLLBACK on multi-step operations
5. **Error Swallowing**: `greska()` function (line 2937) is completely empty - errors silently discarded
6. **Inconsistent Error Messages**: Some functions use specific identifiers ("artGet", "predracunInsert"), others use generic numbers ("-20a", "13sf")
7. **Date Format Inconsistency**: printracuni.datr migration (line 588) attempts format conversion with potential data loss
8. **Duplicate Schema Migrations**: `bazaSql` (lines 1235-2161) contains legacy SQL Server syntax alongside MySQL `izmjene()` (lines 497-671)
9. **Hard-coded Paths**: "C:\Program Files\IMEDIA\HotelPro\" (line 318), "C:\Program Files\IMEDIA\HotelPRO\" (line 2188), "d:\CreatesT.sql" (line 726)
10. **Connection Not Properly Closed**: Many functions use pattern where connection might not close on exception paths without Finally block