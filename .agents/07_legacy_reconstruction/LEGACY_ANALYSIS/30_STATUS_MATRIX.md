# Legacy Status Transition Matrix — Complete Cross-Reference

> This document catalogs EVERY status field, magic value, and flag found across all legacy analysis documents, with exact source references and transition rules.

---

## 1. Room Status

### 1.1 fnSobaStatus (Computed DB Function)

Source: `02_MODULEKOD_FUNCTIONS.md:1177`, `12_ROOM_STATUS.md:19-33`

| Value | Display Text | Condition | Set By | Read By |
|-------|-------------|-----------|--------|---------|
| 0 | SLOBODNA (Free) | No active guests, no reservations | Computed | `frmSobe.vb:250`, `frmRezervacije.vb:715` |
| 1 | ZAUZETA (Occupied) | Active guests with `odjavljen=0 AND rezervacija=0` | Computed | `frmSobe.vb:251`, `frmRezervacije.vb:718` |
| 2 | ZAUZETA (Departing) | Guests with checkout today or past `checkOutDate` | Computed | `frmSobe.vb:254`, `frmRezervacije.vb:718` |
| 3 | REZERVISANA - potvrdjeno | Confirmed reservation (`rezervP=1`), no active guests | Computed | `frmSobe.vb:258`, `frmRezervacije.vb:722` |
| 4 | ZAUZETA i REZERVISANA | Confirmed reservation AND active guests | Computed | `frmSobe.vb:262`, `frmRezervacije.vb:726` |
| 5 | VAN UPOTREBE (OOO) | `sobe.ooo=1` | `frmSobaInfo.vb:140-208` (updateOOO checkbox) | `fnSobaStatus`, `frmSobe.vb:266` |
| 6 | REZERVISANA - nepotvrdjeno | Unconfirmed reservation (`rezervP=0`), no guests | Computed | `frmSobe.vb:270`, `frmRezervacije.vb:734` |

### 1.2 Room Clean Status Override

Source: `12_ROOM_STATUS.md:280-284`

| Value | Meaning | Set By | Read By | Override Rule |
|-------|---------|--------|----------|---------------|
| 0 | Dirty (needs cleaning) | `Data.vb:126` (PrljavaSoba after checkout), `frmPlacanje.vb:4211` (checkout) | `frmSobe.vb:280-284` | **Overrides ALL other status colors** — even occupied rooms appear gray |
| 1 | Clean (default) | `frmPrijava1.vb:504` (after check-in via updateSobaClean SP), `frmSobaInfo.vb:282` (manual clean checkbox) | `fnSobaStatus` (not used in computation) | Clean rooms show their computed status color |

**Transition**: Free → Occupied (`check-in` sets `clean=1` per `10_CHECKIN.md:504`) → Checkout → Dirty (`clean=0` per `Data.vb:126`) → Housekeeping → Clean (`clean=1` per `12_ROOM_STATUS.md:259-309`)

### 1.3 Room OOO (Out of Order)

Source: `12_ROOM_STATUS.md:299-310`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | In service (available) | `frmSobaInfo.vb:180-208` (updateOOO uncheck) | `fnSobaStatus` (not 5), `frmPrijava1.vb:105` (WHERE ooo=0) |
| 1 | Out of order | `frmSobaInfo.vb:140-208` (updateOOO checkbox) | `fnSobaStatus` → returns 5, `frmRezervacije.vb:573,583` (black row) |

**Transition**: Any → OOO (checkbox + `updateSobaOOO` SP) → In service (uncheck + `updateSobaOOO` SP). Requires confirmation if room was previously in service (`12_ROOM_STATUS.md:305-308`).

### 1.4 Room Controller/Key Type (sobe.idkon)

Source: `10_CHECKIN.md:314-318`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Key lock (mechanical) | `frmPrijava1.vb:1353` (toggle), `frmSobaInfo.vb:2292` (toggle) | `frmPrijava1.vb:164` (shows key icon), `frmSobaInfo.vb:2284` PictureBox |
| 1 | Card lock (electronic) | `frmPrijava1.vb:1353` (toggle), `frmSobaInfo.vb:2292` (toggle) | `frmPrijava1.vb:167` (shows card icon, triggers card encoding) |

