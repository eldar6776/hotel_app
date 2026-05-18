# Legacy Guest Management (frmGosti) Analysis

> **Source:** `legacy_code/frmGosti.vb` (2161 lines)
> **Cross-ref:** `LEGACY_ANALYSIS/00_DATABASE_SCHEMA.md`, `LEGACY_ANALYSIS/02_MODULEKOD_FUNCTIONS.md`

---

## 1. Business Flow: Guest Management (search, add, edit, delete, document management)

### 1.1 Form Overview

`frmGosti` is a WinForms form with 4 tabs (`TabControl1`):
- **TabPage1 "Gosti"** — Main guest listing with search, TZ registration, boraviste report, group management
- **TabPage2** — Guest search/filter by date range with Crystal Reports (tourist statistics)
- **TabPage3** — Tourist registration book (gostiknjiga/e-stranac XML export)
- **TabPage4** — Reports by date range (hotel statistics, tourist report, nationality report)

Key UI controls:
- `dgvGosti` — Main guest DataGridView (lines 23-24, 449-451)
- `dgv`, `dgv1`, `dgv2` — Bottom detail grids (stay history, etc.)
- `TextBoxX1` — Live search filter within current guest list (line 264, 1601-1614)
- `TextBox3` — Guest database search (searches `gosti` table directly) (line 117, 1959-1967)
- `CheckBox1 "Napomena"` — Toggle note column visibility (line 639, 1057-1058)
- `CheckBox3 "Cijena"` — Toggle tariff/price column; when checked, uses `0 as cijena` instead of `tarifa` (line 719, 1059-1062)
- `NumericUpDown1` — Day offset for tourist registration date filtering (line 345)
- `txteuser`, `txteknjiga` — E-stranac user/book IDs parsed from `setings.sobekuc` (lines 285-287)
- `cmbGrupe` — Group filter dropdown (line 93)

### 1.2 Form Load (lines 1282-1306)

```
frmGosti_Load:
  1. Parse setings.sobekuc by "#" → extract txteuser (index 5), txteknjiga (index 6), ktz (index 7)
  2. Call getGosti(0) — load all current guests
  3. Set DateTimePicker1 = now-7 days, DateTimePicker2 = now
  4. Set DateTimePicker5 = now, DateTimePicker6 = now-10 days
  5. radim = 0 (flag: not processing)
```

### 1.3 Guest List Loading: `getGosti(g)` (lines 1049-1144)

**Two modes** controlled by parameter `g`:

**Mode g=0 (default):** Load all currently checked-in guests
- SQL: `SELECT relgostsoba.tid, sobe.naziv, concat(g.ime,' ',g.prezime) As ImePrezime, relgostsoba.usluga, rezervacijegrupe.naziv as grupa, ... FROM relgostsoba INNER JOIN gosti g ON relgostsoba.gostID=g.ID INNER JOIN sobe ON relgostsoba.sobaID=sobe.ID left JOIN rezervacijegrupe ON relgostsoba.grupaID=rezervacijegrupe.ID left join nocenja n on n.rid=relgostsoba.id WHERE prijavaodjava=0 AND (odjavljen=0) AND (rezervacija=0)` (line 1062)
- If `CheckBox3` checked: `0 as cijena` instead of `tarifa as cijena`; omit `nocenja` join (line 1060)
- If `CheckBox1` checked: `'' as napomenapl` instead of `relgostsoba.napomenapl` (lines 1057-1058)

**Mode g=1 (foreign citizens for TZ):** Load domestic vs. foreign guests with extended columns for police reporting
- First fetch domestic country ID: `SELECT d.ID FROM drzave d WHERE domaca=1` (line 1066)
- SQL: Complex join with drzave table, formatting dates, transforming gender (`if(g.pol='M',1,2)`), mapping document types (`if(g.dokument>2,3,g.dokument)`) (line 1075)
- Filter: `did<> {dom}` (foreign citizens only)

**Displayed columns:** tid, gostID, ID (GSID), naziv (room), ImePrezime, usluga, grupa, checkInDate, checkOutDate, print1, print2, napomenapl, cijena, status, ime, prezime, adresa, datumRodjenja, mjestodrzavaR, pol, drzavljanstvo, DID, dokument, brDokument

### 1.4 Guest Search: `gosti(id, vrsta)` (lines 1969-2021)

