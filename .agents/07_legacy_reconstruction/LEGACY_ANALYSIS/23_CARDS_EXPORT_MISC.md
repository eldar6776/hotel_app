# Legacy Analysis: Card/RFID System, Export Integration, Email, and Journal

## 1. Card/RFID System (classKard, frmKardRw, frmKardPro, kard_imedia, kard_imedia1)

### 1.1 classKard — Winsock-Based Card Encoder Interface

**Purpose:** Original (legacy) card encoder communication class using the `Winsock_Control.Winsock` ActiveX component, later superseded by direct TCP and USB HID.

**Key behaviors:**
- Connection via `Winsock_Control.Winsock` to `Settings.ipAdres:Settings.portK` (`classKard.vb:26`)
- Protocol uses `Chr(2)` / `Chr(3)` / `Chr(179)` framing with pipe-delimited fields (`classKard.vb:109`)
- Card write command `pisi()` sends: command, quantity, motorised flag, room (with prefix), rooms 2-4, access granted/denied, start/end time, worker, guest card type, tracks 2-3, serial (`classKard.vb:108-114`)
- Error code parsing in `citaj()`: ES=error, NC=comm break, NF=db damaged, OV=encoder busy, EP=card error, TD=unknown room, ED=operation cancelled, EA=room checked out, OS=error, EO=coded on other system, EV=validation error, ER=general error (`classKard.vb:161-222`)
- Card type detection: CN/CC=card OK, CA=card accepted, LT=card status query returning subtypes: LM=maid, LR=guest reserve, LC=invalid card, LD=unknown card (`classKard.vb:228-287`)
- A direct TCP fallback method `pisString()` exists that connects to hardcoded IP `192.168.1.100:9760` (`classKard.vb:51-106`), bypasses Winsock, reads 20-byte response array

**Critical issues:**
- Hardcoded IP in `pisString()` (`classKard.vb:52`)
- Mixed encoding: `Encoding.Default` for Winsock path, `Encoding.ASCII` for TCP path
- `citaj()` has dead code after `Return g` on line 154 — all error/LT parsing is unreachable
- No null/error checks before array indexing in response parsing

### 1.2 frmKardRw — USB HID Card Reader/Writer (Microchip PIC-based)

**Purpose:** Low-level USB HID device driver for a Microchip PIC-based RFID card encoder. Uses raw Windows API calls (`setupapi.dll`, `kernel32.dll`) to communicate via USB HID protocol.

**Architecture:**
- VID `04d8`, PID `002f` — Microchip custom HID device (`frmKardRw.vb:91`)
- USB HID GUID `4D1E55B2-F16F-11CF-88CB-001111000030` (`frmKardRw.vb:263`)
- Plug-and-play detection via `WM_DEVICECHANGE` messages (`frmKardRw.vb:370-418`)
- Read/Write handles opened via `CreateFile()` for synchronous USB I/O (`frmKardRw.vb:319-321`)
- Background `ReadWriteThread` for USB communication (`frmKardRw.vb:357`)

**USB Protocol (65-byte packets):**
- OUT buffer byte 0 = Report ID (always 0x00)
- Commands use byte 1 as command code, bytes 2-3 as sector/address
- Read sector: byte1=`0x52`('R'), byte2=`0x53`('S'), byte3=sector 0x30-0x38 (`frmKardRw.vb:844-1618`)
- Write sector: byte1=`0x57`('W'), byte2=`0x53`('S'), byte3=sector 0x30-0x38, data in bytes 4+ (`frmKardRw.vb:1193-1578`)
- Get card ID: byte1=`0x52`('R'), byte2=`0x49`('I') — command `0x17` (`frmKardRw.vb:1593-1616`)
- Response byte 1 = `0x53`('S') confirms success for sector ops, `0x52`('R') for ID request

**Sector structure:** 9 sectors (0-8), each 60 bytes of payload (bytes 4-64). Sector 0 stores card metadata; sectors 1-8 store room/guest data.

**Critical issues:**
- Requires x86 (32-bit) build due to P/Invoke with 32-bit DLLs (`frmKardRw.vb:67-78`)
- All USB I/O is synchronous/blocking on a background thread — no cancellation token pattern
- Massive code duplication: each sector read/write is copy-pasted with only byte3 changed (`frmKardRw.vb:844-1578`)
- No error recovery — exceptions silently swallowed (`frmKardRw.vb:1652`)

### 1.3 frmKardPro — USB HID Card Encoder (Production)

**Purpose:** Production card encoding form building on the same USB HID platform as frmKardRw, but implementing the actual hotel card encoding protocol.