---

## 2. Guest/Stay Status

### 2.1 relgostsoba.odjavljen (Check-out Status)

Source: `10_CHECKIN.md:240-244`, `14_PAYMENT.md:264-268`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Currently checked in (active stay) | `addRelGostSoba` SP (insert with 0), `frmRezervacijePrebaci.vb:648` (check-in from reservation) | `frmPrijava1.vb:63` (WHERE odjavljen=0), `01_DATABASE_SCHEMA.md:1634` (bzs view) |
| 1 | Checked out | `Data.vb:191` (full room checkout), `Data.vb:193` (single guest checkout), `frmPlacanje.vb:4260` (check-out via payment) | All checkout/daily-balance queries |

**Transition**: 0 → 1 occurs at checkout (never reversed except "return to room" → `frmRacuni.vb` btnVratiuSobu reopens folio with `zakljucen=0`)

### 2.2 relgostsoba.rezervacija (Reservation Link)

Source: `10_CHECKIN.md:247-249`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Actual stay (not reservation) | `frmPrijava1.vb:601` (always inserted as False/0) | `fnSobaStatus` (occupied if rezervacija=0), `bzs` view (WHERE rezervacja=0) |

### 2.3 relgostsoba.rezervP (Reservation Confirmation Link)

Source: `10_CHECKIN.md:252-254`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Not from confirmed reservation | `frmPrijava1.vb:603` (always inserted as False/0) | `fnSobaStatus` (status 6 if rezervP=0 and no guests) |
| 1 | From confirmed reservation | `frmRezervacijePrebaci.vb:648` (checked in from reservation) | `fnSobaStatus` (status 3 or 4 if rezervP=1) |

### 2.4 relgostsoba.stampanaPrijava (Printed Registration)

Source: `10_CHECKIN.md:256-259`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Registration not printed | `addRelGostSoba` SP (default 0) | — |
| 1 | Registration printed | `20_GUESTS.md:1227` (UPDATE SET print1=1) | `20_GUESTS.md` R1 column |

### 2.5 relgostsoba.status (Guest Status / Tax Category)

Source: `10_CHECKIN.md:262-268`

| Value | Meaning | Evidence | Set By |
|-------|---------|----------|--------|
| 0 | Default/unknown | `frmPrijava1.vb:625` | Auto-assigned when no date of birth |
| 1 | Adult (standard rate) | `frmPrijavaGostiUnos.vb:~895` (age ≥ 18) | Auto-assigned from birth date |
| 3 | Minor (under 18) | `frmPrijavaGostiUnos.vb:~895` (age < 18 and ≥ 12) | Auto-assigned from birth date |
| 4 | Child (under 12) | `frmPrijavaGostiUnos.vb:~895` (age < 12) | Auto-assigned from birth date, `frmPrijavaGostiKucice.vb:~710` |

**Linked to**: `goststatus.taksa` (tax amount per status), `setings.dijecagod` and `setings.dijecapop` for auto-discount logic.

### 2.6 relgostsoba.taksa (Tax Override)

Source: `10_CHECKIN.md:269-273`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Default (no tax override) | `frmPrijava1.vb:623` |
| From goststatus | Tax amount from status lookup | `frmPrijavaGostiKucice.vb:~710` |

### 2.7 relgostsoba.popust (Discount %)

Source: `10_CHECKIN.md:275-279`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | No discount | `frmPrijava1.vb:623` (default) |
| setings.dijecapop | Auto-discount for children | `frmPrijavaGostiUnos.vb:~895` |
| Manual entry | User-entered discount with reason | ` frmPrijava1` PopustRazlog field |

### 2.8 relgostsoba.pl (Payment Flag)

Source: `02_MODULEKOD_FUNCTIONS.md:1260-1263`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Unpaid / no accommodation payment | `bazaSql` line 1260 (reset all), `Data.vb:185-189` (set to 1 if uplataNocenja > 0) |
| 1 | Accommodation payment recorded | `bazaSql` line 1262 (SET pl=1 where payment exists), `Data.vb:185-189` |

