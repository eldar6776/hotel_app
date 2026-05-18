# Legacy Hotel Database Schema (hotelhi2018)

**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql`
**Database:** hotelhi2018 | **Server:** MySQL 5.6.17 | **Engine:** InnoDB (most tables), MyISAM (views)
**Collation:** utf8/utf8mb4

---

## 1. Tables

### 1.1 `alarm`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:25`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) | NO (PK, AUTO_INCREMENT) |  |
| opis | varchar(255) | YES | NULL |
| opis1 | varchar(255) | YES | NULL |
| odgovor | varchar(255) | YES | NULL |
| vrijeme | datetime | YES | NULL |
| vrijeme1 | varchar(45) | YES | '' |
| tip | int(10) | YES | NULL |
| chk | int(10) | YES | NULL |
| rpt | int(10) | YES | NULL |
| week | int(10) | YES | NULL |
| pon | int(10) | YES | NULL |
| uto | int(10) | YES | NULL |
| sri | int(10) | YES | NULL |
| cet | int(10) | YES | NULL |
| pet | int(10) | YES | NULL |
| sub | int(10) | YES | NULL |
| ned | int(10) | YES | NULL |
| radnik | varchar(50) | YES | NULL |
| soba | varchar(30) | YES | NULL |
| storno | int(10) | YES | NULL |
| vr_upis | datetime | YES | NULL |
| vr_potvrde | datetime | YES | NULL |
| radnStorn | varchar(50) | YES | NULL |
| radnCHK | varchar(50) | YES | NULL |
| stornovr | datetime | YES | NULL |
| radnCHKvr | datetime | YES | NULL |
| idd | varchar(45) | YES | NULL |

**PK:** `id`

---

### 1.2 `drzave`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:99`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(45) | YES | NULL |
| skr | varchar(45) | YES | NULL |
| cod | varchar(45) | YES | NULL |
| br | int(10) unsigned | YES | '0' |
| pozbr | varchar(45) | YES | '0' |
| prim | int(10) unsigned | NO | '1' |
| del | int(10) unsigned | YES | '0' |
| upd | int(10) unsigned | NO | '0' |
| domaca | int(4) unsigned | YES | '0' |
| sifra | varchar(10) | YES | NULL |

**PK:** `id`

---

### 1.3 `export`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:132`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| dat | datetime | YES | NULL |
| datt | datetime | YES | NULL |
| tip | int(4) unsigned | YES | '0' |

**PK:** `id`

---

### 1.4 `fisc`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:158`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| br | int(10) | YES | NULL |
| naziv | varchar(100) | YES | NULL |

**No PK defined** (heap table for fiscal receipt number tracking)

---

### 1.5 `godine`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:195`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(20) | YES | NULL |
| n1 | varchar(100) | YES | NULL |
| n2 | varchar(100) | YES | NULL |
| n3 | varchar(100) | YES | NULL |
| i1 | int(10) | YES | NULL |

**PK:** `id`

---

### 1.6 `gostdokument`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:222`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | smallint(5) | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(63) | NO |  |

**PK:** `ID`
**Lookup table:** Document types (Pasos, Licna, Vozacka, Ostalo)

---

### 1.7 `gosti`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:246`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| ime | varchar(30) | YES | NULL |
| prezime | varchar(30) | NO |  |
| adresa | varchar(255) | YES | NULL |
| datumRodjenja | datetime | YES | NULL |
| pol | char(1) | YES | NULL |
| drzavljanstvo | varchar(30) | YES | NULL |
| dokument | smallint(5) | YES | '0' |
| brDokument | varchar(30) | YES | NULL |
| telefon | varchar(20) | YES | NULL |
| mobitel | varchar(20) | YES | NULL |
| email | varchar(30) | YES | NULL |
| mjestodrzavaR | varchar(100) | YES | NULL |
| DID | int(10) | YES | NULL |
| Rid | int(10) | YES | NULL |

**PK:** `ID`
**Indexes:** `FK_gosti_gostDokument` (`dokument`)

**FK implied:** `dokument` -> `gostdokument.ID`, `DID` -> `drzave.id`

---

### 1.8 `gostiknjiga`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:287`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | varchar(45) | NO (PK) |  |
| prbr | int(10) unsigned | NO | '0' |
| pid | varchar(45) | YES | NULL |
| domaci_broj | varchar(45) | YES | NULL |
| kniga_reg_broj | varchar(45) | YES | NULL |
| redni_broj | int(10) unsigned | YES | NULL |
| subjekt | int(10) unsigned | YES | NULL |
| prijava_za_strance | varchar(45) | YES | NULL |
| ime | varchar(95) | YES | NULL |
| prezime | varchar(95) | YES | NULL |
| vrsta_isprave | varchar(95) | YES | NULL |
| pocetak_boravka | datetime | YES | NULL |
| drzavljanstvo | varchar(95) | YES | NULL |
| broj_isprave | varchar(45) | YES | NULL |
| spol | varchar(10) | YES | NULL |
| datum_rodjenja | datetime | YES | NULL |
| status_gosta | varchar(95) | YES | NULL |
| prestanak_boravka | datetime | YES | NULL |
| vrsta_vize | varchar(45) | YES | NULL |
| rok_vazenja_vize | datetime | YES | NULL |
| rok_vazenja_pi | datetime | YES | NULL |
| datum_ulaska | datetime | YES | NULL |
| mjesto_ulaska | varchar(45) | YES | NULL |
| mjesto_rodjenja | varchar(45) | YES | NULL |
| del | int(4) unsigned | NO | '0' |
| rad | varchar(45) | YES | NULL |
| dat | datetime | YES | NULL |

**PK:** `id`

**Purpose:** Guest registration book (police reporting / tourist registry)

---

### 1.9 `goststatus`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:355`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(45) | NO |  |
| del | int(4) unsigned | NO | '0' |
| taksa | double | NO | '0' |

**PK:** `id`
**Lookup:** Guest status types (Turist, Vlasnik kuće, Dijete do 12 godina, etc.) with tax amounts

---

### 1.10 `isprave`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:381`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(45) | NO |  |
| del | int(4) unsigned | NO | '0' |

**PK:** `id`
**Lookup:** Document/ID types (Pasos, Licna, Vozacka, etc.)

---

### 1.11 `kard`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:406`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| kard | int(10) unsigned | NO |  |
| tip | int(4) unsigned | NO |  |
| soba | varchar(45) | NO |  |
| vazido | datetime | NO |  |
| vrijeme | datetime | NO |  |
| rad | varchar(45) | NO |  |
| sobaid | varchar(45) | NO |  |
| ime | varchar(45) | NO |  |
| del | int(4) unsigned | NO | '0' |

**PK:** `id`
**Purpose:** Key card tracking per room

---

### 1.12 `komercijalista`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:438`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| gostiju | int(10) unsigned | YES | '0' |
| ime | varchar(45) | YES | NULL |
| prezime | varchar(45) | YES | NULL |
| telefon | varchar(65) | YES | NULL |
| mobitel | varchar(65) | YES | NULL |
| napomena | varchar(355) | YES | NULL |
| tarifaid | int(11) | YES | NULL |
| cjenovnik | int(10) unsigned | YES | NULL |
| idd | varchar(45) | YES | NULL |
| promjena | varchar(45) | YES | NULL |
| pom | int(10) unsigned | YES | '0' |
| pom1 | int(10) unsigned | YES | '0' |
| pusac | int(10) unsigned | YES | '0' |

**PK:** `id`
**Purpose:** Commercial agents / travel agencies

---

### 1.13 `konta`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:473`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| idprog | int(10) unsigned | YES | NULL |
| konto | varchar(45) | YES | NULL |
| sif1 | varchar(45) | YES | NULL |
| sif2 | varchar(45) | YES | NULL |

