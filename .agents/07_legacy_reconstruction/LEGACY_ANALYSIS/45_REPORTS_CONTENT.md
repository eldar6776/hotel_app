# Reports Content — Complete Data Field Analysis

> Source files analyzed:
> - `legacy_code/rptRacunFrm.vb` — Invoice report form (395 lines)
> - `legacy_code/rptRacun.rdlc` — Invoice RDLC template (1649 lines)
> - `legacy_code/rptRacun1.rdlc` — Invoice RDLC template variant (1319 lines)
> - `legacy_code/rptRacunDet.rdlc` — Invoice detail RDLC template (599 lines)
> - `legacy_code/iMediaIzvjestaj.xsd` — Typed dataset schema for invoice reports (80 lines)
> - `legacy_code/bin/DnevniPlacanje.xml` — Daily payment sample data
> - `legacy_code/bin/GostiLista.xml` — Guest list sample data
> - `legacy_code/bin/sobaricaShema.xml` — Housekeeping schema
> - `legacy_code/bin/IzvjestajRezervacijaPoj.xml` — Reservation report schema
> - `legacy_code/bin/TelefonskiRacun.xml` — Phone bill schema
> - `legacy_code/troskoviVrste.xml` — Expense types reference data
> - `legacy_code/bin/printGore.xml`, `printSredina.xml`, `printFooter.xml` — Invoice data tables schema
> - `legacy_code/bin/IzvjestajNaplata.xml` — Payment report schema
> - `legacy_code/bin/DnevniSUB.xml` — Payment summary schema
> - `legacy_code/bin/trenutniDnevni.xml` — Daily P&L schema
> - Cross-reference: `LEGACY_ANALYSIS/21_REPORTS.md`

---

## 1. P0 Reports (MUST implement first)

### 1.1 Invoice Reports (rptRacun.rdlc, rptRacun1.rdlc, rptRacunDet.rdlc)

Three RDLC templates share the same data source (`iMediaIzvjestaj_izvjestaj30` / `iMediaIzvjestaj_izvjestaj15`) from the typed dataset `iMediaIzvjestaj` defined in `iMediaIzvjestaj.xsd`.

#### 1.1.1 Dataset: `izvjestaj30` (used by rptRacun.rdlc and rptRacun1.rdlc)

Source: `iMediaIzvjestaj.xsd:37-76`