### 2.9 relgostsoba.print1 / print2 (Print Flags)

Source: `20_GUESTS.md:144-145,356-359`

| Value | Meaning | Set By |
|-------|---------|--------|
| 0 | Registration report not printed | Default |
| 1 | Registration report R1 printed | `20_GUESTS.md:1227` (UPDATE SET print1=1) |
| 0 | Stay report not printed | Default |
| 1 | Stay report R2 printed | `20_GUESTS.md:1258` (UPDATE SET print2=1) |

### 2.10 relgostsoba.estranac (E-Stranac Registration Number)

Source: `20_GUESTS.md:104-106,213-213`

| Value | Meaning | Set By |
|-------|---------|--------|
| 0 | Not registered with e-stranac | Default |
| >0 | E-stranac book registration number | `20_GUESTS.md:2138` (UPDATE SET estranac={stbr}) |

**Race condition**: MAX(estranac)+1 without locking (`20_GUESTS.md:2125`).

### 2.11 relgostsoba.tid (Tourist Registration ID)

Source: `20_GUESTS.md:1821-1826`

| Value | Meaning | Set By |
|-------|---------|--------|
| 0 | Not registered with tourist authority | Default |
| >0 | TZ registration response ID | `20_GUESTS.md:1821-1826` (UPDATE SET tid='{response}') |

---

## 3. Reservation Status

### 3.1 rezervacije.prijava (Check-in Status)

Source: `13_RESERVATIONS.md:312-317`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Not checked in (reserved) | Default on creation (`13_RESERVATIONS.md:237`) | `frmRezervacije.vb:29` (auto-expire), `13_RESERVATIONS.md:314` |
| 1 | Checked in | `frmRezervacijePrebaci.vb:689,726` (all rooms/guests checked in), `frmPrijava1.vb:463` (direct check-in) | `fnSobaStatus` (status 4 if guests present) |
| 2 | Auto-expired | `frmRezervacije.vb:29` (prijava=0 AND checkInDate < yesterday), `02_MODULEKOD_FUNCTIONS.md:441` | `13_RESERVATIONS.md:317` |

**Transition**: 0 → 1 (check-in), 0 → 2 (auto-expire on form load). No reverse transition.

### 3.2 rezervacije.stornirana (Cancellation Status)

Source: `13_RESERVATIONS.md:319-326`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| '0' (string) | Active reservation | Default on creation | `frmRezervacije.vb:122` (WHERE stornirana='0' filter) |
| '1' (string or int) | Cancelled/storno | `frmRezervacijePregled.vb:357` (toggle), `frmRezervacije_unos.vb:800` (storno checkbox) | `getRezrervacijePrikaziSto` SP (WHERE stornirana=1) |

**Transition**: 0 ↔ 1 (bidirectional toggle from both grid and detail form). Date and reason tracked in `datestorno` and `razlogst`.

### 3.3 rezervacije.potvrda (Confirmation Status)

Source: `13_RESERVATIONS.md:328-335`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Not confirmed | Default, `frmRezervacije_unos.vb:836` (un-confirm) | `getRezrervacijePrikaziPot` SP (WHERE potvrda=1) |
| 1 | Confirmed | `frmRezervacijePregled.vb:457` (grid confirm), `frmRezervacije_unos.vb:828` (checkbox confirm) | `fnSobaStatus` (status 3 or 6) |

**Transition**: 0 ↔ 1 (bidirectional toggle). Confirmation number auto-generated via `MAX(brojPotvrde)+1` (race condition risk). Date tracked in `datepotvrda`.

### 3.4 rezervacije.brojPotvrde (Confirmation Number)

Source: `13_RESERVATIONS.md:337-340`

| Value | Meaning | Set By |
|-------|---------|--------|
| 0 | No confirmation number | Default |
| MAX+1 | Auto-generated confirmation number | `frmRezervacijePregled.vb:422-451`, `frmRezervacije_unos.vb:828` |

