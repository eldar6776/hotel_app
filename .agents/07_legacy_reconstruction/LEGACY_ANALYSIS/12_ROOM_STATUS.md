# Room Status (Sobe) Flow - Legacy Analysis

> Source files analyzed:
> - `legacy_code/frmSobe.vb` (main version, 811 lines)
> - `legacy_code/FormSobe/Krivacuprija/frmSobe.vb` (Krivacuprija variant, 956 lines)
> - `legacy_code/frmSobaInfo.vb` (2436 lines)
> - `legacy_code/frmSobaInfoPromjena.vb` (86 lines)
> - `legacy_code/frmSobaistorija.vb` (104 lines)
> - `legacy_code/frmSobarice.vb` (10 lines, stub)
> - `legacy_code/frmKardSobarica.vb` (18 lines)
> - `legacy_code/frmSobe_Set.vb` (488 lines)
> - Cross-reference: `LEGACY_ANALYSIS/02_MODULEKOD_FUNCTIONS.md`

---

## 1. Room Status System

### 1.1 Status Code Computation: fnSobaStatus (DB Function)

Room status is **not computed in VB.NET code** — it is returned by the database function `fnSobaStatus(SoID, datumP, datumK, tod)` defined at `ModuleKod.vb:1177`. The stored procedure `getSobeShema` (called from `frmSobe.vb:65`, `Krivacuprija/frmSobe.vb:93`) invokes this function to produce a `status` column in the result set.

The function logic (from `02_MODULEKOD_FUNCTIONS.md`, section 4.2):

| Value | Condition | Meaning |
|-------|-----------|---------|
| 0 | No guests, no reservations | SLOBODNA (Free) |
| 1 | Active guests (odjavljen=0, rezervacija=0) | ZAUZETA (Occupied) |
| 2 | Active guests with checkout today or past | ZAUZETA (Occupied, departing) |
| 3 | Confirmed reservation (rezervP=1), no active guests | REZERVISANA - potvrdjeno |
| 4 | Confirmed reservation AND active guests | ZAUZETA i REZERVISANA |
| 5 | OOO flag set (ooo=1 in sobe table) | VAN UPOTREBE |
| 6 | Unconfirmed reservation (rezervP=0), no active guests | REZERVISANA - nepotvrdjeno |

### 1.2 Status Rendering in frmSobe (Dynamic Version)

The main `frmSobe.vb` dynamically creates room buttons in `getSobeShema()` (line 21-382). Status-to-color mapping at lines 250-278:

```vb
status=1 → White/Red (Tag=1)     ZAUZETA
status=2 → White/Orange (Tag=2)  ZAUZETA departing
status=3 → Black/(no BG) (Tag=3)  REZERVISANA potvrdjeno + BackgroundImage=dgm_pozR
status=4 → White/Red (Tag=4)     ZAUZETA i REZERVISANA + BackgroundImage=dgm_pozR
status=5 → White/Black (Tag=5)    VAN UPOTREBE
status=6 → Black/(no BG) (Tag=6)  REZERVISANA nepotvrdjeno + BackgroundImage=dgm_pozR
else  → Black/White (Tag=0)       SLOBODNA
```

Special overlay at lines 280-284:
```vb
clean=False → White/Gray (Tag=7)  NIJE SPREMNA (Not Clean)
```

**Critical**: When `clean=False`, it **overrides** all status colors (even occupied rooms appear gray). The clean flag is checked AFTER the status color is set, effectively overwriting it.

### 1.3 Status Rendering in Krivacuprija Variant

`Krivacuprija/frmSobe.vb` uses a pre-defined static button layout (lines 132-170). Identical status mapping:

```vb
Case 1 → Tag="1", White/Red               (line 138)
Case 2 → Tag="2", White/Orange             (line 142)
Case 3 → Tag="3", Black/GreenYellow        (line 146)
Case 4 → Tag="4", White/Red                (line 149)
Case 5 → Tag="5", White/Black              (line 153)
Case 6 → Tag="6", Black/White              (line 157)
Case Else → Tag="0", Black/Default         (line 160)
```

Clean override at lines 164-168:
```vb
clean=False → White/Gray (Tag=7)
```

**Difference**: Krivacuprija uses GreenYellow for status=3, while main version uses no explicit BackColor (relies on parent/default).

### 1.4 Status Display in frmSobaInfo (Detail View)

`frmSobaInfo.vb:396-431` (also `Krivacuprija/frmSobe.vb:376-420`):

