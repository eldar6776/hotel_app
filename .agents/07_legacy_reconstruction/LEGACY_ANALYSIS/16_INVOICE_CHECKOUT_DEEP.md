# Deep Analysis: frmRacuni.vb — Invoice/Checkout Form

> Source: `legacy_code/frmRacuni.vb` (2117 lines)  
> This file upgrades and extends 16_INVOICE_CHECKOUT.md with complete function inventory, SQL inventory, business rules, and fiscal integration details extracted line-by-line.

---

## 1. Complete Function Inventory (frmRacuni.vb)

| # | Line Range | Name | Visibility | Parameters | Lines | What It Does |
|---|-----------|------|------------|------------|-------|-------------|
| 1 | 18-46 | `ucitajImena` | Private | none | 28 | Loads invoice headers via SP `getPrintHeader` into `dataTableHeader`, then calls `ucit_head()` |
| 2 | 47-85 | `ucit_head` | Public | none | 38 | Configures DataGridView1 columns from `dataTableHeader` (col widths, headers, visibility), calls `ocisti()` |
| 3 | 87-93 | `getnap` | Private | `brr As Integer` | 6 | SELECT `nap` from `printracunifooter` where `BrojRacuna=brr`, sets TextBox6 |
| 4 | 94-172 | `ucitajSredinu` | Public | none | 78 | Loads invoice detail via SP `getPrintDetalji(@RBR)`, populates DataGridView2, sets payment method combo, calls `getnap` |
| 5 | 173-245 | `det` | Private | none | 72 | SELECT from `printracspec` WHERE `broj=akcij2`, loads guest detail into `ds.Tables("det")` |
| 6 | 246-414 | `ucitajRacunPrint` | Private | none | 168 | Prepares invoice for printing: copies header/footer/detalji to `ds.Tables("printGore/printSredina/printFooter")`, handles advance vs regular, euro checkbox, writes XML schema, opens `rptRacunFrm` |
| 7 | 416-464 | `ucitajN` | Private | none | 48 | SELECT from `placanjenacin` WHERE `id<>4 AND id<>5 AND id<>6`, fills `cmbNacin` |
| 8 | 465-467 | `ucitajNacine` | Public | none | 2 | Empty stub (declared but no code) |
| 9 | 468-483 | `frmRacuni_Load` | Private | (Form Load event) | 15 | Sets checkboxes from settings, calls `loa()`, `ucitajKursnu()`, shows GroupBox2 if admin(level 9) |
| 10 | 484-515 | `ucitajKursnu` | Public | none | 31 | SELECT `Naziv_Valute, Vrijednost` from `kursna`, fills `cmbKursna` |
| 11 | 516-547 | `loa` | Public | none | 31 | Calls `ucitajN()`, `vratiSobe()`, `ucitajImena()`, `ucitajImenaAvans()`, selects first rows, calls `citajfirme()` |
| 12 | 550-557 | `ocisti` | Private | none | 7 | Clears text fields and DataGridView2 |
| 13 | 558-565 | `DataGridView1_CellClick` | Private | (event) | 7 | Calls `ocisti()` then `ucitajSredinu()` |
| 14 | 567-592 | `DataGridView1_CellMouseDoubleClick` | Private | (event) | 25 | Double-click on col 13 → fiscal print/storno, col 16 → recalculate (`prov`), col 0 → export |
| 15 | 594-634 | `prov` | Private | `ByValid As Integer, brr As Integer` | 40 | Reconciliation: recalculates insurance/tax/accommodation on invoice detail rows from `placanjedetalji`+`printracunidetalji` |
| 16 | 635-637 | `DataGridView1_Click` | Private | (event) | 2 | Empty handler |
| 17 | 639-641 | `Button1_Click` | Private | (event) | 2 | Close form |
| 18 | 644-651 | `Button2_Click` | Private | (event) | 7 | Log + call `ucitajRacunPrint()` for preview |
| 19 | 653-708 | `Button3_Click` | Private | (event) | 55 | Storno button: checks if already stornano, confirms, calls `storniraj()`, fiscal storno if applicable |
| 20 | 711-788 | `storniraj` | Private Function | returns Boolean | 77 | Transaction: UPDATE placanje/placanjeDetalji set storno=1, UPDATE printracuni set storno=1/exp=2/datstor, UPDATE troskovi reopen, DELETE troskovi TID=1, NSC file write |
| 21 | 790-800 | `DataGridView1_CellFormatting` | Private | (event) | 10 | Grays out stornirano rows |
| 22 | 802-832 | `vratiFooter` | Private | `rac As Integer` | 30 | Loads invoice footer via SP `getPrintFooter(@brojRAC)` |
| 23 | 836-888 | `ucitajImenaAvans` | Private | none | 52 | SELECT from `printracuniavans` ORDER BY BrojRacuna DESC, populates dgvAvansi |
| 24 | 892-901 | `dgvAvansi_CellFormatting` | Private | (event) | 9 | Grays out stornirano advance rows |
| 25 | 903-905 | `dgvAvansi_Click` | Private | (event) | 2 | Empty handler |
| 26 | 907-926 | `btnAvansPregled_Click` | Private | (event) | 19 | Advance invoice preview: calls `avans_print`, opens Crystal Report |
| 27 | 928-935 | `btnIzlazAvans_Click` | Private | (event) | 7 | Closes form, opens frmSobe |
| 28 | 937-955 | `btnAvansiBrisanje_Click` | Private | (event) | 18 | Advance storno: confirms, calls `stornirajAvans()`, grays row |
| 29 | 957-993 | `stornirajAvans` | Private Function | returns Boolean | 36 | Transaction: only `UPDATE printracuniavans SET storno=1 WHERE BrojRacuna=@Rbr` |
| 30 | 997-1078 | `#Region "neplaceni racuni"` | | | | Unpaid invoices section |
| 31 | 1000-1048 | `vratiNeplacene` | Private | none | 48 | SP `getNeplacene` → fills dgvneplaceni (admin level 9 only) |
| 32 | 1050-1078 | `vratiNeplaceneUkupno` | Private | none | 28 | SP `getNeplaceneSUM` → fills txtUkupno total |
| 33 | 1080-1115 | `vratiDetalje` | Private | `pid As Integer` | 35 | SP `getneplaceniDetalji(@PID)` → fills dgvneplaceniDetalji |
| 34 | 1117-1125 | `TabPage2_Enter` | Private | (event) | 8 | Selects first unpaid row |
| 35 | 1129-1144 | `dgvneplaceni_CellFormatting` | Private | (event) | 15 | Alternating gray/pink row colors for unpaid |
| 36 | 1148-1153 | `dgvneplaceni_Click` | Private | (event) | 5 | (Commented out call to vratiDetalje) |
| 37 | 1155-1184 | `vratiSobe` | Private | none | 29 | SELECT sobe.ID, sobe.naziv FROM sobe |
| 38 | 1187-1206 | `btnVratiuSobu_Click` | Private | (event) | 19 | "Return to room": validates room, calls updateSoba chain |
| 39 | 1208-1227 | `updateSoba` | Private | `Sid As Integer` | 19 | Orchestrates: updateTroska, updateGosta, updateneplaceni, nocenja, updateFolio |
| 40 | 1229-1256 | `updateTroska` | Private | `TID, sid As Integer` | 27 | SP `promijeniTrosak(@TID, @SID)` |
| 41 | 1258-1289 | `updateGosta` | Private | `relID, sid As Integer` | 31 | SP `promijeniRelGost(@relID, @SID, @CID=Now, @DOD=tomorrow)` |
| 42 | 1291-1319 | `updateneplaceni` | Private | `PID As Integer` | 28 | UPDATE `neplaceni SET placeno=1 WHERE PID=@PID` |
| 43 | 1321-1361 | `updateFolio` | Private | `pid, sobaid As Integer` | 40 | UPDATE `posjetaFolio SET SID=@Sid, zakljucen=0 WHERE ID=@ID` |
| 44 | 1363-1396 | `imaliSobe` | Private | `naz As String` | 33 | SELECT sobe WHERE room name matches AND ID NOT IN occupied rooms |
| 45 | 1398-1412 | `nocenja` | Private | `brojGostiju, sid, pid As Integer, sobanaziv As String` | 14 | Calculates tariff split, calls `dodajnocenja` for each guest |
| 46 | 1414-1443 | `dodajnocenja` | Private | `gid, DatumP, Tarifa, SID, PID, opis, ssoba` | 29 | SP `Unesinocenja(@RID, @DatumPp, @Tarifa, @SID, @PID, @opis, @ssoba)` |
| 47 | 1447-1451 | `#Region "Predracuni"` | | | | Empty region |
| 48 | 1453-1459 | `btnUnos_Click` | Private | (event) | 6 | Opens frmPredracun |
| 49 | 1461-1469 | `tbpAvansi_Enter` | Private | (event) | 8 | Selects first advance row |
| 50 | 1472-1474 | `DataGridView1_CellContentClick` | Private | (event) | 2 | Empty handler |
| 51 | 1475-1938 | `printfisc` | Private | `br As Integer, id As Integer, st As Byte` | 463 | **Fiscal printing**: branches by fsc(0) value — 4=KTE, 5=ELN, 2=HCP/NSC, 7=Mikroelektronika(XML), 6=simple format, 3=Tring DLL |
| 52 | 1942-1983 | `snimi_trosak` | Private | naziv, kol, cij, ukupno, sifra, porez, rac, racun, dod, dod1, dod2, dod3 | 41 | INSERT INTO `sifarnik` (naziv, kol, cij, ukupno, sifra, porez, racu, racun, dod, dod1, dod2, dod3, placanje) — tracks fiscal receipt items |
| 53 | 1985-2010 | `sifraGet` | Private Function | returns Integer | 25 | SELECT MAX(sifarnik.sifra) FROM sifarnik, returns max+1 (min 4) |
| 54 | 2013-2015 | `Button4_Click` | Private | (event) | 2 | Empty handler |
| 55 | 2017-2033 | `Button5_Click` | Private | (event) | 16 | Pre-dracun preview: opens rptRacunFrm with `printpredr()` |
| 56 | 2036-2038 | `Button6_Click` | Private | (event) | 2 | Empty handler |
| 57 | 2040-2045 | `dgvPredracuni_CellClick` | Private | (event) | 5 | Loads pred-detalji on click |
| 58 | 2047-2049 | `dgvPredracuni_CellContentClick` | Private | (event) | 2 | Empty handler |
| 59 | 2051-2053 | `dgvPredracuni_CellDoubleClick` | Private | (event) | 2 | Empty handler |
| 60 | 2055-2065 | `DataGridView3_CellClick/ContentClick/DoubleClick` | Private | (event) | 2 each | Empty handlers |
| 61 | 2067-2069 | `TabPage2_Click` | Private | (event) | 2 | Empty handler |
| 62 | 2071-2078 | `TabControl1_SelectedIndexChanged` | Private | (event) | 7 | If tab = Predracuni, calls `ucitPred()` |
| 63 | 2083-2123 | `ucitPred` | Private | none | 40 | SELECT from `predracuni ORDER BY id desc` → dgvPredracuni |
| 64 | 2124-2165 | `ucitPreddet` | Private | `id As Integer` | 41 | SELECT from `predracunidet WHERE predid={id}` → DataGridView3 |
| 65 | 2168-2170 | `BrisiToolStripMenuItem_Click` | Private | (event) | 2 | Empty handler |
| 66 | 2172-2174 | `SelektujSveToolStripMenuItem_Click` | Private | (event) | 2 | Empty handler |
| 67 | 2184-2192 | `cmbNacin_SelectedIndexChanged` | Private | (event) | 8 | UPDATE `placanje SET nacin={val}` and `printracunidetalji SET nacinid={val}, nacin='{text}' WHERE BrojRacuna={br}` |
| 68 | 2196-2202 | `chkEuro_CheckedChanged` | Private | (event) | 6 | Saves `My.Settings.PrikaziEur` |
| 69 | 2204-2213 | `TextBoxX1_TextChanged` | Private | (event) | 9 | Filters dataTableHeader by BrojRacuna |
| 70 | 2215-2224 | `TextBoxX2_TextChanged` | Private | (event) | 9 | Filters dataTableHeader by ime |
| 71 | 2226-2235 | `TextBoxX3_TextChanged` | Private | (event) | 9 | Filters dataTableHeader by DrugoIme |
| 72 | 2237-2241 | `Button7_Click` | Private | (event) | 4 | Clear search, reset filter |
| 73 | 2243-2247 | `Button8_Click` | Private | (event) | 4 | Clear search, reset filter |
| 74 | 2249-2253 | `Button9_Click` | Private | (event) | 4 | Clear search, reset filter |
| 75 | 2255-2291 | `Button10_Click` | Private | (event) | 36 | Admin edit invoice: UPDATE printracuni, placanje, printracunifooter with new guest/firm/name/note |
| 76 | 2293-2338 | `txtDrugoIme_Leave` | Private | (event) | 45 | Lookup partner from `firme` table, populate address/PDV fields |
| 77 | 2340-2342 | `txtDrugoIme_GotFocus` | Private | (event) | 2 | Highlight text field |
| 78 | 2344-2348 | `txtDrugoIme_KeyDown` | Private | (event) | 4 | Enter key → focus next field |
| 79 | 2353-2355 | `DataGridView1_CellMouseDown` | Private | (event) | 2 | Empty handler |
| 80 | 2357-2361 | `Button11_Click` | Private | (event) | 4 | Clear filter |
| 81 | 2363-2372 | `TextBoxX5_TextChanged` | Private | (event) | 9 | Filters dataTableHeader by BrojSobe |
| 82 | 2374-2376 | `DataGridView2_CellContentClick` | Private | (event) | 2 | Empty handler |
| 83 | 2378-2384 | `chkpkg_CheckedChanged` | Private | (event) | 6 | Saves `My.Settings.gostisvi` |
| 84 | 2386-2388 | `tbpPredracuni_Click` | Private | (event) | 2 | Empty handler |
| 85 | 2390-2392 | `TextBox6_TextChanged` | Private | (event) | 2 | Sets `TextBox6.Tag = 1` (marks note as edited) |

