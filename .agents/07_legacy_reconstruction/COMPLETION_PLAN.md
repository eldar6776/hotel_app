# Plan za dovršetak legacy analize za kompletnu implementaciju

## Trenutno stanje

Imamo 16 dokumenata (~540KB) koji pokrivaju P0 i P1 tokove na nivou "šta kod radi".
Ali nemamo dovoljno detalja za SAMOSTALNU implementaciju bez pristupa legacy kodu.

## Šta FALI (gap analiza)

### GAP 1: Stored Procedures nisu izvučene
- ModuleKod.vb kreira 40+ stored procedure-a dinamički u kodu
- SQL dump možda ima kreirane procedure ali nismo provjerili
- Bez ovih ne znamo tačnu logiku calculate/validate funkcija

### GAP 2: frmRacuni.vb duboka analiza (227KB)
- Ovo je SRŽ sistema - fakturacija, stampa, sve operacije racuna
- Pokriveno u 16_INVOICE_CHECKOUT.md ali na nivou flow-a, ne na nivou svake funkcije
- Nedostaju: svaka validacija, svaki uslov, svaki IF/ELSE granul

### GAP 3: frmGlavni.vb (184KB) i frmBaza.vb (89KB)
- frmGlavni = glavni meni, navigacija, timer eventi, globalna stanja
- frmBaza = administracija baze, PF друге baze, sinhronizacija
- Nedostaju: koji global eventi pokreću koje tokove

### GAP 4: Cross-flow ovisnosti
- Znamo pojedinačne tokove ali ne znamo tačno:
  - Šta se desi kad se gost prijavi u sobu koja ima rezervaciju za drugog
  - Šta se desi kad se odradi partial checkout
  - Kako nocenje utiče na troškove i obrnuto
  - Koji checkboxi/tasteri blokiraju koje radnje

### GAP 5: Settings/Konfiguracija nije dekodirana
- `setings` tabela koristi `#` i `*` delimiters za compound vrijednosti
- Koje postavke utiču na koje tokove nije mapirano

### GAP 6: RDLC/Crystal report layout nije dokumentovan
- Znamo KOJE izvještaje apli stampa, ne ZAKO izgledaju
- Za implementaciju treba tačan sadržaj svakog izvještaja

### GAP 7: Domain model → legacy column mapping
- Nemamo dokument "koja legacy kolona odgovara kom modernom konceptu"
- Npr: `relgostsoba.prijava` = 0/1/2 ali šta to znači u modernom domain modelu

---

## Plan dovršetka - 7 koraka

---

### KORAK 1: Izvući stored procedures iz SQL dump-a

**Cilj:** Kompletna lista svih stored procedure-a sa njihovim SQL tijelom

**Ulaz:**
- `legacy_code/bin/Database Backup 2019-02-01 08-31-26.sql` (9MB)
- `02_MODULEKOD_FUNCTIONS.md` (već ima referencu na koje SP se kreiraju)

**Aktivnosti:**
1. Pretražiti SQL dump za `CREATE PROCEDURE` i `CREATE FUNCTION`
2. Za svaku SP: ime, parametri, SQL tijelo, koje tabele čita/piše
3. Spojiti sa ModuleKod funkcijama koje pozivaju te SP
4. Identifikovati SP koje se kreiraju dinamički u kodu (ModuleKod linije)

**Output:** `03_STORED_PROCEDURES.md`

**Vrijeme:** ~2h

**Validacija:** Svaka SP ima: ime, parametre, body, tabele, pozivaoc

---

### KORAK 2: Duboka analiza frmRacuni.vb i frmGlavni.vb

**Cilj:** Kompletna funkcionalna specifikacija za fakturaciju i glavni meni

**Ulaz:**
- `legacy_code/frmRacuni.vb` (227KB)
- `legacy_code/frmGlavni.vb` (184KB)
- `legacy_code/frmBaza.vb` (89KB)
- `16_INVOICE_CHECKOUT.md` (već postoji)

**Aktivnosti:**
1. frmRacuni.vb: izvući SVAKU funkciju, SVAKI SQL, SVAKI IF uslov, SVAKI MsgBox
2. frmGlavni.vb: mapirati navigaciju, timere, globalne evente, menu stavke
3. frmBaza.vb: mapirati administrativne operacije, sinhronizaciju
4. Spojiti sa postojećom analizom