| Field | XSD Type | RDLC Type (rptRacun) | RDLC Type (rptRacun1) | Business Meaning | Mapped From |
|-------|----------|---------------------|----------------------|------------------|-------------|
| `d0` | xs:string | System.String | System.String | Currency symbol (e.g. "KM", "€") | `curr` (currency symbol from culture) — `rptRacunFrm.vb:112,120` |
| `d1` | xs:string | System.String | System.String | Expense/line item name (Trosak) | `printSredina` column 1 — `rptRacunFrm.vb:121` |
| `d2` | xs:double | System.Double | System.String** | Quantity (Kol) | `printSredina` column 2 — `rptRacunFrm.vb:122` |
| `d3` | xs:double | System.Double | System.String | Unit price without VAT (CijBezPdv) | `printSredina` column 3 — `rptRacunFrm.vb:123` |
| `d4` | xs:double | System.Double | System.String | Total without VAT (UkupnoBezPdv) | `printSredina` column 4 — `rptRacunFrm.vb:124` |
| `d5` | xs:string | System.String | System.String | VAT rate (Pdv) — percentage string | `printSredina` column 5 — `rptRacunFrm.vb:125` |
| `d6` | xs:double | System.Double | System.String | VAT amount (IznosPdv) | `printSredina` column 6 — `rptRacunFrm.vb:126` |
| `d7` | xs:double | System.Double | System.String | Total with VAT (Ukupno) | `printSredina` column 7 — `rptRacunFrm.vb:127` |
| `d8` | xs:string | System.String | System.String | Payment method name (Nacin) | `printSredina` column 8 / predracuni `vrplac` — `rptRacunFrm.vb:128,224` |
| `d9` | xs:string | System.String | System.String | Exchange rate/conversion denominator | `printSredina` column 9 / predracuni `Popust` — `rptRacunFrm.vb:129,225,228` |
| `d10` | xs:string | System.String | System.String | Exchange rate currency code (e.g. "€") or "-" | `printSredina` column 10 / set to "-" default — `rptRacunFrm.vb:130,226` |
| `d11` | xs:double | System.Double | System.String | Discount percentage (Popust) | `printSredina` column 11 — `rptRacunFrm.vb:131` |
| `d12` | xs:string | System.String | System.String | Discount amount (Popust1) | `printSredina` column 12 — `rptRacunFrm.vb:132` (forced to 0 if non-numeric) |
| `d13` | xs:string | System.String | System.String | (Additional text/additional line) | `printSredina` column 13 — `rptRacunFrm.vb:134` |
| `d14` | xs:string | System.String | System.String | (Used in "For Pay" calculation: total minus discount) | `printSredina` column 14 — `rptRacunFrm.vb:135` |
| `d15` | xs:string | System.String | System.String | Worker/employee initials | `printSredina` column `rad` from row 0 — `rptRacunFrm.vb:136` |
| `d16` | xs:string | System.String | N/A | Line type flag: 0=normal, 1=Boravišna taksa (tourist tax), 2=Osiguranje (insurance) | Set programmatically: 0 default, 1 if d1 contains "Boravi", 2 if d1 contains "Osiguranje" — `rptRacunFrm.vb:138-145` |
| `f0` | xs:string | System.String | System.String | Invoice/receipt number | `printGore` column 0 — `rptRacunFrm.vb:150,240` |
| `f1` | xs:string | System.String | System.String | Guest name (Ime) | `printGore` column 1 / predracuni `ime` — `rptRacunFrm.vb:151,241`, merged guest names if `rimep=1` — `rptRacunFrm.vb:152-161` |
| `f2` | xs:string | System.String | System.String | (Additional guest info) | `printGore` column 2 — `rptRacunFrm.vb:162` |
| `f3` | xs:string | System.String | System.String | (Address/ID info) | `printGore` column 3 — `rptRacunFrm.vb:163` |
| `f4` | xs:string | System.String | System.String | (Address/ID info continued) | `printGore` column 4 — `rptRacunFrm.vb:164` |
| `f5` | xs:string | System.String | System.String | Company/organization name | `printGore` column 5 — `rptRacunFrm.vb:165` |
| `f6` | xs:string | System.String | System.String | Secondary name / company name (Frima) | `printGore` column 6 / predracuni `frima` — `rptRacunFrm.vb:166,246` |
| `f7` | xs:string | System.String | System.String | Invoice prefix/reference | `printGore` column 7 / predracuni `brojpred` — `rptRacunFrm.vb:167,247` |
| `f8` | xs:string | System.String | System.String | Invoice date (Datum) | `printGore` column 8 / predracuni `dadtum` — `rptRacunFrm.vb:168,248` |
| `f9` | xs:string | System.String | System.String | Connection/relationship info | `printGore` column 9 / predracuni `veza` — `rptRacunFrm.vb:169,249` |
| `f10` | xs:string | System.String | System.String | Payment method label | `printGore` column 10 / predracuni `nazivp` — `rptRacunFrm.vb:170,250` |
| `f11` | xs:string | System.String | System.String | Payment due date | `printGore` column 11 / "Valuta:" + predracuni `datumval` — `rptRacunFrm.vb:171,251` |
| `f12` | xs:string | System.String | System.String | Notes/memo | `printGore` column 12 / predracuni `napomnena` — `rptRacunFrm.vb:172,252` |
| `f13` | xs:string | System.String | System.String | (Additional info line) | `printGore` column 13 — `rptRacunFrm.vb:173` |
| `f14` | xs:string | System.String | System.String | (Additional info line 2) | `printGore` column 14 — `rptRacunFrm.vb:174` |
| `f15` | xs:string | System.String | System.String | Storno flag: 0=Storno, 1=Normal | Set to 0 if `akcij2="storno"`, else 1 — `rptRacunFrm.vb:176-180` |
| `lokac` | xs:string | N/A (rptRacun only) | N/A | Location/municipality for invoice | `setings` table `lokac` — `rptRacunFrm.vb:175,255` |

**Note**: In `rptRacun1.rdlc`, ALL fields are typed as `System.String` (lines 42-173), whereas `rptRacun.rdlc` uses mixed types (`System.Double` for d2-d7, d11). This indicates `rptRacun1` was created later and may have been simplified to avoid type conversion issues.

#### 1.1.2 Dataset: `izvjestaj15` (used by rptRacunDet.rdlc)

Source: `iMediaIzvjestaj.xsd:15-36`

| Field | XSD Type | RDLC Type | Business Meaning |
|-------|----------|-----------|------------------|
| `d0` | xs:string | System.String | Row number (RB) |
| `d1` | xs:string | System.String | Expense/item name or receipt number (shown in title) |
| `d2` | xs:string | System.String | Room number (Soba) |
| `d3` | xs:string | System.String | Category type (Vrsta) |
| `d4` | xs:string | System.String | Guest name (Ime) |
| `d5` | xs:string | System.String | Grouping/sort field — used for page break grouping |
| `d6` | xs:string | System.String | Date (Datum) |
| `d7` | xs:string | System.String | Service description (Usluga) |
| `d8` | xs:string | System.String | Amount (Ukupno) |
| `d9` | xs:string | System.String | (Additional detail) |
| `d10` | xs:string | System.String | (Additional detail 2) |
| `d11` | xs:string | System.String | (Additional detail 3) |
| `d12` | xs:string | System.String | (Additional detail 4) |
| `d13` | xs:string | System.String | (Additional detail 5) |
| `d14` | xs:string | System.String | Receipt number (shown as bold title via `=Fields!d14.Value`) |
| `d15` | xs:string | System.String | (Additional field) |

The detail report columns (from RDLC header labels `rptRacunDet.rdlc:370-525`):

