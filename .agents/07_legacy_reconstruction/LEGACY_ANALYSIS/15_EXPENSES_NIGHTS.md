# Legacy Expenses (troskovi) & Nights (nocenja) Flows Analysis

> Source files:
> - `legacy_code/frmTroskovi.vb` (515 lines)
> - `legacy_code/frmTroskoviNoc.vb` (242 lines)
> - `legacy_code/frmTrosSvi.vb` (28 lines)
> - Cross-ref: `legacy_code/ModuleKod.vb`, `legacy_code/frmPlacanje.vb`, `legacy_code/frmRacuni.vb`, `legacy_code/frmSobaInfo.vb`, `legacy_code/frmZurnal1.vb`

---

## 1. Business Flow: Expenses and Night Ledger

### 1.1 Expense (troskovi) Flow

**Entry points (forms that open expense dialogs):**

| Form | Trigger | Action | Evidence |
|------|---------|--------|----------|
| `frmGlavni` | Menu action | Opens `frmTroskovi` to add expense to a room | `frmGlavni.vb:1142` |
| `frmSobe` | Room context | Opens `frmTroskovi` | `frmSobe.vb:679` |
| `frmSobaInfo` | Room info | Opens `frmTroskovi` | `frmSobaInfo.vb:2191` |
| `frmPlati1` | Payment screen | Opens `frmTroskovi` | `frmPlati1.vb:733` |
| `frmPlacanje` | "Dodaj" menu | Opens `frmTroskovi` as `trosakP` action | `frmPlacanje.vb:5897-5906` |
| `frmPlacanje` | "UmanjiNocenje" menu | Opens `frmTroskoviNoc` to modify night charge | `frmPlacanje.vb:6605-6614` |
| `frmRezervacije` | Reservation context | Opens `frmTroskovi` / `frmTrosSvi` | `frmRezervacije.vb:1033,1078` |

**Expense add flow (`frmTroskovi`):**

1. On load: `ucitajGoste()` populates guest/room dropdowns via SP `getGosti`; `ucitajUsluge()` loads expense types excluding ID=1 (accommodation) via inline SQL (`frmTroskovi.vb:87`)
2. User selects a guest → auto-selects associated room via `cmbGost0`/`cmbSoba0` binding
3. User clicks an expense type in DataGridView → `dgv_CellClick` sets `lblUsl.Tag` to the TID and `lblUsl.Text` to the name (`frmTroskovi.vb:330-349`)
4. Special handling for TID=3 (restaurant): warning that it won't be printed on fiscal device (`frmTroskovi.vb:331`)
5. Special handling for TID=5 (mini-bar/POS): expands form, loads items from KASA database (`mysqlReaderK`), populates `dgvmini` grid, disables quantity/price editing (`frmTroskovi.vb:332-346`)
6. User adjusts quantity (`txtkol`) and possibly unit price (`txtcjen`); total auto-calculates via `pror()` or `prorM()` (`frmTroskovi.vb:466-506`)
7. On "Save" (`ButtonX2_Click`): validates room selected, amount > -200, confirms dialog (`frmTroskovi.vb:399-439`)
8. If TID=5, calls `snimi_mini()` which writes to KASA database tables (`zbirni`, `kasa`, `kasa_detalji`) and returns a GUID-based `iddm` (`frmTroskovi.vb:367-398`)
9. Calls `upisiTroskove(iddm)` which invokes SP `addTroskovi` with params: GSID, SID, TID, vrijeme=Now(), kolicina, iznos, radnikID=1 (hardcoded!), iddzid (`frmTroskovi.vb:136-202`)
10. Logs action via `funkcije.logs()` (`frmTroskovi.vb:414`)

**Expense type (troskovivrste) add flow (`frmTroskovi`):**

1. User types new service name in `txtUsluga`; blocked if name starts with "osigu" or "bora" (insurance/tax prohibition) (`frmTroskovi.vb:357-364`)
2. Calculates next ID via `SELECT MAX(ID) + 1 FROM troskovivrste` (`frmTroskovi.vb:218`)
3. INSERTs new row: `INSERT INTO troskovivrste (ID, naziv) VALUES (@ID, @naziv)` (`frmTroskovi.vb:244`)
4. Expense type code (`sifra`) can be edited inline in DataGridView, UPDATEs via: `UPDATE troskovivrste SET sifra=... WHERE id=...` (`frmTroskovi.vb:511`)

**Expense modification flow:**

