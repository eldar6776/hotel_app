# Legacy Settings & Configuration System Analysis

## 1. All Application Settings (from Settings.Designer.vb)

User-scoped .NET settings stored in `app.config` → `user.config` (per-user roaming profile). Auto-saved on application exit via `My.Settings.Save()`.

| Name | Type | Default | Business Meaning |
|------|------|---------|-----------------|
| `bpe` | Integer | 4915275 | Board/panel encoding — likely baud rate or hardware ID for COM port card reader (`legacy_code/My Project/Settings.Designer.vb:59`) |
| `bim` | Integer | 589833 | Background ARGB color value used for form BackColor (`legacy_code/My Project/Settings.Designer.vb:71`, `frmGlavni.vb:1032`) |
| `vis0`–`vis6` | Integer (×7) | 40 each | DataGridView row height for 7 room grid columns (`legacy_code/My Project/Settings.Designer.vb:83-264`, `frmSobe.vb:121-151`) |
| `sir0`–`sir6` | Integer (×7) | 47 each | DataGridView column width for 7 room grid columns (`legacy_code/My Project/Settings.Designer.vb:95-280`, `frmSobe_Set.vb:361-391`) |
| `poz0`–`poz6` | Boolean (×7) | True each | Column visibility flags for 7 room grid columns (`legacy_code/My Project/Settings.Designer.vb:107-295`) |
| `si` | String | "0" | Sort/index flag (commented out usage in frmGlavni.vb:1027-1029) |
| `razs0`–`razs6` | Integer (×7) | 50 each | Row spacing/height for 7 room grid columns (`legacy_code/My Project/Settings.Designer.vb:347-427`) |
| `ipAdres` | String | "127.0.0.1" | IP address for card/door lock controller (`legacy_code/My Project/Settings.Designer.vb:432`, `ModuleKod.vb:11`) |
| `portK` | Integer | 5010 | TCP port for card/door lock controller (`legacy_code/My Project/Settings.Designer.vb:444`, `frmpostavke.vb:364`) |
| `dodatnav` | String | "" | Additional navigation path setting (unused in analyzed code) |
| `predKard` | String | "Soba " | Prefix printed on key cards before room number (`legacy_code/My Project/Settings.Designer.vb:468`, `classKard.vb:109`) |
| `CardContr` | Boolean | False | Card controller enabled flag — door lock hardware active (`legacy_code/My Project/Settings.Designer.vb:480`, `frmGlavni.vb:1039` (commented)) |
| `PrikaziEur` | Integer | 0 | Show EUR currency toggle: 0=local currency only, 1=show EUR (`legacy_code/My Project/Settings.Designer.vb:492`, `frmPlacanje.vb:530`, `frmRacuni.vb:471`) |
| `update` | Byte | 0 | Update check flag — first-run marker (`frmGlavni.vb:958-1016`) |
| `ch1`–`ch3` | Byte (×3) | 0 each | DataGridView checkbox/column visibility toggles in reservations journal (`legacy_code/My Project/Settings.Designer.vb:516-535`, `frmRezervacije.vb:32-34`, `frmZurnal1.vb:6-8`) |
| `co0`–`co2` | Integer (×3) | 100 each | Column widths for payment/reservation grids (`legacy_code/My Project/Settings.Designer.vb:551-575`, `frmRezervacije.vb:653-655`, `frmZurnal1.vb:220-222`) |
| `ch4`, `ch5` | Byte (×2) | 0 each | Additional checkbox toggles in reservations (`legacy_code/My Project/Settings.Designer.vb:587-607`, `frmRezervacije.vb:35-36`) |
| `god` | Integer | 0 | Year override — forces specific year database when non-zero |
| `godbaza` | String | "" | Source database name for year data import (`legacy_code/My Project/Settings.Designer.vb:624`, `ModuleKod.vb:174,185`) |
| `kardtip` | Integer | 0 | Card controller type selection (`legacy_code/My Project/Settings.Designer.vb:636`, `frmpostavke.vb:15`, `frmPrijava1.vb:402`) |
| `poruka` | Integer | 1 | Message/notification display counter (`frmpostavke.vb:9`, `frmpostavke.vb:1785-1786`) |
| `rezc` | Integer | 0 | Reservation grid column width persistence (`frmRezervacije.vb:1154-1166`) |
| `rezr` | Integer | 0 | Reservation grid row height persistence (`frmRezervacije.vb:1156-1173`) |
| `gostisvi` | Integer | 0 | Show all guests flag — filter toggle in payments (`legacy_code/My Project/Settings.Designer.vb:684`, `frmPlacanje.vb:531`, `frmRacuni.vb:472`) |

