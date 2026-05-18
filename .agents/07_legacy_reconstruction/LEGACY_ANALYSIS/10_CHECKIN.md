# Check-in (Prijava) Flow — Legacy Analysis

> Source files analyzed:
> - `legacy_code/frmPrijava1.vb` (1368 lines) — Main check-in form
> - `legacy_code/frmPrijavaGostiKucice.vb` (~860 lines) — Guest cottage/separate check-in form
> - `legacy_code/frmPrijavaGostiUnos.vb` (~930 lines) — Guest data entry form
> - `legacy_code/frmPrijavaBoravkaPodaci.vb` (27 lines) — Residence report data form
> - `legacy_code/frmReportPrijavaBoravka.vb` (52 lines) — Crystal Reports residence report viewer
> - `legacy_code/frmPrikazNocenja1.vb` (39 lines) — Night charges display (edit mode)
> - `legacy_code/frmPrikazNocenja2.vb` (72 lines) — Night charges display (room view)
>
> Cross-referenced with:
> - `LEGACY_ANALYSIS/02_MODULEKOD_FUNCTIONS.md`
> - `legacy_code/Data.vb` (nocenjeSo function, lines 474-499)
> - `legacy_code/ModuleKod.vb` (stored procedure definitions)

---

## 1. Business Flow: Check-in Process (step by step)

### 1.1 Form Loading (frmPrijava1_Load, line 271-296)

1. Clear internal state: `izborForme = False`, clear `hTable`
2. Load reservation groups combo (`ucitajComboGrupe`) — queries `rezervacijegrupe` for active groups
3. Load room types combo (`razliciteVrsteSoba`) — queries `sobavrsta`
4. Load room availability grids:
   - Free rooms (`VratiSlobodne`) — stored proc `vratiTrenutnoSlobodne`
   - Reserved rooms (`VratiRezervisane`) — stored proc `vratiTrenutnoRezervisane`
   - Occupied rooms (`VratiZauzete`) — stored proc `vratiTrenutnoZauzete`
5. Load reservations for current date (`dgvRezrvacijaPrijava`)
6. Set default checkout date to tomorrow
7. If coming from room view (`akcij="soba"`), pre-select room type and room

### 1.2 Room Selection (cmbTipSobe_SelectedIndexChanged, line 140-147)

1. User selects room type → triggers `sobe()` to load rooms of that type
2. Loads tariffs for selected room type (`ucitajTarife`) — stored proc `vratiTarifePoVrsta`

### 1.3 Room Name Selection (cmbNazivSobe_SelectedIndexChanged, line 148-171)

1. Loads already-checked-in guests for the room (`ucitajPrijavljeneGostePrijava`)
2. Displays guest list in dgvGosti grid
3. Determines key/card type from `sobe.idkon`: 0=key, 1=card — updates PictureBox icon
4. If no room selected, clears guest grid

### 1.4 Adding Guests (btnDodajGosta_Click, line 249-264)

1. Opens `frmPrijavaGostiUnos` dialog
2. Sets `frm.mycaller = Me` (parent form reference)
3. In frmPrijavaGostiUnos, user enters/edits guest data (see 1.5)
4. On close, guest data returns via `mycaller.dodajGosta(...)` callback

### 1.5 Guest Data Entry (frmPrijavaGostiUnos)

**On Load** (`frmPrijavaGostiUnos_Load`, line ~1380):
1. Load document types (`getDokumenti` SP)
2. Load guest statuses (`goststatus` where `del=0`)
3. Load citizenship/countries list (`drzave` table)
4. Load tariff list from parent form's `ds.Tables("tarifa")`
5. Set default tariff selection from parent

**Searching guests** (`trazi()`, line ~860):
1. Dynamic LIKE search on `gosti` table by surname and name
2. Results displayed in `grdGostiListing`
3. Clicking a row populates all fields and loads previous visits (`getPosjete` SP)

**Auto age-based status** (`mtbDatum_TypeValidationCompleted`, line ~895):
1. On valid date entry, calculates age from birth date
2. Age < 12 → status = 4 (child)
3. Age < 18 → status = 3 (minor/underage)
4. Otherwise → status = 1 (adult)

**Auto discount** (same handler):
1. If `setings.dijecagod > 0` and age < `dijecagod`: automatic discount from `setings.dijecapop`

**Saving guest** (`dgmTraziGosta_Click`, line ~790):
1. If `txtID` empty → new guest → calls `unesi()` (INSERT INTO gosti)
2. If `txtID` has value → existing guest → calls `promijeni()` (SP `promijeniGosti`)
3. After save, calls parent's `dodajGosta(gostID, name, discount, reason, tariff, taksa, status)` or `mycaller1.prijavljeniGosti(...)`
4. Updates `relgostsoba.status` and `relgostsoba.taksa` if editing existing

### 1.6 Adding Guest to Room (frmPrijava1.dodajGosta, line 220-247)

1. Checks if guest already in grid (`dtPrijavljeniGosti.Select("gostID=" & id)`)
2. If duplicate: shows error "Isti gost ne moze biti prijavljen vise puta"
3. If new: adds row to `dtPrijavljeniGosti` with guest data
4. Adds `gostID → popust` to `hTable` (Hashtable tracking new guests)
5. Updates button appearance: red if guests present, lavender if none
6. Updates `frmGlavni.forma` flag (0=no new guests, 1=has new guests)

### 1.7 Removing Guest from List (btnObrisiGosta_Click, line 336-360)

1. Only allows removal if guest is in `hTable` (i.e., newly added)
2. If guest was previously checked-in (not in hTable): "Gost je ranije prijavljen te se nemoze obrisati, molimo idite na Odjavu gosta"
3. Removes from `hTable` and `dtPrijavljeniGosti`
4. Updates button appearance

### 1.8 Submitting Check-in (btnPrijava_Click, line 363-489)

**Validations** (lines 365-386):
1. Room type must be selected
2. Room must be selected
3. Check-in date must not be >20 days before last check-in date for the room (`vratiMaxDatum` SP)
4. Check-out date must be after check-in date
5. At least one guest must be in the grid
6. At least one new guest must be in `hTable`