| Column # | Header Label | Field Value | Sort |
|----------|-------------|-------------|------|
| 1 | R.B. | `=Fields!d0.Value` | — |
| 2 | Soba | `=Fields!d6.Value` | — |
| 3 | Vrsta | `=Fields!d7.Value` | — |
| 4 | Ime | `=Fields!d8.Value` | — |
| 5 | Datum | `=Fields!d9.Value` | — |
| 6 | Usluga | `=Fields!d10.Value` | — |
| 7 | Ukupno | `=Fields!d11.Value` | — |

Grouping: By `Fields!d5.Value` with `PageBreakAtEnd=true` (`rptRacunDet.rdlc:565-569`)

#### 1.1.3 Print Layout — rptRacun.rdlc (Invoice with VAT breakdown)

**Page Header** (`rptRacun.rdlc:17-33`):
- Background image: `file:///C:\Program Files\IMEDIA\HotelPro\logo.jpg`
- `f6` — Company name (guest second name line, with border lines visible only when non-empty)
- `f1` — Guest name (row 0, bold combined with `f0` and `f7`)
- `f10 & f0 & f7` — Combined title line (invoice type prefix + number + suffix)
- `f5` — Company/org name (underlined)
- `f11` — Left side info (address/notes)
- `f9` — Right side info
- `f8` — Date: "Datum fakture: " + f8 value

**Line Items Table** (`rptRacun.rdlc:776-1311`):

| Column # | Header (bilingual) | Field | Format |
|----------|---------------------|-------|--------|
| 1 | Trosak / Cost | `=Fields!d1.Value` | Text |
| 2 | J.mje. / Units | hardcoded "kom" | Text (static) |
| 3 | Kol. / Amo. | `=Fields!d2.Value` | Numeric F |
| 4 | Cij. / Pr. (unit price after discount) | `=FormatNumber((d7+d12)/d2, 2)` | Computed |
| 5 | Popust / Discou. (discount %) | `=Fields!d11.Value & "%"` | Text |
| 6 | bez PDV / Wo. TAX (total without VAT) | `=Fields!d3.Value` | Numeric F |
| 7 | Uk. bez PDV / Sum wo.TAX | `=Fields!d4.Value` (mapped as d3→d4 swap) | Numeric F |
| 8 | Stopa / Perc. (VAT rate) | `=CInt(Replace(d5,"%","")) & "%"` | Text |
| 9 | PDV / VAT (VAT amount) | `=Fields!d6.Value` | Numeric F |
| 10 | UKUPNO / TOTAL | `=Fields!d7.Value` | Numeric F |

**Summary Section** (`rptRacun.rdlc:1312-1585`):

Filtered on `d16 = 0` (normal items only — excludes tourist tax and insurance):

| Element | Label | Expression |
|---------|-------|------------|
| Total | Ukupno / Total | `=FormatNumber(sum(d7), 2) & " " & First(d0)` |
| Discount | Popust / Discount | `=Sum(cdbl(d12))` |
| Sum w/o VAT | Iznos bez PDV / Sum without TAX | `=Sum(cdec(d4.tostring))` |
| VAT | PDV / TAX | `=sum(d6)` |
| Net to Pay | ZA PLATITI / FOR PAY | `=FormatNumber(sum(d7)-cdbl(d14), 2) & " " & First(d0)` |
| Currency conversion (if applicable) | Total in foreign currency | `=FormatNumber(sum(d7)/CDbl(d9), 2) & " " & d10 & " /"` |
| Net in foreign currency | Net in foreign currency | `=FormatNumber((sum(d7)-cdbl(d14))/CDbl(d9), 2) & " " & d10 & " /"` |

Filtered on `d16 > 0` (tourist tax/insurance):

| Element | Expression |
|---------|------------|
| Label | `=iif(d16=1, "Boravišna taksa / Tourist tax", "Osiguranje / Insurance")` |
| Amount | `=FormatNumber(sum(d7), 2) & " " & First(d0)` |

**Storno Watermark**: Large gray "Storno" text displayed if `f15 = 0` (`rptRacun.rdlc:1346`)

**Footer**: Page footer with page numbering and `logo.jpg` background (`rptRacun.rdlc:1595-1648`)

#### 1.1.4 Print Layout — rptRacun1.rdlc (Simplified Invoice)

Similar structure to rptRacun but with these differences:
- Simpler line item table (9 columns instead of 10) — no computed unit price column
- All fields typed as `System.String` instead of mixed `Double/String`
- Column layout: Trosak, J.mje, Kol, Cij.bez PDV, Uk.bez PDV, Stopa, Iznos PDV, Popust, UKUPNO
- Dedicated summary section with explicit labels: "Iznos bez PDV / Sum without TAX", "PDV / TAX", "Popust / Discount", "ZA PLATITI / FOR PAY"
- Summary calculations: `Sum(cdec(d4.tostring))` for w/o VAT, `sum(d6)` for VAT, `Sum(cdbl(d12))` for discount, `FormatNumber(sum(d7)-cdbl(d14), 2)` for net total
- Same storno watermark and logo/potpis signature image pattern
- Page footer: "Hotel Management software - iMEDIA d.o.o. Sarajevo, www.ba"