**PK:** `id`
**Purpose:** Chart of accounts / accounting codes linked to cost types

---

### 1.14 `kontroler`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:500`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(45) | YES | NULL |
| ipadres | varchar(45) | YES | NULL |
| port | int(10) unsigned | YES | NULL |
| hostname | varchar(45) | YES | NULL |
| verpr | varchar(45) | YES | NULL |
| netbios | varchar(45) | YES | NULL |

**PK:** `id`
**Purpose:** Room controller device registry (IP, port, hostname for keycard/door systems)

---

### 1.15 `kursna`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:529`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| Naziv_Valute | char(10) | YES | NULL |
| Vrijednost | decimal(18,3) | YES | NULL |

**PK:** `ID`
**Lookup:** Exchange rates (EUR=1.950 KM)

---

### 1.16 `logcont`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:553`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| idlog | int(10) unsigned | YES | NULL |
| logid | int(10) unsigned | YES | NULL |
| log | varchar(45) | YES | NULL |
| idkont | int(10) unsigned | YES | NULL |
| kard | varchar(45) | YES | NULL |
| vrijeme | datetime | YES | NULL |
| upd | varchar(15) | NO | '0000000000' |
| dat | datetime | YES | NULL |

**PK:** `id`
**Purpose:** Controller log entries

---

### 1.17 `logloby`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:583`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| Id | int(10) | NO (PK, AUTO_INCREMENT) |  |
| Vrijeme | datetime | YES | NULL |
| Opis | longtext | YES | NULL |
| Opis1 | longtext | YES | NULL |
| Radnik | longtext | YES | NULL |
| Dugme | longtext | YES | NULL |

**PK:** `Id`
**Purpose:** Lobby action log

---

### 1.18 `logradnici`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:610`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | bigint(19) | NO (PK, AUTO_INCREMENT) |  |
| radnikID | int(10) | NO |  |
| logIn | datetime | NO |  |
| logOut | datetime | YES | NULL |

**PK:** `ID`
**Indexes:** `FK_logRadnici_radnici` (`radnikID`)
**FK implied:** `radnikID` -> `radnici.ID`

---

### 1.19 `logrestoran`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:637`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| Id | int(10) | NO (PK) |  |
| Vrijeme | datetime | YES | NULL |
| Opis | longtext | YES | NULL |
| Opis1 | longtext | YES | NULL |
| Radnik | longtext | YES | NULL |
| Dugme | longtext | YES | NULL |

**PK:** `Id`

---

### 1.20 `logs`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:663`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| dugme | varchar(255) | YES | NULL |
| vrijeme | datetime | YES | NULL |
| opis | varchar(255) | YES | NULL |
| opis1 | varchar(255) | YES | NULL |
| radnik | varchar(255) | YES | NULL |
| radnikId | int(10) | YES | NULL |

**PK:** `ID`
**Purpose:** Application audit log (check-in, check-out, payments, expenses, etc.)

---

### 1.21 `mailkonfig`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:693`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| komeMail | longtext | YES | NULL |
| subjekt | longtext | YES | NULL |
| tijelo | longtext | YES | NULL |
| odkogaMail | longtext | YES | NULL |
| pass | longtext | YES | NULL |
| smtp | longtext | YES | NULL |
| port | longtext | YES | NULL |
| ssl | longtext | YES | NULL |

**No PK defined** (single-row config table)

---

### 1.22 `napomena`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:721`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | bigint(19) | NO (PK, AUTO_INCREMENT) |  |
| napomena | text | YES | NULL |
| rad | varchar(45) | NO |  |
| vrijeme | datetime | NO |  |

**PK:** `ID`

---

### 1.23 `napomenad`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:746`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | bigint(19) | NO (PK, AUTO_INCREMENT) |  |
| napomena | text | YES | NULL |
| rad | varchar(45) | NO |  |
| vrijeme | datetime | NO |  |

**PK:** `ID`

---

### 1.24 `neplaceni`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:771`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| PID | int(10) | YES | NULL |
| DatumOdlaska | datetime | YES | '0000-00-00 00:00:00' |
| SID | int(10) | YES | NULL |
| TID | int(10) | YES | NULL |
| placeno | tinyint(4) | YES | '0' |

**PK:** `ID`
**Purpose:** Unpaid items tracking (links to posjetafolio via PID, sobe via SID, troskovivrste via TID)

---

### 1.25 `neplaceniplacanja`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:799`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| PID | int(10) | YES | NULL |
| IznosZaPlatit | decimal(18,0) | YES | NULL |
| IznosAvans | decimal(18,0) | YES | NULL |

**PK:** `ID`
**Purpose:** Unpaid payment amounts per stay (PID = posjetafolio.ID)

---

### 1.26 `nocenja`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:825`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| RID | int(10) | YES | NULL |
| DatumP | datetime | YES | NULL |
| DatumOdj | datetime | YES | NULL |
| SID | int(10) | YES | NULL |
| PID | int(10) | YES | NULL |
| PrijavaOdjava | tinyint(4) | YES | NULL |
| Tarifa | decimal(18,2) | YES | NULL |
| popust | int(10) | YES | '0' |
| opis | varchar(255) | YES | NULL |
| soba | varchar(45) | YES | NULL |

**PK:** `ID`
**Purpose:** Accommodation nights (night-by-night ledger). Links: RID->relgostsoba.ID, SID->sobe.ID, PID->posjetafolio.ID

---

### 1.27 `partneri`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:858`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(155) | YES | NULL |
| mjesto | varchar(100) | YES | NULL |
| posta | varchar(55) | YES | NULL |
| ulica | varchar(105) | YES | NULL |
| zemlja | varchar(85) | YES | NULL |
| telefon | varchar(105) | YES | NULL |
| fax | varchar(105) | YES | NULL |
| email | varchar(85) | YES | NULL |
| www | varchar(55) | YES | NULL |
| zrac1 | varchar(55) | YES | NULL |
| zrac2 | varchar(55) | YES | NULL |
| zrac3 | varchar(55) | YES | NULL |
| zrac4 | varchar(55) | YES | NULL |
| pdv | varchar(55) | YES | NULL |
| idd | varchar(55) | YES | NULL |
| rjesenje | varchar(55) | YES | NULL |
| kosoba | varchar(50) | YES | NULL |
| rabat | int(10) | YES | NULL |
| brdanodg | int(10) | YES | NULL |
| vr_upis | datetime | YES | NULL |
| filter | varchar(155) | YES | NULL |
| napomena | varchar(200) | YES | NULL |
| d1 | varchar(100) | YES | NULL |
| d2 | varchar(105) | YES | NULL |
| d3 | int(10) | YES | NULL |
| sifra1 | int(10) | YES | NULL |
| sifraex | varchar(45) | YES | NULL |

**PK:** `id`
**Purpose:** Business partners (travel agencies, companies)

---