**Card encoding protocol (`pisisobaN` method, `frmKardPro.vb:941-1071`):**
- Byte layout of 65-byte USB write buffer:
  - `[0]` = 0x00 (Report ID)
  - `[1]` = 'W' (0x57) — write command
  - `[2]` = card type: 'G'=guest, 'H'=maid, 'M'=manager, 'S'=service (`frmKardPro.vb:678-693,964`)
  - `[3-4]` = day digits (DD)
  - `[5-6]` = month digits (MM)
  - `[7-8]` = year digits (YY, 2-digit)
  - `[9-10]` = hour digits
  - `[11-12]` = minute digits
  - `[13-17]` = 5-digit room number (padded with zeros)
  - `[18]` = invalid flag: 'N'=normal, 'I'=invalid (`frmKardPro.vb:1003`)
  - `[19]` = language: E/Bos/Ger/Arap/Tur/Franc/Ita (`frmKardPro.vb:1004`)
  - `[20]` = logo position (space=no logo, number=position) (`frmKardPro.vb:1005`)
  - `[21]` = gender: 'M'=male, 'Z'=female (`frmKardPro.vb:1006`)
  - `[22-26]` = 5-digit object/facility ID (`frmKardPro.vb:1008-1013`)
  - `[32-48]` = surname (max 17 chars, diacritics stripped — Č→C, Ž→Z, etc.) (`frmKardPro.vb:1016-1020`)
  - `[48-64]` = first name (max 17 chars, diacritics stripped) (`frmKardPro.vb:1023-1028`)

**Card reading protocol (`PROCITAJN` method, `frmKardPro.vb:1073-1178`):**
- Send: byte1='R'(0x52), rest 0xFF (`frmKardPro.vb:1087-1091`)
- Response parsing:
  - bytes 2-6: card ID as `UInt32` (`frmKardPro.vb:1046-1048,1105`)
  - byte 7: type character (G/H/M/S)
  - bytes 10-11: day, 12-13: month, 14-15: year, 16-17: hour, 18-19: minute
  - bytes 23-27: 5-digit address (controller ID), resolved to room name via `ds.Tables("sobe")` (`frmKardPro.vb:1118-1128`)
  - bytes 28-31, 33-37: additional fields
  - bytes 38-47: first name, 48-63: surname (`frmKardPro.vb:1140-1143`)
  - bytes 33-37: object ID, compared against `idobjekta` for validity (`frmKardPro.vb:1169-1171`)

**Bar bracelet variant (`pisisobaBar`, `frmKardPro.vb:820-938`):**
- Similar protocol but byte layout differs:
  - `[1]` = 'P' (0x50)
  - `[2]` = card type (G/H/M/S)
  - `[3-12]` = date+time as individual digits
  - `[12-13]` = 2-digit user number
  - `[14]` = card form: 'C'=card, 'W'=wristband, 'Q'=pendant (`frmKardPro.vb:872-878`)
  - `[15]` = duration type: 'O'=one-time, 'E'=time-based (`frmKardPro.vb:881-885`)
  - `[16-20]` = 5-digit object ID
  - `[21]` = 'B' barcode mode marker

**Card database recording (`snimik`, `frmKardPro.vb:528-540`):**
```sql
INSERT INTO kard (kard, tip, soba, vazido, vrijeme, rad, sobaid, ime, del)
VALUES ('{kardid}', '{tip}', '{soba}', '{doo}', '{Now}', '{RIme}', '{sobaid}', '{ime}', 0)
```

**Card type codes:**
- `tip=1`: Guest card
- `tip=2`: Maid card
- `tip=3`: Guest card (read-only / bar mode)
- `tip=4`: Read card operation
- `tip=5`: Guest card (variant)
- `tip=6`: Manager card

**State machine (`radim` field, `frmKardPro.vb:489-509`):**
- 0: Start write
- 1: Writing in progress (progress bar)
- 9: Write success
- 10: Write complete, save to DB
- 11: Card cancelled
- 30: Error — try again
- 20: Saving

### 1.4 kard_imedia — TCP-Based iMedia Controller (Single Controller)

**Purpose:** TCP client communication with a single iMedia hotel door lock controller. Commands are sent as ASCII strings over TCP.

**Connection:**
- Connects to `Settings.ipAdres:Settings.portK` via TCP (`kard_imedia.vb:23-24`)
- Pings before connecting (`kard_imedia.vb:16`)

**Protocol — command strings:**
| Command | String | Purpose | Ref |
|---------|--------|---------|-----|
| Read outputs | `??DO` | Read digital output state | `kard_imedia.vb:293` |
| Read inputs | `??DI` | Read digital input state | `kard_imedia.vb:297` |
| Read event log | `??EL` | Read last log entry | `kard_imedia.vb:300` |
| Delete event | `??EN` | Delete last log | `kard_imedia.vb:305` |
| Read watchdog reset count | `??RW` | Watchdog reset counter | `kard_imedia.vb:309` |
| Read power-on count | `??RP` | Power-on cycle counter | `kard_imedia.vb:313` |
| Hardware reset | `??RU` | Hardware reset command | `kard_imedia.vb:317` |
| Delete last log | `!!EX` | Clear last log entry | `kard_imedia.vb:322` |
| Write time | `!!ST` + 4-byte Unix timestamp | Set controller clock | `kard_imedia.vb:340` |
| Read time | `??ST` | Read controller clock | `kard_imedia.vb:345` |
| Write card | `!!CD` + reader + slot + 3-byte hex card ID | Encode card on specific reader/slot | `kard_imedia.vb:392-417` |
| Read card | `??CD` + reader + slot | Read card from specific reader/slot | `kard_imedia.vb:418-431` |
| Set output | `!!DO` + binary state bytes | Control digital outputs (16 outputs + speed) | `kard_imedia.vb:432-486` |