**Processing steps** (lines 388-488):
1. Get room ID from combo
2. Count guests for per-person calculations
3. **Check/create folio**: `provjeriPID()` checks if room already has open folio → if 0, create new (`dodajFolio()`)
4. **Insert guest-room relationships**: `unesi(FolID)` iterates `hTable`, calling SP `addRelGostSoba` for each new guest
5. **Create night charges**: `nocenja()` generates accommodation charges for each new guest
6. **Card encoding** (if applicable): if `kardtip=1` and room has card controller (`idkon≠0`), offers to encode card via `frmKardPro`
7. **Update reservation** (if from reservation): marks `rezervacije.prijava=1`
8. **Mark room for cleaning**: calls SP `updateSobaClean` with `clean=1`
9. Show success message, reset form

### 1.9 Guest Edit for Cottages (frmPrijavaGostiKucice)

This form is used for editing existing guest data during check-in for standalone units (kucice):
1. Called from frmSobaInfo with `prenos(GID, rid)`
2. Loads guest data via `getGostiKucice` SP
3. Loads citizenship, document type, status
4. On save (`dgmTraziGosta_Click`): calls `promijeni()` SP then updates `relgostsoba.taksa` and `relgostsoba.status`

### 1.10 Night Charges View (frmPrikazNocenja1 & frmPrikazNocenja2)

**frmPrikazNocenja1** (per-guest night view):
- `ucitaj(br)`: SELECT from `nocenja` WHERE `RID=br AND PrijavaOdjava=0`
- Editing via `clasMysqlAdapt` — direct save to database
- Raises `osvjeziPodatke1` event on form close

**frmPrikazNocenja2** (per-room night view):
- `ucitaj(sid, pid)`: Uses `nocenjeSo()` from Data.vb — SELECT with guest names
- `snimi(id, tar)`: UPDATE `nocenja` SET Tarifa WHERE ID
- Raises `osvjeziPodatke2` event on form close

### 1.11 Residence Report (frmPrijavaBoravkaPodaci → frmReportPrijavaBoravka)

Simple data collection form with fields:
- txtViza (visa number), txtGranica (border crossing), txtBoravak (stay purpose)
- txtPrebivaliste (residence), txtBoraviste (stay location), txtNapomena (note)

Passes values via global variables (`txt1`-`txt7`) to Crystal Reports viewer.
Report uses `ds.Tables("GostiListaTurist")` as data source.

---

## 2. SQL Inventory

### 2.1 Stored Procedures Called

| SP Name | Called In | Parameters | Operation | Purpose |
|---------|-----------|------------|-----------|---------|
| `addRelGostSoba` | frmPrijava1.vb:635 | gostID, sobaID, checkInDate, checkInRadnik, checkOutDate, checkOutRadnik, stampanaPrijava, odjavljen, rezervacija, rezervacijaP, grupaID, tarifaID, PID, popust, nap, usl, taksa, stat | INSERT INTO relgostsoba | Register guest in room |
| `addFolio` | frmPrijava1.vb:723 | sSID, svrijemeD, svrijemeO, szakljucen | INSERT INTO posjetafolio + SELECT @@Identity | Create payment folio for room |
| `Unesinocenja` | frmPrijava1.vb:819 | RID, DatumPp, Tarifa, SID, PID, opis, Pop, ssoba | DELETE + INSERT INTO nocenja | Create nightly accommodation charge (deletes same-month existing first) |
| `vratiMaxDatum` | frmPrijava1.vb:535 | sidd | SELECT COALESCE(MAX(DatumP)) | Get latest night charge date for room |
| `vratiTarifePoVrsta` | frmPrijava1.vb:854 | svid | SELECT sobatarifa JOIN relsobavrstasobatarifa | Get tariffs for room type |
| `vratiTrenutnoSlobodne` | frmPrijava1.vb:907 | (none) | SELECT sobavrsta, COUNT(*) | Count free rooms by type |
| `vratiTrenutnoRezervisane` | frmPrijava1.vb:941 | Dayy (DATE) | SELECT rezervacije JOIN sobavrsta | Count reserved rooms by type for date |
| `vratiTrenutnoZauzete` | frmPrijava1.vb:984 | (none) | SELECT sobavrsta LEFT JOIN occupied | Count occupied rooms by type |
| `updateSobaClean` | frmPrijava1.vb:504 | naziv, clean | UPDATE sobe SET clean | Mark room for cleaning after check-in |
| `getDokumenti` | frmPrijavaGostiUnos.vb:705, frmPrijavaGostiKucice.vb:548 | (none) | SELECT FROM gostdokument | Load document types for guest form |
| `getGostiKucice` | frmPrijavaGostiKucice.vb:640 | br (guest ID) | SELECT FROM gosti | Load single guest data for editing |
| `getPosjete` | frmPrijavaGostiUnos.vb:1215 | GID | SELECT relgostsoba JOIN sobe, sobatarifa | Load guest's previous stays |
| `promijeniGosti` | frmPrijavaGostiUnos.vb:893, frmPrijavaGostiKucice.vb:812 | 14 params | UPDATE gosti | Update guest personal data |

### 2.2 Direct SQL Statements

