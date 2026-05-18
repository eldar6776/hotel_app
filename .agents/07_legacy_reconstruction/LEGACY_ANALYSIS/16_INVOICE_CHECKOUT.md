# Legacy Invoice (Računi) and Checkout (Odjava) Flow Analysis

> Source files analyzed:
> - `legacy_code/frmRacuni.vb` — Invoice list/form (main invoice management)
> - `legacy_code/frmRacun.vb` — Minimal invoice dialog (19 lines, only closes form)
> - `legacy_code/frmRaccopy.vb` — Cross-database invoice copy utility
> - `legacy_code/frmOdjava1.vb` — Guest/room checkout form
> - `legacy_code/frmodjG.vb` — Checkout price dialog (minimal, 10 lines)
> - `legacy_code/rptRacunFrm.vb` — Invoice report/print form
> - `legacy_code/OdjavaReport.vb` — Crystal Reports auto-generated checkout report class
> - `legacy_code/Data.vb` — Core OdjavaSobe/PrljavaSoba/pripremaRcuna functions
> - `legacy_code/frmPlacanje.vb` — Payment/invoice creation form (referenced for cross-reference)
> - `legacy_code/ModuleKod.vb` — Module-level functions (referenced for cross-reference)

---

## 1. Business Flow: Invoice and Checkout

### 1.1 Invoice Lifecycle

1. **Creation (frmPlacanje.vb)**: When a payment is processed, a set of "snapshot" tables is populated:
   - `printracuni` — Invoice header (BrojRacuna, guest name, dates, room, payment type, storno flag, fiscal info)
   - `printracunidetalji` — Invoice line items (charge, quantity, PDV, amount, payment method, discount)
   - `printracunifooter` — Invoice footer (advance payments, nights count, notes)
   - `printracspec` — Invoice specification (guest details, room, tariff)
   - `placanje` — Payment record (broj, PID, amount, date, method, worker)
   - `placanjeDetalji` — Payment detail lines (brojID=broj from placanje, art, quantity, price, amount, nights)
   - `placanjeSlozeno` — Complex payment breakdown (RBR, nacin, iznos)
   - `troskovi` — Expenses are marked as closed (zaklj=1) with Brrac=invoice number

2. **Advance invoice (printracuniavans + printracunidetaljiavans)**: Created for advance payments before full checkout.

3. **Viewing/frmRacuni.vb**: Lists all invoices from `printracuni` via SP `getPrintHeader`. Selecting one loads detail lines via `getPrintDetalji` and footer via `getPrintFooter`. Advance invoices shown in separate tab via `printracuniavans`.

4. **Printing (rptRacunFrm.vb)**: Populates `iMediaIzvjestaj.izvjestaj30` DataTable from `printSredina`/`printGore`/`printFooter` dataset tables, sets fiscal legal note based on storno status (f15=0 for storno, f15=1 for normal).

5. **Storno**: Sets `storno=1` on `placanje`, `placanjeDetalji`, `printracuni` (also sets `exp=2`, `datstor=current timestamp`). Expenses with `TID <> 1` are reopened (zaklj=0, Brrac=null). Accommodation expenses (TID=1) are deleted. Storno on advance invoice only sets `printracuniavans.storno=1`.

6. **Fiscal printing**: Multiple fiscal device integrations (KTE/fsc(0)=4, ELN/fsc(0)=5, HCP/fsc(0)=2, NSC/fsc(0)=22, Tring/fsc(0)=3, Mikroelektronika/fsc(0)=7, fsc(0)=6). Fiscal data saved back via `snimFiskal` into `printracuni.fisrac/fisvr/fisIZN` and `printracunifooter.nap`.

### 1.2 Checkout Lifecycle (Odjava)

1. **Load form (frmOdjava1.vb)**: Calls SPs `getOdjavaCombo` (guest names), `getNeodjavljeneSobe1` (rooms not checked out), `vratiGostSoba` (guest-room details for selected room).

2. **Calculate totals**:
   - `Cijnocenja()` — Night charges (calls `vratiCijenunocenja` for each guest, adds insurance/tax per night from `setings.osig` and `setings.taxa`)
   - `vratiUplatunocenja(PID)` — SP `getPlacanjenocenja` returns paid amount for accommodation
   - `vratiTrosakSoba(SID)` — SP `vratiTrosakSoba` returns expense total
   - `izlistajPlacanja()` — SP `getPlacanjaPID` lists payments, sums them
   - `imeGrupe(SID)` — Gets reservation group name
   - `racunajUkupno()` — Remaining nights = total nights - paid nights; Total = remaining + expenses + paid
   - `racunajZaPlatit()` — Amount due = total - paid - advance

3. **Partial guest checkout (btnOdjavaGosta_Click)**: If only one guest remains, message "Soba nemoze ostati bez gostiju". Otherwise, opens `frmodjG` dialog for per-guest price, calls `OdjavaSobe(price/os, RID, SID, PID, date, uplataNocenja)`.

4. **Full room checkout (btnOdjavaSobe_Click)**: Checks room exists with guests → warns about unpaid balance → calls `OdjavaSobe(0, 0, SID, PID, checkoutDateTime, uplataNocenja)` → `PrljavaSoba(SID)` (marks room dirty) → if unpaid, calls `neplaceno()`.

5. **neplaceno()**: Creates unpaid record chain:
   - `dodajNocenje(SID)` — INSERT INTO troskovi (TID=25=nocenje, zaklj=1)
   - `vratiTroskoveSobe(SID)` — SP `getTroskoviSoba` gets all open expenses
   - `dodajneplacene(PID, TID, SID)` — INSERT INTO neplaceni (PID, DatumOdlaska, SID, TID, placeno=0) for each expense
   - `dodajDetalje(PID, zaPlacanje, troskoviGosta)` — INSERT INTO neplaceniplacanja (PID, IznosZaPlatit, IznosAvans)

### 1.3 Cross-database Copy (frmRaccopy.vb)

Copies invoice records between databases (e.g., "radna" ↔ annual archive) by:
- SELECT from source `printracuni` with replacement `BrojRacuna`
- INSERT INTO target for each of 7 tables: `placanje`, `placanjeDetalji`, `placanjeSlozeno`, `printracspec`, `printracuni`, `printracunidetalji`, `printracunifooter`