---

## 2. Complete SQL Inventory (frmRacuni.vb)

### 2.1 Stored Procedures

| Line | SP Name | Parameters | Purpose |
|------|---------|------------|---------|
| 20 | `getPrintHeader` | none | Load all invoice headers into grid |
| 109 | `getPrintDetalji` | @RBR (Int16) | Load invoice detail lines for selected invoice |
| 807 | `getPrintFooter` | @brojRAC (Int16) | Load invoice footer (avans, nights, notes) |
| 1007 | `getNeplacene` | none | Load unpaid balances (admin) |
| 1053 | `getNeplaceneSUM` | none | Sum of all unpaid |
| 1084 | `getneplaceniDetalji` | @PID (Int16) | Expense details for unpaid record |
| 1231 | `promijeniTrosak` | @TID, @SID | Move expense to different room |
| 1260 | `promijeniRelGost` | @relID, @SID, @CID, @DOD | Move guest to different room |
| 1416 | `Unesinocenja` | @RID, @DatumPp, @Tarifa, @SID, @PID, @opis, @ssoba | Insert night record for returned guest |

### 2.2 Direct SQL Statements

| Line | Operation | Table(s) | Condition/Purpose |
|------|-----------|----------|-------------------|
| 89 | SELECT | `printracunifooter` | `WHERE BrojRacuna={brr}` — get invoice note |
| 203 | SELECT | `printracspec` | `WHERE broj={akcij2}` — get invoice specification |
| 423 | SELECT | `placanjenacin` | `WHERE id<>4 AND id<>5 AND id<>6` — load payment methods |
| 486 | SELECT | `kursna` | `Naziv_Valute, Vrijednost` — load exchange rates |
| 596 | SELECT | `placanjedetalji` | `WHERE p.brojID={ByValid} AND art=1` — get accommodation payment details |
| 600 | SELECT | `printracunidetalji` | `WHERE p.BrojRacuna={ByValid} AND trosakId=1` — get invoice items for reconciliation |
| 620 | UPDATE | `printracunidetalji` | `SET CijBezPdv={osig}, UkupnoBezPdv={calc}, Pdv='0%', IznosPdv='0', Ukupno={calc} WHERE id={ido}` — recalculate insurance |
| 621 | UPDATE | `printracunidetalji` | `SET CijBezPdv={taxa}, UkupnoBezPdv={calc}, Pdv='0%', IznosPdv='0', Ukupno={calc} WHERE id={idt}` — recalculate tax |
| 627 | UPDATE | `printracunidetalji` | `SET CijBezPdv={calc}, UkupnoBezPdv={calc}, Pdv='17%', IznosPdv={calc}, Ukupno={u} WHERE id={idn}` — recalculate net accommodation |
| 731 | UPDATE | `placanje` | `SET storno=1 WHERE broj=@Rbr` — storno payment |
| 734 | UPDATE | `placanjeDetalji` | `SET storno=1 WHERE brojID=@Rbr` — storno payment details |
| 738 | UPDATE | `printracuni` | `SET storno=1, exp=2, datstor='{Now}' WHERE BrojRacuna=@Rbr` — storno invoice header |
| 741 | UPDATE | `troskovi` | `SET zaklj=0, Brrac=null WHERE Brrac=@Rbr AND TID<>1` — reopen non-accommodation expenses |
| 743 | DELETE | `troskovi` | `WHERE Brrac=@Rbr AND TID=1` — delete accommodation expenses |
| 839 | SELECT | `printracuniavans` | `ORDER BY BrojRacuna DESC` — load advance invoices |
| 978 | UPDATE | `printracuniavans` | `SET storno=1 WHERE BrojRacuna=@Rbr` — storno advance invoice |
| 1159 | SELECT | `sobe` | `ID, naziv FROM sobe` — load room list |
| 1293 | UPDATE | `neplaceni` | `SET placeno=1 WHERE PID=@PID` — mark unpaid as paid |
| 1333 | UPDATE | `posjetaFolio` | `SET SID=@Sid, zakljucen=0 WHERE ID=@ID` — reopen folio |
| 1367 | SELECT | `sobe` | `WHERE sobe.naziv=@naziv AND sobe.ID NOT IN (SELECT sobaID FROM relgostsoba WHERE odjavljen=0)` — find free room |
| 1946 | INSERT | `sifarnik` | `VALUES(naziv, kol, cij, ukupno, sifra, porez, racu, racun, dod, dod1, dod2, dod3, placanje=1)` — save fiscal receipt item |
| 1988 | SELECT | `sifarnik` | `MAX(sifra) FROM sifarnik` — get next fiscal item code |
| 2087 | SELECT | `predracuni` | `ORDER BY id desc` — load proforma invoices |
| 2127 | SELECT | `predracunidet` | `WHERE predid={id}` — load proforma details |
| 2189 | UPDATE | `placanje` | `SET nacin={val} WHERE broj={br}` — change payment method |
| 2190 | UPDATE | `printracunidetalji` | `SET nacinid={val}, nacin='{text}' WHERE BrojRacuna={br}` — change payment method on invoice line items |
| 2272 | UPDATE | `printracuni` | `SET idkl='{tag}', peri='{text}', datr='{date}', TipPlacanja='{text}', Ime='{text}', DrugoIme='{firm}' WHERE BrojRacuna='{cell}'` — edit invoice guest info |
| 2273 | UPDATE | `placanje` | `SET firma='{tag}' WHERE broj='{cell}'` — update firm on payment |
| 2279 | UPDATE | `printracunifooter` | `SET nap='{text}' WHERE BrojRacuna='{cell}'` — update invoice note |