- Clears `ds.Tables("gosti")`
- Searches `gosti` table directly: `SELECT * FROM gosti WHERE (ime LIKE '%{text}%' or prezime LIKE '%{text}%')` (line 1990)
- **SQL injection risk**: TextBox3 text concatenated directly
- Calls `funkcije.SelectDistinct()` to filter for listing view
- Sets dgvGosti columns: ID, Prezime, Ime, Drzavljanstvo, Broj Dokumenta

### 1.5 Previous Guest Search: `getGostir()` (lines 2022-2104)

- Loads **checked-out** guests: `WHERE (relgostsoba.odjavljen=1)` plus name filter (line 2031-2033)
- Same column set as `getGosti` but shows historical guests
- If `CheckBox3` checked: `0 as cijena`, no `nocenja` join

### 1.6 Guest Visit History: `ucitajPosjete(i)` (lines 1372-1423)

- Calls stored procedure `gosti1` with `@id` parameter = `gostID` (line 1381-1384)
- Returns: checkInDate, checkOutDate, Soba, checkInRadn, Tarifa, popust, PopustRazlog
- Populates `dgv` grid at bottom

### 1.7 Tourist Registration (TZ) — `Button3_Click` (lines 1776-1909)

**Single guest registration:**
1. Confirm dialog: "Sigurni ste da zelite prijaviti gosta u TZ!"
2. Extract guest data from dgvGosti row: ime, prezime, dokument, checkInDate, did (nationality), brDokument, pol, datumRodjenja, status, tid
3. Determine foreign citizen: `if drzavljanstvo <> 23 then prijava_za_strance = 1`
4. Build URL parameters for clasTZ.prijavi() API call
5. If `tid > 0`: action="promjena" (update existing); else action="prijava" (new registration)
6. Parse checkoutDate for `prestanak_boravka` parameter
7. Parse `adresa` field as date for `rok_vazenja_pi` (document expiry) — **BUG: misuse of adresa field**
8. Call `tz.prijavi(pr)` — external API registration
9. On success (`odg > 1`): UPDATE `relgostsoba` set tid and estranac (lines 1821-1826)
10. For foreign citizens: UPDATE with `estranac = max(estranac)+1` (line 1823)

**Bulk registration (SelectedRows):**
- Same logic per selected row (lines 1841-1897)
- Does NOT set estranac for foreign citizens in bulk mode (line 1880 only sets tid)

### 1.8 Tourist Registration Book — `getgostdrz(go)` (lines 1032-1048)

- Two modes based on `ktz` (from `setings.sobekuc` parsed at index 7):
  - `ktz=0`: All foreign citizens (`d.id <> 23`) with extended columns for police reporting (line 1038)
  - `ktz>0`: Only guests with estranac > 1 (line 1040)
- Date filter: `checkInDate >= now+NumericUpDown1.Value OR checkOutDate between now+NumericUpDown1 and now+1day`
- Maps gender: `if(g.pol='M',1,2)`, maps document: `if(g.dokument>2,3,g.dokument)`

### 1.9 E-Stranac XML Export — `Button14_Click` (lines 2119-2152)

1. Get next estranac number: `SELECT if(max(estranac),max(estranac),0)+1 as est FROM relgostsoba` (line 2125)
2. For each row in DataGridView2:
   - If `Knjiga br. = 0`: assign sequential number, UPDATE `relgostsoba set estranac={stbr}` (line 2138)
   - Build XML `<foreign_citizen>` element with all TZ fields
3. Wrap in `<catalog>` root element
4. Save as `estranac.xml` via SaveFileDialog

### 1.10 Report Buttons

| Button | Lines | Action | Report Form |
|--------|-------|--------|-------------|
| btnTZ "Prijava" | 1308-1317 | getGostiTurist() + izmjeniR1() + frmReportTurist | Tourist registration report |
| btnBoraviste | 1319-1329 | getGostiTurist() + izmjeniR2() + frmPrijavaBoravkaPodaci | Stay registration report |
| btnLista "Lista gostiju" | 1331-1339 | getGosti(0) + GostiListingrpt | Guest listing report |
| btnGrupa "Promijeni grupu" | 1341-1345 | frmGrupeIzmjena dialog | Change guest group assignment |
| Button5 "Gost izmjena" | 1927-1936 | frmPrijavaGostiKucice.prenos(gostID, id) | Edit guest data |
| Button4 "Datum odlaska" | 1938-1953 | frmSobaInfoPromjena with dates | Change checkout date |
| Button2 (report tab) | 1755-1757 | prov_go() | Statistics reports |

### 1.11 Statistics Reports: `prov_go()` (lines 1657-1725)

Three report modes based on RadioButton selection:

**RadioButton7 — Hotel Statistics:**
- Count total rooms: `SELECT COUNT(ID) AS BS FROM sobe` (line 1662)
- Count nights: `SUM(if(DATEDIFF(checkOutDate,checkInDate)=0,1,DATEDIFF(...)))` with date filter (lines 1667/1669)
- Count unique stays: `COUNT(pid) GROUP BY pid` with date filter (lines 1675/1677)
- Admin override (`sbNivo.Text="9"`): no printracspec join filter
- Report: `HotelStatistika1`

**RadioButton6 — Tourist Report:**
- Full guest listing with date range, formatted dates, optional tariff (lines 1702/1704)
- Report: `rptTuristicka`

**RadioButton5 — Nationality Statistics:**
- `SELECT drzavljanstvo, count(gosti.ID) as gosti, SUM(DATEDIFF...) AS DiffDate GROUP BY drzavljanstvo` (lines 1713/1715)
- Report: `rptTurdrz`

### 1.12 Print Flag Updates

**`izmjeniR1()`:** `UPDATE relgostsoba SET print1=1 WHERE ID={row}` (line 1227)
**`izmjeniR2()`:** `UPDATE relgostsoba SET print2=1 WHERE ID={row}` (line 1258)

### 1.13 Validation: `provjeriprij()` (lines 1762-1775)

- Iterates dgvGosti rows
- Highlights invalid entries:
  - `brDokument.Length < 2` → row header red
  - `!IsDate(datumRodjenja)` → row header red
  - `tid > 0` → tid cell colored green (successfully registered with TZ)

### 1.14 Live Search: `TextBoxX1_TextChanged` (lines 1601-1614)

- If text > 2 characters and numeric: filter by `naziv LIKE '%{text}%'` (room number)
- If text > 2 characters and non-numeric: filter by `ImePrezime LIKE '%{text}%'` (name)
- Clear filter if text ≤ 2 characters

---

## 2. SQL Inventory

### 2.1 SELECT Operations