---

## 2. SQL Inventory

### 2.1 Stored Procedures Called

| SP Name | Called From | Parameters | Purpose |
|---------|------------|------------|---------|
| `getPrintHeader` | frmRacuni.ucitajImena | none | List all invoices for grid |
| `getPrintDetalji` | frmRacuni.ucitajSredinu | @RBR | Invoice detail lines |
| `getPrintFooter` | frmRacuni.vratiFooter | @brojRAC | Invoice footer |
| `getOdjavaCombo` | frmOdjava1.ucitajImena | none | Guest names+IDs for checkout combo |
| `getNeodjavljeneSobe1` | frmOdjava1.ucitajSobe | none | Rooms not checked out |
| `vratiGostSoba` | frmOdjava1.ucitajGoste | @SID | Guests for a room |
| `getPlacanjenocenja` | frmOdjava1.vratiUplatunocenja, Data.vratiUplatunocenja | @PID | Sum paid for accommodation |
| `vratiTrosakSoba` | frmOdjava1.vratiTrosakSoba | @SID | Sum expenses for room |
| `odjaviGosta` | frmOdjava1.OdjaviGosta | @GSID, @sobaID, @checkInDate, @checkOutDate, @radnik, @brojDana, @tarifaID, @popust, @ostaliTroskovi | Check out single guest (unused in current flow) |
| `Unesinocenja` | frmOdjava1.dodajnocenja | @RID, @DatumPp, @Tarifa, @SID, @PID, @opis, @Pop, @ssoba | Insert night record (deletes existing for same month first) |
| `vratiPrijasnji` | frmOdjava1.vratiIDgostijuSID1 | @RID, @PID | Get previous guest-room records |
| `getPlacanjaPID` | frmOdjava1.izlistajPlacanja | @PID | Payments by folio ID |
| `getAvansUplataOdjava` | frmOdjava1.ucitajAvanse | @brSobe | Advance payments for room |
| `getNeplacene` | frmRacuni.vratiNeplacene | none | Unpaid balances |
| `getNeplaceneSUM` | frmRacuni.vratiNeplaceneUkupno | none | Sum of unpaid |
| `getneplaceniDetalji` | frmRacuni.vratiDetalje | @PID | Unpaid expense details |
| `promijeniTrosak` | frmRacuni.updateTroska | @TID, @SID | Move expense to different room |
| `promijeniRelGost` | frmRacuni.updateGosta | @relID, @SID, @CID, @DOD | Move guest to different room |
| `sp_databases` | frmRaccopy.GetDatabases | none | List databases for copy utility |

### 2.2 Direct SQL Statements