**Pattern**: Settings `vis0`-`vis6`, `sir0`-`sir6`, `poz0`-`poz6`, `razs0`-`razs6` form 7 parallel groups for DataGridView column customization in the room overview grid (`frmSobe.vb:121-153`).

---

## 2. Database Settings Table Columns (`setings`)

Source: `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2013-1283`
Queried at: `Data.vb:78`, `frmGlavni.vb:1860`, `frmpostavke.vb:46-82,569`

| Column | SQL Type | Default | Decoded Meaning |
|--------|----------|---------|-----------------|
| `id` | int(10) PK AUTO_INCREMENT | — | Settings row ID (supports per-station rows via `stan` filter) |
| `verbaz` | varchar(50) | NULL | Database version identifier (`verB` in code, `ModuleKod.vb:5`) |
| `verpr` | varchar(50) | NULL | Program version identifier (`verP` in code, `ModuleKod.vb:6`) |
| `keyk` | varchar(205) | NULL | License/encryption key — parsed for license validation (`frmGlavni.vb:1196`) |
| `vrijU` | datetime | NULL | Timestamp of settings update |
| `t1` | varchar(100) | NULL | Toggle 1: "1"=show receipt date column (`frmpostavke.vb:73`) |
| `t2` | varchar(100) | NULL | Toggle 2: "1"=show receipt month column (`frmpostavke.vb:74`) |
| `t3` | varchar(100) | NULL | Composite: `value;id` parsed by `;` — tourist registration API URL and object ID (`frmpostavke.vb:81-82`) |
| `izmjver` | varchar(50) | NULL | Version change timestamp string |
| `rad` | varchar(50) | NULL | Worker name who last modified settings |
| `stan` | int(4) unsigned | 0 | **Station/workstation number** — key filter for multi-station setups (`Data.vb:78`) |
| `naplposo` | int(10) unsigned | 0 | Payment mode: 0=per person, 1=per room (`frmpostavke.vb:72-78`, `frmpostavke.vb:537-539`) |
| `pribora` | varchar(50) | NULL | Number of bed linens (label used for hotel info field `txthoteltur`) (`frmpostavke.vb:68`) |
| `pdv` | double | 0 | VAT (PDV) rate — primary rate (e.g., 17%) (`frmpostavke.vb:67`) |
| `pdvo` | double | 0 | VAT on services (`frmpostavke.vb:65`, `txtpdvOs`) |
| `pdvtax` | double | 0 | VAT on tourist tax (`frmpostavke.vb:64`, `txtpdvTa`) |
| `pdvtr` | double | 0 | VAT on tourism/room rate (`frmpostavke.vb:66`, `txtpdvTr`) |
| `osig` | double | 0 | Insurance rate (`frmpostavke.vb:63`, `TextBox4`) |
| `taxa` | double | 0 | Tourist tax amount (`frmpostavke.vb:62`, `TextBox5`) |
| `cultur` | varchar(40) | NULL | Culture/locale string (e.g., "hr-BA") for `CultureInfo` (`frmGlavni.vb:823,847`, `frmpostavke.vb:69`) |
| `sobegrupa` | varchar(150) | NULL | Room group setting (apartment grouping) |
| `sobekuc` | varchar(150) | NULL | Apartment/room grouping codes (delimited by `#`, see Section 3) |
| `dijecagod` | double | 0 | Children age limit (years) (`frmpostavke.vb:61`, `txtdijGod`) |
| `dijecapop` | double | 0 | Children discount percentage (`frmpostavke.vb:60`, `txtdijProc`) |
| `fiscal` | varchar(150) | NULL | **Fiscal device configuration** (delimited by `*`, see Section 3) |
| `valuta` | varchar(20) | NULL | Currency name (e.g., "KM", "EUR") (`frmpostavke.vb:56`) |
| `racunbr` | varchar(50) | NULL | Receipt/invoice number format (`frmpostavke.vb:57`) |
| `napomena` | text | NULL | Notes on settings |
| `lokac` | varchar(50) | NULL | Location code — used for fiscal receipt filtering (`frmGlavni.vb:1601-1608`) |
| `cijt` | int(10) unsigned | 0 | Price type: 1=price includes tax, 0=price excludes tax (`frmpostavke.vb:59-60`) |
| `minchi` | int(10) unsigned | 8 | Minimum check-in hour (used in night calculation) (`frmpostavke.vb:70`, `Data.vb:419`) |
| `maxcho` | int(10) unsigned | 12 | Maximum check-out hour (used in night calculation) (`frmpostavke.vb:71`, `Data.vb:340,425`) |
| `decim` | int(10) unsigned | 2 | Decimal precision setting for currency display |