```vb
Tag=0 → lblStanje.Text = "SLOBODNA",          imgKvaka.Visible=True,  imgStop.Visible=False
Tag=1 → lblStanje.Text = "ZAUZETA",            imgKvaka.Visible=False, imgStop.Visible=True
Tag=2 → lblStanje.Text = "ZAUZETA",            imgKvaka.Visible=False, imgStop.Visible=True
Tag=3 → lblStanje.Text = "REZERVISANA - potvrdjeno", imgKvaka=False, imgStop=True
Tag=4 → lblStanje.Text = "ZAUZETA i REZERVISANA",    imgKvaka=False, imgStop=True
Tag=5 → lblStanje.Text = "VAN UPOTREBE",       imgKvaka.Visible=False, imgStop.Visible=True
Tag=6 → lblStanje.Text = "REZERVISANA - nepotvrdjeno", imgKvaka=False, imgStop=True
Tag=7 → lblStanje.Text = "NIJE SPREMNA",       imgKvaka.Visible=False, imgStop.Visible=True
```

### 1.5 Status Transition Triggers

Room status transitions are triggered by these actions:

| Transition | Trigger | Evidence |
|------------|---------|----------|
| Free → Occupied | Check-in (frmPrijava) via `frmGlavni.prijavi()` | `frmSobe.vb:657-659` |
| Occupied → Free | Check-out (frmOdjava) via `frmGlavni.ZatvoriChild()` + frmOdjava1 | `frmSobe.vb:663-669` |
| Any → Out of Order | OOO checkbox + updateOOO() | `frmSobaInfo.vb:140-208` |
| Not Clean → Clean | Clean checkbox + updateClean() | `frmSobaInfo.vb:259-309` |
| Clean → Not Clean | updateClean1() (auto on guest transfer) | `frmSobaInfo.vb:209-258` |
| Any → Reserved | Reservation system (external forms) | Status codes 3, 6 |
| Occupied+Reserved | Check-in on reserved room | Status code 4 |

---

## 2. SQL Inventory

### 2.1 SELECT Operations

| Line | File | SP/SQL | Table(s) | Columns | Condition | Purpose |
|------|------|--------|----------|---------|-----------|---------|
| 65 | frmSobe.vb | `getSobeShema` (SP) | sobe+sobavrsta+kontroler (via fnSobaStatus) | sobaID, sobaNaziv, status, ooo, objekat, sobavrstaNaziv, clean, tekst, idvrsta1 | @datumP, @datumK (today) | Load room grid schema |
| 73 | frmSobe.vb | `SELECT * FROM sobavrsta` | sobavrsta | * | None | Load room types (default view) |
| 76 | frmSobe.vb | `SELECT * FROM sobavrsta1` | sobavrsta1 | * | None | Load room types (alt view) |
| 93 | Krivacuprija/frmSobe.vb | `imedia.getSobeShema` (SP) | Same as above | Same as above | Same | Load room grid schema (SQL Server) |
| 100 | Krivacuprija/frmSobe.vb (commented) | `SELECT * FROM imedia.sobaVrsta` | sobavrsta | * | None | Load room types |
| 38-78 | frmSobaInfo.vb | `getSobaPodaci` (SP) | sobe+related | vrstanaziv, brojKreveta, lokal, ooo, razlog, tekst, idkon | @sobaNaziv | Load room details |
| 496-526 | frmSobaInfo.vb | `getSobeSadrzaji` (SP) | sobe+sadrzaj | (room amenities) | @sobaNaziv | Load room amenities |
| 671 | frmSobaInfo.vb | `UPDATE nocenja ...` (inline) | nocenja | SID, PID | PID=@StariPID AND PrijavaOdjava=0 | Transfer nights on room transfer |
| 773 | frmSobaInfo.vb | `SELECT relgostsoba.ID FROM relgostsoba WHERE relgostsoba.gostID=@GID AND relgostsoba.odjavljen=0 AND relgostsoba.sobaID=@sobaID` | relgostsoba | ID | guest+room | Get relation ID |
| 835 | frmSobaInfo.vb | `SELECT * FROM nocenja where rid={rid} and pid={pid}` | nocenja | * | rid, pid | Load old nights (SQL injection!) |
| 872 | frmSobaInfo.vb | `SELECT * FROM nocenja where sid={sid} and pid={pid}` | nocenja | * | sid, pid | Load new room nights |
| 922 | frmSobaInfo.vb | `SELECT sobe.ID AS sobaID FROM sobe WHERE sobe.naziv='{broj}'` | sobe | ID | naziv (SQL injection!) | Get room ID by name |
| 1285 | frmSobaInfo.vb | `SELECT relgostsoba.ID,relgostsoba.PID FROM relgostsoba INNER JOIN sobe ON relgostsoba.sobaID = sobe.ID WHERE relgostsoba.odjavljen = 0 AND sobe.naziv = '{brojSobe}'` | relgostsoba+sobe | ID, PID | odjavljen=0, room name | Check if room occupied |
| 1328 | frmSobaInfo.vb | `SELECT sobe.ID FROM sobe WHERE sobe.naziv = '{daliPostoji}'` | sobe | ID | naziv (SQL injection!) | Check if room exists |
| 1613 | frmSobaInfo.vb | `SELECT relgostsoba.PID FROM relgostsoba WHERE odjavljen = 0 AND sobaID = {noviBroj}` | relgostsoba | PID | odjavljen=0, room ID | Get folio PID for occupied room |
| 1661 | frmSobaInfo.vb | `SELECT ID FROM posjetaFolio WHERE SID = @SID AND zakljucen = 0` | posjetaFolio | ID | SID, zakljucen=0 | Get open folio for room |
| 1707 | frmSobaInfo.vb | `SELECT ID FROM posjetaFolio WHERE SID = @SID AND zakljucen = 0` | posjetaFolio | ID | SID, zakljucen=0 | Get open folio (current room) |
| 1846 | frmSobaInfo.vb | `SELECT relgostsoba.checkInDate FROM relgostsoba WHERE relgostsoba.gostID = @IDgosta AND odjavljen = 0` | relgostsoba | checkInDate | guest ID | Get check-in date for transfer |
| 22 | frmSobaistorija.vb | `SELECT g.ime, g.prezime, r.checkInDate, r.checkOutDate, r.checkInRadnik, r.checkOutRadnik, r.napomenapl, r.usluga FROM relgostsoba r join gosti g on g.id=r.gostid where r.odjavljen=1 and r.sobaid={Tag}` | relgostsoba+gosti | 8 columns | odjavljen=1, sobaid | Room history (guests) |
| 66 | frmSobaistorija.vb | `SELECT l.vrijeme, l.log, l.kard, l.id, l.idlog, l.logid, l.idkont FROM logcont l where idkont='{idkon}' order by id desc limit 500` | logcont | 6 columns | idkon | Room controller history |
| 730 | frmSobaInfo.vb | `getTarifaSve` (SP) | sobatarifa | sobatarifaNaziv, sobatarifaID | None | Load tariff list |
| 802 | frmSobaInfo.vb | `Unesinocenja` (SP) | nocenja | (INSERT) | RID, DatumPp, Tarifa, SID, PID, Pop, opis, ssoba | Insert night record |
| 1162 | frmSobaInfo.vb | `getTroskoveLista` (SP) | troskovi+related | (expense columns) | @brojSobe | Load expenses for room |