---

## 3. Business Rules (frmRacuni.vb)

### 3.1 Status & Validation Checks

| Rule | Line | Condition | Effect |
|------|------|-----------|--------|
| Storno check before action | 656 | `DataGridView1.Rows(i).Cells(8).Value = True` | If already stornirano, shows "Racun je storniran" msg, offers fiscal storno |
| Fiscal storno offer | 658-671 | `fsc(0) = 3 Or fsc(0) = 7 Or fsc(0) = 2` | Prompts "Zelite li stornirati fiskalni racun?" |
| Confirm storno | 677 | `MsgBox OkCancel` | User must confirm before `storniraj()` |
| Fiscal confirm print | 573 | `MsgBox YesNo "Zelite li stampati fiskalni racun!"` | Only if fiscal not yet printed (cell value empty/0) |
| Fiscal confirm copy | 577 | `MsgBox YesNo "kopiju fiskalnog racuna"` | When fiscal number already exists |
| Admin access for edit | 2256 | `dozvole(6) = 0` | Blocks edit if no permission |
| Admin access for unpaid | 475 | `sbNivo.Text = 9` | Shows unpaid section only for admin level 9 |
| Confirm name change | 2262 | `MsgBox YesNo "izmjeniti naziv"` | Must confirm before updating invoice name |
| Payment method change confirm | 2187 | `MsgBox YesNo "promjeniti nacin placanja"` | Must confirm before changing payment method |
| No detail = skip actions | 645 | `DataGridView2.Rows.Count = 0` | Return early if no items |
| Euro display setting | 2196 | `chkEuro.Checked` | Saves to My.Settings.PrikaziEur |
| Pkg display setting | 2378 | `chkpkg.Checked` | Saves to My.Settings.gostisvi |
| Prevent empty room | 1200 | `dtVratiSobu.Rows.Count = 0` | "Soba nepostoji ili je zauzeta" |
| Must be numeric room | 1188 | `IsNumeric(txtVratiuSobu.Text)` | "Unesite broj sobe" if not |