**Key query**: `SELECT verbaz, verpr, keyk, vrijU, t1, t2, t3, id, izmjver, rad, minchi, maxcho, pribora, pdv, pdvo, pdvtax, pdvtr, osig, taxa, cultur, sobegrupa, sobekuc, dijecagod, dijecapop, fiscal, valuta, racunbr, napomena, naplposo, cijt, lokac, decim FROM setings WHERE stan=<station> ORDER BY id DESC LIMIT 1` (`frmGlavni.vb:1860`)

---

## 3. Compound/Delimiter Settings

### 3.1 `sobekuc` — Apartment/Room Grouping (Delimiter: `#`)

**Format**: `TextBox12#TextBox13#TextBox14#NumericUpDow8#0#Texteuser#Texeknjiga#ktz#`

**Decoded fields**:

| Index | Example | Meaning | Evidence |
|-------|---------|---------|----------|
| `0` | URL string | **Tourist registration API URL** — double-clicking sets `http://test.prijava.ba/api/api.php` (`frmpostavke.vb:21-26,1772`) |
| `1` | String | **Tourist registration username** (`frmpostavke.vb:23, Texteuser`) |
| `2` | String | **Tourist registration book number** (knjiga reference) (`frmpostavke.vb:27, Texeknjiga`) |
| `3` | Number | **Numeric configuration value** (NumericUpDown8) (`frmpostavke.vb:26`) |
| `4` | "0" | **Placeholder/reserved** (`frmpostavke.vb:26`) |
| `5` | String | **Tourist registration user** (Texteuser) (`frmpostavke.vb:28`) |
| `6` | String | **Tourist registration book label** (Texeknjiga) (`frmpostavke.vb:29`) |
| `7` | 0 or 1 | **Card access flag for tourist registration** — CheckBox3: 0=disabled, 1=enabled (`frmpostavke.vb:30, ktz`) |

**Save**: `frmpostavke.vb:1696` — `TextBox12.Text & "#" & TextBox13.Text & "#" & TextBox14.Text & "#" & NumericUpDown8.Value & "#0#" & Texteuser.Text & "#" & Texeknjiga.Text & "#" & ktz & "#"`

### 3.2 `fiscal` — Fiscal Device Configuration (Delimiter: `*`)

**Format**: `typeIndex*path1*path2*path3*fiscalDeviceModel*printQuantities`

**Decoded fields**:

| Index | Name | Meaning | Evidence |
|-------|------|---------|----------|
| `0` | typeIndex | **Fiscal device type number** (0=None, 1=Eltrade, 2=NSC/Datecs, 3=Tring, 4=TermoL, 5=ERP/Star, 6=Mikroelektronika, 7=HCP) (`frmpostavke.vb:698-763`, `cmdFisk.SelectedIndex`) |
| `1` | nscTextBox1 | **IOSA number / device ID** (NSC: "1000000000000023", Tring: description) (`frmpostavke.vb:49,712`) |
| `2` | nscTextBox2 | **Path/file 1** — NSC: startup file path, Tring: port number, TermoL/HCP: shared directory (`frmpostavke.vb:50,711,735,743`) |
| `3` | nscTextBo3 | **Path/file 2** — NSC: response directory, TermoL/HCP: "C:\HCP\TO_FP" (`ModuleKod.vb:2853,2974,3037,3103`) |
| `4` | ComboBox8 | **Fiscal device model name** (e.g., "FP-550H") or location/host (`frmpostavke.vb:51`) |
| `5` | fiskkol | **Print quantities flag**: 1=include quantities on fiscal receipt, 0=don't (`frmpostavke.vb:52,562-563`, `frmPlacanje.vb:2886`, `frmRacuni.vb:1837`) |

**Fiscal device type details** (from `frmpostavke.vb:698-762`):

| Index | Name | Default path1 | Default path2 | Default ID | fiscbr value |
|-------|------|-------------|-------------|-------------|-------------|
| 0 | "---- Ne koristi se ----" (Not used) | — | — | — | 0 |
| 1 | "Eltrade - Montenegro" | — | — | — | 1 |
| 2 | "NSC doo - Datecs" | "C:\FPrint_logs\start" | "C:\FPrint_logs\odg" | "1000000000000023" | 2 |
| 3 | "Tring doo - Tring" | "" | 8085 | "localhost" | 3 |
| 4 | "KimTec doo - Termol" | "C:\HCP\FROM_FP" | "C:\HCP\TO_FP" | "HCP" | 4 |
| 5 | "Erp doo - Star" | — | — | — | 5 |
| 6 | "Mikroelektronika - Republika Srpska" | — | — | — | 6 |
| 7 | "KimTec doo - HCP" | "C:\HCP\FROM_FP" | "C:\HCP\TO_FP" | "HCP" | 7 |
| 8 | "ComTrade - Srbija" (combo item) | — | — | — | — |

### 3.3 `t3` — Tourist Registration Config (Delimiter: `;`)

**Format**: `passwordOrUrl;objectId`

| Part | Meaning | Evidence |
|------|---------|----------|
| Part before `;` | **Tourist registration password/key** | `frmpostavke.vb:81`: `ds.Tables("setings").Rows(0).Item("t3").ToString.Split(";")(0)` → `TextBox10` |
| Part after `;` | **Tourist registration object ID** | `frmpostavke.vb:82`: `ds.Tables("setings").Rows(0).Item("t3").ToString.Split(";")(1)` → `idobj` |

Saved as: `TextBox10.Text & ";" & idobj.Text` (`frmpostavke.vb:569`)

### 3.4 `keyk` — License Key (Parsed format)

**Usage**: Parsed at `frmGlavni.vb:1196` with string position extraction for license validation:
- `str.Substring(7,2)` → day component + 3
- `str.Substring(5,2)` → month component + 4
- Format includes encoded date/expiry information

---

## 4. Settings That Affect Business Flows

