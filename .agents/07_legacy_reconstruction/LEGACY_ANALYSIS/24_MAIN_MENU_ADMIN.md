# Deep Analysis: frmGlavni.vb (Main Menu) + frmBaza.vb (Database Admin)

> Sources:
> - `legacy_code/frmGlavni.vb` (1848 lines)
> - `legacy_code/frmBaza.vb` (1541 lines)

---

## 1. Main Menu / Navigation (frmGlavni.vb)

### 1.1 Toolbar Buttons and Navigation

The main form uses a `ToolBar` control with the following buttons (defined in `InitializeComponent`, lines 113-630):

| Index | Button Name | Text | Opens Form | Global State |
|-------|-------------|------|------------|--------------|
| 0 | `dgmPrijava` | "Prijava" | `frmPrijava1` (MDI child) | Checks `forma=1` — warns if guests not checked in |
| 2 | `dgmOdjava` | "Odjava" | `frmOdjava1` (MDI child) | Closes all children first |
| 4 | `dgmPlacanje` | "Placanje" | `frmPlacanje` (MDI child) | Dropdown menu with Placanje/Predracun/Troskovi/Tarife/Racuni |
| 6 | `dgmGosti` | "Gosti" | `frmGosti` (MDI child) | Closes all children first |
| 8 | `dgmSobe` | "Hotel" | `frmSobe` (MDI child) | Closes all children first |
| 10 | `dgmRezervacije` | "Rezervacije" | `frmRezervacije` (MDI child) | Closes all children first |
| 12 | `dgmIzvjestaji` | "Izvjestaji" | `frmIzvjestaji` (MDI child) | Dropdown: Dnevni izvjestaj, Fiskalni izvjestaji, Export |
| 14 | `dgmTelefon` | "Telefon" | `frmTelefon` (MDI child) | Dropdown: Telefonski Imenik |
| 16 | `dgmZurnal` | "Žurnal" | `frmZurnal1` (MDI child) | Closes all children first |
| 18 | `dgmSticky` | "Napomene" | `frmNapomene` (non-MDI) | Opens as regular window |
| 20 | `dgmLogin` | "Logiranje" | `frmLogin` (dialog) | Confirms shift change first |

**Key:** `ZatvoriChild()` closes all MDI children before opening a new form (line 634-638).

### 1.2 Placanje Dropdown Menu (`meniTroskovi`)

| Menu Item | Click Handler | Opens |
|-----------|-------------|-------|
| "Placanje" (mnPlacanje) | `mnPlacanje_Click` | (not in frmGlavni — presumably opens frmPlacanje directly) |
| "Predračun" (MenuItem2) | `MenuItem2_Click` line 1334 | `frmPredracun` (MDI child) |
| "Troskovi" (mnTroskovi) | `mnTroskovi_Click` line 1141 | `frmTroskovi` (MDI child) |
| "Tarife" (mnTarife) | `mnTarife_Click` line 1147 | `frmTarife` (MDI child) |
| "Računi" (mnRacuni) | `mnRacuni_Click` line 1153 | `frmRacuni` (MDI child) |

### 1.3 Izvjestaji Dropdown Menu (`meniIzvjestaji`)

| Menu Item | Handler |
|-----------|---------|
| "Dnevni izvjestaj" (mnDnevniIzvjestaj) | Opens `frmIzvjestajiDnevni` |
| "Fiskalni izvjestaji" (MenuItem3) | Opens `frmFiskal` |
| "Export" (MenuItem4) | Opens `frmExport` |
| "Backup" (MenuItem5) | MySQL dump backup |

### 1.4 Menu Strip Items

| Menu | Text | Handler |
|------|------|---------|
| IZLAZToolStripMenuItem | "IZLAZ" | Confirms exit, closes form |
| INFOToolStripMenuItem | "iNFO" | Dropdown → "Info program" → `SplashScreen1` |
| PostavkeToolStripMenuItem | "Postavke" | Opens `frmpostavke` (if level 5 or dozvole(0)="1"), sets pk=1 |
| UpdateToolStripMenuItem | "Update" | (no handler) |
| OProgramuToolStripMenuItem | "O programu" | Opens `SplashScreen1` |
| UpdtToolStripMenuItem | "Program" | Dropdown with sub-items |
| — SobaricaKarticaToolStripMenuItem | "Sobarica kartica" | Opens `frmKardSobarica` (requires dozvole(5)="1") |
| — PomocToolStripMenuItem | "Pomoc" | Opens `frmRezervacije1` (MDI child) |
| — RezervacijeToolStripMenuItem | "rezervacije" | Opens `frmRezervacijeNove` (dialog) |
| — ToolStripMenuItem2 | "Licenca" | Opens `frmlic` (dialog) |
| — CitajpisiKarticuToolStripMenuItem | "citaj-pisi karticu" | Opens `frmKardRw` (requires dozvole(5)="1") |
| — ToolStripMenuItem3 | (empty) | (no handler) |
| IToolStripMenuItem | "_" | (spacer) |
| PoslovnaGodinaToolStripMenuItem | "Poslovna Godina" | Dropdown with year list |
| cmbgodine | (ToolStripComboBox) | Year selector — switches database connection |