- When guest moves to different room, individual expenses are transferred via SP `unesiPojedinacne`: `UPDATE troskovi SET SID=noviSID WHERE SID=stariSID AND zaklj=0 AND ID=ID` (`ModuleKod.vb:1086`, called from `frmSobaInfo.vb:1243-1275`)

### 1.2 Night Charge (nocenja) Flow

**Night charge creation during check-in (`frmRacuni`):**

1. `nocenja()` method calculates per-guest tariff: `tarifa / brojGostiju` (`frmRacuni.vb:1398-1413`)
2. For each selected guest row, calls `dodajnocenja()` which invokes SP `Unesinocenja` with params: RID, DatumP=Now(), Tarifa, SID, PID, opis="", ssoba (room name) (`frmRacuni.vb:1414-1437`)

**SP `Unesinocenja` logic (`ModuleKod.vb:1081`):**

1. DELETE existing nocenja records for same RID and same month: `DELETE FROM nocenja WHERE RID=@RID AND DATE_FORMAT(datump, '%Y-%d-%m')=DATE_FORMAT(DatumPp, '%Y-%d-%m')`
2. INSERT new record: `INSERT INTO nocenja (RID, DatumP, Tarifa, SID, PID, PrijavaOdjava, opis, popust, soba) VALUES (...)`
   - `PrijavaOdjava` is always set to `0` (active charge)
3. **BUG**: DATE_FORMAT uses `%Y-%d-%m` instead of `%Y-%m-%d` — this could cause incorrect month matching since day and month are swapped

**Night charge modification in payment (`frmPlacanje`):**

1. `imalinocenja()` checks if dataTableKrajnji has rows with TID=1 (accommodation type) (`frmPlacanje.vb:1061-1075`)
2. `nocenjePromjena(dod0)`: if dod0=0 calls `dodavanjenocenja()`, if dod0=1 calls `brisanjenocenja()` (`frmPlacanje.vb:1076-1092`)
3. `dodavanjenocenja()`: adds a row to in-memory dataTableKrajnji with TID=1, name="Nocenje", price from `vratiCijenunocenja()`, quantity from `VratiBrojDana()` (`frmPlacanje.vb:1093-1163`)
4. `trosaknocenjaDodavanje()`: populates in-memory row with stay data and night price calculation (`frmPlacanje.vb:1121-1163`)
5. `brisanjenocenja()`: removes night charge row from in-memory dataTableKrajnji for current guest/room (`frmPlacanje.vb:1231-1257`)
6. `VratiBrojDana()`: calculates nights using check-in/check-out times with 08:00 AM cutoff and noon checkout rules (`frmPlacanje.vb:1184-1203`)

**Night charge checkout flow (`frmPlacanje`):**

On checkout/payment, two critical nocenja operations happen:

1. `UPDATE nocenja SET PrijavaOdjava = 1, datumodj = checkout_datetime, brrac = receipt_number WHERE SID = roomID AND PrijavaOdjava = 0` (`frmPlacanje.vb:3568`)
2. `INSERT INTO nocenja (RID, DatumP, SID, PID, PrijavaOdjava, Tarifa, popust, opis, soba) SELECT RID, checkout_date, SID, PID, 0, Tarifa, popust, opis, soba FROM nocenja WHERE SID = roomID AND pid = folioID` (`frmPlacanje.vb:3569`)
   - This copies active night charges with PrijavaOdjava=0, setting the new copy's DatumP to checkout date (for next-day charging)

**Checkout transaction (`OdjaviSobu`):**

Within a single database transaction (`frmPlacanje.vb:4240-4288`):
1. `UPDATE nocenja SET PrijavaOdjava = 1 WHERE SID = @SID AND PID = @PID` (close nights)
2. `UPDATE relgostsoba SET odjavljen = 1, checkOutDate = @checkOutDate, checkOutRadnik = @radnik WHERE sobaID = @SID AND odjavljen=0` (check out)
3. `UPDATE Avans SET placeno = 1 WHERE brSobe = @SID AND placeno=0` (mark advances as paid)
4. `UPDATE posjetaFolio SET vrijemeO = @datumO, zakljucen = @zakljucen WHERE ID = @PID` (close folio)

### 1.3 All-Expenses View (`frmTrosSvi`)

Minimal form that loads all open expenses (`zaklj=0` and `sid>0`) with room name, service name, time, quantity, amount, worker, note, and receipt number via:

```sql
SELECT sobe.naziv as soba, troskovivrste.naziv as usluga, vrijeme, kolicina, iznos, radnici.ime as radnik, napomena, brrac
FROM troskovi
JOIN sobe ON sobe.id=troskovi.sid
JOIN troskovivrste ON troskovi.tid=troskovivrste.id
JOIN radnici ON radnici.id=troskovi.radnikid
WHERE zaklj=0 AND sid>0
```
(`frmTrosSvi.vb:4`)

Computes `SUM(iznos)` and saves to XML file at hardcoded path `C:\Program Files\IMEDIA\HotelPro\trosk.xml`. Also has a Crystal Reports button (`frmTrosSvi.vb:16-27`).

---

## 2. SQL Inventory for troskovi and nocenja Tables

### 2.1 `troskovi` Table Schema

| Column | Type | Nullable | Default | Purpose |
|--------|------|----------|---------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) | | Expense record ID |
| GSID | int(10) | YES | | Guest-room stay ID (FK → relgostsoba.ID) |
| SID | int(10) | YES | | Room ID (FK → sobe.ID) |
| TID | int(10) | YES | | Expense type ID (FK → troskovivrste.ID) |
| vrijeme | datetime | NO | | Timestamp of charge |
| kolicina | int(10) | YES | | Quantity |
| iznos | decimal(18,2) | NO | | Total amount |
| radnikID | int(10) | YES | | Worker who entered charge (FK → radnici.ID) |
| napomena | varchar(50) | YES | | Note/comment |
| zaklj | tinyint(4) | YES | 0 | **Locked/finalized flag**: 0=open, 1=closed |
| Brrac | decimal(18,0) | YES | | Receipt number (linked to printracuni.BrojRacuna) |
| Djelimicno | tinyint(4) | YES | | Partial payment flag |
| iddzid | varchar(45) | YES | | KASA linking ID (for mini-bar/POS items) |
| idzid | varchar(45) | YES | | KASA linking ID |
| loc | varchar(45) | YES | | Location identifier |
| zidbr | varchar(45) | YES | | KASA zbirni receipt number |
| fisbr | text | YES | | Fiscal receipt number |
| stan | int(10) unsigned | NO | 0 | Status flag |
| opis | text | YES | | Description |
| fis | int(10) unsigned | NO | 0 | Fiscal device flag |

Source: `00_DATABASE_SCHEMA.md:1511-1539`

### 2.2 `nocenja` Table Schema

| Column | Type | Nullable | Default | Purpose |
|--------|------|----------|---------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) | | Night record ID |
| RID | int(10) | YES | | Stay record ID (FK → relgostsoba.ID) |
| DatumP | datetime | YES | | Date of night charge |
| DatumOdj | datetime | YES | | Checkout date (added by ALTER) |
| SID | int(10) | YES | | Room ID (FK → sobe.ID) |
| PID | int(10) | YES | | Folio ID (FK → posjetafolio.ID) |
| PrijavaOdjava | tinyint(4) | YES | | **Night status**: 0=active charge, 1=closed |
| Tarifa | decimal(18,2) → DOUBLE | YES | | Tariff price per night |
| popust | int(10) | YES | 0 | Discount percentage |
| opis | text | YES | | Description (added by ALTER) |
| soba | varchar(45) | YES | | Room name text (added by ALTER) |
| brrac | int(10) unsigned | YES | 0 | Receipt number (added by ALTER) |

Source: `00_DATABASE_SCHEMA.md:479-498`, ALTER at `ModuleKod.vb:511,543,664`

### 2.3 `troskovivrste` Table Schema

| Column | Type | Nullable | Default | Purpose |
|--------|------|----------|---------|---------|
| ID | decimal(10,0) | NO (PK) | | Expense type ID |
| naziv | varchar(150) | YES | | Name (e.g. "Nocenje sa doruckom", "Mini Bar") |
| cijenaID | int(10) | YES | | Price list link |
| tip | int(10) unsigned | NO | 0 | **Type flag**: 0=regular, 1=per-night |
| del | int(10) unsigned | NO | 0 | Soft delete flag |
| sifra | varchar(45) | YES | '' | Code/identifier |

Source: `00_DATABASE_SCHEMA.md:1558-1572`, ALTER at `ModuleKod.vb:666`

### 2.4 `troskovipojedinacni` Table Schema

| Column | Type | Nullable | Default | Purpose |
|--------|------|----------|---------|---------|
| Auto | bigint(19) | NO (PK, AUTO_INCREMENT) | | Auto ID |
| IDtroska | bigint(19) | NO | | FK → troskovi.ID |
| datum | datetime | YES | | Date of partial payment |
| iznos | decimal(19,4) | NO | | Partial amount |