| Line | File | Operation | SQL | Purpose |
|------|------|-----------|-----|---------|
| 20 | frmPrijava1.vb | SELECT | `SELECT ID,naziv FROM sobavrsta` | Load room types |
| 63 | frmPrijava1.vb | SELECT | `SELECT relgostsoba.gostID,concat(gosti.prezime,' ',gosti.ime) AS ImePrezime, relgostsoba.checkInDate, relgostsoba.popust, gosti.prezime as opis, sobatarifa.naziv as tarifa, relgostsoba.taksa, relgostsoba.status FROM relgostsoba INNER JOIN gosti ON relgostsoba.gostID = gosti.ID inner join sobatarifa on relgostsoba.tarifaID = sobatarifa.ID WHERE (relgostsoba.sobaID = {sid}) AND (relgostsoba.odjavljen = 0)` | Load checked-in guests for a room |
| 105 | frmPrijava1.vb | SELECT | `SELECT ID,naziv,idkon FROM sobe WHERE sobe.vrsta = @sid AND sobe.ooo=0 ORDER BY naziv` | Load rooms by type (excluding OOO) |
| 194 | frmPrijava1.vb | SELECT | `SELECT rezervacijegrupe.ID, rezervacijegrupe.naziv FROM rezervacijegrupe WHERE rezervacijegrupe.odjavljena = 0` | Load active reservation groups |
| 212 | frmPrijava1.vb | SELECT | `select id,naziv from troskovivrste where del=0 and tip=1` | Load accommodation expense types |
| 299 | frmPrijava1.vb | SELECT | `SELECT ID,naziv,idkon FROM sobe WHERE sobe.naziv like '{sid}' AND sobe.ooo=0 ORDER BY naziv` | Search room by name (SQL injection risk) |
| 684 | frmPrijava1.vb | SELECT | `SELECT relgostsoba.PID FROM relgostsoba WHERE (relgostsoba.sobaID = {x}) AND (relgostsoba.odjavljen = 0)` | Check open folio for room |
| 790 | frmPrijava1.vb | SELECT | `SELECT relgostsoba.ID FROM relgostsoba WHERE relgostsoba.gostID = @ID AND relgostsoba.odjavljen = 0 AND relgostsoba.sobaID = @sobaID` | Get relgostsoba ID for night charge insertion |
| 1062 | frmPrijava1.vb | SELECT | `SELECT t1.ID,napomena,t1.GID,t1.memo,concat(ime,' ',prezime) as ime,t1.checkInDate,t1.sobaVrstaID ,t1.brojRezSoba,t1.checkOutDate,sobavrsta.naziv,t1.tarifa,t1.blokID,t1.prijava FROM rezervacije t1 left JOIN gosti ON t1.GID = gosti.ID left JOIN sobavrsta ON t1.sobaVrstaID=sobavrsta.ID WHERE t1.CheckInDate='{date}' AND t1.stornirana = 0` | Load reservations for date (SQL injection risk) |
| 1256 | frmPrijava1.vb | SELECT | `SELECT s.gost as ime,s.memo as prezime,checkInDate,checkOutDate,r.soba,r.gid as gostID,s.kontakt as drzavljanstvo,s.id,r.sid as sobaID,s.radnikid as checkInRadnik,... FROM rezervacije s inner join rezervacijasobe r on r.rezid=s.id left JOIN sobe so ON r.sid=so.id where s.id={id}` | Load reservation detail with room info |
| 463 | frmPrijava1.vb | UPDATE | `update rezervacije set prijava=1 where id={id}` (via mysqlExScalar) | Mark reservation as checked-in |
| 1336 | frmPrijava1.vb | UPDATE | `Update sobatarifa set del=1 where id={id}` (via mysqlExScalar) | Soft-delete tariff |
| 1353 | frmPrijava1.vb | UPDATE | `update sobe set idkon={tag} where id='{selectedValue}'` (via mysqlExScalar) | Toggle room key/card type |
| ~690 | frmPrijavaGostiUnos.vb | INSERT | `INSERT INTO gosti (prezime, ime, adresa, datumrodjenja, mjestodrzavaR, pol, drzavljanstvo, dokument, brDokument, telefon, mobitel, email, DID) VALUES(...); Select @@IDENTITY` | Create new guest record |
| ~710 | frmPrijavaGostiKucice.vb | UPDATE | `update relgostsoba set taksa=0, status='{status}' where id='{rid}'` (via mysqlExScalar) | Update guest tax and status |
| ~830 | frmPrijavaGostiUnos.vb | SELECT | `SELECT * FROM gosti WHERE prezime LIKE '%{text}%' and ime LIKE '%{text}%' limit 100` | Search guests (SQL injection risk) |
| ~850 | frmPrijavaGostiUnos.vb | INSERT | `insert into gostdokument (naziv) values ('{text}')` or `insert into drzave (id,naziv,domaca,sifra) select (max+1),'{text}',0,'{text}'` | Add document type or country (SQL injection risk) |
| ~860 | frmPrijavaGostiUnos.vb | SELECT | `select naziv from gostdokument where naziv='{text}'` or `select naziv,domaca,sifra from drzave where naziv='{text}'` | Check existing document/country (SQL injection risk) |
| ~690 | frmPrijavaGostiKucice.vb | SELECT | `SELECT drzave.ID, drzave.naziv FROM drzave` | Load citizenship list |
| ~740 | frmPrijavaGostiKucice.vb | SELECT | `SELECT g.id, g.naziv FROM goststatus g where del=0` | Load guest status types |
| ~750 | frmPrijavaGostiKucice.vb | SELECT | `SELECT ID, gostID, sobaID, taksa, status, tid FROM relgostsoba where id='{rid}'` | Load current guest-room record |
| ~820 | frmPrijavaGostiUnos.vb | SELECT | `SELECT drzave.ID, drzave.naziv, domaca FROM drzave order by naziv` | Load citizenship with domestic flag |
| 5 | frmPrikazNocenja1.vb | SELECT | `SELECT DatumP, Tarifa, ID, popust, opis FROM nocenja WHERE RID = {br} AND PrijavaOdjava = 0` | Load night charges for guest |
| 27 | frmPrikazNocenja2.vb | UPDATE | `UPDATE nocenja SET Tarifa = '{tar}' WHERE ID = {id}` | Update night charge tariff |
| 474-484 | Data.vb | SELECT | `SELECT gosti.ime, gosti.prezime, nocenja.soba, nocenja.DatumP, nocenja.Tarifa, nocenja.ID, nocenja.popust, nocenja.opis, nocenja.sid, nocenja.pid, nocenja.rid FROM nocenja JOIN relgostsoba JOIN gosti WHERE [conditions]` | Load night charges with guest names |

---

## 3. Database Writes

### 3.1 INSERT Operations