### 1.5 Status Bar Panels

| Panel | Name | Text/Usage |
|-------|------|------------|
| 0 | sbpApp | "Hotelsko Poslovanje - Hotel PRO" |
| 1 | sbpRadnik | "Radnik" — displays worker name |
| 2 | sbpStart | "Start" — startup time |
| 3 | sbRadnikID | "ID" — worker ID |
| 4 | sbNivo | "Nivo" — access level (used for admin checks, e.g. level 9) |
| 5 | salto | "Salto nije konektovano" — card lock system status |
| 6 | ver | "Ver:" — version info |

---

## 2. Timer Events and Background Processes

### 2.1 Timer1 (line 593-594)

`Interval = 1000` (1 second), `Enabled = True` on form creation.

**Timer1_Tick handler** (lines 1221-1245):

| Tick Action | Condition | What Happens |
|-------------|-----------|--------------|
| License check | `lic > 100 And lic < 260` | Calls `provjerilic()` |
| Clock display | Always | `lblVrijeme.Text = Now` |
| Night reset | `Hour=1 And Minute=1 And Second<2` | Calls `provrezstare()` |
| Salto status | Always | `salto.Text = kardst` |
| Controller response | `kontod <> ""` | `ver.Text = kontod`, then clears `kontod` |
| Alarm check | `Minute=55 And Second>57` | Calls `alarmChk()` |
| Alarm trigger | `Second=1` | Calls `provjeri_alarm()` |
| Auto-update | `upd=0` | Calls `updat()` then sets `upd=1` |
| Card check | Every 5 or 10 seconds | Commented out: `provjeri_kard()` |

### 2.2 FileSystemWatcher (`filevoch`)

**Created in `putfiskal()`** (lines 1099-1114): Monitors fiscal device directory for response files.

**FileSystemWatcher1_Changed** (lines 1571-1687): Handles HCP/NSC (fsc(0)=2) fiscal device responses:
- Reads response file content
- If contains "56,1," → checks for "Ok" + location tag → extracts receipt number
- Calls `snimFiskal()` to save fiscal data to database
- If contains "350," → extracts timestamp for fiscal device time
- If contains "301," or "39," or "53," or "56," → error handling, deletes file

**FileSystemWatcher1_Created** (lines 1690-1808): Handles Mikroelektronika (fsc(0)=7) fiscal device responses:
- If `.OK` file → sends receipt state query, writes back receipt number
- If `Prodaja_*` file → reads XML response, checks for "Successful operation.", clears fiscal queue
- If `Storno_*` file → reads storno XML response
- If `register_state.xml` → sets `txt9=562`
- If `bill_state.xml` → reads XML for receipt number, calls `snimFiskal()`

### 2.3 License Check (`provjerilic`, lines 1191-1218)

- Reads `setings.keyk` field
- Decrypts date from positions 7-9, 5-7, 11-13 with offsets +3, +4, +5
- Compares with current date
- If expired: `lic = 300`, message "Ogranicen pristup!"
- If expiring within 3 days: warning message

### 2.4 Alarm System

- `alarmChk()` (line 1290): Spawns thread for `provjerialarm()`
- `provjeri_alarm()` (lines 1247-1265): Checks `dtalarm` rows, if `Now > vrijeme`, shows `frmAlshow` with alarm details and beeps

### 2.5 Auto-Update (`updat`, lines 1268-1289)

- Checks for `Cftp.exe`
- Writes version info to `cft.txt`
- Launches `Cftp.exe` as separate process

---

## 3. Database Administration (frmBaza.vb)

### 3.1 Form Overview

frmBaza is a general-purpose database administration tool supporting both **MySQL** and **MS SQL Server** connections (toggled via `CheckBox2`). It is NOT part of the normal application flow — it is a standalone utility.

**Key UI elements:**