#### 1.1.5 Print Layout — rptRacunDet.rdlc (Receipt Detail)

- Title: "Detalji računa br.:" + `=Fields!d14.Value` (receipt number)
- Subtitle: `=Fields!d1.Value` (large, slate gray)
- Data table sorted by `=Fields!d5.Value` (grouping field)
- Columns: R.B., Soba, Vrsta, Ime, Datum, Usluga, Ukupno
- Footer: Page "X od Y" numbering
- No VAT breakdown; simple line-item detail

#### 1.1.6 Data Flow: rptRacunFrm.vb → RDLC

The `print1()` method (`rptRacunFrm.vb:67-201`) orchestrates the data binding:

1. **Culture initialization** (`Init()` at `rptRacunFrm.vb:35-63`):
   - Sets decimal separator to `,`, group separator to `.`, 3 decimal digits
   - Sets date format to `dd.MM.yyyy`
   - Sets currency symbol from `ds.Tables("setings").Rows(0).Item("valuta")`

2. **Line items** (`rptRacunFrm.vb:118-147`): Iterates `ds.Tables("printSredina").Rows`, maps each column to `izvjestaj30` d0-d16 fields
   - Special logic at `rptRacunFrm.vb:139-142`: sets `d16=1` if `d1` contains "Boravi", `d16=2` if contains "Osiguranje"

3. **Header** (`rptRacunFrm.vb:148-181`): From `ds.Tables("printGore").Rows(0)`, maps columns 0-14 to f0-f14
   - Guest name merging at `rptRacunFrm.vb:152-161`: if `rimep=1`, concatenates `det` table guest names into `f1`
   - Storno flag at `rptRacunFrm.vb:176-180`: `f15=0` for storno, `f15=1` for normal
   - Location at `rptRacunFrm.vb:175`: `lokac` from settings table

4. **Preview/Print** (`rptRacunFrm.vb:291-380`): `Run()` → `Export()` → `Print()` pipeline renders to EMF images and sends to default printer with `akcij` copies

5. **Pre-invoice variant**: `printpredr()` (`rptRacunFrm.vb:203-269`) loads directly from `predracuni`/`predracunidet` tables via MySQL queries

#### 1.1.7 Source Tables for Invoice Data

The `ds` (DataSet) object referenced in `rptRacunFrm.vb` contains these tables populated before the form is opened:

| DataSet Table | Source | Columns (from printGore.xml/printSredina.xml schema) | Used As |
|--------------|--------|------------------------------------------------------|---------|
| `printGore` | Receipt header data | `_x0030_` through `_x0039_` (0-9), `_x0031_0` (10), `_x0031_1` (11), `napom` (12), `avans` (13), `ranpla` (14) | Invoice header → f0-f14 |
| `printSredina` | Receipt line items | `_x0030_` through `_x0039_` (0-9), `_x0031_0` (10), `_x0031_1` (11), `_x0031_2` (12), `_x0031_3` (13), `avan` (14), `rad` (15) | Invoice lines → d0-d16 |
| `printFooter` | Receipt footer | `_x0030_` through `_x0034_` (0-4) | (Not directly used in RDLC) |
| `setings` | Hotel settings | `lokac`, `valuta`, `cultur` | Location name, currency symbol, culture |
| `det` | Guest details | `ime` column used for name merging | Guest name list |

---

### 1.2 Daily Payment Report (DnevniPlacanje.xml)

Source: `legacy_code/bin/DnevniPlacanje.xml`

| Field | XML Sample Type | Business Meaning |
|-------|----------------|------------------|
| `broj` | integer | Receipt number (BrojRacuna) |
| `datum` | dateTime | Receipt date |
| `naziv` | string (nullable) | Room number (can be empty) |
| `ImePrezime` | string | Guest full name |
| `NazivOstalo` | string (nullable) | Company/organization name + address + ID |
| `iznos` | decimal | Receipt total amount |
| `nacin` | string | Payment method: "Cash", "Kartica" (card), "Virman" (bank transfer), "Slozeno(...)" (mixed) |
| `datumOD` | dateTime | Report date range start |
| `datumDO` | dateTime | Report date range end |
| `storno` | byte | 0=normal, non-zero=storno (cancelled receipt) |

**Grouping/Sorting**: By date (implicit from SP ordering). Storno receipts are included with negated amounts or special handling.

**SP Source**: `getIzvjestajDnevniPlacanje` or `getIzvjestajDnevniPlacanjeN` (filtered by method). Advance payments from `printracuniavans` table are merged in via `ImportRow`.

---

### 1.3 Guest List (GostiLista.xml)

Source: `legacy_code/bin/GostiLista.xml`