| Operation | Table | Columns | Trigger | Business Reason | Evidence |
|-----------|-------|---------|---------|-----------------|----------|
| `addRelGostSoba` SP | relgostsoba | gostID, sobaID, checkInDate, checkInRadnik, checkOutDate, checkOutRadnik, stampanaPrijava, odjavljen, rezervacija, grupaID, tarifaID, PID, rezervP(=rezervacijaP), popust, usluga(=nap), napomenapl(=nap), taksa, status | btnPrijava_Click → unesi() | Register guest in room with all check-in parameters | frmPrijava1.vb:635 |
| `addFolio` SP | posjetafolio | SID, vrijemeD, vrijemeO(=NULL), zakljucen(=0) | btnPrijava_Click | Create payment folio for room stay | frmPrijava1.vb:723 |
| `Unesinocenja` SP | nocenja | RID, DatumP, Tarifa, SID, PID, PrijavaOdjava(=0), opis, popust, soba | btnPrijava_Click → nocenja() → dodajnocenja() | Create nightly accommodation charge (deletes same-month existing first!) | frmPrijava1.vb:819 |
| Direct INSERT | gosti | prezime, ime, adresa, datumrodjenja, mjestodrzavaR, pol, drzavljanstvo, dokument, brDokument, telefon, mobitel, email, DID | frmPrijavaGostiUnos.unesi() | Create new guest record | frmPrijavaGostiUnos.vb:~690 |
| Direct INSERT | gostdokument | naziv | frmPrijavaGostiUnos.dodaj(0,...) | Add new document type | frmPrijavaGostiUnos.vb:~850 |
| Direct INSERT | drzave | id, naziv, domaca, sifra | frmPrijavaGostiUnos.dodaj(1,...) | Add new country/citizenship | frmPrijavaGostiUnos.vb:~850 |

### 3.2 UPDATE Operations

| Operation | Table | SET | WHERE | Trigger | Business Reason | Evidence |
|-----------|-------|-----|-------|---------|-----------------|----------|
| Direct UPDATE | rezervacije | prijava=1 | id={selectedReservationID} | btnPrijava_Click (via mysqlExScalar) | Mark reservation as checked-in | frmPrijava1.vb:463 |
| `updateSobaClean` SP | sobe | clean=1 | naziv={roomName} | btnPrijava_Click → updateClean() | Mark room needs cleaning after check-in | frmPrijava1.vb:504 |
| `promijeniGosti` SP | gosti | prezime, ime, adresa, datumRodjenja, mjestodrzavaR, pol, drzavljanstvo, dokument, brDokument, telefon, mobitel, email, DID | ID=brojGosta | frmPrijavaGostiUnos.promijeni(), frmPrijavaGostiKucice.promijeni() | Update guest personal data on check-in | frmPrijavaGostiUnos.vb:893, frmPrijavaGostiKucice.vb:812 |
| Direct UPDATE | relgostsoba | taksa=0, status='{status value}' | id='{rid}' | frmPrijavaGostiKucice.dgmTraziGosta_Click() | Update tax and status for existing guest-room record | frmPrijavaGostiKucice.vb:~710 |
| Direct UPDATE | sobatarifa | del=1 | id={selectedTariffID} | frmPrijava1.BrisiToolStripMenuItem_Click | Soft-delete tariff | frmPrijava1.vb:1336 |
| Direct UPDATE | sobe | idkon={0 or 1} | id={roomID} | frmPrijava1.PictureBox1_Click | Toggle key/card lock type for room | frmPrijava1.vb:1353 |
| Direct UPDATE | nocenja | Tarifa={tar} | ID={id} | frmPrikazNocenja2.snimi() | Edit night charge rate | frmPrikazNocenja2.vb:27 |
| Direct UPDATE | drzave | naziv, domaca, sifra | naziv={selectedCountryName} | frmPrijavaGostiUnos.IzmjeniToolStripMenuItem_Click | Edit country record | frmPrijavaGostiUnos.vb:~860 |

### 3.3 DELETE Operations (within SP logic)

| Operation | Table | WHERE | Trigger | Business Reason | Evidence |
|-----------|-------|-------|---------|-----------------|----------|
| `Unesinocenja` SP (DELETE part) | nocenja | RID={RID} AND DATE_FORMAT(datump,'%Y-%d-%m')=DATE_FORMAT(DatumPp,'%Y-%d-%m') | btnPrijava_Click → nocenja() → dodajnocenja() | Remove existing night charges for same month before re-inserting (month-level upsert) | ModuleKod.vb:1081 |

**CRITICAL**: The `Unesinocenja` SP first DELETES all night charges for the same month for this guest-room, then INSERTS a new one. This means any manual edits to night charges for that month are lost on re-check-in.

---

## 4. Statuses / Flags / Magic Values

### 4.1 relgostsoba.odjavljen
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Currently checked in (active) | frmPrijava1.vb:63 WHERE odjavljen=0 selects active guests; frmPrijava1.vb:684 WHERE odjavljen=0 finds open folio |
| 1 | Checked out (implied by WHERE odjavljen=0 exclusion) | addRelGostSoba inserts with odjavljen=0 |

### 4.2 relgostsoba.rezervacija
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Actual stay (not a reservation) | frmPrijava1.vb:601 — always inserted as False |

### 4.3 relgostsoba.rezervacijaP (mapped to rezervP in SP)
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Not confirmed reservation | frmPrijava1.vb:603 — always inserted as False |

### 4.4 relgostsoba.stampanaPrijava
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Registration not printed | frmPrijava1.vb:595 — always inserted as False |

### 4.5 relgostsoba.status (goststatus reference)
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Default/unknown | frmPrijava1.vb:625 — default value; frmPrijavaGostiKucice.vb:~710 set on edit |
| 1 | Adult (standard rate) | frmPrijavaGostiUnos.vb:auto-assign on valid date; frmPrijavaGostiKucice.vb: status=1 default |
| 3 | Minor (under 18) | frmPrijavaGostiUnos.vb: age < 18 → status 3; frmPrijavaGostiKucice.vb: same logic |
| 4 | Child (under 12) | frmPrijavaGostiUnos.vb: age < 12 → status 4; frmPrijavaGostiKucice.vb: same logic |

### 4.6 relgostsoba.taksa
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Default (no tax override) | frmPrijava1.vb:623 |
| From grid | Copied from dtPrijavljeniGosti | frmPrijava1.vb:628 |

### 4.7 relgostsoba.popust (Discount %)
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | No discount | frmPrijava1.vb:623 default; auto-populated from settings for children |
| Settings.dijecapop | Auto-discount percentage for children under `dijecagod` years | frmPrijavaGostiUnos.vb: auto-assign logic |