**Output:** 
- `16_INVOICE_CHECKOUT_DEEP.md` (nadogradnja postojećeg)
- `24_MAIN_MENU_ADMIN.md`

**Vrijeme:** ~4h

**Validacija:** Svaka funkcija dokumentovana, svaki IF imenuje poslovno pravilo

---

### KORAK 3: Cross-flow dependency map

**Cilj:** Mapirati kako tokovi utiču jedan na drugi

**Aktivnosti:**
1. Za svaku formu, izvući koje globalne varijable čita/piše
2. Mapirati: check-in utiče na room status koji utiče na reservation prikaz
3. Mapirati: payment utiče na invoice koji utiče na fiscal
4. Mapirati: expense zakljucavanje utiče na checkout
5. Identifikovati conflict scenarios (šta kad dva gosta plate istu sobu)

**Ulaz:** Svi postojeći dokumenti + ModuleKod + Data.vb

**Output:** `35_CROSS_FLOW_DEPENDENCIES.md`

**Struktura:**
```
## Check-in → impact na:
- Room status (sobe.status, sobe.clean)
- Reservation (stornirana, potvrda)
- Expense (novi troskovi se automatski kreiraju?)
- Night (nocenje se generiše)

## Payment → impact na:
- Invoice (placanje se vezuje za racun)
- Fiscal (fiskalizacija se pokrece)

## Checkout → impact na:
- Room status (sobe.status → 2)
- Night (nocenje se zakljucava)
- Expense (troskovi.zaklj → 1)

## Conflict scenarios:
- Gost A u sobi, gost B ima rezervaciju za istu sobu
- Partial checkout - što se desi sa ostatkom
- Storno racuna kad je vec fiskalizovan
```

**Vrijeme:** ~3h

**Validacija:** Svaki dependency ima `legacy_code/file:line` referencu

---

### KORAK 4: Settings/konfiguracija dekodiranje

**Cilj:** Kompletna mapa šta svaka konfiguracijska vrijednost radi

**Ulaz:**
- `legacy_code/frmpostavke.vb` (83KB)
- `legacy_code/Settings.Designer.vb` (27KB)
- `legacy_code/app.config`
- `00_DATABASE_SCHEMA.md` (setings tabela)

**Aktivnosti:**
1. Izvući svaku postavku iz Settings.Designer.vb sa default vrijednošću
2. Izvući svaku kolonu iz `setings` tabele
3. Mapirati koje postavke utiču na koje tokove (grep po kodu)
4. Dekodirati compound vrijednosti (delimiter `#` i `*`)

**Output:** `25_SETTINGS_CONFIGURATION.md`

**Struktura:**
```
| Setting Key | Type | Default | Used In | Business Meaning |
|-------------|------|---------|---------|-----------------|
| ipAdres | String | localhost | frmKardPro, kard_imedia | Card reader IP |
| ... | ... | ... | ... | ... |
```

**Vrijeme:** ~2h

---

### KORAK 5: Domain model mapping (legacy → modern)

**Cilj:** Precizna mapa "legacy kolona → moderni koncept" za implementaciju

**Aktivnosti:**
1. Za svaku tabelu iz 00_DATABASE_SCHEMA.md, imenovati moderni domain koncept
2. Za svaku kolonu, imenovati moderni atribut sa business značenjem
3. Identifikovati kolone koje treba spojiti, razdvojiti, ili ignorisati
4. Identifikovati redundanse (iste podatke u 2 tabele)
5. Mapirati status kodove na moderne enum vrijednosti

**Ulaz:** Svi postojeći dokumenti

**Output:** `50_DOMAIN_MODEL_MAPPING.md`

