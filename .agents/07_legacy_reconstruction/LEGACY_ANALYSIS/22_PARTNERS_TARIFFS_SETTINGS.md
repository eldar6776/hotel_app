# Legacy Analysis: Partners, Tariffs, Settings & Database Administration

## 1. Partners/Agencies (Structure, CRUD, SQL)

### 1.1 Two Duplicate Partner Forms

The system has **two near-identical** partner management forms:

- **frmPartner1.vb** — Full CRUD with in-memory DataSet + search/filter (`legacy_code/frmPartner1.vb`)
- **frmPartneri.vb** — Same CRUD, direct DB query without local DataSet copy (`legacy_code/frmPartneri.vb`)

### 1.2 Partner Table: `partneri`

Inferred schema from field mappings (`legacy_code/frmPartner1.vb:37-59`, `legacy_code/frmPartneri.vb:242-268`):

| Column | Type | Maps to TextBox | Notes |
|--------|------|-----------------|-------|
| id | INT (auto) | TextBoxx2 / TextBoxX2 | PK, displayed but not user-editable |
| naziv | VARCHAR | TextBoxx3 / TextBoxX3 | Name/company name |
| mjesto | VARCHAR | TextBoxx4 / TextBoxX4 | City |
| posta | VARCHAR | TextBoxx5 / TextBoxX5 | Postal code |
| ulica | VARCHAR | TextBoxx6 / TextBoxX6 | Street address |
| zemlja | VARCHAR | TextBoxx7 / TextBoxX7 | Country |
| telefon | VARCHAR | TextBoxx8 / TextBoxX8 | Phone |
| fax | VARCHAR | TextBoxx9 / TextBoxX9 | Fax |
| email | VARCHAR | TextBoxx10 / TextBoxX10 | Email |
| www | VARCHAR | TextBoxx11 / TextBoxX11 | Website |
| zrac1 | VARCHAR | TextBoxx12 / TextBoxX12 | Airport code 1 |
| zrac2 | VARCHAR | TextBoxx13 / TextBoxX13 | Airport code 2 |
| zrac3 | VARCHAR | TextBoxx14 / TextBoxX14 | Airport code 3 |
| zrac4 | VARCHAR | TextBoxx15 / TextBoxX15 | Airport code 4 |
| pdv | VARCHAR | TextBoxx16 / TextBoxX16 | VAT number |
| idd | VARCHAR | TextBoxx17 / TextBoxX17 | ID number |
| rjesenje | VARCHAR | TextBoxx18 / TextBoxX18 | Resolution/decision number |
| kosoba | VARCHAR | TextBoxx19 / TextBoxX19 | Contact person |
| rabat | INT (Int16) | TextBoxx20 / TextBoxX20 | Discount percentage |
| brdanodg | INT (Int16) | TextBoxx21 / TextBoxX21 | Days until payment deadline |
| vr_upis | DATETIME | auto (Now) | Record creation timestamp |
| filter | VARCHAR | TextBoxx22 / TextBoxX22 | Filter/category tag |
| napomena | VARCHAR | TextBoxx24 / TextBoxX24 | Notes |
| del | INT (boolean) | — | Soft-delete flag |

### 1.3 Partner CRUD Operations

**INSERT** (`legacy_code/frmPartner1.vb:151-193`, `legacy_code/frmPartneri.vb:40-82`):
```sql
INSERT INTO partneri (naziv,mjesto,posta,ulica,zemlja,telefon,fax,email,www,zrac1,zrac2,zrac3,zrac4,pdv,idd,rjesenje,kosoba,rabat,brdanodg,vr_upis,filter,napomena)
VALUES (@naziv,@mjesto,@posta,@ulica,@zemlja,@telefon,@fax,@email,@www,@zrac1,@zrac2,@zrac3,@zrac4,@pdv,@idd,@rjesenje,@kosoba,@rabat,@brdanodg,@vr_upis,@filter,@napomena)
```
- Uses parameterized queries (good)
- `vr_upis` set to `Now()` automatically
- No `id` in INSERT → auto-increment

**UPDATE** (`legacy_code/frmPartner1.vb:106-149`, `legacy_code/frmPartneri.vb:103-146`):
```sql
UPDATE partneri SET naziv=@naziv,mjesto=@mjesto,posta=@posta,ulica=@ulica,zemlja=@zemlja,telefon=@telefon,fax=@fax,email=@email,www=@www,zrac1=@zrac1,zrac2=@zrac2,zrac3=@zrac3,zrac4=@zrac4,pdv=@pdv,idd=@idd,rjesenje=@rjesenje,kosoba=@kosoba,rabat=@rabat,brdanodg=@brdanodg,filter=@filter,napomena=@napomena WHERE id=@id
```

**DELETE (soft)** (`legacy_code/frmPartner1.vb:364`):
```sql
update partneri set del=1 where id='{id}'
```

**RESTORE** (`legacy_code/frmPartneri.vb:329`):
```sql
update partneri set del=0 where id='{id}'
```

**SELECT** (`legacy_code/frmPartneri.vb:87`):
```sql
select id,naziv,mjesto,posta,ulica,zemlja,telefon,fax,email,www,zrac1,zrac2,zrac3,zrac4,pdv,idd,rjesenje,kosoba,rabat,brdanodg,vr_upis,filter,napomena from partneri where del=0
```

