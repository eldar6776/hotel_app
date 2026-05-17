# LEGACY FUNCTION INVENTORY

Status: INITIAL_INVENTORY
Date: 2026-05-17

## Scope

Source root: `legacy_code/`

This inventory is evidence-first. `TARGETED_READ` means only relevant blocks were read. It does not mean full file review.

## File Counts

| Extension | Count | Notes |
|---|---:|---|
| `.vb` | 259 | VB.NET forms, modules, reports, helper classes |
| `.resx` | 104 | WinForms/resources |
| `.rpt` | 80 | Crystal Reports |
| `.xml` | 77 | report/config templates |
| `.sql` | 8 | MySQL schema/data/procedure dumps |
| other/binary | 538 | dll/exe/resources/images/build artifacts |

## P0 Target Files

| File | Type | Class/Module/Form | Key Functions/Events | SQL | Tables | Module | Risk | Read Status | Notes |
|---|---|---|---|---|---|---|---|---|---|
| `Data.vb` | VB module | `Data` | `PrljavaSoba`, `OdjavaSobe`, `pripremaRcuna`, `gettroskovi`, `nocenjeSo` | SELECT/UPDATE/INSERT | `sobe`, `nocenja`, `relgostsoba`, `posjetaFolio`, `troskovi`, `placanjedetalji` | checkout, folio, ledger | P0 | TARGETED_READ | Checkout uses transaction and materialized ledger updates. |
| `frmPrijava1.vb` | WinForms | `frmPrijava1` | `btnPrijava_Click`, `unesi`, `dodajFolio`, `nocenja`, `vratiRID`, `dodajnocenja` | SELECT + stored procedures | `posjetafolio`, `relgostsoba`, `nocenja`, `rezervacije`, `sobe` | check-in | P0 | TARGETED_READ | Check-in creates/reuses folio, inserts stay rows, then night ledger rows. |
| `frmPlacanje.vb` | WinForms | `frmPlacanje` | `placanje`, `placanje(Integer)`, `dodajPlacanje`, `generisanjeReporta`, fiscal methods | SELECT/INSERT/UPDATE | `placanje`, `placanjedetalji`, `troskovi`, `nocenja`, `printracuni`, `printracunifooter` | payment, invoice, checkout | P0 | TARGETED_READ | Payment and invoice generation are coupled with optional checkout. |
| `frmOdjava1.vb` | WinForms | `frmOdjava1` | checkout UI/event handlers | SELECT/INSERT | `relgostsoba`, `sobe`, `troskovi` | checkout | P0 | TARGETED_READ | Needs deeper caller/path review. |
| `frmRacun.vb` | WinForms | `frmRacun` | invoice/storno paths | UNKNOWN | `printracuni`, likely payment tables | invoice/storno | P0 | INVENTORIED | Full read pending. |
| `frmRezervacije*.vb` | WinForms | reservation forms | create/edit/transfer reservation | UNKNOWN | `rezervacije`, `rezervacijasobe`, `rezervacijegrupe` | booking | P0 | INVENTORIED | P0 booking flow pending. |
| `ModuleKod.vb` | VB module | global module | startup/data loading, migrations | SELECT/ALTER/UPDATE | many core tables | bootstrap/shared | P0 | TARGETED_READ | Loads room read model and table registry; contains schema evolution. |
| `novaBazaJHotel 20150602 0848.sql` | MySQL dump | schema/procedures/data | `addFolio`, `addRelGostSoba`, `Unesinocenja`, room/status/report procedures | CREATE/INSERT/UPDATE/DELETE | all core tables | database | P0 | TARGETED_READ | Authoritative persistence map source. |

## Known Entry Points

| Entry Point | File | Trigger | Downstream Calls | Status |
|---|---|---|---|---|
| `btnPrijava_Click` | `frmPrijava1.vb` | user clicks check-in/prijava | `provjeriPID`, `dodajFolio`, `unesi`, `nocenja`, RFID/card flow | TARGETED_READ |
| `unesi` | `frmPrijava1.vb` | check-in after folio resolution | stored procedure `addRelGostSoba` | TARGETED_READ |
| `nocenja` | `frmPrijava1.vb` | check-in after stay insert | `vratiRID`, `dodajnocenja` | TARGETED_READ |
| `dodajnocenja` | `frmPrijava1.vb` | night ledger creation | stored procedure `Unesinocenja` | TARGETED_READ |
| `OdjavaSobe` | `Data.vb` | checkout/payment flow | updates `nocenja`, `relgostsoba`, `posjetaFolio`, `troskovi` | TARGETED_READ |
| `PrljavaSoba` | `Data.vb` / called from `frmPlacanje.vb` | successful checkout | updates `sobe.clean = 0` | TARGETED_READ |
| `placanje` | `frmPlacanje.vb` | user completes payment | payment detail, invoice snapshot, optional checkout | TARGETED_READ |

## Open Inventory Work

- Extract all functions/events from 259 `.vb` files into a complete table.
- Separate report-only classes from business-mutating forms.
- Find callers for every P0 function before marking a rule complete.
- Classify all SQL writes from VB and stored procedures.