### 2.2 Krivacuprija: ucitajRadnika

| Line | File | SQL | Table(s) | Columns | Condition | Purpose |
|------|------|-----|----------|---------|-----------|---------|
| 431 | Krivacuprija/frmSobe.vb | `SELECT TOP 1 imedia.smjene.smjenaID, imedia.smjene.start, imedia.radnici.ime, imedia.smjene.radnik, imedia.radnici.ID FROM imedia.radnici INNER JOIN imedia.smjene ON imedia.radnici.ID = imedia.smjene.radnik ORDER BY imedia.smjene.start DESC` | radnici+smjene | 5 columns | TOP 1, ORDER BY start DESC | Load current shift worker |

---

## 3. Database Writes

### 3.1 UPDATE Operations

| Line | File | Table | SET | WHERE | SP/Inline | Purpose |
|------|------|-------|-----|-------|------------|---------|
| 180 | frmSobaInfo.vb | sobe | ooo, razlog | naziv=@naziv | `updateSobaOOO` (SP) | Set room out-of-order status |
| 231 | frmSobaInfo.vb | sobe | clean | naziv=@naziv, clean=0 | `updateSobaClean` (SP) | Mark room as not clean |
| 282 | frmSobaInfo.vb | sobe | clean | naziv=@naziv, clean=1 | `updateSobaClean` (SP) | Mark room as clean |
| 969 | frmSobaInfo.vb | relgostsoba | sobaID=@novibroj | gostID=@IDgosta AND odjavljen=0 | Inline SQL | Transfer guest to new room |
| 1017 | frmSobaInfo.vb | relgostsoba | sobaID=@novibroj, PID=@PID | gostID=@IDgosta AND odjavljen=0 | Inline SQL | Transfer guest to occupied room with folio |
| 1070 | frmSobaInfo.vb | troskovi | SID=@IDstari | SID=@novibroj AND zaklj=0 | Inline SQL | Transfer expenses to original room (weird logic) |
| 1507 | frmSobaInfo.vb | posjetaFolio | SID, PID | (via SP `PromjenaFolio`) | SP | Change folio SID on room transfer |
| 1698 | frmSobaInfo.vb | UPDATE relgostsoba | sobaID, PID | gostID AND odjavljen=0 | Inline SQL | Update guest room assignment |
| 1756 | frmSobaInfo.vb | relgostsoba | sobaID=@broj, PID=@PID | gostID=@IDgosta AND odjavljen=0 | Inline SQL | Transfer guest to occupied room |
| 1890 | frmSobaInfo.vb | relgostsoba | sobaID=@broj, PID=@PID | gostID=@IDgosta AND odjavljen=0 | Inline SQL | Same as above (duplicate pattern) |
| 1923 | frmSobaInfo.vb | posjetaFolio | zakljucen=1 | ID=@ID | Inline SQL | Close folio on transfer |
| 2276 | frmSobaInfo.vb | relgostsoba | napomenapl='{text}' | id='{tag}' | Inline SQL (SQL injection!) | Save guest note |
| 2292 | frmSobaInfo.vb | sobe | idkon={tag} | id='{IDbroj}' | Inline SQL (SQL injection!) | Toggle room controller type (key/card) |
| 38 | frmSobaInfoPromjena.vb | (via SP `promijeniDatumVrijeme`) | relgostsoba (presumed) | checkOutDate | DD, DOD, GID | Change guest checkout date/time |