### 3.2 Storno Rules (Deep)

**Regular Invoice Storno** (`storniraj()`, lines 711-788):
1. Transaction started
2. `UPDATE placanje SET storno = 1 WHERE broj = @Rbr`
3. `UPDATE placanjeDetalji SET storno = 1 WHERE brojID = @Rbr`
4. `UPDATE printracuni SET storno = 1, exp=2, datstor='{Now}' WHERE BrojRacuna = @Rbr`
5. `UPDATE troskovi SET zaklj = 0, Brrac = null WHERE Brrac = @Rbr AND TID <> 1` — reopen non-accommodation
6. `DELETE FROM troskovi WHERE Brrac = @Rbr AND TID = 1` — remove accommodation expenses
7. **If fsc(0) = 22 (NSC)**: Write storno file `rr.in` with command 48
8. **If fsc(0) = 3 (Tring)**: No special handling (storno handled separately)
9. Transaction committed; on error → rollback

**Advance Invoice Storno** (`stornirajAvans()`, lines 957-993):
1. Transaction started
2. `UPDATE printracuniavans SET storno = 1 WHERE BrojRacuna = @Rbr`
3. Only this single UPDATE — does NOT affect payments, expenses, or folios

### 3.3 Reconciliation Logic (`prov()`, lines 594-633)

