# Cross-Flow Dependency Map — Legacy Hotel System

> This document synthesizes all 19 analysis documents (00–30) into a single cross-flow dependency reference. Every cell references its evidence source.

---

## 1. Shared Tables Matrix

Which business flows read (R) and write (W) each core table. **R** = read-only, **W** = write, **R/W** = both.

| Table | Check-in | Room Status | Reservation | Payment | Expense/Night | Invoice/Checkout | Fiscal | Guest | Settings | Main Menu/Admin |
|-------|----------|-------------|-------------|---------|---------------|------------------|--------|-------|----------|----------------|
| sobe | R | R/W | R | R | — | R | — | — | — | R |
| gosti | W | — | R | — | — | — | — | R/W | — | — |
| relgostsoba | W | R | W | R | — | W | — | R/W | — | — |
| troskovi | — | — | — | W | R/W | R/W | — | — | — | — |
| troskovivrste | R | — | — | R | R/W | — | — | — | — | — |
| nocenja | W | — | — | R/W | W | R/W | — | — | — | — |
| uplate/placanje | — | — | — | W | — | R/W | — | — | — | — |
| placanjeDetalji | — | — | — | W | — | R/W | — | — | — | — |
| placanjeSlozeno | — | — | — | W | — | — | — | — | — | — |
| printracuni | — | — | — | W | — | R/W | R/W | — | — | — |
| printracunidetalji | — | — | — | W | — | R/W | R/W | — | — | — |
| printracunifooter | — | — | — | W | — | R/W | R/W | — | — | — |
| printracspec | — | — | — | — | — | W | — | — | — | — |
| rezervacije | R | — | R/W | — | — | — | — | — | — | — |
| rezervacijasobe | R | — | R/W | — | — | — | — | — | — | — |
| setings | R | — | — | R | R | R | R | R | R/W | R/W |
| firme/partneri | — | — | R | — | — | — | — | R/W | — | — |
| nplac | — | — | — | R | — | — | — | — | — | — |
| sobavrsta | R | R | R | — | — | — | — | — | — | — |
| tarifa | R | — | — | R | — | — | — | — | — | — |
| posjetafolio | W | — | — | R/W | — | W | — | — | — | — |
| racunigod | — | — | — | — | — | R/W | — | — | — | — |
| sobe_oo | — | R/W | — | — | — | — | — | — | — | — |
| alarm | — | — | W | — | — | — | — | — | — | R |
| drzave | R | — | — | — | — | — | — | R | — | — |
| goststatus | R | — | — | — | — | — | — | R | — | — |
| kursna | — | — | — | R | — | — | — | — | — | — |
| komercijalista | — | — | R | — | — | — | — | — | — | — |
| logs | W | — | — | W | — | W | — | W | — | W |
| neplaceni | — | — | — | — | — | W | — | — | — | — |
| neplaceniplacanja | — | — | — | — | — | W | — | — | — | — |
| radnici | — | — | — | — | — | — | — | — | — | R |
| gostiknjiga | — | — | — | — | — | — | — | W | — | — |
| smjene | — | — | — | — | — | — | — | — | — | W |
| predracuni | — | — | — | R | — | — | — | — | — | — |
| predracunidet | — | — | — | R | — | — | — | — | — | — |

---

## 2. Flow-to-Flow Impact Map

### 2.1 Check-in → Other Flows

| Target Flow | Impact | Mechanism | Evidence |
|-------------|--------|-----------|----------|
| Room Status | Room changes from Free/Reserved to Occupied (status 1 or 4) | `relgostsoba` INSERT with `odjavljen=0`; `fnSobaStatus` recomputes; `sobe.clean` set to 1 via `updateSobaClean` SP | `10_CHECKIN.md:504`, `12_ROOM_STATUS.md:30` |
| Reservation | Reservation marked as checked-in (`prijava=1`); room reserved for guest via `rezervacijaPrijava` | `UPDATE rezervacije SET prijava=1`; INSERT into `rezervacijaprijava` | `13_RESERVATIONS.md:78-81` |
| Expense/Night | Night charge (`nocenja`) auto-created via SP `Unesinocenja`; accommodation expense row may be generated | SP `Unesinocenja` DELETEs same-month records then INSERTs new; `troskovi` row TID=1 inserted | `15_EXPENSES_NIGHTS.md:56-63`, `10_CHECKIN.md:522` |
| Payment | New stay creates open folio (`posjetafolio.zakljucen=0`) ready for payments; `relgostsoba.pl=0` (unpaid) | SP `addFolio`; `Data.vb:185-189` | `10_CHECKIN.md:284-289` |
| Guest | New `gosti` row INSERTed if guest doesn't exist; existing guest linked to room in `relgostsoba` | `frmPrijavaGostiUnos` INSERT/UPDATE; `frmPrijava1.dodajGosta` callback | `10_CHECKIN.md:77-79` |
| Settings | `setings.minchi` used to validate check-in time; `setings.dijecagod`/`dijecapop` drive auto-discount and child status | `Data.vb:419`; age calculation in `frmPrijavaGostiUnos` | `01_GLOBALS.md:196-197` |