### 1.28 `placanje`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:923`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| broj | decimal(19,0) | NO |  |
| relGostSobaID | decimal(19,0) | NO |  |
| iznos | decimal(19,4) | YES | '0.0000' |
| popust | decimal(19,4) | YES | NULL |
| datum | datetime | NO |  |
| nacin | int(10) | NO |  |
| radnikID | int(10) | NO |  |
| naziv | varchar(4000) | YES | NULL |
| PID | decimal(19,0) | NO |  |
| uplaceno | decimal(19,4) | NO |  |
| brdana | int(10) | NO |  |
| datumOD | datetime | YES | NULL |
| datumDO | datetime | YES | NULL |
| placanjeID | int(10) | YES | '1' |
| poslovna | varchar(50) | YES | NULL |
| storno | tinyint(4) | YES | '0' |
| folio | int(10) | YES | NULL |
| idgost | int(10) | YES | NULL |
| predracun | int(10) | YES | NULL |
| posjeta | int(10) | YES | NULL |
| firma | int(10) | YES | NULL |
| tip | int(10) unsigned | YES | '0' |
| racn | varchar(55) | YES | NULL |
| racime | varchar(55) | YES | NULL |
| pdv | int(10) unsigned | YES | '0' |
| ctax | int(10) unsigned | YES | '0' |
| sobar | varchar(455) | YES | NULL |
| perio | varchar(455) | YES | NULL |
| veza | varchar(255) | YES | NULL |
| napom | text | YES | NULL |
| napokraj | text | YES | NULL |
| napomena | text | YES | NULL |
| fiskalni | varchar(355) | YES | NULL |
| fiskal | int(10) unsigned | YES | '0' |
| fiskalizn | varchar(355) | YES | NULL |
| fiskalvr | varchar(355) | YES | NULL |
| fiskalrek | int(10) unsigned | YES | '0' |
| fiskalnrekvr | varchar(355) | YES | NULL |
| placnaz | varchar(55) | YES | NULL |
| uplatetex | varchar(255) | YES | NULL |
| hotelid | varchar(30) | YES | NULL |
| idd | varchar(55) | YES | NULL |

**PK:** `ID`
**Key relationships:** `relGostSobaID` -> `relgostsoba.ID`, `PID` -> `posjetafolio.ID`, `nacin` -> `placanjenacin.ID`, `radnikID` -> `radnici.ID`

---

### 1.29 `placanjedetalji`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:988`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| brojid | int(10) unsigned | YES | '0' |
| art | int(10) unsigned | YES | '0' |
| kolicina | double | YES | NULL |
| cijena | decimal(19,4) | NO |  |
| iznos | decimal(19,4) | NO |  |
| napomena | varchar(50) | YES | NULL |
| brojNocenja | int(10) | YES | NULL |
| PID | int(10) | YES | NULL |
| storno | tinyint(4) | YES | '0' |
| ranijeUplate | tinyint(4) | YES | '0' |
| rid | int(10) unsigned | YES | '0' |
| sid | int(10) unsigned | YES | '0' |
| gid | int(10) unsigned | YES | '0' |
| soba | varchar(55) | YES | NULL |
| sobavr | varchar(55) | YES | NULL |
| sobavrid | varchar(55) | YES | NULL |
| periodod | datetime | YES | NULL |
| perioddo | datetime | YES | NULL |
| period | varchar(55) | YES | NULL |
| ime | text | YES | NULL |
| usluga | text | YES | NULL |
| popust | double | YES | NULL |
| pdv | double | YES | NULL |
| hotelid | varchar(30) | YES | NULL |
| idd | varchar(55) | YES | NULL |

**PK:** `ID`
**Purpose:** Payment line items (details of each payment)

---

### 1.30 `placanjenacin`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1036`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | decimal(10,0) | NO (PK) |  |
| nacin | char(10) | NO |  |
| konto | varchar(45) | YES | NULL |
| partner | varchar(45) | YES | NULL |

**PK:** `ID`
**Lookup values:** 1=Cash, 2=Virman, 3=Kartica, 4=Gratis, 5=Slozeno

---

### 1.31 `placanjeslozeno`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1061`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| rbr | int(10) unsigned | NO |  |
| nacin | int(10) | NO |  |
| iznos | double | NO |  |

**PK:** `id`
**Purpose:** Split/compound payment method breakdown (nacin -> placanjenacin.ID)

---

### 1.32 `porezi`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1087`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK) |  |
| boravisnaTaxa | decimal(19,4) | YES | NULL |
| osiguranje | decimal(19,4) | YES | NULL |

**PK:** `ID`

---

### 1.33 `posjetafolio`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1110`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | bigint(19) | NO (PK, AUTO_INCREMENT) |  |
| SID | int(10) | NO |  |
| vrijemeD | datetime | YES | NULL |
| vrijemeO | datetime | YES | NULL |
| zakljucen | tinyint(4) | NO | '0' |

**PK:** `ID`
**Purpose:** Visit folio (groups expenses/payments per stay). SID -> sobe.ID. zakljucen=0 means open folio.

---

### 1.34 `posjete`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1137`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| datum | datetime | YES | NULL |
| idg | varchar(45) | YES | NULL |
| ime | varchar(95) | YES | NULL |
| tip | int(4) unsigned | YES | '0' |
| dolazak | datetime | YES | NULL |
| odlazak | datetime | YES | NULL |
| status | int(4) unsigned | YES | '0' |

**PK:** `id`

---

### 1.35 `predracuni`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1166`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) | NO (PK, AUTO_INCREMENT) |  |
| broj | int(10) | YES | NULL |
| brojpred | varchar(100) | YES | NULL |
| ime | varchar(255) | YES | NULL |
| frima | varchar(255) | YES | NULL |
| frimaid | int(10) | YES | NULL |
| dadtum | varchar(85) | YES | NULL |
| datumval | varchar(80) | YES | NULL |
| aktiv | int(10) | YES | NULL |
| ukupno | varchar(85) | YES | NULL |
| kontakt | varchar(100) | YES | NULL |
| napomnena | varchar(255) | YES | NULL |
| veza | varchar(255) | YES | NULL |
| vezaid | int(10) | YES | NULL |
| rabat | varchar(100) | YES | NULL |
| vr_upis | datetime | YES | NULL |
| d1 | varchar(255) | YES | NULL |
| d2 | varchar(105) | YES | NULL |
| d3 | int(10) | YES | NULL |
| sifra1 | int(10) | YES | NULL |
| vrplac | varchar(50) | YES | NULL |
| nazivp | varchar(150) | YES | NULL |
| nazivid | int(10) | YES | NULL |
| gostid | int(10) | YES | NULL |

**PK:** `id`
**Purpose:** Proforma invoices (predračuni)

---

### 1.36 `predracunidet`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1211`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| BrojRacuna | int(10) | YES | NULL |
| Trosak | varchar(255) | YES | NULL |
| Kol | char(10) | YES | NULL |
| CijBezPdv | char(10) | YES | NULL |
| UkupnoBezPdv | char(10) | YES | NULL |
| Pdv | char(10) | YES | NULL |
| IznosPdv | char(10) | YES | NULL |
| Ukupno | char(10) | YES | NULL |
| Nacin | varchar(100) | YES | NULL |
| Valuta | char(10) | YES | NULL |
| OznakaValute | char(10) | YES | NULL |
| Popust | int(10) | YES | '0' |
| popust1 | varchar(20) | YES | NULL |
| razlogp | varchar(300) | YES | NULL |
| pop | varchar(20) | YES | NULL |
| trosakId | int(10) | YES | NULL |
| predid | int(10) | YES | NULL |
| t1 | varchar(150) | YES | NULL |
| trosakid1 | int(10) | YES | NULL |

**PK:** `ID`
**Purpose:** Proforma invoice line items

---

### 1.37 `printracspec`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1252`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) | NO (PK, AUTO_INCREMENT) |  |
| broj | int(10) | YES | NULL |
| ime | varchar(200) | YES | NULL |
| frima | varchar(155) | YES | NULL |
| firmaid | int(10) | YES | NULL |
| dadtum | varchar(85) | YES | NULL |
| datumodj | varchar(80) | YES | NULL |
| soba | varchar(85) | YES | NULL |
| vrsobe | varchar(100) | YES | NULL |
| napomnena | text | YES | NULL |
| veza | varchar(100) | YES | NULL |
| vr_upis | datetime | YES | NULL |
| tarif | varchar(100) | YES | NULL |
| d1 | varchar(100) | YES | NULL |
| d2 | varchar(105) | YES | NULL |
| d3 | int(10) | YES | NULL |
| sifra1 | int(10) | YES | NULL |
| folio | int(10) | YES | NULL |
| idgost | int(10) | YES | NULL |
| predracun | int(10) | YES | NULL |
| posjeta | int(10) | YES | NULL |

