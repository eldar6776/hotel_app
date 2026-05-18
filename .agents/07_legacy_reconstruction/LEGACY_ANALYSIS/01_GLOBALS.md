# Legacy Application: Global Variables, Configuration & Infrastructure Analysis

## 1. Global Variables

### Module `Data` (Data.vb)

| Name | Type | Default Value | File:Line | Business Meaning |
|------|------|--------------|-----------|-----------------|
| `dst` | DataSet | New DataSet | Data.vb:2 | Working dataset for invoice/billing calculation (pripremaRacuna) |
| `servDB` | String | "" | Data.vb:4 | Primary database name, loaded from `C:\Program Files\IMEDIA\HotelPro\set.xml` |
| `server` | String | "" | Data.vb:5 | MySQL server hostname, loaded from set.xml |
| `UIDd` | String | "" | Data.vb:6 | MySQL user ID, loaded from set.xml |
| `passwo` | String | "" | Data.vb:7 | MySQL password (obfuscated with `%&rt!h23` suffix in storage), loaded from set.xml |
| `port` | String | "3306" | Data.vb:8 | MySQL port number, loaded from set.xml |
| `ConnStr` | String | concatenated | Data.vb:9 | **Primary MySQL connection string**: `server={server};database={servDB}{year};UID={UIDd};password={passwo};port={port}; character set=utf8;` — year appended in `punigod()` |
| `ConnStrKasa` | String | concatenated | Data.vb:10 | **Secondary (POS/cash register) MySQL connection string** — separate server/db/user/pass/port from set.xml |
| `ds` | DataSet | New DataSet() | Data.vb:11 | **Global shared dataset** — central in-memory data store used across all forms for tables: sobe, setings, zagl, status, firme, nplac, ime, tros, uplate, noc, pror, GostiLista, etc. |
| `PlacanjeSlozRBR` | Integer | 0 (implicit) | Data.vb:12 | Running counter for compound payment receipt line numbers |
| `selekcija` | Integer | 0 (implicit) | Data.vb:13 | Selection state variable (usage unclear from analyzed files) |
| `Brojnocenja1` | Integer | 0 (implicit) | Data.vb:14 | Night count variable 1 (used in daily report calculations) |
| `Brojnocenja2` | Integer | 0 (implicit) | Data.vb:15 | Night count variable 2 (used in daily report calculations) |
| `txt1`..`txt9` | String | "" (implicit) | Data.vb:16 | Generic string variables used as inter-form communication fields (txt1 used for card/room selection, txt9 for numeric IDs like 562) |
| `akcij` | String | "" (implicit) | Data.vb:17 | Inter-form action string — carries commands between forms (e.g., "trosakP", "placanje", "Placanje") |
| `akcij1` | String | "" (implicit) | Data.vb:18 | Inter-form action parameter 1 (e.g., room number, action type like "provjeriF" or "pla" or "p") |
| `akcij2` | String | "" (implicit) | Data.vb:19 | Inter-form action parameter 2 (e.g., record ID, "storno" flag) |
| `RID` | Integer | 0 (implicit) | Data.vb:20 | **Current logged-in worker/employee ID** — set at login (`frmLogin.vb:145`) |
| `RIme` | String | "" (implicit) | Data.vb:21 | **Current logged-in worker name** — set at login (`frmLogin.vb:146`), also declared as `Shared` in `funkcije.vb:6` |
| `digi` | Double | 0 (implicit) | Data.vb:22 | Partner/company ID numeric value passed between partner selection and payment forms |
| `stanica` | Integer | 0 | Data.vb:23 | **Station/workstation number** identifying the POS terminal, loaded from set.xml |
| `stanicaK` | Integer | 0 | Data.vb:24 | **POS cash register station number** (secondary station for kasa subsystem), loaded from set.xml |
| `pime` | String | "" (implicit) | Data.vb:25 | Worker name used in printed receipt headers |
| `printajNocenje` | Boolean | False (implicit) | Data.vb:589 | Flag controlling whether to print accommodation/nights report |
| `izborForme` | Boolean | False (implicit) | Data.vb:591 | Flag indicating form selection mode (used in guest registration flow) |
| `previewRacuna` | Boolean | False (implicit) | Data.vb:592 | Flag controlling invoice/receipt preview mode |
| `imeRadnika` | String | `frmLogin.txtIme.Text` | Data.vb:593 | Worker name (duplicate of RIme, initialized from login form) |
| `objOdjava` | Object | Nothing (implicit) | Data.vb:594 | Stores toolbar button sender object for checkout action |
| `eOdjava` | ToolBarButtonClickEventArgs | Nothing (implicit) | Data.vb:595 | Stores toolbar button event args for checkout action |

### Module `ModuleKod` (ModuleKod.vb)