### 3.2 INSERT Operations

| Line | File | Table | Columns | SP/Inline | Purpose |
|------|------|-------|----------|------------|---------|
| 1803 | frmSobaInfo.vb | posjetaFolio | SID, vrijemeD, zakljucen | Inline SQL + SELECT @@Identity | Create new folio for empty room transfer |

### 3.3 Stored Procedure Calls (No Direct SQL Visible)

| SP Name | Called From | Parameters | Purpose |
|---------|------------|------------|---------|
| `getSobeShema` | frmSobe.vb:65, Krivacuprija/frmSobe.vb:93 | @datumP, @datumK | Load room status grid |
| `getSobaPodaci` | frmSobaInfo.vb:38 | @sobaNaziv | Load room details |
| `getSobeSadrzaji` | frmSobaInfo.vb:496 | @sobaNaziv | Load room amenities |
| `updateSobaOOO` | frmSobaInfo.vb:180 | @naziv, @razlog, @ooo | Update OOO status |
| `updateSobaClean` | frmSobaInfo.vb:231, :282 | @naziv, @clean | Update clean status |
| `promijeniDatumVrijeme` | frmSobaInfoPromjena.vb:38 | @DD, @DOD, @GID | Change arrival/departure time |
| `PromjenaFolio` | frmSobaInfo.vb:1507 | @noviSID, @PID | Change folio room assignment |
| `Unesinocenja` | frmSobaInfo.vb:802 | @RID, @DatumPp, @Tarifa, @SID, @PID, @Pop, @opis, @ssoba | Insert night record |
| `unesiPojedinacne` | frmSobaInfo.vb:1243 | @noviSID, @ID, @stariSID | Transfer individual expense |
| `getTroskoveLista` | frmSobaInfo.vb:1162 | @brojSobe | Load room expenses |
| `getTarifaSve` | frmSobaInfo.vb:730 | (none) | Load all tariffs |
| `podaciGosti` | frmSobaInfo.vb:85 (via funkcije.podaciGosti) | @sobaNaziv | Load guest data for room |

---

## 4. Room Status Codes (Complete Table)

| Value | Display Text | BackColor | ForeColor | Tag | Tag Meaning | imgKvaka | imgStop | Set By | Read By |
|-------|-------------|-----------|-----------|-----|-------------|----------|---------|--------|----------|
| 0 | SLOBODNA | White/Default | Black | 0 | Free | Visible | Hidden | fnSobaStatus (no guests/reservations) | frmSobe.vb:250, Kriva:160 |
| 1 | ZAUZETA | Red | White | 1 | Occupied | Hidden | Visible | fnSobaStatus (active guests) | frmSobe.vb:251, Kriva:137 |
| 2 | ZAUZETA | Orange | White | 2 | Occupied departing | Hidden | Visible | fnSobaStatus (checkout today/past) | frmSobe.vb:254, Kriva:141 |
| 3 | REZERVISANA - potvrdjeno | Default/GreenYellow | Black | 3 | Reserved confirmed | Hidden | Visible | fnSobaStatus (rezervP=1, no guests) | frmSobe.vb:258, Kriva:145 |
| 4 | ZAUZETA i REZERVISANA | Red + dgm_pozR BG | White | 4 | Occupied+Reserved | Hidden | Visible | fnSobaStatus (rezervP=1 + guests) | frmSobe.vb:262, Kriva:149 |
| 5 | VAN UPOTREBE | Black | White | 5 | Out of order | Hidden | Visible | fnSobaStatus (ooo=1) or updateOOO() | frmSobe.vb:266, Kriva:154 |
| 6 | REZERVISANA - nepotvrdjeno | Default/White + dgm_pozR BG | Black | 6 | Reserved unconfirmed | Hidden | Visible | fnSobaStatus (rezervP=0, no guests) | frmSobe.vb:270, Kriva:158 |
| 7 | NIJE SPREMNA | Gray | White | 7 | Not clean | Hidden | Visible | clean=0 override | frmSobe.vb:283, Kriva:167 |