| Line | Table(s) | Columns | WHERE/Condition | Purpose |
|------|----------|---------|-----------------|---------|
| 1038 | relgostsoba, gosti g, drzave d, sobe, rezervacijegrupe, nocenja n | tid, estranac, ime, prezime, pol, datumRodjenja, naziv(drzava), brDokument, checkInDate, checkOutDate, sifra, mjestodrzavaR, dokument, id | `checkInDate >= now+NumericUp or (checkOutDate between now+NumericUp and now+1d) and d.id<>23 GROUP BY id ORDER BY estranac DESC` | Foreign citizens for TZ registration (ktz=0) |
| 1040 | Same tables | Same columns | Same conditions + `estranac > 1 GROUP BY id ORDER BY estranac DESC` | Foreign citizens for TZ (ktz>0, filtered) |
| 1060 | relgostsoba, gosti g, sobe, rezervacijegrupe | tid, naziv, ImePrezime, usluga, grupa, naziv1, checkInDate, checkOutDate, print1, print2, napomenapl, 0 as cijena, gostID, ID, status, ime, prezime, adresa, datumRodjenja, mjestodrzavaR, pol, drzavljanstvo, DID, dokument, brDokument | `odjavljen=0 AND rezervacija=0` | Load current guests (no price) |
| 1062 | relgostsoba, gosti g, sobe, rezervacijegrupe, nocenja n | Same + tarifa as cijena | `prijavaodjava=0 AND odjavljen=0 AND rezervacija=0` | Load current guests (with tariff) |
| 1066 | drzave | ID | `domaca=1` | Get domestic country ID |
| 1075 | relgostsoba, gosti g, sobe, rezervacijegrupe | 0 as redni_broj, 0 as serijski_broj_knjige, 'recepcija' as prijavljen... g.ime, g.prezime, pol, datumRodjenja, mjestodrzavaR, drzavljanstvo, brDokument, checkInDate, checkOutDate, ID | `odjavljen=0 AND rezervacija=0 AND did<>{dom}` | Foreign guests for police registration |
| 1162 | rezervacijegrupe | ID, naziv | `odjavljena=0` | Load active groups |
| 1193-1194 | relgostsoba, gosti, sobe, gostdokument | ID, gostID, PID, naziv, ime, prezime, checkInDate, checkOutDate, datumRodjenja, mjestodrzavaR, drzavljanstvo, naziv(dokument), brDokument, redniBroj | `relgostsoba.ID={selected_row_id}` | Get tourist details for selected guest |
| 1381-1384 | (stored proc gosti1) | checkInDate, checkOutDate, Soba, checkInRadn, Tarifa, popust, PopustRazlog | `@id={gostID}` | Get guest visit history |
| 1619 | relgostsoba, sobe, printracspec c, sobatarifa, radnici, radnici_1, gosti, gostdokument | pl, checkInDate, checkOutDate, ime, prezime, Soba, checkInRadn, Tarifa, popust, PopustRazlog, ID, adresa, datumRodjenja, telefon, mobitel, email, brDokument, dokument.naziv, drzavljanstvo, pol, gostID, p1, p2, p3 | `checkInDate > {od} AND checkInDate < {doo}` | All guests by date range |
| 1662 | sobe | COUNT(ID) AS BS | (none) | Total room count |
| 1667 | relgostsoba, sobe, sobatarifa, radnici, radnici_1, gosti, gostdokument | SUM(nights), date params | `checkInDate > {from} AND checkInDate < {to}` | Night count for statistics (admin) |
| 1669 | Same + printracspec c | Same | Same | Night count (non-admin, filtered by printracspec) |
| 1675 | relgostsoba, sobe, sobatarifa, radnici, radnici_1, gosti, gostdokument | COUNT(pid), day diff, date params | `checkInDate > {from} AND checkInDate < {to} GROUP BY pid` | Unique stays count (admin) |
| 1677 | Same + printracspec c | Same | Same | Unique stays (non-admin) |
| 1702 | relgostsoba, sobe, sobatarifa, radnici, radnici_1, gosti, gostdokument | Full guest listing with dates | `checkInDate > {from} AND checkInDate < {to}` | Tourist report (admin) |
| 1704 | Same + printracspec c | Same | Same | Tourist report (non-admin) |
| 1713 | relgostsoba, sobe, sobatarifa, radnici, gosti, gostdokument | drzavljanstvo, COUNT, SUM(nights) | `checkInDate range GROUP BY drzavljanstvo` | Nationality stats (admin) |
| 1715 | Same + printracspec c | Same | Same | Nationality stats (non-admin) |
| 1990 | gosti | * | `(ime LIKE '%{text}%' OR prezime LIKE '%{text}%')` | Search guests by name |
| 2031 | relgostsoba, gosti g, sobe, rezervacijegrupe, nocenja n | Full guest columns + 0 as cijena | `odjavljen=1 AND concat(ime,' ',prezime) LIKE '%{text}%'` | Previous guests search (no tariff) |
| 2033 | Same + tarifa as cijena | Same | Same | Previous guests search (with tariff) |
| 2125 | relgostsoba | `if(max(estranac),max(estranac),0)+1 as est` | (none) | Get next estranac number |

### 2.2 UPDATE Operations

| Line | Table | SET | WHERE | Purpose |
|------|-------|-----|-------|---------|
| 1227 | relgostsoba | print1=1 | ID={selected_row_id} | Mark registration report printed (R1) |
| 1258 | relgostsoba | print2=1 | ID={selected_row_id} | Mark stay report printed (R2) |
| 1821 | relgostsoba | tid='{tz_response}', estranac=0 | id={row_id} | Save TZ registration ID (domestic) |
| 1823 | relgostsoba AS t CROSS JOIN (SELECT MAX(estranac)+1) AS m | tid='{odg}, estranac=max_number+1' | t.id={row_id} | Save TZ registration ID (foreign) — **BUG: concatenation in tid field** |
| 1825 | relgostsoba | tid='{odg}', estranac=0 | id={row_id} | Save TZ registration ID (domestic, duplicate of 1821) |
| 1880 | relgostsoba | tid='{odg}' | id={selected_row_id} | Bulk TZ registration update |
| 2138 | relgostsoba | estranac={stbr} | id={row_id} | Assign sequential estranac number |

### 2.3 INSERT Operations

| Line | Table | Columns | Purpose |
|------|-------|----------|---------|
| (none) | — | — | No direct INSERT operations in frmGosti |

Note: The form relies on external forms (`frmPrijavaGostiKucice`, `frmSobaInfoPromjena`, `frmGrupeIzmjena`) for guest creation/editing. Those forms are not included in this file.

### 2.4 DELETE Operations

| Line | Table | WHERE | Purpose |
|------|-------|------|---------|
| (none) | — | — | No direct DELETE operations in frmGosti |

---

## 3. Database Writes

### 3.1 relgostsoba Updates (all via mysqlExScalar)