**FILTER (client-side)** (`legacy_code/frmPartner1.vb:353`):
```vb
dtp.DefaultView.RowFilter = "naziv like '%" & TextBoxX25.Text & "%'"
```
- Note: SQL injection risk in filter string concatenation

### 1.4 Partner Selection Pattern

Both forms serve a **dual purpose**: browse/edit AND pick a partner. When "OK" is clicked (`legacy_code/frmPartneri.vb:281-297`, `legacy_code/frmPartner1.vb:306-321`):
- Sets global variables: `akcij` (name), `akcij1` (street), `akcij2` (PDV or IDD), `digi` (ID)
- frmPartneri also sets `Me.Tag = TextBoxX2.Text`
- Uses PDV if available, falls back to IDD

### 1.5 Key Partner Issues
- **Code duplication**: frmPartner1 and frmPartneri are 80%+ identical
- **Soft delete**: del=1/0 pattern, never hard-deletes
- **Global variable coupling**: akcij, akcij1, akcij2, digi — module-level shared state
- **No validation**: email, web, phone format not validated
- **rabat/brdanodg**: numeric check is `If Not IsNumeric() Then =0`, no range validation
- **SQL injection in soft-delete**: `where id='" & TextBoxx2.Text.Trim & "'"` — string concat for numeric ID

---

## 2. Tariffs and Discounts (Structure, CRUD, SQL)

### 2.1 Tariff Form: frmTarife

(`legacy_code/frmTarife.vb` — 680 lines)

### 2.2 Tariff Tables

**Table: `sobatarifa`** (room tariffs)

From SELECT at `legacy_code/frmTarife.vb:653`:
```sql
SELECT s.`ID`, s.`naziv`, s.`naziv2`, s.`del` FROM sobatarifa s
```

| Column | Type | Purpose |
|--------|------|---------|
| ID | INT AUTO_INCREMENT | PK |
| naziv | DECIMAL | Tariff price/amount |
| naziv2 | VARCHAR | Tariff name/label |
| del | BOOLEAN | Soft-delete flag |

**Table: `relsobavrstaobatarifa`** (relationship: tariff ↔ room types)

Stored procedure `unesiRelSobaVrstaSobaTarifa` with parameters:
- `@ttarifaID` — references sobatarifa.ID
- `@sSobaVrstaID` — references room type (sobavrsta) ID

The form has hardcoded checkbox mappings for up to **16 room types** (`legacy_code/frmTarife.vb:23-475`):

| Checkbox | sobavrstaID | Likely meaning |
|----------|-------------|----------------|
| chkJ | 1 | Jednokrevetna (single) |
| chkD | 2 | Dvokrevetna (double) |
| chkFL | 3 | Francuski ležaj |
| chkA | 4 | Apartman |
| chkJ1 | 5 | Jednokrevetna 2 |
| chkD1 | 6 | Dvokrevetna 2 |
| chkFL1 | 7 | Francuski ležaj 2 |
| chkA1 | 8 | Apartman 2 |
| chk10 | 9–13 | Additional room types |
| chk14–17 | 14–16 | Additional room types |

**Table: `goststatus`** (guest status / tax tariffs, managed in frmpostavke)

From `legacy_code/frmpostavke.vb:93`:
```sql
SELECT g.`id`, g.`naziv`, g.`taksa`, g.`del` FROM goststatus g
```

| Column | Type | Purpose |
|--------|------|---------|
| id | INT AUTO_INCREMENT | PK |
| naziv | VARCHAR | Status name |
| taksa | DECIMAL | Tax amount |
| del | BOOLEAN | Soft-delete flag |

### 2.3 Tariff CRUD Operations

**INSERT tariff** via stored procedure at `legacy_code/frmTarife.vb:580-619`:
```sql
CALL unesiSobaTarifa(@nazivv, @naziv2, @usl, @ID OUTPUT)
```
- `@nazivv` = DECIMAL (price)
- `@naziv2` = VARCHAR (tariff name)
- `@usl` = always 0
- Returns new ID via `@ID` (ParameterDirection.ReturnValue)

**UPDATE tariff** inline at `legacy_code/frmTarife.vb:667`:
```sql
update sobatarifa set del={del}, naziv='{price}', naziv2='{name}' where id='{id}'
```
- Note: direct string concatenation — SQL injection risk

**INSERT tariff-roomtype link** via stored procedure:
```sql
CALL unesiRelSobaVrstaSobaTarifa(@ttarifaID, @sSobaVrstaID)
```
- Called once per checked room type (up to 16 times per new tariff)

**UPDATE guest status/tax** at `legacy_code/frmpostavke.vb:1798`:
```sql
update goststatus set taksa='{price}', del={flag} where id={id}
```

**INSERT guest status/tax** at `legacy_code/frmpostavke.vb:1536`:
```sql
insert into tarifatxe(naziv,iznos) values('naziv',1)
```

**Soft-delete guest status/tax** at `legacy_code/frmpostavke.vb:1542`:
```sql
update tarifatxe set del=1 where id='{id}'
```

