# Golden Scenarios — Step-by-Step Business Flows with Exact Evidence

> This document defines 7 P0 business flows as step-by-step scenarios, with exact source references (file:line or LEGACY_ANALYSIS/XX_NAME.md:section) for every status transition, database write, and business rule.

---

## Golden Scenario 1: Check-in (Prijava)

### 1.1 Happy Path: Direct Check-in (No Reservation)

**Preconditions:** Room is free (fnSobaStatus=0), clean (clean=1), not OOO (ooo=0). At least one guest exists in `gosti` table. Worker is logged in.

| Step | Action | Status Transitions | DB Writes | Evidence |
|------|--------|-------------------|-----------|----------|
| 1 | User opens frmPrijava1 | — | — | `10_CHECKIN.md:1.1` |
| 2 | Form loads: calls `vratiTrenutnoSlobodne`, `vratiTrenutnoRezervisane`, `vratiTrenutnoZauzete` SPs | — | SELECT only | `10_CHECKIN.md:1.1`, `02_MODULEKOD_FUNCTIONS.md:1136-1141` |
| 3 | User selects room type → `cmbTipSobe_SelectedIndexChanged` → loads rooms of that type (ooo=0 filter) | — | — | `10_CHECKIN.md:1.2`, `frmPrijava1.vb:105` |
| 4 | User selects room → `cmbNazivSobe_SelectedIndexChanged` → loads already-checked-in guests, sets key/card icon (sobe.idkon: 0=key, 1=card) | — | — | `10_CHECKIN.md:1.3` |
| 5 | User clicks "Dodaj gosta" → opens frmPrijavaGostiUnos | — | — | `10_CHECKIN.md:1.4` |
| 6 | frmPrijavaGostiUnos_Load: loads document types (getDokumenti SP), guest statuses (goststatus WHERE del=0), citizenship (drzave), tariffs | — | SELECT only | `10_CHECKIN.md:1.5` |
| 7 | User searches guest by surname → `trazi()`LIKE search on `gosti` table | — | — | `10_CHECKIN.md:1.5`, `frmPrijavaGostiUnos.vb:~860` |
| 8 | User enters birth date → auto-calculation: age<12→status=4 (child), age<18→status=3 (minor), age>=18→status=1 (adult) | `relgostsoba.status` determined | — | `10_CHECKIN.md:4.5`, `frmPrijavaGostiUnos.vb:~895` |
| 9 | If `setings.dijecagod > 0` and guest age < dijecagod → auto-applies discount from `setings.dijecapop` | `relgostsoba.popust` set | — | `10_CHECKIN.md:5.10` |
| 10 | User saves guest → `promijeni()` SP (existing) or `unesi()` INSERT (new guest) | — | INSERT/UPDATE `gosti` | `10_CHECKIN.md:1.5`, `frmPrijavaGostiUnos.vb:~790` |
| 11 | Callback `dodajGosta()` → adds row to dtPrijavljeniGosti, adds gostID→popust to hTable | — | — | `10_CHECKIN.md:1.6` |
| 12 | User clicks "Prijava" button → validations: room type selected, room selected, date check, at least 1 guest, at least 1 NEW guest in hTable | — | — | `10_CHECKIN.md:1.8`, lines 365-386 |
| 13 | **Check/create folio**: `provjeriPID(roomID)` → if PID=0, `dodajFolio()` creates `posjetaFolio(SID, vrijemeD=Now, vrijemeO=NULL, zakljucen=0)` | `posjetaFolio.zakljucen=0` | INSERT `posjetaFolio` | `10_CHECKIN.md:5.4`, `frmPrijava1.vb:719-723` |
| 14 | **Per-guest INSERT**: For each new guest in hTable, SP `addRelGostSoba` inserts: `relgostsoba(gostID, sobaID, checkInDate=Now, checkInRadnik, checkOutDate, checkOutRadnik, stampanaPrijava=0, odjavljen=0, rezervacija=0, rezervP=0, grupaID, tarifaID, PID, popust, usl, taksa, status)` | `relgostsoba.odjavljen=0`, `relgostsoba.rezervacija=0`, `relgostsoba.stampanaPrijava=0` | INSERT `relgostsoba` × N | `10_CHECKIN.md:3.1`, `frmPrijava1.vb:635` |
| 15 | **Night charges**: For each new guest, calls `nocenja()` → `dodajnocenja()` → SP `Unesinocenja` which DELETES existing same-month nights then INSERTs: `nocenja(RID, DatumP=Now, Tarifa, SID, PID, PrijavaOdjava=0, opis, popust, soba)` | `nocenja.PrijavaOdjava=0` | DELETE+INSERT `nocenja` × N | `10_CHECKIN.md:3.1`, `ModuleKod.vb:1081` |
| 16 | Tariff calculation: `setings.naplposo=0` → split per person (tariff/guestCount); `setings.naplposo=1` → full tariff per person. If calculated price=0 → minimum charge = `setings.taxa + setings.osig` | — | — | `10_CHECKIN.md:5.5-5.6` |
| 17 | If room has card controller (idkon=1) AND kardtip=1 → offer card encoding via frmKardPro | — | — | `10_CHECKIN.md:1.8`, lines 402-420 |
| 18 | **Mark room clean**: SP `updateSobaClean(naziv, clean=1)` | `sobe.clean=1` | UPDATE `sobe` | `10_CHECKIN.md:3.2`, `frmPrijava1.vb:504` |
| 19 | Success message, reset form; `frmGlavni.forma=0` | — | — | `10_CHECKIN.md:1.8` |

**Resulting State**: Room status changes from SLOBODNA (0) → ZAUZETA (1). Guests are checked in with `odjavljen=0`. Nights created with `PrijavaOdjava=0`. Folio open with `zakljucen=0`. Room marked `clean=1`.

### 1.2 Variant: Check-in from Reservation (Rezervacija Prebaci)

**Preconditions:** Reservation exists with `prijava=0`, `stornirana='0'`. Room assigned in `rezervacijasobe`.