### 4.8 posjetafolio.zakljucen
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Folio is open (not closed) | frmPrijava1.vb:719 — always False on creation |

### 4.9 posjetafolio.vrijemeO
| Value | Meaning | Evidence |
|-------|---------|----------|
| NULL | Checkout time not set (ongoing stay) | frmPrijava1.vb:718 — DBNull.Value on creation |

### 4.10 rezervacije.prijava
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Not checked in yet | frmPrijava1.vb:1062 WHERE prijava filter; ModuleKod.vb:441 |
| 1 | Guest checked in / reservation realized | frmPrijava1.vb:463, 1121; ModuleKod.vb:441 auto-setting |

### 4.11 rezervacije.stornirana
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Active (not cancelled) | frmPrijava1.vb:1062 WHERE stornirana=0 |

### 4.12 sobe.ooo (Out of Order)
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Room in service (available) | frmPrijava1.vb:105 WHERE ooo=0 |
| 1 | Room out of order (excluded from check-in) | Excluded by filter |

### 4.13 sobe.clean
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 or NULL | Room clean or not flagged | updateSobaClean sets clean=1 |
| 1 | Room needs cleaning | Set after check-in to flag housekeeping |

### 4.14 sobe.idkon (Key/Card type)
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Key lock (mechanical) | frmPrijava1.vb:164 — shows key icon |
| 1 | Card lock (electronic) | frmPrijava1.vb:167 — shows card icon; triggers card encoding |

### 4.15 nocenja.PrijavaOdjava
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Regular accommodation charge (night) | frmPrikazNocenja1.vb:5, Data.vb:479-483 |

### 4.16 Guest gender (pol)
| Value | Meaning | Evidence |
|-------|---------|----------|
| "M" | Male | frmPrijavaGostiUnos.vb, frmPrijavaGostiKucice.vb |
| "Z" | Female | Same |

### 4.17 sobatarifa.del (Soft delete)
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | Active tariff | Loaded by vratiTarifePoVrsta WHERE del=0 |
| 1 | Deleted tariff | frmPrijava1.vb:1336 sets del=1 |

### 4.18 troskovivrste.tip
| Value | Meaning | Evidence |
|-------|---------|----------|
| 1 | Accommodation service type | frmPrijava1.vb:212 WHERE tip=1 |

### 4.19 frmGlavni.forma (UI state)
| Value | Meaning | Evidence |
|-------|---------|----------|
| 0 | No new check-in guests | frmPrijava1.vb:236, 350, 480 |
| 1 | Has new check-in guests (red button) | frmPrijava1.vb:241, 355, 484 |

### 4.20 Guest age categories (from setings)
| Setting | Meaning | Evidence |
|---------|---------|----------|
| dijecagod | Age threshold for child discount | frmPrijavaGostiUnos.vb: auto-discount logic |
| dijecapop | Discount percentage for children below threshold | frmPrijavaGostiUnos.vb: txtPopust.Text = dijecapop |

### 4.21 Automatic age-to-status mapping
| Age Range | Status | Evidence |
|-----------|--------|----------|
| < 12 years | status=4 (child) | frmPrijavaGostiUnos.vb:~895, frmPrijavaGostiKucice.vb:~730 |
| 12-17 years | status=3 (minor) | Same |
| >= 18 years | status=1 (adult) | Same |

---

## 5. Business Rules

### 5.1 Check-in Validations (frmPrijava1.vb:363-386)

| Rule | Condition | Error Message | Line |
|------|-----------|---------------|------|
| Room type required | `cmbTipSobe.SelectedIndex <= 0` | "Izaberite Tip sobe" | 366 |
| Room required | `cmbNazivSobe.SelectedIndex <= 0` | "Izaberite sobu" | 372 |
| Date ordering | `DateDiff(checkIn, maxDate) < -20` | "Datum nove prijave mora biti veci nakon zadnjeg datuma prijave!" | 376 |
| Check-out after check-in | `datePrijava > dateOdjava` | "Datum odjave mora biti nakon datuma prijave!" | 379 |
| At least one guest | `dgvGosti.RowCount = 0` | "Morate dodati imena gostiju!" | 382 |
| At least one NEW guest | `hTable.Count = 0` | "Nema novih gostiju za prijaviti" | 385 |

### 5.2 Duplicate Guest Prevention (frmPrijava1.vb:222-224)

Rule: A guest cannot be checked into the same room twice. `dtPrijavljeniGosti.Select("gostID=" & id)` returns existing rows → error.

### 5.3 Guest Removal Restriction (frmPrijava1.vb:340-345)

Rule: Only newly-added guests (in `hTable`) can be removed from the check-in list. Already-checked-in guests cannot be removed from this form — user must use Check-out (Odjava) instead.

### 5.4 Folio Management (frmPrijava1.vb:394-398)

Rule: If a room already has an open folio (`PID != 0` from `provjeriPID`), reuse it. Otherwise, create a new folio via `addFolio`. This means multiple check-ins to the same room share the same folio.

### 5.5 Per-Person vs Per-Room Billing (frmPrijava1.vb:755-783)

Based on `setings.naplposo`:
- `naplposo = 0`: Split tariff per person — `tariff = totalTariff / numberOfGuests`
- `naplposo = 1`: Full tariff per person — each guest pays the full rate

### 5.6 Minimum Tariff Override (frmPrijava1.vb:783)

If calculated price (`cij`) equals 0, it defaults to `setings.taxa + setings.osig` (tourist tax + insurance). This ensures minimum charges even for zero-rate stays.

### 5.7 Check-out Date Defaults (frmPrijava1.vb:282, 1168-1201)

- Default check-out is tomorrow (`Now.AddDays(1)`)
- If check-out date set before check-in, reset to check-in + 1 day, show warning on second attempt

### 5.8 Date Validation - 20 Day Rule (frmPrijava1.vb:375-377)

Check-in date cannot be more than 20 days before the last recorded night charge for that room. This prevents retroactive check-ins that would conflict with existing charges.

### 5.9 Auto Age Status (frmPrijavaGostiUnos.vb:~895, frmPrijavaGostiKucice.vb:~730)