**Reader addressing:** reader 1=byte 16/0x10, reader 2=64/0x40, reader 3=112/0x70, reader 4=160/0xA0 (`kard_imedia.vb:402-404`)
**Card slot addressing:** slot 1=0, slot 2=5, slot 3=10, slot 4=16, slot 5=21, slot 6=26, slot 7=32, slot 8=37, slot 9=42 (`kard_imedia.vb:403-405`)

**Response parsing (`pisiByte` response dispatch, `kard_imedia.vb:70-87`):**
| Response code | Handler | Meaning |
|---------------|---------|---------|
| `[0]=12, [1]=193/0xC1` | `odgvrijeme` | Time response |
| `[0]=12, [1]=177/0xB1` | `odgulaz` | Input state response |
| `[0]=12, [1]=178/0xB2` | `odguIzl` | Output state response |
| `[0]=12, [1]=186/0xBA` | `odguLOG` | Event log response |
| `[0]=12, [1]=189/0xBD` | `brisilog` | Delete log response |
| `[0]=12, [1]=185/0xB9` | `brLogova` | Log count response |
| `[0]=12, [1]=190/0xBE` | `vchdoog` | Watchdog count response |
| `[0]=12, [1]=191/0xBF` | `brpaljenja` | Power-on count response |
| `[0]=4, [1]=12` | `odgKard` | Card read response |

**Event log types (`odguLOG`):**
- b1=15: Card invalid → tip=14
- b1=16: Card valid → tip=7
- b1=208: SOS alarm → tip=11
- b1=240: Fire alarm → tip=12
- b1=20-23: Unauthorized maid entry (maid 1-4) → tip=15
- b1=32: Minibar → tip=13
- b1=48: Inert card → tip=17
- b1=49: Card removed → tip=18

**kardTable result structure:** Columns: `ulaz, izlaz, log, odg, kard, sat, tip` (referenced at `kard_imedia.vb:89`)

### 1.5 kard_imedia1 — Multi-Controller TCP iMedia Client

**Purpose:** Async TCP socket client supporting up to 4 iMedia controllers per facility. An evolution of kard_imedia with multi-controller addressing.

**Key differences from kard_imedia:**
- Async socket with `BeginConnect/BeginSend/BeginReceive` pattern (`kard_imedia1.vb:53-68`)
- Events: `onConnect`, `onError`, `onDataArrival`, `onDisconnect`, `onSendComplete` (`kard_imedia1.vb:14-18`)
- Controller addresses: `u1=11, u2=12(byte), u3=14, u4=15` (`kard_imedia1.vb:7-10`) — note: `u1` is decimal 11 (was byte?), `u2` is decimal 12

**Multi-controller command framing:**
All commands prefixed with `@@IU` bridge header:
- `@@IU` + controller_address + data_length + command_bytes
- Example time write: `@@IU` + `u2(12)` + `8` + `!!ST` + `4-byte-timestamp` (`kard_imedia1.vb:777-804`)
- Example card write: `@@IU` + controller_addr + `12` + `!!CD` + reader_byte + slot_byte + 3-byte-card-hex (`kard_imedia1.vb:869-922`)
- Example card read: `@@IU` + controller_addr + `6` + `??CD` + reader_byte + slot_byte (`kard_imedia1.vb:923-960`)

**Additional command variants over kard_imedia:**
- `citaj_logove()`: Sequential log reading across controllers using `citl` counter (`kard_imedia1.vb:665-676`)
- `citajLog(ure, tip)`: Read log from specific controller with `@@IU` bridge (`kard_imedia1.vb:677-706`)
- `brisiZadnjiLog(ure)`: Delete last log from specific controller (`kard_imedia1.vb:748-775`)
- `vrijemeUpisi(ure)`: Sync time to specific controller (`kard_imedia1.vb:776-805`)
- `vrijemeCitaj(ure)`: Read time from specific controller (`kard_imedia1.vb:806-832`)
- `citajIzlaz(ure)`, `citajUlaz(ure)`: Read outputs/inputs from specific controller (`kard_imedia1.vb:606-663`)
- `izlazSet(stanje, brIzlaz, brzina, ure)`: Set output on specific controller with 12-output support (`kard_imedia1.vb:961-1025`)

**Extended event types (`odguLOG`):**
- Adds SOS detection (b2=209-212), minibar (b2=33-36), fire alarm (b2=241-244), power (b2=50-53)
- Room name resolution via `ds.Tables("sobe").Select("idkon=" & kon & " and redulko=" & ulaz)` (`kard_imedia1.vb:362-368`)
- Auto time-sync: if log timestamp is not today, calls `vrijemeUpisi()` then `brisiZadnjiLog()` (`kard_imedia1.vb:457-461`)
- Sequential log reading with `citl` counter across multiple controllers (`kard_imedia1.vb:470-484`)