| Step | Action | Status Transitions | DB Writes | Evidence |
|------|--------|-------------------|-----------|----------|
| 1 | User opens frmRezervacijePrebaci | — | — | `13_RESERVATIONS.md:1.4` |
| 2 | Loads available rooms (`getVratiZauzete` SP) and reservation data | — | SELECT only | `13_RESERVATIONS.md:1.4` |
| 3 | Mode selection: by rooms (rbtSobe) or by guests (rbtGosti) | — | — | `13_RESERVATIONS.md:1.4` |
| 4 | **Per room/guest**: `PrijaviGosta()` INSERTs new `gosti` record (clone of reservation guest) | — | INSERT `gosti` | `13_RESERVATIONS.md:2.2`, `frmRezervacijePrebaci.vb:593-641` |
| 5 | **Per room**: `VratiPid()` checks for open folio; if none, INSERTs `posjetaFolio(SID, vrijemeD=Now)` | `posjetaFolio.zakljucen=0` | INSERT `posjetaFolio` | `13_RESERVATIONS.md:2.2`, `frmRezervacijePrebaci.vb:562-591` |
| 6 | **Per guest-room**: INSERT `relGostSoba(gostID, sobaID, checkInDate=Now, checkOutDate, checkInRadnik, grupaID, tarifaID, PID, rezervP=1)` | `relgostsoba.rezervP=1` (marks as from reservation) | INSERT `relGostSoba` | `13_RESERVATIONS.md:2.2`, `frmRezervacijePrebaci.vb:644-682` |
| 7 | **Night charges**: SP `Unesinocenja` for each guest | `nocenja.PrijavaOdjava=0` | DELETE+INSERT `nocenja` | `13_RESERVATIONS.md:2.2`, `frmRezervacijePrebaci.vb:791-821` |
| 8 | **Mapping record**: INSERT `rezervacijaPrijava(IDrez, IDGost, sobaID)` | — | INSERT `rezervacijaPrijava` | `13_RESERVATIONS.md:2.2`, `frmRezervacijePrebaci.vb:955-981` |
| 9 | If all rooms checked in: UPDATE `rezervacije SET prijava=1` WHERE ID | `rezervacije.prijava: 0→1` | UPDATE `rezervacije` | `13_RESERVATIONS.md:1.4`, `frmRezervacijePrebaci.vb:689` |
| 10 | If partial: UPDATE `rezervacije SET brojRezSoba={remaining}` WHERE ID | `rezervacije.brojRezSoba decreased` | UPDATE `rezervacije` | `13_RESERVATIONS.md:1.4`, `frmRezervacijePrebaci.vb:692` |
| 11 | UPDATE `gosti SET Rid=0` WHERE ID=checked_in_guest | `gosti.Rid=0` (clears reservation link) | UPDATE `gosti` | `13_RESERVATIONS.md:2.3`, `frmRezervacijePrebaci.vb:760` |

**Resulting State**: fnSobaStatus changes from 3 or 6 → 1 or 4 (depending on reservation confirmation). Reservation marked `prijava=1`.

### 1.3 Known Issues in Check-in

| Issue | Evidence | Impact |
|-------|----------|--------|
| No transaction wrapping for 3-5 DB operations | `10_CHECKIN.md:8.2.1` | Partial check-in can leave inconsistent data |
| Race condition on folio creation (provjeriPID → addFolio) | `10_CHECKIN.md:8.2.3` | Concurrent check-ins could create duplicate folios |
| Unesinocenja DELETE+INSERT destroys manual edits | `10_CHECKIN.md:3.3`, `ModuleKod.vb:1081` | Same-month night charge edits lost on re-check-in |
| br_gost variable never incremented (always 0) | `10_CHECKIN.md:6.2` | Night charge loop may process all guests, not just new ones |
| Check-in from reservation clones guest record instead of referencing | `13_RESERVATIONS.md:7.1.4` | Data inconsistency — duplicate guest records |

---

## Golden Scenario 2: Room Status Change

### 2.1 Room Status Computation (fnSobaStatus)

**Source**: `12_ROOM_STATUS.md:1.1`, `02_MODULEKOD_FUNCTIONS.md:1177`

fnSobaStatus is a MySQL stored function taking (SoID, datumP, datumK, tod) that returns:

| Condition | Value | Display |
|-----------|-------|---------|
| sobe.ooo=1 | 5 | VAN UPOTREBE |
| Active guests (odjavljen=0 AND rezervacija=0) | 1 | ZAUZETA |
| Active guests with checkout today/past | 2 | ZAUZETA (departing) |
| Confirmed reservation (rezervP=1) AND no active guests | 3 | REZERVISANA - potvrdjeno |
| Confirmed reservation (rezervP=1) AND active guests | 4 | ZAUZETA i REZERVISANA |
| Unconfirmed reservation (rezervP=0) AND no active guests | 6 | REZERVISANA - nepotvrdjeno |
| None of the above | 0 | SLOBODNA |

**Clean Override**: After fnSobaStatus computes base status, `clean=0` overrides ALL statuses to display as 7 (NIJE SPREMNA, gray). Evidence: `12_ROOM_STATUS.md:4.1`, `frmSobe.vb:280-284`.

### 2.2 Status Transition Map

```
                    ┌──────────────┐
                    │   SLOBODNA   │
                    │   (Tag=0)    │
                    └──────┬───────┘
                           │ Check-in (addRelGostSoba, odjavljen=0)
              ┌────────────▼────────────┐
              │       ZAUZETA           │
              │       (Tag=1)           │
              └────┬──────────────┬────┘
                   │              │
        Departure  │              │ checkOutDate <= today
        date >     │              │
        today      │              ▼
                   ▼        ZAUZETA (departing)
                   │        (Tag=2)

  Any state ──OOO checkbox──► VAN UPOTREBE (Tag=5)
  Any state ──clean=0───────► NIJE SPREMNA (Tag=7)

  SLOBODNA──Reservation confirmed──► REZERVISANA potvrdjeno (Tag=3)
  SLOBODNA──Reservation unconfirmed─► REZERVISANA nepotvrdjeno (Tag=6)
  REZERVISANA + Check-in──────────► ZAUZETA i REZERVISANA (Tag=4)
```

### 2.3 Room Transfer (promijenaSobe)

**Source**: `12_ROOM_STATUS.md:5.2`, `frmSobaInfo.vb:1392`

**Preconditions**: Room has at least 1 guest. Target room exists. Worker has permissions.

**Mode 1: Transfer ALL guests (rbt1.Checked)**