Recalculates insurance/tax/accommodation on an invoice:
1. Fetch `placanjedetalji` WHERE `brojID=ByValid AND art=1` (accommodation payment details)
2. Fetch `printracunidetalji` WHERE `BrojRacuna=ByValid AND trosakId=1` (invoice detail items)
3. Among fetched items, identify:
   - "Osiguranje" (insurance) row → update with `setings.osig * quantity`
   - "Boravisna Taksa" (tourist tax) row → update with `setings.taxa * quantity`
   - Remaining row → recalculate: `(total - insurance - tax) / quantity = unit price`, then split PDV 17%
4. All three UPDATE operations are done without a transaction

---

## 4. Invoice Creation as Snapshot

The invoice creation flow in frmRacuni.vb **does not create invoices** — it only **views** them. Invoice creation happens in `frmPlacanje.vb`. However, the **print preparation** in frmRacuni.vb creates a snapshot structure:

### 4.1 Print Preparation (`ucitajRacunPrint()`, lines 246-414)

1. **Determine source**: If on Advance tab → use `dataTableHeaderA`, else → use `dataTableHeader`
2. **Copy header to `printGore`** (in-memory DataTable):
   - Column 0 ← Item(0) = BrojRacuna
   - Column 1 ← Item(2) = Ime
   - Column 2 ← Item(4) = PeriodOd
   - Column 3 ← Item(5) = PeriodDo
   - Column 4 ← Item(7) = BrojSobe
   - Column 5 ← Item(6) = Tip
   - Column 6 ← Item(3) = DrugoIme(Firma)
   - Column 7 ← Item(1) = (hidden column)
   - Column 8 ← Item(9) = (date)
   - Column 9 ← Item(10) = (hidden)
   - Column 10 ← Item(11) = (hidden)
   - Column 11 ← Item(12) = (hidden)