| Line | Operation | Context | Risk |
|------|-----------|---------|------|
| 1227 | `UPDATE relgostsoba SET print1=1 WHERE ID={id}` | Mark tourist report printed | Low |
| 1258 | `UPDATE relgostsoba SET print2=1 WHERE ID={id}` | Mark stay report printed | Low |
| 1821/1825 | `UPDATE relgostsoba SET tid='{odg}', estranac=0 WHERE id='{id}'` | Save TZ registration (domestic) | **SQL injection: id from grid directly** |
| 1823 | `UPDATE relgostsoba AS t CROSS JOIN (SELECT MAX(estranac)+1) SET t.tid='{odg}, t.estranac=m.max_number+1'` | Save TZ registration (foreign) | **BUG: comma in tid field concatenation; race condition on MAX(estranac)** |
| 1880 | `UPDATE relgostsoba SET tid='{odg}' WHERE id='{id}'` | Bulk TZ registration | SQL injection |
| 2138 | `UPDATE relgostsoba SET estranac={stbr} WHERE id={id}` | Assign estranac book number | SQL injection |

### 3.2 No Transaction Safety

- None of the UPDATE operations use transactions
- The estranac MAX+1 pattern in line 1823 has a race condition
- No error handling or rollback for partial updates during bulk TZ registration

---

## 4. Guest Data Model

### 4.1 Tables Used by frmGosti

| Table | Usage | Key Columns Accessed |
|-------|-------|---------------------|
| `relgostsoba` | Core stay/guest-room link | ID, gostID, sobaID, checkInDate, checkOutDate, odjavljen, rezervacija, tid, estranac, grupaID, print1, print2, napomenapl, usluga, status, tarifaID (via nocenja), popust, PopustRazlog, PID, redniBroj, pl |
| `gosti` | Guest master data | ID, ime, prezime, adresa, datumRodjenja, pol, drzavljanstvo, DID, dokument, brDokument, mjestodrzavaR, telefon, mobitel, email |
| `sobe` | Room info | ID, naziv |
| `sobatarifa` | Tariff info | ID, naziv |
| `drzave` | Country lookup | ID, naziv, sifra, domaca |
| `gostdokument` | Document type lookup | ID, naziv |
| `rezervacijegrupe` | Guest groups | ID, naziv, odjavljena |
| `nocenja` | Night entries | rid (→relgostsoba.id), tarifa |
| `radnici` | Staff | ID, ime (checkInRadnik, checkOutRadnik) |
| `setings` | Hotel config | sobekuc (indexes 5,6,7 for TZ config) |
| `printracspec` | Special prints | d1 (→relgostsoba.id) |

### 4.2 Data Flow: Guest Fields in frmGosti

| Field | DB Source | Display Column | Used In |
|-------|-----------|----------------|---------|
| `gostID` | relgostsoba.gostID | "GID" (width 50) | Guest identification, passed to edit forms |
| `ID` (GSID) | relgostsoba.ID | "GSID" (width 50) | Stay record identification, UPDATE target |
| `naziv` | sobe.naziv | "Soba" (width 50, red) | Room name |
| `ImePrezime` | concat(g.ime,' ',g.prezime) | "Ime i Prezime" (dynamic width) | Full name display |
| `usluga` | relgostsoba.usluga | "Usluga" (width 150) | Service description |
| `grupa` | rezervacijegrupe.naziv | "Grupa" (width 120) | Group assignment |
| `checkInDate` | relgostsoba.checkInDate | "Prijavljen" (width 120) | Check-in date |
| `checkOutDate` | relgostsoba.checkOutDate | "Datum odjave" (width 120, format dd/MM/yyyy) | Checkout date |
| `print1` | relgostsoba.print1 | "R1" (width 40) | Registration print flag |
| `print2` | relgostsoba.print2 | "R2" (width 40) | Stay report print flag |
| `tid` | relgostsoba.tid | Auto-sized | Tourist registration ID (green when > 0) |
| `status` | relgostsoba.status | Hidden | Guest status (used for TZ api) |
| `ime` | g.ime | Hidden | First name |
| `prezime` | g.prezime | Hidden | Last name |
| `adresa` | g.adresa | Hidden | **Reused as document expiry date in TZ registration** |
| `datumRodjenja` | g.datumRodjenja | Hidden | Date of birth |
| `mjestodrzavaR` | g.mjestodrzavaR | Hidden | Place of birth / residence |
| `pol` | g.pol | Hidden | Gender (M/Z) |
| `drzavljanstvo` | g.drzavljanstvo | Hidden | Nationality text |
| `DID` | g.DID | Hidden | Country ID (FK to drzave) |
| `dokument` | g.dokument | Hidden | Document type ID |
| `brDokument` | g.brDokument | Hidden | Document number |
| `cijena` | nocenja.tarifa or 0 | Hidden | Tariff price |
| `napomenapl` | relgostsoba.napomenapl or '' | Hidden | Stay note |

