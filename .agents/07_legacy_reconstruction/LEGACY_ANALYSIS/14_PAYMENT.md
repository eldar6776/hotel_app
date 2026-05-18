# Payment (Placanje) Flow - Legacy Analysis

> **Source Files:**
> - `legacy_code/frmPlacanje.vb` (6679 lines) — Main payment form
> - `legacy_code/frmPlati1.vb` (758 lines) — Payment receipt/tax form
> - `legacy_code/frmPlacanjeSlozeno.vb` (237 lines) — Split/compound payment form
> - `legacy_code/frmPlacanjePo.vb` (95 lines) — Per-person payment calculation
> - `legacy_code/frmPlacanjeTarifa.vb` (85 lines) — Tariff editing grid
> - `legacy_code/frmPlacproc.vb` (51 lines) — Per-night calculation (insurance/tax)

> **Cross-references:**
> - `LEGACY_ANALYSIS/00_DATABASE_SCHEMA.md` — Database schema
> - `LEGACY_ANALYSIS/02_MODULEKOD_FUNCTIONS.md` — ModuleKod business logic

---

## 1. Business Flow

### 1.1 Main Payment Flow (frmPlacanje)

1. **Form Load** — Loads active guest-room stays (`relgostsoba` WHERE `odjavljen=0 AND rezervacija=0`), expense types (`troskovivrste`), room types (`sobavrsta`), currency rates (`kursna`), reservation groups (`rezervacijegrupe`), and settings (`setings`)
   - `frmPlacanje.vb:25-97` — Guest/room data loading queries
   - `frmPlacanje.vb:172` — Currency rates
   - `frmPlacanje.vb:596` — Expense types (`troskovivrste` WHERE `del=0`)
   - `frmPlacanje.vb:457` — Room types

2. **Guest Selection** — When a guest is selected, load their expenses, insurance/tax data, and existing folio information
   - `frmPlacanje.vb:308` — Load expenses: `troskovi` JOIN `troskovivrste` WHERE `zaklj=0`

3. **Cost Item Display** — Expenses shown in grid with columns: name, quantity, amount, discount, PDV rate
   - Insurance (`osig`) and tourist tax (`taxa`) calculated per person per day from `setings` table
   - `frmPlacanje.vb:1208` — Room type lookup for tariff calculation

4. **Payment Recording** — On "Pay" button:
   - Get next receipt number: `SELECT MAX(placanje.broj) FROM placanje` (`frmPlacanje.vb:1262`)
   - Insert payment header into `placanje` table (`frmPlacanje.vb:4027`)
   - Insert payment detail lines into `placanjedetalji` table (`frmPlacanje.vb:3825`)
   - Mark expenses as closed: `UPDATE troskovi SET zaklj=1, Brrac={receipt#} WHERE ID={id}` (`frmPlacanje.vb:3808, 3550`)
   - Mark accommodation nights: `UPDATE nocenja SET PrijavaOdjava=1` (`frmPlacanje.vb:3568, 4256`)
   - Close folio: `UPDATE posjetaFolio SET vrijemeO={datetime}, zakljucen=1 WHERE ID={PID}` (`frmPlacanje.vb:4268`)

5. **Print/Invoice Flow** — Generates receipt data and optionally prints to fiscal device:
   - Insert into `printracuni`/`printracunidetalji` for printed receipts (`frmPlacanje.vb:3172, 3268`)
   - Insert into `printracunifooter` for receipt footer data (`frmPlacanje.vb:3229`)
   - Fiscal device dispatch based on `setings.fiscal` setting (`frmPlacanje.vb:2506-2512`):
     - `fsc(0)=1` → `FMRacun()` (custom file-based receipt)
     - `fsc(0)=2` → `FMRacunNSC()` (NSC fiscal printer)
     - `fsc(0)=3` → `FMRacuntring()` (Tring fiscal printer via `Tring.Fiscal.Driver`)
     - `fsc(0)=5` → `FMRacunE()` (ELN fiscal printer)
     - `fsc(0)=6` → `FMRacunMikroelektornika()` (Mikroelektronika fiscal)
     - `fsc(0)=7` → `FMRacunHCP()` (HCP fiscal printer)

6. **Checkout (Odjava)** — On guest departure:
   - `UPDATE nocenja SET PrijavaOdjava=1 WHERE SID={roomID} AND PID={folioID}` (`frmPlacanje.vb:4256`)
   - `UPDATE relgostsoba SET odjavljen=1, checkOutDate={now}, checkOutRadnik={worker} WHERE sobaID={roomID} AND odjavljen=0` (`frmPlacanje.vb:4260`)
   - `UPDATE Avans SET placeno=1 WHERE brSobe={roomID} AND placeno=0` (`frmPlacanje.vb:4264`)
   - `UPDATE posjetaFolio SET vrijemeO={now}, zakljucen=1 WHERE ID={PID}` (`frmPlacanje.vb:4268`)
   - `UPDATE troskovi SET zaklj=1 WHERE SID={roomID}` (`frmPlacanje.vb:4295`)
   - `UPDATE sobe SET clean=0 WHERE ID={roomID}` (`frmPlacanje.vb:4211`)
   - All executed within a SQL transaction (`frmPlacanje.vb:4250-4273`)