**UPDATE tarifatxe** at `legacy_code/frmpostavke.vb:1548`:
```sql
update tarifatxe set naziv='{name}', iznos='{amount}' where id='{id}'
```

### 2.4 Room Types (sobavrsta / sobavrsta1)

**Table: `sobavrsta`** — managed via DataGrid in frmpostavke:

```sql
SELECT ID, naziv, brojKreveta, defaultTarifa FROM sobavrsta
```
(`legacy_code/frmpostavke.vb:1169`)

| Column | Type | Purpose |
|--------|------|---------|
| ID | INT | PK |
| naziv | VARCHAR | Room type name |
| brojKreveta | INT | Number of beds |
| defaultTarifa | INT | Default tariff FK |

**Add room type** (`legacy_code/frmpostavke.vb:1197`):
```sql
insert into sobavrsta (id,naziv,brojKreveta,defaultTarifa) SELECT max(ID)+1,'-','1','1' FROM sobavrsta
```

**Delete room type** (`legacy_code/frmpostavke.vb:1204`):
```sql
delete sobavrsta from sobavrsta where id = {id}
```

**Update room type** (`legacy_code/frmpostavke.vb:1215`):
```sql
update sobavrsta set naziv='{name}', brojKreveta='{beds}', defaultTarifa='{tariff}' where id = {id}
```

**Table: `sobavrsta1`** — secondary room type classification:

```sql
select id1, id, naziv from sobavrsta1
```
(`legacy_code/frmpostavke.vb:281`)

| Column | Type | Purpose |
|--------|------|---------|
| id1 | INT | PK (auto) |
| id | INT | Room type ID (FK?) |
| naziv | VARCHAR | Name |

**INSERT sobavrsta1** (`legacy_code/frmpostavke.vb:646`):
```sql
insert into sobavrsta1 (id, naziv) values ({id},'{naziv}')
```

**UPDATE sobavrsta1** (`legacy_code/frmpostavke.vb:649`):
```sql
UPDATE sobavrsta1 SET id={id}, naziv='{naziv}' where id1={id1}
```

**DELETE sobavrsta1** (`legacy_code/frmpostavke.vb:679`):
```sql
delete sobavrsta1 from sobavrsta1 where id1 = {id1}
```

### 2.5 Key Tariff Issues
- **16 copy-pasted blocks** for checkbox→roomtype mapping (`legacy_code/frmTarife.vb:18-576`) — should be a loop over dynamically-fetched room types
- **No audit trail** on tariff changes except `funkcije.logs()` call on update/save
- **Mixed inline SQL & stored procedures** — inconsistency
- **Decimal comma/period handling**: `.Replace(".", "").Replace(",", ".")` for price — fragile locale-dependent

---

## 3. Login and Users (Authentication, Roles)

### 3.1 Login Form: frmLogin

(`legacy_code/frmLogin.vb` — 270 lines)

### 3.2 User Table: `radnici`

From login query at `legacy_code/frmLogin.vb:25`:
```sql
SELECT * from radnici
```

Fields accessed (`legacy_code/frmLogin.vb:53-81`):

| Column | Type | Purpose |
|--------|------|---------|
| ID | INT | PK |
| username | VARCHAR | Login username |
| password | VARCHAR | Login password |
| ime | VARCHAR | Full name |
| nivo | INT | Access level (role) |
| jmbg | VARCHAR | Used as permission bitmask (13+ chars) |
| printn | VARCHAR | Print name |
| disabled | INT | Whether user is active |

### 3.3 Authentication Flow

1. **All employees loaded** on login (`legacy_code/frmLogin.vb:25`):
   ```sql
   SELECT * from radnici
   ```
2. **Plaintext password comparison** at `legacy_code/frmLogin.vb:54`:
   ```vb
   If CType(ds.Tables("listingRadnici").Rows(i)("password"), String) = Me.txtLozinka.Text
   ```
   - Passwords stored in **cleartext** — no hashing, no encryption
   - Compared by exact string match — case-sensitive

3. **Permission bitmask** from `jmbg` field at `legacy_code/frmLogin.vb:55`:
   ```vb
   dozvole = CStr(ds.Tables("listingRadnici").Rows(i)("jmbg"))
   ```
   - If `dozvole.Length < 7` → defaults to `"0000000000000"` (no permissions)
   - Individual characters checked as flags: e.g. `dozvole(1).ToString = "0"` means no access

4. **Role levels** at `legacy_code/frmLogin.vb:158`:
   - `nivo = 3` → limited access (reservation-only user)
   - Other nivo values → full access (toolbar buttons enabled)
   - Toolbar buttons 2, 4, 6, 8, 12, 14, 16 are disabled for nivo=3

5. **Mission logging** via `funkcije.prijaviRadnika(RID)` at `legacy_code/frmLogin.vb:144`

6. **Fiscal device integration**: If fiscal type = 4, calls `upisi_radnikeTermol(RID, RIme)` at `legacy_code/frmLogin.vb:183`

### 3.4 Global State After Login