**PK:** `id`
**Purpose:** Special print invoice records

---

### 1.38 `printracuni`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1295`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| BrojRacuna | int(10) | NO (PK, AUTO_INCREMENT) |  |
| Poslovna | varchar(20) | YES | NULL |
| Ime | varchar(500) | YES | NULL |
| DrugoIme | varchar(500) | YES | NULL |
| PeriodOd | char(10) | YES | NULL |
| PeriodDo | char(10) | YES | NULL |
| TipPlacanja | varchar(200) | YES | NULL |
| BrojSobe | varchar(50) | YES | NULL |
| storno | tinyint(4) | YES | '0' |
| fisrac | varchar(30) | YES | NULL |
| fisvr | varchar(50) | YES | NULL |
| fisizn | varchar(50) | YES | NULL |
| racin | varchar(30) | YES | NULL |
| napo | text | YES | NULL |
| datr | varchar(30) | YES | NULL |
| peri | varchar(100) | YES | NULL |
| rad | varchar(100) | YES | NULL |
| dat | datetime | YES | NULL |
| knj | int(4) unsigned | YES | '0' |
| printime | varchar(45) | YES | NULL |
| kid | int(10) unsigned | YES | NULL |
| exp | int(4) unsigned | NO | '0' |
| datstor | datetime | YES | NULL |

**PK:** `BrojRacuna`
**Purpose:** Printed invoice headers (fiscal receipts)

---

### 1.39 `printracuniavans`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1340`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| BrojRacuna | int(10) unsigned | NO (PK) |  |
| Poslovna | varchar(20) | YES | NULL |
| Ime | varchar(500) | YES | NULL |
| DrugoIme | varchar(500) | YES | NULL |
| PeriodOd | char(10) | YES | NULL |
| PeriodDo | char(10) | YES | NULL |
| TipPlacanja | varchar(50) | YES | NULL |
| BrojSobe | varchar(50) | YES | NULL |
| storno | tinyint(4) | YES | '0' |
| folio | int(10) | YES | NULL |
| idgost | int(10) | YES | NULL |
| predracun | int(10) | YES | NULL |
| posjeta | int(10) | YES | NULL |
| racin | varchar(30) | YES | NULL |
| napo | text | YES | NULL |
| datr | varchar(30) | YES | NULL |
| peri | varchar(200) | YES | NULL |
| usluga | varchar(250) | YES | NULL |
| kol | varchar(50) | YES | NULL |
| iznos | varchar(50) | YES | NULL |
| porezSt | varchar(50) | YES | NULL |
| porezIz | varchar(50) | YES | NULL |
| ukupno | varchar(50) | YES | NULL |
| artbr | int(10) | YES | NULL |
| datum | varchar(50) | YES | NULL |
| imeid | int(10) | YES | NULL |
| firmaid | int(10) | YES | NULL |
| textpr | varchar(50) | YES | NULL |
| textiz | varchar(150) | YES | NULL |
| napom | varchar(250) | YES | NULL |
| p1 | varchar(250) | YES | NULL |
| p2 | varchar(250) | YES | NULL |
| p3 | int(10) | YES | NULL |
| p4 | int(10) | YES | NULL |
| p5 | varchar(150) | YES | NULL |
| datu | datetime | YES | NULL |

**PK:** `BrojRacuna`
**Purpose:** Advance payment invoice records

---

### 1.40 `printracunidetalji`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1398`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| BrojRacuna | int(10) | YES | NULL |
| Trosak | varchar(255) | YES | NULL |
| Kol | char(10) | YES | NULL |
| CijBezPdv | char(10) | YES | NULL |
| UkupnoBezPdv | char(10) | YES | NULL |
| Pdv | char(10) | YES | NULL |
| IznosPdv | char(10) | YES | NULL |
| Ukupno | char(10) | YES | NULL |
| Nacin | varchar(100) | YES | NULL |
| Valuta | char(10) | YES | NULL |
| OznakaValute | char(10) | YES | NULL |
| Popust | int(10) | YES | '0' |
| popust1 | varchar(20) | YES | NULL |
| razlogp | varchar(300) | YES | NULL |
| pop | varchar(20) | YES | NULL |
| trosakId | int(10) | YES | NULL |
| nacinid | int(4) unsigned | YES | '0' |

**PK:** `ID`
**Purpose:** Printed invoice line items

---

### 1.41 `printracunifooter`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1438`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| BrojRacuna | int(10) | YES | NULL |
| Avansno | decimal(18,0) | YES | '0' |
| Nocenja | decimal(18,0) | YES | '0' |
| nap | text | YES | NULL |
| pri | int(10) | YES | NULL |

**PK:** `ID`
**Purpose:** Invoice footer data (legal notes, advance amounts)

---

### 1.42 `rac`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1466`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) | NO (PK, AUTO_INCREMENT) |  |
| brrac | int(10) | YES | NULL |
| oznaka | varchar(50) | YES | NULL |
| god | int(10) | YES | NULL |
| datum | datetime | YES | NULL |
| soba | varchar(50) | YES | NULL |
| sobaid | int(10) | YES | NULL |
| gost | varchar(300) | YES | NULL |
| gostid | int(10) | YES | NULL |
| gostiid | varchar(100) | YES | NULL |
| firma | varchar(200) | YES | NULL |
| firmaid | int(10) | YES | NULL |
| period | varchar(255) | YES | NULL |
| popust | varchar(100) | YES | NULL |
| popustraz | varchar(255) | YES | NULL |
| ukupno | varchar(50) | YES | NULL |
| pdv | varchar(50) | YES | NULL |
| bezpdv | varchar(50) | YES | NULL |
| placanjetext | varchar(100) | YES | NULL |
| plcash | varchar(50) | YES | NULL |
| plkard | varchar(50) | YES | NULL |
| plvirman | varchar(50) | YES | NULL |
| plcheck | varchar(50) | YES | NULL |
| gratis | varchar(50) | YES | NULL |
| valutapl | varchar(50) | YES | NULL |
| stranaval | varchar(10) | YES | NULL |
| ranijeupl | varchar(50) | YES | NULL |
| avansi | varchar(50) | YES | NULL |
| napomena | varchar(255) | YES | NULL |
| radnik | varchar(80) | YES | NULL |
| izmjenaid | int(10) | YES | NULL |
| mjisporuke | varchar(50) | YES | NULL |
| datetime | varchar(50) | YES | NULL |
| fiskbr | int(10) | YES | NULL |
| fiskdatum | varchar(50) | YES | NULL |
| fiskivr | varchar(50) | YES | NULL |
| fiskizn | varchar(50) | YES | NULL |
| fiskr1 | varchar(50) | YES | NULL |
| storno | int(10) | YES | NULL |
| r1 | int(10) | YES | NULL |
| r2 | int(10) | YES | NULL |
| rt1 | varchar(100) | YES | NULL |
| rt2 | varchar(250) | YES | NULL |

**PK:** `id`
**Purpose:** Invoice (racun) records - appears to be an alternate/newer invoice table

---

### 1.43 `radnici`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1530`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| ime | varchar(50) | NO |  |
| JMBG | varchar(20) | YES | NULL |
| adresa | varchar(255) | YES | NULL |
| telefon | varchar(30) | YES | NULL |
| username | varchar(15) | NO |  |
| password | varchar(15) | YES | NULL |
| disabled | tinyint(4) | NO | '0' |
| nivo | int(10) | YES | NULL |
| printn | varchar(45) | YES | '' |