### 1.2 Payment Receipt Form (frmPlati1)

1. **Load** — Selects active guests and rooms, loads payment history from `printracuni`, loads `setings` and `kursna` (exchange rates)
   - `frmPlati1.vb:604` — Guest/room list: `sobe JOIN relgostsoba JOIN gosti WHERE odjavljen=0`
   - `frmPlati1.vb:608` — Distinct rooms with active guests
   - `frmPlati1.vb:623` — Existing receipts: `SELECT BrojRacuna, concat(...), storno, datr FROM printracuni ORDER BY BrojRacuna DESC`

2. **PDV/Tax Calculation**:
   - Per-item tax: `bezpdvd = (ukupno - tax*qty) / (pdv%/100 + 1)` (`frmPlati1.vb:32`)
   - PDV amount: `pdvizd = ukupno - tax*qty - bezpdvd` (`frmPlati1.vb:33`)
   - Insurance: `numOsiguranje * setings.osig` (`frmPlati1.vb:440`)
   - Tourist tax: `numTaksa * setings.taxa` (`frmPlati1.vb:448`)
   - PDV toggle: `CheckBox5.Checked` controls whether PDV is included (`frmPlati1.vb:506-507`)
   - Tax toggle: `CheckBox1.Checked` controls whether tourist tax is applied (`frmPlati1.vb:509`)

3. **Payment Recording** — Full `placanje` INSERT with all 40+ columns (`frmPlati1.vb:511`):
   - `INSERT INTO placanje(broj, relGostSobaID, iznos, popust, datum, nacin, radnikID, naziv, PID, uplaceno, brdana, datumOD, datumDO, placanjeID, poslovna, storno, folio, idgost, predracun, posjeta, firma, tip, racn, racime, pdv, ctax, sobar, perio, veza, napom, napokraj, napomena, fiskalni, fiskal, fiskalizn, fiskalvr, fiskalrek, fiskalnrekvr, placnaz, uplatetex, hotelid, idd)`
   - Also inserts detail lines into `placanjedetalji` (`frmPlati1.vb:522`)

### 1.3 Split Payment (frmPlacanjeSlozeno)

1. **Load** — Loads payment methods: `SELECT ID, nacin FROM placanjenacin WHERE id<>5` (`frmPlacanjeSlozeno.vb:30`)
   - ID=5 ("Slozeno"/compound) is excluded to prevent recursive split payments

2. **Add Payment** — Adds a row to in-memory `ds.Tables("PlacanjaSlozena")` DataTable (`frmPlacanjeSlozeno.vb:62-113`)
   - Stores: ID, RBR, iznos, nacin
   - Calculates remaining: `razlika = ostalo - uplaceno` (`frmPlacanjeSlozeno.vb:205`)
   - Warns if overpayment attempted (`frmPlacanjeSlozeno.vb:207`)
   - Closes only when remaining = 0 (`frmPlacanjeSlozeno.vb:225`)

3. **Persist** — Called from `frmPlacanje.placanje_slozeno()` which iterates all rows and calls stored procedure `addPlacanjeSlozeno` for each (`frmPlacanje.vb:4973-5026`)

### 1.4 Per-Person Payment (frmPlacanjePo)

1. **Calculation** — Divides total by number of persons: `cij = ukupno / brda` (`frmPlacanjePo.vb:11, 21`)
   - Reverse: `ukupno = cij * brda` (`frmPlacanjePo.vb:40`)
2. **Insurance/Tax** — Opens `frmPlacproc` for detailed calculation (`frmPlacanjePo.vb:73`)
3. **Permission check** — `dozvole(8).ToString = "1"` disables discount checkbox (`frmPlacanjePo.vb:91`)

### 1.5 Tariff Editing (frmPlacanjeTarifa)

1. **Display** — Receives `DataTable` of nights with tariffs, shows in editable grid
2. **Save** — Iterates grid rows, runs `UPDATE nocenja SET Tarifa = '{tar}' WHERE ID = {id}` for each changed row (`frmPlacanjeTarifa.vb:46`)
   - Uses string replacement for decimal formatting: replace `.` with empty, `,` with `.` (`frmPlacanjeTarifa.vb:44-45`)
   - **SQL INJECTION RISK**: Direct string interpolation in SQL

### 1.6 Per-Night Calculation (frmPlacproc)

1. **Calculation** — Mostly commented out (`izracun()` calls are all commented)
2. **Active formula** (`frmPlacproc.vb:21-33`):
   - Insurance per person: `brda * brosig * setings.osig` (line 28)
   - Tourist tax per person: `brda * brosig * setings.taxa` (line 29)
   - Total surcharge: `osiguranje + taksa` (line 30)
   - Net amount: `n = ukupno - (osiguranje + taksa)` (line 31)
   - Discount %: `Abs(((cijena - surcharge) - net) / net * 100)` (line 32)

---

## 2. SQL Inventory

### 2.1 SELECT Operations