### 4.3 Validations

| Check | Location | Action |
|-------|----------|--------|
| brDokument length < 2 | `provjeriprij()` line 1764 | Row header → Red background |
| datumRodjenja not a date | `provjeriprij()` line 1765 | Row header → Red background |
| tid > 0 (TZ registered) | `provjeriprij()` line 1766 | tid cell → GreenYellow |
| No selected rows on Button3 | line 1781 | Skip single mode, go to bulk |
| CurrentRow.Index < 0 on Button5/Button4 | lines 1928, 1939 | Return (no action) |
| Search text ≤ 2 chars | lines 1602, 1960 | Clear filter |

---

## 5. Business Rules

### 5.1 Document Types

| Value | Meaning | Usage in frmGosti |
|-------|---------|-------------------|
| 1 | Pasos (Passport) | Mapped directly from `gosti.dokument` |
| 2 | Licna (ID Card) | Mapped directly |
| 3+ | Vozacka/Ostalo | Mapped to 3 in TZ export: `if(dokument>2,3,dokument)` (line 1038) |

**TZ gender mapping:** `if(g.pol='M',1,2)` — M→1, anything else→2 (lines 1038, 1075)

### 5.2 Tourist Registration (Turistička Zajednica) Flow

1. **Classification:** `domaca=1` country ID (hardcoded reference, typically ID 23 = BiH)
   - Domestic: `did=23` → not sent to TZ as foreign
   - Foreign: `did<>23` → `prijava_za_strance=1`
2. **Actions:**
   - New registration: `akcija=prijava`
   - Update existing: `akcija=promjena&id={tid}` (when `tid > 0`)
3. **Fields sent to clasTZ API:** ime, prezime, vrsta_isprave, pocetak_boravka, drzavljanstvo, broj_isprave, spol, datum_rodjenja, status_gosta, prestanak_boravka (optional), kniga_reg_broj, redni_broj, rok_vazenja_pi (from adresa field!), mjesto_rodjenja, prijava_za_strance
4. **Response handling:** Numeric response > 1 = success (update tid), > 0 = change success, else = failure
5. **Estranac assignment:** Only for foreign citizens, sequential MAX+1 (race condition risk)

### 5.3 E-Stranac XML Export

- Configuration stored in `setings.sobekuc` split by `#`:
  - Index 5: `txteuser` (registered user name)
  - Index 6: `txteknjiga` (serial book number)
  - Index 7: `ktz` (foreign citizen zone/filter mode)
- XML structure: `<catalog><foreign_citizen>...</foreign_citizen></catalog>`
- Each foreign_citizen element has 20+ fields matching BiH Ministry of Interior requirements
- Sequential `Knjiga br.` assigned when current value = 0, using MAX(estranac)+1

### 5.4 Duplicate Prevention

- **No explicit duplicate prevention** in frmGosti for guest creation (handled by frmPrijavaGostiKucice, not in this file)
- Document number uniqueness not enforced at this layer
- Guest name uniqueness not enforced
- The `tid` field serves as the TZ registration reference; zero means not registered

### 5.5 Checked-out Guest Behavior

- When `TextBox3_TextChanged` triggers `getGostir()`: searches `odjavljen=1` guests (line 2031-2033)
- This is separate from the live search in `TextBoxX1_TextChanged` which filters by `ImePrezime` on the in-memory `GostiLista` table
- The form does NOT provide edit/delete for checked-out guests in this module

### 5.6 Guest Status Relgostsoba.status

- Referenced in line 1797 for TZ API `status_gosta` parameter
- Links to `goststatus` lookup table (see `00_DATABASE_SCHEMA.md` section 1.9: id, naziv, del, taksa)
- Values include: Turist, Vlasnik kuće, Dijete do 12 godina — each with associated tax amount

### 5.7 Print Flags

- `print1`: Tourist registration report (Prijava) printed — set by izmjeniR1() after printing
- `print2`: Stay registration report (Boravište) printed — set by izmjeniR2() after printing
- Both displayed as small columns "R1" and "R2" in dgvGosti

### 5.8 Admin Role Filtering (Lines 1666-1679, 1701-1704, 1712-1715)