| Line | File | SQL | Table(s) | Operation | Condition/Purpose |
|------|------|-----|----------|-----------|--------------------|
| ~245 | frmRacuni.vb | `SELECT p.nap FROM printracunifooter p WHERE BrojRacuna={brr}` | printracunifooter | SELECT | Get invoice note |
| ~320 | frmRacuni.vb | `SELECT broj,ime,frima,dadtum,datumodj,soba,vrsobe,napomnena,veza,vr_upis,tarif,d1,d2 FROM printracspec WHERE broj={akcij2}` | printracspec | SELECT | Get invoice spec header |
| ~540 | frmRacuni.vb | `SELECT placanjenacin.ID,placanjenacin.nacin FROM placanjenacin WHERE id<>4 AND id<>5 AND id<>6` | placanjenacin | SELECT | Load payment methods (excluding 4,5,6) |
| ~596 | frmRacuni.vb | `SELECT kursna.Naziv_Valute, kursna.Vrijednost FROM kursna` | kursna | SELECT | Load exchange rates |
| ~731 | frmRacuni.vb | `UPDATE placanje SET storno = 1 WHERE broj = @Rbr` | placanje | UPDATE | Storno payment |
| ~734 | frmRacuni.vb | `UPDATE placanjeDetalji SET storno = 1 WHERE brojID = @Rbr` | placanjeDetalji | UPDATE | Storno payment details |
| ~738 | frmRacuni.vb | `UPDATE printracuni SET storno = 1, exp=2, datstor='{Now}' WHERE BrojRacuna = @Rbr` | printracuni | UPDATE | Storno invoice header |
| ~741 | frmRacuni.vb | `UPDATE troskovi SET zaklj = 0, Brrac = null WHERE Brrac = @Rbr AND TID <> 1` | troskovi | UPDATE | Reopen non-accommodation expenses |
| ~743 | frmRacuni.vb | `DELETE FROM troskovi WHERE Brrac = @Rbr AND TID = 1` | troskovi | DELETE | Delete accommodation expenses |
| ~860 | frmOdjava1.vb | `SELECT sobe.ID FROM sobe INNER JOIN relgostsoba ON relgostsoba.sobaID = sobe.ID WHERE relgostsoba.odjavljen = 0 AND sobe.naziv = '{brojSobe}'` | sobe, relgostsoba | SELECT | Verify room has active guests |
| ~313 | frmOdjava1.vb | `SELECT rezervacijegrupe.naziv FROM rezervacijegrupe INNER JOIN relgostsoba ON rezervacijegrupe.ID = relgostsoba.grupaID WHERE relgostsoba.odjavljen = 0 AND relgostsoba.sobaID = {sid}` | rezervacijegrupe, relgostsoba | SELECT | Get group name for room |
| ~895 | frmOdjava1.vb | `INSERT INTO troskovi (GSID,SID,TID,vrijeme,kolicina,iznos,radnikID,napomena,zaklj) VALUES (0,{SID},25,{Now},{updDani},{iznos},1,'{dateRange}',1); SELECT @@Identity` | troskovi | INSERT | Add accommodation expense for unpaid checkout |
| ~963 | frmOdjava1.vb | `INSERT INTO neplaceni (PID,DatumOdlaska,SID,TID,placeno) VALUES ({PID},{Now},{SID},{TID},0)` | neplaceni | INSERT | Record unpaid expense |
| ~991 | frmOdjava1.vb | `INSERT INTO neplaceniplacanja (PID,IznosZaPlatit,IznosAvans) VALUES ({PID},{neplaceno},{avans})` | neplaceniplacanja | INSERT | Record unpaid payment detail |
| ~978 | frmRacuni.vb | `UPDATE printracuniavans SET storno = 1 WHERE BrojRacuna = @Rbr` | printracuniavans | UPDATE | Storno advance invoice |
| Data.vb:126 | Data.vb | `UPDATE sobe SET clean = 0 WHERE ID = {sobaid}` | sobe | UPDATE | Mark room dirty after checkout |
| Data.vb:172 | Data.vb | `UPDATE nocenja SET PrijavaOdjava = 1, datumodj = @checkOutDate WHERE SID = @SID AND PID = @PID` | nocenja | UPDATE | Mark nights as checked-out (whole room) |
| Data.vb:175 | Data.vb | `UPDATE nocenja SET PrijavaOdjava = 1, datumodj = @checkOutDate WHERE SID = @SID AND PID = @PID AND rid=@gid` | nocenja | UPDATE | Mark nights as checked-out (single guest) |
| Data.vb:178 | Data.vb | `INSERT INTO nocenja(RID,DatumP,SID,PID,PrijavaOdjava,Tarifa,popust,opis,soba) SELECT RID,@checkOutDate,SID,PID,0,@izn,popust,opis,soba FROM nocenja WHERE SID=@SID AND PID=@PID AND rid<>@gid` | nocenja | INSERT | Split night records on partial guest checkout |
| Data.vb:191 | Data.vb | `UPDATE relgostsoba SET odjavljen = 1, checkOutDate = @checkOutDate, checkOutRadnik = @radnik, pl={pl} WHERE sobaID = @SID AND odjavljen=0` | relgostsoba | UPDATE | Check out all guests from room |
| Data.vb:193 | Data.vb | `UPDATE relgostsoba SET odjavljen = 1, checkOutDate = @checkOutDate, checkOutRadnik = @radnik, pl={pl} WHERE id = @gid AND odjavljen=0` | relgostsoba | UPDATE | Check out single guest |
| Data.vb:203 | Data.vb | `UPDATE posjetaFolio SET vrijemeO = @datumO, zakljucen = @zakljucen WHERE ID = @PID` | posjetaFolio | UPDATE | Close folio |
| Data.vb:208 | Data.vb | `UPDATE troskovi SET zaklj = 1 WHERE SID = @SID AND zaklj = 0` | troskovi | UPDATE | Close all open expenses for room |
| frmRacuni.reconcile | frmRacuni.vb | `SELECT p.brojID, p.art, p.kolicina, p.iznos FROM placanjedetalji p WHERE p.brojID={ByValid} AND art=1` | placanjedetalji | SELECT | Get accommodation payment details for recalculation |
| frmRacuni.reconcile | frmRacuni.vb | `SELECT id, p.BrojRacuna, p.Trosak, p.Kol, p.CijBezPdv, p.UkupnoBezPdv, p.Pdv, p.IznosPdv, p.Ukupno, p.trosakId FROM printracunidetalji p WHERE p.BrojRacuna={ByValid} AND trosakId=1` | printracunidetalji | SELECT | Get invoice detail for recalculation |
| frmRacuni.vb | UPDATE | `Update placanje set nacin={cmbNacin.SelectedValue} where broj={br}` | placanje | UPDATE | Change payment method |
| frmRacuni.vb | UPDATE | `Update printracunidetalji set nacinid={val}, nacin='{text}' where BrojRacuna={br}` | printracunidetalji | UPDATE | Change payment method on invoice details |
| frmOdjava1.vb:795 | frmOdjava1.vb | SP `unesinocenjaDatumOdj` | nocenja | UPDATE | Set checkout date/time on night record |
| frmOdjava1.vb:860 | frmOdjava1.vb | Room existence check query | sobe + relgostsoba | SELECT | Verify room has guests |

### 2.3 frmRacuni.vb UPDATE for Invoice Editing

| Line | SQL | Table | Columns | Condition | Purpose |
|------|-----|-------|---------|-----------|---------|
| Button10_Click | `Update printracuni set idkl='{tag}', peri='{text}', datr='{date}', TipPlacanja='{text}', Ime='{text}', DrugoIme='{firm}' where BrojRacuna='{cell}'` | printracuni | idkl, peri, datr, TipPlacanja, Ime, DrugoIme | BrojRacuna | Edit guest name/firm on invoice |
| Button10_Click | `Update placanje set firma='{tag}' where broj='{cell}'` | placanje | firma | broj | Update firm on payment |
| Button10_Click | `update printracunifooter set nap='{text}' where BrojRacuna='{cell}'` | printracunifooter | nap | BrojRacuna | Update note on footer |

### 2.4 frmRaccopy.vb INSERT Operations (Cross-DB Copy)

| Line | Target Table | Columns (same as source, BrojRacuna replaced) | Source |
|------|-------------|------|--------|
| 142 | placanje | broj→br1, relgostsobaID, iznos, popust, datum, nacin, radnikID, naziv, PID, uplaceno, brdana, datumOD, datumDO, placanjeID, poslovna, storno, firma, folio, idgost, predracun, posjeta | source DB |
| 150 | placanjeDetalji | brojID→br1, art, kolicina, cijena, iznos, napomena, brojnocenja, PID, storno, ranijeUplate | source DB |
| 163 | placanjeSlozeno | RBR→br1, nacin, iznos | source DB |
| 170 | printracspec | broj→br1, ime, frima, firmaid, dadtum, datumodj, soba, vrsobe, napomnena, veza, vr_upis, tarif, d1, d2, d3, sifra1, folio, idgost, predracun, posjeta | source DB |
| 177 | printracuni | BrojRacuna→br1, Poslovna, Ime, DrugoIme, PeriodOd, PeriodDo, TipPlacanja, BrojSobe, storno, racin, napo, datr, peri, fisrac, fisvr, fisizn, rad | source DB |
| 186 | printracunidetalji | BrojRacuna→br1, Trosak, Kol, CijBezPdv, UkupnoBezPdv, Pdv, IznosPdv, Ukupno, Nacin, Valuta, OznakaValute, Popust, popust1, razlogp, pop | source DB |
| 195 | printracunifooter | BrojRacuna→br1, Avansno, nocenja, nap, pri | **target DB** (not source!) |