**Race condition**: `SELECT MAX(brojPotvrde)+1` is not atomic.

### 3.5 rezervacije.brojStorna (Storno Number)

Source: `13_RESERVATIONS.md:343-346`

| Value | Meaning | Set By |
|-------|---------|--------|
| NULL/0 | Not storned | Default |
| MAX+1 | Auto-generated storno number | `frmRezervacijePregled.vb:323-351` |

**Race condition**: `SELECT MAX(brojStorna)+1` is not atomic.

### 3.6 rezervacijegrupe.odjavljena (Group Active)

Source: `10_CHECKIN.md:197`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Active group | `frmRezervacijeNove.vb:279` (INSERT) | `frmPrijava1.vb:194` (WHERE odjavljena=0) |
| 1 | Closed group | (manual update) | Filtered out from group combos |

---

## 4. Folio Status

### 4.1 posjetafolio.zakljucen (Folio Closed)

Source: `10_CHECKIN.md:284-289`, `14_PAYMENT.md:256-261`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Open folio (active stay) | `addFolio` SP (insert with zakljucen=0), `frmRacuni.vb` btnVratiuSobu (reopen folio) | `frmPrijava1.vb:684` (check open folio for room), `01_DATABASE_SCHEMA.md:1646` (folio view WHERE zakljucen=0) |
| 1 | Closed folio (checkout complete) | `Data.vb:203` (checkout), `frmPlacanje.vb:4268` (payment checkout) | — |

**Transition**: 0 → 1 at checkout (within transaction). Exception: 1 → 0 for "return to room" unpaid guest flow (`16_INVOICE_CHECKOUT.md:213`).

### 4.2 posjetafolio.vrijemeO (Checkout Time)

Source: `10_CHECKIN.md:287-289`

| Value | Meaning | Set By |
|-------|---------|--------|
| NULL | Checkout time not set (ongoing stay) | `addFolio` SP (insert with NULL) |
| datetime | Checkout timestamp | `Data.vb:203`, `frmPlacanje.vb:4268` |

---

## 5. Night Status

### 5.1 nocenja.PrijavaOdjava (Night Closed)

Source: `10_CHECKIN.md:320-323`, `15_EXPENSES_NIGHTS.md:244-247`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Active accommodation charge | `Unesinocenja` SP (INSERT with PrijavaOdjava=0), `frmPlacanje.vb:3569` (checkout copy) | `frmPrikazNocenja1.vb:5` (WHERE PrijavaOdjava=0), `15_EXPENSES_NIGHTS.md:233` |
| 1 | Closed/checked-out | `frmPlacanje.vb:3568,4256` (checkout), `Data.vb:172,175` (checkout) | `frmPlacanje.vb:3568` (WHERE PrijavaOdjava=0 filter) |

**Transition**: 0 → 1 at checkout. New rows with 0 are created to carry forward remaining night charges for guests still in room (`15_EXPENSES_NIGHTS.md:79-81`).

### 5.2 nocenja.brrac (Receipt Number)

Source: `15_EXPENSES_NIGHTS.md:152`

| Value | Meaning | Set By |
|-------|---------|--------|
| 0 | No receipt number (default) | INSERT |
| receipt# | Linked to payment receipt | `frmPlacanje.vb:3568` (UPDATE SET brrac=receipt#) |

---

## 6. Expense Status

### 6.1 troskovi.zaklj (Expense Locking)

Source: `15_EXPENSES_NIGHTS.md:229-242`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Open/active expense | `addTroskovi` SP (INSERT with zaklj=0) | `ModuleKod.vb:1051` (SP getTroskoviSoba WHERE zaklj=0), `frmPlacanje.vb:308` (WHERE zaklj=0) |
| 1 | Closed/paid | `frmPlacanje.vb:3808,3550,4295,6108` (UPDATE SET zaklj=1), `Data.vb:208` (checkout close all room expenses) | — |

**Transition**: 0 → 1 on payment. Storno path: 1 → 0 for non-accommodation expenses (`16_INVOICE_CHECKOUT.md:741`).