| Field | XML Type | Business Meaning |
|-------|----------|------------------|
| `tid` | int | Guest transaction ID |
| `naziv` | string | Room number |
| `ImePrezime` | string | Guest full name |
| `usluga` | string | Service name (e.g. "Nocenje sa doruckom") |
| `grupa` | string | Group name (or "Nema podataka") |
| `naziv1` | string | Additional name info (or "Nema podataka") |
| `checkInDate` | dateTime | Check-in timestamp |
| `checkOutDate` | dateTime | Check-out timestamp |
| `napomenapl` | string | Reservation notes (free text, may contain booking reference, payment info) |
| `cijena` | decimal | Price per night |
| `gostID` | int | Guest ID |
| `ID` | int | Stay/registration ID |
| `status` | int | Status code (1=occupied per `21_REPORTS.md`) |
| `ime` | string | First name |
| `prezime` | string | Last name |
| `adresa` | string | Address (misused as expiration date in sample!) |
| `datumRodjenja` | dateTime | Date of birth |
| `mjestodrzavaR` | string | Place/country of residence |
| `pol` | string | Gender ("M"/"Z") |
| `drzavljanstvo` | string | Nationality/citizenship |
| `DID` | int | Document type ID |
| `dokument` | int | Document type code (1=passport, 4=ID card, 6=?) |
| `brDokument` | string | Document number |

**Purpose**: Tourist registration report — lists all currently occupied rooms with full guest details for national tourist registration requirements.

---

### 1.4 Reservation Report (IzvjestajRezervacijaPoj.xml)

Source: `legacy_code/bin/IzvjestajRezervacijaPoj.xml`

| Field | XML Type | Max Length | Business Meaning |
|-------|----------|-----------|------------------|
| `GID` | decimal | — | Guest ID |
| `ID` | int | — | Reservation ID |
| `ime` | string | 30 | Guest first name |
| `prezime` | string | 30 | Guest last name |
| `telefon` | string | 20 | Phone number |
| `BrojRezervacija` | int | — | Number of reservations |
| `checkInDate` | dateTime | — | Check-in date |
| `checkOutDate` | dateTime | — | Check-out date |
| `sobaVrstaID` | int | — | Room type ID |
| `naziv` | string | 30 | Room type name |
| `Broj_Potvrde` | int | — | Confirmation number |
| `Potvrda` | byte | — | Confirmation flag |
| `brojStorna` | int | — | Cancellation number |
| `stornirana` | byte | — | Cancelled flag (0/1) |
| `tarifa` | int | — | Rate/tariff |
| `radnik` | string | 50 | Employee who created reservation |
| `naziv1` | string | 50 | Room/suite name |
| `naziv2` | string | 50 | (Additional info) |
| `naziv3` | string | 50 | (Additional info) |
| `memo` | string | 21845 | Notes (large text field) |
| `ID1` | unsignedInt | — | (Additional ID) |

---

### 1.5 Housekeeping Report (sobaricaShema.xml)

Source: `legacy_code/bin/sobaricaShema.xml`

| Field | XML Type | Business Meaning |
|-------|----------|------------------|
| `sobaID` | int | Room ID |
| `sobaNaziv` | string | Room number/name |
| `sobavrstaID` | int | Room type ID |
| `sobavrstaNaziv` | string | Room type name |
| `status` | int | Status code (0-5) |
| `ooo` | byte | Out of order flag |
| `objekat` | int | Building/object ID |
| `clean` | byte | Clean/dirty flag |
| `tekst` | string | Additional text/notes |
| `statusNaziv` | string | Status label (VAC/OCC/CO-UNK/R/R-OCC/OOO) |

See `21_REPORTS.md:210-220` for status code mapping.

---

### 1.6 Phone Bill Report (TelefonskiRacun.xml)

Source: `legacy_code/bin/TelefonskiRacun.xml`

| Field | XML Type | Business Meaning |
|-------|----------|------------------|
| `lokal` | int | Room/extension number |
| `datum` | dateTime | Call date |
| `vrijeme` | dateTime | Call time |
| `trajanjePoziva` | string | Call duration (formatted string) |
| `telefonskiBroj` | string | Dialed phone number |
| `pozBroj` | string | Call type/prefix code |
| `Drzava` | string | Country/destination |
| `Cijena` | decimal | Call cost |

---

### 1.7 Expense Types Reference (troskoviVrste.xml)

Source: `legacy_code/troskoviVrste.xml`

| ID | naziv (Expense Type Name) |
|----|--------------------------|
| 1 | Nocenje (Night/lodging) |
| 2 | Najam (Rental) |
| 3 | Restoran (Restaurant) |
| 4 | Lobby Bar |
| 5 | Mini Bar |
| 6 | Telefon (Phone) |
| 7 | Sauna |
| 8 | Solarij (Solarium) |
| 9 | Masaža (Massage) |
| 10 | Fitness |
| 11 | Frizerski (Hairdresser) |
| 12 | Taxi |
| 13 | Cvijece (Flowers) |
| 14 | Laptop |
| 15 | Trafika (Newsstand/kiosk) |
| 16 | Rent A Car |
| 17 | Autopraonica (Car wash) |
| 18 | Ostalo (Other) |
| 19 | Zakup sale (Hall rental) |
| 20 | Stampanje materijala (Printing) |
| 21 | Prevoz (Transport) |
| 22 | Fax |
| 23 | Autobuski prevoz (Bus transport) |
| 24 | Internet |
| 25 | Neplaceno Nocenje (Unpaid night) |
| 26 | telefonski troškovi (Phone expenses) |