**PK:** `ID`
**Purpose:** Hotel staff/employees. `nivo` = access level (1=receptionist, 5=admin)

---

### 1.44 `relgostsoba`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1578`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| gostID | decimal(19,0) | NO |  |
| sobaID | int(10) | NO |  |
| checkInDate | datetime | NO |  |
| checkOutDate | datetime | YES | NULL |
| checkInRadnik | int(10) | NO |  |
| checkOutRadnik | int(10) | YES | NULL |
| stampanaPrijava | tinyint(4) | NO | '0' |
| odjavljen | tinyint(4) | NO | '0' |
| rezervacija | int(10) unsigned | YES | '0' |
| grupaID | int(10) | YES | '1' |
| brojDana | int(10) | YES | '0' |
| tarifaID | int(10) | YES | NULL |
| popust | double | YES | '0' |
| ostaliTroskovi | decimal(19,4) | YES | NULL |
| PID | bigint(19) | YES | NULL |
| print1 | tinyint(4) | YES | NULL |
| print2 | tinyint(4) | YES | NULL |
| rezervP | tinyint(4) | YES | '0' |
| redniBroj | int(10) | YES | NULL |
| PopustRazlog | varchar(255) | YES | NULL |
| pl | int(10) | YES | '0' |
| napomenapl | text | YES | NULL |
| napomena | text | YES | NULL |
| usluga | text | YES | NULL |
| taksa | int(4) unsigned | YES | '0' |
| status | int(4) unsigned | YES | '0' |
| tid | int(10) unsigned | YES | '0' |

**PK:** `ID`
**Indexes:** `FK_relGostSoba_radnici` (`checkOutRadnik`), `FK_relGostSoba_radnici1` (`checkInRadnik`), `FK_relGostSoba_gosti` (`gostID`), `FK_relGostSoba_sobe` (`sobaID`)
**FKs implied:** `gostID` -> `gosti.ID`, `sobaID` -> `sobe.ID`, `checkInRadnik` -> `radnici.ID`, `checkOutRadnik` -> `radnici.ID`, `PID` -> `posjetafolio.ID`

**THIS IS THE CORE TABLE** - links guests to rooms for stays (check-in/check-out).

---

### 1.45 `relsobavrstasadrzaj`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1632`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| sobaID | int(10) | NO (PK part) |  |
| sobaSadrzajID | int(10) | NO (PK part) |  |

**PK:** (`sobaID`, `sobaSadrzajID`)
**FK implied:** `sobaSadrzajID` -> `sobasadrzaji.ID`

---

### 1.46 `relsobavrstasobatarifa`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1656`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| sobaVrstaID | int(10) | NO (PK part) |  |
| sobaTarifaID | int(10) | NO (PK part) |  |

**PK:** (`sobaVrstaID`, `sobaTarifaID`)
**FK implied:** `sobaVrstaID` -> `sobavrsta.ID`, `sobaTarifaID` -> `sobatarifa.ID`

---

### 1.47 `rezervacijaprijava`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1680`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| IDrez | int(10) | YES | NULL |
| IDgost | int(10) | YES | NULL |
| sobaID | int(10) | YES | NULL |

**PK:** `ID`
**Purpose:** Links reservation guests to rooms upon check-in

---

### 1.48 `rezervacijasobe`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1705`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| rezid | int(10) unsigned | YES | NULL |
| sobtid | int(10) unsigned | YES | NULL |
| sobatip | varchar(45) | YES | NULL |
| sid | int(10) unsigned | YES | NULL |
| soba | varchar(45) | YES | NULL |
| tid | int(10) unsigned | YES | NULL |
| tarifa | double | YES | NULL |
| gid | int(10) unsigned | YES | NULL |
| gost | varchar(75) | YES | NULL |
| brgost | int(10) unsigned | YES | NULL |
| gost1 | varchar(75) | YES | NULL |
| idd | varchar(45) | YES | NULL |
| promjena | varchar(45) | YES | NULL |
| pom | int(10) unsigned | YES | '0' |
| pom1 | int(10) unsigned | YES | '0' |
| pusac | int(10) unsigned | YES | '0' |
| cjenovnik | int(10) unsigned | YES | '0' |

**PK:** `id`
**FK implied:** `rezid` -> `rezervacije.ID`

---

### 1.49 `rezervacijasobe1`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1745`

Same structure as `rezervacijasobe` but without PK/AUTO_INCREMENT. Appears to be a staging/temp copy.

---

### 1.50 `rezervacije`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1783`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| GID | decimal(19,0) | YES | NULL |
| checkInDate | datetime | YES | NULL |
| checkOutDate | datetime | YES | NULL |
| potvrda | tinyint(4) | YES | '0' |
| brojPotvrde | int(10) | YES | NULL |
| blokID | int(10) | NO | '1' |
| tipID | int(10) | NO | '1' |
| izvorID | int(10) | NO | '1' |
| sobaVrstaID | int(10) | YES | NULL |
| stornirana | tinyint(4) | YES | '0' |
| brojStorna | int(10) | YES | NULL |
| brojRezSoba | int(10) | YES | NULL |
| godina | varchar(50) | YES | NULL |
| prijava | tinyint(4) | YES | '0' |
| tarifa | int(10) | YES | NULL |
| memo | text | YES | NULL |
| radnik | varchar(50) | YES | NULL |
| radnikID | int(10) | YES | NULL |
| vrijeme | datetime | YES | NULL |
| idd | varchar(45) | YES | '0' |
| gost | varchar(145) | YES | NULL |
| tex | varchar(245) | YES | NULL |
| napomena | text | YES | NULL |
| alarmid | int(10) unsigned | YES | '0' |
| gostgrupa | varchar(95) | YES | NULL |
| promjena | int(10) unsigned | YES | '0' |
| promjenat | text | YES | NULL |
| kontakt | varchar(75) | YES | NULL |
| kontakttel | varchar(75) | YES | NULL |
| kontaktfax | varchar(75) | YES | NULL |
| kontaktmob | varchar(75) | YES | NULL |
| kontaktmail | varchar(75) | YES | NULL |
| plac | int(10) unsigned | YES | '0' |
| placanje | varchar(75) | YES | NULL |
| firma | varchar(75) | YES | NULL |
| firmaid | int(10) unsigned | YES | '0' |
| agencija | varchar(75) | YES | NULL |
| komerc | varchar(75) | YES | NULL |
| agencijaid | int(10) unsigned | YES | '0' |
| komercid | int(10) unsigned | YES | '0' |
| brosoba | int(10) unsigned | YES | '1' |
| brdjeca | int(10) unsigned | YES | '1' |
| dateizmjena | datetime | YES | NULL |
| datestorno | datetime | YES | NULL |
| datepotvrda | datetime | YES | NULL |
| razlogst | varchar(255) | YES | NULL |

**PK:** `ID`
**FK implied:** `GID` -> `gosti.ID`, `tipID` -> `rezervacijetip.ID`, `izvorID` -> `rezervacijeizvor.ID`, `sobaVrstaID` -> `sobavrsta.ID`, `radnikID` -> `radnici.ID`

---

### 1.51 `rezervacije1`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1852`

Same structure as `rezervacije` but without PK/AUTO_INCREMENT (staging/temp copy).

---

### 1.52 `rezervacijegrupe`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1915`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(50) | YES | NULL |
| odjavljena | tinyint(4) | YES | '0' |
| idd | varchar(45) | YES | NULL |

**PK:** `ID`
**Lookup:** Reservation groups (travel agencies/tour groups)

---

### 1.53 `rezervacijeizvor`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1941`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(50) | YES | NULL |