### 6.2 troskovi.Djelimicno (Partial Payment)

Source: `14_PAYMENT.md:280`

| Value | Meaning | Set By |
|-------|---------|--------|
| 0 | Not partially paid | Default |
| 1 | Partially paid | `frmPlacanje.vb:3812` (UPDATE SET Djelimicno=1, iznos=@remaining) |

### 6.3 troskovi.stan (Expense Status Flag)

Source: `00_DATABASE_SCHEMA.md:1533`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Default/normal | Default |

### 6.4 troskovi.fis (Fiscal Device Flag)

Source: `00_DATABASE_SCHEMA.md:1535`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Not sent to fiscal device | Default |

### 6.5 troskovivrste.tip (Expense Type Category)

Source: `02_MODULEKOD_FUNCTIONS.md:516-517`, `15_EXPENSES_NIGHTS.md:250-254`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Regular/one-time expense | Default in schema, shown in dropdown |
| 1 | Per-night accommodation type ("Nocenje sa doruckom") | `02_MODULEKOD_FUNCTIONS.md:580` (seed data), `frmTroskovi.vb:87` (excluded from dropdown: WHERE ID<>1) |

### 6.6 troskovivrste.del (Soft Delete)

Source: `15_EXPENSES_NIGHTS.md:264-269`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Active expense type | Default | `frmPlacanje.vb:596` (WHERE del=0) |
| Non-0 | Deleted expense type | — | — |

---

## 7. Payment Status

### 7.1 placanje.storno (Payment Storno)

Source: `14_PAYMENT.md:242-248`, `16_INVOICE_CHECKOUT.md:231-235`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Normal/active payment | `frmPlacanje.vb:4027` (INSERT), `frmPlati1.vb:511` (INSERT) | Default |
| 1 | Cancelled/storno | `frmRacuni.vb:731` (UPDATE SET storno=1) within transaction | `14_PAYMENT.md:244` |

### 7.2 placanjenacin (Payment Methods)

Source: `00_DATABASE_SCHEMA.md:642`

| ID | Method | Evidence |
|----|--------|----------|
| 1 | Gotovina (Cash) | `14_PAYMENT.md:231` |
| 2 | Virman (Transfer) | `14_PAYMENT.md:231` |
| 3 | Kartica (Card) | `14_PAYMENT.md:231` |
| 4 | Gratis | `14_PAYMENT.md:231` |
| 5 | Slozeno (Compound/Split) | `14_PAYMENT.md:231`, excluded in `frmPlacanjeSlozeno.vb:30` |

### 7.3 placanje.pdv (VAT Registrant Flag)

Source: `14_PAYMENT.md:288-293`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Not a VAT registrant (PDV exempt) | `frmPlati1.vb:506-507` (CheckBox5 not checked) |
| 1 | VAT registrant (standard PDV applies) | `frmPlati1.vb:507` |

### 7.4 placanje.ctax (Tourist Tax Flag)

Source: `14_PAYMENT.md:296-300`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | No tourist tax | `frmPlati1.vb:509` (CheckBox1 not checked) |
| 1 | Tourist tax applied | `frmPlati1.vb:509` (CheckBox1 checked) |

### 7.5 placanje.fiskal (Fiscal Receipt Sent)

Source: `00_DATABASE_SCHEMA.md:579`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Not sent to fiscal device | Default |

---

## 8. Invoice Status

### 8.1 printracuni.storno (Invoice Storno)

Source: `16_INVOICE_CHECKOUT.md:230-235`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 (false) | Normal/active invoice | Default | `frmRacuni.vb` DataGridView_CellFormatting |
| 1 (true) | Stornirano (cancelled) | `storniraj()` line ~738 (UPDATE SET storno=1) | Grid row gets gray background |

### 8.2 printracuni.exp (Export Status)

Source: `23_CARDS_EXPORT_MISC.md:343`

| Value | Meaning | Set By |
|-------|---------|--------|
| 0 | Normal/not exported | Default |
| 2 | Cancelled, awaiting export review | `storniraj()` line ~738 (UPDATE SET exp=2 alongside storno=1) |
| 3 | Processed/exported | `frmExport.vb:609` (UPDATE SET exp=3) |