---

### 1.8 Additional Report Schemas

#### IzvjestajNaplata.xml (Payment Summary)

| Field | Type | Business Meaning |
|-------|------|------------------|
| `nacin` | string | Payment method name |
| `SUMA` | double | Total amount for that method |

#### DnevniSUB.xml / dtNacini (Payment Method Totals)

| Field | Type | Max Length | Business Meaning |
|-------|------|-----------|------------------|
| `ID` | decimal | — | Payment method ID |
| `nacin` | string | 10 | Payment method name |
| `Ukupno` | decimal | — | Total amount for this method |

#### trenutniDnevni.xml (Daily P&L)

| Field | Type | Business Meaning |
|-------|------|------------------|
| `ExtraStari` | decimal | Extra charges (previous day) |
| `ExtraDanasnji` | decimal | Extra charges (today) |
| `AvansJucer` | decimal | Advance payments (yesterday) |
| `AvansDanasnji` | decimal | Advance payments (today) |
| `AvansUkupno` | decimal | Total advance payments |
| `Staranocenja` | decimal | Old/previous accommodation revenue |
| `Novanocenja` | decimal | New/today accommodation revenue |
| `NaplacenoDanas` | decimal | Amount collected today |
| `NaplacenoUkupno` | decimal | Total collected all time |
| `Datum` | dateTime | Report date |

---

## 2. P1 Reports

### 2.1 Daily Payment Summary (IzvjestajNaplata)
Already documented above.

### 2.2 Daily P&L (trenutniDnevni)
Already documented above. Uses `vratinocenjaPrije`, `vratiTroskoveSve`, `vratinocenjaDanas`, `vratiUplaceneDanas`, `vratiNaplaceno` stored procedures with time-of-day logic (before/after noon) as documented in `21_REPORTS.md:176-183`.

### 2.3 Tourism/Overnight Statistics
Uses `getGostiTurizam1` SP with `GostiLista.xml`-style data. Date overlap calculation documented in `21_REPORTS.md:226-235`.

### 2.4 Hotel Statistics
Uses `getStatistika` SP (no schema XML found in bin/).

### 2.5 Restaurant Breakfast List
Uses `getBrojGostiju` SP (no schema XML found in bin/).

### 2.6 Reservation Header Detail
From `bin/IzvjestajRezervacijaHeader.xml` — not analyzed in detail here but indexed in the bin/ directory.

### 2.7 Other bin/ XML Schemas Found (not in original task list)

| File | Table Name | Likely Purpose |
|------|-----------|----------------|
| `restoranDorucak.xml` | (not read) | Restaurant breakfast guest count |
| `sobaricaShema1.xml` | (not read) | Housekeeping report variant 2 |
| `izvjestajStatistika.xml` | (not read) | Hotel statistics dashboard |
| `NocenjeSum.xml` | (not read; referenced in `21_REPORTS.md`) | Tourism night statistics |
| `dettrosk.xml` | (commented out per `21_REPORTS.md`) | Expense detail |
| `GostiListaTurist.xml` | (not read) | Tourist registration variant |
| `Dnevni.xml` | (not read) | Daily report |
| `Repperiod.xml` | (not read) | Period report |
| `presjek.xml` | (not read) | Cross-section report |
| `estrane.xml` | (not read) | Foreign guests |
| `dtgosti.xml`, `dtgostis.xml`, `dtgostd.xml` | (not read) | Guest data tables |
| `det.xml` | (not read) | Detail data |
| `te.xml` | (not read) | Unknown |
| `new file path.xml` | (not read) | Unknown |

---

## 3. RDLC Field Mapping

### 3.1 Complete Field Mapping — `izvjestaj30` (Invoice Main Report)

**From `rptRacunFrm.vb:118-181` (print1 method) and `rptRacunFrm.vb:213-260` (printpredr method):**