**Error logging:** Writes to local `Err.log` file (`kard_imedia1.vb:1026-1046`)

**Controller byte addressing:** Changes from kard_imedia: reader 2 now = 64 (was also 64), reader 3 = 112, reader 4 = 160 (`kard_imedia1.vb:883-885`)

---

## 2. Export Integration (frmExport)

**Purpose:** Financial export module for accounting integration. Generates fixed-width text files in two formats: GK (general ledger) and KIF (cash invoice fiscal).

### 2.1 Export Workflow

**Load (`frmExport_Load`, `frmExport.vb:3-21`):**
```sql
SELECT konto FROM konta WHERE idprog=0;                          -- Default account
SELECT e.id, e.dat, e.datt, e.tip FROM export e ORDER BY id;     -- Export history
```

**Date range calculation (`frmExport.vb:48-49`):**
- From: selected date 06:00
- To: selected date + 1 day, 06:00

### 2.2 Main Export — `expoFin(tip)` (`frmExport.vb:42-222`)

Three export modes:
- `tip=0`: Write to `.POI` files on disk (GK + KIF)
- `tip=1`: Output to TextBox
- `tip=2`: Output to TextBox (KIF variant)

**Database queries for financial data:**
```sql
-- Unmapped expense items requiring account mapping
SELECT p.brojracuna, datr AS datum, trosak, kol, cijbezpdv, ukupnobezpdv, pdv,
       iznospdv, ukupno, nacin, nacinid, trosakid
FROM printracuni p
JOIN printracunidetalji d ON p.brojracuna = d.brojracuna
LEFT JOIN konta k ON k.idprog = d.trosakid
WHERE dat BETWEEN {from} AND {to} AND k.konto IS NULL
GROUP BY d.trosakid                                          -- frmExport.vb:51

-- Main invoice data with account mapping
SELECT p.brojracuna, datr AS datum, trosak, kol, cijbezpdv, ukupnobezpdv, pdv,
       iznospdv, SUM(CONVERT(REPLACE(REPLACE(ukupno,'.',''),',','.'), DECIMAL(10,2))) AS ukupno,
       nacin, nacinid, trosakid, peri
FROM printracuni p
JOIN printracunidetalji d ON p.brojracuna = d.brojracuna
JOIN konta k ON k.idprog = d.trosakid
WHERE dat BETWEEN {from} AND {to} AND storno = 0
GROUP BY p.brojracuna                                        -- frmExport.vb:60

-- Partner/account info for invoice-linked payments
SELECT p.id, p.nacin, p.konto, p.broj, firma
FROM placanje p
JOIN partneri k ON k.id = p.firma
WHERE p.broj = {invoice_number}
GROUP BY p.broj                                              -- frmExport.vb:93-94

-- Payment method details
SELECT n.konto, n.partner, p.brojracuna AS broj, datr AS datum, brojsobe AS naziv,
       ime AS ImePrezime, drugoime AS NazivOstalo, trosak, kol, cijbezpdv,
       ukupnobezpdv, pdv, iznospdv,
       SUM(CONVERT(REPLACE(REPLACE(ukupno,'.',''),',','.'), DECIMAL(10,2))) AS ukupno,
       d.nacin, d.nacinid, trosakid
FROM printracuni p
JOIN printracunidetalji d ON p.brojracuna = d.brojracuna
JOIN placanjenacin n ON n.id = d.nacinid
WHERE p.brojracuna = '{invoice}' AND storno = 0
GROUP BY d.brojracuna                                        -- frmExport.vb:157

-- Complex payment for method ID 5
SELECT p.id, '{datum}' AS datum, p.rbr AS broj, p.nacin AS nacinid,
       p.iznos AS ukupno, n.konto, n.partner, n.nacin
FROM placanjeslozeno p
JOIN placanjenacin n ON n.id = p.nacin
WHERE p.rbr = {broj}                                         -- frmExport.vb:159
```

### 2.3 GK File Format — `dodajGk()` (`frmExport.vb:223-271`)

Fixed-width positional format:
| Position | Length | Field |
|----------|--------|-------|
| 1-12 | 12 | Account code (konto) |
| 13-27 | 15 | P+amount (credit) with comma decimal |
| 28-35 | 8 | Business partner code |
| 36-43 | 8 | Organizational unit |
| 44-63 | 20 | Document ref (F=invoice/R=receipt + number) |
| 64-71 | 8 | Date YYYYMMDD |
| 72-91 | 20 | Description (expense name, cleaned) |
| 92-103 | 12 | (reserved) |

### 2.4 GK VAT Line — `dodajGkpdv()` (`frmExport.vb:272-319`)

Same format, account fixed to `473000` (VAT payable), description "PDV 17%".

### 2.5 Payment Line — `dodahGkplac()` (`frmExport.vb:320-363`)

Same format, D+amount (debit), partner from payment method or invoice.

### 2.6 KIF File Format — `dodajKIF()` (`frmExport.vb:364-407`)