### 8.3 printracuniavans.storno (Advance Invoice Storno)

Source: `02_MODULEKOD_FUNCTIONS.md:498-504`, `16_INVOICE_CHECKOUT.md:258-260`

| Value | Meaning | Set By |
|-------|---------|--------|
| 0 | Normal advance invoice | `frmPlacanje.vb:4382` (INSERT) |
| 1 | Storno advance invoice | `frmRacuni.vb:978` (UPDATE SET storno=1) |

**Note**: Advance invoice storno does NOT reverse payments or expenses, unlike regular invoice storno.

---

## 9. Alarm Status

### 9.1 alarm.storno (Alarm Cancelled)

Source: `02_MODULEKOD_FUNCTIONS.md:456-461`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Active alarm | `ModuleKod.vb:356` (INSERT with storno=0) | `provjerialarm WHERE storno=0` |
| 1 | Cancelled alarm | `ModuleKod.vb:389` (UPDATE SET storno=1), `frmRezervacije_unos.vb:385` (unchecked alarm) | `citajAlarm` (displayed as 'Stornirano') |

### 9.2 alarm.tip (Alarm Type)

Source: `02_MODULEKOD_FUNCTIONS.md:453-454`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 1 | Active alarm (reminder) | `ModuleKod.vb:356` (INSERT with tip=1), `frmRezervacije_unos.vb:320` (reservation alarm) | `provjerialarm WHERE tip=1` |

---

## 10. Guest Status / Tax

### 10.1 goststatus.del (Guest Status Soft Delete)

Source: `10_CHECKIN.md:331-335`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Active status | Default | `10_CHECKIN.md:143` (WHERE del=0) |

### 10.2 goststatus.taksa (Tax Per Status)

Source: `00_DATABASE_SCHEMA.md:198-203`

| Value | Meaning | Evidence |
|-------|---------|----------|
| Decimal amount | Tax amount applied to guests with this status | `goststatus` table lookup, `frmPrijavaGostiUnos.vb` (loaded into combo) |

**Known status types** (from `00_DATABASE_SCHEMA.md:202`): Turist, Vlasnik kuće, Dijete do 12 godina, etc.

### 10.3 drzave.del / drzave.domaca (Country Filters)

Source: `22_PARTNERS_TARIFFS_SETTINGS.md:460-466`

| Column | Value | Meaning | Read By |
|--------|-------|---------|----------|
| del=0 | Active country | Default | `20_GUESTS.md:1066` (WHERE domaca=1 for domestic) |
| domaca=1 | Home country (typically BiH) | `00_DATABASE_SCHEMA.md:60` | `20_GUESTS.md:1066-1073` (foreign guest identification) |

---

## 11. Other Status Fields

### 11.1 partneri.del (Partner Soft Delete)

Source: `22_PARTNERS_TARIFFS_SETTINGS.md:62-64`

| Value | Meaning | Set By |
|-------|---------|--------|
| 0 | Active partner | `frmPartneri.vb:87` (WHERE del=0) |
| 1 | Deleted partner | `frmPartner1.vb:364` (UPDATE SET del=1) |

### 11.2 sobatarifa.del (Tariff Soft Delete)

Source: `10_CHECKIN.md:331-335`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Active tariff | Default | `frmPrijava1.vb:912` (WHERE del=0) |
| 1 | Deleted tariff | `frmPrijava1.vb:1336` (UPDATE SET del=1) | Filtered out from tariff combos |

### 11.3 radnici.disabled (Worker Disabled)

Source: `00_DATABASE_SCHEMA.md:999`

| Value | Meaning | Set By | Read By |
|-------|---------|--------|----------|
| 0 | Active user | Default | `22_PARTNERS_TARIFFS_SETTINGS.md:1149` (WHERE disabled=0 AND nivo<9) |
| 1 | Disabled user | Manual update | Filtered out from login dropdown |

### 11.4 radnici.nivo (Access Level)