| Control | Purpose |
|---------|---------|
| `txtserver` | Server address |
| `userid` | Database username (defaults to "root") |
| `password` | Database password |
| `databaseList` | Dropdown populated with databases |
| `tables` | Dropdown populated with tables |
| `dataGrid` | Displays selected table data |
| `connectBtn` | Connect to database |
| `updateBtn` | Save changes to data |
| `Button1` ("run") | Execute arbitrary SQL |
| `Button2` ("kopiraj naziv tabele") | Append table name to TextBox1 |
| `Button3` ("polja u tabeli") | Show table columns/structure |
| `CheckBox2` | Toggle MySQL/SQL Server mode |
| `cmbpri` | SQL example templates (26 items) |
| `txt` | SQL example text display |
| `Button4` ("BrisiRac") | Delete invoice by number (calls brisi0-5) |
| `Button5` ("Nova baza") | Create seed data in selected database |
| `Button6` ("snimi pocetne") | Export seed tables to XML |
| `Button7` ("Snimi tabelu u xml") | Export selected table to XML |
| `Button8` ("Snimi BAZU") | Backup (commented out) |
| `Button9` ("Snim sve tabele") | Export ALL tables to XML |
| `Button10` ("Import podataka iz dataset") | Import data from XML file |
| `Button11` ("truncate table") | Truncate all tables |
| `Button12` ("Import rac") | Import invoice records between databases |
| `NumericUpDown1/2/3` | Old/new invoice number range for import |
| `TextBox4/5` | Old/new database names for import |

### 3.2 Admin Operations

#### Connect (`connectBtn_Click`, line 541)
- If CheckBox2 (SQL Server): Uses `SqlConnection` + `sp_databases`
- If not checked (MySQL): Uses `MySqlConnection` + `SHOW DATABASES`
- Populates `databaseList` dropdown

#### Load Tables (`databaseList_SelectedIndexChanged`, line 624)
- SQL Server: `SELECT * FROM SYSOBJECTS WHERE TYPE='U' ORDER BY NAME`
- MySQL: `SHOW TABLES`
- Populates `tables` dropdown

#### Load Table Data (`tables_SelectedIndexChanged`, line 671)
- `SELECT * FROM {table}` — loads full table data into DataGrid
- Auto-generates INSERT/UPDATE/DELETE commands via CommandBuilder

#### Update Data (`updateBtn_Click`, line 703)
- `DataTable.GetChanges()` → `DataAdapter.Update()` — saves edits back to database

#### Run SQL (`Button1_Click`, line 718)
- Executes arbitrary SQL from TextBox1
- Supports both MySQL and SQL Server

#### Show Table Structure (`Button3_Click`, line 759)
- SQL Server: `SELECT table_catalog, table_schema, table_name, column_name, data_type, character_maximum_length FROM information_schema.columns WHERE table_name='{tables.Text}'`
- MySQL: `DESCRIBE {table}`

### 3.3 Invoice Delete Operations (Button4_Click, line 865)

When `NumericUpDown3.Value > 0` and user confirms:

| Procedure | SQL | Table |
|-----------|-----|-------|
| `brisi(br)` | `DELETE placanje WHERE broj={br}` | placanje |
| `brisi1(br)` | `DELETE placanjeDetalji WHERE brojID={br}` | placanjeDetalji |
| `brisi2(br)` | `DELETE placanjeSlozeno WHERE RBR={br}` | placanjeSlozeno |
| `brisi3(br)` | `DELETE printracuni WHERE brojRacuna={br}` | printracuni |
| `brisi4(br)` | `DELETE printracunidetalji WHERE brojRacuna={br}` | printracunidetalji |
| `brisi5(br)` | `DELETE printracunifooter WHERE brojRacuna={br}` | printracunifooter |

**WARNING**: This is a destructive hard-delete without transaction, backup, or storno flag. No verification of related records (troskovi, nocenja, etc.).

### 3.4 Seed Data Creation (Button5_Click, line 1016)

Creates initial dataset from XML files and inserts into selected database:
1. `drzave.xml` → `drzave` table (columns: naziv, domaca, sifra)
2. `gostDokument.xml` → `gostDokument` table (column: naziv)
3. `kursna.xml` → `kursna` table (columns: Naziv_Valute, Vrijednost)
4. `troskovivrste.xml` → `troskovivrste` table (columns: ID, naziv)
5. `placanjenacin.xml` → `placanjenacin` table (columns: ID, nacin)

Uses `provjer()` function to check existence before inserting each row.

### 3.5 Export Tables to XML (Button6_Click, line 1145)

