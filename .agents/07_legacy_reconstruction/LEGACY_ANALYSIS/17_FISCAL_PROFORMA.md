# Legacy Fiscal & Proforma Invoice Analysis

## 1. Business Flow: Fiscal Device Integration

### 1.1 Fiscal Configuration (`setings.fiscal` column)

The `fiscal` column in the `setings` table stores a `*`-delimited configuration string parsed at runtime:

```
fsc = ds.Tables("setings").Rows(0).Item("fiscal").ToString.Split("*")
```

- **fsc(0)** — Device type code (integer selector)
- **fsc(1)** — Device-specific parameter (NSC operator ID / HCP device name / Microelectronics path)
- **fsc(2)** — Port/path parameter (serial port or file path)
- **fsc(3)** — Target directory (fiscal device shared folder / printer name)
- **fsc(4)** — Optional flag (e.g., `"DP55"` for NSC subtype)
- **fsc(5)** — Optional pricing mode flag (`"1"` = total-based pricing for Tring)

Settings UI maps these fields: `frmpostavke.vb:46-52`

| fsc(0) | Device Type | Key | Integration Method |
|--------|-------------|-----|---------------------|
| 1 | FM (TMT) | File-based | `FMRacun` — writes `FS/FC/FP/FE` format `.in` files to `C:\tmt\` and copies to `Y:\` |
| 2 | NSC (Bosch) | File-based | `FMRacunNSC` — writes NSC protocol (commands 107/52/53/55/56) to `.in` files |
| 3 | Tring | COM/DLL | `FMRacuntring` / `prFiskal` — uses `Tring.Fiscal.Driver.TringFiskalniPrinter` .NET DLL |
| 4 | Tremol | XML file-based | Writes `TremolFpServer` XML commands to shared folder |
| 5 | Eastronics/Eltrade | File-based | `FMRacunE` — writes `S/N/T/J` lines to `rr.eln` file; uses `FileSystemWatcher` for response |
| 6 | Microelectronics | File-based | `FMRacunMikroelektornika` — writes `S/T` format to `rr.in` / `rr.inp`, includes header `E` lines for branding |
| 7 | HCP | XML file-based | `FMRacunHCP` — writes `<RECEIPT>` XML with BCR/VAT/DSC/PRC/AMN attributes; uses CMD.OK trigger file |

### 1.2 Fiscal Command Dispatch (Receipt Printing)

The receipt dispatch happens in `frmPlacanje.vb:2504-2513`:

```
If racunPr = 0 And fsc(0) = 1 Then FMRacun(...)
If racunPr = 0 And fsc(0) = 2 Then FMRacunNSC(...)
If racunPr = 0 And fsc(0) = 3 Then FMRacuntring(...)
If racunPr = 0 And fsc(0) = 5 Then FMRacunE(...)
If racunPr = 0 And fsc(0) = 6 Then FMRacunMikroelektornika(...)
If racunPr = 0 And fsc(0) = 7 Then FMRacunHCP(...)
```

`racunPr = 0` means "not a test receipt." Each receipt function:
1. Iterates `dgw` rows (the invoice line items grid)
2. Maps tax rate string (e.g., `"17%"`) to device-specific tax group codes
3. Handles special PLU codes (Osiguranje=Insurance, Boravisna Taksa=Residence Tax)
4. Writes device-specific command stream to file or API
5. Optionally calls `snimi_trosak()` to record fiscal item codes to DB
6. After printing, calls `snimFiskal()` to store fiscal receipt number back to `printracuni`

### 1.3 Fiscal Device Status & Reports (`frmFiskal`)

`frmFiskal.vb` is a management form for fiscal device operators. It dispatches based on fsc(0):

**Initialization:**
- Type 3: `prFiskal.Inicijalizacija(fsc(3), fsc(2), 0, "0")` — Tring COM init (`frmFiskal.vb:22`)
- Type 7: `provjerihcp()` — creates `CMD_stanje.xml` + `CMD.OK` trigger file (`frmFiskal.vb:29-73`)

**Daily Z-Report (Close Day):**
- Type 2: `dnevni()` — writes `69,1,______,_,__;0;NA;` to `rz.in` (`frmFiskal.vb:597-602`)
- Type 3: `prFiskal.StampatiDnevniIzvjestaj()` (`frmFiskal.vb:587`)
- Type 4: Tremol XML `Report/DailyZ` (`frmFiskal.vb:609-635`)
- Type 5: `Z,1,______,_,__;Z;` to `rr.eln` (`frmFiskal.vb:606`)
- Type 7: HCP XML `Z_REPORT` command (`frmFiskal.vb:126-166`)

**Daily X-Report (Status):**
- Type 2: `69,1,______,_,__;2;;` to `rrX.in` (`frmFiskal.vb:658-664`)
- Type 3: `prFiskal.StampatiPresjekStanja()` (`frmFiskal.vb:642`)
- Type 4: Tremol XML `Report/DailyX` (`frmFiskal.vb:676-698`)
- Type 5: `Z,1,______,_,__;X;` to `rr.eln` (`frmFiskal.vb:651-655`)
- Type 6: Multi-line `E/X/E` format with branding to `dnevnX.inp` (`frmFiskal.vb:665-675`)
- Type 7: HCP XML `X_REPORT` (`frmFiskal.vb:83-125`)

**Periodic Report:**
- Type 2: NSC command 79 with date range (`frmFiskal.vb:718-728`)
- Type 3: `prFiskal.StampatiPeriodicniIzvjestaj()` (`frmFiskal.vb:194`)
- Type 4: Tremol XML `Report/Date` with date range (`frmFiskal.vb:246-276`)
- Type 5: `Z,1,______,_,__;5;` with date range to `rr.eln` (`frmFiskal.vb:277-285`)
- Type 6: Multi-line `E/R/E` format (`frmFiskal.vb:729-742`)
- Type 7: HCP XML `SUMMARY_REPORT` with date range (`frmFiskal.vb:199-244`)

**Duplicate Receipt:**
- Type 2: NSC command 109 (`frmFiskal.vb:393-408`)
- Type 3: `prFiskal.StampatiDuplikatFiskalnogRacuna()` (`frmFiskal.vb:390`)
- Type 4: Tremol XML `PrintDuplicate/Type=0` (`frmFiskal.vb:458-484`)
- Type 5: `E,1,______,_,__;P;` to `rr.eln` (`frmFiskal.vb:409-415`)
- Type 7: HCP XML `RECEIPT_COPY` (`frmFiskal.vb:416-457`)

**Duplicate Z-Report:**
- Type 2: NSC command 73 (`frmFiskal.vb:298-307`)
- Type 3: `prFiskal.StampatiDuplikatDnevnogIzvjestaja()` (`frmFiskal.vb:290`)
- Type 4: Tremol XML `PrintDuplicate/Type=2` (`frmFiskal.vb:356-384`)
- Type 5: `E,1,______,_,__;Z;` to `rr.eln` (`frmFiskal.vb:311-314`)
- Type 7: HCP XML `Z_REPORT_COPY` (`frmFiskal.vb:315-355`)

**Refund Receipt Copy:**
- Type 3: `prFiskal.StampatiDuplikatReklamiranogRacuna()` (`frmFiskal.vb:492`)
- Type 7: HCP XML `REFUND_RECEIPT_COPY` (`frmFiskal.vb:494-535`)

**Cash In/Out:**
- Type 2: NSC command 70 (`frmFiskal.vb:850-858`)
- Type 3: `prFiskal.UnosNovca(Gotovina, iznos)` (`frmFiskal.vb:747`)
- Type 5: `V,1,______,_,__;` + amount to `rr.eln` (`frmFiskal.vb:752-759`)
- Type 6: `I,1,______,_;0;` (cash-in) or `1;` (cash-out) + amount to `depozit.inp` (`frmFiskal.vb:859-871`)
- Type 7: HCP XML `CASH_IN` / `CASH_OUT` (`frmFiskal.vb:760-807`)
- Type 4: Tremol XML `CashIn` (`frmFiskal.vb:808-833`)

**Exit/Logout from Fiscal:**
- Type 2: Sends commands 301, 39, 53+56 (logout sequence) with 3-second delays (`frmFiskal.vb:543-574`)
- Type 3: `prFiskal.NapustiFiskalniPrinter()` (`frmFiskal.vb:539`)

**`frmFiskall.vb`** — Wrapper form that opens `frmFiskal` with an optional `setFis` parameter (e.g., "WIEN") to override the default device context. This was used for multi-location fiscal devices.

### 1.4 Fiscal Receipt Printing Detail (by Device Type)

**Type 1 — FM (TMT):** `frmPlacanje.vb:2592-2644`
- Writes `FS;1;<operator>` header
- `FC;0;<item>;<unit>;<tax>;<qty>;<price>` per line item
- `F%;P;<discount>` per line item discount
- `F%;M;<total_discount>` total discount
- `FP;<payment_type>;` footer
- Copies `rr.in` → `FS_rr.in` and `FS_rr_tr.in` → `trigger.in`
- Receipt template visible in `0FS_rr.in` and `0rr.in` files

**Type 2 — NSC (Bosch):** `frmPlacanje.vb:2645-2813`
- First clears old response files via `putfiskal()`
- Sends PLU definitions: `107,1,______,_,__;2;<tax>;<plu>;<price>;<name>` and `107,1,______,_,__;4;<plu>;<price>`
- Hardcoded PLU codes: Osiguranje=20000, Boravisna Taksa=20001 (with sub-codes 19997-19999 for price variants)
- Sells items: `52,1,______,_,__;<plu>;<qty>;[<discount>;]`
- Closes: `51,1,______,_,__;` then payment `53,1,______,_,__;<type>;<total>`
- Buyer details: `55,1,______,_,__;<JMB>;<name>;<address>;<city>` if JMBG is 12 or 13 chars
- Waits for response with `FileSystemWatcher`

**Type 3 — Tring:** `frmPlacanje.vb:2819-2999`
- Uses `Tring.Fiscal.Driver.TringFiskalniPrinter` COM/DLL
- Initializes with `prFiskal.Inicijalizacija(putDo, putOd, 0, "0")`
- Creates `Artikal` + `RacunStavka` objects per line
- Tax mapping: `A_Nulta_stopa`, `K_Poreska_stopa_PDV_za_artikle_oslobodjene_PDV`, `E_Opca_poreska_stopa_PDV`
- Configurable pricing mode (fsc(5)="1" → total price, else unit price × quantity=1)
- Buyer (Kupac) attached if JMBG present (12-13 chars → prefix "4" for 12-digit)
- Payment: Gotovina/Kartica/Virman based on `cmbNacin.SelectedIndex`
- Calls `prFiskal.StampatiFiskalniRacun(rac)`
- On success, reads `odg.Odgovori(0-3)` for fiscal receipt number, date, time, amount

**Type 4 — Tremol:** (referenced in `frmFiskal.vb` and `frmRacuni.vb`)
- XML-based protocol with `<TremolFpServer>` root element
- `Command="Report"` with `Type` attribute for reports
- `Command="PrintDuplicate"` with `Type` and `Document` attributes
- `Command="CashIn"` with `<Cash Amount="...">` element

**Type 5 — Eastronics/Eltrade:** `ModuleKod.vb:2969-3038`
- Writes `N,1,______,_,__;` header (new receipt)
- Per item: `S,1,______,_,__;+;<name>;<price>;<qty>;<tax>;<0>;<id>`
- Tax codes: `E`=17% PDV, `K`=exempt/tourist PDV, `A`=zero rate
- Footer: `T,1,______,_,__;` and `J,1,______,_,__;;;;;`
- Copies `.in` → `rr.eln` to fiscal device folder
- Uses `FileSystemWatcher` to detect response `.eln` files

**Type 6 — Microelectronics:** `ModuleKod.vb:3001-3042` (also `frmFiskal.vb:859-871`)
- Writes `S,1,______,_,__;<name>;<price>;<qty>;1;1;<tax>;0;<plu>` per item
- Footer: `T,1,______,_,__;`
- Reports use `E/X/E` branding lines with contact info
- Reads data from `dgw` grid columns (cells 0-8)

**Type 7 — HCP:** `frmPlacanje.vb:3043-3134`
- XML `<RECEIPT>` envelope with `<DATA>` elements per item
- Attributes: BCR (barcode/PLU), VAT (1=17%, 3=other), MES, DEP, DSC (name ≤18 chars), PRC (price), AMN (quantity)
- PLU logic: Osiguranje=10000, Boravisna Taksa=10001, else `sifraGet()` auto-increment
- Payment: `<DATA PAY="0" AMN="0" />` (zero = auto-detect full amount)
- Writes `RCP_<receiptNo>.xml` to device folder + `CMD.OK` trigger

---

## 2. Business Flow: Proforma Invoices (Predracuni)

### 2.1 Overview

Proforma invoices (`frmPredracun.vb`) are standalone pre-invoices that quote prices before a formal fiscal receipt is issued. They involve:

1. Creating a header record (`predracuni` table)
2. Creating line items (`predracunidet` table)
3. Printing via `rptRacunFrm.printpredr()`

### 2.2 Header Creation (`snimiPred` — `frmPredracun.vb:720-751`)

Populates a `DataTable` row with fields:

| Field | Source | Description |
|-------|--------|-------------|
| `broj` | `txtBrojRacuna` | Sequential proforma number |
| `brojpred` | `txtPoslovna` | Business year prefix (e.g., "RAC2026") |
| `ime` | `TextBox6` | Customer name |
| `frima` | `txtDrugoIme` + `txtdrime1` + `txtdrime2` + `TextBox3` | Combined company info |
| `frimaid` | `firmaid` (integer) | Partner/company FK |
| `dadtum` | `DateTimePicker1` | Date issued |
| `datumval` | `DateTimePicker2` | Valid until date (default: +15 days) |
| `aktiv` | 0 | Status (0=pending) |
| `ukupno` | `txtSveUkupno` | Total amount with PDV |
| `kontakt` | `RIme` | Employee/responsible person |
| `napomnena` | `TextBox2` | Notes |
| `veza` | `txtnap` | Reference/link |
| `vezaid` | 0 | Reference ID |
| `d1` | `txtPopust` | Total discount amount |
| `d2` | `txtUkupno` | PDV total |
| `vrplac` | `cmbNacin` | Payment method description |
| `nazivp` | `txtRacInv` | Invoice/RacInv reference |
| `nazivid` | 0 | Invoice ID |
| `gostid` | 0 | Guest ID |

### 2.3 Line Items (DataGrid `dgw` — `frmPredracun.vb:200-432`)

Columns (from `predracunidet` table):

| Column | Description |
|--------|-------------|
| `id` | Auto-increment PK |
| `BrojRacuna` | FK to proforma header `broj` |
| `Trosak` | Item/service name (autocomplete from `Troskovi` table) |
| `Kol` | Quantity |
| `CijBezPdv` | Unit price without PDV |
| `UkupnoBezPdv` | Line total without PDV (Kol × CijBezPdv) |
| `Pdv` | PDV rate % (default from `setings.pdv`) |
| `IznosPdv` | PDV amount |
| `Ukupno` | Line total with PDV |
| `Nacin` | Payment method (from `cmbNacin`) |
| `Valuta` | Currency code |
| `OznakaValute` | Currency label |
| `Popust` | Discount percentage |
| `popust1` | Calculated discount amount |
| `razlogp` | Discount reason |
| `pop` | Discount flag |
| `trosakId` | Item type ID |
| `predid` | Sub-item ID |
| `t1` | Temporary field |
| `trosakid1` | Secondary item type ID |

### 2.4 Calculation Logic (`proracun` — `frmPredracun.vb:456-516`)

When a cell changes in `dgw`:
1. Reads Kol, CijBezPdv, Pdv, Popust from the grid row
2. Defaults: Kol=1 if null, CijBezPdv=0, Pdv=0, Popust=0
3. If column 8 (Ukupno) was edited: reverse-calculates unit price from total
4. If column 12 (Popust): applies discount to total
5. Default path: `UkupnoBezPdv = Kol × CijBezPdv`; `Ukupno = UkupnoBezPdv × (1 + Pdv/100)`; `IznosPdv = Ukupno - UkupnoBezPdv`; discount applied as `popust1 = Ukupno × Popust/100`; final `Ukupno -= popust1`
6. All amounts formatted to 2 decimal places

### 2.5 Partner Lookup (`txtDrugoIme_Leave` — `frmPredracun.vb:585-628`)

1. On leave, selects from `firme` table by `naziv`
2. If exactly 1 match: fills `txtdrime1` (address), `txtdrime2` (JMBG/ID), `firmaid`
3. If >1 match: opens `frmPartner1` dialog for disambiguation
4. On `TextChanged`: resets `firmaid = 0`

### 2.6 Proforma Save Flow (`btnPlacanje_Click` — `frmPredracun.vb:840-873`)

1. Checks `txtBrojRacuna` is numeric
2. Verifies proforma number doesn't already exist via `SELECT broj FROM predracuni WHERE broj=...`
3. Calls `snimiPred()` → `predracunInsert()` + `predracunDetInsert()`
4. Opens `rptRacunFrm.printpredr()` for printing
5. Resets form with `novip()` for next proforma
6. Refreshes the proforma list

---

## 3. SQL Inventory

### 3.1 Tables Referenced

| Table | Used In | Purpose |
|-------|---------|---------|
| `setings` | Multiple forms | Configuration (fiscal params, PDV rates, location, etc.) |
| `predracuni` | `frmPredracun`, `ModuleKod` | Proforma invoice headers |
| `predracunidet` | `frmPredracun`, `ModuleKod` | Proforma invoice line items |
| `troskovivrste` | `frmPredracun`, `ModuleKod` | Cost/service type lookup (id, naziv, sifra) |
| `firme` | `frmPredracun` | Partner/company lookup |
| `placanjenacin` | `frmPredracun` | Payment methods (id, nacin) |
| `kursna` | `frmPredracun` | Exchange rates (Naziv_Valute, Vrijednost) |
| `printracuni` | `frmPlacanje`, `ModuleKod` | Fiscal receipt headers (including `fisrac`, `fisvr`, `fisIZN` columns) |
| `printracunidetalji` | `ModuleKod`, `frmRacuni` | Fiscal receipt line items |
| `printracunifooter` | `ModuleKod` | Receipt footer notes (including fiscal reference line) |
| `printracspec` | `frmPlacanje` | Special receipt details (guest/soba/veza) |
| `printracuniavans` | `frmPlacanje` | Advance payment receipts |

### 3.2 Key SQL Statements

**Insert proforma header** (`ModuleKod.vb:2280-2302`):
```sql
INSERT INTO predracuni (broj, brojpred, ime, frima, frimaid, dadtum, datumval, aktiv, ukupno, kontakt, napomnena, veza, vezaid, rabat, vr_upis, d1, d2, d3, sifra1, vrplac, nazivp, nazivid, gostid)
VALUES (...)
```

**Insert proforma line items** (`ModuleKod.vb:2384-2412`):
```sql
INSERT INTO predracunidet (BrojRacuna, Trosak, Kol, CijBezPdv, UkupnoBezPdv, Pdv, IznosPdv, Ukupno, Nacin, Valuta, OznakaValute, Popust, popust1, razlogp, pop, trosakId, predid, t1, trosakid1)
VALUES (...)
```
Only inserted when `Trosak` is non-empty AND `ukupno > 0`.

**Get next proforma number** (`ModuleKod.vb:2255-2278`):
```sql
SELECT broj FROM predracuni ORDER BY broj DESC LIMIT 1
```
Returns `result + 1`.

**Select proformas** (`ModuleKod.vb:2327-2355`):
```sql
SELECT broj, brojpred, ime, frima, frimaid, dadtum, datumval, aktiv, ukupno, kontakt, napomnena, veza, vezaid, rabat, vr_upis, d1, d2, d3, sifra1, vrplac, nazivp, nazivid, gostid FROM predracuni ORDER BY broj DESC
```

**Select proforma details** (`ModuleKod.vb:2438-2466`):
```sql
SELECT convert(id,char) as id, BrojRacuna, Trosak, Kol, CijBezPdv, UkupnoBezPdv, Pdv, IznosPdv, Ukupno, Nacin, Valuta, OznakaValute, Popust, popust1, razlogp, pop, trosakId, predid, t1, trosakid1 FROM predracunidet WHERE BrojRacuna = <id>
```

**Update fiscal receipt info** (`ModuleKod.vb:3107-3143`):
```sql
UPDATE printracuni SET fisrac = <br>, fisvr = <vr>, fisIZN = <izn> WHERE BrojRacuna = <id>
UPDATE printracunifooter SET nap = concat(nap, <text>) WHERE BrojRacuna = <id>
```

**Get next PLU/sifra** (`frmPlacanje.vb:6253-6277`):
```sql
SELECT max(convert(sifra,signed)) as ms FROM troskovivrste
```
Returns `max + 1`, minimum 4.

---

## 4. Database Writes

### 4.1 Proforma Invoice Writes

| Operation | Location | Method |
|-----------|----------|--------|
| INSERT header | `ModuleKod.vb:2280-2302` | `predracunInsert()` — raw SQL INSERT with string concatenation |
| INSERT detail rows | `ModuleKod.vb:2384-2412` | `predracunDetInsert()` — loops DataTable rows, INSERT per row |
| UPDATE header | `ModuleKod.vb:2303-2326` | `predracunUpdate()` — UPDATE with WHERE broj= |
| UPDATE detail rows | `ModuleKod.vb:2413-2437` | `predracunDetUpdate()` — loop UPDATE per row |
| DELETE proforma | `ModuleKod.vb:2356-2382` | `predracunBris()` — DELETE header + (attempted) DELETE details |

### 4.2 Fiscal Receipt Writes

| Operation | Location | Method |
|-----------|----------|--------|
| INSERT receipt header | `frmPlacanje.vb:3166-3198` | `printHeader()` — parameterized MySQL INSERT into `printracuni` or `printracuniavans` |
| INSERT receipt detail | `frmPlacanje.vb:2548-2577` | `snimidetalje()` — parameterized MySQL INSERT into `printracspec` |
| UPDATE fiscal result | `ModuleKod.vb:3107-3143` or `frmPlacanje.vb:3139-3165` | `snimFiskal()` — UPDATE `printracuni` SET `fisrac`, `fisvr`, `fisIZN` + UPDATE `printracunifooter.nap` |
| INSERT troskovi/sifra | `frmPlacanje.vb` (via `snimi_trosak`) | Records PLU/sifra mapping per receipt line |

### 4.3 Critical Security Issues

- **SQL Injection**: All proforma queries use string concatenation (`ModuleKod.vb:2286`, `2393`, `2309`, `2420`, `2334-2336`, `2447`). Not parameterized.
- **No Transaction**: Proforma header + detail inserts are NOT wrapped in a transaction. Partial writes possible.
- **Duplicate check is TOCTOU**: `frmPredracun.vb:843` checks existence then inserts — race condition.

---

## 5. Fiscal Template Format (.in files)

### 5.1 FM/TMT Format (`0FS_rr.in`, `0rr.in`)

```
FS;1;Administrator           ← Receipt start: type=1, operator name
FC;0;Autobuski prevoz;kom;1;2,7    ← Item: dept=0, name, unit, tax_group, unit_price
F%;P;System.Windows.Forms...← Per-item discount placeholder
FC;0;Noćenje sa doručkom;kom;1;44,18
F%;P;...
FC;0;Osiguranje;kom;1;2
F%;P;...
FC;0;Boravisna Taksa;kom;1;1,5
F%;P;...
FC;0;usluga;kom;1;45
F%;P;...
F%;M;0                        ← Total discount (0 = none)
FP;2;                         ← Payment type (2 = card/virman)
FE                             ← Receipt end
```

Structure: `FS;top;l_operator` → `FC;dept;name;unit;tax;price` → `F%;P;discount` → `F%;M;discount` → `FP;pay_type;` → `FE`

### 5.2 NSC/Bosch Format (created in `FMRacunNSC`)

```
107,1,______,_,__;2;<tax>;<plu>;<price>;<name>    ← Define PLU + set price
107,1,______,_,__;4;<plu>;<price>                  ← Set current price
48,1,______,_,__;<operator>;<pwd>;<sif>;;;          ← Operator login
52,1,______,_,__;<plu>;<qty>[;<discount>;]          ← Sell item
51,1,______,_,__;                                   ← Subtotal
55,1,______,_,__;<jmb>;<name>;<addr>;<city>;       ← Buyer info (optional)
56,1,______,_,__;                                   ← Close receipt (after payment)
53,1,______,_,__;<pay_type>;<total>                 ← Payment
53,1,______,_,__;;;_<receipt#>#<location>           ← Receipt reference
```

### 5.3 Eastronics/Eltrade Format (created in `FMRacunE`)

```
N,1,______,_,__;                                   ← New receipt
S,1,______,_,__;+;<name>;<price>;<qty>;<tax>;<0>;<id>  ← Sell item
T,1,______,_,__;                                    ← Total
J,1,______,_,__;;;;;                                ← Close receipt
```
Tax codes: `E`=17% PDV, `K`=exempt, `A`=zero. Response read via `FileSystemWatcher` on `.eln` files.

### 5.4 Microelectronics Format (created in `FMRacunMikroelektornika`)

```
S,1,______,_,__;<name>;<price>;<qty>;1;1;<tax>;0;<plu>   ← Sell item
T,1,______,_,__;                                    ← Total
```
Report templates use `E` lines for header/branding and `X/Z/R` for report types.

### 5.5 HCP Format (XML, created in `FMRacunHCP`)

```xml
<RECEIPT>
  <DATA BCR="10000" VAT="1" MES="0" DEP="1" DSC="Osiguranje" PRC="2.00" AMN="1.000" />
  <DATA PAY="0" AMN="0" />