| Step | Action | DB Writes | Evidence |
|------|--------|-----------|----------|
| 1 | User selects target room in frmSobaInfo | — | `12_ROOM_STATUS.md:5.2` |
| 2 | If target is occupied (`zauzece=True`): get target room's PID (`zauzetPID`) | SELECT | `12_ROOM_STATUS.md:5.2` |
| 3 | For each guest: `samoSobeZauzeto()` UPDATE `relgostsoba SET sobaID=@novibroj, PID=@PID` | UPDATE `relgostsoba` | `frmSobaInfo.vb:1017` |
| 4 | `sviGosti()` — UPDATE all night records to new SID/PID | UPDATE `nocenja` | `12_ROOM_STATUS.md:3.1`, `frmSobaInfo.vb:671` |
| 5 | `zakljuciFolio()` — UPDATE `posjetaFolio SET zakljucen=1` WHERE ID=old_PID | `posjetaFolio.zakljucen: 0→1` | UPDATE `posjetaFolio` | `12_ROOM_STATUS.md:3.2`, `frmSobaInfo.vb:1923` |
| 6 | If target empty: `novaSobaFolio()` — INSERT new `posjetaFolio` | — | INSERT `posjetaFolio` | `frmSobaInfo.vb:1803` |
| 7 | `samoTroskovi()` — UPDATE `troskovi SET SID=@novibroj WHERE SID=@stariSID AND zaklj=0` | UPDATE `troskovi` | `frmSobaInfo.vb:1070` |
| 8 | SP `PromjenaFolio` — UPDATE `posjetaFolio SET SID=@noviSID WHERE PID` | UPDATE `posjetaFolio` | `frmSobaInfo.vb:1507` |
| 9 | SP `Unesinocenja` — recreate night charges | DELETE+INSERT `nocenja` | `frmSobaInfo.vb:802` |
| 10 | `updateClean1()` — UPDATE `sobe SET clean=0` WHERE old room | `sobe.clean: 1→0` | UPDATE `sobe` | `12_ROOM_STATUS.md:5.3`, `frmSobaInfo.vb:209-258` |

**Known Issue**: No transaction wrapping — if mid-transfer fails, data is left inconsistent. `12_ROOM_STATUS.md:7.1.2`

### 2.4 OOO Toggle

| Step | Action | DB Write | Evidence |
|------|--------|-----------|----------|
| 1 | User checks/unchecks `chkVanUpotrebe` in frmSobaInfo | — | `12_ROOM_STATUS.md:5.3` |
| 2 | SP `updateSobaOOO` — UPDATE `sobe SET ooo={0|1}, razlog=@razlog WHERE naziv=@naziv` | `sobe.ooo: 0↔1` | `12_ROOM_STATUS.md:3.1`, `frmSobaInfo.vb:180` |
| 3 | fnSobaStatus recomputes: ooo=1→status 5; ooo=0→normal status | Computed | `12_ROOM_STATUS.md:1.1` |

### 2.5 Clean Toggle

| Step | Action | DB Write | Evidence |
|------|--------|-----------|----------|
| 1 | User checks/unchecks `chkClean` in frmSobaInfo | — | `12_ROOM_STATUS.md:5.3` |
| 2 | SP `updateSobaClean` — UPDATE `sobe SET clean={0|1} WHERE naziv=@naziv` | `sobe.clean: 0↔1` | `12_ROOM_STATUS.md:3.1`, `frmSobaInfo.vb:231,282` |
| 3 | UI override: clean=0 → Tag=7 (NIJE SPREMNA) regardless of fnSobaStatus | Computed | `12_ROOM_STATUS.md:4.1`, `frmSobe.vb:280-284` |

---

## Golden Scenario 3: Reservation → Check-in

### 3.1 Create Reservation

**Source**: `13_RESERVATIONS.md:1.1`

Three creation paths exist: Calendar quick-create, Dedicated entry form, Simplified form.

**Calendar Quick-Create (frmRezervacije)**:

| Step | Action | DB Writes | Evidence |
|------|--------|-----------|----------|
| 1 | User double-clicks empty cell on reservation grid | — | `13_RESERVATIONS.md:1.1, Path A` |
| 2 | INSERT `rezervacije(GID=0, tipID=1, izvorID=0, sobaVrstaID=0, potvrda=0, prijava=0, stornirana='0', brojRezSoba=1, brosoba=1, brdjeca=0)` | INSERT `rezervacije` | `frmRezervacije.vb:942` |
| 3 | INSERT `rezervacijasobe(rezid, sobtid, sobatip, sid, soba, tarifa, ...)` per selected cell | INSERT `rezervacijasobe` | `frmRezervacije.vb:967` |

**Entry Form (frmRezervacije_unos)**:

| Step | Action | DB Writes | Evidence |
|------|--------|-----------|----------|
| 1 | User fills: guest (GID), dates, room type, rooms, tariffs, contact, memo | — | `13_RESERVATIONS.md:1.1, Path B` |
| 2 | INSERT `rezervacije` with all form fields (returns @@Identity) | INSERT `rezervacije` | `frmRezervacije_unos.vb:338` |
| 3 | If alarm checkbox: INSERT `alarm` | INSERT `alarm` | `frmRezervacije_unos.vb:320` |
| 4 | INSERT `rezervacijasobe` per grid row | INSERT `rezervacijasobe` | `frmRezervacije_unos.vb:357` |

### 3.2 Confirm Reservation (Potvrda)

| Step | Action | Status Transition | DB Writes | Evidence |
|------|--------|-------------------|-----------|----------|
| 1 | User clicks "Potvrdi" in frmRezervacijePregled or checks chkpotvrd in frmRezervacije_unos | — | — | `13_RESERVATIONS.md:1.2` |
| 2 | Calculate brojPotvrde = MAX(brojPotvrde)+1 | — | SELECT `rezervacije` | `13_RESERVATIONS.md:4.4`, `frmRezervacijePregled.vb:422` |
| 3 | UPDATE `rezervacije SET potvrda=1, brojPotvrde={next}, datepotvrda=Now()` | `rezervacije.potvrda: 0→1`, `rezervacije.brojPotvrde={next}` | UPDATE `rezervacije` | `13_RESERVATIONS.md:1.2`, `frmRezervacije_unos.vb:828` |

**Race Condition**: brojPotvrde uses MAX()+1 which is not atomic. `13_RESERVATIONS.md:4.4`

### 3.3 Cancel Reservation (Storno)