Source: `00_DATABASE_SCHEMA.md:1543-1556`

---

## 3. Database Writes (every INSERT/UPDATE/DELETE)

### 3.1 INSERT Operations

| # | Table | Columns | Source | Trigger |
|---|-------|---------|--------|---------|
| 1 | `troskovi` | iddzid, GSID, SID, TID, vrijeme, kolicina, iznos, radnikID | SP `addTroskovi` (`ModuleKod.vb:903`) | frmTroskovi.ButtonX2_Click → upisiTroskove() |
| 2 | `troskovi` | GSID=0, SID, TID=1, vrijeme=Now, kolicina, iznos, radnikID=1, napomena, zaklj=1, Brrac | `frmPlacanje.vb:3822` | Payment — night charge line creation |
| 3 | `nocenja` | RID, DatumP, Tarifa, SID, PID, PrijavaOdjava=0, opis, Pop, soba | SP `Unesinocenja` (`ModuleKod.vb:1081`) | Check-in (frmRacuni.nocenja → dodajnocenja) |
| 4 | `nocenja` (bulk copy) | RID, checkout_date, SID, PID, PrijavaOdjava=0, Tarifa, popust, opis, soba | `frmPlacanje.vb:3569` | Checkout — copies active nights for next-day |
| 5 | `troskovivrste` | ID (MAX+1), naziv | `frmTroskovi.vb:244` | Add new expense type |
| 6 | `troskovivrste` (seed) | naziv='Nocenje sa doruckom', tip=1 | `ModuleKod.vb:580` | Schema migration seed |
| 7 | `placanjedetalji` | brojID, art, kolicina, cijena, iznos, (brojnocenja|napomena), PID, (ranijeUplate) | `frmPlacanje.vb:3819,3825,3917` | Payment detail line |
| 8 | `troskovipojedinacni` | IDtroska, datum, iznos | `frmPlacanje.vb:3815` | Partial payment detail |
| 9 | `zbirni` | pjs, stan, br_racun, ... | `frmTroskovi.vb:376` (via `snimi_mini`) | KASA mini-bar integration |
| 10 | `kasa` | pjs, locid, brrc, ... | `frmTroskovi.vb:379` (via `snimi_mini`) | KASA receipt record |
| 11 | `kasa_detalji` | gang, termid, ... kolicina, art_broj, ... | `frmTroskovi.vb:392` (via `snimi_mini`) | KASA receipt line items |

### 3.2 UPDATE Operations

| # | Table | SET | WHERE | Source | Trigger |
|---|-------|-----|-------|--------|---------|
| 1 | `troskovi` | zaklj=1, Brrac=@brojrac | troskovi.ID=@ID | `frmPlacanje.vb:3808` | Payment — close expense |
| 2 | `troskovi` | zaklj=1, Brrac=receipt# | troskovi.ID=idtrr (string concat) | `frmPlacanje.vb:3550` | Batch payment — close expense |
| 3 | `troskovi` | zaklj=1 | SID=sobID | `frmPlacanje.vb:4295` (via `izmjeniTroskoviZaklj`) | Checkout — close all room expenses |
| 4 | `troskovi` | Djelimicno=1, iznos=@iznosUp | troskovi.ID=@IDtroska | `frmPlacanje.vb:3812` | Partial payment — mark and adjust amount |
| 5 | `troskovi` | zaklj=1, Brrac=@brojrac | troskovi.ID=idtroska (string concat) | `frmPlacanje.vb:6108` | Alternative payment close |
| 6 | `troskovi` | zaklj=0, Brrac=null | Brrac=@Rbr AND TID<>1 | `frmRacuni.vb:741` | Storno — reopen non-accommodation expenses |
| 7 | `troskovi` | SID=noviSID | SID=stariSID AND zaklj=0 AND ID=ID | SP `unesiPojedinacne` (`ModuleKod.vb:1086`) | Room transfer — move expense to new room |
| 8 | `nocenja` | PrijavaOdjava=1, datumodj=checkout, brrac=receipt# | SID=roomID AND PrijavaOdjava=0 | `frmPlacanje.vb:3568` | Checkout — close active nights |
| 9 | `nocenja` | PrijavaOdjava=1 | SID=@SID AND PID=@PID | `frmPlacanje.vb:4256` | Checkout via `OdjaviSobu` — close nights |
| 10 | `troskovivrste` | sifra=value | id=value | `frmTroskovi.vb:511` | Inline edit of expense type code |
| 11 | `nocenja` | Tarifa=DOUBLE | (ALTER TABLE) | `ModuleKod.vb:511` | Schema migration — change column type |
| 12 | `posjetaFolio` | vrijemeO=@datumO, zakljucen=@zakljucen | ID=@PID | `frmPlacanje.vb:4268` | Checkout — close folio |