**PK:** `ID`
**Lookup:** 1="nema podataka"

---

### 1.54 `rezervacijetip`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1965`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(50) | YES | NULL |

**PK:** `ID`
**Lookup values:** 1=Nema podataka, 2=Mail, 3=Fax, 4=Telefon

---

### 1.55 `sadrzajtarifa`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:1989`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(30) | YES | NULL |
| uslov | varchar(255) | YES | NULL |

**PK:** `ID`

---

### 1.56 `setings`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2013`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| verbaz | varchar(50) | YES | NULL |
| verpr | varchar(50) | YES | NULL |
| keyk | varchar(205) | YES | NULL |
| vrijU | datetime | YES | NULL |
| t1 | varchar(100) | YES | NULL |
| t2 | varchar(100) | YES | NULL |
| t3 | varchar(100) | YES | NULL |
| id | int(10) | NO (PK, AUTO_INCREMENT) |  |
| izmjver | varchar(50) | YES | NULL |
| rad | varchar(50) | YES | NULL |
| naplposo | int(10) unsigned | YES | '0' |
| pribora | varchar(50) | YES | NULL |
| pdv | double | YES | '0' |
| pdvo | double | YES | '0' |
| pdvtax | double | YES | '0' |
| pdvtr | double | YES | '0' |
| osig | double | YES | '0' |
| taxa | double | YES | '0' |
| cultur | varchar(40) | YES | NULL |
| sobegrupa | varchar(150) | YES | NULL |
| sobekuc | varchar(150) | YES | NULL |
| dijecagod | double | YES | '0' |
| dijecapop | double | YES | '0' |
| fiscal | varchar(150) | YES | NULL |
| valuta | varchar(20) | YES | NULL |
| racunbr | varchar(50) | YES | NULL |
| napomena | text | YES | NULL |
| lokac | varchar(50) | YES | NULL |
| cijt | int(10) unsigned | YES | '0' |
| minchi | int(10) unsigned | YES | '8' |
| maxcho | int(10) unsigned | YES | '12' |
| decim | int(10) unsigned | YES | '2' |
| stan | int(4) unsigned | NO | '0' |

**PK:** `id`
**Purpose:** Multi-hotel configuration settings (PDV=17%, tax rates, child ages, etc.)

---

### 1.57 `sifarnik`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2068`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(255) | YES | NULL |
| kol | varchar(100) | YES | NULL |
| cij | varchar(100) | YES | NULL |
| ukupno | varchar(100) | YES | NULL |
| sifra | int(10) | YES | NULL |
| porez | int(10) | YES | NULL |
| racu | int(10) | YES | NULL |
| racun | varchar(155) | YES | NULL |
| placanje | int(10) | YES | NULL |
| dod | varchar(255) | YES | NULL |
| dod1 | varchar(255) | YES | NULL |
| dod2 | varchar(255) | YES | NULL |
| dod3 | int(10) | YES | NULL |

**PK:** `id`

---

### 1.58 `smjene`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2103`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| smjenaID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| radnik | int(10) | YES | NULL |
| start | datetime | YES | NULL |
| kraj | char(10) | YES | NULL |

**PK:** `smjenaID`
**FK implied:** `radnik` -> `radnici.ID`

---

### 1.59 `sobaricalog`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2129`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| vrijeme | datetime | YES | NULL |
| tip | int(10) unsigned | YES | '0' |
| sobalokal | int(11) | YES | NULL |
| sobaricaid | int(10) unsigned | YES | NULL |
| idd | varchar(45) | YES | NULL |
| p1 | varchar(155) | YES | NULL |
| p2 | varchar(155) | YES | NULL |
| i1 | int(10) unsigned | YES | '0' |

**PK:** `id`

---

### 1.60 `sobasadrzaji`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2160`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(255) | YES | NULL |
| defaultTarifa | int(10) | YES | NULL |

**PK:** `ID`
**Indexes:** `FK_sobaSadrzaji_sadrzajTarifa` (`defaultTarifa`)
**FK implied:** `defaultTarifa` -> `sadrzajtarifa.ID`

---

### 1.61 `sobatarifa`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2185`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| naziv | decimal(18,2) | YES | NULL |
| naziv2 | varchar(30) | YES | NULL |
| uslov | varchar(255) | YES | NULL |
| del | int(10) unsigned | NO | '0' |

**PK:** `ID`
**Lookup:** Tariff prices (20.60=standard, 164.05=Single, 212.30=DBL/TWN, etc.)

---

### 1.62 `sobavrsta`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2212`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK) |  |
| naziv | varchar(30) | YES | NULL |
| brojKreveta | smallint(5) | YES | NULL |
| defaultTarifa | int(10) | YES | NULL |

**PK:** `ID`
**Lookup:** Room floor types (Prvi sprat, Drugi sprat, etc.) with bed count and default tariff

---

### 1.63 `sobavrsta1`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2238`

Same structure as `sobavrsta` but without NOT NULL on id, plus auto-increment `id1` as PK. Appears to be a staging table.

---

### 1.64 `sobe`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2264`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(50) | NO |  |
| vrsta | int(10) | NO | '1' |
| lokal | int(10) | NO | '1' |
| ooo | tinyint(4) | NO | '0' |
| razlog | varchar(255) | NO | '-' |
| zgradaID | int(10) | NO | '1' |
| clean | tinyint(4) | NO | '1' |
| tekst | varchar(50) | NO | 'Soba' |
| idvrsta1 | int(10) | YES | NULL |
| sprat | varchar(45) | YES | '0' |
| idkon | int(10) unsigned | YES | '0' |
| redulko | int(10) unsigned | YES | '0' |

**PK:** `ID`
**FK implied:** `vrsta` -> `sobavrsta.ID`, `zgradaID` -> `zgrade.ID`

---

### 1.65 `tarifatxe`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2317`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| naziv | varchar(45) | YES | NULL |
| iznos | double | NO | '0' |
| del | int(4) unsigned | NO | '0' |
| upd | int(10) unsigned | NO | '0' |

**PK:** `id`

---

### 1.66 `telefonskiimenik`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2344`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | varchar(45) | NO (PK) |  |
| Ime | longtext | NO |  |
| Prezime | longtext | YES | NULL |
| Funkcija | longtext | YES | NULL |
| Adresa | longtext | YES | NULL |
| Mjesto | longtext | YES | NULL |
| Fiksni | varchar(45) | YES | NULL |
| Mobilni | varchar(45) | YES | NULL |
| Fax | varchar(45) | YES | NULL |
| Mail | longtext | YES | NULL |
| Ostalo | longtext | YES | NULL |

**PK:** `id`

---

### 1.67 `telpozivi`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2376`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| Datum | datetime | YES | NULL |
| Vrijeme | datetime | YES | NULL |
| Lokal | int(10) | YES | NULL |
| Izlaz | int(10) | YES | NULL |
| TelefonskiBroj | longtext | YES | NULL |
| TrajanjePoziva | longtext | YES | NULL |
| PozBroj | longtext | YES | NULL |
| Drzava | longtext | YES | NULL |
| IDtarifa | int(10) | YES | NULL |
| Cijena | decimal(18,4) | YES | NULL |
| SpecPozivni | longtext | YES | NULL |
| SpecDrzava | longtext | YES | NULL |
| Cost | longtext | YES | NULL |
| Placeno | tinyint(4) | YES | NULL |

**PK:** `ID`
**Purpose:** Room telephone call logs (linked to rooms via Lokal=sobe.lokal)

---