| Step | Action | Status Transition | DB Writes | Evidence |
|------|--------|-------------------|-----------|----------|
| 1 | User clicks "Storniraj" or checks chkstorno | — | — | `13_RESERVATIONS.md:1.3` |
| 2 | Calculate brojStorna = MAX(brojStorna)+1 | — | SELECT `rezervacije` | `13_RESERVATIONS.md:4.5`, `frmRezervacijePregled.vb:323` |
| 3a | If storno: UPDATE `rezervacije SET stornirana=1, brojStorna={next}, datestorno=Now(), razlogst={reason}` | `rezervacije.stornirana: '0'→'1'` | UPDATE `rezervacije` | `13_RESERVATIONS.md:2.3`, `frmRezervacije_unos.vb:800` |
| 3b | If reactivate: UPDATE `rezervacije SET stornirana=0, razlogst=''` | `rezervacije.stornirana: '1'→'0'` | UPDATE `rezervacije` | `frmRezervacije_unos.vb:811` |

### 3.4 Auto-Expire Reservation

| Step | Action | Status Transition | DB Writes | Evidence |
|------|--------|-------------------|-----------|----------|
| 1 | On frmRezervacije form load | — | — | `13_RESERVATIONS.md:1.6` |
| 2 | UPDATE `rezervacije SET prijava=2 WHERE prijava=0 AND checkInDate < yesterday` | `rezervacije.prijava: 0→2` | UPDATE `rezervacije` | `frmRezervacije.vb:29` |

**Issue**: Runs per-client, not as server-side job. Race condition possible. `13_RESERVATIONS.md:7.1.6`

### 3.5 Transfer Reservation to Check-in (Prebaci)

See Golden Scenario 1.2 above for full step-by-step.

---

## Golden Scenario 4: Payment

### 4.1 Add Expense (troskovi)

**Source**: `15_EXPENSES_NIGHTS.md:1.1`

| Step | Action | DB Writes | Evidence |
|------|--------|-----------|----------|
| 1 | User opens frmTroskovi from room info, reservation, or payment screen | — | `15_EXPENSES_NIGHTS.md:1.1` |
| 2 | Form loads: SP `getGosti` for guest dropdown; SELECT `troskovivrste WHERE del=0 AND ID<>1` (excludes accommodation) | SELECT only | `frmTroskovi.vb:87` |
| 3 | User selects guest → auto-selects associated room | — | `15_EXPENSES_NIGHTS.md:1.1` |
| 4 | User clicks expense type in grid → sets TID and name (special: TID=3=restaurant warning, TID=5=mini-bar/POS) | — | `frmTroskovi.vb:330-346` |
| 5 | If TID=5: form expands, loads mini-bar items from KASA database | — | `frmTroskovi.vb:332-346` |
| 6 | User enters quantity and price → auto-calculates total via `pror()` or `prorM()` | — | `15_EXPENSES_NIGHTS.md:1.1` |
| 7 | Validate: room selected, amount > -200 | — | `frmTroskovi.vb:399-404` |
| 8 | If TID=5: `snimi_mini()` writes to KASA tables `zbirni`, `kasa`, `kasa_detalji` | INSERT KASA tables | `frmTroskovi.vb:367-398` |
| 9 | SP `addTroskovi(GSID, SID, TID, vrijeme=Now, kolicina, iznos, radnikID=1, iddzid)` | `troskovi.zaklj=0` (open) | INSERT `troskovi` | `15_EXPENSES_NIGHTS.md:3.1`, `ModuleKod.vb:903` |
| 10 | `funkcije.logs()` audit log | — | INSERT | `frmTroskovi.vb:414` |

**Known Issue**: radnikID hardcoded to 1. `15_EXPENSES_NIGHTS.md:7.1.2`

### 4.2 Add Night Charge Modification (frmTroskoviNoc)

| Step | Action | DB Writes | Evidence |
|------|--------|-----------|----------|
| 1 | User opens frmTroskoviNoc from payment screen "UmanjiNocenje" menu | — | `15_EXPENSES_NIGHTS.md:1.6`, `frmPlacanje.vb:6605-6614` |
| 2 | Same UI as frmTroskovi but TID hardcoded to 1 (accommodation) | — | `frmTroskoviNoc.vb:34` |
| 3 | SP `addTroskovi` with TID=1, iddzid="" | `troskovi.zaklj=1` (closed), `troskovi.Brrac=receipt#` | INSERT `troskovi` | `15_EXPENSES_NIGHTS.md:3.1` |

### 4.3 Record Payment (frmPlacanje)

**Source**: `14_PAYMENT.md:1.1`

| Step | Action | DB Writes | Evidence |
|------|--------|-----------|----------|
| 1 | User opens frmPlacanje → loads active guests, expense types, currencies, groups | SELECT only | `14_PAYMENT.md:1.1` |
| 2 | Select guest → load expenses (troskovi JOIN troskovivrste WHERE zaklj=0), insurance/tax data, folio | SELECT only | `frmPlacanje.vb:308` |
| 3 | Calculate totals: accommodation costs, insurance, tourist tax, discounts | — | `14_PAYMENT.md:1.1` |
| 4 | User clicks "Pay" → generate receipt number: SELECT MAX(placanje.broj)+1 | — | SELECT `placanje` | `14_PAYMENT.md:5.1`, `frmPlacanje.vb:1262` |
| 5 | Calculate PDV/VAT from `setings.pdv` | — | — | `14_PAYMENT.md:5.4` |
| 6 | Begin transaction (for some paths) | — | — | `14_PAYMENT.md:3.1` |
| 7 | INSERT `placanje` (receipt header) with 40+ columns | INSERT `placanje` | `frmPlacanje.vb:4027` |
| 8 | INSERT `placanjedetalji` per expense line | INSERT `placanjedetalji` | `frmPlacanje.vb:3819,3825` |
| 9 | UPDATE `troskovi SET zaklj=1, Brrac={receipt#}` per expense | `troskovi.zaklj: 0→1`, `troskovi.Brrac=receipt#` | UPDATE `troskovi` | `14_PAYMENT.md:5.2`, `frmPlacanje.vb:3808` |
| 10 | For partial payments: UPDATE `troskovi SET Djelimicno=1, iznos=@remaining` | `troskovi.Djelimicno=1` | UPDATE `troskovi` | `14_PAYMENT.md:5.2`, `frmPlacanje.vb:3812` |
| 11 | INSERT `placanjedetalji` with accommodation line (art=1) | INSERT `placanjedetalji` | `frmPlacanje.vb:3819` |
| 12 | Fiscal printing (based on `setings.fiscal`) — one of 6 device integrations | — | — | `14_PAYMENT.md:1.1`, lines 2506-2512 |
| 13 | INSERT `printracuni` (invoice snapshot header) | INSERT `printracuni` | `frmPlacanje.vb:3172` |
| 14 | INSERT `printracunidetalji` (invoice snapshot line items) | INSERT `printracunidetalji` | `frmPlacanje.vb:3268` |
| 15 | INSERT `printracunifooter` (invoice footer) | INSERT `printracunifooter` | `frmPlacanje.vb:3229` |
| 16 | Commit transaction (if started) | — | — | `14_PAYMENT.md:3.1` |