### 3.3 DELETE Operations

| # | Table | WHERE | Source | Trigger |
|---|-------|-------|--------|---------|
| 1 | `nocenja` | RID=@RID AND DATE_FORMAT(datump,'%Y-%d-%m')=DATE_FORMAT(DatumPp,'%Y-%d-%m') | SP `Unesinocenja` (`ModuleKod.vb:1081`) | Re-insert nights — delete same-month entries first |

**Note**: There is NO direct DELETE on `troskovi`. Expenses are only logically deleted by setting `zaklj=1` or storno patterns.

---

## 4. Status Fields

### 4.1 Expense `zaklj` Locking Flag

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Open/active expense — visible in billing, editable | `ModuleKod.vb:1051` (SP `getTroskoviSoba` WHERE zaklj=0), `frmPlacanje.vb:308` (SELECT ... WHERE zaklj=0), `Data.vb:233,245,334` |
| 1 | Closed/finalized — expense has been paid/invoiced | `frmPlacanje.vb:3808,3550,4295,6108` (SET zaklj=1 on payment) |

**Locking behavior:**
- When payment is processed: `zaklj` is set to `1` and `Brrac` is set to the receipt number (`frmPlacanje.vb:3808`)
- On checkout without individual receipt: `zaklj=1` for all room expenses via `izmjeniTroskoviZaklj` (`frmPlacanje.vb:4290-4313`)
- Storno (receipt cancellation): `zaklj=0, Brrac=null` for non-accommodation expenses (`frmRacuni.vb:741`)
- Partial payment: `Djelimicno=1` flag set alongside adjusted amount (`frmPlacanje.vb:3812`)

### 4.2 Night `PrijavaOdjava` Status

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Active accommodation charge — guest is still in house | SP `Unesinocenja` INSERT PrijavaOdjava=0 (`ModuleKod.vb:1081`), `frmPlacanje.vb:3569` |
| 1 | Checked out — night charge closed | `frmPlacanje.vb:3568,4256` (UPDATE SET PrijavaOdjava=1 on checkout) |

### 4.3 Expense Type `tip` Categories

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Regular/one-time expense | Default in schema (`00_DATABASE_SCHEMA.md:1567`) |
| 1 | Per-night accommodation type ("Nocenje sa doruckom") | Seed: `ModuleKod.vb:580`, excluded from dropdown: `frmTroskovi.vb:87` (WHERE ID<>1) |

### 4.4 Expense Type Protected Names

Names starting with "osigu" (osiguranje=insurance) or "bora" (boravisna taksa=tourist tax) are blocked from manual creation (`frmTroskovi.vb:357-360`).

### 4.5 Night Record Date Matching

SP `Unesinocenja` deletes existing records where `DATE_FORMAT(datump, '%Y-%d-%m') = DATE_FORMAT(DatumPp, '%Y-%d-%m')`. **This appears to be a BUG** — the format `%Y-%d-%m` swaps day and month, meaning it would match records from the same day-of-month in different months (e.g., Jan 15 matches Feb 15).

### 4.6 `troskovivrste.del` Soft Delete

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Active expense type | `frmPlacanje.vb:596` (WHERE del=0) |
| Non-0 | Deleted/hidden expense type | Implied |

---

## 5. Business Rules

### 5.1 Add Expense

1. **Accommodation (TID=1) is excluded** from the expense type dropdown (`frmTroskovi.vb:87`: `WHERE troskovivrste.ID <> 1`). Nights are managed separately via `nocenja` table.
2. **Insurance and tourist tax names are prohibited** from manual creation (`frmTroskovi.vb:357-360`).
3. **Minimum amount validation**: iznos must be > -200 (`frmTroskovi.vb:404`, `frmTroskoviNoc.vb:170`). This allows negative charges (refunds/discounts) up to -200.
4. **KASA integration for TID=5 (mini-bar)**: When expense type ID is 5, form expands to show a mini-bar item grid loaded from external KASA database. Items are written to `zbirni`, `kasa`, `kasa_detalji` tables and the expense itself is written via `addTroskovi` with `iddzid` linking back to KASA records (`frmTroskovi.vb:332-398`).
5. **Restaurant TID=3**: Warning shown that this expense won't print on fiscal device (`frmTroskovi.vb:331`).
6. **radnikID is hardcoded to 1** in both frmTroskovi and frmTroskoviNoc, instead of using the logged-in worker's ID.
7. **Expense type codes** (`sifra`) can be edited inline in the DataGridView grid (`frmTroskovi.vb:508-514`).
8. **New expense type IDs** are calculated as `MAX(ID) + 1` without transaction safety (`frmTroskovi.vb:218`, race condition risk).