| Setting Key | Affected Flow | Effect | Evidence |
|-------------|---------------|--------|----------|
| `pdv` | Night calculation, billing | VAT rate applied to all invoices | `frmpostavke.vb:67`, `Data.vb:336-414` |
| `pdvtr` | Night calculation | VAT on tourism rate | `frmpostavke.vb:66` |
| `pdvo` | Billing | VAT on services rate | `frmpostavke.vb:65` |
| `pdvtax` | Billing | VAT on tourist tax | `frmpostavke.vb:64` |
| `osig` | Night calculation | Insurance rate added to nightly rate | `frmpostavke.vb:63`, `funkcije.vb:248-374` |
| `taxa` | Night calculation | Tourist tax per person per night | `frmpostavke.vb:62`, `Data.vb:336-414` |
| `minchi` | Check-in time logic | Check-in hour threshold (default 8) — affects day counting in `VratiBrojDana` | `Data.vb:419`, `frmpostavke.vb:70` |
| `maxcho` | Check-out time logic | Check-out hour threshold (default 12) — if checkout after this, extra night counted | `Data.vb:340,425`, `frmpostavke.vb:71` |
| `dijecagod` | Pricing | Children age limit for discount | `frmpostavke.vb:61` |
| `dijecapop` | Pricing | Children discount percentage | `frmpostavke.vb:60` |
| `cijt` | Pricing display | 0=price excluded tax, 1=price includes tax | `frmpostavke.vb:59-60` |
| `naplposo` | Payment | 0=charge per room, 1=charge per person | `frmpostavke.vb:72-78` |
| `cultur` | Localization | Sets `CultureInfo` for thread, affects date/number formatting | `frmGlavni.vb:823,847` |
| `fiscal` | Receipt printing | Determines which fiscal device driver to use, file paths for communication | `frmGlavni.vb:1041,1095,1572,1691`, `ModuleKod.vb:2851-3103` |
| `valuta` | Currency display | Currency name shown on receipts | `frmpostavke.vb:56` |
| `racunbr` | Receipt numbering | Invoice/receipt number format string | `frmpostavke.vb:57` |
| `lokac` | Fiscal receipt | Location code used to filter fiscal response files | `frmGlavni.vb:1601-1608` |
| `t1` | Receipt printing | "1"=show receipt date column | `frmpostavke.vb:73` |
| `t2` | Receipt printing | "1"=show receipt month column | `frmpostavke.vb:74` |
| `t3` | Tourist registration | Tourist registration API credentials (URL;ID) | `frmpostavke.vb:81-82` |
| `sobekuc` | Room grouping | Apartment codes for grouped rooms, tourist registration API config | `frmpostavke.vb:21-33,1696`, `frmGosti.vb:1283` |
| `PrikaziEur` | Payment display | 1=show EUR equivalent column | `frmPlacanje.vb:530,4880-4882`, `frmRacuni.vb:471,2198-2200` |
| `gostisvi` | Guest display | 1=show all guests (not just current) | `frmPlacanje.vb:531,6617-6619`, `frmRacuni.vb:472,2380-2382` |
| `CardContr` | Door lock system | Enables/disables card controller connection | `frmGlavni.vb:1039` (commented) |
| `ipAdres` | Door lock system | IP address for card controller hardware | `ModuleKod.vb:11` |
| `portK` | Door lock system | TCP port for card controller | `frmpostavke.vb:364` |
| `kardtip` | Door lock system | Card controller protocol type | `frmpostavke.vb:15`, `frmPrijava1.vb:402` |
| `godbaza` | Year data import | Source database name for transferring data between years | `ModuleKod.vb:174,185` |
| `keyk` | Licensing | Parsed for license validation (date expiry encoded in key) | `frmGlavni.vb:1196` |
| `bim` | UI theming | Form background color when ≠ 589833 | `frmGlavni.vb:1031-1032` |

---

## 5. Connection Strings and Server Configuration

### 5.1 Primary Connection String

**Source**: `Data.vb:9,114`

**Construction pattern**: `server={server};database={servDB}{year};UID={UIDd};password={passwo};port={port};character set=utf8;`

**Example**: `server=localhost;database=iMEDIAHotel2019;UID=root;password=mypass;port=3306;character set=utf8;`

**Config file**: `C:\Program Files\IMEDIA\HotelPro\set.xml` (`Data.vb:33-34`)

### 5.2 set.xml Structure (XML Dataset)