Source: `00_DATABASE_SCHEMA.md:1004`, `22_PARTNERS_TARIFFS_SETTINGS.md:310-313`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 3 | Limited (reservation-only) | `22_PARTNERS_TARIFFS_SETTINGS.md:312` |
| 5 | Standard receptionist | Implied (default) |
| 9 | Super admin | `22_PARTNERS_TARIFFS_SETTINGS.md:315` |

### 11.5 setings.naplposo (Billing Mode)

Source: `10_CHECKIN.md:389-393`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Split tariff per person (tariff ÷ number of guests) | `frmPrijava1.vb:755` |
| 1 | Full tariff per person | `frmPrijava1.vb:755` |

### 11.6 setings.cijt (Price Includes Tax)

Source: `22_PARTNERS_TARIFFS_SETTINGS.md:374`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Price excludes tax (cijena minus taxe) | Name implies |
| 1 | Price includes tax (cijena plus taxe) | Name implies |

### 11.7 neplaceni.placeno (Unpaid Item Paid)

Source: `16_INVOICE_CHECKOUT.md:59-60`

| Value | Meaning | Set By |
|-------|---------|--------|
| 0 | Not yet paid | `frmOdjava1.vb:963` (INSERT with placeno=0) |
| 1 | Paid (guest returned to room) | `frmRacuni.vb` (UPDATE SET placeno=1) |

### 11.8 posjete.status (Visit Status)

Source: `00_DATABASE_SCHEMA.md:702`

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Default/unknown | `01_DATABASE_SCHEMA.md:702` |

---

## 12. Status Transition Maps

### 12.1 Room Status Transition Map

```
                  ┌──────────────┐
                  │   SLOBODNA   │
                  │   (Tag=0)    │
                  └──────┬───────┘
                         │
        ┌────────────────┼────────────────┐
        │                │                │
   Check-in         Reservation         OOO checkbox
        │                │                │
        ▼                ▼                ▼
  ┌───────────┐  ┌─────────────────┐  ┌──────────────┐
  │  ZAUZETA  │  │ REZERVISANA    │  │ VAN UPOTREBE  │
  │  (Tag=1)  │  │ (Tag=3 or 6)  │  │  (Tag=5)     │
  └─────┬─────┘  └────────┬────────┘  └──────┬───────┘
        │                 │                    │
   Check-out          Confirm + Check-in    Uncheck OOO
        │                 │                    │
        ▼                 ▼                    ▼
  ┌───────────┐  ┌─────────────────┐  ┌──────────────┐
  │ ZAUZETA   │  │ ZAUZETA i      │  │ SLOBODNA     │
  │ departing │  │ REZERVISANA    │  │  (Tag=0)      │
  │ (Tag=2)   │  │ (Tag=4)        │  └──────────────┘
  └─────┬─────┘  └────────────────┘
        │
   Checkout
   + Clean=0
        │
        ▼
  ┌──────────────┐
  │ NIJE SPREMNA │  ◄── clean=0 OVERRIDES all other statuses
  │  (Tag=7)      │
  └──────────────┘
```

### 12.2 Reservation Status Transition Map

```
    ┌──────────────┐
    │  NEW (prijava=0, stornirana=0, potvrda=0)
    └──────┬───────┘
           │
     ┌─────┴────────────┐
     │                  │
  Confirm           Cancel
  (potvrda→1)    (stornirana→1)
     │                  │
     ▼                  ▼
┌───────────┐   ┌───────────┐
│ CONFIRMED │   │ STORNIRANA│
│ potvrda=1 |◄──┤stornirana=1│   ◄── Reactivate (stornirana→0)
└─────┬─────┘   └─────┬─────┘
      │               │
      │ toggle        │ toggle
      ▼               ▼
┌───────────┐   ┌───────────┐
│UNCONFIRMED│   │ REACTIVATED│
│ potvrda=0  │   │stornirana=0│
└──────────┘   └───────────┘

    Any state (prijava=0)
         │
         │ Check-in (prebaci)
         ▼
   ┌──────────────┐
   │  CHECKED-IN   │
   │  prijava=1     │
   └──────────────┘

    Any state (prijava=0, expired checkInDate)
         │
         │ Auto-expire on load
         ▼
   ┌──────────────┐
   │   EXPIRED     │
   │  prijava=2    │
   └──────────────┘
```