Set at `legacy_code/frmLogin.vb:145-153`:
- `RID` = employee ID
- `RIme` = employee name
- `frmGlavni.sbpRadnik.Text` = "Ime radnika: {name}"
- `frmGlavni.sbRadnikID.Text` = employee ID
- `frmGlavni.sbpStart.Text` = shift start time
- `frmGlavni.sbNivo.Text` = access level

### 3.5 Worker Management

`radnici` table queried in frmpostavke (`legacy_code/frmpostavke.vb:1149`):
```sql
SELECT ID, ime, adresa, telefon, username, disabled, nivo, printn FROM radnici WHERE disabled=0 AND nivo<9
```

Double-click opens `frmRadnik` for editing (`legacy_code/frmpostavke.vb:1648-1654`).

### 3.6 Backdoor / Admin Access

`legacy_code/frmLogin.vb:211-218`: Ctrl+F1 opens database password form (`frmBazaPas`), then settings form. This is a **hidden admin entry point** bypassing normal login.

### 3.7 Key Login Issues
- **Cleartext passwords** stored and compared
- **No password hashing** whatsoever
- **Permission system** uses the `jmbg` (national ID) field as a bitmask — misuse of personal data
- **No brute-force protection**: No lockout, no delay on failed login
- **Hidden admin backdoor**: Ctrl+F1 → frmBazaPas → frmpostavke without proper auth
- **Full table scan** on every login: loads ALL radnici instead of parameterized WHERE clause
- **No session management**: logged-in state is just global variables

---

## 4. Settings/Postavke (Configuration Parameters)

### 4.1 Settings Table: `setings`

Read at `legacy_code/frmpostavke.vb:21-83` — single-row configuration table.

| Column | Type | Purpose | Example/Notes |
|--------|------|---------|---------------|
| sobekuc | VARCHAR | Home/check-in controller config | `"{ip}#{port}#{path}#{id}#0#{user}#{book}#{flag}#"` — `#`-delimited |
| keyk | VARCHAR | License/encryption key | Used in `provj()` function |
| stan | INT | Station number | `stanica` global |
| pdv | DECIMAL | VAT rate | Main PDV percentage |
| pdvtr | DECIMAL | VAT on tourist tax | |
| pdvo | DECIMAL | Reduced VAT rate | |
| pdvtax | DECIMAL | VAT on tax | |
| osig | DECIMAL | Insurance amount | |
| taxa | DECIMAL | Tax amount per person | |
| cultur | VARCHAR | Culture/locale | e.g., "hr-HR" |
| dijecagod | DECIMAL | Children's age threshold | |
| dijecapop | DECIMAL | Children's discount percentage | |
| naplposo | INT | Billing mode | 0=Per guest, 1=Per room |
| cijt | INT | Price includes tax | 1=cijena plus taxe, 0=cijena minus taxe |
| fiscal | VARCHAR | Fiscal device config | `{type}*{param1}*{param2}*{param3}*{model}*{collation}` — `*`-delimited |
| valuta | VARCHAR | Currency code | e.g., "BAM", "EUR" |
| racunbr | VARCHAR | Invoice numbering format | |
| lokac | VARCHAR | Location code | |
| minchi | INT | Min check-in hour | |
| maxcho | INT | Max check-out hour | |
| t1 | INT | Invoice date by arrival date | Boolean flag |
| t2 | INT | Monthly billing flag | Boolean flag |
| verpr | VARCHAR | Server version prefix | e.g., `servDB & year` |
| verbaz | VARCHAR | DB version | |
| izmjver | DATETIME | Last version change | |
| rad | VARCHAR | Worker name | Current worker |

### 4.2 zaglavlje Table (Invoice Header)

From `legacy_code/frmpostavke.vb:35-39`:
```sql
-- Read: SELECT from zagl table
-- Fields: nazivfirm, adresa
```

| Column | Type | Purpose |
|--------|------|---------|
| id | INT | PK (always 1) |
| red, red1, red2, red3 | VARCHAR | Header text lines |
| idbr | VARCHAR | ID number |
| pdv | VARCHAR | VAT number |
| nazivfirm | VARCHAR | Company name |
| adresa | VARCHAR | Company address |
| prijbor | VARCHAR | Registration number |

**UPDATE** at `legacy_code/frmpostavke.vb:1702`:
```sql
update zaglavlje set nazivfirm='{name}', adresa='{address}' where id='1'
```

**INSERT** at `legacy_code/frmpostavke.vb:1704`:
```sql
insert into zaglavlje (id,red,red1,red2,red3,idbr,pdv,nazivfirm,adresa,prijbor) values ('1','','','','','','','{name}','{address}','')
```

### 4.3 Fiscal Device Types

From `cmdFisk_SelectedIndexChanged` at `legacy_code/frmpostavke.vb:698-762`:

| Index | Device | Notes |
|-------|--------|-------|
| 0 | (none) | Groupnsc hidden |
| 1 | Eltrade - Montenegro | |
| 2 | NSC doo - Datecs | Default: IOSA=1000000000000023, FP-550H |
| 3 | Tring doo - Tring | localhost:8085 |
| 4 | KimTec doo - Termol | HCP paths |
| 5 | Erp doo - Star | |
| 6 | Mikroelektronika - Republika Srpska | |
| 7 | KimTec doo - HCP | Same as Termol |