- `frmGlavni.sbNivo.Text = "9"` → admin level, no printracspec join filter
- Non-admin: All report queries include `inner join printracspec c on c.d1=relgostsoba.id` — filtering to only show guests that have a print record
- This means non-admin users only see guests who have been printed on reports

---

## 6. Cross-Reference

### 6.1 External Forms Called

| Form | Method | Lines | Purpose |
|------|--------|-------|---------|
| `frmReportTurist` | Show() | 1313 | Tourist registration Crystal Report |
| `frmPrijavaBoravkaPodaci` | Show() | 1325 | Stay registration report |
| `GostiListingrpt` | Show() | 1336 | Guest listing report (with formaGosti reference) |
| `frmGrupeIzmjena` | ShowDialog() | 1342-1344 | Change guest group assignment |
| `frmPrijavaGostiKucice` | ShowDialog() | 1929-1933 | Edit guest data (prenos method) |
| `frmSobaInfoPromjena` | ShowDialog() | 1940-1950 | Change checkout date |
| `clasTZ` | prijavi(pr) | 1817, 1876 | Tourist registration API call |
| `HotelStatistika1` | SetDataSource/RefreshReport | 1697-1698 | Hotel statistics Crystal Report |
| `rptTuristicka` | SetDataSource/RefreshReport | 1707-1709 | Tourist report Crystal Report |
| `rptTurdrz` | SetDataSource/RefreshReport | 1719-1721 | Nationality statistics Crystal Report |

### 6.2 ModuleKod Functions Called

| Function | Lines | Purpose |
|----------|-------|---------|
| `mysqlReader()` | 1038, 1040, 1066, 1619, 1662, 1667, 1669, 1675, 1677, 1702, 1704, 1713, 1715, 2031, 2033, 2125 | Execute SELECT queries |
| `mysqlExScalar()` | 1821, 1823, 1825, 1880, 2138 | Execute UPDATE/INSERT queries |
| `funkcije.SelectDistinct()` | 2003 | DISTINCT filter on DataTable |
| `funkcije.WriteSQLError()` | 1082, 1202, 1235, 1265, 1405, 2040 | Log SQL errors |
| `funkcije.WriteSystemError()` | 1086, 1206, 1239, 1269, 1412, 2044 | Log system errors |
| `ds` (global DataSet) | Throughout | In-memory data cache shared across forms |

### 6.3 Stored Procedures Called

| Procedure | Line | Purpose |
|-----------|------|---------|
| `gosti1` | 1381-1384 | Get guest visit history by gostID |

### 6.4 Database Views Referenced

| View | Line | Purpose |
|------|------|---------|
| (none directly) | — | frmGosti builds all queries inline |

### 6.5 Related Schema Tables (from 00_DATABASE_SCHEMA.md)

| Table | Relationship to frmGosti |
|-------|--------------------------|
| `gosti` | Source of guest master data; searched directly in `gosti()` method |
| `relgostsoba` | Core table for all guest-room stay queries and TZ updates |
| `sobe` | Joined for room name (naziv) |
| `drzave` | Country lookup; filtered by `domaca=1`; joined by `DID` |
| `gostdokument` | Document type descriptions joined in getGostiTurist |
| `goststatus` | Referenced via `relgostsoba.status` column; types with tax amounts |
| `gostiknjiga` | Not directly used in frmGosti but e-stranac export produces similar data |
| `rezervacijegrupe` | Group names loaded and joined |
| `nocenja` | Joined for tariff pricing in guest list |
| `radnici` | Joined for worker names (checkInRadnik) |
| `sobatarifa` | Tariff names in reports |
| `printracspec` | Non-admin filter (`c.d1=relgostsoba.id`) in report queries |

---

## 7. Key Findings for Modern System

### 7.1 Critical Bugs

| # | Bug | Line(s) | Impact |
|---|-----|---------|--------|
| 1 | **SQL Injection**: TextBox3 text directly concatenated in WHERE clause (`gosti()` line 1990, `getGostir()` lines 2031/2033) | 1990, 2031, 2033 | Full SQL injection vulnerability |
| 2 | **SQL Injection**: DataGridView cell values directly concatenated into UPDATE statements (lines 1227, 1258, 1821, 1823, 1825, 1880, 2138) | Multiple | SQL injection via manipulated grid data |
| 3 | **Race Condition**: `MAX(estranac)+1` without locking (lines 1823, 2125-2138) | 1823, 2125-2138 | Duplicate estranac numbers under concurrent access |
| 4 | **tid field concatenation bug**: Line 1823 has `tid='{odg}, t.estranac=m.max+1'` — the comma and extra field are inside the tid string value | 1823 | Corrupted tid values |
| 5 | **Field misinterpretation**: Guest `adresa` (address) field parsed as `rok_vazenja_pi` (document expiry date) in TZ registration (lines 1812-1814) | 1812-1814 | Data integrity: address misused as document expiry |
| 6 | **Missing error handling**: Empty Catch blocks (lines 1366-1367, 1894-1895) swallow exceptions silently | 1366, 1894 | Errors hidden from users |