| Position | Length | Field |
|----------|--------|-------|
| 1-20 | 20 | Document ref (R/F + number) |
| 21-45 | 25 | Date+Date+Partner |
| 46-75 | 30 | Org unit |
| 76-90 | 15 | Total amount (+ prefix) |
| 91-105 | 15 | Amount without VAT (where pdv>0) |
| 106-120 | 15 | VAT amount |
| 121-135 | 15 | Insurance amount (trosakid=99998) |
| 136-150 | 15 | Tourist tax (trosakid=99999) |

### 2.7 Account Mapping Tabs

**Payment methods tab (`TabPage2_Enter`, `frmExport.vb:509-521`):**
```sql
SELECT p.id, p.nacin, p.konto, partner FROM placanjenacin p WHERE p.id <> 5;   -- Payment methods
SELECT t.id, t.naziv, k.konto FROM troskovivrste t LEFT JOIN konta k ON k.idprog = t.id;  -- Expense types
```

**Partners tab (`TabPage3_Enter`, `frmExport.vb:523-529`):**
```sql
SELECT id, naziv, sifraex FROM partneri WHERE del = 0;   -- Export partner codes
```

**Account mapping updates (inline SQL):**
```sql
UPDATE konta SET konto = '{value}' WHERE idprog = '{id}';                  -- frmExport.vb:559
INSERT INTO konta (konto, idprog) VALUES ('{value}', '{id}');             -- frmExport.vb:561
UPDATE placanjenacin SET konto = '{value}', partner = '{value}' WHERE id = '{id}';  -- frmExport.vb:573
UPDATE partneri SET sifraex = '{value}' WHERE id = '{id}';                -- frmExport.vb:542
INSERT INTO export (dat, datt, tip) VALUES ('{date}', '{now}', 1);        -- frmExport.vb:65,211
UPDATE printracuni SET exp = 3 WHERE exp = 2;                              -- frmExport.vb:609
```

### 2.8 Critical Issues

- **SQL injection**: All queries use string concatenation, no parameterized queries (`frmExport.vb:51,60,65,88-89,93,157,159,211,542,553,559,561,573,580-585,609`)
- **Decimal parsing**: `CONVERT(REPLACE(REPLACE(ukupno,'.',''),',','.'), DECIMAL(10,2))` — stores monetary values as strings with locale-dependent decimal separators (`frmExport.vb:60,157`)
- **Hardcoded VAT rate**: "PDV 17%" and account "473000" are hardcoded (`frmExport.vb:277,311`)
- **File path**: `GK{date}.POI` and `KIF{date}.POI` in user-chosen folder (`frmExport.vb:81-82`)
- **Export tracking**: inserts into `export` table with date, completion timestamp, and type=1 (`frmExport.vb:65,211`)
- **Storno handling**: `exp=2` marks cancelled invoices for review; `exp=3` marks them as processed (`frmExport.vb:595-611`)

---

## 3. Email System (frmMail, frmMailKonfig)

### 3.1 frmMail — Tourist Registration Email Sender

**Purpose:** Generates a tourist registration PDF report and emails it to a government authority. Uses Crystal Reports for PDF generation.

**Workflow (`btnMail_Click`, `frmMail.vb:17-62`):**
1. Update `relgostsoba.redniBroj` with the row number from form (`frmMail.vb:31`)
2. Regenerate tourist list dataset `GostiListaTurist` (`frmMail.vb:49`)
3. Generate PDF via Crystal Reports `PrijavaTurist` report → `C:\Program Files\IMEDIA\HotelPRO\TuristickiIzvjestaj.pdf` (`frmMail.vb:53-57`)
4. Send email with PDF attachment via SMTP (`frmMail.vb:60`)

**SQL queries:**
```sql
UPDATE relgostsoba SET redniBroj = {number} WHERE ID = {id};               -- frmMail.vb:31

-- After update, regenerate tourist list:
SELECT relgostsoba.ID, relgostsoba.gostID, relgostsoba.PID, sobe.naziv,
       gosti.ime, gosti.prezime, relgostsoba.checkInDate, relgostsoba.checkOutDate,
       gosti.datumRodjenja, gosti.mjestodrzavaR, gosti.drzavljanstvo,
       gostdokument.naziv, gosti.brDokument, relgostsoba.redniBroj
FROM relgostsoba
INNER JOIN gosti ON relgostsoba.gostID = gosti.ID
INNER JOIN sobe ON relgostsoba.sobaID = sobe.ID
INNER JOIN gostdokument ON gosti.dokument = gostdokument.ID
WHERE relgostsoba.ID = {id};                                                -- frmMail.vb:199-200

SELECT MAX(relgostsoba.redniBroj) AS RB FROM relgostsoba;                  -- frmMail.vb:236
```

**Email sending (`StavljanjeAttacha`, `frmMail.vb:64-111`):**
- Reads SMTP config from `ds.Tables("mailkonfig")`: `odkogaMail`, `pass`, `smtp`, `port`, `ssl` (`frmMail.vb:72-76`)
- Uses `System.Net.Mail.SmtpClient` with optional SSL (`frmMail.vb:92-98`)
- PDF path hardcoded: `C:\Program Files\IMEDIA\HotelPRO\TuristickiIzvjestaj.pdf` (`frmMail.vb:56,88`)