| RDLC Field | printSredina Source Column | printpredr Source Column | Business Meaning in Print Layout |
|------------|---------------------------|-------------------------|----------------------------------|
| `d0` | `curr` (currency symbol from culture) | `curr` | Currency symbol displayed after totals |
| `d1` | Column 1 (Trosak name) | `Trosak` | Line item description |
| `d2` | Column 2 (Kol) | `Kol` | Quantity |
| `d3` | Column 3 (CijBezPdv) | `CijBezPdv` | Unit price without VAT |
| `d4` | Column 4 (UkupnoBezPdv) | `UkupnoBezPdv` | Total without VAT |
| `d5` | Column 5 (Pdv rate) | `Pdv` | VAT rate percentage string |
| `d6` | Column 6 (IznosPdv) | `IznosPdv` | VAT amount |
| `d7` | Column 7 (Ukupno) | `Ukupno` | Total with VAT |
| `d8` | Column 8 (payment method) | `vrplac` from header | Payment method name |
| `d9` | Column 9 (exchange denominator) | `Popust` / euro value | Exchange rate denominator or discount |
| `d10` | Column 10 (exchange currency) | `-` default or `ozn` | Currency code for conversion or "-" |
| `d11` | Column 11 (discount %) | `Popust` | Discount percentage |
| `d12` | Column 12 (discount amount) | `popust1` (Popust1) | Discount amount in currency |
| `d13` | Column 13 | `razlogp` | Additional text |
| `d14` | Column 14 | 0 (set in code) | Used in "ZA PLATITI" net calculation |
| `d15` | `rad` from row 0 | `""` (empty) | Worker/employee initials |
| `d16` | Computed: 0 default, 1 if "Boravi", 2 if "Osiguranje" | Computed same way | Line type classification flag |

| RDLC Field | printGore Source Column | printpredr Source | Business Meaning in Print Layout |
|------------|-------------------------|------------------|----------------------------------|
| `f0` | Column 0 | `broj` | Receipt/invoice number |
| `f1` | Column 1 | `ime` | Guest name (may be merged from `det` table) |
| `f2` | Column 2 | `""` | (Additional info) |
| `f3` | Column 3 | `""` | (Additional info) |
| `f4` | Column 4 | `""` | (Additional info) |
| `f5` | Column 5 | `frima` | Company/org name |
| `f6` | Column 6 | `frima` (second name) | Secondary name/company (with border box visible when non-empty) |
| `f7` | Column 7 | `brojpred` | Pre-invoice number |
| `f8` | Column 8 (date) | `dadtum` | Invoice date (formatted as dd.MM.yyyy) |
| `f9` | Column 9 | `veza` | Connection/reference |
| `f10` | Column 10 | `nazivp` | Payment method label |
| `f11` | Column 11 | "Valuta:" + `datumval` | Due date |
| `f12` | Column 12 | `napomnena` | Notes/memo |
| `f13` | Column 13 | `""` | (Additional info line) |
| `f14` | Column 14 | `""` | (Additional info line) |
| `f15` | Computed: 0=storno, 1=normal | 1 (always) | Storno watermark control |
| `lokac` | `settings.lokac` | `settings.lokac` | Municipality/location for invoice |

### 3.2 RDLC Expression Summary (Computed Fields)

**rptRacun.rdlc** computed expressions:

| Expression | Location | Purpose |
|------------|----------|---------|
| `=iif(First(f15)=0, "Storno", "")` | Line 1346 | Storno watermark |
| `=FormatNumber(sum(cdbl(d7)), 2) & " " & First(d0)` | Line 638 | Total with VAT + currency |
| `=FormatNumber(sum(cdbl(d7))-cdbl(d14), 2) & " " & First(d0)` | Line 619 | Net to pay + currency |
| `=IIf(d10<>"-", formatnumber(sum(cdbl(d7))/CDbl(d9),2) & " " & d10 & " /", "")` | Line 437 | Currency conversion (total in foreign currency) |
| `=IIf(d10<>"-", formatnumber((sum(cdbl(d7))-cdbl(d14))/CDbl(d9),2) & " " & d10 & " /", "")` | Line 459 | Currency conversion (net in foreign currency) |
| `=FormatNumber((d7+d12)/d2, 2)` | Line 870 | Unit price after discount |
| `=CInt(Replace(d5,"%","")) & "%"` | Line 959 | Clean VAT percentage display |
| `=d11 & "%"` | Line 892 | Discount percentage display |
| `=First(f10) & First(f0) & First(f7)` | Line 736 | Combined invoice title |
| `="Datum fakture: " & DateAdd("d",0,First(f8)).ToString("dd.MM.yyyy")` | Line 208 | Invoice date formatted |
| `="Mjesto izdavanja: " & First(lokac)` | Line 225 | Location of issuance |

**rptRacun1.rdlc** computed expressions (simplified):

| Expression | Location | Purpose |
|------------|----------|---------|
| `=Sum(cdec(d4.tostring))` | Line 624 | Sum w/o VAT |
| `=sum(d6)` | Line 641 | Sum VAT |
| `=Sum(cdbl(d12))` | Line 658 | Sum discount |
| `=FormatNumber(sum(cdbl(d7))-cdbl(d14), 2) & " " & First(d0)` | Line 607 | Net to pay + currency |
| `=d11 & " %"` | Line 956 | Discount percentage |

---

## 4. Print/Fiscal Integration

### 4.1 Print Flow (rptRacunFrm.vb)

The invoice form `rptRacunFrm` uses Microsoft Reporting Services (`Microsoft.Reporting.WinForms`) for rendering, NOT Crystal Reports. The flow:

1. **`rptRacunFrm_Load`** (`rptRacunFrm.vb:19-29`): If `r=0`, calls `print1()`, then refreshes ReportViewer