**Known Issue**: Steps 13-16 are NOT always in a transaction. Receipt inserts have no rollback. `14_PAYMENT.md:7.1.5`

### 4.4 Split Payment (frmPlacanjeSlozeno)

| Step | Action | Evidence |
|------|--------|----------|
| 1 | User selects "Slozeno" method → opens frmPlacanjeSlozeno | `14_PAYMENT.md:1.3` |
| 2 | Loads payment methods excluding ID=5 (compound) | `frmPlacanjeSlozeno.vb:30` |
| 3 | User adds payment rows (method + amount) until remainder=0 | `14_PAYMENT.md:1.3`, `frmPlacanjeSlozeno.vb:205-225` |
| 4 | Persist: SP `addPlacanjeSlozeno` per row | `frmPlacanje.vb:4996` |

### 4.5 Advance Payment (Avans)

| Step | Action | DB Writes | Evidence |
|------|--------|-----------|----------|
| 1 | User checks chkAvans, enters amount | — | `14_PAYMENT.md:5.10` |
| 2 | INSERT `printracuniavans(storno=0, ...)` | INSERT `printracuniavans` | `frmPlacanje.vb:4382` |
| 3 | Generate receipt XML and print via Crystal Reports | — | `14_PAYMENT.md:5.10` |

---

## Golden Scenario 5: Expense & Night Charges

### 5.1 Night Charge Creation (Check-in Time)

**Source**: `15_EXPENSES_NIGHTS.md:1.2`, `10_CHECKIN.md:1.8`

| Step | Action | DB Writes | Evidence |
|------|--------|-----------|----------|
| 1 | During check-in, `nocenja()` is called for each new guest | — | `10_CHECKIN.md:1.8`, step 5 |
| 2 | Tariff calculation per guest: `setings.naplposo=0` → tariff/guestCount; `=1` → full tariff | — | `10_CHECKIN.md:5.5` |
| 3 | For each guest: SP `Unesinocenja(RID, DatumP=Now, Tarifa, SID, PID, PrijavaOdjava=0, opis='', Pop, ssoba)` | DELETE `nocenja` (same month, same RID) + INSERT `nocenja` | `15_EXPENSES_NIGHTS.md:3.1`, `ModuleKod.vb:1081` |
| 4 | Result: `nocenja.PrijavaOdjava=0` (active charge) | — | `15_EXPENSES_NIGHTS.md:4.2` |

**Known Bug**: `Unesinocenja` uses DATE_FORMAT `%Y-%d-%m` instead of `%Y-%m-%d`, potentially matching wrong months. `15_EXPENSES_NIGHTS.md:4.5`, `ModuleKod.vb:1081`

### 5.2 Night Charge Modification (Payment Time)

**Source**: `15_EXPENSES_NIGHTS.md:1.2`, `14_PAYMENT.md:1.1`

| Step | Action | DB Writes | Evidence |
|------|--------|-----------|----------|
| 1 | In frmPlacanje, `imalinocenja()` checks for TID=1 lines in memory | — | `frmPlacanje.vb:1061-1075` |
| 2 | `nocenjePromjena(0)` → `dodavanjenocenja()`: adds night line to in-memory grid with tariff from `vratiCijenunocenja()` | — | `frmPlacanje.vb:1093-1163` |
| 3 | Day count: `VratiBrojDana()` — check-in hour <08:00 → subtract 1 day; checkout hour ≥12:00 → add 1 day; minimum 1 night | — | `14_PAYMENT.md:5.12`, `frmPlacanje.vb:1184-1203` |
| 4 | `trosaknocenjaDodavanje()`: populates in-memory stays with per-guest breakdown | — | `frmPlacanje.vb:1121-1163` |

### 5.3 Night Charge Checkout Flow

**Source**: `15_EXPENSES_NIGHTS.md:1.2`, `14_PAYMENT.md:1.1`

| Step | Action | Status Transitions | DB Writes | Evidence |
|------|--------|-------------------|-----------|----------|
| 1 | On checkout, two operations on nocenja: | — | — | `15_EXPENSES_NIGHTS.md:1.2` |
| 2 | UPDATE `nocenja SET PrijavaOdjava=1, datumodj=checkout_datetime, brrac=receipt# WHERE SID=roomID AND PrijavaOdjava=0` | `nocenja.PrijavaOdjava: 0→1` | UPDATE `nocenja` | `frmPlacanje.vb:3568` |
| 3 | INSERT INTO `nocenja(RID, DatumP=checkout_date, SID, PID, PrijavaOdjava=0, Tarifa, popust, opis, soba) SELECT ... FROM nocenja WHERE SID=roomID AND pid=folioID` | New `nocenja.PrijavaOdjava=0` (next-day charge) | INSERT `nocenja` | `frmPlacanje.vb:3569` |

### 5.4 Expense Status Lifecycle

```
    ┌──────────────┐
    │  OPEN (zaklj=0)  │
    │  Created via     │
    │  addTroskovi SP  │
    └──────┬───────┘
           │ Payment processed
           │ (frmPlacanje)
           ▼
    ┌──────────────┐
    │  CLOSED (zaklj=1) │
    │  Brrac=receipt#   │
    └──────┬───────┘
           │ Invoice storno
           │ (TID<>1 only)
           ▼
    ┌──────────────┐
    │  REOPENED (zaklj=0) │
    │  Brrac=null         │
    └──────────────┘
    
    Special: TID=1 (accommodation)
    CLOSED ──storno──► DELETED (physical DELETE from troskovi)
```

Evidence: `15_EXPENSES_NIGHTS.md:4.1`, `16_INVOICE_CHECKOUT.md:4.3`

### 5.5 Night Charge Status Lifecycle