**Struktura:**
```
## Modern Domain Entity: Room
Legacy table: sobe

| Legacy Column | Modern Attribute | Type | Notes |
|---------------|-----------------|------|-------|
| sobe.broj | roomNumber | String | Primary business key |
| sobe.status | RoomStatus | Enum | 0=Available, 1=Occupied, 2=CheckoutDue, ... |
| sobe.clean | isClean | Boolean | 0=clean, 1=dirty |
| sobe.ooo | isOutOfOrder | Boolean | ... |
| ... | ... | ... | ... |

## Status Enum Mapping

| Enum: RoomStatus | Value | Legacy Meaning | Used In |
|------------------|-------|---------------|---------|
| Available | 0 | Slobodna | fnSobaStatus |
| Occupied | 1 | Zauzeta | frmPrijava1 |
| ... | ... | ... | ... |
```

**Vrijeme:** ~4h

**Validacija:** Svaka legacy kolona ima moderni pandan, svaki status kod ima enum

---

### KORAK 6: API specification za P0 tokove

**Cilj:** Definisati ekzakatan API koji treba implementirati

**Aktivnosti:**
1. Za svaki P0 flow iz 40_GOLDEN_SCENARIOS.md, definisati REST API endpointe
2. Za svaki endpoint: request body, response body, status codes, validacije
3. Definisati Error kodove bazirane na legacy MsgBox porukama
4. Definisati Webhook/Event tokove (šta se desi kad se soba promijeni status)

**Ulaz:** 40_GOLDEN_SCENARIOS.md + 30_STATUS_MATRIX.md + svi flow dokumenti

**Output:** `60_API_SPECIFICATION.md`

**Struktura:**
```
### POST /api/v1/check-in
Request:
  guestId: int (required)
  roomId: string (required)
  checkInDate: date (required)
  numberOfGuests: int
  reservationId: int? (if from reservation)

Response 200:
  stayId: int
  roomId: string
  roomStatus: "occupied"

Response 400:
  code: "ROOM_NOT_AVAILABLE"
  message: "Room is currently occupied"

Validations:
  - Room must be status 0 or 3 (legacy: fnSobaStatus)
  - Guest must exist in gosti table
  - If reservationId provided, must be confirmed (potvrda=1)

Legacy evidence:
  - frmPrijava1.vb:2195 (check for room status)
  - ModuleKod.vb:156 (fnSobaStatus)
```

**Vrijeme:** ~5h

**Validacija:** Svaki endpoint ima legacy evidence, svaki error kod ima porijeklo

---

### KORAK 7: Izvještaji - sadržaj i format

**Cilj:** Dokumentovati šta SVAKI izvještaj sadrži (kolone, redove, grupisanje)

**Ulaz:**
- `21_REPORTS.md` (već postoji - ima listu izvještaja)
- Svi `rptXxx.vb` fajlovi koji definišu data source
- Svi `.rdlc` i `bin/XxxXml` fajlovi koji definišu layout

**Aktivnosti:**
1. Za svaki izvještaj: imena data table/kolone, grupisanje, sortiranje
2. Za RDLC izvještaje (rptRacun.rdlc, rptRacun1.rdlc, rptRacunDet.rdlc): pročitati XML i izvući tačne kolone
3. Identifikovati koji izvještaji su P0 (racuni, prijava) vs P1/P2

**Output:** `45_REPORTS_CONTENT.md`

**Vrijeme:** ~3h

---

## Ukupan pregled koraka

| Korak | Opis | Output fajl | Vrijeme | Prioritet |
|-------|-------|-------------|---------|-----------|
| 1 | Stored Procedures | 03_STORED_PROCEDURES.md | 2h | HIGH |
| 2 | frmRacuni+frmGlavni+frmBaza deep | 16_DEEP + 24_MAIN_MENU_ADMIN.md | 4h | HIGH |
| 3 | Cross-flow dependencies | 35_CROSS_FLOW_DEPS.md | 3h | HIGH |
| 4 | Settings/konfiguracija | 25_SETTINGS_CONFIG.md | 2h | MEDIUM |
| 5 | Domain model mapping | 50_DOMAIN_MODEL_MAPPING.md | 4h | HIGH |
| 6 | API specification | 60_API_SPECIFICATION.md | 5h | HIGH |
| 7 | Reports content | 45_REPORTS_CONTENT.md | 3h | MEDIUM |
| **UKUPNO** | | **7 novih dokumenata** | **~23h** | |

## Redosled izvršavanja