| Name | Type | Default Value | File:Line | Business Meaning |
|------|------|--------------|-----------|-----------------|
| `lictex` | String | "" (implicit) | ModuleKod.vb:3 | License text |
| `lic` | Integer | 0 | ModuleKod.vb:4 | License status/level |
| `verB` | String | "1.1" | ModuleKod.vb:5 | Database version identifier |
| `verP` | String | "1.1" | ModuleKod.vb:6 | Program version identifier |
| `kardTable` | DataTable | New DataTable (10 cols) | ModuleKod.vb:7 | Card controller data — stores card reader events (id, loc, ulaz, izlaz, log, odg, kard, sat, tip) |
| `kardst` | String | "Salto nije konektovan" | ModuleKod.vb:8 | Card controller connection status string |
| `kontr` | kard_imedia1 | New instance | ModuleKod.vb:9 | Card controller (door lock) interface object |
| `kontod` | String | "" | ModuleKod.vb:10 | Card controller ID/code |
| `dozvole` | String | "" (implicit) | ModuleKod.vb:12 | Permissions/access rights string for the logged-in user |
| `dsk` | DataSet | New DataSet (in Sub New) | ModuleKod.vb:13-16 | Secondary dataset for kasa (POS) subsystem |
| `fisnsc` | Integer | 0 | ModuleKod.vb:14 | Fiscal device type (NSC) identifier |
| `pk` | Byte | 0 | ModuleKod.vb:52 | Card reader poll lock flag |
| `dtalarm` | DataTable | New DataTable | ModuleKod.vb:410 | Alarm/reminder data table |

### Class `funkcije` (funkcije.vb)

| Name | Type | Default Value | File:Line | Business Meaning |
|------|------|--------------|-----------|-----------------|
| `funkcije.RID` (Shared) | Integer | 0 (implicit) | funkcije.vb:5 | Worker ID (duplicate/shadow of Data.RID) |
| `funkcije.RIme` (Shared) | String | "" (implicit) | funkcije.vb:6 | Worker name (duplicate/shadow of Data.RIme) |

---

## 2. Connection Strings

### Primary Connection String (`ConnStr`)

**Source**: `Data.vb:9` — initially assembled from empty globals, then overwritten at runtime.

**Runtime construction**: `Data.vb:114` — `punigod()` method:
```
ConnStr = "server=" & server & ";database=" & servDB & frmGlavni.cmbgodine.Text & ";UID=" & UIDd & ";password=" & passwo & ";port= " & port & "; character set=utf8; "
```

Key behavior: The **current year** (from `frmGlavni.cmbgodine` combo box) is **appended to the database name**. This enables multi-year database schemas (e.g., `iMEDIAHotel2025`, `iMEDIAHotel2026`).

**Config file path**: `C:\Program Files\IMEDIA\HotelPro\set.xml` (`Data.vb:33-34`)

**set.xml fields read** (`Data.vb:36-63`):
| Field | Obfuscation | Target Global | Meaning |
|-------|-------------|-------------|---------|
| `serv` | Trim last 2 chars | `server` | MySQL server address |
| `user` | Trim last 2 chars | `UIDd` | MySQL username |
| `pass` | Strip `%&rt!h23` suffix, Trim last 3 chars | `passwo` | MySQL password |
| `port` | Trim last 2 chars | `port` | MySQL port |
| `baza` | Trim last 2 chars | `servDB` | Database name base (year appended at runtime) |
| `stan` | (none) | `stanica` | Primary station/workstation ID |

### Secondary (Cash Register) Connection String (`ConnStrKasa`)

**Source**: `Data.vb:63` — assembled from separate set.xml fields:

```
ConnStrKasa = "server=" & srv1 & ";database=" & baza1 & ";UID=" & user1 & ";password=" & pass1 & ";port=" & port1 & "; character set=utf8;"
```

**set.xml cash register fields** (`Data.vb:52-62`):
| Field | Obfuscation | Target | Meaning |
|-------|-------------|--------|---------|
| `serv1` | Trim last 2 chars | srv1 (local) | Cash register MySQL server |
| `user1` | Trim last 2 chars | user1 (local) | Cash register MySQL user |
| `pass1` | Strip `%&rt!h23`, Trim last 3 chars | pass1 (local) | Cash register MySQL password |
| `port1` | Trim last 2 chars | port1 (local) | Cash register MySQL port |
| `baza1` | Trim last 2 chars | baza1 (local) | Cash register database name |
| `stan1` | (none) | `stanicaK` | Cash register station number |

### Hardcoded Legacy Connection (commented out)

`Data.vb:3` (comment): `user id=imedia;password=mm22in;database=iMEDIAHotel;server=P4` — original SQL Server connection string, now commented out.

`konfiguracija.vb` (entire file commented out): Same pattern with `UIDd`, `servDB`, `server` variable interpolation. Also references `My.Settings.sqlessp` for SQL Express attach mode.

### Direct Connection constructions in ModuleKod.vb

`ModuleKod.vb:683-684` — A connection without specifying a database is built for schema operations:
```
"server=" & server & ";UID=" & UIDd & ";password=" & passwo & ";port=" & port & "; character set=utf8;"
```

`ModuleKod.vb:704-705` — Similar (without port):
```
"server=" & server & ";UID=" & UIDd & ";password=" & passwo & "; character set=utf8;"
```

### Connection String in app.config

`app.config:8`: `<add name="DefaultConnection" connectionString="Data Source = |SQL/CE|" />` — Placeholder/default, not used for MySQL connections at runtime.

### Password Obfuscation