### 2.2 Room Status → Other Flows

| Target Flow | Impact | Mechanism | Evidence |
|-------------|--------|-----------|----------|
| Check-in | Only rooms with status 0 (Free) or 3 (Reserved confirmed) or 6 (Reserved unconfirmed) can be checked in; OOO rooms (5) blocked | `fnSobaStatus` computed value; `frmPrijava1.vb:105` WHERE `ooo=0` | `12_ROOM_STATUS.md:35-48`, `10_CHECKIN.md:105` |
| Reservation | Available rooms shown in reservation grid via `getSobeShema` SP; reserved rooms (3,4,6) colored distinctly | `frmRezervacije.vb:715-734` | `12_ROOM_STATUS.md:91-97` |
| Payment | Room occupancy drives payment (must have active stay to charge); checkout changes room to dirty (clean=0) | `Data.vb:119-141` (PrljavaSoba); `frmPlacanje.vb:4211` | `14_PAYMENT.md:59`, `Data.vb:126` |
| Expense/Night | `sobe_oo` (room availability override table) affects night charge eligibility | `12_ROOM_STATUS.md:219-260` | `12_ROOM_STATUS.md:219` |

### 2.3 Reservation → Other Flows

| Target Flow | Impact | Mechanism | Evidence |
|-------------|--------|-----------|----------|
| Check-in | Reservation data pre-fills check-in form; `rezervacijasobe` provides room/tariff assignments; `rezervP` flag set to 1 in `relgostsoba` | `frmRezervacijePrebaci` creates `relgostsoba` with `rezervP=1` | `13_RESERVATIONS.md:70-93` |
| Room Status | Confirmed reservation → status 3; unconfirmed → status 6; both reservation+occupancy → status 4 | `fnSobaStatus` reads `rezervacije.potvrda` and `relgostsoba.rezervP` | `02_MODULEKOD_FUNCTIONS.md:340-347` |
| Alarm | New reservation can create alarm record linked to room | `frmRezervacije_unos.vb:320` optionally INSERTs alarm | `13_RESERVATIONS.md:33` |
| Payment | Reservation can record advance payment via `placanje.predracun` flag | `placanje.predracun` column | `00_DATABASE_SCHEMA.md:565` |

### 2.4 Payment → Other Flows

| Target Flow | Impact | Mechanism | Evidence |
|-------------|--------|-----------|----------|
| Invoice/Checkout | Payment triggers invoice creation (`printracuni` + `printracunidetalji` + `printracunifooter`); expenses marked as `zaklj=1`; nights marked as `PrijavaOdjava=1`; folio closed `zakljucen=1` | `frmPlacanje.vb:3172-3268` (invoice), `3825` (details), `3808,3550` (expenses closed) | `14_PAYMENT.md:30-39` |
| Fiscal | Payment dispatches fiscal device command based on `setings.fiscal`; response saved to `printracuni.fisrac/fisvr/fisIZN` and `printracunifooter.nap` | `frmPlacanje.vb:2504-2513`; `ModuleKod.vb:3107-3143` | `17_FISCAL_PROFORMA.md:32-43` |
| Expense/Night | Expenses linked to receipt via `troskovi.Brrac=receipt#`; night charges closed via `nocenja.PrijavaOdjava=1, brrac=receipt#` | `frmPlacanje.vb:3808,3550,3568,4256` | `14_PAYMENT.md:38-39` |
| Room Status | Checkout marks room dirty `sobe.clean=0` via `PrljavaSoba()` | `Data.vb:119-141`; `frmPlacanje.vb:4211` | `Data.vb:126` |
| Guest | Guest checked out via `relgostsoba.odjavljen=1, checkOutDate, checkOutRadnik` | `Data.vb:191-193`; `frmPlacanje.vb:4260` | `16_INVOICE_CHECKOUT.md:55-56` |
| Settings | `setings.pdv/pdvo/pdvtax/osig/taxa` drive tax calculations; `setings.fiscal` drives fiscal device; `setings.racunbr` formats receipt numbers | `14_PAYMENT.md:21-23`; `17_FISCAL_PROFORMA.md:6-11` | `01_GLOBALS.md:192-196` |