```
    ┌───────────────────┐
    │  ACTIVE (PrijavaOdjava=0) │
    │  Created at check-in       │
    └──────┬────────────────┘
           │ Checkout
           │ (Data.vb:172,175 or frmPlacanje.vb:4256)
           ▼
    ┌───────────────────┐
    │  CLOSED (PrijavaOdjava=1) │
    │  datumodj=checkout_time    │
    │  brrac=receipt#            │
    └───────────────────┘
```

Evidence: `15_EXPENSES_NIGHTS.md:4.2`

### 5.6 Expense Room Transfer

| Step | Action | DB Writes | Evidence |
|------|--------|-----------|----------|
| 1 | When guest transfers room, SP `unesiPojedinacne` moves expenses | UPDATE `troskovi SET SID=noviSID WHERE SID=stariSID AND zaklj=0` | `15_EXPENSES_NIGHTS.md:1.1`, `ModuleKod.vb:1086` |

---

## Golden Scenario 6: Invoice Creation & Checkout

### 6.1 Invoice Creation (Snapshot Creation)

**Source**: `16_INVOICE_CHECKOUT.md:1.1`

When a payment is processed (frmPlacanje), the following snapshot tables are populated:

| Table | Content | When | Transaction? | Evidence |
|-------|---------|------|---------------|----------|
| `printracuni` | Invoice header (BrojRacuna, guest, dates, room, storno=0, payment type) | Payment processing | No for most paths | `16_INVOICE_CHECKOUT.md:3.1`, `frmPlacanje.vb:3172` |
| `printracunidetalji` | Invoice line items (charge, qty, PDV, amount, method, discount) | Payment processing | No for most paths | `frmPlacanje.vb:3268` |
| `printracunifooter` | Invoice footer (advance, nights, notes) | Payment processing | No | `frmPlacanje.vb:3229` |
| `printracspec` | Invoice spec (guest, room, tariff) | Payment processing | No | `frmPlacanje.vb:2551` |
| `placanje` | Payment record (broj, PID, amount, date, method) | Payment processing | Yes (some paths) | `frmPlacanje.vb:4027` |
| `placanjeDetalji` | Payment detail lines | Payment processing | Yes (some paths) | `frmPlacanje.vb:3819,3825` |
| `placanjeSlozeno` | Split payment breakdown | Split payment | No (SP call) | `frmPlacanje.vb:4996` |

### 6.2 Checkout (Odjava) — Full Room

**Source**: `16_INVOICE_CHECKOUT.md:1.2`, `Data.vb:142-208`

| Step | Action | Status Transitions | DB Writes | Evidence |
|------|--------|-------------------|-----------|----------|
| 1 | User opens frmOdjava1 → SPs `getOdjavaCombo`, `getNeodjavljeneSobe1`, `vratiGostSoba` | — | SELECT only | `16_INVOICE_CHECKOUT.md:1.2` |
| 2 | Calculate totals: `Cijnocenja()` (night charges), `vratiUplatunocenja(PID)` (paid nights), `vratiTrosakSoba(SID)` (expenses), `izlistajPlacanja()` (payments) | — | SELECT only | `16_INVOICE_CHECKOUT.md:1.2` |
| 3 | Calculate: remaining nights = total - paid; total = remaining + expenses + paid | — | — | `16_INVOICE_CHECKOUT.md:1.2` |
| 4 | User clicks "Odjavi Sobe" → confirm warning if unpaid balance | — | — | `16_INVOICE_CHECKOUT.md:1.2` |
| 5 | **Begin transaction** (Data.vb) | — | BEGIN TRANSACTION | `Data.vb:142`, `16_INVOICE_CHECKOUT.md:1.2` |
| 6 | UPDATE `nocenja SET PrijavaOdjava=1, datumodj=checkout WHERE SID=@SID AND PID=@PID` | `nocenja.PrijavaOdjava: 0→1` | UPDATE `nocenja` | `Data.vb:172` |
| 7 | UPDATE `relgostsoba SET odjavljen=1, checkOutDate=@date, checkOutRadnik=@radnik, pl={pl} WHERE sobaID=@SID AND odjavljen=0` | `relgostsoba.odjavljen: 0→1` | UPDATE `relgostsoba` | `Data.vb:191` |
| 8 | UPDATE `Avans SET placeno=1 WHERE brSobe=@SID AND placeno=0` | `Avans.placeno: 0→1` | UPDATE `Avans` | `Data.vb:196`, `frmPlacanje.vb:4264` |
| 9 | UPDATE `posjetaFolio SET vrijemeO=@datumO, zakljucen=1 WHERE ID=@PID` | `posjetaFolio.zakljucen: 0→1` | UPDATE `posjetaFolio` | `Data.vb:203` |
| 10 | UPDATE `troskovi SET zaklj=1 WHERE SID=@SID AND zaklj=0` | `troskovi.zaklj: 0→1` | UPDATE `troskovi` | `Data.vb:208` |
| 11 | **Commit transaction** | — | COMMIT | `Data.vb:208` |
| 12 | (Outside transaction) `PrljavaSoba(SID)` → UPDATE `sobe SET clean=0 WHERE ID=@SID` | `sobe.clean: 1→0` (or any→0) | UPDATE `sobe` | `Data.vb:126`, `16_INVOICE_CHECKOUT.md:4.5` |
| 13 | If unpaid: `neplaceno()` chain → INSERT `troskovi(GSID=0, TID=25, zaklj=1)`, INSERT `neplaceni(placeno=0)`, INSERT `neplaceniplacanja` | `neplaceni.placeno=0` | INSERT × 3 | `16_INVOICE_CHECKOUT.md:1.2`, `frmOdjava1.vb:893-1038` |

**Known Issue**: Step 12 (room dirty) is outside the transaction. If it fails, room stays marked as clean. `16_INVOICE_CHECKOUT.md:7.3.4`

### 6.3 Partial Guest Checkout