**Default settings query:**
```sql
SELECT mailkonfig.odkogaMail, mailkonfig.pass, mailkonfig.smtp,
       mailkonfig.port, mailkonfig.ssl, mailkonfig.komeMail,
       mailkonfig.subjekt, mailkonfig.tijelo
FROM mailkonfig;                                                             -- frmMail.vb:162
```

### 3.2 frmMailKonfig — Mail Configuration Editor

**Purpose:** Edit SMTP settings stored in the `mailkonfig` database table.

**SQL (all unparameterized, concatenation-based):**
```sql
UPDATE mailkonfig SET odkogaMail = {value}, pass = {value}, smtp = {value}, port = {value}, ssl = {value};   -- frmMailKonfig.vb:48-49
UPDATE mailkonfig SET komeMail = {value}, subjekt = {value}, tijelo = {value};                                -- frmMailKonfig.vb:100-101
```

**Critical issues:**
- SQL injection vulnerabilities in both forms (`frmMail.vb:31,frmMailKonfig.vb:48,100`)
- Hardcoded PDF output path (`frmMail.vb:56`)
- SMTP password stored in plaintext in database (`frmMail.vb:73`)
- Crystal Reports dependency for PDF generation (`frmMail.vb:3-4`)

---

## 4. Journal/Audit (frmZurnal1)

**Purpose:** A journal/occupancy calendar form showing a DataGridView grid with rooms as rows and dates as columns. Displays reservations, daily payments, and expenses in a timeline view.

### 4.1 Data Loading

**Room data (`frmZurnal1.vb:12`):**
```sql
SELECT sobe.id, sobe.naziv, sobavrsta.naziv AS vrstanaziv, vrsta, lokal,
       ooo, razlog, zgradaID, clean, tekst, idvrsta1, sprat
FROM sobe LEFT JOIN sobavrsta ON sobavrsta.id = sobe.vrsta;
```

**Active guest stays (`frmZurnal1.vb:89`):**
```sql
SELECT g.ime, g.prezime, g.drzavljanstvo, r.ID, r.gostID, r.sobaID,
       r.checkInDate, r.checkOutDate, r.checkInRadnik, r.checkOutRadnik,
       r.stampanaPrijava, r.odjavljen, r.rezervacija, r.grupaID,
       r.brojDana, r.tarifaID, r.popust, r.ostaliTroskovi, r.PID,
       r.print1, r.print2, r.rezervP, r.redniBroj, r.PopustRazlog, r.pl,
       s.naziv, s.vrsta, s.lokal, s.ooo, s.razlog, s.zgradaID,
       s.clean, s.tekst, s.idvrsta1, 0 AS tip
FROM sobe s
JOIN relgostsoba r ON s.id = r.sobaid
JOIN gosti g ON g.id = r.gostid
WHERE r.odjavljen = 0;
```

**Future reservations (`frmZurnal1.vb:90-91`):**
```sql
SELECT r.gost AS ime, '-' AS prezime, '-' AS drzavljanstvo, s.id,
       r.gid AS gostID, r.sid AS sobaID, checkInDate, checkOutDate,
       s.radnikid AS checkInRadnik, 0 AS checkOutRadnik, ...
FROM rezervacije s
INNER JOIN rezervacijasobe r ON r.rezid = s.id
WHERE stornirana = '0' AND checkInDate > '{from}' AND checkInDate < '{to}';
```

**Daily payments (`frmZurnal1.vb:541-544`):**
```sql
-- Stored procedure: getIzvjestajDnevniPlacanje  (with date parameters)
```

**Avance data (`frmZurnal1.vb:681-704`):**
```sql
SELECT BrojRacuna AS broj, datu AS datum, 'Avans' AS naziv,
       ime AS ImePrezime, DrugoIme AS NazivOstalo, ukupno AS iznos,
       TipPlacanja AS nacin, @datumOD, @datumDO, storno
FROM printracuniavans
WHERE datu BETWEEN @datumOD AND @datumDO AND storno = 0;
```

**Expenses (`frmZurnal1.vb:321,512,584`):**
```sql
SELECT tv.naziv, t.ID, t.GSID, t.SID, t.TID, t.vrijeme, t.kolicina,
       t.iznos, t.radnikID, t.napomena, t.zaklj, t.Brrac, t.Djelimicno
FROM troskovi t
INNER JOIN troskovivrste tv ON tv.id = t.tid
WHERE zaklj = '0' AND sid > 0
ORDER BY vrijeme;
```

**Occupancy with room types (`frmZurnal1.vb:718`):**
```sql
SELECT g.ime, g.prezime, s.naziv, s.id, r.gostID, r.sobaID, ...,
       sv.naziv AS vrstanaziv
FROM sobe s
JOIN relgostsoba r ON s.id = r.sobaid
JOIN gosti g ON g.id = r.gostid
LEFT JOIN sobavrsta sv ON sv.id = s.vrsta
WHERE r.odjavljen = 0;
```

### 4.2 Reservation Creation via Cell Edit