### 12.3 Expense Status Transition Map

```
  ┌──────────────┐    Payment closes     ┌──────────────┐
  │    OPEN      │ ───────────────────►  │   CLOSED     │
  │   zaklj=0    │                       │   zaklj=1    │
  └──────┬───────┘                       └──────┬───────┘
         │                                      │
         │ Storno (TID<>1 only)                │
         │ (reopens non-accommodation)         │
         ◄──────────────────────────────────────┘
         
  ┌──────────────┐    Partial payment     ┌─────────────────┐
  │    OPEN      │ ───────────────────►  │ PARTIALLY PAID │
  │   zaklj=0    │    + Djelimicno=1    │ Djelimicno=1   │
  └──────────────┘                       └─────────────────┘
```

### 12.4 Night Status Transition Map

```
  ┌──────────────┐    Checkout closes    ┌──────────────┐
  │  ACTIVE      │ ───────────────────►  │  CLOSED      │
  │ PrijavaOdjava│                       │ PrijavaOdjava │
  │     = 0      │                       │     = 1       │
  └──────┬───────┘                       └──────────────┘
         │
         │ Checkout creates copy with PrijavaOdjava=0
         │ for remaining guests (Data.vb:178)
         ▼
  ┌──────────────┐
  │  NEW ACTIVE   │  (for remaining guests in room)
  │ PrijavaOdjava │
  │     = 0       │
  └──────────────┘
```

### 12.5 Folio Status Transition Map

```
  ┌──────────────┐    Checkout closes    ┌──────────────┐
  │  OPEN FOLIO  │ ───────────────────►  │ CLOSED FOLIO │
  │ zakljucen=0  │                       │ zakljucen=1  │
  └──────┬───────┘                       └──────┬───────┘
         │                                      │
         │ "Return to room" (unpaid)           │
         │ (reopens folio)                     │
         ◄──────────────────────────────────────┘
```

### 12.6 Invoice Storno Transition Map

```
  ┌──────────────┐    Regular storno     ┌──────────────────────────────────┐
  │ NORMAL INVOICE│ ───────────────────►  │         STORNIRANA              │
  │  storno=0     │                        │ storno=1, exp=2               │
  │  exp=0/2/3    │                        │ datstor=timestamp              │
  └──────────────┘                        └────────────┬─────────────────────┘
                                                       │
                                          ┌────────────┼────────────────┐
                                          │            │                │
                                    Reopens expenses  Deletes accommodation  Cancels payment
                                    (zaklj→0,        (troskovi DELETE   (placanje.storno=1
                                     Brrac→null,       TID=1 only)        placanjeDetalji.storno=1)
                                     TID<>1 only)
```

### 12.7 Advance Invoice Storno Transition Map

```
  ┌──────────────┐    Advance storno    ┌──────────────────┐
  │ ADVANCE INVOICE│ ─────────────────►  │ STORNIRANA       │
  │  storno=0      │                      │  storno=1         │
  └────────────────┘                      └──────────────────┘
  
  Note: Does NOT affect payments, expenses, or folios.
```

### 12.8 Guest/Stay Status Transition Map

```
  ┌──────────────┐    Check-out          ┌──────────────┐
  │  CHECKED IN  │ ─────────────────►  │  CHECKED OUT  │
  │  odjavljen=0  │                       │ odjavljen=1   │
  └──────────────┘                       └──────────────┘
  
  Partial guest checkout (single guest):
  odjavljen → 1 for specific guest, others remain 0
  
  Folio transitions:
  posjetafolio.zakljucen: 0 → 1 (all guests out)
  
  Night transitions:
  nocenja.PrijavaOdjava: 0 → 1 (all or specific guest)
  Plus: INSERT new nocenja for remaining guests (split share)
  
  Room clean status:
  sobe.clean: 1 → 0 (marked dirty after checkout)
```