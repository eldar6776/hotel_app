# Reservation (Rezervacije) Flow - Legacy Analysis

> Source files analyzed:
> - `legacy_code/frmRezervacije.vb` (1178 lines)
> - `legacy_code/frmRezervacije_unos.vb` (1133 lines)
> - `legacy_code/frmRezervacijeNove.vb` (571 lines)
> - `legacy_code/frmRezervacijePrebaci.vb` (1060 lines)
> - `legacy_code/frmRezervacijePregled.vb` (919 lines)
> - `legacy_code/frmRezervacije1.vb` (7 lines - stub only)

---

## 1. Business Flow: Reservation Process

### 1.1 Creating a Reservation

There are **3 distinct creation paths** in the legacy code:

**Path A: Calendar Grid Quick-Create (`frmRezervacije`)**
- User selects cells on the room/date calendar grid (`dgv`)
- Double-click on an empty (non-red, non-yellow) cell opens `frmRezervacije_unos` with Tag=0 (new)
- OR: User types a value directly into a cell on the grid → `dgv_CellValueChanged` fires
- On confirmation ("Zelite li upisati rezervaciju!"), the system:
  1. INSERTs into `rezervacije` with minimal data (GID=0, tipID=1, izvorID=0, sobaVrstaID=0, potvrda=0, prijava=0, brojRezSoba=1, brosoba=1, brdjeca=0) — `frmRezervacije.vb:942`
  2. For each selected cell's row, INSERTs into `rezervacijasobe` with room type/soba/tarifa data — `frmRezervacije.vb:967`

**Path B: Dedicated Entry Form (`frmRezervacije_unos`)**
- Opened via Button3 (`frmRezervacije.vb:803`) or ButtonX3 (`frmRezervacije.vb:1071`) with Tag=0
- User fills: guest (GID), checkIn/Out dates, tipID (type), izvorID (source), rooms, tariffs, contact info, memo
- On save (Button1_Click, `frmRezervacije_unos.vb:290`):
  1. If `Me.Tag <> 0`: calls `snimiizmjene()` (edit mode)
  2. If `Me.Tag = 0`: INSERTs new reservation into `rezervacije` with all form fields — `frmRezervacije_unos.vb:338`
  3. Optionally INSERTs alarm if CheckBox1 checked — `frmRezervacije_unos.vb:320`
  4. INSERTs each room row into `rezervacijasobe` — `frmRezervacije_unos.vb:357`

**Path C: Simplified Reservation (`frmRezervacijeNove`)**
- Simple form with room type, guest, date range, number of rooms
- On save (Button1_Click, `frmRezervacijeNove.vb:323`):
  1. Validates date range and guest
  2. Optionally creates group (`rezervacijegrupe`) if group checkbox checked — `frmRezervacijeNove.vb:279`
  3. INSERTs into `rezervacije` with parameterized query — `frmRezervacijeNove.vb:367`
  4. Does NOT create `rezervacijasobe` entries directly (room assignment is separate)

### 1.2 Confirming a Reservation (Potvrda)

**From `frmRezervacijePregled` (btnPotvrdi_Click):**
- Checks current `potvrda` status from grid cell(11)
- If `potvrda=0` (not confirmed): confirms → UPDATE `rezervacije SET potvrda=1, brojPotvrde={next}` WHERE ID — `frmRezervacijePregled.vb:457`
- If `potvrda=1` (already confirmed): un-confirms → UPDATE `rezervacije SET potvrda=0, brojPotvrde=Value` WHERE ID — `frmRezervacijePregled.vb:459`
- `brojPotvrde` is generated as MAX(brojPotvrde)+1 — `frmRezervacijePregled.vb:422`

**From `frmRezervacije_unos` (chkpotvrd_CheckedChanged):**
- If checking the checkbox: UPDATE `rezervacije SET datepotvrda={now}, brojPotvrde={MAX+1}, potvrda=1` WHERE ID — `frmRezervacije_unos.vb:828`
- If un-checking: UPDATE `rezervacije SET potvrda=0` WHERE ID — `frmRezervacije_unos.vb:836`

### 1.3 Canceling a Reservation (Storno)

**From `frmRezervacijePregled` (btnStoniraj_Click):**
- Toggles `stornirana` field: 0→1 (cancel) or 1→0 (reactivate)
- UPDATE `rezervacije SET stornirana=1, brojStorna={MAX+1}` or `stornirana=0, brojStorna=Value` — `frmRezervacijePregled.vb:357-359`

**From `frmRezervacije_unos` (chkstorno_CheckedChanged):**
- Cancel: UPDATE `rezervacije SET datestorno={now}, brojStorna={MAX+1}, stornirana=1, razlogst={reason}` — `frmRezervacije_unos.vb:800`
- Reactivate: UPDATE `rezervacije SET stornirana=0, razlogst=''` — `frmRezervacije_unos.vb:811`

### 1.4 Transferring a Reservation to Check-In (Prebaci)

**`frmRezervacijePrebaci` (btnPrijavi_Click):**
Two modes: "by rooms" (rbtSobe) and "by guests" (rbtGosti)

**By Rooms mode (`frmRezervacijePrebaci.vb:409-443`):**
1. Validate total bed count vs reserved beds
2. For each selected room:
   - INSERT into `gosti` (clone guest data) → `PrijaviGosta()` — `frmRezervacijePrebaci.vb:593-641`
   - INSERT into `posjetaFolio` (SID, vrijemeD) → `VratiPid()` — `frmRezervacijePrebaci.vb:562-591`
   - INSERT into `relGostSoba` (gostID, sobaID, checkInDate=Now, checkOutDate, checkInRadnik=RID, grupaID, tarifaID, PID, rezervP=1) — `frmRezervacijePrebaci.vb:644-682`
   - Call stored procedure `Unesinocenja` for each guest — `frmRezervacijePrebaci.vb:791-821`
   - INSERT into `rezervacijaPrijava` (IDrez, IDGost, sobaID) — `frmRezervacijePrebaci.vb:955-981`
3. Call `PrijavaRezervacija(brSoba, brRez)`:
   - If all rooms checked in: UPDATE `rezervacije SET prijava=1` — `frmRezervacijePrebaci.vb:689`
   - If partial: UPDATE `rezervacije SET brojRezSoba={remaining}` — `frmRezervacijePrebaci.vb:692`