### 1.68 `telpozivi_stara`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2413`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | bigint(19) | NO (PK, AUTO_INCREMENT) |  |
| datum | datetime | YES | NULL |
| lokal | int(10) | YES | NULL |
| izlaz | int(10) | YES | NULL |
| telefonskiBroj | longtext | YES | NULL |
| trajanjePoziva | longtext | YES | NULL |
| pozBroj | longtext | YES | NULL |
| Drzava | longtext | YES | NULL |
| IDtarifa | int(10) | YES | NULL |
| Cijena | decimal(18,4) | YES | NULL |
| specPozivni | longtext | YES | NULL |
| specDrzava | longtext | YES | NULL |
| vrijeme | datetime | YES | NULL |

**PK:** `ID`
**Purpose:** Legacy phone calls table

---

### 1.69 `troskovi`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2447`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK, AUTO_INCREMENT) |  |
| GSID | int(10) | YES | NULL |
| SID | int(10) | YES | NULL |
| TID | int(10) | YES | NULL |
| vrijeme | datetime | NO |  |
| kolicina | int(10) | YES | NULL |
| iznos | decimal(18,2) | NO |  |
| radnikID | int(10) | YES | NULL |
| napomena | varchar(50) | YES | NULL |
| zaklj | tinyint(4) | YES | '0' |
| Brrac | decimal(18,0) | YES | NULL |
| Djelimicno | tinyint(4) | YES | NULL |
| iddzid | varchar(45) | YES | NULL |
| idzid | varchar(45) | YES | NULL |
| loc | varchar(45) | YES | NULL |
| zidbr | varchar(45) | YES | NULL |
| fisbr | text | YES | NULL |
| stan | int(10) unsigned | NO | '0' |
| opis | text | YES | NULL |
| fis | int(10) unsigned | NO | '0' |

**PK:** `ID`
**FK implied:** `GSID` -> `relgostsoba.ID`, `SID` -> `sobe.ID`, `TID` -> `troskovivrste.ID`
**Purpose:** Individual expense/charge line items for guest stays

---

### 1.70 `troskovipojedinacni`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2489`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| Auto | bigint(19) | NO (PK, AUTO_INCREMENT) |  |
| IDtroska | bigint(19) | NO |  |
| datum | datetime | YES | NULL |
| iznos | decimal(19,4) | NO |  |

**PK:** `Auto`
**FK implied:** `IDtroska` -> `troskovi.ID`

---

### 1.71 `troskovivrste`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2528`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | decimal(10,0) | NO (PK) |  |
| naziv | varchar(150) | YES | NULL |
| cijenaID | int(10) | YES | NULL |
| tip | int(10) unsigned | NO | '0' |
| del | int(10) unsigned | NO | '0' |

**PK:** `ID`
**Lookup:** Expense types (Nocenje, Restoranski troskovi, Mini Bar, Masaža, Boravisna taksa, etc.)
**tip values:** 0=regular, 1=per-night type

---

### 1.72 `zaglavlje`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2555`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| id | int(10) unsigned | NO (PK, AUTO_INCREMENT) |  |
| red | text | YES | NULL |
| red1 | text | YES | NULL |
| red2 | text | YES | NULL |
| red3 | text | YES | NULL |
| idbr | varchar(20) | YES | NULL |
| pdv | varchar(20) | YES | NULL |
| nazivfirm | varchar(300) | YES | NULL |
| adresa | varchar(60) | YES | NULL |
| prijbor | varchar(60) | YES | NULL |

**PK:** `id`
**Purpose:** Invoice header/company info per hotel

---

### 1.73 `zgrade`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2586`

| Column | Type | Nullable | Default |
|--------|------|----------|---------|
| ID | int(10) | NO (PK) |  |
| naziv | char(10) | YES | NULL |
| opis | longtext | YES | NULL |

**PK:** `ID`
**Purpose:** Buildings/wings (e.g., Hotel Hills, Hotel Pino)

---

## 2. Stored Procedures and Functions