### 2.5 Expense/Night → Other Flows

| Target Flow | Impact | Mechanism | Evidence |
|-------------|--------|-----------|----------|
| Payment | Open expenses (`zaklj=0`) listed for payment; total charges calculated before invoicing | `frmPlacanje.vb:308` (WHERE zaklj=0); `14_PAYMENT.md:30-39` | `14_PAYMENT.md:28` |
| Invoice/Checkout | Expenses finalized at checkout: `zaklj=1, Brrac=receipt#`; accommodation expenses (TID=1) may be deleted on storno | `16_INVOICE_CHECKOUT.md:37`; `30_STATUS_MATRIX.md:290-297` | `16_INVOICE_CHECKOUT.md:37` |
| Room Status | Expense for room X exists only if guest is checked into room X (linked via `troskovi.SID` and `troskovi.GSID`) | `troskovi.SID` → `sobe.ID`; `troskovi.GSID` → `relgostsoba.ID` | `00_DATABASE_SCHEMA.md:1538` |
| Guest | When guest moves to different room, expenses transfer via SP `unesiPojedinacne`: `UPDATE troskovi SET SID=noviSID WHERE SID=stariSID AND zaklj=0` | `12_ROOM_STATUS.md:1243-1275` | `15_EXPENSES_NIGHTS.md:49` |

### 2.6 Invoice/Checkout → Other Flows

| Target Flow | Impact | Mechanism | Evidence |
|-------------|--------|-----------|----------|
| Room Status | Checkout → room marked dirty (`sobe.clean=0`); status reverts to Free (0) once guests leave | `Data.vb:126` (PrljavaSoba); `fnSobaStatus` recompute | `16_INVOICE_CHECKOUT.md:56` |
| Guest | `relgostsoba.odjavljen=1, checkOutDate=now, checkOutRadnik=worker`; unpaid balances tracked via `neplaceni` and `neplaceniplacanja` | `Data.vb:191-193`; `16_INVOICE_CHECKOUT.md:58-62` | `16_INVOICE_CHECKOUT.md:55-56` |
| Payment | Invoice storno reopens expenses (`zaklj=0` for TID≠1, deletes TID=1); storno flag set on `placanje`, `placanjeDetalji`, `printracuni`; fiscal storno dispatched if applicable | `16_INVOICE_CHECKOUT.md:37`; `30_STATUS_MATRIX.md:346-353` | `16_INVOICE_CHECKOUT.md:37` |
| Fiscal | Invoice prints to fiscal device; storno sends fiscal storno command; fiscal response stored in `printracuni.fisrac/fisvr/fisIZN` | `ModuleKod.vb:2707-3082`; `17_FISCAL_PROFORMA.md:32-51` | `02_MODULEKOD_FUNCTIONS.md:118-127` |

### 2.7 Fiscal → Other Flows

| Target Flow | Impact | Mechanism | Evidence |
|-------------|--------|-----------|----------|
| Invoice/Checkout | Fiscal response updates `printracuni` and `printracunifooter`; legal note appended: "Po clanu 42 fiskalnog zakona..." | `ModuleKod.vb:3107-3143` (snimFiskal); `02_MODULEKOD_FUNCTIONS.md:388-389` | `02_MODULEKOD_FUNCTIONS.md:126-127` |
| Settings | Fiscal device type parsed from `setings.fiscal` (`*`-delimited string); paths for device files extracted from same string | `17_FISCAL_PROFORMA.md:6-19` | `01_GLOBALS.md:192` |
| Payment | Fiscal dispatch happens at payment completion; if fiscal device fails, receipt still created in DB but without fiscal confirmation | `14_PAYMENT.md:44-51` | `17_FISCAL_PROFORMA.md:32-51` |

### 2.8 Settings → Other Flows