### 5.2 Modify/Transfer Expense

1. **Room transfer**: When a guest moves rooms, `unesiPojedinacne` SP moves individual open expenses (`zaklj=0`) from old SID to new SID (`ModuleKod.vb:1086`, called from `frmSobaInfo.vb:1243-1275`).
2. **Partial payment**: If an expense is partially paid, `Djelimicno=1` is set, `iznos` is updated to remaining amount, and original amount is stored in `troskovipojedinacni` (`frmPlacanje.vb:3812-3816`).

### 5.3 Delete Expense

1. **No physical DELETE** on `troskovi` exists in the codebase. Deletion is always logical:
   - Set `zaklj=1` (close/finalize)
   - Set `zaklj=1, Brrac=null` on storno (`frmRacuni.vb:741`: TID<>1 only, meaning accommodation stays closed)
2. **Night records ARE physically deleted** in SP `Unesinocenja` before re-insert (same-month same-RID records deleted).

### 5.4 Locking/Unlocking Rules

1. **Close expense**: `zaklj=1, Brrac=receipt_number` on payment processing (`frmPlacanje.vb:3808`)
2. **Close all room expenses**: `zaklj=1 WHERE SID=roomID` on room checkout (`frmPlacanje.vb:4295`)
3. **Storno (cancel receipt)**: `zaklj=0, Brrac=null WHERE Brrac=@Rbr AND TID<>1` — reopens non-accommodation expenses, accommodation stays locked (`frmRacuni.vb:741`)

### 5.5 Night Charge Calculation Rules

1. **Day calculation**: `VratiBrojDana()` (`frmPlacanje.vb:1184-1203`):
   - If check-in hour < 08:00, subtract one day from check-in date
   - If checkout hour >= 12:00, add one day to checkout date
   - If resulting difference is 0, set to 1 (minimum 1 night)
2. **Tariff splitting**: When multiple guests share room, tariff is divided: `tarifa / brojGostiju` (`frmRacuni.vb:1409`)
3. **Prior night payments**: `vratiUplatunocenja()` retrieves previously paid amounts for display in payment dialog (`frmPlacanje.vb:927,1150`)

### 5.6 Night Charge Modification (`frmTroskoviNoc`)

1. Opened from `frmPlacanje` → "UmanjiNocenje" menu (`frmPlacanje.vb:6605-6614`)
2. Uses same SP `addTroskovi` with TID=1 (hardcoded) and `iddzid=""` (empty, no KASA link) (`frmTroskoviNoc.vb:34`)
3. Confirmation message says "izmjena nocenje" (modify night) rather than "add expense" (`frmTroskoviNoc.vb:171`)
4. Otherwise structurally identical to `frmTroskovi` but without expense type selector or KASA integration

### 5.7 Night Checkout Business Logic

1. On checkout, active night charges are closed (`PrijavaOdjava=1`) and new ones are created for the checkout date (`frmPlacanje.vb:3568-3569`).
2. The checkout transaction is atomic within a MySQL transaction: night close → guest checkout → advance close → folio close (`frmPlacanje.vb:4249-4273`).
3. Night records carry `brrac` (receipt number) to link them to the payment receipt.

---

## 6. Cross-Reference

### 6.1 Shared Stored Procedures

| SP | Used By | Purpose |
|----|---------|---------|
| `addTroskovi` | frmTroskovi.vb:172, frmTroskoviNoc.vb:52 | INSERT expense record |
| `Unesinocenja` | frmRacuni.vb:1416 | DELETE+INSERT night records (per month) |
| `unesiPojedinacne` | frmSobaInfo.vb:1243 | UPDATE expense SID (room transfer) |
| `getGosti` | frmTroskovi.vb:28, frmTroskoviNoc.vb:94 | Load guest list for dropdowns |
| `getTroskoviSoba` | ModuleKod.vb:1051 | SELECT open expenses for room (WHERE zaklj=0) |
| `vratiTrosakSoba` | ModuleKod.vb:1146 | SUM(expenses) for room (WHERE zaklj=0) |
| `getGlavniTrosakIme` | ModuleKod.vb:931 | SELECT expenses with guest names |
| `getTroskoveLista` | ModuleKod.vb:1226 | SELECT expenses with type names for a room |