### 4.1 Status Priority / Override Rules

1. `fnSobaStatus` computes base status (0-6) from database state
2. **Clean flag override**: If `clean=0` (false), status is forced to 7 (NIJE SPREMNA) regardless of base status — `frmSobe.vb:280-284`, `Krivacuprija/frmSobe.vb:164-168`
3. **OOO flag**: If `ooo=1`, fnSobaStatus returns 5 regardless of other conditions
4. **Object icon**: Building number (objekat column: 1-5) adds icon to button corner — `frmSobe.vb:230-243`
5. **Unpaid expenses overlay**: Rooms with unpaid expenses (`troskoviSVineplaceni`) get `FlatStyle.Standard` and yellow/gray foreground — `frmSobe.vb:336-344`

### 4.2 Status Transitions Matrix

```
                 ┌──────────────┐
                 │   SLOBODNA   │
                 │   (Tag=0)    │
                 └──────┬───────┘
                        │ Check-in
           ┌────────────▼────────────┐
           │       ZAUZETA           │
           │       (Tag=1)           │
           └────┬──────────────┬────┘
                │              │
     Departure  │              │ Day of departure
     date >     │              │ checkout <= today
     today      │              │
                ▼              ▼
          SLOBODNA     ZAUZETA (departing)
          (Tag=0)      (Tag=2)

  Any status ──OOO checkbox──► VAN UPOTREBE (Tag=5)
  Any status ──clean=0──────► NIJE SPREMNA (Tag=7)

  SLOBODNA──Reservation confirmed──► REZERVISANA potvrdjeno (Tag=3)
  SLOBODNA──Reservation unconfirmed─► REZERVISANA nepotvrdjeno (Tag=6)
  REZERVISANA + Check-in──────────► ZAUZETA i REZERVISANA (Tag=4)
```

---

## 5. Business Rules

### 5.1 Availability Rules

1. **Room availability is computed by `fnSobaStatus`** — a MySQL stored function that checks:
   - `sobe.ooo = 1` → returns 5 (out of order)
   - Active guests with `odjavljen=0 AND rezervacija=0` → returns 1 or 2
   - Active guests with confirmed reservation → returns 4
   - Confirmed reservation, no guests → returns 3
   - Unconfirmed reservation, no guests → returns 6
   - Otherwise → returns 0 (free)

2. **Clean flag is separate** — stored in `sobe.clean` column (BIT/BOOLEAN). When `clean=0`, the UI overrides display to Tag=7 (NIJE SPREMNA).

3. **Unpaid expenses indicator** — `funkcije.troskoviSVineplaceni` returns a DataTable of room IDs with unpaid expenses. Rooms with expenses get special formatting on the grid (`frmSobe.vb:336-344`).

### 5.2 Room Transfer Rules (frmSobaInfo.vb)

The `promijenaSobe` method (line 1392) handles room transfers with two modes:

#### Mode 1: "Prebaci sve" (rbt1.Checked) — Transfer all guests

**If target room is occupied (`zauzece=True`):**
- Get target room's PID (`zauzetPID`)
- Calculate tariff: `cmbTarifa.Text / totalGuests`
- For each guest: `samoSobeZauzeto()` + `dodajnocenja()`
- Transfer all existing guests in target room
- `sviGosti()` — update all night records to new SID/PID
- `zakljuciFolio()` — close old folio

**If target room is empty (`zauzece=False`):**
- `samoSobe()` for each guest — change room assignment
- `promjenaPosjetaFolio()` — create/transfer folio
- `sviGosti()` — update night records
- `dodajnocenja()` — add new night records with tariff

**Common to both cases:**
- `samoTroskovi()` — transfer open expenses
- Log: "PSG" (Prebaci Sve Goste)
- `updateOOO()` — update OOO status
- `updateClean1()` — mark old room as not clean
- Close and reopen frmSobe