**Table**: `constr`

| Field | Obfuscation | Target Global | Meaning |
|-------|-------------|---------------|---------|
| `serv` | Trim last 2 chars | `server` | MySQL server address |
| `user` | Trim last 2 chars | `UIDd` | MySQL username |
| `pass` | Strip `%&rt!h23` suffix, trim last 3 chars | `passwo` | MySQL password |
| `port` | Trim last 2 chars | `port` (default 3306) | MySQL port |
| `baza` | Trim last 2 chars | `servDB` | Database name base (year appended runtime) |
| `stan` | (none) | `stanica` | Primary station/workstation number |
| `serv1` | Trim last 2 chars | (local) srv1 | Cash register MySQL server |
| `user1` | Trim last 2 chars | (local) user1 | Cash register MySQL user |
| `pass1` | Strip `%&rt!h23`, trim last 3 chars | (local) pass1 | Cash register MySQL password |
| `port1` | Trim last 2 chars | (local) port1 | Cash register MySQL port |
| `baza1` | Trim last 2 chars | (local) baza1 | Cash register database name |
| `stan1` | (none) | `stanicaK` | Cash register station number |

**Write**: `frmPosBaze.vb:8-49` — writes set.xml with obfuscation suffix (adds `-i` to text fields, `%&rt!h23` to passwords, `mm2` suffix)

**Password obfuscation**: Passwords are stored with `%&rt!h23` prepended and `mm2` appended. All text fields have arbitrary trailing characters (2 chars for most, 3 for passwords) removed at read time. This is NOT encryption — only trivial obfuscation.

### 5.3 Secondary (Cash Register) Connection String

**Source**: `Data.vb:63`

```vb
ConnStrKasa = "server={srv1};database={baza1};UID={user1};password={pass1};port={port1};character set=utf8;"
```

### 5.4 Direct Schema Operations (No Database Specified)

**Source**: `ModuleKod.vb:683-684,704-705`

```vb
"server={server};UID={UIDd};password={passwo};port={port};character set=utf8;"
"server={server};UID={UIDd};password={passwo};character set=utf8;"
```

Used for creating databases and schemas without specifying a database name.

### 5.5 Commented Out Connection String

`konfiguracija.vb:2-16` — Entire file is commented out. Shows SQL Server and SQL Express patterns:

```vb
"user id=imedia;password=mm22in;database=iMEDIAHotel;server=P4"
"Data Source=.\SQLEXPRESS;AttachDbFilename=...;Integrated Security=True..."
```

### 5.6 app.config Connection String

`app.config:8`: `<add name="DefaultConnection" connectionString="Data Source = |SQL/CE|" />` — Placeholder, NOT used at runtime. MySQL connections are built from set.xml.

### 5.7 Multi-Year Database Architecture

`punigod()` at `Data.vb:100-115` appends the selected year from `frmGlavni.cmbgodine` to `servDB`:
- Database names follow pattern: `{servDB}{year}` (e.g., `iMEDIAHotel2019`, `iMEDIAHotel2020`)
- Year combo is populated by querying MySQL for existing schemas matching `{servDB}%` pattern
- `My.Settings.god` overrides year selection when non-zero
- `My.Settings.godbaza` specifies source database for cross-year data import

---

## 6. Fiscal Device Configuration

### 6.1 Fiscal Device Types and Protocol Details

The `fiscal` column in `setings` table stores a `*`-delimited string with device configuration:

**Format**: `{typeIndex}*{deviceID}*{path1}*{path2}*{modelName}*{printQuantities}`