```
Korak 1 (SP) ─┐
               ├─→ Korak 3 (Cross-flow) ─→ Korak 5 (Domain) ─→ Korak 6 (API)
Korak 2 (Deep)─┘                                                    ↓
                                                              Korak 7 (Reports)
Korak 4 (Settings) ──→ Korak 5 (Domain)
```

- Korak 1 i 2 mogu paralelno
- Korak 3 zavisi od 1 i 2
- Korak 4 može paralelno sa 1-3
- Korak 5 zavisi od 1,2,3,4
- Korak 6 zavisi od 5
- Korak 7 može paralelno sa 5 i 6

## Kada je "dovoljno" za implementaciju?

**MINIMUM za početak implementacije (Koraci 1+2+5):**
- Stored procedures → znamo tačnu logiku baze
- Deep analysis frmRacuni/Glavni → znamo kompletnu fakturaciju i navigaciju
- Domain model mapping → znamo šta gradimo

**PUNO za kompletnu implementaciju (svi koraci):**
- Svi koraci + API specifikacija = implementacija može početi bez potrebe za dodatnim čitanjem legacy koda

## Kako validirati svaki korak

Svaki output dokument mora zadovoljiti:
1. **Evidence test**: Svaka tvrdnja ima `legacy_code/file:line` referencu
2. **Completeness test**: Nema "UNKNOWN" u kritičnim mjestima (P0 statusi, P0 tokovi)
3. **Contradiction test**: Ne kontradiktuje drugi dokument
4. **Implementation test**: Developer može pročitati dokument i implementirati bez dodatnih pitanja

---

### KORAK 8: Commit i push na GitHub

**Cilj:** Sva analiza bude dostupna na GitHubu za daljni rad

**Aktivnosti:**
1. `git add .` - dodati sve nove i izmijenjene fajlove
2. `git commit -m "feat: legacy analysis complete - 16 docs + completion plan + 7 new steps pending"`
3. `git push origin main` (ili odgovarajuću branch)

**Fajlovi za commit:**

Postojeći (već stage-ani):
- `LEGACY_ANALYSIS/00_DATABASE_SCHEMA.md`
- `LEGACY_ANALYSIS/01_GLOBALS.md`
- `LEGACY_ANALYSIS/02_MODULEKOD_FUNCTIONS.md`
- `LEGACY_ANALYSIS/10_CHECKIN.md`
- `LEGACY_ANALYSIS/12_ROOM_STATUS.md`
- `LEGACY_ANALYSIS/13_RESERVATIONS.md`
- `LEGACY_ANALYSIS/14_PAYMENT.md`
- `LEGACY_ANALYSIS/15_EXPENSES_NIGHTS.md`
- `LEGACY_ANALYSIS/16_INVOICE_CHECKOUT.md`
- `LEGACY_ANALYSIS/17_FISCAL_PROFORMA.md`
- `LEGACY_ANALYSIS/20_GUESTS.md`
- `LEGACY_ANALYSIS/21_REPORTS.md`
- `LEGACY_ANALYSIS/22_PARTNERS_TARIFFS_SETTINGS.md`
- `LEGACY_ANALYSIS/23_CARDS_EXPORT_MISC.md`
- `LEGACY_ANALYSIS/30_STATUS_MATRIX.md`
- `LEGACY_ANALYSIS/40_GOLDEN_SCENARIOS.md`
- `ANALYSIS_PLAN.md`
- `COMPLETION_PLAN.md`

Novi (nakon koraka 1-7):
- `LEGACY_ANALYSIS/03_STORED_PROCEDURES.md`
- `LEGACY_ANALYSIS/24_MAIN_MENU_ADMIN.md`
- `LEGACY_ANALYSIS/25_SETTINGS_CONFIGURATION.md`
- `LEGACY_ANALYSIS/35_CROSS_FLOW_DEPENDENCIES.md`
- `LEGACY_ANALYSIS/45_REPORTS_CONTENT.md`
- `LEGACY_ANALYSIS/50_DOMAIN_MODEL_MAPPING.md`
- `LEGACY_ANALYSIS/60_API_SPECIFICATION.md`

**VAŽNO:** Commit se radi NAKON što se svi koraci 1-7 završe,
i prije toga se provjeri da nema secrets/credentials u fajlovima.