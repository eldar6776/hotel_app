# ANALYSIS 02 — CRITICAL FORMS

**Datum:** 2026-05-17 (rekreirano)
**Scope:** FAZA 2 — kritične forme: Check-in, Check-out, Payment, Invoice, Booking, Guests
**Agent:** GitHub Copilot (Claude Sonnet 4.6)

---

## PREGLED

| Fajl | Veličina | Risk | Ključni nalaz |
|---|---|---|---|
| frmPrijava1.vb | ~60KB | 🔴 P0 | Check-in workflow, no atomic transaction |
| frmOdjava1.vb | ~44KB | 🔴 P0 | Check-out workflow, partial rollback |
| frmPlacanje.vb | ~318KB | 🔴 P0 | Najveći fajl, god class, mixed business logic |
| frmRacuni.vb | ~262KB | 🔴 P0 | Invoice cancel/reversal, fiscal interaction |
| frmRezervacije_unos.vb | ~66KB | 🟠 P1 | Booking entry, SQL injection risk |
| frmGosti.vb | ~269KB | 🟠 P1 | Guest management, GDPR osjetljivi podaci |

---

## DETALJNA ANALIZA

### 1. frmPrijava1.vb — Check-in Form
**Risk: 🔴 P0**

**Poslovni flow:**
1. Odabir sobe i gosta
2. Unos check-in / check-out datuma i tarife
3. Poziv `addRelGostSoba` → insert u `relgostsoba`
4. Poziv `Unesinocenja` → insert/update `nocenja` tabela
5. Otvaranje folija (`posvetaFolio` / `PID`)
6. Opciono: grupna prijava

**Kritični problemi:**

**a) Nema atomske transakcije kroz čitav flow:**
```vb
Call addRelGostSoba(...)    ' Step 1: insert relgostsoba
Call Unesinocenja(...)      ' Step 2: insert nocenja
' Step 3: update folio
' Ako Step 3 failuje, Steps 1 i 2 nisu rollback-ovani
```
- Djelimičan check-in ostavlja "zombie" zapise u bazi
- **CWE-398 Poor Code Quality — Finansijski integritet**

**b) MAX(ID)+1 pattern za PID (folio ID):**
```vb
PID = CInt(mysqlScalar("SELECT MAX(pid) FROM posjetafolio")) + 1
INSERT INTO posjetafolio (pid, ...) VALUES (' & PID & ', ...)
```
- Race condition: dva simultana check-in-a dobivaju isti PID
- **CWE-362 Race Condition / Finansijski integritet**

**c) Check-in bez provjere dostupnosti sobe:**
- Forma ne provjerava je li soba već zauzeta u odabranom periodu (u nekim kod-putanjama)
- Moguće double booking

**Migracioni zahtjev:**
```
CheckInService.CheckInAsync() mora biti:
1. Atomska transakcija (svi koraci ili ništa)
2. Optimistic locking na sobi (ETag / RowVersion)
3. Availability check u istoj transakciji (SELECT FOR UPDATE ekvivalent)
4. Sequence generisan na DB nivou (SEQUENCE objekt u PostgreSQL)
```

---

### 2. frmOdjava1.vb — Check-out Form
**Risk: 🔴 P0**

**Poslovni flow checkout-a:**
1. `OdjavaSobe()` iz Data.vb — zaključuje aktivna noćenja
2. `PrljavaSoba()` — postavlja `sobe.clean = 0`
3. Zaključivanje folija
4. Zaključivanje otvorenih troškova
5. Opciono: generisanje računa

**Kritični problemi:**

**a) Djelimična odjava nije jasno handlovana:**
```vb
' Ako je u sobi više gostiju i odjavljuje se samo jedan:
' updateuje se relgostsoba za tog gosta
' ali folio može ostati otvoren sa ostalim gostima
```
- Poslovni slučaj djelimičnog checkout-a nije atomski pokriven

**b) Fiskalizacija bez potvrde:**
- Račun se može generisati bez potvrde da je fiskalni printer odgovorio
- Ako printer failuje → račun evidentiran ali ne odštampan

**Housekeeping veza:**
```vb
' sobe.clean = 0 → jedini trigger za housekeeping status
UPDATE sobe SET clean=0 WHERE id=@sid
```
- Sobarica mora eksplicitno označiti sobu čistom da bi bila ponovo prodajna

---

### 3. frmPlacanje.vb — Payment Form
**Risk: 🔴 P0 (~318KB, najveći fajl u projektu)**

God class sa svim vrstama plaćanja: gotovina, kartica, ček, transfer, kombinirano.

**Kritični problemi:**