### 6.2 Related Views

| View | Definition | Purpose |
|------|-----------|---------|
| `troskovisuma` | SELECT troskovivrste.naziv, SUM(troskovi.iznos) GROUP BY naziv | Expense summary by type (`ModuleKod.vb:831`) |

### 6.3 Related ModuleKod Functions

| Function | Module | Relevance |
|----------|--------|-----------|
| `artGet()` | P1-Expenses | Gets all expense type names (`ModuleKod.vb:2205-2228`) |
| `artProvjera()` | P1-Expenses | Verifies expense types (always returns 0 — incomplete, `ModuleKod.vb:2229-2253`) |
| `vratiMaxDatum()` | P0-Nights | Gets max nocenja date for a room — used for calculating stay end (`ModuleKod.vb:1116`) |
| `vratiRIDNocenja()` | P0-Nights | Gets distinct RIDs for room's active nights (`ModuleKod.vb:1121`) |

### 6.4 Forms That Query troskovi/nocenja

| Form | Query Pattern | Key Fields |
|------|--------------|------------|
| `frmPlacanje` | JOIN relgostsoba+troskovi+troskovivrste WHERE zaklj=0 | IDtroska, TID, naziv, iznos, kolicina (`frmPlacanje.vb:308`) |
| `frmPlati1` | troskovi JOIN troskovivrste WHERE zaklj=0 AND sid=sobaid | ID, GSID, SID, TID, vrijeme, kolicina, iznos, zaklj, Brrac, Djelimicno (`frmPlati1.vb:189`) |
| `Data.vb` | troskovi JOIN troskovivrste WHERE zaklj=0 AND sid=sobaid | Same fields (`Data.vb:233,245,334`) |
| `frmZurnal1` | troskovi JOIN troskovivrste WHERE zaklj='0' AND sid>0 ORDER BY vrijeme | Daily journal view (`frmZurnal1.vb:512,584`) |
| `frmTrosSvi` | troskovi+sobe+troskovivrste+radnici WHERE zaklj=0 AND sid>0 | All open expenses report (`frmTrosSvi.vb:4`) |

### 6.5 Cross-Reference to 02_MODULEKOD_FUNCTIONS.md

| Section | Relevance |
|---------|-----------|
| 3.1 SELECT Operations | `getTroskoviSoba` (line 1051), `getGlavniTrosakIme` (line 931), `vratiTrosakSoba` (line 1146), `getTroskoveLista` (line 1226) |
| 3.2 INSERT Operations | `addTroskovi` SP (line 903), troskovivrste seed (line 580) |
| 3.5 Views | `troskovisuma` (line 831) |
| 3.6 Stored Procedures | `addTroskovi` (line 903), `Unesinocenja` (line 1081), `unesiPojedinacne` (line 1086), `vratiMaxDatum` (line 1116), `vratiRIDNocenja` (line 1121), `getTroskoviSoba` (line 1051), `getGlavniTrosakIme` (line 931), `vratiTrosakSoba` (line 1146), `getTroskoveLista` (line 1226) |
| 5.12 Expense Type tip | tip=1 = "Nocenje sa doruckom" (accommodation night), tip=0 = regular |
| 5.15 Night Record PrijavaOdjava | 0=active charge, 1=closed |
| 5.16 Expense zaklj Status | 0=open, 1=closed |

---

## 7. Key Findings for Modern System

### 7.1 Critical Bugs

| # | Bug | Location | Impact | Modern Fix |
|---|-----|----------|--------|------------|
| 1 | **DATE_FORMAT month swap** in `Unesinocenja` | `ModuleKod.vb:1081` | Uses `%Y-%d-%m` instead of `%Y-%m-%d`; will match wrong months | Use correct `%Y-%m-%d` format or use `YEAR()= AND MONTH()=` |
| 2 | **Hardcoded radnikID=1** | `frmTroskovi.vb:168`, `frmTroskoviNoc.vb:48` | All expenses record worker ID 1, losing audit trail | Use logged-in user's ID |
| 3 | **Race condition in expense type ID** | `frmTroskovi.vb:218-244` | MAX(ID)+1 without locking; concurrent inserts could have same ID | Use AUTO_INCREMENT |
| 4 | **No DELETE on troskovi** | Entire codebase | Closed expenses (zaklj=1) are never purged; table grows indefinitely | Add archival/cleanup policy |
| 5 | **Storno only reopens TID<>1** | `frmRacuni.vb:741` | Accommodation (TID=1) stays locked on storno, potentially incorrect | Review business rule; allow storno for accommodation too |