#### Mode 2: "Prebaci jednog" (rbt2.Checked) — Transfer single guest

- If only 1 guest in room: refuse ("room can't be empty")
- Otherwise: `ama()` method — complex transfer logic with night recalculation
  - If target occupied: `novaSoba()` + `samoGost()` + recalculate nights across both rooms
  - If target empty: `novaSobaFolio()` creates new folio, `samoGost()` + `dodajnocenja()`
- Log: "PGB" (Prebaci Gosta Brzo)

### 5.3 Housekeeping Rules

1. **Clean status is set manually** via `chkClean` checkbox in frmSobaInfo
   - `updateClean()` (line 259): Sets `clean=1` in DB + changes button to white/black
   - `updateClean1()` (line 209): Sets `clean=0` in DB + changes button to gray/white
   - Called after room transfer to mark old room as not clean

2. **OOO (Out of Order) status** via `chkVanUpotrebe` checkbox
   - `updateOOO()` (line 140): Sets `ooo=1/0` and `razlog` in DB
   - Visual: Black/White for OOO, White/Black for back in service
   - When checking OOO, requires confirmation if room was previously in service
   - `chkVanUpotrebe_CheckedChanged` (line 1961): Interactive UI logic

3. **Room clean status interaction** (`chkClean_CheckedChanged`, line 2172):
   - If clean checked AND room is OOO: asks "Are you sure room is back in service?"
   - If Yes: unchecks OOO, sets clean
   - If No: unchecks clean

4. **Card/Key indicator** (`PictureBox1_Click`, line 2284):
   - Toggles `sobe.idkon` between 0 (key icon) and 1 (card icon)
   - Direct SQL update without parameters: `update sobe set idkon={tag} where id='{IDbroj}'` — **SQL injection risk**

### 5.4 Controller Integration (ClassLuxM)

- `frmSobaInfo.getstatus()` (line 337): Reads controller status via `ClassLuxM.citaj_status()`
  - Parses temperature, guest presence, SOS, minibar, window status
  - Displays heating/cooling mode, current/set temperature
  - Supports multiple thermostat zones
- `buttemp()` (line 2328): Sends temperature commands to controller
- `setporuka()` (line 2409): Sends message/language to controller display
- `PicSOS_Click` (line 2384): Resets SOS alarm on controller

---

## 6. Cross-Reference with ModuleKod

### 6.1 Shared Data Structures

| ModuleKod Item | Used In | Purpose |
|----------------|---------|---------|
| `sobe_load()` (line 124) | frmSobe.Form_Load → `sobe_load()` then `getSobeShema()` | Populates `ds.Tables("sobe")` used by frmSobaInfo |
| `ConnStr` (global) | All forms | MySQL connection string |
| `ds` (global DataSet) | frmSobaInfo prominently | Shared dataset across forms |
| `funkcije` class | All forms | Business logic functions |
| `mysqlExScalar()` | frmSobaInfo.vb:2276, 2292 | Direct SQL execution (2 calls, both SQL injection risks) |
| `mysqlReader()` | frmSobaInfo.vb:835, 872 | Load night data for transfers |
| `troskoviSVineplaceni` | frmSobe.vb:44 | Unpaid expenses per room for overlay |
| `fnSobaStatus` (DB function) | `getSobeShema` SP | Computes room status code |
| `provjeri_kard()` | frmSobe.vb timer (commented out) | Card reader status polling |
| `lictex` (global) | frmSobe.vb:598 | License text display |
| `akcij`, `akcij1`, `akcij2` (globals) | frmSobe.vb:657, 694-700 | Navigation action flags |

### 6.2 Stored Procedures Called from Room Forms