**a) Miješanje UI, business logic i SQL u jednom fajlu:**
- UI event handler direktno poziva SQL
- Nema servisnog sloja
- Nema validacije ulaznih podataka na servisnom nivou

**b) Fiskalizacija hardkodirana u UI:**
```vb
' Fiskalni printer poziv iz UI event handlera
Call fiskalPrint(iznos, pdv, tip)
```
- Ako UI thread blokira → UI zamrzne tokom fiskalizacije
- Nema retry logic za fiskalni printer

**c) Popust bez autorizacijske provjere:**
```vb
' Popust se unosi slobodno, nema provjere role korisnika
iznos = iznos * (1 - popust/100)
```
- Svaki recepcioner može dati 100% popust bez odobrenja menadžera

**Poslovne regule koje MORAJU biti u novom sistemu:**
- Tip plaćanja (gotovina/kartica/ček/transfer) mora biti evidentiran
- PDV se obračunava po kategoriji
- Fiskalizacija je obavezna za gotovinska plaćanja
- Storno plaćanja zahtijeva isti tip plaćanja (ne možeš stornirati gotovinu karticom)

---

### 4. frmRacuni.vb — Invoice Reversal / Cancellation
**Risk: 🔴 P0 (~262KB)**

**Poslovni flow:**
- Storno računa → kreiranje negativnog računa
- Korekcija → izmjena stavki
- Reprint → ponovni ispis bez storna

**Kritičan problem — storno bez transakcije:**
```vb
' Storno operacija radi u 3+ SQL koraka bez transakcije
UPDATE printracuni SET storno=1 WHERE id=@rid
INSERT INTO printracuni (storno, ...)  ' novi storno račun
UPDATE folio SET ...                    ' folio korekcija
' Ako ikoji korak failuje → nekonzistentno stanje
```
- **CWE-398 — Finansijski integritet**

**Fiskalni storno:**
- Legacy sistem čuva fiskalni broj (`fiskalBroj`) na računu
- Storno mora imati referencu na originalni fiskalni broj
- Novi sistem mora implementirati isti model

---

### 5. frmRezervacije_unos.vb — Booking Entry Form
**Risk: 🟠 P1 (~66KB)**

**SQL Injection rizici:**
```vb
"WHERE datumOD <= '" & datOD & "' AND datumDO >= '" & datDO & "'"
```
- Datumi se konkateniraju direktno
- Input iz datepicker-a — niži rizik, ali još uvijek wrong pattern

**Grupna rezervacija logika:**
- Grupna rezervacija kreira `rezervacijeGrupe` master record
- Individualne sobe vezuju se za grupu
- `MAX(ID)+1` pattern za group ID → race condition

---

### 6. frmGosti.vb — Guest Management
**Risk: 🟠 P1 (~269KB)**

**GDPR osjetljivi podaci:**
- Ime, prezime, datum rođenja, broj dokumenta (pasoš/lična karta), JMBG, državljanstvo, adresa
- Svi podaci u MySQL bez enkripcije at-rest
- Nema right-to-forget implementacije
- Nema audit loga pristupa gostima

**OCR/ručni unos dokumenata:**
- Broj dokumenta se unosi ručno
- Nema validacije formata dokumenta po državi
- JMBG se koristi i za permissioning (misuse!)

---

## BAZA PODATAKA — KLJUČNE TABELE

Iz analize svih kritičnih formi, identifikovane su ove centralne tabele:

```
relgostsoba     — veza gost-soba (boravak ili rezervacija)
posvetaFolio    — folio (naplatna jedinica)
nocenja         — materializovana noćenja (ledger)
troskovi        — otvoreni troškovi na foliju
placanje        — plaćanja
placanjeDetalji — stavke plaćanja
printRacuni     — štampani/fiskalni računi
printRacunDetalji — stavke računa
sobe            — sobe (clean flag je housekeeping trigger)
rezervacije     — rezervacije
rezervacijeGrupe — grupne rezervacije
gosti           — gosti (PII podaci)
radnici         — zaposlenici
tarife          — cjenovnici
```

## MIGRACIONI ZAHTJEVI

1. Svi kritični workflow-i (check-in, check-out, payment, invoice) moraju biti atomske transakcije
2. MAX(ID)+1 → PostgreSQL SEQUENCE ili UUID
3. Fiskalizacija u background service, ne u UI threadu
4. GDPR: enkripcija PII at-rest, right-to-forget, audit log pristupa
5. Popusti zahtijevaju role-based autorizaciju
6. Storno mora atomski kreirati negativni račun i referencirati fiskalni broj originala