### 7.2 Architecture Issues

| # | Issue | Detail | Recommendation |
|---|-------|--------|----------------|
| 1 | **Nights and expenses are separate but conceptually linked** | `nocenja` (TID=1 in troskovi context) is stored in a different table with different lifecycle; `frmTroskoviNoc` adds via `addTroskovi` (which writes to `troskovi`) but `frmRacuni` writes directly to `nocenja` table via SP | Unify: treat nights as a specialized expense type in a single ledger, or clearly separate accommodation charges from incidental charges |
| 2 | **No transaction safety in expense creation** | `addTroskovi` SP does a single INSERT without wrapping in transaction; payment operations use manual transaction management | Use database transactions consistently |
| 3 | **KASA (POS) tight coupling** | TID=5 triggers a completely different code path that writes to 3 KASA tables (`zbirni`, `kasa`, `kasa_detalji`) with SQL injection-vulnerable string concatenation (`frmTroskovi.vb:376-392`) | Use parameterized queries; decouple POS integration via service layer |
| 4 | **In-memory state manipulation** | `frmPlacanje` builds `dataTableKrajnji` in memory for night charge calculation, calculates totals in hash tables, then writes to DB — no persistence, data loss risk on crash | Persist in-progress payment state or use a staging table |
| 5 | **Duplicate nocenja INSERT on checkout** | `frmPlacanje.vb:3569` copies active nights with a new date then closes originals — this could create duplicates if the transaction fails between operations | Use transaction; consider UPDATE-only approach |
| 6 | **Fixed path for XML export** | `frmTrosSvi.vb:9` hardcodes `C:\Program Files\IMEDIA\HotelPro\trosk.xml` | Use configurable path |

### 7.3 Data Model Simplification Opportunities

| Current | Problem | Proposed |
|---------|---------|----------|
| `troskovi.zaklj` (tinyint) | Boolean flag as tinyint; mixed with Brrac for finalization | Separate `status` enum: DRAFT, OPEN, CLOSED, PARTIAL |
| `troskovi.Brrac` as decimal(18,0) | Receipt number stored as decimal | Integer FK → printracuni.BrojRacuna |
| `nocenja.PrijavaOdjava` | Confusingly named; 0/1 stored as tinyint | Rename to `is_closed` boolean |
| Separate `nocenja` + `troskovi` tables | Night charges in nocenja are also sometimes in troskovi (via frmTroskoviNoc) | Unified `charges` table with `category` enum: ACCOMMODATION, MINIBAR, RESTAURANT, OTHER |
| `troskovipojedinacni` | Linked by IDtroska but no cascade | Consider embedding partial payments in payment detail lines |
| Hardcoded `radnikID=1` | No audit trail for who entered expenses | FK to `users` table with NOT NULL constraint |

### 7.4 Business Rules to Preserve

| Rule | Description | Evidence |
|------|-------------|----------|
| Accommodation type (TID=1) excluded from regular expense entry | Prevents double-charging for nights | `frmTroskovi.vb:87` |
| Insurance/tax names blocked from manual creation | Prevents unauthorized border-crossing charges | `frmTroskovi.vb:357-360` |
| Minimum charge validation (>-200) | Allows refunds up to 200 but prevents large negative entries | `frmTroskovi.vb:404`, `frmTroskoviNoc.vb:170` |
| Night charge day calculation (08:00 / 12:00 cutoffs) | Hotel industry standard for day boundary | `frmPlacanje.vb:1187-1203` |
| Tariff per-guest splitting | Multi-guest rooms split tariff proportionally | `frmRacuni.vb:1409` |
| Night records deleted-before-reinsert (upsert pattern) | Ensures current month has only one active night record per guest | SP `Unesinocenja` |
| Partial payment flag | `Djelimicno=1` marks expenses that were partially paid | `frmPlacanje.vb:3812` |
| Room transfer moves expenses | Open expenses move with guest | SP `unesiPojedinacne` |
| Checkout closes night records atomically | 4-step transaction: close nights → checkout guest → close advances → close folio | `frmPlacanje.vb:4249-4273` |