| Step | Action | Status Transitions | DB Writes | Evidence |
|------|--------|-------------------|-----------|----------|
| 1 | User clicks "Odjavi Gosta" (single guest checkout) | — | — | `16_INVOICE_CHECKOUT.md:1.2` |
| 2 | If only 1 guest remains: error "Soba nemoze ostati bez gostiju" | — | — | `16_INVOICE_CHECKOUT.md:5.4` |
| 3 | Opening price dialog (frmodjG) | — | — | — |
| 4 | `OdjavaSobe(price, 0, RID, SID, PID, date, uplataNocenja)` | — | — | `Data.vb:142` |
| 5 | UPDATE `nocenja SET PrijavaOdjava=1, datumodj=date WHERE SID=@SID AND PID=@PID AND rid=@gid` | Checked-out guest's `nocenja.PrijavaOdjava: 0→1` | UPDATE `nocenja` | `Data.vb:175` |
| 6 | INSERT INTO `nocenja(RID, DatumP=checkOutDate, SID, PID, PrijavaOdjava=0, Tarifa=split_price, popust, opis, soba) SELECT RID, @checkOutDate, SID, PID, 0, @izn, popust, opis, soba FROM nocenja WHERE SID=@SID AND PID=@PID AND rid<>@gid` | New `nocenja.PrijavaOdjava=0` for remaining guests | INSERT `nocenja` | `Data.vb:178` |
| 7 | UPDATE `relgostsoba SET odjavljen=1, checkOutDate, checkOutRadnik, pl WHERE id=@gid AND odjavljen=0` | Only departing guest: `relgostsoba.odjavljen: 0→1` | UPDATE `relgostsoba` | `Data.vb:193` |
| 8 | Folio remains open (no zakljucen change) | `posjetaFolio.zakljucen` stays 0 | — | `Data.vb:203` (only full checkout closes folio) |

### 6.4 Checkout Status Transition Summary

```
relgostsoba.odjavljen: 0 ──checkout──► 1
nocenja.PrijavaOdjava: 0 ──checkout──► 1
posjetaFolio.zakljucen: 0 ──checkout──► 1
troskovi.zaklj: 0 ──checkout──► 1
sobe.clean: * ──checkout──► 0
Avans.placeno: 0 ──checkout──► 1
```

---

## Golden Scenario 7: Invoice Storno

### 7.1 Regular Invoice Storno

**Source**: `16_INVOICE_CHECKOUT.md:4.3`, `frmRacuni.vb:711-750`

Executed within a **MySQL transaction**:

| Step | Action | Status Transitions | DB Writes | Evidence |
|------|--------|-------------------|-----------|----------|
| 1 | User selects invoice in frmRacuni, clicks "Storniraj" | — | — | `16_INVOICE_CHECKOUT.md:4.3` |
| 2 | Begin transaction | — | BEGIN TRANSACTION | `frmRacuni.vb:711` |
| 3 | UPDATE `placanje SET storno=1 WHERE broj=@Rbr` | `placanje.storno: 0→1` | UPDATE `placanje` | `frmRacuni.vb:731` |
| 4 | UPDATE `placanjeDetalji SET storno=1 WHERE brojID=@Rbr` | `placanjeDetalji.storno: 0→1` | UPDATE `placanjeDetalji` | `frmRacuni.vb:734` |
| 5 | UPDATE `printracuni SET storno=1, exp=2, datstor=Now() WHERE BrojRacuna=@Rbr` | `printracuni.storno: 0→1`, `printracuni.exp=2` | UPDATE `printracuni` | `frmRacuni.vb:738` |
| 6 | UPDATE `troskovi SET zaklj=0, Brrac=null WHERE Brrac=@Rbr AND TID<>1` | `troskovi.zaklj: 1→0` (non-accommodation reopened) | UPDATE `troskovi` | `frmRacuni.vb:741` |
| 7 | DELETE FROM `troskovi WHERE Brrac=@Rbr AND TID=1` | — | DELETE `troskovi` (accommodation charges removed) | `frmRacuni.vb:743` |
| 8 | Commit transaction | — | COMMIT | — |
| 9 | If fiscal device (fsc(0)=22): generate NSC storno file | — | — | `16_INVOICE_CHECKOUT.md:4.3` |
| 10 | If fiscal device (fsc(0)=3/7/2): offer fiscal storno receipt | — | — | `16_INVOICE_CHECKOUT.md:4.3` |

**Critical**: Step 7 physically DELETES accommodation expense rows (TID=1). Step 6 only reopens non-accommodation expenses. This means accommodation charges are permanently lost on storno, while other charges can be re-billed. Evidence: `16_INVOICE_CHECKOUT.md:4.3`, `15_EXPENSES_NIGHTS.md:5.3`

**Affected Status Fields**:
```
placanje.storno:      0 ──storno──► 1
placanjeDetalji.storno: 0 ──storno──► 1
printracuni.storno:   0 ──storno──► 1
printracuni.exp:        ──storno──► 2
troskovi.zaklj:       1 ──storno──► 0 (TID<>1 only)
troskovi.Brrac:       # ──storno──► null (TID<>1 only)
troskovi (TID=1):    exist ──storno──► DELETED
```

### 7.2 Advance Invoice Storno

**Source**: `16_INVOICE_CHECKOUT.md:4.3`, `frmRacuni.vb:957`

| Step | Action | DB Writes | Evidence |
|------|--------|-----------|----------|
| 1 | User selects advance invoice, clicks "Storniraj Avans" | — | — |
| 2 | UPDATE `printracuniavans SET storno=1 WHERE BrojRacuna=@Rbr` | UPDATE `printracuniavans` | `frmRacuni.vb:978` |

**Critical Difference**: Advance invoice storno ONLY sets a flag on `printracuniavans`. It does NOT:
- Reverse payments (`placanje.storno` unchanged)
- Reopen expenses (`troskovi.zaklj` unchanged)
- Delete accommodation charges
- Mark nights as open (`nocenja.PrijavaOdjava` unchanged)

Evidence: `30_STATUS_MATRIX.md:5.2`, `16_INVOICE_CHECKOUT.md:4.3`

### 7.3 Storno Status Comparison

| Aspect | Regular Invoice Storno | Advance Invoice Storno |
|--------|----------------------|------------------------|
| `placanje.storno` | Set to 1 | **Unchanged** |
| `placanjeDetalji.storno` | Set to 1 | **Unchanged** |
| `printracuni.storno` | Set to 1 | N/A (different table) |
| `printracuni.exp` | Set to 2 | N/A |
| `printracuniavans.storno` | N/A | Set to 1 |
| `troskovi` (TID<>1) | Reopened (zaklj→0, Brrac→null) | **Unchanged** |
| `troskovi` (TID=1) | Physically deleted | **Unchanged** |
| `nocenja.PrijavaOdjava` | **Unchanged** | **Unchanged** |
| Transaction wrapped | Yes | Yes |
| Fiscal receipt storno | Attempted if device present | N/A |