### 4.4 Controller Table: `kontroler`

From `legacy_code/frmpostavke.vb:314`:
```sql
select * from kontroler
```

Columns (from CRUD at line 1622-1625):

| Column | Type | Purpose |
|--------|------|---------|
| id | INT AUTO | PK (0 means new insert) |
| naziv | VARCHAR | Controller name |
| ipadres | VARCHAR | IP address |
| port | VARCHAR | Port |
| hostname | VARCHAR | Host name |
| verpr | VARCHAR | Version |
| netbios | VARCHAR | NetBIOS name |

**INSERT** at `legacy_code/frmpostavke.vb:1622`:
```sql
insert into kontroler (naziv, ipadres, port, hostname, verpr, netbios) values('{naziv}','{ipadres}','{port}','{hostname}','{verpr}','{netbios}')
```

**UPDATE** at `legacy_code/frmpostavke.vb:1625`:
```sql
update kontroler set naziv='{naziv}', ipadres='{ipadres}', port='{port}', hostname='{hostname}', verpr='{verpr}', netbios='{netbios}' where id='{id}'
```

### 4.5 Countries: `drzave`

From `legacy_code/frmpostavke.vb:1135`:
```sql
SELECT ID, naziv, domaca, sifra FROM drzave
```

| Column | Type | Purpose |
|--------|------|---------|
| ID | INT | PK |
| naziv | VARCHAR | Country name |
| domaca | BOOLEAN | Is home country |
| sifra | VARCHAR | Country code |

**INSERT** at `legacy_code/frmpostavke.vb:1672`:
```sql
insert into drzave (ID, naziv, domaca, sifra) select (select max(id)+1 from drzave),'{name}',0,'{code}'
```

**UPDATE** at `legacy_code/frmpostavke.vb:1766`:
```sql
update drzave set sifra='{code}', naziv='{name}', domaca='{flag}' where id='{id}'
```

**RESET all codes** at `legacy_code/frmpostavke.vb:1839`:
```sql
update drzave set sifra=0
```

### 4.6 Room Import (XML-based)

`legacy_code/frmpostavke.vb:981-1018`: `ucitajsob(xm)` loads room data from XML files with two INSERT variants depending on whether `idvrsta1` column exists:

```sql
insert into sobe (ID, naziv, vrsta, lokal, ooo, razlog, zgradaID, clean, tekst, idvrsta1) values(...)
-- or without idvrsta1:
insert into sobe (ID, naziv, vrsta, lokal, ooo, razlog, zgradaID, clean, tekst) values(...)
```

### 4.7 Settings Write (snimiS)

At `legacy_code/frmpostavke.vb:524-572`, the massive `snimiS()` function performs an INSERT into `setings` with all configuration values as a single INSERT statement:

```sql
insert into setings(sobekuc,keyk,stan,pdv,pdvtr,osig,taxa,cultur,dijecagod,dijecapop,naplposo,cijt,pdvo,pdvtax,fiscal,valuta,racunbr,lokac,minchi,maxcho,t1,t2,verpr,verbaz,izmjver,rad,t3)
values('{sobekuc}','{keyk}',{stan},{pdv},{pdvtr},{osig},{taxa},'{cultur}',{dijec},{dijecp},{nappsobi},{cijtax},{pdvo},{pdvta},'{fis}','{valuta}','{racunbr}','{lokac}',{minchi},{maxcho},{racundatumvr},{racunmjesec},'{servDB}{year}','{verB}','{now}','{worker}','{t3}')
```

Note: Always INSERT, never UPDATE — likely deletes previous row first or uses table truncation.

### 4.8 License/Piracy Protection

`provj()` at `legacy_code/frmpostavke.vb:104-124`: Decodes a `copyright@...` string combined with motherboard serial number (`MBSerialNumber()`) to validate the license. The `snimi()` function at line 125-148 is **disabled** (returns early with a message).

---

## 5. Database Administration (frmBaza)

### 5.1 Overview

`frmBaza.vb` (1696 lines) is a **Swiss-army knife DB admin tool** — connects to both MySQL and MS SQL Server.

### 5.2 Dual Database Support

- `conn` / `da` / `cb` — MySQL connection (`MySqlConnection`)
- `conS` / `daS` / `cbS` — SQL Server connection (`SqlClient.SqlConnection`)
- Checkbox toggles between MySQL and SQL Server modes (`legacy_code/frmBaza.vb:543-577`)

### 5.3 Core Database Admin SQL

**List databases** (`legacy_code/frmBaza.vb:604`):
```sql
SHOW DATABASES
```
(SQL Server: `sp_databases`)

**List tables** (`legacy_code/frmBaza.vb:652`):
```sql
SHOW TABLES
```
(SQL Server: `select * from SYSOBJECTS where TYPE = 'U' order by NAME`)

**Describe table** (`legacy_code/frmBaza.vb:775`):
```sql
describe {tablename}
```
(SQL Server: `SELECT table_catalog, table_schema, table_name, column_name, data_type, character_maximum_length FROM information_schema.columns where table_name='{tablename}'`)

**Generic SELECT** (`legacy_code/frmBaza.vb:690`):
```sql
SELECT * FROM {tablename}
```