| Type Index | Device | fiscbr | Protocol | Evidence |
|-----------|--------|--------|----------|----------|
| 0 | Not used | 0 | None | `frmpostavke.vb:757-761` |
| 1 | Eltrade (Montenegro) | 1 | XML file-based | `frmpostavke.vb:699-703` |
| 2 | NSC/Datecs | 2 | File-based (COM path) | `frmpostavke.vb:704-717` |
| 3 | Tring | 3 | HTTP REST (localhost:8085) | `frmpostavke.vb:718-730` |
| 4 | TermoL (KimTec) | 4 | File exchange (HCP folders) | `frmpostavke.vb:731-738` |
| 5 | Star (ERP) | 5 | File-based | `frmpostavke.vb:747-751` |
| 6 | Mikroelektronika (Republika Srpska) | 6 | — | `frmpostavke.vb:752-756` |
| 7 | HCP (KimTec) | 7 | File exchange (HCP folders) | `frmpostavke.vb:739-746` |

### 6.2 Fiscal Communication Patterns

**NSC/Datecs (type 2)**:
- Uses `FileSystemWatcher` on `fiscal.Split("*")(3)` directory watching `*.*` files
- Reads response `.err` files containing fiscal receipt confirmation
- Parses `BF:` prefix for fiscal receipt numbers
- Evidence: `frmGlavni.vb:1565-1680`

**TermoL/HCP (types 4, 7)**:
- Writes `ra.in` / `rr.in` command files to application startup path
- Copies command files to `fiscal.Split("*")(2)` directory as `rr.eln`
- Uses `FileSystemWatcher` on `fiscal.Split("*")(3)` watching `*.eln` responses
- Reads response files, parses receipt numbers after `#` + `lokac` location code
- Evidence: `ModuleKod.vb:2853,2932,2974,3037,3103-3105`

**Eltrade (type 1)**:
- XML file-based communication using `TremolFpServer` XML protocol
- `ModuleKod.vb:2800-2874`: creates `Prodaja_{br}.xml` with receipt data
- Copies XML to `fiscal.Split("*")(3)` directory
- Evidence: `ModuleKod.vb:2853`

**Tring (type 3)**:
- HTTP REST communication via `localhost:8085`
- Uses `WebClient.DownloadString` for HTTP-based fiscal controller
- Evidence: `ClassLuxM.vb:93-104`

### 6.3 Fiscal Device Data Flow

1. **Save settings**: `frmpostavke.vb:560-564` — assembles `fis = cmdFisk.SelectedIndex & "*" & nscTextBox1.Text & "*" & nscTextBox2.Text & "*" & nscTextBo3.Text & "*" & ComboBox8.Text & "*" & fiskkol`
2. **On login**: `frmLogin.vb:183` — if `fiscal.Split("*")(0) = 4`, writes worker to TermoL device
3. **On payment**: `frmPlacanje.vb:2505-2648`, `frmRacuni.vb:549-746` — reads `fiscal.Split("*")` for device type and paths, calls appropriate fiscal driver
4. **On receipt print**: `frmGlavni.vb:1082-1095` — starts `FileSystemWatcher` on fiscal response directory
5. **Response handling**: `frmGlavni.vb:1565-1680` — processes fiscal device response files, extracts receipt numbers, updates `printracuni` table via `snimFiskal()`

### 6.4 Card Controller Configuration

**Settings involved**:
- `CardContr` (bool) — Master enable/disable for door lock hardware
- `ipAdres` (string) — IP address of card controller device
- `portK` (int) — TCP port for card controller
- `kardtip` (int) — Card controller protocol version (1=some type with check-in card writing)
- `bpe` (int) — Likely baud rate or hardware parameter for card reader
- `predKard` (string) — Prefix text ("Soba ") printed on cards before room number

**Database**: `kontroler` table stores per-room controller assignments (`ipadres`, `port`, `hostname`, `verpr`, `netbios`) — `frmpostavke.vb:307-331`

**Card protocol**: `classKard.vb:109` — sends command string: `Chr(2) & Chr(179) & {command} & Chr(179) & {kol} & Chr(179) & {motorised} & Chr(179) & Settings.predKard & {soba} & Chr(179) & ... & Chr(3) & Chr(13)`

---

## 7. Key Findings for Modern System

### 7.1 Two-Layer Configuration Architecture (Must Unify)