| Target Flow | Impact | Mechanism | Evidence |
|-------------|--------|-----------|----------|
| Check-in | `minchi`/`maxcho` (check-in/out hour thresholds); `dijecagod`/`dijecapop` (child age/discount); `sobekuc` (#-delimited compound fields) | `01_GLOBALS.md:196-197`; `20_GUESTS.md:33` | `Data.vb:340,419` |
| Payment | `pdv`/`pdvo`/`pdvtax`/`pdvtr` (VAT rates); `osig` (insurance); `taxa` (tourist tax); `cijt` (price type); `racunbr` (receipt number format); `fiscal` (device config) | `01_GLOBALS.md:180-196` | `22_PARTNERS_TARIFFS_SETTINGS.md` |
| Room Status | `sobekuc` (apartment codes); `sobegrupa` (room groupings) affect room grid display; `stan` (station ID) | `01_GLOBALS.md:182,198` | `01_GLOBALS.md:182-183` |
| Fiscal | `fiscal` (`*`-delimited) drives device type selection and file paths | `17_FISCAL_PROFORMA.md:6-19` | `01_GLOBALS.md:192` |
| All | `pdv`/`valuta`/`decim` used across all monetary calculations | `01_GLOBALS.md:184-205` | `22_PARTNERS_TARIFFS_SETTINGS.md` |

---

## 3. Global Variable Dependencies

| Global Variable | Declared In | Set By | Read By | Risk |
|-----------------|-------------|--------|---------|------|
| `ds` (DataSet) | `Data.vb:11` | `sobe_load()`, `loadsetg()`, `ucitajNacine()`, `citajfirme()`, `pripremaRacuna()`, multiple forms | All forms via `ds.Tables("sobe")`, `ds.Tables("setings")`, `ds.Tables("nplac")`, etc. | **Central shared state** — any form can read/write any table; no locking; race conditions on concurrent form access |
| `dst` (DataSet) | `Data.vb:2` | `pripremaRacuna()` in Data.vb | `racundo()`, `frmPlacanje`, `frmRacuni` | Working billing dataset; persists across payment operations |
| `ConnStr` | `Data.vb:9` | `punigod()` appends year from `frmGlavni.cmbgodine.Text` | Every database operation | **Year change reconnects to different DB** — if year changes while guests are checked in, active stay data may be in wrong DB |
| `ConnStrKasa` | `Data.vb:10` | `citajpod()` from set.xml | `frmTroskovi` (KASA DB), `ModuleKod.vb` (mysqlReaderK/mysqlExScalarK) | Separate POS/cash register database for mini-bar expenses |
| `RID` | `Data.vb:20`, `funkcije.vb:5` | `frmLogin.vb:145` | All write operations as `radnikID` or `checkInRadnik`/`checkOutRadnik` | **Duplicate declaration** in Data.vb and funkcije.vb — must stay synchronized |
| `RIme` | `Data.vb:21`, `funkcije.vb:6` | `frmLogin.vb:146` | Receipt headers, audit logs (`funkcije.logs()`), reservation records | Same duplicate issue as RID |
| `akcij` / `akcij1` / `akcij2` | `Data.vb:17-19` | `frmRacuni.vb`, `frmPlacanje.vb`, `frmGlavni.vb` | `rptRacunFrm.vb`, `frmTroskovi.vb` | **String-typed inter-form commands** — no type safety, no validation, easy to break by misspelling |
| `txt1`..`txt9` | `Data.vb:16` | Various forms | Multiple forms | Generic string communication channels; `txt1` used for room selection ("ci", "nek"), `txt9` for numeric IDs |
| `digi` | `Data.vb:22` | `frmPartneri.vb:291` | `frmPlacanje` | Partner/company ID passed between partner selection and payment form |
| `izborForme` | `Data.vb:591` | `frmPrijava1.vb` | `frmPrijavaGostiUnos` | Controls guest data entry flow mode |
| `previewRacuna` | `Data.vb:592` | `frmPlacanje.vb`, `frmRacuni.vb` | `rptRacunFrm.vb` | Flag: preview vs print mode for invoices |
| `printajNocenje` | `Data.vb:589` | `frmPlacanje.vb` | `frmReportRacun.vb` | Flag: whether to print accommodation report |
| `PlacanjeSlozRBR` | `Data.vb:12` | `frmPlacanjeSlozeno.vb:64` | `frmPlacanje.vb` | Running counter for compound payment line numbers |
| `Brojnocenja1` / `Brojnocenja2` | `Data.vb:14-15` | `frmIzvjestajiDnevni.vb:253-278` | Daily reports | Night count totals; module-level variables shared across report calculations |
| `RBr` (invoice counter) | `frmRacuni.vb` | `frmRacuni.vb` (incremented per invoice line) | Invoice line numbering | Local to frmRacuni, not shared globally |
| `BrojRac` (receipt number) | `frmPlacanje.vb` | `frmPlacanje.vb:1262` (SELECT MAX(broj)+1) | Invoice/fiscal receipt numbering | **Race condition**: MAX+1 without locking — concurrent users may get same number |
| `godina` | `frmGlavni.cmbgodine` | Year combo box on main form | `punigod()` which rebuilds `ConnStr` | **Critical**: changing year changes which database all operations target |
| `RacunG` | `frmPlacanje.vb` | Invoice-in-progress flag | Controls whether invoice operations are active | Local state; if form closes unexpectedly, flag may be stale |
| `kardTable` | `ModuleKod.vb:7` | `provjeri_kard()` (currently disabled) | `frmGlavni` room status display columns `gost`, `sos`, `vatr` | Currently non-functional (code disabled at lines 59-60) |
| `stanica` / `stanicaK` | `Data.vb:23-24` | set.xml config | `Data.vb:78` (setings WHERE stan=), `frmTroskovi.vb:376-392` | Station number for multi-terminal identification |
| `fiscal` (from setings) | `setings` table | `frmpostavke` settings form | `frmLogin.vb:183`, `ModuleKod.vb:2851-3103`, `frmPlacanje.vb:2504-2513` | Parsed at runtime as `*`-delimited string; misconfiguration causes fiscal device failure |
| `dozvole` | `ModuleKod.vb:12` | Login form (permissions string from `radnici` record) | Multiple forms for UI enable/disable | Access control string; no structured permission model |

---

## 4. Stored Procedure Cross-Reference

| Stored Procedure | Called From (Forms/Modules) | Tables Read | Tables Written | Business Purpose |
|-----------------|---------------------------|-------------|----------------|------------------|
| getSobeShema | `frmSobe.vb:65`, `frmRezervacije.vb` | sobe, sobavrsta, relgostsoba, rezervacije, rezervacijasobe | — (computes fnSobaStatus) | Room status grid for check-in and reservation screens |
| getGosti | `frmTroskovi.vb`, `frmPrijava1.vb` | relgostsoba, gosti, sobe | — | Load current guests for guest/room selection |
| vratiTrenutnoSlobodne | `frmPrijava1.vb` | sobe, relgostsoba (via fnSobaStatus=0) | — | Free rooms for check-in |
| vratiTrenutnoRezervisane | `frmPrijava1.vb` | sobe, relgostsoba, rezervacije (via fnSobaStatus=3,6) | — | Reserved rooms for check-in |
| vratiTrenutnoZauzete | `frmPrijava1.vb` | sobe, relgostsoba (via fnSobaStatus=1,2,4) | — | Occupied rooms for check-in |
| getPrintHeader | `frmRacuni.vb` | printracuni | — | List all invoices |
| getPrintDetalji | `frmRacuni.vb` | printracunidetalji | — | Invoice line items |
| getPrintFooter | `frmRacuni.vb` | printracunifooter | — | Invoice footer/legal notes |
| getPlacanjenocenja | `Data.vb:443-473` | placanje | — | Sum of paid accommodation per folio PID |
| getJedinicnaCijena | `funkcije.vb:181,299` | sobatarifa, relsobavrstasobatarifa, setings | — | Calculate unit price for room/tariff |
| podaciGostiSobe | `funkcije.vb:391` | relgostsoba, gosti, goststatus | — | Guest data for a specific room |
| addSmjenaStart | `funkcije.vb:421` | — | smjene | Record worker shift start |
| getLogs | `funkcije.vb:467` | — | logs | Insert audit log entry |
| getGlavniImena | `funkcije.vb:560` | relgostsoba, gosti, sobe | — | Main screen guest names per room |
| getGlavniPodaci | `funkcije.vb:581` | relgostsoba, gosti, sobe, nocenja | — | Guest/room data by RID |
| vratiRIDnocenja | `funkcije.vb:638` | nocenja | — | Night registration IDs for a room |
| Unesinocenja | `frmRacuni.vb:1414-1437`, `frmRezervacijePrebaci.vb:791-821` | — | nocenja (DELETE + INSERT) | Create accommodation charge (replaces same-month entries) |
| OdjavaSobe | `Data.vb:142-226` | relgostsoba, nocenja, troskovi, posjetafolio | relgostsoba (UPDATE), nocenja (UPDATE), troskovi (UPDATE), posjetafolio (UPDATE) | **Transactional checkout**: marks guest out, closes nights, expenses, folio |
| PrljavaSoba | `Data.vb:119-141` | — | sobe (UPDATE clean=0) | Mark room as dirty after checkout |
| addTroskovi | `frmTroskovi.vb` (via upisiTroskove) | — | troskovi (INSERT) | Add expense line item |
| addPlacanjeSlozeno | `frmPlacanje.vb:4973-5026` | — | placanjeslozeno (INSERT) | Add compound payment line |
| getRezrervacijePrikazi | `frmRezervacije.vb` | rezervacije, rezervacijasobe | — | List active reservations |
| getRezrervacijePrikaziPot | `frmRezervacije.vb` | rezervacije WHERE potvrda=1 | — | List confirmed reservations |
| getRezrervacijePrikaziSto | `frmRezervacije.vb` | rezervacije WHERE stornirana=1 | — | List cancelled reservations |
| fnSobaStatus | `getSobeShema`, `frmSobe`, `frmRezervacije` | sobe, relgostsoba, rezervacije, rezervacijasobe | — (computed function) | Compute room status 0-6 |
| fnBrojGostiju | (referenced in SQL) | relgostsoba | — | Count guests per room |
| fnBrojNocenja | (referenced in reports) | nocenja | — | Count occupied rooms |
| updateSobaClean | `frmPrijava1.vb:504`, `frmSobaInfo.vb:282` | — | sobe (UPDATE clean=1) | Mark room as clean |
| updateSobaOOO | `frmSobaInfo.vb:140-208` | — | sobe (UPDATE ooo, razlog) | Set room out of order |
| provrezstare | `ModuleKod.vb:440-442` | rezervacije | rezervacije (UPDATE prijava=1) | Auto-checkin old reservations |

---

## 5. Conflict Scenarios

### 5.1 Year Change While Guests Checked In

**Problem**: `ConnStr` is rebuilt with year suffix from `frmGlavni.cmbgodine`. If the year changes while guests are checked in, all subsequent DB operations target a different year-database. Active stays, expenses, and payments may be written to the wrong database.

**Affected tables**: relgostsoba, nocenja, troskovi, placanje, printracuni (all year-specific)

**Mitigation in legacy code**: `racunigod` table stores invoice counter per year. The `inportgod()` function imports data between year databases. No runtime guard exists - operator must manually ensure year is correct before operations.

**Evidence**: `01_GLOBALS.md:72-78`, `02_MODULEKOD_FUNCTIONS.md:79-80`

### 5.2 Concurrent Receipt Number Generation

**Problem**: `BrojRac` generated via `SELECT MAX(broj) + 1 FROM placanje` without locking. If two users create receipts simultaneously, they can get the same number.

**Affected tables**: placanje, printracuni, printracunidetalji

**Mitigation**: None in legacy code. MySQL AUTO_INCREMENT on `placanje.ID` prevents PK collision, but `placanje.broj` (the business receipt number) can duplicate.

**Evidence**: `14_PAYMENT.md:35`, `16_INVOICE_CHECKOUT.md:107-109`

### 5.3 Guest A in Room, Guest B Has Reservation for Same Room

**Problem**: If a room is occupied (status 1) by Guest A, and a confirmed reservation (status 3) exists for Guest B, check-in of Guest B produces status 4 (occupied AND reserved). The system allows this overlap, but checkout of Guest A does not automatically update the reservation status for Guest B.

**Flow**: Check-in A → status 1. Reserve for B → status unchanged (still shown as occupied). If B checks in → status 4. If only B's reservation exists (no A), fnSobaStatus returns 3.

**Affected tables**: relgostsoba (multiple records for same sobaID), rezervacije, sobe

**Evidence**: `02_MODULEKOD_FUNCTIONS.md:340-347` (fnSobaStatus logic), `13_RESERVATIONS.md:70-93`

### 5.4 Partial Checkout — What Happens to Remaining Guests

**Problem**: When only some guests in a room check out, `Data.vb:193` updates only that guest's `relgostsoba.odjavljen=1`. The room remains occupied (status 1). Remaining guests' folios and expenses are untouched. However, if the last guest checks out via `btnOdjavaGosta_Click` and it's only one guest, the system blocks with "Soba nemoze ostati bez gostiju."

**Affected tables**: relgostsoba, posjetafolio, troskovi, nocenja

**Evidence**: `16_INVOICE_CHECKOUT.md:53-54`

### 5.5 Invoice Storno Does Not Reverse Fiscal Device

**Problem**: When an invoice is storned (`printracuni.storno=1`), expenses with `TID≠1` are reopened (`zaklj=0`). Accommodation expenses (`TID=1`) are deleted. The fiscal device is sent a storno command file. However, if the fiscal device fails to process the storno, the invoice is still marked as storned in the database — there is no rollback mechanism for the fiscal side.

**Affected tables**: placanje.storno, printracuni.storno, troskovi.zaklj, nocenja

**Evidence**: `16_INVOICE_CHECKOUT.md:37`, `17_FISCAL_PROFORMA.md:32-51`

### 5.6 Night Charge Duplicate on Re-entry (Unesinocenja Bug)

**Problem**: SP `Unesinocenja` DELETEs existing nocenja records for the same RID and same month using `DATE_FORMAT(DatumP, '%Y-%d-%m')`. Since `%d` and `%m` are swapped (should be `%Y-%m-%d`), the month matching is incorrect. This could cause night charges to be deleted for the wrong month or fail to match the intended month.

**Affected tables**: nocenja

**Evidence**: `15_EXPENSES_NIGHTS.md:63`

### 5.7 Global DataSet Race Conditions

**Problem**: `ds` (shared DataSet) is loaded by `sobe_load()`, `loadsetg()`, etc. and read by all forms. Multiple open forms reading/writing to the same DataTable can produce inconsistent views. No synchronization mechanism exists.

**Affected globals**: ds, dst

**Evidence**: `01_GLOBALS.md:270-278`

### 5.8 E-Stranac Registration Number Race Condition

**Problem**: `relgostsoba.estranac` is generated via `MAX(estranac)+1` without locking. Concurrent registrations may produce duplicate numbers.

**Affected tables**: relgostsoba

**Evidence**: `20_GUESTS.md:156`

### 5.9 Advance Invoice Storno Does Not Reverse Payments

**Problem**: Storno of an advance invoice (`printracuniavans.storno=1`) only negates the amounts. It does NOT reverse the linked payment in `placanje` or reopen any expenses. The payment remains valid.

**Affected tables**: printracuniavans, placanje, troskovi

**Evidence**: `02_MODULEKOD_FUNCTIONS.md:498-504`, `30_STATUS_MATRIX.md:416-425`

### 5.10 Reservation Confirmation Number Race Condition

**Problem**: `brojPotvrde` and `brojStorna` generated via `MAX()+1` without locking.

**Affected tables**: rezervacije

**Evidence**: `13_RESERVATIONS.md:48-49`, `30_STATUS_MATRIX.md:213-214`

---

## 6. Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        SETTINGS (setings table)                         │
│  pdv, pdvo, pdvtax, taxa, osig, fiscal, minchi, maxcho, racunbr,      │
│  dijecagod, dijecapop, sobekuc, sobegrupa, valuta, cijt, stan, ...     │
└──────────┬──────────┬──────────┬──────────┬──────────┬─────────────────┘
           │          │          │          │          │
           ▼          ▼          ▼          ▼          ▼
      ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
      │CHECK-IN│ │  ROOM   │ │RESERV- │ │PAYMENT │ │ FISCAL  │
      │        │ │STATUS  │ │ATION   │ │        │ │ DEVICE  │
      └───┬────┘ └────┬───┘ └────┬───┘ └────┬───┘ └────┬───┘
          │           │          │          │          │
          ▼           ▼          ▼          │          │
    ┌──────────┐ ┌──────────┐ ┌──────────┐ │          │
    │  gosti    │ │  sobe    │ │rezervacije│ │          │
    │ (INSERT)  │ │(fnStatus)│ │ (CRUD)    │ │          │
    └─────┬────┘ └────┬─────┘ └─────┬─────┘ │          │
          │           │              │       │          │
          ▼           ▼              ▼       │          │
    ┌──────────────────────────────────────┐  │          │
    │         relgostsoba (CORE LINK)       │  │          │
    │  gostID → gosti.ID                   │  │          │
    │  sobaID → sobe.ID                     │  │          │
    │  PID → posjetafolio.ID                │  │          │
    │  tarifaID → sobatarifa.ID             │  │          │
    │  odjavljen: 0=active, 1=checked out   │  │          │
    │  rezervacija: 0=stay, 1=reservation   │  │          │
    │  rezervP: 0=unconfirmed, 1=confirmed  │  │          │
    └─────────────┬────────────────────┬─────┘  │          │
                  │                    │         │          │
          ┌───────┘              ┌─────┘         │          │
          ▼                      ▼               │          │
    ┌──────────┐          ┌──────────┐          │          │
    │ nocenja  │          │ troskovi  │          │          │
    │(nights)  │          │(expenses) │          │          │
    │ RID→rgs  │          │ GSID→rgs  │          │          │
    │ SID→sobe │          │ SID→sobe  │          │          │
    │ PID→folio│          │ TID→vrste  │          │          │
    └─────┬────┘          └─────┬─────┘          │          │
          │                     │                │          │
          └────────┐  ┌─────────┘                │          │
                   ▼  ▼                          │          │
             ┌──────────┐                        │          │
             │  PAYMENT │◄───────────────────────┘          │
             │ (placanje│───┬──────────────────┐             │
             │  Detalji │   │                  │             │
             │  Slozeno)│   ▼                  ▼             │
             └─────┬────┘ ┌──────────────┐ ┌──────────┐     │
                   │      │printracuni    │ │printracuni│     │
                   │      │printracunidet │ │  footer    │     │
                   │      │printracspec   │ │printracuni│     │
                   │      │(INVOICE       │ │  avans    │     │
                   │      │ SNAPSHOT)     │ │(ADVANCE)  │     │
                   │      └───────┬──────┘ └─────┬────┘     │
                   │              │               │           │
                   │              ▼               │           ▼
                   │      ┌──────────────┐      │    ┌──────────┐
                   │      │ FISCAL DEVICE│◄─────┴────│ STORNO   │
                   │      │ (KTE/NSC/    │           │ COMMAND  │
                   │      │  Tring/Tremol│           │  FILES   │
                   │      │  /Eln/Micro/ │           └──────────┘
                   │      │  HCP)        │
                   │      └──────┬───────┘
                   │             │
                   │             ▼
                   │      ┌──────────────┐
                   │      │ snimFiskal() │
                   │      │ → UPDATE     │
                   │      │  printracuni │
                   │      │  .fisrac/   │
                   │      │  fisvr/     │
                   │      │  fisIZN     │
                   │      │ → UPDATE     │
                   │      │  printracunif│
                   │      │  ooter.nap   │
                   │      └──────────────┘
                   │
      ┌────────────┼────────────────────────────────┐
      │  CHECKOUT  │                                │
      │            ▼                                ▼
      │  ┌──────────────┐  ┌───────────────────────────────┐
      │  │ OdjavaSobe() │  │         neplaceno()           │
      │  │ TRANSACTION: │  │  (unpaid balance tracking)    │
      │  │              │  │  → neplaceni (INSERT)         │
      │  │ 1. UPDATE    │  │  → neplaceniplacanja (INSERT) │
      │  │  nocenja     │  │  → troskovi (INSERT TID=25)   │
      │  │  PrijavaOdj  │  └───────────────────────────────┘
      │  │  =1          │
      │  │ 2. UPDATE    │  ┌───────────────────────────────┐
      │  │  relgostsoba │  │       STORNO PATH             │
      │  │  odjavljen=1 │  │  printracuni.storno=1         │
      │  │  3. UPDATE   │  │  placanje.storno=1            │
      │  │  posjetafolio│  │  placanjeDetalji.storno=1     │
      │  │  zakljucen=1 │  │  troskovi.zaklj=0 (TID≠1)   │
      │  │  4. UPDATE   │  │  troskovi DELETE (TID=1)     │
      │  │  troskovi    │  │  → fiscal storno file         │
      │  │  zaklj=1     │  └───────────────────────────────┘
      │  │  5. UPDATE   │
      │  │  sobe        │
      │  │  clean=0     │
      │  └──────────────┘
      └─────────────────────────────────────────────────────────┘

  KEY GLOBALS AFFECTING ALL FLOWS:
  ┌────────────────────────────────────────────────────────────────┐
  │ ConnStr ── year-dependent database switching                   │
  │ ds ── shared in-memory DataSet (rooms, settings, guests, etc.) │
  │ RID / RIme ── logged-in worker (duplicated in Data & funkcije) │
  │ akcij/akcij1/akcij2 ── string-typed inter-form commands        │
  │ fiscal ── device type & file paths from setings.fiscal          │
  │ godina ── current year selector, rebuilds ConnStr               │
  └────────────────────────────────────────────────────────────────┘
```