3. **Footer** (`vratiFooter`): Calls `getPrintFooter(@brojRAC)` → `dataTableFooter`
   - Item(0) = BrojRacuna (int)
   - Item(1) = Avansno (decimal) — advance payments
   - Item(2) = Nocenja (decimal) — prior night payments
   - Item(3) = nap (string) — notes
   - Item(4) = pri (unknown)
4. **Detail lines**: Copied from `dataTableDole` to `printSredina`:
   - Columns 0-13 mapped directly from detail DataTable columns
   - Column 10 (Euro price) conditioned on `chkEuro.Checked`
   - Virtual column "avan" = footer.Avansno + footer.Nocenja (total prepaid)
   - Virtual column "rad" = DataGridView printime column
5. **Derived values**:
   - `avans` text = "Avansna uplata: {Avansno}" (if Avansno ≠ 0)
   - `noc` text = "Ranije uplate nocenja: {Nocenja}" (if Nocenja ≠ 0)
   - Note text from footer, appended with fiscal legal note

### 4.2 Fiscal Device Output

Each fiscal device type gets different output format:

| fsc(0) | Type | Output | Evidence |
|--------|------|--------|----------|
| 2 | HCP/NSC | File-based `.in` format with command codes (107, 48, 52, 51, 53, 55, 56) | Lines 1481-1579 |
| 3 | Tring | Uses `Tring.Fiscal.Driver.TringFiskalniPrinter` DLL | Lines 1762-1938 |
| 4 | KTE | Calls `FMRacunKTE(id)` | Line 1477 |
| 5 | ELN | Calls `FMRacunE(id)` | Line 1479 |
| 6 | Simple | File format `S,1,______,_,__;name;price;qty;1;1;tax;0;code;` | Lines 1713-1756 |
| 7 | Mikroelektronika | XML format `<RECEIPT><DATA BCR/VAT/MES/DEP/DSC/PRC/AMN>` | Lines 1580-1712 |
| 22 | NSC-Storno | File format with command `48,1,______,_,__` | Lines 747-770 |

### 4.3 Fiscal Response Storage (`snimFiskal` in frmGlavni.vb:1810)

When fiscal device responds:
- `UPDATE printracuni SET fisrac='{receiptNumber}', fisvr='{timestamp}', fisIZN='{amount}' WHERE BrojRacuna={id}`
- `UPDATE printracunifooter SET nap=concat(nap, 'Po clanu 42 fiskalnog zakona... Br. {receiptNumber}') WHERE BrojRacuna={id}`

In frmRacuni.vb, the `snimFiskal` equivalent (line 1908) calls `snimFiskal(brfisrac, datfisrac + " " + vrfisrac, IZN, id)`.

Also in frmRacuni.vb, `snimi_trosak` (line 1942) inserts fiscal receipt items into `sifarnik` table for tracking purposes.

---

## 5. Storno Logic (Deep)

See section 3.2 above for the complete storno flow. Additional details:

### 5.1 Regular Invoice Storno — Post-Transaction Actions

After `storniraj()` succeeds:
- Logs storno event: `funkcije.logs("storniranjeRacuna", Now(), "Broj Racuna: ... iznos: ...")`
- Shows success message
- Closes frmRacuni
- Opens frmSobe

After `storniraj()` fails:
- Shows "Storniranje racuna nije uspjelo!"

### 5.2 Advance Invoice Storno — Post-Transaction Actions

After `stornirajAvans()` succeeds:
- Logs: `funkcije.logs("stornoAvansa", ...)`
- Shows success message
- Sets cell value = True and grays row

### 5.3 Fiscal Storno