When a user edits a cell in the calendar grid (`dgv_CellValueChanged`, `frmZurnal1.vb:404-445`):
```sql
INSERT INTO rezervacije (GID, checkInDate, checkOutDate, potvrda, brojPotvrde,
    blokID, tipID, izvorID, sobaVrstaID, stornirana, brojStorna, brojRezSoba,
    godina, prijava, tarifa, memo, radnik, radnikID, vrijeme, idd, gost, tex,
    napomena, alarmid, gostgrupa, promjena, promjenat, kontakt, kontakttel,
    kontaktfax, kontaktmob, kontaktmail, plac, placanje, firma, firmaid,
    agencija, komerc, agencijaid, komercid, brojsoba, brdjeca)
VALUES ('0', '{date}', '{date}', '0', '0', '1', '1', '0', '0', '0', '0', '1',
    '{year}', '0', '0', '{value}', '{RIme}', '{RID}', '{now}', '{idd}', '-', '-',
    '-', '0', '-', '0', '-', '-', '-', '-', '-', '-', '0', '0', '-', '0', '-',
    '-', '0', '0', '1', '0');
                                                                          -- frmZurnal1.vb:428

INSERT INTO rezervacijasobe (rezid, sobtid, sobatip, sid, soba, tid, tarifa,
    gid, gost, idd, promjena, pom, pom1, pusac, brgost, gost1, cjenovnik)
VALUES ('{rezid}', '{vrsta}', '{vrstanaziv}', '{sobeid}', '{sobaname}', '0',
    '0', '0', '0', '{idd}', '-', '0', '0', '0', '1', '0', '0');
                                                                          -- frmZurnal1.vb:440
```

### 4.3 Room Status Codes (referenced via `otvoriForm`)

| Code | Meaning | `frmZurnal1.vb:259-287` |
|------|---------|--------------------------|
| 0 | SLOBODNA (Free) | line 260 |
| 1,2 | ZAUZETA (Occupied) | line 263 |
| 3 | REZERVISANA - potvrdjeno (Reserved-confirmed) | line 266 |
| 4 | ZAUZETA i REZERVISANA (Occupied+Reserved) | line 270 |
| 5 | VAN UPOTREBE (Out of service) | line 273 |
| 6 | REZERVISANA - nepotvrdjeno (Reserved-unconfirmed) | line 276 |
| 7 | NIJE SPREMNA (Not ready) | line 280 |

---

## 5. SQL Inventory (all SQL from these files)

### classKard.vb
- No active SQL (commented-out stored proc call `imedia.updateSobaClean`, `classKard.vb:306-329`)

### frmKardPro.vb
```sql
INSERT INTO kard (kard, tip, soba, vazido, vrijeme, rad, sobaid, ime, del)
VALUES ('{kardid}', '{tip}', '{soba}', '{doo}', '{Now}', '{RIme}', '{sobaid}', '{ime}', 0);
                                                                              -- frmKardPro.vb:537
```

### kard_imedia1.vb
```sql
SELECT idkon, redulko, naziv FROM sobe WHERE idkon={kon} AND redulko={ulaz};  -- kard_imedia1.vb:362
```
References `ds.Tables("sobe")` and `ds.Tables("kontr")` for controller/log iteration.

### frmExport.vb
All queries documented in Section 2 above. Key tables:
- `konta` — account mapping (idprog, konto)
- `export` — export history (id, dat, datt, tip)
- `printracuni` — invoices (brojracuna, dat, Ime, DrugoIme, ukupno, storno, exp, nacinid, trošakid...)
- `printracunidetalji` — invoice line items (brojracuna, trosak, kol, cijbezpdv, ukupnobezpdv, pdv, iznospdv, ukupno, nacinid, trosakid)
- `placanjenacin` — payment methods (id, nacin, konto, partner)
- `placanjeslozeno` — complex payment splits (id, rbr, nacin, iznos)
- `partneri` — business partners (id, naziv, sifraex, mjesto, posta, ulica, zemlja, telefon, pdv, idd)
- `troskovivrste` — expense types (id, naziv)

### frmMail.vb
All queries documented in Section 3 above. Key tables:
- `relgostsoba` — guest-room assignments (ID, gostID, PID, sobaID, checkInDate, checkOutDate, redniBroj)
- `gosti` — guests (ID, ime, prezime, datumRodjenja, mjestodrzavaR, drzavljanstvo, brDokument, dokument)
- `sobe` — rooms (id, naziv)
- `gostdokument` — document types (id, naziv)
- `mailkonfig` — email configuration (odkogaMail, pass, smtp, port, ssl, komeMail, subjekt, tijelo)

### frmMailKonfig.vb
- UPDATE/INSERT on `mailkonfig` table (see Section 3.2)