---

## Appendix A: Cross-Scenario Status Transition Quick Reference

| Entity | Field | Free → Active | Active → Closed | Storno Reversal | Source |
|--------|-------|---------------|-----------------|-----------------|--------|
| Room | fnSobaStatus | 0→1 (check-in) | 1→0 (checkout) | — | `12_ROOM_STATUS.md:1.1` |
| Room | clean | 1→0 (checkout) | 0→1 (housekeeping/check-in) | — | `30_STATUS_MATRIX.md:1.2` |
| Room | ooo | 0→1 (OOO toggle) | 1→0 (OOO toggle) | — | `30_STATUS_MATRIX.md:1.3` |
| Guest/Stay | odjavljen | 0 (check-in) | 0→1 (checkout) | — | `10_CHECKIN.md:4.1` |
| Guest/Stay | status | Auto-assigned at check-in | — | — | `10_CHECKIN.md:4.5` |
| Reservation | prijava | 0 (new) → 1 (checked in) | — | — | `13_RESERVATIONS.md:4.1` |
| Reservation | prijava | 0 (not expired) → 2 (auto-expired) | — | — | `13_RESERVATIONS.md:4.1` |
| Reservation | stornirana | '0'→'1' (cancel) | — | '1'→'0' (reactivate) | `13_RESERVATIONS.md:4.2` |
| Reservation | potvrda | 0→1 (confirm) | — | 1→0 (un-confirm) | `13_RESERVATIONS.md:4.3` |
| Expense | zaklj | 0 (open) → 1 (paid) | — | 1→0 (storno TID<>1) | `15_EXPENSES_NIGHTS.md:4.1` |
| Expense | Djelimicno | 0 → 1 (partial pay) | — | — | `14_PAYMENT.md:4.4` |
| Night | PrijavaOdjava | 0 (active) → 1 (closed) | — | — | `15_EXPENSES_NIGHTS.md:4.2` |
| Folio | zakljucen | 0 (open) → 1 (closed) | — | 1→0 (return to room) | `16_INVOICE_CHECKOUT.md:4.5` |
| Invoice | storno | 0 → 1 | — | — | `16_INVOICE_CHECKOUT.md:4.1` |
| Advance Inv | storno | 0 → 1 | — | — | `16_INVOICE_CHECKOUT.md:4.1` |
| Invoice | exp | — → 2 (storno) | — | — | `16_INVOICE_CHECKOUT.md:4.2` |
| Payment | storno | 0 → 1 | — | — | `14_PAYMENT.md:4.3` |

## Appendix B: Key Business Rules Summary

| Rule | Description | Evidence |
|-------|-------------|----------|
| BR-01 | Night charges are per-guest; tariff split based on `setings.naplposo` | `10_CHECKIN.md:5.5` |
| BR-02 | Minimum charge = `setings.taxa + setings.osig` if calculated price = 0 | `10_CHECKIN.md:5.6` |
| BR-03 | Duplicate guest in same room is prevented | `10_CHECKIN.md:5.2` |
| BR-04 | Only NEW guests (in hTable) can be removed during check-in | `10_CHECKIN.md:5.3` |
| BR-05 | Folio is shared across all guests in a room during same stay | `10_CHECKIN.md:5.4` |
| BR-06 | Check-in date cannot be >20 days before last night charge | `10_CHECKIN.md:5.8` |
| BR-07 | Check-out date must be after check-in date | `10_CHECKIN.md:5.7` |
| BR-08 | Room status clean=0 overrides ALL other status displays | `12_ROOM_STATUS.md:4.1` |
| BR-09 | Room transfer has no transaction — partial failure leaves inconsistent data | `12_ROOM_STATUS.md:7.1.2` |
| BR-10 | Reservation prijava=2 (auto-expire) runs per-client on form load, not server-side | `13_RESERVATIONS.md:7.1.6` |
| BR-11 | brojPotvrde and brojStorna use MAX()+1 — race condition risk | `13_RESERVATIONS.md:4.4-4.5` |
| BR-12 | Reservation check-in clones guest record instead of referencing | `13_RESERVATIONS.md:7.1.4` |
| BR-13 | Receipt number uses MAX()+1 — race condition risk | `14_PAYMENT.md:5.1` |
| BR-14 | Checkout transaction: close nights → checkout guest → close advances → close folio | `14_PAYMENT.md:5.12` |
| BR-15 | Room clean=0 is set OUTSIDE the checkout transaction | `14_PAYMENT.md:5.12`, `16_INVOICE_CHECKOUT.md:7.3.4` |
| BR-16 | TID=1 (accommodation) expenses are DELETED on storno, not reopened | `16_INVOICE_CHECKOUT.md:4.3` |
| BR-17 | TID<>1 expenses are REOPENED on storno (zaklj→0, Brrac→null) | `16_INVOICE_CHECKOUT.md:4.3` |
| BR-18 | Advance invoice storno ONLY sets flag — does NOT reverse payments/expenses | `16_INVOICE_CHECKOUT.md:4.3` |
| BR-19 | Night count calculation: check-in <08:00 → shift -1 day; checkout ≥12:00 → shift +1 day; minimum 1 night | `14_PAYMENT.md:5.12`, `16_INVOICE_CHECKOUT.md:5.5` |
| BR-20 | Insurance = persons × nights × `setings.osig`; tax = persons × nights × `setings.taxa` | `14_PAYMENT.md:5.3` |
| BR-21 | Partial guest checkout splits night records (INSERT new rows with PrijavaOdjava=0 for remaining guests) | `Data.vb:178`, `16_INVOICE_CHECKOUT.md:5.4` |
| BR-22 | Unesinocenja SP deletes same-month nights before inserting — DATE_FORMAT bug swaps day/month | `15_EXPENSES_NIGHTS.md:4.5` |
| BR-23 | Expense type names starting with "osigu" or "bora" cannot be manually created | `15_EXPENSES_NIGHTS.md:4.4`, `frmTroskovi.vb:357-360` |
| BR-24 | Unpaid checkout creates neplaceni records (TID=25 in troskovi, neplaceni, neplaceniplacanja) | `16_INVOICE_CHECKOUT.md:1.2`, `16_INVOICE_CHECKOUT.md:5.4` |
| BR-25 | "Return to room" for unpaid guests reopens folio, moves expenses, adds new nights | `16_INVOICE_CHECKOUT.md:5.7` |