**By Guests mode (`frmRezervacijePrebaci.vb:445-497`):**
1. For each guest with room assignment:
   - Check if PID already exists for room (`provjeriPID`)
   - Create or reuse `posjetaFolio` record
   - INSERT into `relGostSoba` with tarifa, PID, rezervP=1
   - Call `Unesinocenja` (accommodation charge stored procedure)
   - INSERT into `rezervacijaPrijava`
   - UPDATE `gosti SET Rid=0` (clear reservation link) — `frmRezervacijePrebaci.vb:757-785`
2. Call `PrijavaRezervacijaPojedi(brRez, brSoba)`:
   - Same logic as `PrijavaRezervacija` but with inverted param order

### 1.5 Editing a Reservation

**`frmRezervacije_unos` (snimiizmjene, `frmRezervacije_unos.vb:365-436`):**
- Validates Tag is numeric and non-zero
- Manages alarm: INSERT new alarm or UPDATE existing, or storno alarm if unchecked — `frmRezervacije_unos.vb:378-387`
- UPDATE `rezervacije SET GID, checkInDate, checkOutDate, blokID, tipID, izvorID, sobaVrstaID, brojRezSoba, godina, prijava, tarifa, memo, radnik, radnikID, gost, tex, napomena, alarmid, gostgrupa, promjena+1, promjenat, kontakt, kontakttel, kontaktfax, kontaktmob, kontaktmail, plac, placanje, firma, firmaid, agencija, komerc, agencijaid, komercid, brosoba, brdjeca, dateizmjena, razlogst` — `frmRezervacije_unos.vb:407`
- DELETE all `rezervacijasobe` for this reservation ID — `frmRezervacije_unos.vb:413`
- Re-INSERT all room rows from the grid — `frmRezervacije_unos.vb:427`
- Increments `promjena` counter on each edit

### 1.6 Reservation Calendar View (`frmRezervacije`)

The main grid (`dgv`) renders a room×date matrix:
- **Red cells**: Currently occupied rooms (`relgostsoba` with `odjavljen=0`)
- **Yellow cells**: Reserved but not checked in (`rezervacije` with `stornirana='0'` and `prijava=0`)
- **Orange cells**: Overlap — reservation dates coincide with occupied dates
- **YellowGreen cells**: Double-booked reservations (error state)
- **Black rows**: Room out of order (`ooo=1`)
- **"Sobarica!" text**: Room not clean (`clean=0`)
- Double-click on yellow cell opens `frmRezervacije_unos` for that reservation
- Double-click on red cell opens `frmSobaInfo` (occupied room info)

On form load: UPDATE `rezervacije SET prijava=2 WHERE prijava=0 AND checkInDate < yesterday` — `frmRezervacije.vb:29`

---

## 2. SQL Inventory

### 2.1 SELECT Operations

| # | File:Line | Operation | Table(s) | Columns | WHERE/Condition | Business Purpose |
|---|-----------|-----------|----------|----------|-----------------|-----------------|
| 1 | frmRezervacije.vb:29 | UPDATE→SELECT implicit | rezervacije | prijava | prijava=0 AND checkInDate<yesterday | Auto-mark old reservations |
| 2 | frmRezervacije.vb:74-104 | SP SELECT | getRezrervacijePrikazi | (SP result) | ddatOD, ddatDO params | Load reservations for calendar |
| 3 | frmRezervacije.vb:115 | SELECT | sobe JOIN relgostsoba JOIN gosti | 35 columns | odjavljen=0 | Load occupied rooms for calendar |
| 4 | frmRezervacije.vb:120 | SELECT | rezervacije INNER JOIN rezervacijasobe | 35 columns | stornirana='0' AND date filter | Load reserved rooms (current mode) |
| 5 | frmRezervacije.vb:122 | SELECT | rezervacije INNER JOIN rezervacijasobe | 35 columns | prijava=0 AND stornirana='0' AND date filter | Load reserved rooms (future mode) |
| 6 | frmRezervacije.vb:297 | SELECT | sobe JOIN relgostsoba JOIN gosti | same as #3 | odjavljen=0 | proracun1() occupied rooms |
| 7 | frmRezervacije.vb:298 | SELECT | rezervacije INNER JOIN rezervacijasobe | same as #4 | stornirana='0' | proracun1() reserved rooms |
| 8 | frmRezervacije.vb:463 | SELECT | sobe JOIN relgostsoba JOIN gosti | same as #3 | odjavljen=0 | proracun_stari() occupied rooms |
| 9 | frmRezervacije.vb:464 | SELECT | rezervacije INNER JOIN rezervacijasobe | same as #4 | stornirana='0' | proracun_stari() reserved rooms |
| 10 | frmRezervacije_unos.vb:37 | SELECT | gosti | ime, prezime | (none) | Guest autocomplete |
| 11 | frmRezervacije_unos.vb:42 | SELECT | rezervacijasobe | all columns | rezid={Tag} | Load room details for edit |
| 12 | frmRezervacije_unos.vb:107 | SELECT | rezervacije | 40+ columns | id={Tag} | Load reservation for edit |
| 13 | frmRezervacije_unos.vb:108 | SELECT | alarm | opis, opis1, odgovor, vrijeme, vrijeme1, tip, chk, rpt, radnik, soba, storno, vr_upis, idd | alarmid={id} AND storno='0' | Load reservation alarm |
| 14 | frmRezervacije_unos.vb:210 | SELECT | rezervacijetip | ID, naziv | (none) | Load reservation types dropdown |
| 15 | frmRezervacije_unos.vb:249 | SELECT | rezervacijeizvor | ID, naziv | (none) | Load reservation sources dropdown |
| 16 | frmRezervacije_unos.vb:478 | SELECT | sobavrsta | ID, naziv | (none) | Load room types |
| 17 | frmRezervacije_unos.vb:547 | SELECT | sobe | ID, naziv | vrsta={sid} AND ooo=0 | Load rooms by type |
| 18 | frmRezervacije_unos.vb:912 | SELECT | sobatarifa | ID, naziv | del=0 | Load tariffs |
| 19 | frmRezervacije_unos.vb:849 | SELECT | sobavrsta | id | naziv='{value}' | Find room type ID |
| 20 | frmRezervacijeNove.vb:44-80 | SELECT | sobavrsta | ID, naziv | (none) | Load room types combo |
| 21 | frmRezervacijeNove.vb:84-110 | SELECT | sobe | ID, vrsta, naziv | ooo=0 | Load all active rooms |
| 22 | frmRezervacijeNove.vb:114-146 | SELECT | sobe | vrsta, Count(*) AS BrojSoba | ooo=0 GROUP BY vrsta | Room counts by type |
| 23 | frmRezervacijeNove.vb:149-186 | SELECT | rezervacijetip | ID, naziv | (none) | Load reservation types |
| 24 | frmRezervacijeNove.vb:191-232 | SELECT | rezervacijeIzvor | ID, naziv | (none) | Load reservation sources |
| 25 | frmRezervacijeNove.vb:447-475 | SELECT | rezervacije | MAX(brojPotvrde) | (none) | Get next confirmation number |
| 26 | frmRezervacijeNove.vb:497-531 | SP SELECT | vratiTarifePoVrsta | (SP result) | @svid param | Tariffs by room type |
| 27 | frmRezervacijePrebaci.vb:156-197 | SELECT | sobe | ID, naziv, vrsta | vrsta={vrsta} ORDER BY naziv | Available rooms by type |
| 28 | frmRezervacijePrebaci.vb:240-269 | SP SELECT | getVratiZauzete | (SP result) | ddatOD, ddatDO params | Occupied rooms for date range |
| 29 | frmRezervacijePrebaci.vb:280-308 | SELECT | sobavrsta | brojKreveta | ID={vrsta} | Get bed count per room type |
| 30 | frmRezervacijePrebaci.vb:526-559 | SELECT | gosti | ime, prezime, adresa, datumRodjenja, pol, drzavljanstvo, dokument, brDokument, telefon, mobitel, email, mjestodrzavaR, DID | ID={@ID} | Load guest for check-in |
| 31 | frmRezervacijePrebaci.vb:898-923 | SELECT | poszetaFolio | ID | SID={@SID} AND zakljucen=0 | Check open folio for room |
| 32 | frmRezervacijePrebaci.vb:828-859 | SELECT | gosti | ID, ime, prezime | rid={@ID} | Load guests linked to reservation |
| 33 | frmRezervacijePrebaci.vb:1000-1029 | SELECT | sobe JOIN sobaVrsta JOIN relSobaVrstaSobaTarifa JOIN sobaTarifa | sobaTarifa.naziv | sobaTarifaID={IDtarife} | Get tariff name/name for price |
| 34 | frmRezervacijePregled.vb:29-76 | SP SELECT | getRezrervacijePrikazi / Pot / Sto / OD | (SP result) | ddatOD, ddatDO params | Load reservation list (all/confirmed/cancelled/by date) |
| 35 | frmRezervacijePregled.vb:120 | UPDATE | gosti | ime, prezime, telefon | ID=@GID | Update guest contact from grid |
| 36 | frmRezervacijePregled.vb:170 | UPDATE | rezervacije | checkInDate, checkOutDate, brojRezSoba | GID=@GID AND sobavrstaID=@SID | Update reservation dates from grid |
| 37 | frmRezervacijePregled.vb:227 | SELECT | rezervacijasobe | rezid, soba, tarifa, gost, brgost | rezid={id} | Load room details for selected reservation |
| 38 | frmRezervacijePregled.vb:265 | UPDATE | gosti | rid=null | ID=@ID | Clear guest reservation link |
| 39 | frmRezervacijePregled.vb:323-351 | SELECT | rezervacije | MAX(brojStorna) | (none) | Get next storno number |
| 40 | frmRezervacijePregled.vb:422-451 | SELECT | rezervacije | MAX(brojPotvrde) | (none) | Get next confirmation number |
| 41 | frmRezervacijePregled.vb:624-655 | SELECT | rezervacije LEFT JOIN gosti/sobaVrsta/rezervacijeGrupe/rezervacijeTip | 21 columns | dynamic WHERE | Search reservations |
| 42 | frmRezervacije.vb:849 | SELECT | sobavrsta | id | naziv='{value}' | Find room type ID from name |