| Line | File | Tables | Columns | WHERE | Purpose |
|------|------|--------|----------|-------|---------|
| 25,27 | frmPlacanje.vb | gosti JOIN relgostsoba JOIN sobe | ID, imePrezime, checkInDate, checkOutDate, nazivSobe, SobaID, PID, tarifaID, popust, popustrazlog, status, tid, grupaID | odjavljen=0 AND rezervacija=0 | Load active guests |
| 94,96 | frmPlacanje.vb | gosti JOIN relgostsoba JOIN sobe | (subset of above) | odjavljen=0 AND rezervacija=0 [+ grupaID filter] | Load guests by group |
| 172 | frmPlacanje.vb | kursna | Naziv_Valute, Vrijednost | (none) | Load exchange rates |
| 308 | frmPlacanje.vb | relgostsoba JOIN sobe JOIN troskovi JOIN troskovivrste | ID, nazivSobe, SobaID, PID, IDtroska, TID, naziv, napomena, Trosak, iznos, kolicina, status, grupaID, kol, per, popust, razlog, pop, idt, Nacin, Valuta, OznakaValute | odjavljen=0 AND rezervacija=0 AND zaklj=0 AND ID={ime_id} | Load expenses for guest |
| 457 | frmPlacanje.vb | sobavrsta | ID, naziv | (none) | Load room types |
| 596 | frmPlacanje.vb | troskovivrste | ID, naziv, sifra | del=0 | Load expense types |
| 627 | frmPlacanje.vb | rezervacijegrupe | ID, naziv | odjavljena=0 | Load reservation groups |
| 1208 | frmPlacanje.vb | sobe | vrsta | ID={SID} | Get room type |
| 1262 | frmPlacanje.vb | placanje | MAX(broj) | (none) | Get next receipt number |
| 3135 | frmPlacanje.vb | placanje | broj | broj={br} | Check receipt number exists |
| 4173 | frmPlacanje.vb | placanje | * | broj=@br | Verify receipt number |
| 4335 | frmPlacanje.vb | printracuniavans | MAX(brojracuna) | (none) | Get next advance receipt number |
| 5228 | frmPlacanje.vb | relgostsoba JOIN sobe JOIN troskovi JOIN troskovivrste | (same as line 308) | ID=0 (placeholder for empty grid) | Initialize empty expense grid |
| 6055 | frmPlacanje.vb | sobe | ID | naziv={broj} | Get room ID by name |
| 6256 | frmPlacanje.vb | sifarnik | MAX(sifra) | (none) | Get next codebook number |
| 30 | frmPlacanjeSlozeno.vb | placanjenacin | ID, nacin | id<>5 | Load payment methods (excluding compound) |
| 46 | frmPlacanjeTarifa.vb | (implicit) | (via loa parameter) | (n/a) | Grid data passed from caller |
| 316 | frmPlati1.vb | printracuni | MAX(BrojRacuna) | (none) | Get next receipt number |
| 372 | frmPlati1.vb | kursna | Naziv_Valute, Vrijednost | (none) | Load exchange rates |
| 495 | frmPlati1.vb | placanje | broj | broj={receipt#} | Check duplicate receipt number |
| 533,537 | frmPlati1.vb | printracuni | Tip, Iznos, BrojRacuna, Ime, DrugoIme, BrojSobe, datr, rad | brojracuna={brojid} | Load receipt data for display |
| 604 | frmPlati1.vb | sobe JOIN relgostsoba JOIN gosti | naziv, gostID, sobaID, PID, naziv | odjavljen=0 | Load active guest-room list |
| 608 | frmPlati1.vb | sobe JOIN relgostsoba | id, naziv | odjavljen=0 GROUP BY id, naziv | Load distinct rooms with guests |
| 623 | frmPlati1.vb | printracuni | BrojRacuna, concat(...), storno, datr | ORDER BY BrojRacuna DESC | Load receipt list |

### 2.2 INSERT Operations

| Line | File | Table | Columns | Purpose |
|------|------|-------|---------|---------|
| 2551 | frmPlacanje.vb | printracspec | broj, ime, frima, dadtum, datumodj, soba, vrsobe, napomnena, veza, vr_upis, tarif, d1, d2, folio | Insert special print invoice record |
| 3170 | frmPlacanje.vb | printracuniavans | BrojRacuna, Poslovna, Ime, DrugoIme, PeriodOd, PeriodDo, TipPlacanja, BrojSobe, datr, peri, racin, napo | Insert advance receipt header |
| 3172 | frmPlacanje.vb | printracuni | idkl, grupa, knj, BrojRacuna, Poslovna, Ime, DrugoIme, PeriodOd, PeriodDo, TipPlacanja, BrojSobe, datr, peri, racin, napo, rad, dat, printime | Insert receipt header |
| 3229 | frmPlacanje.vb | printracunifooter | BrojRacuna, Avansno, nocenja, nap, pri | Insert receipt footer |
| 3265 | frmPlacanje.vb | printracunidetaljiavans | BrojRacuna, Trosak, Kol, CijBezPdv, UkupnoBezPdv, Pdv, IznosPdv, Ukupno, Nacin, Valuta, OznakaValute, Popust, razlogp, pop, trosakId | Insert advance receipt detail line |
| 3268 | frmPlacanje.vb | printracunidetalji | BrojRacuna, nacinid, Trosak, Kol, CijBezPdv, UkupnoBezPdv, Pdv, IznosPdv, Ukupno, Nacin, Valuta, OznakaValute, Popust, popust1, razlogp, pop, trosakId | Insert receipt detail line |
| 3569 | frmPlacanje.vb | nocenja | RID, DatumP, SID, PID, PrijavaOdjava, Tarifa, popust, opis, soba | Insert checkout night record (copy active nights) |
| 3819 | frmPlacanje.vb | placanjedetalji | brojID, art, kolicina, cijena, iznos, brojnocenja, PID | Insert payment detail (simplified) |
| 3822 | frmPlacanje.vb | troskovi | GSID=0, SID, TID=1, vrijeme, kolicina, iznos, radnikID=1, napomena, zaklj=1, Brrac | Insert accommodation charge from payment |
| 3825 | frmPlacanje.vb | placanjedetalji | brojID, art, kolicina, cijena, iznos, napomena, brojnocenja, PID, ranijeUplate | Insert payment detail (full) |
| 3917 | frmPlacanje.vb | placanjedetalji | brojID, art, kolicina, cijena, iznos, napomena, brojnocenja, PID, ranijeUplate | Insert payment detail (alternate path) |
| 4027 | frmPlacanje.vb | placanje | broj, relgostsobaID, iznos, popust=0, datum, nacin, radnikID=1, naziv, PID, uplaceno, brdana, datumOD, datumDO, poslovna, firma | Insert payment header (simplified) |
| 4143 | frmPlacanje.vb | placanje | (same as 4027) | Insert payment header (alternate path) |
| 4382 | frmPlacanje.vb | printracuniavans | (full 30+ columns via string concat) | Insert advance receipt (advance payment) |
| 4940 | frmPlacanje.vb | partneri | naziv, ulica, idd, pdv | Insert business partner/firm |
| 511 | frmPlati1.vb | placanje | (40+ columns via string concat) | Insert payment header (full) |
| 522 | frmPlati1.vb | placanjedetalji | (25 columns via backtick notation) | Insert payment detail lines |
| 6214 | frmPlacanje.vb | sifarnik | naziv, kol, cij, ukupno, sifra, porez, racu, racun, dod, dod1, dod2, dod3, placanje | Insert codebook entry |

### 2.3 UPDATE Operations

| Line | File | Table | SET | WHERE | Purpose |
|------|------|-------|-----|-------|---------|
| 3143 | frmPlacanje.vb | printracuni | fisrac, fisvr, fisIZN | BrojRacuna={id} | Save fiscal device response |
| 3550 | frmPlacanje.vb | troskovi | zaklj=1, Brrac={receipt#} | ID={idtroska} | Mark expense as paid |
| 3568 | frmPlacanje.vb | nocenja | PrijavaOdjava=1, datumodj={date}, brrac={receipt#} | SID={roomID} AND PrijavaOdjava=0 | Mark nights as closed |
| 3808 | frmPlacanje.vb | troskovi | zaklj=1, Brrac={@brojrac} | ID={@ID} | Mark expense as paid (parameterized) |
| 3812 | frmPlacanje.vb | troskovi | Djelimicno=1, iznos={@iznosUp} | ID={@IDtroska} | Mark expense as partially paid |
| 4211 | frmPlacanje.vb | sobe | clean=0 | ID={roomID} | Mark room as dirty after checkout |
| 4256 | frmPlacanje.vb | nocenja | PrijavaOdjava=1 | SID=@SID AND PID=@PID | Close accommodation nights on checkout |
| 4260 | frmPlacanje.vb | relgostsoba | odjavljen=1, checkOutDate=@date, checkOutRadnik=@radnik | sobaID=@SID AND odjavljen=0 | Check out guest |
| 4264 | frmPlacanje.vb | Avans | placeno=1 | brSobe=@SID AND placeno=0 | Mark advances as paid on checkout |
| 4268 | frmPlacanje.vb | posjetaFolio | vrijemeO=@datumO, zakljucen=@zakljucen | ID=@PID | Close folio on checkout |
| 4295 | frmPlacanje.vb | troskovi | zaklj=1 | SID={roomID} | Close all room expenses |
| 6108 | frmPlacanje.vb | troskovi | zaklj=1, Brrac={receipt#} | ID={idtroska} | Mark individual expense as paid |
| 46 | frmPlacanjeTarifa.vb | nocenja | Tarifa='{tar}' | ID={id} | Update tariff on individual night |

### 2.4 DELETE Operations

No DELETE operations found in any of the 6 payment forms. Expenses and payments are marked/closed, never deleted.

---

## 3. Database Writes

### 3.1 Transactional Writes (with BeginTransaction/Commit/Rollback)

| Location | Tables | Rollback on Error |
|----------|--------|-------------------|
| frmPlacanje.vb:4020-4038 | placanje | Yes |
| frmPlacanje.vb:3798-4167 | placanjedetalji, troskovi, troskoviPojedinacni, placanje | Yes |
| frmPlacanje.vb:4229-4287 | nocenja, relgostsoba, Avans, posjetaFolio | Yes (checkout transaction) |

### 3.2 Non-Transactional Writes (no transaction)

| Location | Table | Risk |
|----------|-------|------|
| frmPlacanje.vb:2551 | printracspec | Medium — no rollback if fails |
| frmPlacanje.vb:3170-3268 | printracuniavans, printracuni, printracunifooter, printracunidetalji | High — multi-insert without transaction |
| frmPlacanje.vb:4382 | printracuniavans | Medium — advance receipt insert |
| frmPlacanje.vb:4940 | partneri | Low — single insert |
| frmPlati1.vb:511 | placanje | High — 40+ column insert via string concat |
| frmPlati1.vb:522 | placanjedetalji | High — uses backtick notation (broken SQL) |
| frmPlacanje.vb:3143 | printracuni (UPDATE) | Low — fiscal response update |
| frmPlacanje.vb:6214 | sifarnik | Low — codebook insert |
| frmPlacanjeTarifa.vb:46 | nocenja (UPDATE) | Medium — per-row update in loop, no transaction |

---

## 4. Payment Types / Status Codes

### 4.1 Payment Methods (`placanjenacin` table)

| ID | Nacin (Method) | Evidence |
|----|----------------|----------|
| 1 | Gotovina (Cash) | `00_DATABASE_SCHEMA.md:642` |
| 2 | Virman (Transfer) | `00_DATABASE_SCHEMA.md:642` |
| 3 | Kartica (Card) | `00_DATABASE_SCHEMA.md:642` |
| 4 | Gratis | `00_DATABASE_SCHEMA.md:642` |
| 5 | Slozeno (Compound/Split) | `00_DATABASE_SCHEMA.md:642`, excluded in `frmPlacanjeSlozeno.vb:30` |

### 4.2 Payment `placanjeID` Values

| Value | Meaning | Evidence |
|-------|---------|----------|
| 1 | Regular payment | `frmPlati1.vb:511` (hardcoded `placanjeID`, value from `cmbNacin.SelectedValue`) |

### 4.3 Storno Status (`placanje.storno` / `printracuni.storno`)

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Normal/active | `frmPlacanje.vb:4382` (storno=0 in advance receipt INSERT) |
| 1 | Cancelled/storno | `02_MODULEKOD_FUNCTIONS.md:557` (advance receipt storno) |

### 4.4 Expense Status (`troskovi.zaklj`)

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Open/unpaid | `frmPlacanje.vb:308` (WHERE zaklj=0) |
| 1 | Closed/paid | `frmPlacanje.vb:3808, 4295` (SET zaklj=1) |

### 4.5 Folio Status (`posjetafolio.zakljucen`)

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Open folio | `00_DATABASE_SCHEMA.md:686` |
| 1 | Closed folio | `frmPlacanje.vb:4246, 4268` |

### 4.6 Guest Checkout Status (`relgostsoba.odjavljen`)

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Currently checked in | `frmPlacanje.vb:25` (WHERE odjavljen=0) |
| 1 | Checked out | `frmPlacanje.vb:4260` (SET odjavljen=1) |

### 4.7 Night Record Status (`nocenja.PrijavaOdjava`)

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Active accommodation night | `frmPlacanje.vb:3569` (INSERT PrijavaOdjava=0) |
| 1 | Closed/checked-out night | `frmPlacanje.vb:3568, 4256` (SET PrijavaOdjava=1) |

### 4.8 Fiscal Device Types (from `setings.fiscal`)

| Code | Device | Evidence |
|------|--------|----------|
| 1 | Custom file-based receipt (FMRacun) | `frmPlacanje.vb:2506` |
| 2 | NSC fiscal printer (FMRacunNSC) | `frmPlacanje.vb:2508` |
| 3 | Tring fiscal printer (`Tring.Fiscal.Driver`) | `frmPlacanje.vb:2509, 2824` |
| 5 | ELN fiscal printer (FMRacunE) | `frmPlacanje.vb:2510`, `ModuleKod:2969` |
| 6 | Mikroelektronika fiscal printer | `frmPlacanje.vb:2511` |
| 7 | HCP fiscal printer | `frmPlacanje.vb:2512` |

### 4.9 PDV/VAT Rate Flags (`placanje.pdv`)

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Not a VAT registrant (PDV exempt) | `frmPlati1.vb:506-507` (CheckBox5), line 642 |
| 1 | VAT registrant (standard PDV applies) | `frmPlati1.vb:507` |

### 4.10 Tourist Tax Flag (`placanje.ctax`)

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | No tourist tax | `frmPlati1.vb:509` (CheckBox1 not checked) |
| 1 | Tourist tax applied | `frmPlati1.vb:509` (CheckBox1 checked) |

### 4.11 Room Clean Status (after checkout)

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Dirty (needs cleaning) | `frmPlacanje.vb:4211` (SET clean=0) |
| 1 | Clean | `sobe` table default |

---

## 5. Business Rules

### 5.1 Payment Number Generation
- Receipt number is `MAX(placanje.broj) + 1` (`frmPlacanje.vb:1262`)
- **RACE CONDITION**: No locking or transaction on number generation — concurrent payments may get same number

### 5.2 Expense Closure on Payment
- When a payment covers an expense, `troskovi.zaklj` is set to 1 and `troskovi.Brrac` is set to the receipt number (`frmPlacanje.vb:3808`)
- For partial payments, `troskovi.Djelimicno=1` and `iznos` is adjusted to the remaining amount (`frmPlacanje.vb:3812`)

### 5.3 Insurance and Tourist Tax Calculation
- Insurance: `persons * nights * setings.osig` (`frmPlacproc.vb:28`)
- Tourist tax: `persons * nights * setings.taxa` (`frmPlacproc.vb:29`)
- These are subtracted from the total before PDV calculation (`frmPlati1.vb:32`)

### 5.4 PDV (VAT) Calculation
- Base formula: `bezpdvd = (ukupno - tax*qty) / (pdv%/100 + 1)` (`frmPlati1.vb:32`)
- PDV amount: `pdvizd = ukupno - tax*qty - bezpdvd` (`frmPlati1.vb:33`)
- If `CheckBox5.Checked = False` (not VAT registrant), PDV amount is subtracted from total: `txtUkupno.Text -= pdvbez` (`frmPlati1.vb:44`)
- PDV rate comes from `setings.pdv` (`frmPlati1.vb:75, 103, 133, 155, 178`)
- Alternative rates: `setings.pdvo` (line 207), `setings.pdvtr` (line 247)

### 5.5 Fiscal Receipt Tax Rate Mapping (Tring Driver)
- If PDV exempt: `Tring.Fiscal.Driver.VrstePoreskihStopa.A_Nulta_stopa_za_neregistrirane_obveznike` (`frmPlacanje.vb:2904`)
- If reduced rate: `Tring.Fiscal.Driver.VrstePoreskihStopa.K_Poreska_stopa_PDV_za_artikle_oslobodjene_PDV` (`frmPlacanje.vb:2907`)
- If standard rate: `Tring.Fiscal.Driver.VrstePoreskihStopa.E_Opca_poreska_stopa_PDV` (`frmPlacanje.vb:2910`)

### 5.6 Fiscal Payment Method Mapping (Tring Driver)
- Cash: `Tring.Fiscal.Driver.VrstePlacanja.Gotovina` (`frmPlacanje.vb:2958`)
- Card: `Tring.Fiscal.Driver.VrstePlacanja.Kartica` (`frmPlacanje.vb:2960`)
- Transfer: `Tring.Fiscal.Driver.VrstePlacanja.Virman` (`frmPlacanje.vb:2962`)
- Default (fallback): Cash (`frmPlacanje.vb:2965`)

### 5.7 Per-Person Payment Split
- Total / number of persons = per-person amount (`frmPlacanjePo.vb:11,21`)
- Validates: `If IsNumeric(txtuku.Text) = False Then txtuku.Text = 1` (`frmPlacanjePo.vb:10`)
- Translation: `txtcij.Text = txtuku.Text / txtbrda.Text`

### 5.8 Split Payment Logic (Compound)
- User can split a single payment across multiple payment methods (cash + card + transfer)
- Each split stored via `addPlacanjeSlozeno` stored procedure (`frmPlacanje.vb:4996`)
- Remaining amount tracked: `ostalo - uplaceno` (`frmPlacanjeSlozeno.vb:205`)
- User is warned if they try to overpay (`frmPlacanjeSlozeno.vb:207`)
- Form only closes when remaining = 0 (`frmPlacanjeSlozeno.vb:225`)
- **Payment method ID=5 (Slozeno) is excluded from selection** (`frmPlacanjeSlozeno.vb:30`)

### 5.9 Tariff Editing Access Control
- Permission check: `dozvole(4).ToString = "0"` blocks tariff editing (`frmPlacanjeTarifa.vb:71`)
- Logged: `izmjena tarife` action with old value (`frmPlacanjeTarifa.vb:39`)

### 5.10 Advance Payment (Avans) Flow
1. User checks `chkAvans` checkbox and enters amount in `txtAvans`
2. On payment: `avans_print()` generates receipt XML, then prints via Crystal Reports (`frmPlacanje.vb:5042-5047`)
3. Advance receipt inserted into `printracuniavans` with `storno=0` (`frmPlacanje.vb:4382`)

### 5.11 Receipt Number Validation
- `imaliBrojRacuna()` checks if receipt number already exists in `placanje` table (`frmPlacanje.vb:4171-4202`)
- `frmPlati1.vb:497` also checks duplicate receipt numbers in `placanje`

### 5.12 Checkout Transaction (all-or-nothing)
- Checkout is wrapped in `BeginTransaction`/`Commit`/`Rollback` (`frmPlacanje.vb:4250-4287`)
- Steps within transaction:
  1. Close nights: `UPDATE nocenja SET PrijavaOdjava=1` (`frmPlacanje.vb:4256`)
  2. Check out guest: `UPDATE relgostsoba SET odjavljen=1` (`frmPlacanje.vb:4260`)
  3. Close advances: `UPDATE Avans SET placeno=1` (`frmPlacanje.vb:4264`)
  4. Close folio: `UPDATE posjetaFolio SET zakljucen=1` (`frmPlacanje.vb:4268`)
- After successful transaction: mark room as dirty (`frmPlacanje.vb:4211`, but called separately, outside transaction)

### 5.13 Restaurant Expense Filtering for Fiscal
- Items with name "Restoran" are excluded from fiscal printing: `"Usluga sa nazivom 'Restoran' nece biti stampana fiskalno!"` (`frmPlacanje.vb:5291`)

---

## 6. Cross-Reference

### 6.1 ModuleKod Functions Called from Payment Forms

| Function | Called From | Line | Purpose |
|----------|-------------|------|---------|
| `mysqlReader()` | frmPlacanje.vb | 596 | Load expense types |
| `mysqlReader()` | frmPlati1.vb | 372, 495, 533, 537, 604, 608, 623 | Load exchange rates, check receipt, load guests |
| `mysqlExScalar()` | frmPlacanje.vb | 3550, 3568, 3569, 3808, 4295 | Mark expenses/nights as closed |
| `mysqlExScalar()` | frmPlati1.vb | 511, 522 | Insert payment and details |
| `mysqlExScalarLast()` | frmPlacanje.vb | 3822 | Insert troskovi and get ID |
| `avans_print()` | frmPlacanje.vb | 5042 | Print advance receipt |
| `FMRacun()` | frmPlacanje.vb | 2506 | Fiscal receipt (type 1) |
| `FMRacunNSC()` | frmPlacanje.vb | 2508 | Fiscal receipt (type 2, NSC) |
| `FMRacuntring()` | frmPlacanje.vb | 2509, 2819-2999 | Fiscal receipt (Tring driver) |
| `FMRacunE()` | frmPlacanje.vb | 2510 | Fiscal receipt (type 5, ELN) |
| `FMRacunMikroelektornika()` | frmPlacanje.vb | 2511, 3001 | Fiscal receipt (type 6) |
| `FMRacunHCP()` | frmPlacanje.vb | 2512, 3043 | Fiscal receipt (type 7) |
| `snimFiskal()` | ModuleKod.vb | 3107-3143 | Save fiscal response to DB |
| `addPlacanjeSlozeno` (SP) | frmPlacanje.vb | 4996 | Insert split payment row |

### 6.2 Database Tables Used by Payment Forms

| Table | Operations | Forms |
|-------|-----------|-------|
| `placanje` | SELECT, INSERT | frmPlacanje, frmPlati1 |
| `placanjedetalji` | INSERT | frmPlacanje, frmPlati1 |
| `placanjenacin` | SELECT | frmPlacanjeSlozeno |
| `placanjeslozeno` | INSERT (via SP) | frmPlacanje |
| `printracuni` | SELECT, INSERT, UPDATE | frmPlacanje, frmPlati1 |
| `printracunidetalji` | INSERT | frmPlacanje |
| `printracunidetaljiavans` | INSERT | frmPlacanje |
| `printracuniavans` | SELECT, INSERT | frmPlacanje |
| `printracunifooter` | INSERT | frmPlacanje |
| `printracspec` | INSERT | frmPlacanje |
| `relgostsoba` | SELECT, UPDATE | frmPlacanje, frmPlati1 |
| `gosti` | SELECT | frmPlacanje, frmPlati1 |
| `sobe` | SELECT, UPDATE | frmPlacanje, frmPlati1 |
| `sobavrsta` | SELECT | frmPlacanje |
| `troskovi` | SELECT, UPDATE, INSERT | frmPlacanje |
| `troskovivrste` | SELECT | frmPlacanje |
| `troskovipojedinacni` | INSERT | frmPlacanje |
| `nocenja` | UPDATE | frmPlacanje, frmPlacanjeTarifa |
| `posjetafolio` | UPDATE | frmPlacanje |
| `kursna` | SELECT | frmPlacanje, frmPlati1 |
| `setings` | SELECT | frmPlacanje, frmPlati1, frmPlacproc |
| `rezervacijegrupe` | SELECT | frmPlacanje |
| `partneri` | INSERT | frmPlacanje |
| `Avans` | UPDATE | frmPlacanje |
| `sifarnik` | SELECT, INSERT | frmPlacanje |
| `fisc` | (implicit via fiscal) | frmPlacanje |

### 6.3 Stored Procedures Used

| SP Name | Called From | Line | Purpose |
|---------|-------------|------|---------|
| `addPlacanjeSlozeno` | frmPlacanje.vb | 4996 | Insert compound payment breakdown row |

---

## 7. Key Findings

### 7.1 CRITICAL BUGS

1. **SQL Injection in frmPlacanje.vb (line 4382)** — The `printracuniavans` INSERT uses raw string concatenation with `txtBrojRacuna.Text`, `cmbRacunIme.Text`, `txtDrugoIme.Text`, etc. User input is not parameterized.

2. **SQL Injection in frmPlati1.vb (lines 511, 522)** — The `placanje` and `placanjedetalji` INSERTs use string concatenation with control values. The `placanjedetalji` insert at line 522 uses backtick notation which may be syntactically broken (column references without values).

3. **SQL Injection in frmPlacanjeTarifa.vb (line 46)** — `UPDATE nocenja SET Tarifa = '{tar}' WHERE ID = {id}` — direct string interpolation without parameterization.

4. **SQL Injection in frmPlacanje.vb (line 211, 2550, 6055, etc.)** — Multiple locations use `& sid.ToString`, `& txtBrojRacuna.Text`, etc. in SQL commands.

5. **Transaction Scope Gaps in frmPlacanje.vb** — The payment save function (`SnimiPlacanje`, ~lines 3700-4170) uses transactions for some operations but not others. Multiple INSERT statements to `printracunidetalji`, `printracunifooter`, `printracunidetaljiavans` (lines 3170-3268) are NOT within a transaction. If a later insert fails, earlier inserts remain.

6. **Race Condition on Receipt Number** — `SELECT MAX(placanje.broj)+1` (line 1262) is not atomic. Two concurrent users could receive the same receipt number.

### 7.2 DESIGN ISSUES

7. **Commented-Out / Broken Code in frmPlati1.vb** — Lines 493-494 contain commented-out `SELECT` statements showing the intended column list for `placanje` and `placanjedetalji`. The actual INSERT at line 511 uses column names as values (e.g., `\`relGostSobaID\`` instead of an actual value), which suggests **incomplete or broken implementation**. The form's payment button handler has an early `Return` at line 492 that skips the entire payment logic.

8. **Hardcoded radnikID=1** — `frmPlacanje.vb:4027` uses `radnikID=1` instead of the logged-in worker ID. This means all payments from `frmPlacanje.vb` are attributed to worker ID 1 regardless of who processed them.

9. **Fiscal Device Not Ready Handling** — `frmPlacanje.vb:2833-2837` shows an infinite retry loop with `MsgBox` that blocks the UI thread if the fiscal device is not ready. Only offers retry, no graceful fallback.

10. **Avans table** — Line 4264 uses `UPDATE Avans SET placeno=1 WHERE brSobe=@SID AND placeno=0`, but the `Avans` table was **dropped in schema migration** (`ModuleKod.vb:2063`). This UPDATE will fail if the table doesn't exist.

11. **Per-Row SQL in Tariff Editing** — `frmPlacanjeTarifa.vb` executes a separate `UPDATE` for every changed row in the grid (line 27-31), with no transaction or batching. If one update fails mid-way, data is in an inconsistent state.

12. **Missing Error Handling** — Several database operations in `frmPlacanje.vb` (lines 3170-3268, receipt printing inserts) have `Try/Catch` blocks that just show a message box and close the form, with no rollback or data cleanup.

13. **Duplicate Payment Insert Logic** — `frmPlacanje.vb` has two nearly identical payment INSERT paths (lines 4027 and 4143) in different code branches, creating maintenance risk.

### 7.3 OBSERVATIONS

14. **frmPlacproc Is Mostly Dead** — `frmPlacproc.vb` has 51 lines, and all `izracun()` calls except the button click are commented out. The form calculates insurance/tax but only from `setings.osig` and `setings.taxa` per person.

15. **frmPlacanjePo Is Minimal** — `frmPlacanjePo.vb` has 95 lines and only calculates `total / persons = per-person amount`. Opens `frmPlacproc` for tax calculation. Permission check at line 91 disables discount for `dozvole(8)=1`.

16. **frmPlacanjeSlozeno Is In-Memory Only** — The split payment form only stores data in a `DataTable` (`ds.Tables("PlacanjaSlozena")`). Persistence to DB happens via `addPlacanjeSlozeno` stored procedure called from `frmPlacanje.placanje_slozeno()`. The original SP call code in `frmPlacanjeSlozeno.vb` is entirely commented out (lines 69-111).

17. **Expense Type ID=1 Is Accommodation** — `troskovi` INSERT at `frmPlacanje.vb:3822` always uses `TID=1` for the accommodation charge created during payment, consistent with the `troskovivrste` seed data where `tip=1` = "Nocenje sa doruckom" (`ModuleKod.vb:580`).

18. **Multiple Fiscal Device Integrations** — The system supports 6 different fiscal device types (codes 1,2,3,5,6,7), each with its own printing method. The Tring driver integration (`Tring.Fiscal.Driver`) is the most complete with full item-by-item receipt generation, payment method mapping, and buyer identification (lines 2819-2999).

19. **Advance Payment (Avans) Flow Is Separate** — Advance payments are stored in a separate table (`printracuniavans`) with their own column structure, not in the main `placanje` table.

20. **Checkout Process Is Multi-Step** — Checkout requires 5 database updates (nights, guest, advances, folio, expenses) plus a room status update, all within a transaction. The room status update (`clean=0`) is **outside** the transaction.