### 5.4 Invoice Deletion (brisi functions)

Single invoice deletion chain at `legacy_code/frmBaza.vb:865-1013`:

```sql
delete placanje from placanje where broj={id}
delete placanjeDetalji from placanjeDetalji where brojID={id}
delete placanjeSlozeno from placanjeSlozeno where RBR={id}
delete printracuni from printracuni where brojRacuna={id}
delete printracunidetalji from printracunidetalji where brojRacuna={id}
delete printracunifooter from printracunifooter where brojRacuna={id}
```

These 6 tables must be deleted in order for a single invoice — **no transaction wrapping**.

### 5.5 Invoice Import (Cross-Database Transfer)

At `legacy_code/frmBaza.vb:1455-1595`, 6 `import` functions copy invoices between databases:

```sql
insert into {new_db}.placanje (...) select ... from {old_db}.placanje where broj={old_id}
insert into {new_db}.placanjeDetalji (...) select ... from {old_db}.placanjeDetalji where brojID={old_id}
insert into {new_db}.placanjeSlozeno (rbr, nacin, iznos) select '{new_id}', nacin, iznos from {old_db}.placanjeSlozeno where rbr={old_id}
insert into {new_db}.printracuni (...) select ... from {old_db}.printracuni where brojRacuna={old_id}
insert into {new_db}.printracunidetalji (...) select ... from {old_db}.printracunidetalji where brojRacuna={old_id}
insert into {new_db}.printracunifooter (...) select ... from {old_db}.printracunifooter where brojRacuna={old_id}
```

### 5.6 Seed Data (Initial Setup)

At `legacy_code/frmBaza.vb:1017-1100`, reads XML seed files and inserts:

| XML File | Table | Data |
|----------|-------|------|
| drzave.xml | drzave | Countries (name, domestic flag, code) |
| gostDokument.xml | gostDokument | Guest document types |
| kursna.xml | kursna | Currency exchange rates |
| troskovivrste.xml | troskovivrste | Expense types |
| placanjenacin.xml | placanjenacin | Payment methods |

Uses `provjer()` function to check existence before inserting (`legacy_code/frmBaza.vb:1113-1143`).

### 5.7 Backup/Export Functions

- **Single table → XML** (`legacy_code/frmBaza.vb:1197-1238`): `SELECT * FROM {table}` → `DataTable.WriteXml()`
- **All tables → XML** (`legacy_code/frmBaza.vb:1271-1314`): Iterates all tables, exports to single XML
- **XML → Database import** (`legacy_code/frmBaza.vb:1317-1412`): Dynamic INSERT generation from XML

### 5.8 Stored Procedure for Account Reset

At `legacy_code/frmBaza.vb:853`:
```sql
DELIMITER $$
DROP PROCEDURE IF EXISTS `resetbr` $$
CREATE PROCEDURE `resetbr` ()
BEGIN
    truncate table placanje;
    truncate table placanjedetalji;
    truncate table placanjeslozeno;
    truncate table predracuni;
    truncate table predracunidet;
    truncate table printracspec;
    truncate table printracuni;
    truncate table printracuniavans;
    truncate table printracunidetalji;
    truncate table printracunifooter;
END $$
```

### 5.9 Cross-Database Cloning

`Label11_DoubleClick` at `legacy_code/frmBaza.vb:1601-1687`:
```sql
show tables from {old_db}
-- then for each table:
CREATE TABLE {new_db}.{table} SELECT * FROM {old_db}.{table};
ALTER TABLE {new_db}.{table} MODIFY COLUMN `id` INT(10) NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (`id`)
```

### 5.10 Key DB Admin Issues
- **No transactions** on multi-table operations (invoice deletion, import)
- **SQL injection everywhere**: all parameters concatenated into SQL strings
- **Hardcoded connection strings** using global `server`, `UIDd`, `passwo` variables
- **Seed data in flat XML files**, not version-controlled
- **Cross-database operations** use raw `server;UID;password` connection strings

---

## 6. SQL Inventory (All SQL from These Files)

### frmPartner1.vb

| Line | Type | Statement |
|------|------|-----------|
| 111 | UPDATE | `UPDATE partneri SET naziv=@naziv,mjesto=@mjesto,posta=@posta,ulica=@ulica,zemlja=@zemlja,telefon=@telefon,fax=@fax,email=@email,www=@www,zrac1=@zrac1,zrac2=@zrac2,zrac3=@zrac3,zrac4=@zrac4,pdv=@pdv,idd=@idd,rjesenje=@rjesenje,kosoba=@kosoba,rabat=@rabat,brdanodg=@brdanodg,filter=@filter,napomena=@napomena WHERE id=@id` |
| 152 | INSERT | `INSERT INTO partneri (naziv,mjesto,posta,ulica,zemlja,telefon,fax,email,www,zrac1,zrac2,zrac3,zrac4,pdv,idd,rjesenje,kosoba,rabat,brdanodg,vr_upis,filter,napomena) VALUES (...)` |
| 364 | UPDATE | `update partneri set del=1 where id='{id}'` |

### frmPartneri.vb