### 2.2 INSERT Operations

| # | File:Line | Table | Columns | Form Fields | Business Purpose |
|---|-----------|-------|---------|-------------|-----------------|
| 1 | frmRezervacije.vb:942 | rezervacije | GID, checkInDate, checkOutDate, potvrda, brojPotvrde, blokID, tipID, izvorID, sobaVrstaID, stornirana, brojStorna, brojRezSoba, godina, prijava, tarifa, memo, radnik, radnikID, vrijeme, idd, gost, tex, napomena, alarmid, gostgrupa, promjena, promjenat, kontakt, kontakttel, kontaktfax, kontaktmob, kontaktmail, plac, placanje, firma, firmaid, agencija, komerc, agencijaid, komercid, brosoba, brdjeca | Quick-create defaults: GID=0, potvrda=0, brojPotvrde=0, blokID=1, tipID=1, izvorID=0, sobaVrstaID=0, stornirana=0, brojStorna=0, brojRezSoba=1, prijava=0, tarifa=0, brosoba=1, brdjeca=0 | Quick reservation from calendar |
| 2 | frmRezervacije.vb:967 | rezervacijasobe | rezid, sobtid, sobatip, sid, soba, tid, tarifa, gid, gost, idd, promjena, pom, pom1, pusac, brgost, gost1, cjenovnik | Room type, room ID, room name | Room assignment for quick reservation |
| 3 | frmRezervacije_unos.vb:320 | alarm | opis, opis1, odgovor, vrijeme, vrijeme1, tip, chk, rpt, radnik, soba, storno, vr_upis, idd | TextBox17, DateTimePicker1, DateTimePicker2, RIme | Create alarm for reservation |
| 4 | frmRezervacije_unos.vb:338 | rezervacije | GID, checkInDate, checkOutDate, potvrda, brojPotvrde, blokID, tipID, izvorID, sobaVrstaID, stornirana, brojStorna, brojRezSoba, godina, prijava, tarifa, memo, radnik, radnikID, vrijeme, idd, gost, tex, napomena, alarmid, gostgrupa, promjena, promjenat, kontakt, kontakttel, kontaktfax, kontaktmob, kontaktmail, plac, placanje, firma, firmaid, agencija, komerc, agencijaid, komercid, brosoba, brdjeca | Full form fields | New reservation via entry form |
| 5 | frmRezervacije_unos.vb:357 | rezervacijasobe | rezid, sobtid, sobatip, sid, soba, tid, tarifa, gid, gost, idd, promjena, pom, pom1, pusac, brgost, gost1, cjenovnik | Grid values | Room rows for new reservation |
| 6 | frmRezervacije_unos.vb:380 | alarm | (same as #3) | For edit mode alarm creation | Create alarm during edit |
| 7 | frmRezervacijeNove.vb:279 | rezervacijegrupe | naziv | txtGrupne.Text | Create group for group reservation |
| 8 | frmRezervacijeNove.vb:367 | rezervacije | GID, checkInDate, checkOutDate, potvrda, brojPotvrde, blokID, tipID, izvorID, sobavrstaID, brojRezSoba, godina, tarifa, memo, radnik, radnikID | Parameterized | New simplified reservation |
| 9 | frmRezervacijePrebaci.vb:565-591 | poszetaFolio | SID, vrijemeD | brojSobe, Now | Create folio for check-in |
| 10 | frmRezervacijePrebaci.vb:593-641 | gosti | ime, prezime, adresa, datumRodjenja, pol, drzavljanstvo, dokument, brDokument, telefon, mobitel, email, mjestodrzavaR, DID | Clone from original guest | Create guest record for check-in |
| 11 | frmRezervacijePrebaci.vb:644-682 | relGostSoba | gostID, sobaID, checkInDate, checkOutDate, checkInRadnik, grupaID, tarifaID, PID, rezervP | Check-in data, rezervP=1 | Link guest to room on check-in |
| 12 | frmRezervacijePrebaci.vb:955-981 | rezervacijaPrijava | IDrez, IDGost, sobaID | Reservation ID, guest ID, room ID | Track reservation-to-checkin mapping |

### 2.3 UPDATE Operations

| # | File:Line | Table | SET Columns | WHERE | Business Purpose |
|---|-----------|-------|-------------|-------|-----------------|
| 1 | frmRezervacije.vb:29 | rezervacije | prijava=2 | prijava=0 AND checkInDate<yesterday | Auto-expire stale reservations |
| 2 | frmRezervacije_unos.vb:343 | alarm | opis1='Rezervacija {id}' | id={alarid} | Update alarm description after creation |
| 3 | frmRezervacije_unos.vb:382 | alarm | opis, vrijeme, vrijeme1, radnik, vr_upis, storno=0 | id={alarid} | Update alarm on edit |
| 4 | frmRezervacije_unos.vb:385 | alarm | storno=1 | id={alarid} | Cancel alarm when unchecked |
| 5 | frmRezervacije_unos.vb:407 | rezervacije | GID, checkInDate, checkOutDate, blokID, tipID, izvorID, sobaVrstaID, brojRezSoba, godina, prijava, tarifa, memo, radnik, radnikID, gost, tex, napomena, alarmid, gostgrupa, promjena+1, promjenat, kontakt, kontakttel, kontaktfax, kontaktmob, kontaktmail, plac, placanje, firma, firmaid, agencija, komerc, agencijaid, komercid, brosoba, brdjeca, dateizmjena, razlogst | id={Tag} | Full reservation update |
| 6 | frmRezervacije_unos.vb:800 | rezervacije | datestorno={now}, brojStorna={MAX+1}, stornirana=1, razlogst={reason} | id={Tag} | Cancel (storno) reservation |
| 7 | frmRezervacije_unos.vb:811 | rezervacije | stornirana=0, razlogst='' | id={Tag} | Reactivate cancelled reservation |
| 8 | frmRezervacije_unos.vb:828 | rezervacije | datepotvrda={now}, brojPotvrde={MAX+1}, potvrda=1 | id={Tag} | Confirm reservation |
| 9 | frmRezervacije_unos.vb:836 | rezervacije | potvrda=0 | id={Tag} | Un-confirm reservation |
| 10 | frmRezervacijePregled.vb:357 | rezervacije | stornirana=1, brojStorna={MAX+1} | ID={id} | Cancel reservation from grid |
| 11 | frmRezervacijePregled.vb:359 | rezervacije | stornirana=0, brojStorna={Value} | ID={id} | Reactivate from grid |
| 12 | frmRezervacijePregled.vb:457 | rezervacije | potvrda=1, brojPotvrde={MAX+1} | ID={id} | Confirm from grid |
| 13 | frmRezervacijePregled.vb:459 | rezervacije | potvrda=0, brojPotvrde={Value} | ID={id} | Un-confirm from grid |
| 14 | frmRezervacijePregled.vb:170 | rezervacije | checkInDate, checkOutDate, brojRezSoba | GID=@GID AND sobavrstaID=@SID | Update dates from grid |
| 15 | frmRezervacijePregled.vb:120 | gosti | ime, prezime, telefon | ID=@GID | Update guest contact |
| 16 | frmRezervacijePregled.vb:265 | gosti | rid=null | ID=@ID | Clear guest reservation link |
| 17 | frmRezervacijePrebaci.vb:689 | rezervacije | prijava=1 | ID=@ID (when all rooms checked in) | Mark reservation as checked-in |
| 18 | frmRezervacijePrebaci.vb:692 | rezervacije | brojRezSoba={remaining} | ID=@ID (partial check-in) | Reduce remaining room count |
| 19 | frmRezervacijePrebaci.vb:726 | rezervacije | prijava=1 | ID=@ID (guests mode all) | Mark reservation checked-in (guests) |
| 20 | frmRezervacijePrebaci.vb:729 | rezervacije | brojRezSoba={remaining} | ID=@ID (partial guests) | Reduce remaining (guests) |
| 21 | frmRezervacijePrebaci.vb:760 | gosti | Rid=0 | ID=@ID | Clear guest reservation link |

### 2.4 DELETE Operations

| # | File:Line | Table | WHERE | Business Purpose |
|---|-----------|-------|-------|-----------------|
| 1 | frmRezervacije_unos.vb:413 | rezervacijasobe | rezid={Tag} | Delete all room assignments before re-inserting on edit |

### 2.5 Stored Procedures Used

| # | File:Line | SP Name | Params | Purpose |
|---|-----------|---------|--------|---------|
| 1 | frmRezervacije.vb:74 | getRezrervacijePrikazi | ddatOD, ddatDO | Load all reservations for display |
| 2 | frmRezervacije.vb:76-86 | getRezrervacijePrikaziPot / Sto / OD | ddatOD, ddatDO | Load confirmed/cancelled/by-date reservations |
| 3 | frmRezervacije_unos.vb:522 | vratiTarifePoVrsta | @svid | Get tariffs by room type |
| 4 | frmRezervacijeNove.vb:498 | vratiTarifePoVrsta | @svid | Get tariffs by room type |
| 5 | frmRezervacijePrebaci.vb:240 | getVratiZauzete | ddatOD, ddatDO | Get occupied rooms for date range |
| 6 | frmRezervacijePrebaci.vb:793 | Unesinocenja | RID, DatumPp, Tarifa, SID, PID, opis, ssoba, Pop | Create night/accommodation charge |

---

## 3. Database Writes (Every INSERT/UPDATE/DELETE on Reservation Tables)

### 3.1 Table: `rezervacije`

| Operation | File:Line | Trigger | Key Fields |
|-----------|----------|---------|-------------|
| INSERT | frmRezervacije.vb:942 | Calendar quick-create | GID=0, tipID=1, prijava=0, stornirana=0, brojRezSoba=1 |
| INSERT | frmRezervacije_unos.vb:338 | Entry form create | Full form data, returns @@Identity |
| INSERT | frmRezervacijeNove.vb:367 | Simplified create | Parameterized, returns @@Identity |
| UPDATE | frmRezervacije.vb:29 | Form load | prijava=2 WHERE prijava=0 AND checkInDate<yesterday |
| UPDATE | frmRezervacije_unos.vb:407 | Edit form save | Almost all columns, increments promjena |
| UPDATE | frmRezervacije_unos.vb:800 | Storno checkbox | stornirana=1, datestorno, brojStorna=MAX+1, razlogst |
| UPDATE | frmRezervacije_unos.vb:811 | Un-storno | stornirana=0, razlogst='' |
| UPDATE | frmRezervacije_unos.vb:828 | Confirm checkbox | potvrda=1, datepotvrda, brojPotvrde=MAX+1 |
| UPDATE | frmRezervacije_unos.vb:836 | Un-confirm | potvrda=0 |
| UPDATE | frmRezervacijePregled.vb:357 | Grid storno button | stornirana=1, brojStorna=MAX+1 |
| UPDATE | frmRezervacijePregled.vb:359 | Grid un-storno | stornirana=0 |
| UPDATE | frmRezervacijePregled.vb:457 | Grid confirm button | potvrda=1, brojPotvrde=MAX+1 |
| UPDATE | frmRezervacijePregled.vb:459 | Grid un-confirm | potvrda=0 |
| UPDATE | frmRezervacijePregled.vb:170 | Grid date edit | checkInDate, checkOutDate, brojRezSoba |
| UPDATE | frmRezervacijePrebaci.vb:689 | Check-in all rooms | prijava=1 |
| UPDATE | frmRezervacijePrebaci.vb:692 | Check-in partial rooms | brojRezSoba=remaining |
| UPDATE | frmRezervacijePrebaci.vb:726 | Check-in all guests | prijava=1 |
| UPDATE | frmRezervacijePrebaci.vb:729 | Check-in partial guests | brojRezSoba=remaining |

**CRITICAL NOTE**: `brojStorna`/`brojPotvrde` are generated using MAX()+1, which is a race condition — non-atomic, not safe under concurrent access.

### 3.2 Table: `rezervacijasobe`

| Operation | File:Line | Trigger | Key Fields |
|-----------|----------|---------|-------------|
| INSERT | frmRezervacije.vb:967 | Calendar quick-create per room row | rezid, sobtid, sobatip, sid, soba, tarifa, pusac, brgost |
| INSERT | frmRezervacije_unos.vb:357 | Entry form create per room | Same fields from grid |
| INSERT | frmRezervacije_unos.vb:427 | Entry form edit per room (after delete all) | Same, with promjena+1 |
| DELETE | frmRezervacije_unos.vb:413 | Edit form save | DELETE WHERE rezid={Tag} — nuke-and-pave pattern |

### 3.3 Table: `rezervacijagrupe`

| Operation | File:Line | Trigger |
|-----------|----------|---------|
| INSERT | frmRezervacijeNove.vb:279 | Group checkbox checked, returns @@Identity |

### 3.4 Table: `alarm`

| Operation | File:Line | Trigger |
|-----------|----------|---------|
| INSERT | frmRezervacije_unos.vb:320 | New reservation with alarm |
| INSERT | frmRezervacije_unos.vb:380 | Edit reservation, add new alarm |
| UPDATE | frmRezervacije_unos.vb:382 | Edit existing alarm |
| UPDATE | frmRezervacije_unos.vb:385 | Cancel alarm (storno=1) |

### 3.5 Table: `gosti` (during check-in)

| Operation | File:Line | Trigger |
|-----------|----------|---------|
| INSERT | frmRezervacijePrebaci.vb:603 | Check-in — clone guest |
| UPDATE | frmRezervacijePrebaci.vb:760 | Clear Rid after check-in |

### 3.6 Table: `poszetaFolio` (during check-in)

| Operation | File:Line | Trigger |
|-----------|----------|---------|
| INSERT | frmRezervacijePrebaci.vb:565 | Create folio per room |

### 3.7 Table: `relGostSoba` (during check-in)

| Operation | File:Line | Trigger |
|-----------|----------|---------|
| INSERT | frmRezervacijePrebaci.vb:648 | Link guest to room with rezervP=1 |

### 3.8 Table: `rezervacijaPrijava` (during check-in)

| Operation | File:Line | Trigger |
|-----------|----------|---------|
| INSERT | frmRezervacijePrebaci.vb:957 | Track reservation-to-checkin mapping |

---

## 4. Reservation Status Codes

### 4.1 `prijava` Field (Reservation Check-In Status)

| Value | Meaning | Evidence | Transition |
|-------|---------|----------|------------|
| 0 | Not checked in (reserved) | frmRezervacije.vb:29 (WHERE prijava=0), frmRezervacije.vb:122 (load reserved rooms), frmRezervacije_unos.vb:338 (default prijava='0') | Initial state |
| 1 | Checked in | frmRezervacijePrebaci.vb:689,726 (SET prijava=1) | After check-in all rooms/guests |
| 2 | Auto-expired (past check-in date without action) | frmRezervacije.vb:29 (SET prijava=2 WHERE prijava=0 AND checkInDate<yesterday) | System auto-transition |

### 4.2 `stornirana` Field (Cancellation/Active)

| Value | Meaning | Evidence | Transition |
|-------|---------|----------|------------|
| '0' | Active reservation | frmRezervacije.vb:122 (stornirana='0'), frmRezervacije_unos.vb:338 (default stornirana='0') | Default |
| '1' | Cancelled/storno | frmRezervacijePregled.vb:357 (SET stornirana=1), frmRezervacije_unos.vb:800 (SET stornirana='1') | User cancellation |

Transitions: 0→1 (storno), 1→0 (reactivate) — both possible from `frmRezervacije_unos.vb:788-816` and `frmRezervacijePregled.vb:294-389`

### 4.3 `potvrda` Field (Confirmation)

| Value | Meaning | Evidence | Transition |
|-------|---------|----------|------------|
| 0 | Not confirmed | frmRezervacije_unos.vb:338 (default potvrda='0'), frmRezervacije_unos.vb:836 (SET potvrda='0'), frmRezervacijePregled.vb:459 (SET potvrda=0) | Default / un-confirm |
| 1 | Confirmed | frmRezervacijePregled.vb:457 (SET potvrda=1), frmRezervacije_unos.vb:828 (SET potvrda='1') | User confirmation |

Transitions: 0→1 (confirm), 1→0 (un-confirm) — both possible

### 4.4 `brojPotvrde` Field (Confirmation Number)

- Generated as MAX(brojPotvrde)+1 — NOT atomic, race condition risk
- Set when potvrda changes to 1
- Set to `Value` (keyword, likely NULL/0) when potvrda changes to 0

### 4.5 `brojStorna` Field (Storno Number)

- Generated as MAX(brojStorna)+1 — same race condition risk
- Set when stornirana changes to 1

### 4.6 Calendar Color Codes (`frmRezervacije.vb`)

| Color | Room State | Source | Evidence |
|-------|-----------|--------|----------|
| Red | Currently occupied (checked in) | `relgostsoba` with `odjavljen=0` | frmRezervacije.vb:192, 389 |
| Yellow | Reserved, not checked in | `rezervacije` with `stornirana='0'`, `prijava=0` | frmRezervacije.vb:239 |
| Orange | Reservation overlaps with occupancy | Both relgostsoba AND reservation for same cell | frmRezervacije.vb:207, 379 |
| YellowGreen | Two reservations overlap | Two reservations for same room/date | frmRezervacije.vb:234, 413 |
| Black row | Room out of order | `sobe.ooo=1` | frmRezervacije.vb:573, 583 |
| "Sobarica!" | Room needs cleaning | `sobe.clean=0` | frmRezervacije.vb:567-571 |

### 4.7 Grid Cell Values (`otvoriForm` status codes)

| broj param | Meaning | Evidence |
|-----------|---------|----------|
| 0 | SLOBODNA (free) | frmRezervacije.vb:715 |
| 1 | ZAUZETA (occupied) | frmRezervacije.vb:718 |
| 2 | ZAUZETA (occupied, alternate code) | frmRezervacije.vb:718 |
| 3 | REZERVISANA - potvrdjeno (confirmed reservation) | frmRezervacije.vb:722 |
| 4 | ZAUZETA i REZERVISANA (occupied AND reserved) | frmRezervacije.vb:726 |
| 5 | VAN UPOTREBE (out of order) | frmRezervacije.vb:730 |
| 6 | REZERVISANA - nepotvrdjeno (unconfirmed reservation) | frmRezervacije.vb:734 |
| 7 | NIJE SPREMNA (not ready/needs cleaning) | frmRezervacije.vb:738 |

---

## 5. Business Rules

### 5.1 Confirmation Rules
- Confirmation number (`brojPotvrde`) is auto-generated as MAX+1 — no gap management, not transactional
- Confirmation sets `potvrda=1` and records `datepotvrda` timestamp
- Confirmation can be toggled off by setting `potvrda=0`
- Confirmation is available from both the detail form (`frmRezervacije_unos`) and the grid (`frmRezervacijePregled`)
- In `frmRezervacije_unos`, confirmation checkbox also records `brojPotvrde` — `frmRezervacije_unos.vb:828`

### 5.2 Cancellation/Storno Rules
- Storno toggles `stornirana` between 0 and 1
- Storno increments `brojStorna` using MAX+1 (non-atomic)
- Storno records `datestorno` timestamp and `razlogst` (reason) text
- Storno can be reversed (reactivate) by setting `stornirana=0`
- Storno is available from both detail form and grid
- Stornirana reservations are excluded from calendar display (`stornirana='0'` filter) but can be viewed via `getRezrervacijePrikaziSto` stored procedure

### 5.3 Group Rules
- Group reservations use `blokID` field (references `rezervacijegrupe.id`)
- Default `blokID=1` (meaning "no group" or "nema podataka") — `frmRezervacije_unos.vb:338`
- Group is created via INSERT into `rezervacijegrupe` when group checkbox is checked — `frmRezervacijeNove.vb:279`
- Group name is stored in `gostgrupa` field — `frmRezervacije_unos.vb:338`

### 5.4 Room Assignment Rules
- A reservation can have multiple room assignments in `rezervacijasobe`
- Each room assignment has: `sobatip` (room type), `soba` (room name), `sid` (room ID), `tarifa` (tariff amount), `pusac` (smoker flag), `brgost` (guest count), `promjena` (change counter)
- Room assignment uses nuke-and-pave pattern: DELETE all existing rows, then INSERT fresh — `frmRezervacije_unos.vb:413,416-429`
- Quick-create from calendar assigns default values: `gid=0, gost=0, promjena=0, pom=0, pom1=0, brgost=1` — `frmRezervacije.vb:967`

### 5.5 Check-In Transfer Rules (Prebaci)
- Two modes: by rooms (rbtSobe) and by guests (rbtGosti)
- **By rooms**: validates `jednaki()` — total bed count must not exceed `brojKrevZaRez * BrojKreveta`
- **By guests**: validates distinct room count vs reserved room count
- Guest records are **cloned** (INSERT new guest) during check-in — not a reference to original
- `poszetaFolio` is created per room (if not already open)
- `relGostSoba` is created with `rezervP=1` marking it as from reservation
- `nocenja` (accommodation charge) is created via `Unesinocenja` stored procedure
- After all guests/rooms checked in: `prijava=1` on reservation
- For partial check-in: `brojRezSoba` is reduced by number checked in
- `gosti.Rid` is cleared to 0 after each guest is checked in — `frmRezervacijePrebaci.vb:760`

### 5.6 Date Validation Rules
- `dtpOD` (check-in) and `dtpDO` (check-out) have mutual constraints: `dtpDO.MinDate = dtpOD.Value` and `dtpOD.MaxDate = dtpDO.Value` — `frmRezervacije_unos.vb:6-13`
- `frmRezervacijeNove.vb:325`: Rejects if check-in date is after check-out date

### 5.7 Validation Rules in `frmRezervacije_unos`
- `cmbTip` must be selected (index > 0) — `frmRezervacije_unos.vb:292`
- `txtGost.Tag` must be non-zero (guest must be selected) — `frmRezervacije_unos.vb:298`
- `numbrsoba` must be > 0 (number of rooms must be positive) — `frmRezervacije_unos.vb:303`

### 5.8 Audit Trail
- `promjena` field increments on each edit (`promjena+1`) — `frmRezervacije_unos.vb:407`
- `dateizmjena` records the edit timestamp — `frmRezervacije_unos.vb:407`
- `datestorno` records cancellation timestamp — `frmRezervacije_unos.vb:800`
- `datepotvrda` records confirmation timestamp — `frmRezervacije_unos.vb:828`
- `radnik` and `radnikID` track the worker who created/edited — `frmRezervacije_unos.vb:338`
- `vrijeme` records creation time — `frmRezervacije_unos.vb:338`

---

## 6. Cross-Reference

### 6.1 Dependencies on ModuleKod Functions

| ModuleKod Function | Used In | Purpose |
|---------------------|---------|---------|
| `mysqlExScalar(query)` | frmRezervacije.vb:29,967; frmRezervacije_unos.vb:320,338,343,357,382,385,407,413,427,800,811,828,836 | Execute scalar SQL |
| `mysqlExScalarLast(query)` | frmRezervacije.vb:942; frmRezervacije_unos.vb:320,338,380 | INSERT and get @@Identity |
| `mysqlReader(query, table)` | frmRezervacije.vb:115,120,122,297,298,463,464; frmRezervacije_unos.vb:42,107,108,849,912; frmRezervacijePrebaci.vb:547 | Execute reader, return DataTable |
| `sobe_load()` | frmRezervacije.vb:31 (in Load) | Load room data into ds("sobe") |
| `getidd()` | frmRezervacije.vb:936; frmRezervacije_unos.vb:307,338,357,427 | Generate unique ID |
| `funkcije.logs()` | frmRezervacije.vb:943; frmRezervacije_unos.vb:341,313 | Write audit log |
| `funkcije.troskoviSVineplaceni` | frmRezervacije.vb:112,294,460 | Get unpaid expenses per room |
| `funkcije.WriteSQLError()` | Multiple forms | Error logging |
| `funkcije.WriteSystemError()` | Multiple forms | Error logging |

### 6.2 Stored Procedures Referenced

| SP Name | Called From | Purpose |
|---------|------------|---------|
| `getRezrervacijePrikazi` | frmRezervacije.vb:74, frmRezervacijePregled.vb:33 | All reservations in date range |
| `getRezrervacijePrikaziPot` | frmRezervacije.vb:76, frmRezervacijePregled.vb:35 | Confirmed reservations |
| `getRezrervacijePrikaziSto` | frmRezervacije.vb:78, frmRezervacijePregled.vb:37 | Cancelled reservations |
| `getRezrervacijePrikaziOD` | frmRezervacije.vb:84, frmRezervacijePregled.vb:40 | Reservations by date |
| `getVratiZauzete` | frmRezervacijePrebaci.vb:240 | Occupied rooms in date range |
| `vratiTarifePoVrsta` | frmRezervacije_unos.vb:522, frmRezervacijeNove.vb:498 | Tariffs by room type |
| `Unesinocenja` | frmRezervacijePrebaci.vb:793 | Create accommodation charge |

### 6.3 Database Tables Referenced

| Table | Operations | Forms |
|-------|-----------|-------|
| `rezervacije` | SELECT, INSERT, UPDATE | All reservation forms |
| `rezervacijasobe` | SELECT, INSERT, DELETE | frmRezervacije, frmRezervacije_unos |
| `rezervacijegrupe` | INSERT, SELECT | frmRezervacijeNove |
| `rezervacijetip` | SELECT | frmRezervacije_unos, frmRezervacijeNove |
| `rezervacijeizvor` | SELECT | frmRezervacije_unos, frmRezervacijeNove |
| `rezervacijaPrijava` | INSERT | frmRezervacijePrebaci |
| `gosti` | SELECT, INSERT, UPDATE | frmRezervacije_unos, frmRezervacijePrebaci, frmRezervacijePregled |
| `sobe` | SELECT | All forms |
| `sobavrsta` | SELECT | frmRezervacije_unos, frmRezervacijeNove, frmRezervacijePrebaci |
| `sobaTarifa` | SELECT | frmRezervacije_unos, frmRezervacijeNove |
| `relSobaVrstaSobaTarifa` | SELECT (via SP) | frmRezervacijePrebaci |
| `relGostSoba` | SELECT, INSERT | frmRezervacije (calendar), frmRezervacijePrebaci |
| `poszetaFolio` | INSERT, SELECT | frmRezervacijePrebaci |
| `alarm` | INSERT, UPDATE, SELECT | frmRezervacije_unos |
| `partneri` (via firme) | SELECT | frmRezervacije_unos |
| `nocenja` | INSERT (via SP) | frmRezervacijePrebaci |

### 6.4 Form Navigation

```
frmRezervacije (Calendar/Grid)
  ├── Double-click occupied cell → frmSobaInfo
  ├── Double-click yellow cell → frmRezervacije_unos (edit)
  ├── Double-click yellowgreen cell → frmRezervacije_unos (edit, with overlap warning)
  ├── Button3 / ButtonX3 → frmRezervacije_unos (new, Tag=0)
  ├── Cell edit → Direct INSERT (quick-create)
  ├── PrijavaToolStripMenuItem → frmPrijava1 (check-in)
  ├── OdjavaToolStripMenuItem → frmOdjava1 (check-out)
  ├── TroskoviToolStripMenuItem → frmTroskovi (expenses)
  ├── PlacanjeToolStripMenuItem → frmPlati1 (payment)
  ├── ButtonX2 → frmRezervacijePregled (list view)
  └── ButtonX4 → frmTrosSvi (all expenses)

frmRezervacijePregled (List View)
  ├── btnIzmjeni / CellDoubleClick → frmRezervacije_unos (edit)
  ├── btnPrebaci → frmRezervacijePrebaci (transfer to check-in)
  ├── btnStoniraj → Toggle storno
  ├── btnPotvrdi → Toggle confirmation
  └── btnIzvjestaj → rptRezervacijePoj (report)

frmRezervacije_unos (Entry/Edit Form)
  ├── Tag=0 → Create new reservation
  ├── Tag>0 → Edit existing reservation
  ├── txtGost → frmPrijavaGostiUnos (guest selection)
  ├── Button7 → frmPartner1 (company/partner selection)
  └── Button2 → frmNovitip (add reservation type)

frmRezervacijeNove (Simplified New)
  ├── btnPrikazi → frmPrijavaGostiUnos (guest selection)
  ├── dgmDodaj → frmTarife (tariff selection)
  └── btnDodajIzvor → frmNoviIzvor (add source)

frmRezervacijePrebaci (Transfer to Check-In)
  └── (check-in process creates guest, folio, room assignment, charges)
```

---

## 7. Key Findings for Modern System

### 7.1 Critical Issues

1. **Race condition in brojPotvrde/brojStorna**: Using `MAX()+1` without transactions can produce duplicate numbers under concurrency. Should use auto-increment or sequence — `frmRezervacijePregled.vb:323-351,422-451`; `frmRezervacije_unos.vb:800,828`

2. **SQL Injection vulnerabilities**: Multiple queries use string concatenation with user input, especially in `frmRezervacije.vb:942,967` (calendar quick-create), `frmRezervacije_unos.vb:338,357,407,413,427` (insert/update), `frmRezervacijePregled.vb:627` (search)

3. **Nuke-and-pave pattern in rezervacijasobe**: DELETE all rows then INSERT new ones on every edit loses audit history and risks data loss on failure — `frmRezervacije_unos.vb:413-429`

4. **Guest cloning on check-in**: The `PrijaviGosta()` function creates a duplicate `gosti` record rather than linking to the existing guest, which creates data inconsistency — `frmRezervacijePrebaci.vb:593-641`

5. **No transaction wrapping**: Multi-step operations (check-in creates guest, folio, room assignment, charge, reservation update) have no BEGIN/COMMIT/ROLLBACK. Partial failures leave inconsistent state — `frmRezervacijePrebaci.vb:405-497`

6. **Stale reservation auto-expire**: `prijava=2` marks expired reservations on form load (`frmRezervacije.vb:29`), but this runs per-client, not as a server-side job. Multiple clients could race on this.

### 7.2 Architectural Issues

7. **Business logic in UI**: All reservation CRUD, validation, status transitions, and business rules are embedded in form event handlers. No service layer, no repository pattern.

8. **Duplicate code across forms**: `frmRezervacije` has three nearly identical `proracun` methods (lines 107-620) with 90% code duplication. `frmRezervacijePregled` has duplicate `Izvjestaji()` method identical to `vratiSobe()`.

9. **Type inconsistencies**: `stornirana` is compared as string `'0'`/`'1'` in some queries (`frmRezervacije.vb:122`) but as integer in others (`frmRezervacijePregled.vb:357`). `prijava` is compared as numeric 0/1/2.

10. **Missing validation**: Calendar quick-create (frmRezervacije.vb:942) allows creating reservations with GID=0 (no guest), no room type, and all zeros for tipID/izvorID.

11. **No overlap detection on creation**: Reservation creation does not check if the room is already reserved for the same dates. Conflicts are only displayed visually (overlapping colors) in the calendar grid.

12. **Hardcoded defaults**: `blokID=1` (no group), `tipID=1` (default type), `izvorID=0` (no source) — magic numbers with no documentation.

### 7.3 Reservation State Machine

```
   ┌──────────────┐
   │  NEW (CREATE) │
   │  prijava=0    │
   │  stornirana=0 │
   │  potvrda=0    │
   └──────┬───────┘
          │
    ┌─────┴────────────┐
    │                  │
    ▼                  ▼
┌───────────┐   ┌───────────┐
│ CONFIRMED  │   │  STORNIRANA│
│ potvrda=1  │   │ stornirana=1│
└─────┬─────┘   └─────┬─────┘
      │                │
      │ toggle         │ toggle
      ▼                ▼
┌───────────┐   ┌───────────┐
│ UNCONFIRMED│   │ REACTIVATED│
│ potvrda=0  │   │ stornirana=0│
└───────────┘   └───────────┘

    Any state (prijava=0)
         │
         │ Check-in (prebaci)
         ▼
  ┌──────────────┐
  │  CHECKED-IN   │
  │  prijava=1     │
  └──────────────┘

    Any state (prijava=0, expired date)
         │
         │ Auto-expire
         ▼
  ┌──────────────┐
  │   EXPIRED     │
  │  prijava=2    │
  └──────────────┘
```

### 7.4 Key Data Model (Reservation Tables)

```
rezervacije (master)
├── id (PK, auto)
├── GID (FK → gosti)
├── checkInDate, checkOutDate
├── potvrda (0/1)
├── brojPotvrde
├── blokID (FK → rezervacijegrupe, 1=no group)
├── tipID (FK → rezervacijetip)
├── izvorID (FK → rezervacijeizvor)
├── sobaVrstaID (FK → sobavrsta)
├── stornirana ('0'/'1')
├── brojStorna
├── brojRezSoba (number of rooms)
├── godina
├── prijava (0=new, 1=checked-in, 2=expired)
├── tarifa
├── memo
├── radnik, radnikID
├── vrijeme (creation timestamp)
├── idd
├── gost (guest name text)
├── tex
├── napomena (notes)
├── alarmid (FK → alarm)
├── gostgrupa (group name)
├── promjena (change counter)
├── promjenat
├── kontakt, kontakttel, kontaktfax, kontaktmob, kontaktmail
├── plac (payment method ID)
├── placanje (payment method name)
├── firma, firmaid, agencija, komerc, agencijaid, komercid
├── brosoba, brdjeca
├── dateizmjena, datestorno, datepotvrda, razlogst
└── rezervacijasobe[] (1:N)

rezervacijasobe (room lines)
├── id (PK)
├── rezid (FK → rezervacije)
├── sobtid (room type ID)
├── sobatip (room type name)
├── sid (FK → sobe)
├── soba (room name)
├── tid (tariff ID)
├── tarifa (tariff amount)
├── gid (guest ID per room)
├── gost (guest name per room)
├── idd
├── promjena, pom, pom1
├── pusac (smoker flag)
├── brgost (guest count per room)
├── gost1, cjenovnik
└── 

rezervacijaPrijava (check-in mapping)
├── IDrez (FK → rezervacije)
├── IDGost (FK → gosti)
└── sobaID (FK → sobe)

rezervacijegrupe (groups)
├── id (PK)
├── naziv
└── odjavljena

rezervacijetip (types)
├── id (PK)
└── naziv

rezervacijeizvor (sources)
├── id (PK)
└── naziv
```

### 7.5 Recommendations for Modern System

1. **Introduce proper transaction management** for all multi-table operations (especially check-in transfer)
2. **Replace MAX()+1** with database sequences or auto-increment for brojPotvrde, brojStorna
3. **Use parameterized queries** throughout — eliminate all string concatenation SQL
4. **Create a ReservationService** with methods: Create, Confirm, Cancel, CheckIn, Transfer, Update
5. **Implement overlap validation** at the service layer (not just visual)
6. **Separate reservation status** into a proper enum/state machine with defined transitions
7. **Use soft delete pattern** instead of nuke-and-pave for rezervacijasobe
8. **Don't clone guest records** on check-in — use references to existing guest
9. **Schedule prijava=2 expiration** as a server-side scheduled job, not client-side
10. **Normalize redundant fields**: `gost` text is redundant with `GID`; `soba` text redundant with `sid`; `tarifa` amount redundant with `tid`