### 7.2 Architecture Anti-Patterns

| # | Pattern | Details |
|---|---------|--------|
| 1 | **God Form**: frmGosti handles listing, searching, TZ registration, e-stranac XML export, 3 types of Crystal Reports, and group management (2161 lines including designer code) | All in single form |
| 2 | **No separation of concerns**: SQL queries, UI binding, business logic, and XML generation mixed in event handlers | Throughout |
| 3 | **No data validation layer**: Grid cell values cast directly to types without null checks (CInt, CDate on potentially null grid values) | Lines 1787-1797, 1846-1856 |
| 4 | **No ORM/parameterized queries**: All SQL is string concatenation via MySqlCommand.CommandText | Throughout |
| 5 | **Hardcoded domestic country ID**: `did <> 23` assumes BiH country ID is always 23; should use `domaca=1` query result consistently | Lines 1038, 1075, 1792 |
| 6 | **Shared global DataSet**: `ds` (inferred from usage pattern) shared across all forms — no data isolation | Throughout |
| 7 | **Crystal Reports dependency**: Three report types embedded directly in form code | Lines 1696-1721 |
| 8 | **External API tight coupling**: clasTZ class directly called with URL parameter construction — no error recovery, no retry logic | Lines 1817, 1876 |

### 7.3 Business Logic to Preserve

| # | Rule | Implementation |
|---|------|----------------|
| 1 | Foreign citizen identification: domestic country determined by `drzave.domaca=1` flag; guests with different DID are foreign | Lines 1066-1073, 1038 |
| 2 | Document type mapping for TZ: values > 2 mapped to type 3 (Ostalo) | Line 1038 |
| 3 | Gender mapping for TZ: M→1, else→2 | Lines 1038, 1075 |
| 4 | TZ registration distinguishes "prijava" (new) vs "promjena" (update) based on `tid > 0` | Lines 1801-1802, 1860-1861 |
| 5 | Estranac (foreign registration book number) assigned sequentially only to foreign citizens | Lines 1822-1826, 2136-2138 |
| 6 | Print flags (R1, R2) mark which registration reports have been printed for a stay — cannot print twice | Lines 1227, 1258 |
| 7 | Guest search supports both room number (numeric) and name (text) filtering | Lines 1603-1607 |
| 8 | Admin users (nivo=9) see all guests; non-admin see only guests with print records (printracspec join) | Lines 1666-1704 |
| 9 | Checked-out guest search uses `odjavljen=1`; current guest display uses `odjavljen=0 AND rezervacija=0` | Lines 1062, 2031 |
| 10 | TZ registration data includes: ime, prezime, vrsta_isprave, pocetak_boravka, drzavljanstvo, broj_isprave, spol, datum_rodjenja, status_gosta, prestanak_boravka, mjesto_rodjenja, prijava_za_strance | Lines 1787-1816 |

### 7.4 Recommendations for Modern System

1. **Parameterize all queries** — Replace all string-concatenated SQL with parameterized queries
2. **Extract TZ registration** into a separate service with proper error handling, retry logic, and idempotency
3. **Use transaction for estranac assignment** — `SELECT MAX(estranac)+1` must be atomic with the UPDATE
4. **Separate address from document expiry** — Add a dedicated `gosti.dokumentExpiry` field instead of misusing `adresa`
5. **Extract form into components** — GuestList, GuestSearch, TZRegistration, GuestReports as separate components/pages
6. **Replace Crystal Reports** with a modern reporting library (e.g., RDLC, Stimulsoft, or web-based)
7. **Add proper guest CRUD** — This form only reads guests; creation/editing is in separate forms not analyzed here
8. **Enforce referential integrity** — The `tid` and `estranac` fields on `relgostsoba` should be validated against actual TZ responses
9. **Add guest duplicate detection** — No uniqueness check on document number or name+DOB combination exists
10. **Fix the tid concatenation bug** — Line 1823 produces corrupt data in single-registration mode for foreign citizens