Exports 5 seed tables to XML:
- `SELECT * FROM drzave` → `drzave.xml`
- `SELECT * FROM gostDokument` → `gostDokument.xml`
- `SELECT * FROM kursna` → `kursna.xml`
- `SELECT * FROM troskovivrste` → `troskovivrste.xml`
- `SELECT * FROM placanjenacin` → `placanjenacin.xml`

### 3.6 Export Single Table to XML (Button7_Click, line 1197)

Uses `FolderBrowserDialog` to select destination, then:
- `SELECT * FROM {table}` → writes DataSet XML with schema to `{table}.xml`

### 3.7 Export All Tables to XML (Button9_Click, line 1271)

Iterates all tables in `tables` dropdown:
- `SELECT * FROM {table}` → adds to DataSet
- Writes complete `svetabele.xml` with schema

### 3.8 Import from XML (Button10_Click, line 1317)

Reads XML file → iterates tables → generates INSERT statements dynamically:
- Handles data type conversion (True→1, False→0, numeric dot/comma, date format)
- No transaction — each row inserted individually with Try/Catch
- Shows error message per row with full SQL statement

### 3.9 Cross-Database Invoice Import (Button12_Click, line 1440)

Imports invoice records from one database to another using `NumericUpDown1/2` for old/new invoice numbers and `TextBox4/5` for old/new database names:

| Procedure | Source → Target | Tables |
|-----------|----------------|--------|
| `import(brStari, brNovi, baza)` | `{source}.placanje` → `{target}.placanje` | All columns, replaces `broj` with brNovi |
| `import1(brStari, brNovi, baza)` | `{source}.placanjeDetalji` → `{target}.placanjeDetalji` | All columns, replaces `brojID` with brNovi |
| `import2(brStari, brNovi, baza)` | `{source}.placanjeSlozeno` → `{target}.placanjeSlozeno` | All columns, replaces `RBR` with brNovi |
| `import3(brStari, brNovi, baza)` | `{source}.printracuni` → `{target}.printracuni` | All columns, replaces `BrojRacuna` with brNovi |
| `import4(brStari, brNovi, baza)` | `{source}.printracunidetalji` → `{target}.printracunidetalji` | All columns, replaces `BrojRacuna` with brNovi |
| `import5(brStari, brNovi, baza)` | `{source}.printracunifooter` → `{target}.printracunifooter` | All columns, replaces `BrojRacuna` with brNovi |

Each uses a connection string without database name: `server={server};UID={UIDd};password={passwo}; character set=utf8;`

### 3.10 Cross-Database Schema Copy (Label11_DoubleClick, line 1601)