| SP Name | Defined In (ModuleKod) | Called From | Line |
|---------|----------------------|------------|------|
| `getSobeShema` | ModuleKod.vb `bazaProc()` ~line 1025 | frmSobe.vb:65, Kriva:93 | Room grid load |
| `getSobaPodaci` | ModuleKod.vb `bazaProc()` ~line 1015 | frmSobaInfo.vb:38 | Room details load |
| `getSobeSadrzaji` | ModuleKod.vb `bazaProc()` ~line 1020 | frmSobaInfo.vb:496 | Room amenities |
| `updateSobaOOO` | ModuleKod.vb `bazaProc()` ~line 1106 | frmSobaInfo.vb:180 | Update OOO status |
| `updateSobaClean` | ModuleKod.vb `bazaProc()` ~line 1101 | frmSobaInfo.vb:231, 282 | Update clean status |
| `promijeniDatumVrijeme` | ModuleKod.vb `bazaProc()` ~line 1066 | frmSobaInfoPromjena.vb:38 | Change checkout time |
| `PromjenaFolio` | ModuleKod.vb `bazaProc()` ~line 1076 | frmSobaInfo.vb:1507 | Transfer folio |
| `Unesinocenja` | ModuleKod.vb `bazaProc()` ~line 1081 | frmSobaInfo.vb:802 | Insert night record |
| `unesiPojedinacne` | ModuleKod.vb `bazaProc()` ~line 1086 | frmSobaInfo.vb:1243 | Transfer individual expense |
| `getTroskoveLista` | ModuleKod.vb `bazaProc()` ~line 1226 | frmSobaInfo.vb:1162 | Load expenses |
| `getTarifaSve` | ModuleKod.vb `bazaProc()` ~line 1035 | frmSobaInfo.vb:730 | Load tariffs |
| `podaciGosti` | ModuleKod.vb `bazaProc()` ~line 1061 | frmSobaInfo.vb:85 | Load guest data |

### 6.3 Key Database Tables Referenced

| Table | Primary Use | Key Columns |
|-------|------------|-------------|
| `sobe` | Core room data | ID, naziv, vrsta, lokal, ooo, razlog, zgradaID, clean, tekst, idvrsta1, sprat, idkon, redulko, ipadres, port, sos, vatr, gost |
| `sobavrsta` | Room type lookup | sobavrstaID, naziv, brojKreveta, defaultTarifa |
| `sobavrsta1` | Alt room type view | (same structure as sobavrsta) |
| `relgostsoba` | Guest-room relationship | ID, gostID, sobaID, PID, checkInDate, checkOutDate, odjavljen, rezervacija, napomenapl, usluga |
| `nocenja` | Night charges | RID, SID, PID, DatumPp, Tarifa, PrijavaOdjava, opis, Pop, ssoba |
| `posjetaFolio` | Visit folio (payment grouping) | ID, SID, vrijemeD, vrijemeO, zakljucen |
| `troskovi` | Room expenses | ID, SID, zaklj, various charge columns |
| `kontroler` | Room controller info | idkon, redulko, ipadres, port |
| `logcont` | Controller log | vrijeme, log, kard, id, idlog, logid, idkont |
| `gosti` | Guest data | id, ime, prezime |
| `setings` | System settings | t3 (controller IP; semicolon-separated IP;port) |

---

## 7. Key Findings for Modern System

### 7.1 Critical Bugs & Security Issues

1. **SQL Injection vulnerabilities** (`frmSobaInfo.vb`):
   - Line 922: `SELECT sobe.ID FROM sobe WHERE sobe.naziv = '{broj}'` — string concatenation with user input
   - Line 1285: `SELECT ... WHERE ... sobe.naziv = '{brojSobe}'` — same pattern
   - Line 1328: `SELECT sobe.ID FROM sobe WHERE sobe.naziv = '{daliPostoji}'` — same
   - Line 1613: `SELECT ... WHERE odjavljen = 0 AND sobaID = {noviBroj}` — numeric but unsanitized
   - Line 835/872: `SELECT * FROM nocenja where rid={rid} and pid={pid}` — direct concatenation
   - Line 2276: `update relgostsoba set napomenapl='{text}' where id='{tag}'` — **critical SQL injection**
   - Line 2292: `update sobe set idkon={tag} where id='{IDbroj}'` — **critical SQL injection**
   - Line 22-66 in frmSobaistorija: two queries with direct string interpolation

2. **No transaction management** on room transfers — each UPDATE/INSERT is a separate call. If mid-transfer fails (e.g., guest moved but nights not transferred), data is left inconsistent.

3. **`updateClean1()` marks room not clean without user consent** — called automatically during room transfer (`promijenaSobe` line 1462), not from a deliberate user action.

### 7.2 Architecture Issues

1. **Two parallel implementations of frmSobe**: Main version (dynamic button creation, MySQL) and Krivacuprija variant (static layout, SQL Server). Different SP names (`getSobeShema` vs `imedia.getSobeShema`), different connection types (`MySqlConnection` vs `SqlConnection`).

2. **Global DataSet (`ds`) shared across forms** — frmSobaInfo heavily modifies the shared dataset (adds/removes tables like "SobeInfo", "gostInfo", "trenutniTroskovi", etc.) creating tight coupling and potential race conditions in MDI environment.

3. **Mixed data access patterns**: Some operations use stored procedures with parameters, others use inline SQL with string concatenation. No consistent approach.

4. **Status computed in database** (`fnSobaStatus`) but display logic in client — any change to fnSobaStatus requires coordinated DB + client deployment.