When user confirms storno and fiscal device exists:
- If fsc(0) = 3 or 7 or 2: prompts "Zelite li stornirati fiskalni racun!"
  - Calls `printfisc(12, BrojRacuna, 1)` for storno mode
  - For Tring (fsc(0)=3): calls `prFiskal.StampatiReklamiraniRacun(rac)` 
  - For Mikroelektronika (fsc(0)=7): writes XML `<COMMAND><DATA CMD="REFUND_ON" NUM="{br}">`
  - For HCP (fsc(0)=2): writes file-based storno command

---

## 6. Partial Checkout / Return to Room

### 6.1 "Return to Room" Flow (`btnVratiuSobu_Click`, lines 1187-1205)

Only visible when `sbNivo.Text = 9` (admin level 9).

1. User enters room number in `txtVratiuSobu`
2. `imaliSobe(txtVratiuSobu.Text)` — Checks if room exists AND is not occupied (not in relgostsoba WHERE odjavljen=0)
3. If room found: `updateSoba(sid)` orchestrates:
   - `updateTroska(TID, SID)` — SP `promijeniTrosak`: moves each expense to new room
   - `updateGosta(relID, SID, Now, Tomorrow)` — SP `promijeniRelGost`: moves guest to new room
   - `updateneplaceni(PID)` — Marks unpaid record as paid (`placeno=1`)
   - `nocenja(count, SID, PID, roomName)` → `dodajnocenja(gid, Now, Tarifa, SID, PID, "", roomName)` — SP `Unesinocenja`: creates new night records
   - `updateFolio(PID, SID)` — Reopens folio: `UPDATE posjetaFolio SET SID={SID}, zakljucen=0 WHERE ID={PID}`

### 6.2 Unpaid Balances Section

Visible only for admin level 9. Shows:
- `vratiNeplacene()` — SP `getNeplacene` → list of guests with unpaid amounts
- `vratiNeplaceneUkupno()` — SP `getNeplaceneSUM` → total unpaid amount
- `vratiDetalje(PID)` — SP `getneplaceniDetalji(@PID)` → expense details for selected unpaid guest

---

## 7. Print/Fiscal Integration (Deep)

### 7.1 Fiscal Device Configuration

The `fsc` array is parsed from `ds.Tables("setings").Rows(0).Item("fiscal")` by splitting on "*":
- **fsc(0)** = device type code (2=HCP/NSC, 3=Tring, 4=KTE, 5=ELN, 6=Simple, 7=Mikroelektronika, 22=NSC-Storno)
- **fsc(1)** = device identifier / IP
- **fsc(2)** = file path / port
- **fsc(3)** = data path / COM port
- **fsc(4)** = optional sub-configuration (e.g., "DP55" for certain devices)
- **fsc(5)** = price mode flag (1=total price per item, else unit price)

### 7.2 Tring Fiscal Printer (fsc(0)=3) Details

Most complex integration (lines 1762-1938):
1. Initialize: `prFiskal.Inicijalizacija(fsc(3), fsc(2), 0, "0")`
2. If fails: up to 3 retry attempts with user confirmation
3. Create `Tring.Fiscal.Driver.Artikal` for each line item:
   - `ARRT.Sifra` = sequence number + 100
   - `ARRT.Naziv` = item name (max 16 chars)
   - `ARRT.Cijena` = price (total if fsc(5)="1", unit otherwise)
   - `ARRT.Stopa` = PDV tax rate based on `setings.pdv` and per-item rate
   - `ARRT.JM` = "Kom" (unit of measure)
4. Create `Tring.Fiscal.Driver.RacunStavka` with quantity and discount=0
5. Add payment method: Cash/Kartica/Virman based on payment method text
6. If storno: `prFiskal.StampatiReklamiraniRacun(rac)` with BrojRacuna
7. If normal: `prFiskal.StampatiFiskalniRacun(rac)`
8. On success: parse response → `snimFiskal()`, show success, close form, open frmSobe

### 7.3 Mikroelektronika (fsc(0)=7) Details

1. **Storno**: Writes XML `<COMMAND><DATA CMD="REFUND_ON" NUM="{br}">` and creates `CMD.OK` file
2. **Normal invoice**: Writes XML `<RECEIPT>` with `<DATA BCR/VAT/MES/DEP/DSC/PRC/AMN>` per line item
3. Special item codes: "Osiguranje" → Sifra=1, "Boravisna Taksa" → Sifra=2, else → `sifraGet()` (auto-increment from sifarnik)
4. For "Restoran" items (type 3): treated separately with different handling
5. Copies XML to fiscal device directory, creates `CMD.OK` trigger file
6. Response read by `FileSystemWatcher1_Created` (frmGlavni.vb)

### 7.4 HCP/NSC (fsc(0)=2) Details