### frmZurnal1.vb
All queries documented in Section 4 above. Key tables:
- `sobe` — rooms (id, naziv, vrsta, lokal, ooo, razlog, zgradaID, clean, tekst, idvrsta1, sprat)
- `sobavrsta` — room types (id, naziv)
- `relgostsoba` — guest-room assignments
- `gosti` — guests
- `rezervacije` — reservations (with ~30+ columns)
- `rezervacijasobe` — reservation room assignments
- `troskovi` — expenses (ID, GSID, SID, TID, vrijeme, kolicina, iznos, radnikID, napomena, zaklj, Brrac, Djelimicno)
- `troskovivrste` — expense types
- `printracuniavans` — advance payments (BrojRacuna, datu, Ime, DrugoIme, ukupno, TipPlacanja, storno)
- Stored proc: `getIzvjestajDnevniPlacanje` (date range)
- Stored proc: `getRezrervacijePrikazi` (date range)

---

## 6. Key Findings for Modern System

### 6.1 Card/RFID System Architecture

**Three distinct hardware communication channels must be supported:**
1. **USB HID (Microchip PIC)** — Direct USB device at VID 04d8/PID 002f using setupapi.dll + kernel32.dll for plug-and-play HID communication. 65-byte packet protocol with sector-based read/write (sectors 0-8). Used by frmKardRw/frmKardPro for direct USB card encoders.
2. **TCP iMedia (single controller)** — Synchronous TCP client in kard_imedia, sends ASCII command strings (??/!!) prefix. Direct IP connection to `Settings.ipAdres:Settings.portK`.
3. **TCP iMedia (multi-controller)** — Async TCP socket in kard_imedia1 with `@@IU` bridge framing for addressing up to 4 controllers (addresses 11/12/14/15). Supports event log polling, time sync, input/output monitoring, SOS/fire/minibar alarms.

**Card encoding data model (USB HID):**
- Card type: G=guest, H=maid, M=manager, S=service
- Duration: date+time encoding (DDMMYYHHMM)
- Room: 5-digit padded number
- Guest name: surname (17 chars max) + first name (17 chars max), diacritics stripped
- Invalid flag, language code, logo code, gender, object ID
- Card bar variant adds: card form (C/W/Q), one-time/temporary flag

**Card encoding data model (iMedia TCP):**
- 3-byte hex card ID written to specific reader/slot
- Reader addressing: values 16/64/112/160 (kard_imedia) or 16/64/112/160 (kard_imedia1 corrected)
- Slot addressing: 0/5/10/16/21/26/32/37/42
- Supports up to 4 controllers with bridge addressing

**Modern system must abstract:**
- A unified `ICardEncoder` interface supporting both USB HID and TCP iMedia protocols
- Card write/read operations decoupled from hotel business logic
- Event log polling service (SOS, fire, minibar, unauthorized access, card valid/invalid)
- Controller management (time sync, output control, watchdog/power counters)

### 6.2 Export System

- Fixed-width positional text file format for accounting integration (GK + KIF)
- Account mapping tables `konta` (expense types → GL accounts) and `placanjenacin` (payment methods → GL accounts)
- Partner export codes stored in `partneri.sifraex`
- Export tracking in `export` table
- Severe SQL injection risk throughout — must use parameterized queries
- Hardcoded VAT rate (17%) and VAT account (473000)
- Storno (cancelled invoice) handling: `exp` field on `printracuni` (0=pending, 2=cancelled, 3=processed)

### 6.3 Email System

- Crystal Reports dependency for PDF generation must be replaced
- SMTP configuration stored in `mailkonfig` table with plaintext passwords
- Hardcoded file path `C:\Program Files\IMEDIA\HotelPRO\TuristickiIzvjestaj.pdf`
- Tourist registration report relevant for government compliance (e-police reporting)

### 6.4 Journal/Calendar

- Dynamic DataGridView with room rows × date columns
- Reservation creation via cell editing with inline SQL insert
- Supports reservation status coloring (occupied=red, reserved=yellow, expense=azure)
- Night calculation via `vratiCijenunocenja()` and `racunuku()` functions (external dependencies)
- Advance payments from `printracuniavans` table

### 6.5 Cross-Cutting Concerns

1. **Security**: SQL injection in every module — use parameterized queries exclusively
2. **Internationalization**: Serbian/Croatian diacritics (ČĆŽŠĐ) stripped before card encoding (`frmKardPro.vb:1016-1027`) — modern system should handle Unicode properly or document transformation
3. **Hardcoded values**: VAT rate, IP addresses, file paths, account numbers — all need configuration
4. **Error handling**: Silent exception swallowing throughout — need proper logging
5. **Database tables referenced**: `sobe, gosti, relgostsoba, rezervacije, rezervacijasobe, troskovi, troskovivrste, printracuni, printracunidetalji, placanjenacin, placanjeslozeno, partneri, konta, export, kard, mailkonfig, gostdokument, printracuniavans, radnici, sobavrsta, kontr`
6. **Stored procedures**: `getIzvjestajDnevniPlacanje`, `getRezrervacijePrikazi`, `imedia.updateSobaClean` (commented out)
7. **Global state**: Heavy reliance on module-level `ds` (global DataSet), `Settings.ipAdres`, `Settings.portK`, `RIme`, `RID` (worker name/ID), `ConnStr`
8. **Controller table**: `ds.Tables("kontr")` used for multi-controller log iteration (`kard_imedia1.vb:475`)