5. **frmSobarice** is an empty stub (10 lines) — the housekeeping management form was never implemented.

6. **frmKardSobarica** (18 lines) creates `frmKardPro` with `tip=2` (or `tip=6` if checkbox) — minimal form, likely for employee card encoding.

### 7.3 Business Logic Observations

1. **Room transfer is extremely complex** — the `promijenaSobe()` method at `frmSobaInfo.vb:1392-1491` combines folio management, night charge recalculation, expense transfer, and guest reassignment in a single synchronous operation with no rollback capability.

2. **Night recalculation on transfer** — When transferring guests, existing night records (`nocenja`) are deleted and recreated with new tariff amounts. The tariff is divided by total guest count, which could cause rounding errors.

3. **Room controller (Salto/LuxM) integration** — The `getstatus()` method at line 337 parses a binary string from the room controller for: temperature, guest presence, SOS, minibar, window, fan status. Supports multiple thermostat zones. This is tightly coupled to the `ClassLuxM` hardware controller class.

4. **Card system integration** — Multiple card-related features exist but are mostly commented out. Active code uses `frmKardPro` with different `tip` values:
   - `tip=1`: Card encoding (occupied room)
   - `tip=2`: Housekeeper card (from frmKardSobarica)
   - `tip=3`: Card cancellation
   - `tip=4`: Card read
   - `tip=5`: Visitor card (20-minute validity)
   - `tip=6`: Housekeeper card variant

5. **Room history via frmSobaistorija**: Two views:
   - Guest history (RadioButton1): `relgostsoba` with `odjavljen=1`
   - Controller log (RadioButton2): `logcont` by `idkont`

6. **Settings persistence for room grid layout** (`frmSobe_Set.vb`): Stores button dimensions (width, height, spacing) per room type (up to 7 types, indices 0-6) in `My.Settings`. This allows customization of the visual layout without code changes.

7. **Unpaid expense overlay** (`frmSobe.vb:336-344`): Uses `funkcije.troskoviSVineplaceni` to check room IDs with unpaid expenses and changes button style to `FlatStyle.Standard` with yellow/gray text — but doesn't override the status color, creating visual ambiguity.

### 7.4 Data Model Summary for Modern System

```
sobe (rooms)
├── id, naziv (room number), vrsta (type FK)
├── lokal (phone extension), ooo (out of order), razlog (OOO reason)
├── clean (BIT), tekst (tooltip text), zgradaID (building)
├── idkon (controller ID, 0=key, 1=card), redulko, ipadres, port
├── sos, vatr, gost (controller status flags)
└── idvrsta1 (alt type classification)

relgostsoba (guest-room assignments)
├── ID, gostID, sobaID, PID
├── checkInDate, checkOutDate, checkInRadnik, checkOutRadnik
├── odjavljen (checked out flag), rezervacija (reservation flag)
├── napomenapl (note), usluga (service type)
└── pl (payment flag)

nocenja (night charges)
├── RID (relgostsoba ID), SID (room ID), PID (folio ID)
├── Datump, Tarifa, PrijavaOdjava
├── opis, Pop (discount), ssoba (room name)
└── (created/managed via Unesinocenja SP)

posjetaFolio (visit folio - payment grouping)
├── ID, SID (room ID), vrijemeD, vrijemeO
└── zakljucen (0=open, 1=closed)

troskovi (room expenses)
├── SID (room ID), zaklj (0=open)
└── (various charge columns)

sobavrsta (room types)
├── sobavrstaID, naziv (type name)
└── brojKreveta, defaultTarifa

kontroler (room controllers)
└── (hardware integration data)
```

### 7.5 Recommended Modern Architecture Changes

1. **Replace fnSobaStatus with domain logic** — move status computation from SQL function to a service layer that can be tested, versioned, and changed without DB migration.

2. **Unified room transfer service** — replace the monolithic `promijenaSobe()` with a transactional service that handles: guest reassignment, folio creation/transfer, night recalculation, expense transfer, clean status update — all in a single DB transaction.

3. **Eliminate global dataset** — replace shared `ds` with per-form ViewModels or DTOs.

4. **Parameterize all SQL** — replace all string-concatenated queries with parameterized commands.

5. **Separate status from display** — create a clean status enum/value object separate from color/font rendering logic.

6. **Implement proper housekeeping module** — `frmSobarice` is a stub; the clean flag is managed via a checkbox on the detail form rather than a dedicated workflow.

7. **Decouple controller integration** — create an abstraction layer for hardware controllers (Salto, LuxM) rather than direct binary string parsing in the UI form.