When a valid birth date is entered:
- Age < 12 → status = 4 (child)
- Age < 18 → status = 3 (minor)
- Age >= 18 → status = 1 (adult)

### 5.10 Auto Child Discount (frmPrijavaGostiUnos.vb:~895)

If `setings.dijecagod > 0` and guest age < `dijecagod`, automatically sets `txtPopust.Text = setings.dijecapop` and reason text to "Osoba ima {age}. godina".

### 5.11 Guest Search Triggers (frmPrijavaGostiUnos.vb)

- `prezime.TextChanged` → triggers `trazi()` (auto-search on surname change)
- `ime.TextChanged` → triggers `trazi()` (auto-search on name change)
- Search limits results to 100 rows (`limit 100`)

### 5.12 Required Fields (marked with * on forms)

From frmPrijavaGostiUnos / frmPrijavaGostiKucice:
- Surname (`prezime`) — validated in code (line ~810: "Niste unijeli prezime!")
- Name (`ime`) — commented out validation exists
- Date of birth (`mtbDatum`) — validated with format mask
- Gender (`pol`) — dropdown, default "Muski"
- Citizenship (`drzavljanstvo`) — dropdown from `drzave` table
- Document type (`cmbDokument`) — dropdown from `gostdokument` table
- Document number (`brDokument`)
- Status (`cmbstatust`) — dropdown from `goststatus` table
- Tariff (`cmbTarifa`) — from `sobatarifa`

### 5.13 Reservation Pre-fill (frmPrijava1.vb:1291-1305)

When a reservation row is clicked, it auto-fills:
- Room type and room name from reservation
- Check-in and check-out dates from reservation
- Tariff from reservation data

### 5.14 Reservation Check-in (frmPrijava1.vb:1118-1141)

Rule: When clicking "Prijava Rezervacije" button:
- Confirms "Sigurni ste da je gost stigao u hotel!"
- Updates `rezervacije.prijava = 1` via direct UPDATE
- Changes row background to yellow

### 5.15 Night Charge Month-Level Upsert (ModuleKod.vb:1081)

The `Unesinocenja` SP deletes all night charges for a `RID` within the same month as the new charge date, then inserts a new one. **This means if check-in generates night charges for the same month twice, the first batch is lost.**

### 5.16 Tariff Price Calculation (frmPrijava1.vb:1203-1214)

`racunajCijenunocenja()` = days × tariff. Days = `DateDiff(checkIn, checkOut)`.

---

## 6. Error Messages and Edge Cases

### 6.1 Error Messages

| Message | Context | File:Line |
|---------|---------|-----------|
| "Izaberite Tip sobe" | No room type selected | frmPrijava1.vb:366 |
| "Izaberite sobu" | No room selected | frmPrijava1.vb:372 |
| "Datum nove prijave mora biti veci nakon zadnjeg datuma prijave!" | Check-in date too old | frmPrijava1.vb:376 |
| "Datum odjave mora biti nakon datuma prijave!" | Invalid date order | frmPrijava1.vb:379 |
| "Morate dodati imena gostiju!" | No guests in grid | frmPrijava1.vb:382 |
| "Nema novih gostiju za prijaviti" | No new guests in hTable | frmPrijava1.vb:385 |
| "Isti gost ne moze biti prijavljen vise puta" | Duplicate guest in room | frmPrijava1.vb:224 |
| "Prvo morate izabrati sobu!" | Adding guest without room | frmPrijava1.vb:261 |
| "Gost je ranije prijavljen te se nemoze obrisati, molimo idite na Odjavu gosta" | Removing already-checked-in guest | frmPrijava1.vb:345 |
| "Neuspjesna prijava!" | General check-in failure | frmPrijava1.vb:458 |
| "Unos uspjesan!" | Check-in success | frmPrijava1.vb:466 |
| "Greska u kodiranju kartice!!! Obavijestite administratora" | Card encoding failure | frmPrijava1.vb:455 |
| "Sigurni ste da je gost stigao u hotel!" | Reservation check-in confirmation | frmPrijava1.vb:1120 |
| "Zelite kodirati karticu?" | Card encoding offer | frmPrijava1.vb:403 |
| "Datum odjave ne moze biti manji od datuma prijave!" | Date change warning | frmPrijava1.vb:1165 |
| "Niste unijeli prezime!" | Missing surname | frmPrijavaGostiUnos.vb:~810 |
| "Prijava uspjesna! Da li zelite da prijavite jos gostiju za izabranu sobu?" | Success + add more | frmPrijavaGostiUnos.vb:~810 |
| "Naziv vec postoji" | Duplicate document type name | frmPrijavaGostiUnos.vb:~855 |
| "Pogresno unesen datum" (tooltip) | Invalid date mask | frmPrijavaGostiUnos.vb, frmPrijavaGostiKucice.vb |
| "Greska u konekciji sa bazom podataka!" | SQL connection error (generic) | Multiple locations |
| "Greska u konekciji sa bazom podataka!-31" through -40 | SQL errors with location codes | frmPrijavaGostiUnos.vb |

### 6.2 Edge Cases and Bugs