The legacy system stores configuration in **two completely separate systems**:
1. **.NET User Settings** (`My.Settings` / `app.config`): UI layout, card hardware, display preferences — per-user roaming
2. **Database `setings` table**: Business rules, tax rates, fiscal devices, hotel configuration — per-station, shared via MySQL

**Problem**: No single source of truth. Some settings overlap conceptually (card hardware in both layers). The modern system should use a unified configuration model with clear ownership.

### 7.2 Security: Trivial Obfuscation Only

- Passwords in `set.xml` use `%&rt!h23` prefix + `mm2` suffix + trimming (not encryption)
- License key (`keyk`) uses positional character extraction (not cryptography)
- All connection credentials are stored in a hardcoded XML file path (`C:\Program Files\IMEDIA\HotelPro\set.xml`) with no access control

### 7.3 Compound Settings Are Brittle

- `sobekuc` uses `#` delimiter with 8 fields — adding/removing any field breaks all code that parses by index
- `fiscal` uses `*` delimiter with 6 fields — same problem, plus fiscal device type determines how other fields are interpreted
- `t3` uses `;` delimiter with 2 fields — API credentials embedded in a config column

**Recommendation**: Replace with properly structured JSON or separate columns.

### 7.4 Fiscal Device Integration Is File-Based

All fiscal devices communicate via **file exchange** (COM port, shared folders, or HTTP). No standard API or driver interface exists — each device type has hard-coded path handling scattered across `ModuleKod.vb` (2800-3105), `frmPlacanje.vb`, `frmRacuni.vb`, and `frmGlavni.vb`.

**Recommendation**: Create a fiscal device abstraction layer with per-driver implementations.

### 7.5 Multi-Station Design

The `setings` table uses `stan` (station number) as a key filter, allowing different configurations per POS terminal. The `stanica` global from `set.xml` determines which settings row the application loads. This enables multiple workstations with different fiscal devices, locations, and configurations sharing the same database.

### 7.6 Year-Based Database Sharding

Database names append the year (`{servDB}{year}`), creating separate schemas per year. The `godbaza` setting enables cross-year data import. This is a primitive form of partitioning.

### 7.7 Hardcoded Paths

- `C:\Program Files\IMEDIA\HotelPro\set.xml` — configuration file location (`Data.vb:34`)
- `C:\HCP\TO_FP`, `C:\HCP\FROM_FP` — fiscal device directories (`frmpostavke.vb:734-744`)
- `C:\FPrint_logs\` — NSC/Datecs fiscal paths (`frmpostavke.vb:710-713`)
- `Application.StartupPath` — for temporary fiscal command files (`ModuleKod.vb:2853,2994`)

### 7.8 Settings Directly Affecting Financial Calculations

These settings MUST be preserved with audit trails in the modern system:
- `pdv`, `pdvo`, `pdvtr`, `pdvtax` — VAT rates
- `osig` — insurance rate
- `taxa` — tourist tax
- `minchi`, `maxcho` — check-in/out hour thresholds for night counting
- `dijecagod`, `dijecapop` — children's pricing rules
- `cijt` — tax-inclusive vs. tax-exclusive pricing
- `naplposo` — per-room vs. per-person charging
- `valuta` — currency name
- `racunbr` — receipt numbering format

### 7.9 Per-User Persistence (UI State)

`My.Settings` saves DataGridView column widths, row heights, visibility flags, checkbox states, and color preferences. These are purely UI state and should NOT be stored in the database — they should remain per-user/per-device local settings in the modern system.

### 7.10 Configuration Default Values

Critical defaults from the database schema and code:
- `minchi` = 8 (check-in hour: 08:00)
- `maxcho` = 12 (check-out hour: 12:00)
- `pdv` = 17% (typical BIH VAT)
- `decim` = 2 (currency decimal places)
- Card controller defaults: IP=127.0.0.1, Port=5010
- Fiscal device: type 0 (disabled) by default