**No stored procedures or functions were found in the dump.** The database relied entirely on application-layer logic (Java/C# code) rather than database-side routines.

---

## 3. Views

### 3.1 `brojsoba`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2614`

```sql
SELECT COUNT(sobe.ID) AS BS FROM sobe
```
**Columns:** `BS` (total room count)
**Source tables:** `sobe`

---

### 3.2 `bzs`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2633`

```sql
SELECT COUNT(DISTINCT relgostsoba.sobaID) AS SID FROM relgostsoba
WHERE relgostsoba.odjavljen = 0 AND relgostsoba.rezervacija = 0
```
**Columns:** `SID` (count of distinct occupied rooms)
**Source tables:** `relgostsoba`

---

### 3.3 `folio`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2652`

```sql
SELECT posjetafolio.SID, posjetafolio.zakljucen FROM posjetafolio
WHERE posjetafolio.zakljucen = 0
```
**Columns:** `SID`, `zakljucen`
**Source tables:** `posjetafolio`
**Purpose:** Returns open (unlocked) folios

---

### 3.4 `gostipid`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2671`

```sql
SELECT gosti.ime, gosti.prezime, relgostsoba.checkInDate,
       relgostsoba.checkOutDate, relgostsoba.odjavljen,
       relgostsoba.tarifaID, relgostsoba.ID
FROM relgostsoba JOIN gosti ON relgostsoba.gostID = gosti.ID
WHERE relgostsoba.PID = 0 AND relgostsoba.ID <> 0
```
**Columns:** `ime`, `prezime`, `checkInDate`, `checkOutDate`, `odjavljen`, `tarifaID`, `ID`
**Source tables:** `relgostsoba`, `gosti`

---

### 3.5 `pid`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2690`

```sql
SELECT posjetafolio.ID, posjetafolio.SID, posjetafolio.zakljucen
FROM posjetafolio WHERE posjetafolio.zakljucen = 0
```
**Columns:** `ID`, `SID`, `zakljucen`
**Source tables:** `posjetafolio`
**Purpose:** Returns open folio IDs with room ID

---

### 3.6 `radniksmjena`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2709`

```sql
SELECT smjene.smjenaID, smjene.start, radnici.ime, smjene.radnik
FROM radnici JOIN smjene ON radnici.ID = smjene.radnik
ORDER BY smjene.start DESC LIMIT 1
```
**Columns:** `smjenaID`, `start`, `ime`, `radnik`
**Source tables:** `radnici`, `smjene`
**Purpose:** Returns the most recent shift with worker name

---

### 3.7 `sobev`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2728`

```sql
SELECT sobe.naziv, sobe.vrsta, sobe.lokal, sobavrsta.naziv AS vrstanaziv,
       sobavrsta.brojKreveta, sobavrsta.defaultTarifa
FROM sobe JOIN sobavrsta ON sobe.vrsta = sobavrsta.ID
```
**Columns:** `naziv`, `vrsta`, `lokal`, `vrstanaziv`, `brojKreveta`, `defaultTarifa`
**Source tables:** `sobe`, `sobavrsta`

---

### 3.8 `troskovisuma`
**Source:** `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql:2747`

```sql
SELECT troskovivrste.naziv, SUM(troskovi.iznos) AS iznosSuma
FROM troskovi JOIN troskovivrste ON troskovi.TID = troskovivrste.ID
GROUP BY troskovivrste.naziv
```
**Columns:** `naziv`, `iznosSuma`
**Source tables:** `troskovi`, `troskovivrste`

---

## 4. Triggers

**No triggers were found in the dump.** All business logic was implemented at the application layer.

---

## 5. Key Business Tables Classification

### 5.1 Room/Hotel Tables
| Table | Purpose |
|-------|---------|
| `sobe` | Room master list (room number, type, OOO status, clean status, building) |
| `sobavrsta` | Room type/floor definition (name, bed count, default tariff) |
| `sobatarifa` | Tariff rates (price per night per tariff plan) |
| `sobasadrzaji` | Room amenity definitions (with default tariff FK) |
| `relsobavrstasadrzaj` | Room-to-amenity junction table |
| `relsobavrstasobatarifa` | Room type-to-tariff junction table |
| `zgrade` | Buildings/wings |
| `kontroler` | Room controller devices (keycard system integration) |
| `sobaricalog` | Keycard system action log per room |

### 5.2 Guest Tables
| Table | Purpose |
|-------|---------|
| `gosti` | Guest master data (name, DOB, nationality, document, contact) |
| `gostdokument` | Document type lookup (Pasos, Licna, Vozacka, Ostalo) |
| `goststatus` | Guest status types with tax amounts (Turist, Dijete, etc.) |
| `gostiknjiga` | Guest registration book (police/tourist registry reporting) |
| `isprave` | ID document types lookup (passport, ID card, etc.) |
| `drzave` | Countries lookup (with domestic flag, phone codes) |

### 5.3 Reservation Tables
| Table | Purpose |
|-------|---------|
| `rezervacije` | Reservation headers (guest, dates, status, group, notes) |
| `rezervacijasobe` | Reserved rooms per reservation |
| `rezervacijasobe1` | Staging/temp copy of rezervacijasobe |
| `rezervacije1` | Staging/temp copy of rezervacije |
| `rezervacijaprijava` | Reservation-to-check-in link |
| `rezervacijegrupe` | Reservation group names (tour groups) |
| `rezervacijeizvor` | Reservation source types (mail, fax, phone) |
| `rezervacijetip` | Reservation type lookup |
| `komercijalista` | Commercial agents/agencies |

### 5.4 Stay/Check-in Tables
| Table | Purpose |
|-------|---------|
| `relgostsoba` | **CORE**: Guest-room stay records (check-in/out dates, worker, reservation flag, tariff, discount) |
| `posjetafolio` | Visit folio (groups expenses/payments per stay; open/closed status) |
| `posjete` | Visits (appears to be an auxiliary visit tracking table) |
| `nocenja` | Individual accommodation night entries (per-stay, per-room night ledger) |

### 5.5 Expense/Cost Tables
| Table | Purpose |
|-------|---------|
| `troskovi` | Individual guest charges (room service, restaurant, bar, etc.) |
| `troskovivrste` | Expense type catalog (Nocenje, Mini Bar, Restoran, etc.) |
| `troskovipojedinacni` | Individual cost breakdowns |
| `troskovisuma` | **VIEW**: Sum of costs grouped by type |

### 5.6 Payment Tables
| Table | Purpose |
|-------|---------|
| `placanje` | Payment headers (amount, method, date, folio link, room link) |
| `placanjedetalji` | Payment line items with per-item details |
| `placanjenacin` | Payment method lookup (Cash, Virman, Kartica, Gratis, Slozeno) |
| `placanjeslozeno` | Compound payment method breakdowns |
| `neplaceni` | Unpaid items tracking (per stay/room/type) |
| `neplaceniplacanja` | Unpaid payment totals (amount due vs. advance) |

### 5.7 Invoice/Receipt Tables
| Table | Purpose |
|-------|---------|
| `printracuni` | Printed invoice headers (fiscal receipts) |
| `printracunidetalji` | Printed invoice line items |
| `printracunifooter` | Printed invoice footer data |
| `printracuniavans` | Advance payment invoice records |
| `printracspec` | Special invoice print records |
| `predracuni` | Proforma invoices (predračuni) |
| `predracunidet` | Proforma invoice line items |
| `rac` | Invoice records (alternate/newer invoice table) |
| `zaglavlje` | Invoice header/company branding data |
| `fisc` | Fiscal receipt number tracking |

### 5.8 Configuration/Lookup Tables
| Table | Purpose |
|-------|---------|
| `setings` | Multi-hotel configuration (PDV rate, tax, child policies, etc.) |
| `porezi` | Tax rates (boravišna taksa, insurance) |
| `kursna` | Currency exchange rates |
| `konta` | Chart of accounts mapping (for cost types) |
| `sifarnik` | General codebook (items with pricing references) |
| `tarifatxe` | Tax/tariff configuration |
| `partneri` | Business partners (travel agencies, companies) |
| `radnici` | Staff users (username, password hash, access level) |
| `smjene` | Work shifts |
| `napomena` / `napomenad` | Notes/annotations |
| `mailkonfig` | Email server configuration |
| `godine` | Year/season definitions |
| `kard` | Key card tracking |

### 5.9 Audit/Log Tables
| Table | Purpose |
|-------|---------|
| `logs` | Application audit log (all user actions) |
| `logloby` | Lobby action log |
| `logrestoran` | Restaurant action log |
| `logradnici` | Staff login/logout log |
| `logcont` | Controller (keycard) communication log |

### 5.10 Auxiliary/Other Tables
| Table | Purpose |
|-------|---------|
| `telefonskiimenik` | Hotel phone directory |
| `telpozivi` / `telpozivi_stara` | Room telephone call logs |
| `alarm` | Scheduled alarms/reminders |
| `export` | Export batch tracking |
| `sifarnik` | General product/service codebook |

---

## 6. Priority Tables for P0 Flows

### P0-Critical (Must implement first)

| Priority | Table | Flow | Rationale |
|----------|-------|------|-----------|
| **P0-1** | `sobe` | Room Status | All flows depend on room existence and availability (OOO, clean status) |
| **P0-2** | `relgostsoba` | Check-in / Check-out | Core guest-room assignment; `odjavljen`, `rezervacija`, `PID` drive the entire stay lifecycle |
| **P0-3** | `gosti` | Guest Registration | Guest identity required for check-in |
| **P0-4** | `posjetafolio` | Folio Management | Open/closed folio drives billing; `SID` links to rooms |
| **P0-5** | `troskovi` + `troskovivrste` | Expense Posting | Posting charges to guest rooms |
| **P0-6** | `placanje` + `placanjedetalji` + `placanjenacin` | Payment | Processing guest payments |
| **P0-7** | `nocenja` | Night Audit | Per-night accommodation charges |
| **P0-8** | `printracuni` + `printracunidetalji` | Invoice/Receipt | Generating fiscal invoices |
| **P0-9** | `radnici` | Authentication/Authorization | All operations require staff login |
| **P0-10** | `sobavrsta` + `sobatarifa` | Room Pricing | Tariff calculation for stays |

### P0-Key Relationships for Core Flows

```
Check-in Flow:
  gosti.ID  ──>  relgostsoba.gostID
  sobe.ID   ──>  relgostsoba.sobaID
  relgostsoba.ID ──> posjetafolio.SID (via sobe.ID)
  radnici.ID ──>  relgostsoba.checkInRadnik

Expense Posting Flow:
  troskovi.GSID  ──>  relgostsoba.ID  (stay reference)
  troskovi.SID   ──>  sobe.ID         (room reference)
  troskovi.TID   ──>  troskovivrste.ID (expense type)

Payment Flow:
  placanje.relGostSobaID ──> relgostsoba.ID
  placanje.PID           ──> posjetafolio.ID
  placanje.nacin         ──> placanjenacin.ID

Invoice Flow:
  printracuni.BrojRacuna ──> printracunidetalji.BrojRacuna
  placanje.broj          ──> printracuni.BrojRacuna

Night Audit Flow:
  nocenja.RID ──> relgostsoba.ID
  nocenja.SID ──> sobe.ID
  nocenja.PID ──> posjetafolio.ID
```

### Table Count Summary

| Category | Count |
|----------|-------|
| **Tables** | 47 (excluding 3 staging/duplicate tables: rezervacijasobe1, rezervacije1, sobavrsta1) |
| **Views** | 8 |
| **Stored Procedures** | 0 |
| **Triggers** | 0 |
| **Total Database Objects** | 55 |