| Issue | Description | Evidence |
|-------|-------------|----------|
| **SQL Injection** | Multiple queries use string concatenation with user input: room name (`sobe1z`), guest data (`frmPrijavaGostiUnos.dodaj`), reservation queries | frmPrijava1.vb:299, frmPrijavaGostiUnos.vb:~850-860 |
| **Month-level night charge deletion** | `Unesinocenja` SP deletes ALL night charges for the same RID and month before inserting, potentially losing manual edits | ModuleKod.vb:1081 |
| **No transaction on check-in** | Check-in involves 3+ DB writes (addFolio, addRelGostSoba × N, Unesinocenja × N) without transaction — partial failures leave inconsistent state | frmPrijava1.vb:388-456 |
| **Race condition on folio check** | `provjeriPID` reads PID then potentially creates folio — two concurrent check-ins could create duplicate folios | frmPrijava1.vb:394-398 |
| **br_gost variable never incremented** | `br_gost` is initialized to 0 at line 248 and never changed. The `nocenja` loop `For i = br_gost To dtPrijavljeniGosti.Rows.Count - 1` always starts from 0 | frmPrijava1.vb:248, 765 |
| **Tariff display in combo uses "naziv" column for value** | `ds.Tables("tarifa").Select("naziv=" & ...)` compares decimal tariff value to "naziv" string column — potential type mismatch and SQL injection | frmPrijava1.vb:1156, 1299 |
| **promijeni() error swallowing** | In frmPrijavaGostiUnos.promijeni(), error handlers are commented out — errors silently ignored | frmPrijavaGostiUnos.vb:~915 |
| **Same guest different room** | `dtPrijavljeniGosti.Select("gostID=" & id)` only checks current room's grid — same guest could be checked into another room without warning | frmPrijava1.vb:222 |
| **OOO room not excluded from room name search** | `sobe1z` (line 299) filters `ooo=0` but uses LIKE with string input — non-parameterized | frmPrijava1.vb:299 |
| **Card encoding retry without delay** | Card encoding retries up to 3 times via `If fr.DialogResult = Forms.DialogResult.Abort` checks but no backoff | frmPrijava1.vb:413-420 |
| **Date format used in SQL strings** | `dtpPrijava.Value` formatted as `yyyy-MM-dd` and concatenated directly into SQL | frmPrijava1.vb:1062 |

---

## 7. Cross-Reference

### 7.1 ModuleKod Functions Called

| Function | Called From | Purpose |
|----------|------------|---------|
| `mysqlExScalar(query)` | frmPrijava1.vb:463,1121,1336,1353 | Execute UPDATE/SELECT scalar queries |
| `mysqlReader(query, table)` | frmPrijava1.vb:212, frmPrijavaGostiUnos.vb:~740,~830 | Execute SELECT and return DataTable |
| `provjeriPID(roomID)` | frmPrijava1.vb:393 | Check open folio for room |
| `dodajFolio()` | frmPrijava1.vb:395 | Create new payment folio |
| `unesi(folioID)` | frmPrijava1.vb:399 | Insert guest-room relationships |
| `nocenja(count, roomID, folioID)` | frmPrijava1.vb:400 | Generate night charges |
| `vratiDatum(roomID)` | frmPrijava1.vb:370 | Get max night charge date |
| `vratiRID(guestID, roomID)` | frmPrijava1.vb:766 | Get relgostsoba record ID |
| `racunajCijenunocenja()` | frmPrijava1.vb:1151,1177,1198 | Calculate price = nights × tariff |
| `funkcije.WriteSQLError(ex)` | All forms | Log SQL errors |
| `funkcije.WriteSystemError(ex)` | All forms | Log system errors |
| `funkcije.logs(action, time, id, detail)` | frmPrijava1.vb:677, frmPrijavaGostiUnos.vb:~815 | Audit logging |

### 7.2 Stored Procedures (defined in ModuleKod.bazaProc)

| SP Name | ModuleKod Line | Definition Line | Purpose in Check-in |
|---------|---------------|-----------------|---------------------|
| `addRelGostSoba` | 566 | 7699 (SQL dump) | Insert guest-room check-in record |
| `addFolio` | 852 | 7669 | Create payment folio |
| `Unesinocenja` | 1081 | 8389 | Insert/replace night charge |
| `vratiMaxDatum` | 1116 | 8494 | Get max night charge date |
| `vratiTarifePoVrsta` | 1126 | 8524 | Get tariffs for room type |
| `vratiTrenutnoSlobodne` | 1136 | 8554 | Count free rooms by type |
| `vratiTrenutnoRezervisane` | 1131 | 8539 | Count reserved rooms by type |
| `vratiTrenutnoZauzete` | 1141 | 8569 | Count occupied rooms by type |
| `updateSobaClean` | 1101 | 8449 | Mark room for cleaning |
| `getDokumenti` | 915 | 7759 | List document types |
| `getGostiKucice` | 942 | 7834 | Get single guest data |
| `getPosjete` | 989 | 7999 | Get guest previous visits |
| `promijeniGosti` | 1071 | 8359 | Update guest details |

### 7.3 Global Variables Used

| Variable | Source | Used In | Purpose |
|----------|--------|---------|---------|
| `ConnStr` | ModuleKod | All SQL connections | MySQL connection string |
| `ds` | ModuleKod | frmPrijava1.vb (tarifa, setings, sobe tables) | Global DataSet shared across forms |
| `izborForme` | ModuleKod | frmPrijava1.vb:272, frmPrijavaGostiUnos.vb | Flag: 0=fresh, 1=reservation mode |
| `akcij`, `akcij1`, `akcij2` | ModuleKod | frmPrijava1.vb:286-295 | Room pre-selection context |
| `txt1`-`txt7` | ModuleKod | frmPrijavaBoravkaPodaci.vb → frmReportPrijavaBoravka.vb | Pass form data to Crystal Reports |
| `dozvole(2)` | ModuleKod | frmPrijava1.vb:890 | Permission check for tariff editing |
| `frmGlavni.forma` | ModuleKod | frmPrijava1.vb:236,350,480 | UI state flag for main form |
| `frmGlavni.sbRadnikID` | ModuleKod | frmPrijava1.vb:585 | Current worker ID for audit |
| `frmGlavni.sbpRadnik` | ModuleKod | frmPrijava1.vb:429 | Current worker name |
| `tarif` (module-level) | frmPrijava1.vb:1148 | Tariff calculation | Selected tariff value |
| `hTable` (module-level) | frmPrijava1.vb:15 | Guest tracking | Hashtable: gostID → popust |
| `My.Settings.kardtip` | App config | frmPrijava1.vb:402 | Card reader type (0=none, 1=Salto) |
| `kontr` | ModuleKod | frmPrijava1.vb:425 | Card reader controller instance |
| `setings.naplposo` | ModuleKod (ds table) | frmPrijava1.vb:755 | Per-person billing flag |
| `setings.dijecagod` | ModuleKod (ds table) | frmPrijavaGostiUnos.vb | Child age threshold |
| `setings.dijecapop` | ModuleKod (ds table) | frmPrijavaGostiUnos.vb | Child discount percentage |
| `setings.taxa` | ModuleKod (ds table) | frmPrijava1.vb:783 | Tourist tax amount |
| `setings.osig` | ModuleKod (ds table) | frmPrijava1.vb:783 | Insurance amount |

