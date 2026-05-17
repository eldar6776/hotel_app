# LEGACY DATABASE MAP

Status: INITIAL_MAP
Date: 2026-05-17

Primary schema source: `legacy_code/novaBazaJHotel 20150602 0848.sql`

## P0 Tables

| Table | Source Line | Business Role | Key Columns | Status/Flag Columns | Financial Columns | Date Columns | Read By | Written By | Candidate Concept | Risk |
|---|---:|---|---|---|---|---|---|---|---|---|
| `sobe` | 8087 | physical room and room operational state | `ID`, `naziv`, `vrsta`, `lokal`, `zgradaID` | `ooo`, `clean`, `idkon` | none seen | none seen | `ModuleKod.vb`, `frmPrijava1.vb`, SQL procedures | `Data.PrljavaSoba`, `frmPlacanje`, OOO procedures | Room + RoomOperationalState | P0 |
| `relgostsoba` | 7482 | stay/guest-room assignment | `id`, `gostID`, `sobaID`, `PID`, `tarifaID`, `grupaID` | `odjavljen`, `rezervacija`, `rezervP`, `print1`, `print2`, `pl` | `popust`, `ostaliTroskovi` | `checkInDate`, `checkOutDate` | `Data.vb`, `frmPrijava1.vb`, `frmPlacanje.vb`, `frmGosti.vb` | `addRelGostSoba`, `Data.OdjavaSobe` | Stay / RoomAssignment | P0 |
| `posjetafolio` | 7092 | folio/visit lifecycle envelope | `ID`, `SID` | `zakljucen` | none seen | `vrijemeD`, `vrijemeO` | `frmPrijava1.vb`, `Data.vb` | `addFolio`, `Data.OdjavaSobe` | FolioSession | P0 |
| `nocenja` | 5898 | materialized night ledger | `id`, `RID`, `SID`, `PID` | `PrijavaOdjava` | `Tarifa`, `popust` | `DatumP`, `DatumOdj` | `Data.vb`, `frmPlacanje.vb`, reports/procedures | `Unesinocenja`, `Data.OdjavaSobe`, `frmPlacanje.vb` | NightLedger | P0 |
| `troskovi` | 8231 | expense ledger/open charges | `ID`, `GSID`, `SID`, `TID`, `Brrac` | `zaklj`, `Djelimicno` | `kolicina`, `iznos` | `vrijeme` | `Data.vb`, `frmPlacanje.vb`, reports | `frmPlacanje.vb`, `Data.OdjavaSobe`, transfer procedures | ExpenseLedger | P0 |
| `placanje` | 6922 | payment header | `ID`, `broj`, `relgostsobaID`, `PID`, `nacin` | `storno` | `iznos`, `popust`, `uplaceno` | `datum`, `datumOD`, `datumDO` | `frmPlacanje.vb`, reports/procedures | `frmPlacanje.vb` | PaymentLedgerHeader | P0 |
| `placanjedetalji` | 6982 | payment line/detail ledger | `ID`, `brojID`, `art`, `PID` | `storno`, `ranijeUplate` | `kolicina`, `cijena`, `iznos`, `brojNocenja` | none seen | `Data.vb`, `frmPlacanje.vb` | `frmPlacanje.vb` | PaymentLedgerLine | P0 |
| `printracuni` | 7230 | invoice print snapshot/header | `BrojRacuna`, `Ime`, `DrugoIme`, `BrojSobe`, `TipPlacanja` | `storno`, `knj` | `fisizn` | `PeriodOd`, `PeriodDo`, `datr`, `dat` | reports/export | `frmPlacanje.vb`, storno paths pending | InvoiceSnapshotHeader | P0 |
| `printracunidetalji` | 7315 | invoice print snapshot/detail | `BrojRacuna`, `Trosak`, `Nacin` | none confirmed | `Kol`, `CijBezPdv`, `UkupnoBezPdv`, `Pdv`, `IznosPdv`, `Ukupno` | none seen | reports/export | `frmPlacanje.vb` | InvoiceSnapshotLine | P0 |
| `rezervacije` | 7651 | booking/reservation | `ID`, `GID`, `sobaVrstaID`, `blokID`, `izvorID`, `tipID` | `prijava`, `potvrda`, `stornirana` | `tarifa` | `checkInDate`, `checkOutDate` | `frmPrijava1.vb`, procedures | reservation forms pending | Booking | P0 |
| `rezervacijasobe` | 7581 | assigned reservation rooms | `id`, `rezid`, `sid`, `gid`, `tid` | UNKNOWN | `tarifa` | `checkInDate`, `checkOutDate` | `frmPrijava1.vb` | reservation forms pending | BookingRoomAssignment | P0 |
| `rezervacijegrupe` | 7775 | reservation group/block | `ID`, `naziv` | UNKNOWN | UNKNOWN | UNKNOWN | `frmPrijava1.vb`, reservation procedures | group forms pending | BookingGroup | P0 |
| `gosti` | 302 | guest master/profile | `ID`, `ime`, `prezime`, document fields | maybe `del` in related queries UNKNOWN | none seen | `datumRodjenja` | check-in, reports, guest forms | `frmGosti.vb`, check-in path pending | Guest | P0 |

## Initial Relationships

| Relationship | Evidence | Meaning |
|---|---|---|
| `relgostsoba.PID -> posjetafolio.ID` | `frmPrijava1.vb` uses `provjeriPID`, `dodajFolio`, then passes `folioID` into `addRelGostSoba`; `Data.OdjavaSobe` updates `posjetaFolio WHERE ID = @PID` | one active folio can group active stay rows for a room |
| `nocenja.RID -> relgostsoba.id` | `frmPrijava1.vb` `vratiRID`; `Unesinocenja(RID, ...)`; `Data.vb` joins `nocenja` to `relgostsoba` | night ledger line belongs to guest-room stay row |
| `nocenja.SID -> sobe.ID` | `Data.vb`, schema columns | night is also room-scoped |
| `troskovi.SID -> sobe.ID` | `Data.vb`, `frmPlacanje.vb` | open expenses are room-scoped until locked/invoiced |
| `placanje.broj -> placanjedetalji.brojID` | `frmPlacanje.vb` inserts payment header and detail with same account/invoice number | payment ledger header/detail |
| `printracuni.BrojRacuna -> printracunidetalji.BrojRacuna` | report/export SQL joins | invoice snapshot header/detail |

## UNKNOWN

- Explicit foreign keys are not confirmed in the dump; relationships appear implicit.
- Full column definitions for all P0 tables need extraction into this map.
- Storno behavior must be proven from `frmRacun.vb`, `frmRacuni.vb`, `frmPlacanje.vb`, and report files.