| Line | Type | Statement |
|------|------|-----------|
| 43 | INSERT | Same as frmPartner1 INSERT |
| 87 | SELECT | `select id,naziv,mjesto,posta,ulica,zemlja,telefon,fax,email,www,zrac1,zrac2,zrac3,zrac4,pdv,idd,rjesenje,kosoba,rabat,brdanodg,vr_upis,filter,napomena from partneri where del=0` |
| 108 | UPDATE | Same as frmPartner1 UPDATE |
| 329 | UPDATE | `update partneri set del=0 where id='{id}'` |

### frmTarife.vb

| Line | Type | Statement |
|------|------|-----------|
| 36-575 | SP CALL | `unesiRelSobaVrstaSobaTarifa` (called for each checked room type) |
| 598-619 | SP CALL | `unesiSobaTarifa` (returns new tariff ID) |
| 653 | SELECT | `SELECT s.ID, s.naziv, s.naziv2, s.del FROM sobatarifa s` |
| 667 | UPDATE | `update sobatarifa set del={del},naziv='{price}',naziv2='{name}' where id='{id}'` |

### frmLogin.vb

| Line | Type | Statement |
|------|------|-----------|
| 25 | SELECT | `SELECT * from radnici` |

### frmpostavke.vb

| Line | Type | Statement |
|------|------|-----------|
| 93 | SELECT | `SELECT g.id, g.naziv, g.taksa, g.del FROM goststatus g` |
| 230 | UPDATE | `update sobe set idvrsta1={val} where id={id}` |
| 266 | SELECT | `SELECT * FROM sobe` |
| 281 | SELECT | `select id1,id, naziv from sobavrsta1` |
| 314 | SELECT | `select * from kontroler` |
| 405 | SELECT | `SELECT logs.ID as BrLog, logs.dugme as klik, logs.opis, logs.opis1, logs.vrijeme, logs.radnik FROM logs where logs.vrijeme between '{od}' and '{do}' {st}` |
| 435 | SELECT | `SELECT * from logs` |
| 569 | INSERT | `insert into setings(...) values(...)` — massive settings insert |
| 646 | INSERT | `insert into sobavrsta1 (id, naziv) values ({id},'{naziv}')` |
| 649 | UPDATE | `UPDATE sobavrsta1 SET id={id}, naziv='{naziv}' where id1={id1}` |
| 679 | DELETE | `delete sobavrsta1 from sobavrsta1 where id1={id1}` |
| 1004 | INSERT | `insert into sobe (ID,naziv,vrsta,lokal,ooo,razlog,zgradaID,clean,tekst,idvrsta1) values(...)` |
| 1010 | INSERT | `insert into sobe (ID,naziv,vrsta,lokal,ooo,razlog,zgradaID,clean,tekst) values(...)` |
| 1033 | INSERT | `insert into drzave (ID, naziv, domaca, sifra) values(...)` |
| 1135 | SELECT | `SELECT ID, naziv, domaca, sifra FROM drzave` |
| 1149 | SELECT | `SELECT ID, ime, adresa, telefon, username, disabled, nivo, printn FROM radnici where disabled=0 and nivo<9` |
| 1169 | SELECT | `SELECT ID, naziv, brojKreveta, defaultTarifa FROM sobavrsta` |
| 1197 | INSERT | `insert into sobavrsta (id,naziv,brojKreveta,defaultTarifa) SELECT max(ID)+1,'-','1','1' FROM sobavrsta` |
| 1204 | DELETE | `delete sobavrsta from sobavrsta where id={id}` |
| 1215 | UPDATE | `update sobavrsta set naziv='{name}', brojKreveta='{beds}', defaultTarifa='{tariff}' where id={id}` |
| 1536 | INSERT | `insert into tarifatxe(naziv,iznos) values('naziv',1)` |
| 1542 | UPDATE | `update tarifatxe set del=1 where id='{id}'` |
| 1548 | UPDATE | `update tarifatxe set naziv='{name}', iznos='{amount}' where id='{id}'` |
| 1622 | INSERT | `insert into kontroler (naziv, ipadres, port, hostname, verpr, netbios) values(...)` |
| 1625 | UPDATE | `update kontroler set naziv=..., ipadres=..., port=..., hostname=..., verpr=..., netbios=... where id=...` |
| 1672 | INSERT | `insert into drzave (ID, naziv, domaca, sifra) select (select max(id)+1 from drzave),'{name}',0,'{code}'` |
| 1702 | UPDATE | `update zaglavlje set nazivfirm='{name}', adresa='{addr}' where id='1'` |
| 1704 | INSERT | `insert into zaglavlje (id,...) values ('1',...,'{name}','{addr}','')` |
| 1745 | DDL | `CREATE TABLE goststatus (id INT AUTO_INCREMENT, naziv VARCHAR(85), PRIMARY KEY id)` |
| 1766 | UPDATE | `update drzave set sifra=..., naziv=..., domaca=... where id=...` |
| 1798 | UPDATE | `update goststatus set taksa=..., del=... where id=...` |
| 1839 | UPDATE | `update drzave set sifra=0` |

### frmBaza.vb