When double-clicking "Stara - nova baza" label:
1. `SHOW TABLES FROM {old_database}` → get all table names
2. For each table:
   - Try: `SELECT * FROM {new_database}.{table}` — if fails (table doesn't exist)
   - Then: `CREATE TABLE {new_database}.{table} SELECT * FROM {old_database}.{table}` — copy structure + data
   - Then: `ALTER TABLE {new_database}.{table} MODIFY COLUMN id INT(10) NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (id)`
3. Errors appended to `txt` text box

### 3.11 Reset Invoices SQL Example (cmbpri item 26)

The SQL example `cmbpri` item "26. resete racune" shows:
```sql
DELIMITER $$
DROP PROCEDURE IF EXISTS `resetbr` $$
CREATE PROCEDURE `resetbr` ()
BEGIN()
  TRUNCATE TABLE placanje;
  TRUNCATE TABLE placanjedetalji;
  TRUNCATE TABLE placanjeslozeno;
  TRUNCATE TABLE predracuni;
  TRUNCATE TABLE predracunidet;
  TRUNCATE TABLE printracspec;
  TRUNCATE TABLE printracuni;
  TRUNCATE TABLE printracuniavans;
  TRUNCATE TABLE printracunidetalji;
  TRUNCATE TABLE printracunifooter;
END $$
DELIMITER ;
```

### 3.12 Backup (Button8_Click, line 1241)

Commented out — originally planned for SQL Server backup via `Smo.Backup` and `MySqlBackup.ExportToFile`. Currently does nothing.

---

## 4. SQL Inventory (frmGlavni.vb + frmBaza.vb)

### 4.1 frmGlavni.vb SQL Statements

| Line | Operation | Table(s) | Purpose |
|------|-----------|----------|---------|
| 751 | SELECT | `godine` | Load business year names |
| 884 | SELECT | `@@global.time_zone, @@session.time_zone` | Check MySQL timezone |
| 1087 | SELECT | `sobe LEFT JOIN sobavrsta LEFT JOIN kontroler` | Load room data with types and controllers |
| 1090 | SELECT | `radnici` | Load worker data |
| 1092 | SELECT | `kontroler` | Load controller data |
| 1390 | SELECT | `placanje_` | Check if year-archive tables exist |
| 1398-1424 | RENAME | `placanje → placanje_`, `placanjeDetalji → placanjeDetalji_`, etc. (12 tables) | Archive current year tables by adding underscore suffix |
| 1448-1475 | DROP + RENAME | Drop current tables, rename `_` tables back | Restore archived tables (year change rollback) |
| 1525-1551 | SELECT INTO | `placanje FROM p2010placanje`, etc. (13 tables) | Create new year tables from template |
| 1860 | SELECT | `setings` (many columns) WHERE `stan={stanica}` ORDER BY id DESC LIMIT 1 | Load all system settings |
| 1814 | UPDATE | `printracuni` SET `fisrac`, `fisvr`, `fisIZN` WHERE `BrojRacuna={id}` | Save fiscal response |
| 1824 | UPDATE | `printracunifooter` SET `nap=concat(nap, ' fiscal text')` WHERE `BrojRacuna={id}` | Append fiscal legal note |

### 4.2 frmBaza.vb SQL Statements (Non-Standard)

| Line | Operation | Table(s) | Purpose |
|------|-----------|----------|---------|
| 546 | Connection | — | SQL Server connection (no DB specification) |
| 586 | EXEC | `sp_databases` | SQL Server: list databases |
| 604 | SELECT | `SHOW DATABASES` | MySQL: list databases |
| 633 | SELECT | `SYSOBJECTS WHERE TYPE='U'` | SQL Server: list user tables |
| 652 | SHOW | `SHOW TABLES` | MySQL: list tables |
| 676/690 | SELECT | `* FROM {table}` | Load table data |
| 764 | SELECT | `information_schema.columns WHERE table_name='{name}'` | SQL Server: describe table |
| 775 | DESCRIBE | `{table}` | MySQL: describe table |
| 889 | DELETE | `placanje WHERE broj={br}` | Hard-delete invoice payment |
| 912 | DELETE | `placanjeDetalji WHERE brojID={br}` | Hard-delete invoice payment details |
| 934 | DELETE | `placanjeSlozeno WHERE RBR={br}` | Hard-delete complex payment |
| 956 | DELETE | `printracuni WHERE brojRacuna={br}` | Hard-delete invoice header |
| 978 | DELETE | `printracunidetalji WHERE brojRacuna={br}` | Hard-delete invoice details |
| 1000 | DELETE | `printracunifooter WHERE brojRacuna={br}` | Hard-delete invoice footer |
| 1040 | INSERT | `drzave (naziv, domaca, sifra)` | Seed country data from XML |
| 1054 | INSERT | `gostDokument (naziv)` | Seed document type data from XML |
| 1067 | INSERT | `kursna (Naziv_Valute, Vrijednost)` | Seed exchange rate data from XML |
| 1080 | INSERT | `troskovivrste (ID, naziv)` | Seed expense type data from XML |
| 1093 | INSERT | `placanjenacin (ID, nacin)` | Seed payment method data from XML |
| 1121 | SELECT | `{column} FROM {table} WHERE {column}='{text}'` | Check existence before insert |
| 1156 | SELECT | `* FROM drzave` | Export countries |
| 1165 | SELECT | `* FROM gostDokument` | Export document types |
| 1170 | SELECT | `* FROM kursna` | Export exchange rates |
| 1175 | SELECT | `* FROM troskovivrste` | Export expense types |
| 1180 | SELECT | `* FROM placanjenacin` | Export payment methods |
| 1391 | INSERT | Dynamic row-by-row insert from XML | Import data (with type conversion) |
| 1464 | INSERT INTO SELECT | `{target}.placanje` FROM `{source}.placanje WHERE broj={brStari}` | Cross-DB invoice import |
| 1489 | INSERT INTO SELECT | `{target}.placanjeDetalji` FROM `{source}.placanjeDetalji WHERE brojID={brStari}` | Cross-DB detail import |
| 1512 | INSERT INTO SELECT | `{target}.placanjeSlozeno` FROM `{source}.placanjeSlozeno WHERE RBR={brStari}` | Cross-DB complex payment import |
| 1535 | INSERT INTO SELECT | `{target}.printracuni` FROM `{source}.printracuni WHERE brojRacuna={brStari}` | Cross-DB header import |
| 1558 | INSERT INTO SELECT | `{target}.printracunidetalji` FROM `{source}.printracunidetalji WHERE brojRacuna={brStari}` | Cross-DB detail import |
| 1581 | INSERT INTO SELECT | `{target}.printracunifooter` FROM `{source}.printracunifooter WHERE brojRacuna={brStari}` | Cross-DB footer import |
| 1612 | SHOW | `SHOW TABLES FROM {old_db}` | Get all tables in old database |
| 1634 | SELECT | `* FROM {new_db}.{table}` | Check if table exists in new DB |
| 1647 | CREATE TABLE | `{new_db}.{table} SELECT * FROM {old_db}.{table}` | Copy table structure + data |
| 1661 | ALTER TABLE | `MODIFY COLUMN id INT(10) NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (id)` | Add auto-increment PK |

---

## 5. Global State Management

### 5.1 Global Variables in frmGlavni.vb

| Variable | Type | Purpose | Set In | Read In |
|----------|------|---------|--------|----------|
| `forma` | Integer | Flag: 1 = guests not checked in | `Alati_ButtonClick` (checks, doesn't set) | Line 642: warns if 1 |
| `wn` | Integer | (initialized to 0) | — | — |
| `sbNivo` | StatusBarPanel | Access level (text = "9" for admin) | `frmLogin` (external) | frmRacuni line 475/1004/2285: show unpaid section |
| `sbpRadnik` | StatusBarPanel | Worker name | `frmLogin` | Display only |
| `sbRadnikID` | StatusBarPanel | Worker ID | `frmLogin` | Display only |
| `sbpStart` | StatusBarPanel | Start time | — | Display only |
| `salto` | StatusBarPanel | Card lock system status | Timer (from `kardst` variable) | Display only |
| `ver` | StatusBarPanel | Controller response text | Timer (from `kontod` variable) | Display only |
| `verB` | String | Version string | `frmGlavni_Load`: "3.3" | Display in status bar |
| `fsc` | String() | Fiscal device config array from `setings.fiscal` split on "*" | `frmGlavni_Load` | frmRacuni printfisc logic |
| `ds` | DataSet | Global dataset (setings, sobe, rad, kontr, firme) | `loadset()`, `settingsp()` | Many forms |
| `filevoch` | FileSystemWatcher | Fiscal response file watcher | `putfiskal()` | Timer and fiscal callbacks |
| ` ConnStr` | String (module-level) | Database connection string | `frmGlavni_Load`, `PROMJENAGOD`, `cmbgodine_SelectedIndexChanged` | All database operations |
| `Settings.god` | Integer | Current business year | `PROMJENAGOD`, `punigod` | Year filtering |
| `Settings.godbaza` | String | Database base name | `PROMJENAGOD` | Database connection |
| `My.Settings.update` | Integer | Update flag (0=needs update, 1=done) | `frmGlavni_Load` | Auto-update check |
| `My.Settings.PrikaziEur` | Integer | Show EUR column flag | frmRacuni | Invoice display |
| `My.Settings.gostisvi` | Integer | Show all guests flag | frmRacuni | Invoice display |
| `pk` | Integer | Settings flag (set to 1 when opening settings) | `PostavkeToolStripMenuItem_Click` | — |
| `prmgod` | Integer | Primary year flag (set to 1 after load) | `frmGlavni_Load` | — |
| `upd` | Byte | Update check flag (0=pending, 1=done) | `updat()` | Timer |
| `lic` | Integer | License day counter | `provjerilic()`, Timer | License enforcement |
| `lictex` | String | License warning text | `provjerilic()` | Display |
| `kardst` | String | Card system status | (external) | Status bar |
| `kontod` | String | Controller response | (external) | Status bar → ver |
| `txt9` | Integer | (used in fiscal response, line 1758) | FileSystemWatcher | — |
| `filvocbr` | String | Prevents duplicate fiscal response processing | FileSystemWatcher1_Changed | Dedup check |
| `dozvole` | Array | Permission flags (0=settings, 5=cards, 6=invoice edit) | `frmLogin` | frmRacuni, frmGlavni permission checks |
| `stanica` | Integer | Station number | Settings | Settings query filter |

### 5.2 Stored Settings from `setings` table (line 1860)

Loaded via: `SELECT verbaz, verpr, keyk, vrijU, t1, t2, t3, id, izmjver, rad, minchi, maxcho, pribora, pdv, pdvo, pdvtax, pdvtr, osig, taxa, cultur, sobegrupa, sobekuc, dijecagod, dijecapop, fiscal, valuta, racunbr, napomena, naplposo, cijt, lokac, decim FROM setings WHERE stan={stanica} ORDER BY id DESC LIMIT 1`

| Column | Usage |
|--------|-------|
| `verbaz` | Database version check |
| `verpr` | Program version check |
| `keyk` | License key (decrypted in `provjerilic`) |
| `vrijU` | (unknown, reserved) |
| `t1, t2, t3` | (unknown, reserved) |
| `id` | Settings record ID |
| `izmjver` | Version change flag |
| `rad` | Worker name |
| `minchi` | Minimum charge threshold |
| `maxcho` | Maximum checkout hour |
| `pribora` | Board/meal count |
| `pdv` | PDV rate (used for fiscal: 0=zero rate, 17=standard) |
| `pdvo` | PDV rate for some items |
| `pdvtax` | PDV rate for tourist tax |
| `pdvtr` | PDV rate for transit |
| `osig` | Insurance amount per night |
| `taxa` | Tourist tax amount per night |
| `cultur` | Culture/locale code |
| `sobegrupa` | Room grouping |
| `sobekuc` | Room house/building |
| `dijecagod` | Children age threshold |
| `dijecapop` | Children discount percentage |
| `fiscal` | Fiscal device config (pipe-separated "*") |
| `valuta` | Currency code |
| `racunbr` | Invoice numbering |
| `napomena` | Default note |
| `naplposo` | POS location |
| `cijt` | Price type flag |
| `lokac` | Location code (for fiscal receipt reference) |
| `decim` | Decimal places |

---

## 6. Cross-Reference: P0/P1 Flows Triggered from Menu Items

### 6.1 P0 Flows (Core Business)

| Flow | Trigger | Menu/Toolbar Item | Form Opened | Evidence |
|------|---------|-------------------|-------------|----------|
| P0-01: Check-in | `dgmPrijava` button (index 0) | "Prijava" toolbar | `frmPrijava1` | frmGlavni.vb:651-721 |
| P0-02: Checkout | `dgmOdjava` button (index 2) | "Odjava" toolbar | `frmOdjava1` | frmGlavni.vb:653-659 |
| P0-03: Payment/Billing | `dgmPlacanje` dropdown → mnPlacanje | "Placanje" menu | `frmPlacanje` | frmGlavni.vb:661-665 |
| P0-04: Guest Management | `dgmGosti` button (index 6) | "Gosti" toolbar | `frmGosti` | frmGlavni.vb:666-671 |
| P0-05: Room Management | `dgmSobe` button (index 8) | "Hotel" toolbar | `frmSobe` | frmGlavni.vb:672-677 |
| P0-06: Reservations | `dgmRezervacije` button (index 10) | "Rezervacije" toolbar | `frmRezervacije` | frmGlavni.vb:678-683 |
| P0-07: Invoice List | mnRacuni menu item | "Računi" menu | `frmRacuni` | frmGlavni.vb:1153-1157 |
| P0-08: Expenses | mnTroskovi menu item | "Troskovi" menu | `frmTroskovi` | frmGlavni.vb:1141-1145 |
| P0-09: Proforma | MenuItem2 menu item | "Predračun" menu | `frmPredracun` | frmGlavni.vb:1334-1340 |
| P0-10: Invoice Print/View | frmRacuni button | In frmRacuni | `rptRacunFrm` | frmRacuni.vb:397 |
| P0-11: Invoice Storno | frmRacuni Button3 | In frmRacuni | — | frmRacuni.vb:653 |
| P0-12: Fiscal Print | frmRacuni double-click col 13 | In frmRacuni | — | frmRacuni.vb:571-590 |

### 6.2 P1 Flows (Administrative)

| Flow | Trigger | Menu/Toolbar Item | Form/Action | Evidence |
|------|---------|-------------------|-------------|----------|
| P1-01: Daily Reports | mnDnevniIzvjestaj | "Dnevni izvjestaj" menu | `frmIzvjestajiDnevni` | frmGlavni.vb:1165-1168 |
| P1-02: Fiscal Reports | MenuItem3 | "Fiskalni izvjestaji" menu | `frmFiskal` | frmGlavni.vb:1565-1569 |
| P1-03: Data Export | MenuItem4 | "Export" menu | `frmExport` | frmGlavni.vb:1962-1965 |
| P1-04: Database Backup | MenuItem5 | "Backup" menu | MySQL dump process | frmGlavni.vb:1967-2014 |
| P1-05: Settings | PostavkeToolStripMenuItem | "Postavke" menu | `frmpostavke` (if level 5) | frmGlavni.vb:1295-1304 |
| P1-06: System Settings | `settingsp()` | Loaded at startup | Loads `setings` table into `ds` | frmGlavni.vb:1853-1897 |
| P1-07: License Check | Timer + `provjerilic()` | Automatic (every ~100 seconds) | Warning message | frmGlavni.vb:1191-1218 |
| P1-08: Year Change | `PROMJENAGOD()` + `cmbgodine_SelectedIndexChanged` | "Poslovna Godina" menu | Creates new year DB | frmGlavni.vb:902-951 |
| P1-09: Card System | `CitajpisiKarticuToolStripMenuItem` | "citaj-pisi karticu" | `frmKardRw` | frmGlavni.vb:1937-1943 |
| P1-10: New Reservations | `RezervacijeToolStripMenuItem` | "rezervacije" menu | `frmRezervacijeNove` | frmGlavni.vb:1932-1934 |
| P1-11: Notes | `dgmSticky` button (index 18) | "Napomene" toolbar | `frmNapomene` | frmGlavni.vb:703-705 |
| P1-12: Shift Login | `dgmLogin` button (index 20) | "Logiranje" toolbar | `frmLogin` (dialog) | frmGlavni.vb:707-710 |
| P1-13: Phone Book | `dgmTelefon` → MenuItem1 | "Telefonski Imenik" | `frmtelefonskiimenik` | frmGlavni.vb:1170-1172 |
| P1-14: Tariffs | mnTarife menu item | "Tarife" menu | `frmTarife` | frmGlavni.vb:1147-1151 |
| P1-15: Housekeeper Cards | `SobaricaKarticaToolStripMenuItem` | "Sobarica kartica" | `frmKardSobarica` | frmGlavni.vb:1952-1959 |
| P1-16: DB Admin | `frmBazaPas` (shown on startup if no settings) | Auto | frmBazaPas (settings password) | frmGlavni.vb:774-808 |
| P1-17: Salary Card Read/Write | `CitajpisiKarticuToolStripMenuItem` | Requires `dozvole(5)="1"` | `frmKardRw` | frmGlavni.vb:1937-1943 |

### 6.3 Year Change (Business Year) Flow Detail

**PROMJENAGOD()** (lines 902-951):

1. Check if schema for `servDB + Now.Year` exists (`checkchema`)
2. If exists, set `Settings.god = Now.Year` and return
3. If not, ask user to reset invoice numbering
4. If previous year schema doesn't exist:
   - Create it with `crecshem(servDB & Now.AddYears(-1).Year)`
   - Create tables with `createTable()`
   - Create views/procs with `viewcr()` and `bazaProc()`
   - Import initial data with `inportgod(1)`
5. Create current year schema (`crecshem(servDB & Now.Year)`)
6. Update ConnStr to current year
7. Create tables, views, procs for current year
8. Import initial data with `inportgod(0)`
9. Update `setings.verpr` with new database name

**Year change involves renaming 13 tables** (lines 1398-1425):
`placanje`, `placanjeDetalji`, `predracuni`, `predracunidet`, `placanjeSlozeno`, `printracspec`, `printracuni`, `printracuniavans`, `printracunidetalji`, `printracunidetaljiavans`, `printracunifooter`, `neplaceni`, `neplaceniplacanja`, `Avans`

All renamed with `_` suffix (backup), then new empty copies created from `p2010*` templates.

---

## Key Findings for Modern System

1. **frmBaza is a standalone DBA tool**, not integrated into the hotel workflow. It should be separated into an admin utility, not embedded in the application.

2. **Hard invoice deletion** (brisi/brisi1-5) bypasses all business logic — no storno flag, no transaction, no cleanup of related records (troskovi, nocenja). This is extremely dangerous.

3. **Cross-database import** (Button12/import functions) uses raw connection strings without database specification in `ConnStr`, then relies on `INSERT INTO {db}.table SELECT * FROM {src}.table` syntax — MySQL-specific, no parameterization.

4. **Year change logic** renames tables with `sp_rename` (SQL Server syntax) but the application uses MySQL — this is likely a porting issue. The `godineizmj()` function should use `RENAME TABLE` for MySQL.

5. **frmGlavni loads everything at startup**: settings, rooms, workers, controllers into global `ds` DataSet. This makes the form slow to load and creates tight coupling.

6. **Fiscal file-watcher pattern** (FileSystemWatcher) is fragile — file operations can be missed under high load. The Mikroelektronika integration handles both CREATED and CHANGED events separately.

7. **No proper separation of concerns**: frmGlavni contains database connection management, fiscal integration, license checking, year management, and settings loading — all in one 1848-line form.