</RECEIPT>
```
Trigger: `CMD.OK` file creation in shared folder.

Report XML uses `<COMMAND><DATA CMD="Z_REPORT" ... /></COMMAND>` structure.

### 5.6 Tremol Format (XML)

```xml
<TremolFpServer Command="Report" Type="DailyZ" />
<TremolFpServer Command="Report" Type="DailyX" />
<TremolFpServer Command="PrintDuplicate" Type="0" Document="5" />
<TremolFpServer Command="CashIn"><Cash Amount="50.00" /></TremolFpServer>
```

### 5.7 Trigger Files

- **Type 1 (FM)**: `FS_rr_tr.in` contains filename of data file; `trigger.in` contains `FS_rr.in` — `0FS_rr_tr.in:1`
- **Type 1**: `0trigger.in` = `FS_rr.in` — drives file-copy polling
- **Type 2 (NSC)**: No trigger file; file presence in shared folder triggers processing
- **Type 5 (Eltrade)**: `rr.eln` file placed in device folder
- **Type 6 (Micro)**: Data file renamed to `.inp` extension for pick-up
- **Type 7 (HCP)**: `CMD.OK` zero-byte trigger file signals command readiness

---

## 6. Business Rules

### 6.1 Tax Rate Mapping

| PDV % | Tring | NSC | Tremol | HCP | Eltrade |
|-------|-------|-----|--------|-----|---------|
| 17% | `E_Opca_poreska_stopa_PDV` | 2 | — | 1 | E |
| 0% (exempt/tourist) | `K_Poreska_stopa_PDV_za_artikle_oslobodjene_PDV` | 4 | — | 3 | K |
| 0% (neregistrirani) | `A_Nulta_stopa_za_neregistrirane_obveznike` | — | — | — | A |
| Default rate | From `setings.pdv` | — | — | — | — |

### 6.2 Special PLU Codes

| Item | NSC | HCP | Microelectronics |
|------|-----|-----|-------------------|
| Osiguranje (Insurance) | 20000 | 10000 | 35000 |
| Boravisna Taksa (Residence Tax) | 20001 | 10001 | 35001 |
| Boravisna Taksa 1KM | 19999 | — | — |
| Boravisna Taksa 1.5KM | 19998 | — | — |
| Boravisna Taksa 0.5KM | 19997 | — | — |
| Other items | Auto PLU from `sifraGet()` | Auto PLU from `sifraGet()` | 30000+ (row offset) |

### 6.3 Payment Types

| Index | NSC Code | Tring | HCP | Description |
|-------|----------|-------|-----|-------------|
| 0 | 0 | Gotovina | CASH_IN/CASH_OUT | Cash |
| 1 | 1 | Kartica | — | Card |
| 2 | 3 | Virman | — | Bank transfer |
| 3+ | — | Gotovina | — | Default fallback |

### 6.4 Receipt Mode (`racunPr`)

- `racunPr = 0` → Normal fiscal receipt (sent to device)
- `racunPr ≠ 0` → Test receipt (NOT sent to fiscal device, skip all `FMRacun*` calls)

### 6.5 Pricing Modes

**Tring fsc(5) flag** (`frmPlacanje.vb:2886-2891`):
- `"1"` → Total price mode: `ARRT.Cijena = total`, `racst.Kolicina = 1`
- Other → Unit price mode: `ARRT.Cijena = unit_price`, `racst.Kolicina = quantity`

### 6.6 Buyer Identification

If `txtdrime2.Text` (partner JMBG) is numeric and length is 12, it's prefixed with `"4"` to make 13-digit ID. Applied in:
- NSC: `55,1,______,_,__;<jmb>;<name>;<addr>;<city>;` (`frmPlacanje.vb:2777-2788`)
- Tring: `kup.IDbroj`, `kup.Naziv`, `kup.Adresa`, `kup.PostanskiBroj=71000`, `kup.Grad` (`frmPlacanje.vb:2925-2945`)
- HCP: Not explicitly in receipt XML (buyer data stored in receipt header)

### 6.7 Proforma Invoice Rules

1. Numbering: Sequential via `SELECT MAX(broj) + 1` from `predracuni` (`ModuleKod.vb:2261`)
2. Validity: Default 15 days from creation date (`frmPredracun.vb:76`)
3. PDV rate: Loaded from `setings.pdv` (`frmPredracun.vb:421`)
4. Calculations: Total = (Unit × Qty) × (1 + PDV/100) - Discount; Discount = Total × Popust%/100
5. Currency: Dual currency display using `kursna` table (`cmbKursna`/`ComboBox1`)
6. Duplicate prevention: Checked by `SELECT broj FROM predracuni WHERE broj=...` before insert (`frmPredracun.vb:843-845`)
7. Proforma is NOT fiscal — no interaction with fiscal devices

---

## 7. Key Findings for Modern System

### 7.1 Architecture Issues

1. **Massive device-type branching**: Every fiscal operation has 7 code paths scattered across multiple files. A **Strategy Pattern** with a `IFiscalDevice` interface would collapse this.

2. **File-based fiscal communication**: Types 1, 2, 4, 5, 6, 7 use file-copy polling (write `.in` → copy to shared folder → read `.out`/`.eln` via `FileSystemWatcher`). This is fragile and hard to test. Modern systems should use HTTP/TCP APIs or at minimum a message queue.

3. **No transactional integrity**: Proforma header + detail row inserts are independent non-transactional INSERTs. A failure mid-way leaves orphaned data.

4. **SQL injection throughout**: All proforma and some fiscal queries use string concatenation, not parameterized queries.

5. **Hardcoded special items**: PLU codes for "Osiguranje" and "Boravisna Taksa" are hard-coded in multiple places per device type. These should be configurable lookup tables.

### 7.2 Data Model Gaps

1. **`printracuni.fisrac/fisvr/fisIZN`** — Fiscal receipt reference stored as string/integer in the receipts table. Modern system needs a proper `fiscal_receipt` table with device type, receipt number, date/time, verification code (JIK), and amount.

2. **`predracunidet`** has no explicit FK constraint to `predracuni` in the code (only `BrojRacuna` as logical FK). Delete logic (`predracunBris`) tries to SELECT then not DELETE details properly.

3. **`setings.fiscal`** — A `*`-delimited string holding 6 parameters. Should be a separate `fiscal_config` table.

### 7.3 Business Logic to Preserve

1. **Tax mapping** differs per device: The modern system must maintain the 3-way PDV classification (standard 17%, exempt/tourist, zero-rate) and map to device-specific codes.

2. **Buyer JMBG 12→13 digit conversion**: Prefix "4" for 12-digit IDs must be preserved.

3. **Proforma calculations**: The discount model (percentage discount on total-with-PDV) and reverse-calculation (total → unit price when total is edited) must be preserved exactly.

4. **Payment type mapping**: Cash/Card/Virman must map to device-specific payment codes.

5. **Daily close**: Z-report dispatch is per-device and includes specific date format requirements (e.g., `ddMMyy` for NSC, `ddMM` + `yy` for HCP).

6. **Cash in/out**: Must support positive (CASH_IN) and negative (CASH_OUT) amounts.

### 7.4 Modernization Recommendations

1. **Extract fiscal device communication into `IFiscalDeviceService`** with methods:
   - `InitializeAsync()`
   - `PrintReceiptAsync(Receipt receipt)`
   - `PrintDuplicateAsync(int receiptNumber)`
   - `PrintDailyReportAsync(ReportType type)`
   - `PrintPeriodicReportAsync(DateTime from, DateTime to)`
   - `CashInOutAsync(decimal amount)`
   - `GetStatusAsync()`

2. **Replace file polling with async request/response**: Each device driver should handle its own protocol internally (serial, TCP, file-share) behind the interface. HCP and Tremol already use XML — these are candidates for HTTP API wrapping.

3. **Use parameterized queries and EF Core**: Eliminate all SQL injection vectors. Wrap proforma CRUD in a repository with transaction support.

4. **Normalize fiscal configuration**: Replace `setings.fiscal` with a `fiscal_devices` table keyed by location/station.

5. **Separate proforma from fiscal receipt**: Currently proforma is entirely independent of fiscal (correct), but the modern system should allow converting a proforma to a fiscal receipt (currently manual).

6. **Audit trail**: Add created_at/updated_at, created_by columns to both `predracuni` and `predracunidet`. Currently only `vr_upis` stores date on the header.

7. **Remove `FileSystemWatcher` response handling**: `ModuleKod.vb:3040-3096` uses `fw_Created` event on `.eln` file to read fiscal response. This is unreliable. Replace with proper response API or polling mechanism.