1. Creates `rrH.in` file with line-by-line item commands:
   - `107,1,______,_,__;2;{porez};{sifra};{cijena};{naziv}` — Define item
   - `107,1,______,_,__;4;{sifra};{cijena}` — Set price
   - `48,1,______,_,__;{fisc_id};20;{sif};;;{storno flag}` — Header
   - `52,1,______,_,__;{sifra};{kol};{pop}` — Quantity
   - `51,1,______,_,__;` — Subtotal
   - `53,1,______,_,__;{payment_type};{amount};` — Payment type
   - `53,1,______,_,__;{payment_type};{amount};#{location}` — With location
   - `55,1,______,_,__;` — Customer (optional)
   - `56,1,______,_,__;` — End
2. File copy to fiscal device path
3. Response handled by `FileSystemWatcher1_Changed` in frmGlavni.vb
4. Payment type mapping: 0=Cash, 1=Card, 3=BankTransfer

### 7.5 Simple (fsc(0)=6) Details

1. Creates `{fsc(3)}\rr.in` file with:
   - `S,1,______,_,__;{name};{price};{qty};1;1;{taxCode};0;{sifra};` per item
   - `T,1,______,_,__;` at end
2. Copies to `{fsc(3)}\rr.inp`
3. No response handling visible

---

## 8. Error Messages and Edge Cases

### 8.1 Error Messages

| Line | Message (Bosnian) | Context |
|------|-------------------|---------|
| 32 | "Greska u konekciji sa bazom podataka!-19" | ucitajImena MySQL exception |
| 38 | "Greska!-r19" | ucitajImena system exception |
| 657 | "Racun je storniran" | Storno check — already cancelled |
| 660 | "Zelite li stornirati fiskalni racun!" | Fiscal storno confirmation |
| 677 | "Jeste li sigurni da zelite da stornirate racun?" | Storno confirmation |
| 666 | "Nema fiskalnog racuna za storniranje!" | Fiscal storno — no receipt to cancel |
| 693 | "Racun je uspjesno storniran!" | Storno success |
| 699 | "Storniranje racuna nije uspjelo!" | Storno failure |
| 2313 | "U sistemu ima vise partnera sa predlozenim nazivom!" | Multiple company name matches |
| 1201 | "Soba nepostoji ili je zauzeta" | Return to room — room not found |
| 1203 | "Unesite broj sobe" | Return to room — no room number |
| 1777 | "Fiskalni uređaj nije spreman za rad..." | Tring init failure |
| 1493 | "nema podataka za pravljenje racuna..." | No detail data for fiscal print |
| 2187 | "Sigurni ste da zelite promjeniti nacin placanja!" | Payment method change confirm |
| 2262 | "Sigurni ste da zelite izmjeniti naziv gosta ili firme!" | Invoice edit confirm |
| 2257 | "Nemate pristup ovoj opciji!!!" | Permission denied for invoice edit |
| 940 | "Racun je storniran" | Advance invoice already stornirano |
| 943 | "Jeste li sigurni da zelite da stornirate račun?" | Advance storno confirm |
| 946 | "Avansni racun je uspjesno storniran!" | Advance storno success |
| 948 | "Storniranje racuna nije uspjelo!" | Advance storno failure |

### 8.2 Edge Cases

1. **Fiscal retry logic** (lines 1776-1796): If Tring init fails, offers up to 2 retries with confirmation dialogs before giving up
2. **Fiscal response tracking** (line 1624): `filvocbr` variable prevents duplicate processing of the same fiscal response file
3. **sifraGet minimum** (line 2008): Returns minimum value of 4 if MAX(sifra) is below 4 or no records exist
4. **Empty radio flag** (line 86): `radi As Byte = 0` — Used to suppress `cmbNacin_SelectedIndexChanged` during initial load (checked at line 2185, set to 1 during load at line 470, then 0 at line 482)
5. **Advance invoice preview** (lines 911-916): If `CheckBox1.Checked = False` AND stornirano = True, calls `avans_print()` with parameter 1 (storno mode); otherwise parameter 0
6. **Name field splitting** (lines 100-106): `DrugoIme` field split by `vbCrLf` into up to 4 sub-fields (txtDrugoIme, txtdrime1, txtdrime2, txtpdv)
7. **Euro display toggle** (lines 344-349, 382-386): Column 10 of printSredina shows Euro price only if `chkEuro.Checked`; otherwise shows "-"
8. **Pkg flag** (line 398): If `chkpkg.Checked`, sets `frm.rimep = 1` before showing report
9. **Storno flag in report** (lines 401-413): If invoice is stornirano, sets `akcij2 = "storno"` before showing report
10. **Reconciliation rounding** (line 627): Net accommodation price calculated as `uk - osig - tax` per night, then split: `CijBezPdv = u / kol`, `UkupnoBezPdv = u / 1.17`, `IznosPdv = u - (u/1.17)` — hardcoded 17% PDV rate