### 2.5 rptRacunFrm.vb SQL Operations

| Line | SQL | Table | Purpose |
|------|-----|-------|---------|
| 206 | `SELECT p.id, p.broj, p.brojpred, p.ime, p.frima, p.frimaid, p.dadtum, p.datumval, p.aktiv, p.ukupno, p.kontakt, p.napomnena, p.veza, p.vezaid, p.rabat, p.vr_upis, p.d1, p.d2, p.d3, p.sifra1, p.vrplac, p.nazivp, p.nazivid, p.gostid FROM predracuni p WHERE p.broj={br}` | predracuni | Load proforma for print |
| 213 | `SELECT p.BrojRacuna, p.Trosak, p.Kol, p.CijBezPdv, p.UkupnoBezPdv, p.Pdv, p.IznosPdv, p.Nacin, p.Ukupno, p.Valuta, p.OznakaValute, p.Popust, p.popust1, p.razlogp, p.pop, p.trosakId, p.predid, p.t1, p.trosakid1 FROM predracunidet p WHERE p.BrojRacuna={br}` | predracunidet | Load proforma details for print |

---

## 3. Database Writes

### 3.1 All INSERT Operations

| File | Table | Columns | When | Transaction? |
|------|-------|---------|------|-------------|
| frmOdjava1.vb:895 | troskovi | GSID=0, SID, TID=25, vrijeme=Now, kolicina, iznos, radnikID=1, napomena=dateRange, zaklj=1 | Unpaid checkout (neplaceno) | No |
| frmOdjava1.vb:963 | neplaceni | PID, DatumOdlaska=Now.Date, SID, TID, placeno=0 | Unpaid checkout (neplaceno) | No |
| frmOdjava1.vb:991 | neplaceniplacanja | PID, IznosZaPlatit, IznosAvans | Unpaid checkout (neplaceno) | No |
| Data.vb:178 | nocenja | RID, DatumP=checkOutDate, SID, PID, PrijavaOdjava=0, Tarifa=izn, popust, opis, soba | Partial guest checkout (split night) | Yes (in OdjavaSobe transaction) |
| frmRacuni.vb | sifarnik | naziv, kol, cij, ukupno, sifra, porez, racu, racun, dod, dod1, dod2, dod3, placanje=1 | Fiscal receipt item tracking | No |
| frmRaccopy.vb:142 | placanje | All columns from source DB with new broj | Cross-DB copy | No (individually wrapped in Try) |
| frmRaccopy.vb:150 | placanjeDetalji | All columns from source DB with new brojID | Cross-DB copy | No |
| frmRaccopy.vb:163 | placanjeSlozeno | RBR→new, nacin, iznos | Cross-DB copy | No |
| frmRaccopy.vb:170 | printracspec | All columns from source DB with new broj | Cross-DB copy | No |
| frmRaccopy.vb:177 | printracuni | All columns from source DB with new BrojRacuna | Cross-DB copy | No |
| frmRaccopy.vb:186 | printracunidetalji | All columns from source DB with new BrojRacuna | Cross-DB copy | No |
| frmRaccopy.vb:195 | printracunifooter | BrojRacuna→new, Avansno, nocenja, nap, pri | Cross-DB copy (from target DB!) | No |
| frmPlacanje.vb:3170 | printracuniavans | BrojRacuna..napo | Advance invoice creation | No |
| frmPlacanje.vb:3172 | printracuni | idkl, grupa, knj, BrojRacuna..printime | Invoice header creation | No |
| frmPlacanje.vb:3229 | printracunifooter | BrojRacuna, Avansno, nocenja, nap, pri | Invoice footer | No |
| frmPlacanje.vb:3265 | printracunidetaljiavans | BrojRacuna..trosakId (if advance) | Advance detail line | No |
| frmPlacanje.vb:3268 | printracunidetalji | BrojRacuna..trosakId (if not advance) | Invoice detail line | No |
| frmPlacanje.vb:3819 | placanjeDetalji | brojID, art=1, kolicina, cijena, iznos, brojnocenja, PID | Payment detail (accommodation) | No |
| frmPlacanje.vb:3825 | placanjeDetalji | brojID, art, kolicina, cijena, iznos, napomena, brojnocenja, PID, ranijeUplate | Payment detail (other) | No |
| frmPlacanje.vb:3822 | troskovi | GSID=0, SID, TID=1, vrijeme=Now, kolicina, iznos, radnikID=1, napomena, zaklj=1, Brrac | Close expense with invoice number | No |
| frmPlacanje.vb:4027 | placanje | broj, relgostsobaID, iznos, popust=0, datum, nacin, radnikID=1, naziv, PID, uplaceno, brdana, datumOD, datumDO, poslovna, firma | Payment record | No |

### 3.2 All UPDATE Operations