`Data.vb:40,56`: Passwords stored in set.xml have `%&rt!h23` appended as a simple obfuscation suffix, which is stripped during read. Additionally, both passwords and all other fields have 2-3 trailing characters trimmed (likely padding to prevent casual file reading).

---

## 3. Configuration Settings

### app.config User Settings (Radna.My.MySettings)

| Key | Type | Default | Business Meaning |
|-----|------|---------|-----------------|
| `bpe` | Integer | 4915275 | Unknown numeric code (possibly baud rate or hardware ID for card reader) |
| `bim` | Integer | 589833 | Unknown numeric code (possibly hardware ID) |
| `vis0`..`vis6` | Integer | 40 (each) | Column/row heights for 7 DataGridView columns (0-6) |
| `sir0`..`sir6` | Integer | 47 (each) | Column widths for 7 DataGridView columns (0-6) |
| `poz0`..`poz6` | Boolean | True (each) | Column visibility flags for 7 DataGridView columns (0-6) |
| `si` | String | "0" | Sort/index setting |
| `razs0`..`razs6` | Integer | 50 (each) | Column spacing/row separation for 7 columns (0-6) |
| `ipAdres` | String | "127.0.0.1" | **Card controller IP address** (used in `ModuleKod.vb:11`: `Dim ipadr As String = My.Settings.ipAdres`) |
| `portK` | Integer | 5010 | **Card controller TCP port** for door lock hardware |
| `dodatnav` | String | "" | Additional navigation/path setting |
| `predKard` | String | "Soba " | **Card prefix** — printed before room number on key cards |
| `CardContr` | Boolean | False | **Card controller enabled flag** — whether door lock hardware is active |
| `PrikaziEur` | Integer | 0 | **Show EUR currency** display flag (0=local currency only) |
| `update` | Byte | 0 | Update check flag |
| `ch1`..`ch3` | Byte | 0 (each) | Channel/check settings 1-3 |
| `co0`..`co2` | Integer | 100 (each) | Column order/offset settings 0-2 |
| `ch4`, `ch5` | Byte | 0 (each) | Channel/check settings 4-5 |
| `god` | Integer | 0 | **Year override** — when non-zero, forces a specific year database |
| `godbaza` | String | "" | **Year database name** for data import between year schemas (`My.Settings.godbaza` used in `ModuleKod.vb:174,185`) |
| `kardtip` | Integer | 0 | **Card type** — key card encoding format |
| `poruka` | Integer | 1 | Message/notification display flag |
| `rezc` | Integer | 0 | Reservation color setting |
| `rezr` | Integer | 0 | Reservation row setting |
| `gostisvi` | Integer | 0 | Show all guests flag |

### app.config AppSettings

| Key | Value | Meaning |
|-----|-------|---------|
| `dgmPrijava.ImageIndex` | -1 | Check-in DataGridView image index (no image) |
| `ClientSettingsProvider.ServiceUri` | "" | Empty — no remote settings provider |

### Database Settings Table (`setings`)

The `setings` table (queried via `Data.vb:78`, `ModuleKod.vb:131-152`, `frmGlavni.vb:1860`) contains operational configuration rows with columns referenced in code:

| Column | Referenced In | Business Meaning |
|--------|--------------|-----------------|
| `stan` | Data.vb:50,78 | Station/workstation number |
| `sobekuc` | frmpostavke.vb:569, frmGosti.vb:1283 | Room grouping key (apartment codes delimited by #) |
| `keyk` | frmpostavke.vb:569 | License/encryption key |
| `pdv`, `pdvtr` | frmpostavke.vb:569 | VAT (PDV) rate, VAT on tourism |
| `osig` | frmpostavke.vb:569 | Insurance rate |
| `taxa` | frmpostavke.vb:569 | Tourist tax |
| `cultur` | frmpostavke.vb:569 | Culture/locale setting |
| `dijecagod`, `dijecapop` | frmpostavke.vb:569 | Children age limit, children discount |
| `naplposo` | frmpostavke.vb:569 | POS payment mode |
| `cijt` | frmpostavke.vb:569 | Price type |
| `pdvo`, `pdvtax` | frmpostavke.vb:569 | VAT on services, VAT on tax |
| `fiscal` | frmLogin.vb:183, ModuleKod.vb:2851-3103 | **Fiscal device config** (format: `type*path1*path2*path3*...` where type: 1=Thermal, 2=NSC, 3=Tring, 4=TermoL, 6=Mikroelektronika, 7=HCP) |
| `valuta` | frmpostavke.vb:569 | Currency name |
| `racunbr` | frmpostavke.vb:569 | Receipt/invoice number format |
| `lokac` | frmpostavke.vb:569 | Location code |
| `minchi` | Data.vb:419, frmpostavke.vb:569, frmGlavni.vb:1860 | **Minimum check-in hour** (used in night calculation) |
| `maxcho` | Data.vb:340,425, frmpostavke.vb:569, frmGlavni.vb:1860 | **Maximum check-out hour** (used in night calculation, compared against checkout time) |
| `t1`, `t2`, `t3` | frmpostavke.vb:569, frmGlavni.vb:1860 | Additional threshold/config values |
| `verbaz`, `verpr` | frmGlavni.vb:1860 | Database version, program version |
| `izmjver` | frmpostavke.vb:569 | Version change timestamp |
| `rad` | frmpostavke.vb:569, frmGlavni.vb:1860 | Worker name who last modified settings |
| `pribora` | frmGlavni.vb:1860 | Number of bed linens |
| `sobegrupa` | UNKNOWN | Room group setting |
| `napomena` | frmpostavke.vb:569 | Note on settings |
| `decim` | UNKNOWN from analyzed files | Decimal precision setting |

---

## 4. Data Access Patterns

### Pattern 1: Direct MySqlConnection + MySqlCommand (Primary Pattern)

Used in ~95% of all database operations. Each method creates its own connection, opens, executes, closes.

```vb
Dim konekcija As New MySqlConnection(ConnStr)
Dim komanda As New MySqlCommand(sql, konekcija)
konekcija.Open()
' ExecuteReader, ExecuteNonQuery, ExecuteScalar
konekcija.Close()
```

**Evidence**: Data.vb:75-89, 121-139, 354-414, 476-498, 506-540, 599-621; funkcije.vb:132, 250, 379, 408, 443, 498, 559, 581, 637, 662; ModuleKod.vb:156, 298, 339, etc.

### Pattern 2: DataAdapter Fill (Secondary Pattern)

Used in `funkcije.vb` for stored procedure calls with output parameters:

```vb
Dim objDataAdapter As MySqlDataAdapter = New MySqlDataAdapter(command)
objDataAdapter.Fill(dataset, tableName)
```

**Evidence**: funkcije.vb:192-207, 310, 394-403

### Pattern 3: clasMysqlAdapt Helper (Light ORM)

A simple data access wrapper class in `clasMysqlAdapt.vb`:
- `getdata(textDb)` — Fill DataTable from SQL text
- `snimi()` — Save changes via `MySqlCommandBuilder`
- `CANCELcHANGES()` — Reject changes
- `start()` — Open connection using global `ConnStr`

**Evidence**: clasMysqlAdapt.vb:1-64

### Pattern 4: Module-Level Utility Functions (ModuleKod.vb)

Centralized query helpers in `ModuleKod`:
- `mysqlReader(query, tableName)` → DataTable (`ModuleKod.vb:2610`) — **Most widely used** query function
- `mysqlReaderK(query, tableName)` → DataTable (`ModuleKod.vb:2555`) — Uses `ConnStrKasa` (cash register DB)
- `mysqlExScalar(query)` → Boolean (`ModuleKod.vb:2592`) — Execute scalar, returns success
- `mysqlExScalarLast(query)` → Integer (`ModuleKod.vb:2573`) — Execute + return last insert ID
- `mysqlExScalarK(query)` → String (`ModuleKod.vb:2536`) — Execute scalar on cash register DB
- `mysqlProcedure(query, tableName)` → DataTable (`ModuleKod.vb:2628`) — For stored procedures

### Pattern 5: Transaction-Based Operations

Only used for critical multi-step operations like checkout (`OdjavaSobe` in `Data.vb:142-225`):

```vb
konekcija.Open()
sqlTrans = konekcija.BeginTransaction()
komanda.Transaction = sqlTrans
' Multiple ExecuteNonQuery calls
sqlTrans.Commit() / sqlTrans.Rollback()
```

### Pattern 6: Global DataSet (`ds`)

The `ds` module-level DataSet is the **central in-memory data cache**. Tables loaded into it include:
- `sobe` (rooms), `setings` (configuration), `zagl` (header info), `status` (guest status)
- `nplac` (payment methods), `firme` (partners/companies), `GostiLista`, `GostiPosjete`
- `IzvjestajNaplata`, `DnevniPlacanje`, `NocenjeSTAT`, `NocenjeSUM`
- `PlacanjaSlozena`, `avans`, `avansd`, `pred` (pre-invoices)
- `printSredina`, `printFooter`, `Informacije`
- And many more

**Evidence**: ModuleKod.vb:126-129 (sobe_load), 131-152 (loadsetg), Data.vb:506-540 (ucitajNacine), 598-621 (citajfirme)

### Stored Procedures Called

| Procedure | File:Line | Purpose |
|-----------|-----------|---------|
| `getPlacanjenocenja` | Data.vb:443 | Get paid night totals by folio PID |
| `getJedinicnaCijena` | funkcije.vb:181, 299 | Get unit price, tax, insurance for a rate |
| `podaciGostiSobe` | funkcije.vb:391 | Get guest data by room |
| `addSmjenaStart` | funkcije.vb:421 | Record worker shift start |
| `getLogs` | funkcije.vb:467 | Insert log entry |
| `getGlavniImena` | funkcije.vb:560 | Get main names (room display) |
| `getGlavniPodaci` | funkcije.vb:581 | Get main data by RID |
| `vratiRIDnocenja` | funkcije.vb:638 | Get night registration IDs by room |
| `Unesinocenja` | ModuleKod.vb:1081 | Insert night registrations (defined inline) |
| `vratiRIDNocenja` | ModuleKod.vb:1121 | Get RID night registration (defined inline) |
| `Cijenanocenja` | Data.vb:356 (commented SQL) | Calculate night price (migrated to VB) |

---

## 5. Utility Functions (funkcije.vb)

| Function | Parameters | Return Type | Purpose | File:Line |
|----------|-----------|-------------|---------|-----------|
| `Poredjenje` | (A As Object, B As Object) | Boolean | Compare two values handling DBNull | funkcije.vb:10-14 |
| `SelectDistinct` | (dataSet, imeNoveTabele, sourceTabela, distinctPolje, sortPolje, polja(), filter, prviRed(), zadnjiRed()) | DataTable | Create distinct/unique rows from a source table with filtering | funkcije.vb:15-45 |
| `WriteSQLError` | (e As Object) | Object | Log MySQL exception to Windows Event Log under source "iMEDIA Hotel" | funkcije.vb:87-111 |
| `WriteSystemError` | (e As SystemException) | Object | Log System exception to Windows Event Log under source "iMEDIA Hotel" | funkcije.vb:113-126 |
| `getRadnik` | (none) | Object | Returns worker ID — currently hardcoded to `1` | funkcije.vb:127-129 |
| `izracunajCijenu` | (dataset, tabela, ID, datePrijava, dateOdjava) | Sub | Calculate accommodation unit price via SP `getJedinicnaCijena`, populate dataset table "cijene" with price, tax, amounts | funkcije.vb:130-247 |
| `izracunajCijenuOdjava` | (dataset, tabela, ID, brojDana) | Sub | Calculate checkout price via SP `getJedinicnaCijena` with day count, include tourist tax & insurance | funkcije.vb:248-374 |
| `podaciGosti` | (dz As DataSet, sobaID As String) | DataTable | Get guest info for a room via SP `podaciGostiSobe`, fills "Informacije" table in passed dataset | funkcije.vb:375-405 |
| `prijaviRadnika` | (radnikID As Integer) | Sub | Record worker shift start via SP `addSmjenaStart` | funkcije.vb:406-438 |
| `logs` | (dugme As String, vrijeme As Date, opis As String, opis1 As String) | Sub | Write audit log entry via SP `getLogs`, includes worker name/ID from frmGlavni | funkcije.vb:441-493 |
| `noviFolio1` | (SobaID As Integer) | Integer | Insert new folio record in `posjetaFolio` table, return identity | funkcije.vb:496-527 |
| `ValidacijaDatuma` | (datOD As DateTime, datDO As DateTime) | Boolean | Validate date range — returns True if start > end (invalid) | funkcije.vb:528-534 |
| `ucitajImena` | (none) | DataTable | Load room guest names via SP `getGlavniImena` | funkcije.vb:557-577 |
| `ucitajImePodatak` | (rid As Integer) | DataTable | Load guest data by RID via SP `getGlavniPodaci` | funkcije.vb:578-602 |
| `vratiCijenunocenjaSoba` | (dtmdo As DateTime, dtmVrij As DateTime, SID As Integer) | Decimal | Calculate total night price for a room (calls `ucitajGostenocenja`) | funkcije.vb:604-633 |
| `ucitajGostenocenja` | (sobID As Integer) | DataTable | Get night registration RIDs per room via SP `vratiRIDnocenja` | funkcije.vb:635-659 |
| `troskoviSVineplaceni` | (none) | DataTable | Get all unsettled expenses (zaklj=0) | funkcije.vb:660-680 |

### ModuleKod Key Utility Functions

| Function | Parameters | Return Type | Purpose | File:Line |
|----------|-----------|-------------|---------|-----------|
| `sobe_load` | (none) | Sub | Load rooms table into `ds.Tables("sobe")` | ModuleKod.vb:124-130 |
| `loadsetg` | (none) | Sub | Load settings (`zagl`, `status`) into `ds` | ModuleKod.vb:131-153 |
| `inportgod` | (tip As Byte) | Sub | Import data between year databases (`My.Settings.godbaza` → current) | ModuleKod.vb:154-196 |
| `avans_print` | (br, storno) | Sub | Print advance payment receipt | ModuleKod.vb:295-336 |
| `alarm` | (opis, opis1, odgovor, vrijeme, vrijeme1, tipalarm1, rpt, week, p-u-ned, radn, sobaid) | Sub | Insert alarm/reminder record | ModuleKod.vb:337-376 |
| `alarStorno` | (radnik, id) | Sub | Cancel/delete an alarm | ModuleKod.vb:378-409 |
| `provjerialarm` | (none) | Sub | Check and display active alarms | ModuleKod.vb:411-439 |
| `provrezstare` | (none) | Sub | Check old reservations | ModuleKod.vb:440-442 |
| `citajAlarm` | (none) | DataTable | Read active alarms from DB | ModuleKod.vb:443-488 |
| `baza` | (none) | Sub | Show database configuration form | ModuleKod.vb:489-496 |
| `izmjene` | (none) | Sub | Check/set version changes | ModuleKod.vb:497-671 |
| `sobanaziv` | (sid As Integer) | String | Get room name by room ID | ModuleKod.vb:672-679 |
| `checkchema` | (shem As String) | DataTable | Check database schema existence | ModuleKod.vb:680-700 |
| `crecshem` | (str As String) | String | Create/read schema string | ModuleKod.vb:701-755 |
| `createTable` | (none) | Sub | Create all database tables if not exist (massive DDL) | ModuleKod.vb:756-779 |
| `viewcr` | (none) | Sub | Create/alter views | ModuleKod.vb:780-843 |
| `bazaProc` | (none) | Sub | Create/replace stored procedures (DDL) | ModuleKod.vb:844-1233 |
| `bazaSql` | (iddi As Integer) | Sub | Execute SQL script by ID | ModuleKod.vb:1235-2161 |
| `mysqlReader` | (queri, tablename) | DataTable | Execute SQL query, return DataTable | ModuleKod.vb:2610-2626 |
| `mysqlReaderK` | (queri, tablename) | DataTable | Execute SQL on Kasa DB, return DataTable | ModuleKod.vb:2555-2571 |
| `mysqlExScalar` | (queri) | Boolean | Execute scalar query, return success | ModuleKod.vb:2592-2608 |
| `mysqlExScalarK` | (queri) | String | Execute scalar on Kasa DB, return string result | ModuleKod.vb:2536-2553 |
| `mysqlExScalarLast` | (queri) | Integer | Execute INSERT, return last insert ID | ModuleKod.vb:2573-2591 |
| `mysqlProcedure` | (queri, tablename) | DataTable | Call stored procedure, return DataTable | ModuleKod.vb:2628-2645 |

### Data Module Key Utility Functions

| Function | Parameters | Return Type | Purpose | File:Line |
|----------|-----------|-------------|---------|-----------|
| `citajpod` | (none) | Sub | Read connection config from `set.xml` | Data.vb:30-71 |
| `checkchemac` | (none) | Sub | Validate database connection and settings table | Data.vb:73-98 |
| `punigod` | (none) | Sub | Load year databases and rebuild ConnStr with year suffix | Data.vb:100-115 |
| `promjenaConnStr` | (none) | Sub | Rebuild ConnStr (without year suffix) | Data.vb:574-576 |
| `PrljavaSoba` | (sobaid As Integer) | Sub | Mark room as dirty (clean=0) | Data.vb:119-141 |
| `OdjavaSobe` | (izn, gid, sid, pids, checkOutDate, uplatanocenja) | Boolean | **P0: Checkout** — transactional: updates nocenja, relgostsoba, posjetaFolio, troskovi | Data.vb:142-226 |
| `pripremaRacuna` | (sobaid, grup) | Sub | **P0: Prepare billing** — loads guest/expenses/payments for a room | Data.vb:229-261 |
| `racundo` | (doo As String) | Sub | Calculate billing for all guests in dst | Data.vb:262-269 |
| `vratiCijenunocenja` | (dtmOd, dtmdo, dtmVrij, RID) | Decimal | Calculate night price for a guest registration | Data.vb:336-414 |
| `VratiBrojDana` | (odd, doo) | Integer | Calculate day count using `minchi`/`maxcho` settings | Data.vb:416-436 |
| `vratiUplatunocenja` | (pid1 As Integer) | Decimal | Get paid accommodation total via SP `getPlacanjenocenja` | Data.vb:437-473 |
| `nocenjeSo` | (sid, pid) | DataTable | Get night registrations by room/folio | Data.vb:474-499 |
| `ucitajNacine` | (none) | Sub | Load payment methods into `ds.Tables("nplac")` | Data.vb:505-541 |
| `citajfirme` | (none) | DataTable | Load partners/companies into `ds.Tables("firme")` | Data.vb:596-622 |
| `getidd` | (none) | String | Generate unique ID from timestamp + computer name | Data.vb:501-504 |

---

## 6. Business-Critical Globals

### P0 — Room Status (Occupied/Vacant/Dirty)

| Global | Critical For | Evidence |
|--------|--------------|----------|
| `ds` (DataSet) | Holds `sobe` table with room status (`gost`, `sos`, `vatr`, `clean` columns) | ModuleKod.vb:94-128, Data.vb:11 |
| `ConnStr` | All DB operations for room data | ModuleKod.vb:126 (sobe_load), Data.vb:75 |
| `stanica` | Station ID for room status queries | Data.vb:78 (`setings where stan=`) |
| `kardTable` | Card reader data feeding into room status (`gost`, `sos`, `vatr`) | ModuleKod.vb:91-116 |
| `CardContr` (Setting) | Whether card controller is active | app.config:124 (False) |
| `ipAdres` (Setting) | Card controller IP | app.config:112 |
| `portK` (Setting) | Card controller port | app.config:115 |
| `PrljavaSoba()` | Marks room dirty on checkout | Data.vb:119-141 |

### P0 — Guest Check-in

| Global | Critical For | Evidence |
|--------|--------------|----------|
| `RID` | Worker ID recorded at check-in | frmLogin.vb:145, Data.vb:20 |
| `RIme` | Worker name recorded at check-in | frmLogin.vb:146, Data.vb:21 |
| `ConnStr` | All DB operations | Data.vb:9 |
| `ds.Tables("setings")` | Check-in time thresholds (`minchi`, `maxcho`) | Data.vb:340, 419, 425 |
| `izborForme` | Controls check-in guest form flow | frmPrijava1.vb:272 |
| `getidd()` | Generates unique ID for check-in records | Data.vb:501-504 |

### P0 — Guest Check-out (OdjavaSobe)

| Global | Critical For | Evidence |
|--------|--------------|----------|
| `ConnStr` | Transactional checkout op | Data.vb:146 |
| `PlacanjeSlozRBR` | Receipt line numbering | Data.vb:12, frmPlacanjeSlozeno.vb:64 |
| `previewRacuna` | Receipt preview control | Data.vb:592, frmPlacanje.vb:569,664,695 |
| `objOdjava` / `eOdjava` | Toolbar button state for checkout | Data.vb:594-595, frmGlavni.vb:656-657 |
| `ds.Tables("setings")` | Checkout hour config (`maxcho`, `minchi`) | Data.vb:340,419 |

### P0 — Payment/Billing

| Global | Critical For | Evidence |
|--------|--------------|----------|
| `dst` (DataSet) | Working billing data (ime, tros, uplate, noc, pror tables) | Data.vb:2, 229-261 |
| `ConnStr` | All payment DB operations | Throughout frmPlacanje.vb |
| `ConnStrKasa` | POS/cash register payments | ModuleKod.vb:2539,2557 |
| `stanicaK` | Cash register station | Data.vb:24, frmTroskovi.vb:376-392 |
| `akcij`, `akcij1`, `akcij2` | Inter-form payment action commands | Data.vb:17-19, frmPlacanje.vb:2525-2543, frmRacuni.vb:203-410 |
| `digi` | Partner/company ID in payment | Data.vb:22, frmPartneri.vb:291 |
| `pime` | Worker name for receipt header | Data.vb:25, frmPlacanje.vb:2458,3175 |
| `PlacanjeSlozRBR` | Receipt line counter | Data.vb:12 |
| `printajNocenje` | Print accommodation flag | Data.vb:589, frmReportRacun.vb:8 |

### P0 — Reservations

| Global | Critical For | Evidence |
|--------|--------------|----------|
| `RIme` | Worker name on reservation record | frmRezervacije.vb:942, frmZurnal1.vb:428 |
| `RID` | Worker ID on reservation record | frmRezervacije.vb:942 |
| `akcij`, `akcij1`, `akcij2` | Inter-form reservation actions | frmRezervacije.vb:797-909 |
| `ConnStr` | All reservation DB operations | Throughout frmRezervacije.vb |

### P0 — Night Calculation (Nocenje)

| Global | Critical For | Evidence |
|--------|--------------|----------|
| `Brojnocenja1`, `Brojnocenja2` | Night count totals in reports | Data.vb:14-15, frmIzvjestajiDnevni.vb:253-278 |
| `ds.Tables("setings")` | `minchi`/`maxcho` for day counting logic | Data.vb:419-435 |
| `ConnStr` | All night calculation queries | Data.vb:354, funkcije.vb:132 |

---

## 7. Cross-Reference

### Which Forms/Modules Reference Which Globals

| Form/Module | `ConnStr` | `ConnStrKasa` | `ds` | `dst` | `stanica`/`stanicaK` | `RID`/`RIme` | `akcij`/`akcij1`/`akcij2` | `printajNocenje` | `previewRacuna` | `PlacanjeSlozRBR` |
|-------------|-----------|---------------|------|-------|----------------------|--------------|--------------------------|------------------|-----------------|-------------------|
| **Data.vb** | Def:9,574 | Def:10,63 | Def:11 | Def:2 | Def:23-24,50,62 | Def:20-21 | Def:17-19 | Def:589 | Def:592 | Def:12 |
| **ModuleKod.vb** | 156,298,339,+40 | 2539,2557 | 94,126-152,+30 | — | — | 1934(RIme) | — | — | — | — |
| **funkcije.vb** | 132,250,379,+8 | — | 377(Informacije) | — | — | Def:5-6(Shared) | — | — | — | — |
| **frmGlavni.vb** | — | — | setings,sobe | — | 1860(stanica) | — | 1343-1360 | — | — | — |
| **frmLogin.vb** | — | — | — | — | — | 145-146(RID,RIme) | — | — | — | — |
| **frmPrijava1.vb** | — | — | — | — | — | — | — | — | — | — |
| **frmOdjava1.vb** | — | — | — | — | — | — | — | — | — | — |
| **frmPlacanje.vb** | — | — | — | — | 3175(sobekuc) | — | 2525-2543,6136-6538 | — | 569,664,695,6322,6335,6391 | 4986,5677,6386 |
| **frmPlacanjeSlozeno.vb** | — | — | — | — | — | — | — | — | — | 64 |
| **frmRacuni.vb** | 19,108,+20 | — | — | — | — | — | 203-410 | — | — | — |
| **frmGosti.vb** | 1051,+7 | — | GostiLista,+10 | — | — | — | — | — | — | — |
| **frmIzvjestajiDnevni.vb** | 20,+13 | — | +5 | — | — | — | — | — | — | — |
| **frmpostavke.vb** | 222,+16 | — | setings | — | 16,525,569 | — | — | — | — | — |
| **frmPosBaze.vb** | — | — | — | — | 36(stanica) | — | — | — | — | — |
| **frmTroskovi.vb** | — | — | — | — | 376-392(stanicaK) | — | — | — | — | — |
| **frmRezervacije.vb** | — | — | — | — | — | 942(RIme,RID) | 797-909 | — | — | — |
| **frmRezervacije_unos.vb** | — | — | — | — | — | 338,407(RIme,RID) | — | — | — | — |
| **frmReportRacun.vb** | — | — | — | — | — | — | — | 8 | — | — |
| **rptRacunFrm.vb** | — | — | — | — | — | — | 176-387 | — | — | — |
| **frmNapomene.vb** | — | — | — | — | — | 22,61(RIme) | — | — | — | — |
| **frmAlarm.vb** | — | — | — | — | — | 165(RIme) | — | — | — | — |
| **frmZurnal1.vb** | — | — | — | — | — | 428(RIme,RID) | — | — | — | — |
| **clasMysqlAdapt.vb** | 57(start) | — | — | — | — | — | — | — | — | — |
| **konfiguracija.vb** | (commented) | — | — | — | — | — | — | — | — | — |

### Settings Cross-Reference

| Setting | Referenced In | Context |
|---------|--------------|---------|
| `ipAdres` | ModuleKod.vb:11 | Card controller IP address |
| `portK` | UNKNOWN from analyzed code | Card controller port |
| `CardContr` | UNKNOWN from analyzed code (likely frmGlavni or startup) | Enable/disable card controller |
| `predKard` | UNKNOWN from analyzed code | Card prefix string |
| `PrikaziEur` | UNKNOWN from analyzed code | EUR display toggle |
| `godbaza` | ModuleKod.vb:174,185 | Year DB name for data import |
| `god` | frmGlavni (year combo) | Force specific year DB |
| `fiscal` (from setings DB) | frmLogin.vb:183, ModuleKod.vb:2851-3103 | Fiscal device type & paths |
| `minchi` (from setings DB) | Data.vb:419 | Min check-in hour |
| `maxcho` (from setings DB) | Data.vb:340,425 | Max check-out hour |

### Inter-Form Communication via Globals

The application uses **global variables** (`akcij`, `akcij1`, `akcij2`, `txt1`..`txt9`, `digi`) as a mechanism for passing commands and data between forms, replacing formal event or parameter passing:

| Global | Source Form | Target Form | Command Values |
|--------|-----------|-------------|----------------|
| `akcij` | frmRacuni, frmPlacanje, frmRezervacije | rptRacunFrm, frmTroskovi | "trosak", "trosakP", "Placanje", integer (receipt copies) |
| `akcij1` | frmRacuni, frmPlacanje | rptRacunFrm, frmTroskovi | "provjeriF", "pla", "p", room number, button text |
| `akcij2` | frmPlacanje, frmGlavni | rptRacunFrm | "storno", button name, timestamp |
| `digi` | frmPartneri | frmPlacanje | Partner/company ID |
| `txt1` | frmPrijava1 | Multiple | "ci" (check-in), "nek" |
| `izborForme` | frmPrijava1 | frmPrijavaGostiUnos | Form selection mode flag |
| `previewRacuna` | frmPlacanje, frmRacuni | rptRacunFrm | Preview vs print mode |

### Database Tables Referenced (by global ds)

The global `ds` DataSet is the central in-memory store. Key tables loaded:

| Table Name | Load Location | Business Purpose |
|-----------|---------------|-----------------|
| `sobe` | ModuleKod.vb:128 | Rooms (id, naziv, vrsta, lokal, ooo, razlog, clean, idkon, redulko, ipadres, port, gost, sos, vatr) |
| `setings` | frmGlavni (via checkchemac) | Hotel configuration row |
| `zagl` | ModuleKod.vb:134 | Header/restaurant details |
| `status` | ModuleKod.vb:144 | Guest status types |
| `nplac` | Data.vb:523 | Payment methods |
| `firme` | Data.vb:606 | Partners/companies |
| `ime` | Data.vb:232 | Guest-room relationships for billing |
| `tros` | Data.vb:233 | Room expenses for billing |
| `uplate` | Data.vb:235 | Payments for billing |
| `noc` | Data.vb:237-238 | Night registrations for billing |
| `pror` | Data.vb:299 | Billing period calculation |
| `GostiLista` | frmGosti.vb | Guest list |
| `Informacije` | funkcije.vb:377 | Room guest info |
| `cijene` | funkcije.vb:225-244 | Price calculation results |
| `IzvjestajNaplata` | frmIzvjestajiDnevni.vb | Payment report |
| `DnevniPlacanje` | frmIzvjestajiDnevni.vb | Daily payments |
| `NocenjeSTAT/SUM` | frmIzvjestajiDnevni.vb | Night stay statistics |
| `PlacanjaSlozena` | frmPlacanje.vb | Compound payments |
| `avans`/`avansd` | ModuleKod.vb:2173-2187 | Advance payments |
| `pred` | ModuleKod.vb | Pre-invoices |