### 7.4 Form Dependencies

| Form | Opens | Purpose |
|------|-------|---------|
| frmPrijava1 | frmPrijavaGostiUnos | Add/edit guest |
| frmPrijava1 | frmGrupe | Manage reservation groups |
| frmPrijava1 | frmTarife | Edit tariffs |
| frmPrijava1 | frmKardPro | Card encoding |
| frmPrijava1 | frmPrijavaBoravkaPodaci | Residence report data entry |
| frmPrijava1 | frmRezervacije_unos | Edit reservation |
| frmPrijavaGostiUnos | frmDodaj | Add document type or country |
| frmPrijavaGostiUnos | frmDodajdrzave | Add country |
| frmPrijavaGostiUnos | frmTarife | Edit tariffs |
| frmPrijavaGostiKucice | frmSobaInfo | Return to room info (on close) |
| frmPrijavaBoravkaPodaci | frmReportPrijavaBoravka | Show Crystal Report |

---

## 8. Key Findings for Modern System

### 8.1 Critical Business Logic to Preserve

1. **Guest ↔ Room relationship (relgostsoba)**: Central to check-in. Multiple guests per room. Each with tariff, discount, status, and tax.
2. **Folio (posjetafolio)**: One folio per room-stay. Shared across all guests checking into the same room while it's occupied. Must be created/reused correctly.
3. **Night charges (nocenja)**: Generated on check-in for each guest. Subject to same-month upsert rule (DELETE then INSERT). Per-person or per-room billing based on `naplposo` setting.
4. **Age-based status auto-assignment**: Children under 12 → status=4, minors under 18 → status=3, adults → status=1. This drives pricing logic.
5. **Auto child discount**: When `dijecagod` is set, guests below that age threshold automatically get `dijecapop` discount percentage.
6. **Minimum charge rule**: If calculated price is 0, use `taxa + osig` (tourist tax + insurance) as minimum.
7. **Room cleaning flag**: After check-in, room is marked `clean=1` to notify housekeeping.
8. **Reservation check-in**: Two-mode flow — direct check-in or conversion from reservation. Reservations can be marked as checked-in separately.

### 8.2 Architecture Issues to Address

1. **No transaction management**: Check-in is 3-5+ DB operations without BEGIN/COMMIT/ROLLBACK. A failure mid-process leaves partial data.
2. **SQL injection vulnerabilities**: At least 6 locations use string concatenation with user input.
3. **Duplicate folio race condition**: `provjeriPID` and `dodajFolio` are separate calls — concurrent check-ins to same room could create duplicates.
4. **Month-level night charge upsert**: The DELETE+INSERT pattern in `Unesinocenja` can destroy manually edited night charges.
5. **Monolithic form logic**: frmPrijava1.vb is 1368 lines mixing UI, business logic, data access, and card hardware integration.
6. **Global mutable state**: `ds`, `hTable`, `tarif`, `izborForme`, `txt1-txt7` are shared via module-level globals.
7. **No validation on guest form**: Required fields marked with * in UI but only `prezime` is checked in code.
8. **Error swallowing**: `promijeni()` in frmPrijavaGostiUnos has error handlers commented out.

### 8.3 Data Model Summary for Migration

| Table | Key Columns | Check-in Role |
|-------|-------------|---------------|
| `relgostsoba` | gostID, sobaID, checkInDate, checkOutDate, checkInRadnik, odjavljen, rezervacija, grupaID, tarifaID, PID, rezervP, popust, usluga, napomenapl, taksa, status | Guest-room assignment (core check-in record) |
| `posjetafolio` | SID, vrijemeD, vrijemeO, zakljucen | Payment folio per room stay |
| `nocenja` | RID, DatumP, Tarifa, SID, PID, PrijavaOdjava, opis, popust, soba | Night charges |
| `gosti` | ID, prezime, ime, adresa, datumRodjenja, mjestodrzavaR, pol, drzavljanstvo, dokument, brDokument, telefon, mobitel, email, DID | Guest master data |
| `sobe` | ID, naziv, vrsta, idkon, ooo, clean | Room master data |
| `sobavrsta` | ID, naziv, brojKreveta, defaultTarifa | Room type catalog |
| `sobatarifa` | ID, naziv, naziv2, del | Tariff catalog |
| `rezervacije` | ID, GID, checkInDate, checkOutDate, sobavrstaID, brojRezSoba, prijava, stornirana, tarifa | Reservations |
| `rezervacijegrupe` | ID, naziv, odjavljena | Reservation groups |
| `goststatus` | id, naziv, del, taksa | Guest status catalog |
| `drzave` | ID, naziv, domaca, sifra | Country catalog |
| `gostdokument` | ID, naziv | Document type catalog |
| `setings` | naplposo, dijecagod, dijecapop, taxa, osig, kardtip | System configuration |

### 8.4 Recommended Modern Architecture Changes

1. **Wrap check-in in a database transaction**: All operations (addFolio, addRelGostSoba × N, Unesinocenja × N, updateClean, reservation update) should be atomic.
2. **Separate business logic from UI**: Extract `CheckInService` with methods: `validateCheckIn()`, `createFolio()`, `assignGuestToRoom()`, `generateNightCharges()`, `markReservationCheckedIn()`, `flagRoomForCleaning()`.
3. **Use parameterized queries everywhere**: Replace all string concatenation with parameters.
4. **Change night charge upsert to true upsert**: Instead of DELETE-month + INSERT, use UPSERT/ON DUPLICATE KEY UPDATE by (RID, date).
5. **Add optimistic concurrency to folio creation**: Use INSERT with dupe-check instead of SELECT-then-INSERT.
6. **Unify guest status logic**: Create a domain enum/lookup with clear semantics (Adult, Minor, Child) instead of magic numbers 1, 3, 4.
7. **Remove global state**: Pass data via form constructors or service methods, not module-level variables.
8. **Add proper input validation**: Validate all required guest fields server-side, not just surname.
9. **Separate card hardware integration**: Use an interface/adapter pattern for card readers instead of directly calling Salto SDK.