| Line | Type | Statement |
|------|------|-----------|
| 604 | SELECT | `SHOW DATABASES` |
| 652 | SELECT | `SHOW TABLES` |
| 690 | SELECT | `SELECT * FROM {table}` |
| 775 | DESCRIBE | `describe {table}` |
| 889-1000 | DELETE | 6 DELETE statements for invoice tables |
| 1017-1100 | INSERT | Seed data inserts from XML (drzave, gostDokument, kursna, troskovivrste, placanjenacin) |
| 1113-1143 | SELECT | `select {col} from {table} where {col} = '{val}'` (existence check) |
| 1156 | SELECT | `select * from drzave` |
| 1464-1595 | INSERT | 6 cross-database invoice import INSERT...SELECT statements |
| 1612 | SELECT | `show tables from {db}` |
| 1647 | DDL | `CREATE TABLE {new_db}.{table} SELECT * FROM {old_db}.{table}` |
| 1661 | DDL | `ALTER TABLE {db}.{table} MODIFY COLUMN id INT(10) NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (id)` |
| 853 | DDL/SP | `CREATE PROCEDURE resetbr()` — truncates 10 invoice tables |

---

## 7. Key Findings for Modern System

### 7.1 Critical Security Issues

1. **Plaintext password storage and comparison** (`frmLogin.vb:54`) — must be replaced with bcrypt/argon2 hashing
2. **SQL injection vulnerabilities** in virtually all string-concatenated queries — parameterized queries or ORM needed
3. **Hidden admin backdoor** (Ctrl+F1) bypasses authentication entirely
4. **`jmbg` misused as permission bitmask** — not a proper RBAC system
5. **No audit logging** for most data changes (only `funkcije.logs()` in tariff form)

### 7.2 Architecture Anti-Patterns

1. **Massive code duplication**: frmPartner1 ≈ frmPartneri (near-identical); tariff room-type mapping copy-pasted 16 times
2. **Business logic in UI forms**: All data access, validation, and business rules are in event handlers
3. **Global mutable state**: `akcij`, `digi`, `RID`, `RIme`, `dozvole`, `ds`, `ConnStr`, `servDB`, etc.
4. **No data layer**: Direct SQL in every form, no repository/service pattern
5. **No transaction management**: Multi-table operations (invoice delete, import) have no transaction wrapping
6. **Mixed concerns in settings form**: frmpostavke handles settings, rooms, room types, controllers, countries, workers, fiscal devices, card readers, logs, and licensing all in one form

### 7.3 Data Model Summary

| Table | Source File | Purpose |
|-------|------------|---------|
| partneri | frmPartner1/frmPartneri | Partner/agency directory |
| sobatarifa | frmTarife | Room tariffs/pricing |
| relsobavrstaobatarifa | frmTarife (SP) | Tariff ↔ room type junction |
| sobavrsta | frmpostavke | Room type definitions |
| sobavrsta1 | frmpostavke | Secondary room type classification |
| sobe | frmpostavke | Room inventory |
| goststatus | frmpostavke | Guest status/tax definitions |
| tarifatxe | frmpostavke | Extra tax/tariff items |
| radnici | frmLogin/frmpostavke | Employee/user accounts |
| setings | frmpostavke | System settings (single row) |
| zaglavlje | frmpostavke | Invoice header data |
| kontroler | frmpostavke | Card reader controllers |
| drzave | frmpostavke, frmBaza | Countries |
| logs | frmpostavke | Action log |
| gostDokument | frmBaza | Guest document types |
| kursna | frmBaza | Currency exchange rates |
| troskovivrste | frmBaza | Expense types |
| placanjenacin | frmBaza | Payment methods |
| placanje | frmBaza | Payments |
| placanjeDetalji | frmBaza | Payment details |
| placanjeSlozeno | frmBaza | Compound payments |
| printracuni | frmBaza | Printed invoices |
| printracunidetalji | frmBaza | Invoice details |
| printracunifooter | frmBaza | Invoice footers |
| printracuniavans | frmBaza (SP) | Advance invoices |
| printracspec | frmBaza (SP) | Special invoices |
| predracuni | frmBaza (SP) | Pre-invoices |
| predracunidet | frmBaza (SP) | Pre-invoice details |

### 7.4 Modernization Recommendations

1. **Replace partner forms** with a single Partner CRUD service + ViewModel; merge frmPartner1/frmPartneri
2. **Replace tariff room-type checkboxes** with a dynamic many-to-many UI driven by `sobavrsta` table
3. **Implement proper authentication**: Hash passwords, role-based access control (not jmbg bitmask), session tokens
4. **Create a Settings service** to replace the monolithic `setings` row with a proper key-value or typed configuration
5. **Add transaction management** for all multi-table operations (invoice CRUD, imports)
6. **Fix fiscal settings**: Delimiter-based encoding in `fiscal` and `sobekuc` columns should become proper related tables
7. **Separate concerns**: Break frmpostavke into multiple focused admin screens (general settings, rooms, fiscal, controllers, countries, users, logs)
8. **Add Z-offset reporting** for the tax/VAT fields that are currently free-text decimals
9. **Soft-delete pattern** (del=0/1) should be replaced with proper archival or at minimum a consistent query filter pattern
10. **Cross-database operations** in frmBaza should use proper ETL, not raw SQL with concatenated credentials