2. **`print1()`** (`rptRacunFrm.vb:67-201`):
   - Calls `Init()` to set locale/currency
   - Copies data from `ds.Tables("printSredina")` → `iMediaIzvjestaj.izvjestaj30` (line items: d0-d16)
   - Copies data from `ds.Tables("printGore")` → `iMediaIzvjestaj.izvjestaj30` (header: f0-f14, lokac)
   - Sets storno flag `f15`
   - Data is now bound to the RDLC via typed dataset `iMediaIzvjestaj`

3. **`printpredr()`** (`rptRacunFrm.vb:203-269`): Variant for pre-invoices (predračuni)
   - Sets `r=1` (prevents auto-print)
   - Loads directly from MySQL tables `predracuni` and `predracunidet`
   - Maps pre-invoice fields to same `izvjestaj30` structure

4. **Direct Print** (`rptRacunFrm.vb:291-380`):
   - `Run()` calls `Export()` then `Print()`
   - `Export()` renders RDLC to EMF images via `LocalReport.Render("Image", ...)`
   - `Print()` sends to default printer with copies from `akcij` variable
   - `CreateStream()` creates files in `..\..\` directory

### 4.2 Variables Controlling Print Behavior

| Variable | Type | Source | Purpose | Evidence |
|----------|------|--------|---------|----------|
| `r` | Byte | Set before form show | 0=auto-print on load, 1=preview only | `rptRacunFrm.vb:17,20-22` |
| `rimep` | Byte | Set before form show | 0=use printGore name, 1=merge all guest names | `rptRacunFrm.vb:18,152-161` |
| `akcij` | Integer | Set by caller | Number of print copies | `rptRacunFrm.vb:315` |
| `akcij1` | String | Set by caller | "p" = print mode (dispatches to `Run()`) | `rptRacunFrm.vb:280-286` |
| `akcij2` | String | Set by caller | "storno" = storno mode (sets f15=0) | `rptRacunFrm.vb:176-181` |

### 4.3 Fiscal Integration

The legacy application does NOT have explicit fiscal device integration in `rptRacunFrm.vb`. The fiscal integration appears to be:

1. **Printer-only**: The form prints to the default Windows printer via EMF rendering
2. **No fiscal device protocol**: No serial port communication, no fiscal command sequences
3. **Fiscal compliance**: Achieved through invoice format compliance (invoice number, VAT breakdown, company info) rather than hardware fiscalization
4. **The `akcij` copies variable** likely corresponds to legal requirements for duplicate/triplicate invoices

### 4.4 External Resources Referenced in RDLC

| Resource | Path | Used In |
|----------|------|---------|
| Logo image | `file:///C:\Program Files\IMEDIA\HotelPro\logo.jpg` | rptRacun page header background, rptRacun1 page header |
| Signature image | `file:///C:\Program Files\IMEDIA\HotelPro\potpis.jpg` | rptRacun rectangle background, rptRacun1 rectangle background |
| Footer image | `file:///C:\Program Files\IMEDIA\HotelPro\footer.jpg` | rptRacun page footer background, rptRacun1 page footer |

### 4.5 Page Dimensions

| Report | Width | Height | Top Margin | Bottom Margin | Left Margin | Right Margin |
|--------|-------|--------|------------|---------------|-------------|--------------|
| rptRacun.rdlc | 21cm | 29cm | 0.5cm | 0.5cm | (default) | (default) |
| rptRacun1.rdlc | 20.5cm | 29cm | (default) | 0.2cm | 0.2cm | 0.2cm |
| rptRacunDet.rdlc | 21cm | 29.7cm | 0.2cm | 0.2cm | 0.2cm | 0.2cm |

All use A4/portrait format. rptRacunDet uses slightly taller pages (29.7cm = DIN A4 height).

---

## 5. Key Cross-References

| Modern Report | Legacy Source | Key Data Tables | Priority |
|--------------|---------------|----------------|----------|
| Invoice/Receipt | rptRacun.rdlc + rptRacun1.rdlc | printGore, printSredina, setings → izvjestaj30 | P0 |
| Invoice Detail | rptRacunDet.rdlc | izvjestaj15 | P0 |
| Pre-Invoice | rptRacunFrm.printpredr | predracuni, predracunidet (MySQL) | P1 |
| Daily Payment List | DnevniPlacanje.xml schema | getIzvjestajDnevniPlacanje(N), printracuni, printracuniavans | P0 |
| Payment Summary | IzvjestajNaplata.xml / DnevniSUB.xml | getIzvjestajDnevni, placanje1-3 | P0 |
| Guest List (Registration) | GostiLista.xml | Guest stay data with ID/docs | P1 |
| Housekeeping Status | sobaricaShema.xml | getSobeShema SP | P0 |
| Reservation Report | IzvjestajRezervacijaPoj.xml | Reservation data | P1 |
| Phone Bill | TelefonskiRacun.xml | Phone call records | P1 |
| Expense Types | troskoviVrste.xml | Reference data (26 types) | P0 |
| Daily P&L | trenutniDnevni.xml | 5 SP calls | P1 |