| File | Table | SET | WHERE | When | Transaction? |
|------|-------|-----|-------|------|-------------|
| frmRacuni.vb:731 | placanje | storno=1 | broj=@Rbr | Invoice storno | Yes |
| frmRacuni.vb:734 | placanjeDetalji | storno=1 | brojID=@Rbr | Invoice storno | Yes |
| frmRacuni.vb:738 | printracuni | storno=1, exp=2, datstor=Now | BrojRacuna=@Rbr | Invoice storno | Yes |
| frmRacuni.vb:741 | troskovi | zaklj=0, Brrac=null | Brrac=@Rbr AND TID<>1 | Reopen expenses on storno | Yes |
| frmRacuni.vb | printracuni | idkl, peri, datr, TipPlacanja, Ime, DrugoIme | BrojRacuna={id} | Edit invoice | No |
| frmRacuni.vb | placanje | firma | broj={id} | Edit invoice | No |
| frmRacuni.vb | printracunifooter | nap | BrojRacuna={id} | Edit note | No |
| frmRacuni.vb | placanje | nacin | broj={id} | Change payment method | No |
| frmRacuni.vb | printracunidetalji | nacinid, nacin | BrojRacuna={id} | Change payment method | No |
| frmRacuni.vb | printracuniavans | storno=1 | BrojRacuna=@Rbr | Advance invoice storno | Yes |
| frmOdjava1.vb:792-797 | nocenja | PrijavaOdjava=1, datumodj=dateOdjava+" "+time | where defined by SP | Set checkout date on nights (DodajDatumOdjave) | No |
| Data.vb:126 | sobe | clean=0 | ID={sobaid} | Mark room dirty after checkout | No |
| Data.vb:172 | nocenja | PrijavaOdjava=1, datumodj=@checkOutDate | SID=@SID AND PID=@PID (whole room) | Full room checkout | Yes |
| Data.vb:175 | nocenja | PrijavaOdjava=1, datumodj=@checkOutDate | SID=@SID AND PID=@PID AND rid=@gid (per guest) | Single guest checkout | Yes |
| Data.vb:191 | relgostsoba | odjavljen=1, checkOutDate, checkOutRadnik, pl | sobaID=@SID AND odjavljen=0 (whole room) | Full room checkout | Yes |
| Data.vb:193 | relgostsoba | odjavljen=1, checkOutDate, checkOutRadnik, pl | id=@gid AND odjavljen=0 (per guest) | Single guest checkout | Yes |
| Data.vb:203 | posjetaFolio | vrijemeO=@datumO, zakljucen=@zakljucen(=true) | ID=@PID | Close folio | Yes |
| Data.vb:208 | troskovi | zaklj=1 | SID=@SID AND zaklj=0 | Close all open expenses | Yes |
| frmRacuni.vb | printracunidetalji | CijBezPdv, UkupnoBezPdv, Pdv, IznosPdv, Ukupno | where id={id} (insurance/tax/sleep recalc) | Recalculate (prov) | No |
| frmRacuni.vb | neplaceni | placeno=1 | PID=@PID | Return to room (unpaid resolved) | No |
| frmRacuni.vb | posjetaFolio | SID=@Sid, zakljucen=0 | ID=@ID | Reopen folio for returned room | No |
| ModuleKod.vb:3111 | printracuni | fisrac, fisvr, fisIZN | BrojRacuna={id} | Save fiscal response | No |
| ModuleKod.vb:3121 | printracunifooter | nap=concat(nap, text) | BrojRacuna={id} | Append fiscal note | No |
| frmPlacanje.vb:3550 | troskovi | zaklj=1, Brrac={invoice#} | ID={id} | Close expense with invoice | No |
| frmPlacanje.vb:3568 | nocenja | PrijavaOdjava=1, datumodj=ddoo, brrac={invoice#} | SID={sid} AND PrijavaOdjava=0 | Close nights with invoice | No |
| frmPlacanje.vb:3812 | troskovi | Djelimicno=1, iznos=@iznosUp | ID=@IDtroska | Partial expense close | No |

### 3.3 All DELETE Operations

| File | Table | WHERE | When | Transaction? |
|------|-------|-------|------|-------------|
| frmRacuni.vb:743 | troskovi | Brrac=@Rbr AND TID=1 | Invoice storno (remove accommodation charges) | Yes |

---

## 4. Invoice Status Codes and Storno Logic

### 4.1 Invoice Status (printracuni.storno)

| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 (false) | Normal/active invoice | Default, `storniraj()` sets to 1 |
| 1 (true) | Stornirano (cancelled) | `storniraj()` line ~738: `UPDATE printracuni SET storno = 1` |
| Cell(8) boolean | Checked/displayed in grid; rows with storno=true get gray background | frmRacuni.vb DataGridView1_CellFormatting |

### 4.2 Invoice exp Status

| Value | Meaning | Evidence |
|-------|---------|----------|
| 2 | Stornirano | frmRacuni.vb:738: `SET storno = 1, exp=2, datstor=Now` |
| Default | Normal | (exp column exists but normal value unclear) |

### 4.3 Storno Flow (Regular Invoice)

Executed in `storniraj()` within a **MySQL transaction** (frmRacuni.vb:711-750):

1. `UPDATE placanje SET storno = 1 WHERE broj = @Rbr` — Cancel payment
2. `UPDATE placanjeDetalji SET storno = 1 WHERE brojID = @Rbr` — Cancel payment details
3. `UPDATE printracuni SET storno = 1, exp=2, datstor='{Now}' WHERE BrojRacuna = @Rbr` — Cancel invoice, mark storno datetime
4. `UPDATE troskovi SET zaklj = 0, Brrac = null WHERE Brrac = @Rbr AND TID <> 1` — Reopen non-accommodation expenses
5. `DELETE FROM troskovi WHERE Brrac = @Rbr AND TID = 1` — Delete accommodation expenses created by this invoice

If fiscal device (fsc(0) = 22): Also generates NSC storno file.
If fiscal device (fsc(0) = 3/7/2): Offers to storno fiscal receipt via `printfisc(12, ...)`.

**Storno for Advance Invoice** (`stornirajAvans()` at line ~957):
- Only: `UPDATE printracuniavans SET storno = 1 WHERE BrojRacuna = @Rbr`
- Single table UPDATE, transactional

### 4.4 Payment Method Codes (placanjenacin)

| ID | Name (implied) | Evidence |
|----|---------------|----------|
| 1-3 | Available | `WHERE id<>4 AND id<>5 AND id<>6` (frmRacuni.ucitajN) |
| 4 | Excluded | Filtered from payment combo |
| 5 | Excluded (default fallback) | ModuleKod.vb:577: `UPDATE printracunidetalji SET nacinid=5 WHERE nacinid=0` |
| 6 | Excluded | Filtered from payment combo |

In fiscal printing context (Tring integration):
- 0 = Gotovina (Cash)
- 1 = Kartica (Card)
- 3 = Virman (Bank transfer)

### 4.5 Room Checkout Status Transitions

| From | To | Operation | Evidence |
|------|----|-----------|----------|
| nocenja.PrijavaOdjava=0 | 1 | UPDATE PrijavaOdjava=1, set datumodj | Data.vb:172,175 |
| relgostsoba.odjavljen=0 | 1 | UPDATE odjavljen=1, set checkOutDate, checkOutRadnik | Data.vb:191,193 |
| posjetaFolio.zakljucen=0 | 1 | UPDATE vrijemeO=Now, zakljucen=true | Data.vb:203 |
| troskovi.zaklj=0 | 1 | UPDATE zaklj=1 for SID | Data.vb:208 (full checkout) or frmPlacanje.vb:3550 (with invoice) |
| sobe.clean | 0 | UPDATE clean=0 | Data.vb:126 (PrljavaSoba) |

### 4.6 Expense Types (TID)

| TID | Meaning | Evidence |
|-----|---------|----------|
| 1 | Accommodation/Night charge | Deleted on storno, created during checkout |
| 25 | Unpaid accommodation (neplaceno) | frmOdjava1.vb:895, only created for unpaid amounts |
| Other | Various expense types | Kept open on storno (zaklj→0, Brrac→null) |

---

## 5. Business Rules

### 5.1 Invoice Snapshot Creation

When a payment/billing is processed (frmPlacanje.vb), a complete snapshot is created:

1. **printracuni** (header): `idkl, grupa, knj, BrojRacuna, Poslovna, Ime, DrugoIme, PeriodOd, PeriodDo, TipPlacanja, BrojSobe, datr, peri, racin, napo, rad, dat, printime`
2. **printracunidetalji** (line items): `BrojRacuna, nacinid, Trosak, Kol, CijBezPdv, UkupnoBezPdv, Pdv, IznosPdv, Ukupno, Nacin, Valuta, OznakaValute, Popust, popust1, razlogp, pop, trosakId`
3. **printracunifooter** (footer): `BrojRacuna, Avansno, nocenja, nap, pri`
4. **printracspec** (specification): `broj, ime, frima, dadtum, datumodj, soba, vrsobe, napomnena, veza, vr_upis, tarif, d1, d2, folio`
5. **placanje** (payment record): `broj, relgostsobaID, iznos, popust, datum, nacin, radnikID, naziv, PID, uplaceno, brdana, datumOD, datumDO, poslovna, firma`
6. **placanjeDetalji** (payment details): `brojID, art, kolicina, cijena, iznos, napomena/blank, brojnocenja, PID, ranijeUplate`

Each expense (troskovi) is linked via `Brrac = invoice_number` and `zaklj = 1`.

Advance invoices use parallel tables: `printracuniavans`, `printracunidetaljiavans`.

### 5.2 What Gets Copied (frmRaccopy.vb)

Cross-database copy of invoices copies 7 tables, replacing `BrojRacuna` with a new value:
- `placanje`, `placanjeDetalji`, `placanjeSlozeno`, `printracspec`, `printracuni`, `printracunidetalji`
- `printracunifooter` — copied **from target DB** (not source), which may be a **BUG** or intentional (copy footer from working DB to preserve current state)

### 5.3 Storno Rules

1. **Regular invoice storno** — transactional, 5 operations:
   - Cancel payment (placanje.storno=1)
   - Cancel payment details (placanjeDetalji.storno=1)
   - Cancel invoice header (printracuni.storno=1, exp=2, datstor=timestamp)
   - Reopen expenses (troskovi.zaklj=0, Brrac=null) — but only TID<>1
   - Delete accommodation expenses (troskovi DELETE WHERE TID=1)

2. **Advance invoice storno** — single operation:
   - `printracuniavans.storno=1`
   - Does NOT affect payments or expenses

3. **Fiscal storno**: If fiscal device is configured (fsc array), attempts to storno the fiscal receipt via device-specific protocol (KTE/ELN/Tring/NSC/HCP).

### 5.4 Partial Checkout

1. **Partial guest checkout** (`btnOdjavaGosta_Click` in frmOdjava1.vb):
   - Disallows if only 1 guest remains ("Soba nemoze ostati bez gostiju")
   - Opens `frmodjG` for per-guest price input
   - Calls `OdjavaSobe(price/os, RID, SID, PID, date, uplataNocenja)`
   - This triggers: UPDATE nocenja (mark checkout for specific guest), INSERT new nocenja rows for remaining guests with split price, UPDATE relgostsoba (mark only that guest as checked out)

2. **Full room checkout** (`btnOdjavaSobe_Click` in frmOdjava1.vb):
   - Verifies room has guests
   - Warns if amount due > 0
   - `OdjavaSobe(0, 0, SID, PID, checkOutDateTime, uplataNocenja)` 
   - Then `PrljavaSoba(SID)` to mark room dirty
   - If unpaid → `neplaceno()` creates unpaid records

### 5.5 Night Calculation

From `frmOdjava1.Cijnocenja()` and `Data.VratiBrojDana()`:
- If checkout time >= 12:00 → charged for an extra day (checkout date + 1 day)
- If checkin hour < 8:00 → checkin date shifted back 1 day for calculation
- Based on `setings.cijt` flag and `setings.maxcho` hour threshold
- Each guest's night tariff comes from nocenja table, with per-guest insurance (`setings.osig`) and tax (`setings.taxa`) multiplied by night count

### 5.6 Invoice Editing

Permission: `dozvole(6)` must be "1" (checked in Button10_Click). Changes:
- `printracuni.Ime`, `DrugoIme`, `idkl`, `peri`, `datr`, `TipPlacanja`
- `placanje.firma`
- `printracunifooter.nap`
- Also updates payment method: `placanje.nacin`, `printracunidetalji.nacinid+nacin`

### 5.7 "Return to Room" (Unpaid Guest Reentry)

From `btnVratiuSobu_Click` in frmRacuni.vb (GroupBox2, admin level 9 only):
- Calls `imaliSobe(roomName)` to find free room
- `updateTroska(TID, SID)` — moves expenses to new room via SP `promijeniTrosak`
- `updateGosta(relID, SID, checkIn=Today, checkout=Tomorrow)` — moves guest via SP `promijeniRelGost`
- `updateneplaceni(PID)` — marks unpaid record as paid (placeno=1)
- `nocenja(count, SID, PID, roomName)` — adds new night records via SP `Unesinocenja`
- `updateFolio(PID, SID)` — reopens folio with `zakljucen=0`

---

## 6. Cross-Reference

### 6.1 Tables Referenced Across All Files

| Table | Files | Purposes |
|-------|-------|----------|
| printracuni | frmRacuni, frmRaccopy, rptRacunFrm, ModuleKod, frmPlacanje | Invoice header (snapshot), storno, fiscal, display |
| printracunidetalji | frmRacuni, frmRaccopy, ModuleKod, frmPlacanje | Invoice line items, payment method update |
| printracunifooter | frmRacuni, frmRaccopy, ModuleKod, frmPlacanje | Invoice footer (advance, nights, notes) |
| printracspec | frmRacuni, frmRaccopy, frmPlacanje | Invoice specification (guest details) |
| printracuniavans | frmRacuni, ModuleKod, frmPlacanje | Advance invoice header |
| printracunidetaljiavans | ModuleKod, frmPlacanje | Advance invoice line items |
| placanje | frmRacuni, frmRaccopy, frmPlacanje | Payment records, storno |
| placanjeDetalji | frmRacuni, frmRaccopy, frmPlacanje | Payment detail lines, storno |
| placanjeSlozeno | frmRaccopy, ModuleKod | Complex payment breakdown |
| placanjenacin | frmRacuni, Data.vb | Payment method lookup |
| troskovi | frmOdjava1, Data.vb, frmRacuni, frmPlacanje, ModuleKod | Expenses (opened/closed/storno) |
| troskovivrste | ModuleKod | Expense types (1=accommodation) |
| nocenja | frmOdjava1, Data.vb, frmPlacanje, ModuleKod | Night records (opened/closed) |
| relgostsoba | frmOdjava1, Data.vb, frmRacuni, ModuleKod | Guest-room relationships |
| sobe | frmOdjava1, Data.vb, frmRacuni | Rooms |
| gosti | Data.vb, ModuleKod | Guests |
| posjetaFolio | Data.vb, ModuleKod, frmRacuni | Folio (check-in/check-out tracking) |
| neplaceni | frmOdjava1, frmRacuni | Unpaid checkout records |
| neplaceniplacanja | frmOdjava1 | Unpaid payment details |
| setings | frmOdjava1, frmRacuni, rptRacunFrm, ModuleKod, Data.vb | System settings (PDV, taxa, osig, currency, fiscal config) |
| sifarnik | frmRacuni, ModuleKod | Fiscal receipt item tracking |
| kursna | frmRacuni | Exchange rates |
| partneri | Data.vb | Business partners |
| predracuni | rptRacunFrm, ModuleKod | Proforma invoices |
| predracunidet | rptRacunFrm, ModuleKod | Proforma invoice details |

### 6.2 Stored Procedures Used in Invoice/Checkout

| SP | Called By | Reference |
|----|----------|-----------|
| getPrintHeader | frmRacuni | 02_MODULEKOD_FUNCTIONS.md:1005 |
| getPrintDetalji | frmRacuni | 02_MODULEKOD_FUNCTIONS.md:994 |
| getPrintFooter | frmRacuni | 02_MODULEKOD_FUNCTIONS.md:1000 |
| getOdjavaCombo | frmOdjava1 | 02_MODULEKOD_FUNCTIONS.md:974 |
| getNeodjavljeneSobe1 | frmOdjava1 | 02_MODULEKOD_FUNCTIONS.md:957 |
| vratiGostSoba | frmOdjava1 | 02_MODULEKOD_FUNCTIONS.md:1111 |
| getPlacanjenocenja | frmOdjava1, Data.vb | 02_MODULEKOD_FUNCTIONS.md:984 |
| vratiTrosakSoba | frmOdjava1 | 02_MODULEKOD_FUNCTIONS.md:1146 |
| getPlacanjaPID | frmOdjava1 | 02_MODULEKOD_FUNCTIONS.md:979 |
| getNeplacene | frmRacuni | 02_MODULEKOD_FUNCTIONS.md:963 |
| getNeplaceneSUM | frmRacuni | 02_MODULEKOD_FUNCTIONS.md:969 |
| getneplaceniDetalji | frmRacuni | Not in ModuleKod (likely defined separately) |
| Unesinocenja | frmOdjava1, frmRacuni | 02_MODULEKOD_FUNCTIONS.md:1081 |
| promijeniTrosak | frmRacuni | ModuleKod (likely) |
| promijeniRelGost | frmRacuni | ModuleKod (likely) |
| odjaviGosta | frmOdjava1 | 02_MODULEKOD_FUNCTIONS.md (stored proc named but flow uses Data.vb OdjavaSobe instead) |

### 6.3 Key Functions Cross-Referenced

| Function | Defined In | Called From | Purpose |
|----------|-----------|-------------|---------|
| `OdjavaSobe()` | Data.vb:142 | frmOdjava1, frmPlacanje | Core checkout logic (transaction) |
| `PrljavaSoba()` | Data.vb:119 | frmOdjava1, frmPlacanje | Mark room dirty (clean=0) |
| `storniraj()` | frmRacuni.vb:711 | frmRacuni | Regular invoice storno (transaction) |
| `stornirajAvans()` | frmRacuni.vb:957 | frmRacuni | Advance invoice storno |
| `printfisc()` | frmRacuni.vb:1475 | frmRacuni | Fiscal device printing (KTE/ELN/Tring/NSC/HCP) |
| `snimFiskal()` | ModuleKod.vb:3107, frmPlacanje.vb:3139, frmGlavni.vb:1810 | Multiple | Save fiscal response to DB |
| `neplaceno()` | frmOdjava1.vb:1016 | frmOdjava1 | Create unpaid records chain |
| `dodajNocenje()` | frmOdjava1.vb:893 | frmOdjava1 | Insert accommodation expense (TID=25) |
| `dodajneplacene()` | frmOdjava1.vb:961 | frmOdjava1 | INSERT neplaceni record |
| `dodajDetalje()` | frmOdjava1.vb:989 | frmOdjava1 | INSERT neplaceniplacanja record |
| `ucitajRacunPrint()` | frmRacuni.vb | frmRacuni | Prepare invoice for printing |
| `print1()` | rptRacunFrm.vb:67 | rptRacunFrm | Render invoice report |
| `printpredr()` | rptRacunFrm.vb:203 | rptRacunFrm | Print proforma |
| `citajfirme()` | Data.vb:596 | frmRacuni | Load partner/company data |
| `pripremaRcuna()` | Data.vb:229 | frmPlacanje | Prepare invoice data for billing |
| `vratiCijenunocenja()` | Data.vb:336 | frmOdjava1, frmPlacanje | Calculate night charge |
| `VratiBrojDana()` | Data.vb:416 | Data.vb | Calculate number of days for billing |

---

## 7. Key Findings for Modern System

### 7.1 Architecture Issues

1. **7-table snapshot with no FK integrity**: Invoice creation writes to 7+ tables (`printracuni`, `printracunidetalji`, `printracunifooter`, `printracspec`, `placanje`, `placanjeDetalji`, `placanjeSlozeno`) without transactions in frmPlacanje.vb. Only `storniraj()` and `OdjavaSobe()` use transactions. **Modern system must use a single transaction for all invoice creation writes.**

2. **Storno inconsistency**: Regular invoice storno reopens expenses and deletes accommodation rows, but advance invoice storno only sets a flag. No reversal of folio/night status on storno. **Modern system needs consistent reversal logic across all linked entities.**

3. **Cross-database copy (frmRaccopy.vb)**: Copies records between yearly archive databases with individual Try/Catch per table (no transaction). `printracunifooter` is copied from the **target** database, not source — potentially a bug. **Modern system should use server-side backup/restore rather than application-level table copying.**

4. **SQL injection risks**: Multiple direct string concatenations in queries (e.g., frmRacuni.vb `BrojRacuna={akcij2}`, frmOdjava1.vb room existence check `naziv = '{brojSobe}'`). **Modern system must use parameterized queries throughout.**

5. **Fiscal device coupling**: 6 different fiscal device protocols hardcoded (fsc(0) values 2,3,4,5,6,7,22). Each requires different XML/file/command format. **Modern system should abstract fiscal integration behind an interface.**

### 7.2 Data Model Redundancy

1. **Invoice snapshot tables duplicate live data**: `printracuni`/`printracunidetalji`/`printracunifooter`/`printracspec` are historical snapshots of billing data that also exists in `placanje`/`placanjeDetalji`/`troskovi`/`nocenja`. The `printracun*` tables serve as immutable print records, while `placanje*`/`troskovi` serve as mutable operational records. **Modern system should have one source of truth with versioning/audit trail rather than dual-write.**

2. **neplaceni/neplaceniplacanja are denormalized**: These store computed unpaid balances that derive from `troskovi` + `placanje`. **Modern system should compute these on demand rather than storing redundant aggregates.**

3. **nocenja table dual purpose**: Used both for tariff calculation (PrijavaOdjava=0 = active) and as checkout record (PrijavaOdjava=1 = closed). The `Data.vb:178` INSERT splits nights on partial guest checkout, creating a new row for remaining guests. **Modern system should model night stays as date-range records rather than per-night rows that get split.**

### 7.3 Missing Functionality

1. **No partial storno**: The legacy system only supports full invoice cancellation. There is no mechanism to cancel individual line items from an invoice. **Modern system should support line-level credit notes.**

2. **No invoice numbering sequence visible**: `BrojRacuna` appears to be auto-generated but the mechanism is in frmPlacanje.vb (not fully analyzed here). **Modern system must implement proper sequential invoice numbering with gap detection.**

3. **No audit trail**: Changes to invoices (e.g., Button10_Click edit of Ime/DrugoIme) have no history tracking. Only `funkcije.logs()` provides partial audit. **Modern system needs full change tracking.**

4. **Room status not atomically maintained**: `PrljavaSoba()` (clean=0) is called after `OdjavaSobe()` but not in the same transaction. If `PrljavaSoba` fails, room stays marked as clean. **Modern system must update room status atomically within checkout transaction.**

### 7.4 Business Rules Summary for Reimplementation

| # | Rule | Evidence |
|---|------|----------|
| R1 | Invoice is a snapshot of billing state, frozen at creation time | printracun* tables populated from live data |
| R2 | Storno sets storno=1 on payment, payment detail, and invoice header | frmRacuni.vb:731-743 |
| R3 | Storno reopens non-accommodation expenses (zaklj→0, Brrac→null) | frmRacuni.vb:741 |
| R4 | Storno deletes accommodation expenses (TID=1) | frmRacuni.vb:743 |
| R5 | Storno records timestamp (datstor) | frmRacuni.vb:738 |
| R6 | Advance invoice storno only sets flag on printracuniavans | frmRacuni.vb:978 |
| R7 | Checkout marks all nights as PrijavaOdjava=1 | Data.vb:172,175 |
| R8 | Checkout marks all guests in room as odjavljen=1 | Data.vb:191,193 |
| R9 | Checkout closes folio (zakljucen=true) | Data.vb:203 |
| R10 | Checkout closes all open expenses (zaklj=1) only for full room checkout | Data.vb:208 |
| R11 | Partial guest checkout splits night records (INSERT new for remaining guests with split tariff) | Data.vb:178 |
| R12 | Partial guest checkout only marks that specific relgostsoba row as odjavljen=1 | Data.vb:193 |
| R13 | After checkout, room is marked dirty (clean=0) | Data.vb:126 |
| R14 | Unpaid checkout creates troskovi (TID=25), neplaceni, neplaceniplacanja records | frmOdjava1.vb:893-1038 |
| R15 | Night count calculation: checkout hour ≥ check-out threshold → +1 day, same-day check-in → minimum 1 night | frmOdjava1.vb:394-466, Data.vb:416-436 |
| R16 | Insurance and tourist tax per night added to accommodation total | frmOdjava1.vb:456 |
| R17 | Fiscal receipt required for all invoices; 6 device types supported | frmRacuni.vb printfisc, frmPlacanje.vb |
| R18 | Payment methods 4, 5, 6 are excluded from payment combo | frmRacuni.ucitajN |
| R19 | Method 5 is default fallback for unmapped methods | ModuleKod.vb:577 |
| R20 | Cross-database copy replaces BrojRacuna with new value for all 7 tables | frmRaccopy.vb |
| R21 | "Return to room" for unpaid guests: moves expenses, guest, nights, and reopens folio | frmRacuni.vb btnVratiuSobu_Click |
| R22 | `pl` flag on relgostsoba set to 1 if uplataNocenja > 0 | Data.vb:185-189 |
| R23 | Invoice note (printracunifooter.nap) can be edited by admin (level 9) | frmRacuni.vb Button10_Click |
| R24 | Legal notice appended on fiscal response: "Po clanu 42 fiskalnog zakona..." | ModuleKod.vb:3121 |