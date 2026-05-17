# ANALYSIS 03 — SUPPORTING FORMS

**Datum:** 2026-05-17 (rekreirano)
**Scope:** FAZA 3 — 21 supporting/administrative forme
**Agent:** GitHub Copilot (Claude Sonnet 4.6)

---

## PREGLED

| Fajl | Veličina | Risk | Ključni nalaz |
|---|---|---|---|
| frmRezervacije.vb | ~150KB | 🟠 P1 | SQL injection, god class |
| frmSobe.vb | ~80KB | 🟠 P1 | Memory leak (Image), inconsistent state |
| frmTroskovi.vb | ~120KB | 🔴 P0 | God class, SQL injection, race condition |
| frmRadnik.vb | ~50KB | 🔴 P0 | Plaintext password, JMBG misuse |
| frmAlarm.vb | ~20KB | 🟠 P1 | SQL injection, WMI |
| frmGrupe.vb | ~40KB | 🟠 P1 | MAX(ID)+1 race condition |
| frmPredracun.vb | ~90KB | 🟠 P1 | God class, no validation |
| frmBaza.vb | ~15KB | 🔴 P0 | Dual DB connection, no auth |
| frmPartneri.vb | ~60KB | 🟠 P1 | 22-param procedure, no transaction |
| frmTarife.vb | ~45KB | 🟠 P1 | No transaction na multi-step update |
| frmPlacanjeSlozeno.vb | ~200KB | 🟠 P1 | 50% dead code |
| frmPlacanjePo.vb | ~80KB | 🟡 P2 | Division by zero risk |
| frmKorisnik.vb | ~25KB | 🔴 P0 | Plaintext password check |
| frmDodatno.vb | ~30KB | 🟡 P2 | UI-only, low risk |
| frmDodaj.vb | ~40KB | 🟡 P2 | Standard CRUD |
| frmBazaPas.vb | ~5KB | 🔴 P0 | Plaintext DB password change |
| frmDodajDrzave.vb | ~15KB | 🟡 P2 | Lookup management |
| frmIzvjestaji.vb | ~70KB | 🟠 P1 | Date SQL injection |
| frmbanke.vb | ~20KB | 🟡 P2 | Bank master data |
| Explorer1.vb | ~10KB | 🟡 P2 | File browser, path validation needed |
| frmDnevniIzvjestaj.vb | ~30KB | 🟠 P1 | Night audit trigger |

---

## DETALJNA ANALIZA

### 1. frmTroskovi.vb — Charges / Extras Management
**Risk: 🔴 P0**

**a) God class problema:**
- Sadrži hotelske troškove, usluge, F&B, mini-bar, parking
- UI + business logic + SQL sve u jednoj klasi (~120KB)

**b) SQL Injection u troškovima:**
```vb
"INSERT INTO troskovi (rid, naziv, kolicina, cijena) VALUES (" & rid & ", '" & naziv & "', " & kolicina & ", " & cijena & ")"
```
- `naziv` (naziv usluge) direktno konkateniran
- Recepcioner može unijeti malicious SQL kroz naziv usluge
- **OWASP A3 — CWE-89**

**c) Race condition na RID:**
```vb
rid = CInt(mysqlScalar("SELECT MAX(rid) FROM relgostsoba WHERE soba=" & sid))
' ... 
mysqlExScalar("INSERT INTO troskovi ... rid=" & rid)
```
- Između SELECT i INSERT može drugi check-in promijeniti MAX(rid)

---

### 2. frmRadnik.vb — Employee Management
**Risk: 🔴 P0**

**a) Plaintext password storage:**
```vb
mysqlExScalar("UPDATE radnici SET pass='" & txtPass.Text & "' WHERE id=" & radnikID)
```
- Lozinka zaposlenika upisana direktno u bazu bez hashiranja
- **CWE-256 Plaintext Storage of Password**

**b) JMBG misuse:**
```vb
' JMBG se koristi i kao unique key u nekim operacijama
WHERE jmbg='" & jmbgInput & "'"
```
- JMBG (biometrički ID) korišten kao DB ključ
- GDPR kršenje: JMBG je posebna kategorija osobnih podataka u BiH/regiji

**c) No password policy:**
- Nema provjere jačine lozinke
- Nema expiry
- Nema account lockout

---

### 3. frmKorisnik.vb — User Management / Login
**Risk: 🔴 P0**

**a) Plaintext password provjera:**
```vb
WHERE korisnik='" & txtUser.Text & "' AND pass='" & txtPass.Text & "'"
```
- SQL Injection u login formi
- Plaintext password matching u bazi
- **OWASP A2, A3 — CWE-89, CWE-256**

**b) Nema session token:**
- Nakon uspješnog logina, identitet se čuva u global var `korisnikID`
- Nema JWT, nema session hash, nema expiry

---

### 4. frmBaza.vb — Database Configuration
**Risk: 🔴 P0**

```vb
' Forma za konekciju na drugu bazu (archivska)
Dim conn2 As New MySqlConnection("server=" & srv2 & ";user=" & usr2 & ";pass=" & pwd2 & "...")
```
- Aplikacija može da se konektuje na drugu/arhivsku bazu
- Nema autorizacijske provjere — svaki korisnik može promijeniti DB konekciju
- **CWE-284 Improper Access Control**

---

### 5. frmBazaPas.vb — Database Password Change
**Risk: 🔴 P0**

```vb
txtNewPass.Text  ' Plaintext u UI
' Mijenja set.xml direktno
```
- Mijenja DB lozinku u `set.xml` fajlu
- XML fajl nije enkriptovan
- Nema autorizacijske provjere ko smije mijenjati DB lozinku

---

### 6. frmAlarm.vb — Alarm / Notification System
**Risk: 🟠 P1**

**SQL Injection:**
```vb
"SELECT * FROM alarmi WHERE datum='" & dtpDatum.Value.ToString("yyyy-MM-dd") & "'"
```
- DateTimePicker vrijednost konkatenirana direktno
- U praksi kontrolisana, ali pattern je pogrešan
- **CWE-89 (niži rizik zbog datepicker kontrole)**

---

### 7. frmGrupe.vb — Group Booking Management
**Risk: 🟠 P1**

**MAX(ID)+1 race condition:**
```vb
Dim gid As Integer = CInt(mysqlScalar("SELECT MAX(groupid) FROM rezervacijeGrupe")) + 1
' Između SELECT i INSERT, drugi agent može uzeti isti ID
```
- Grupna rezervacija može imati duplicirani groupid
- **CWE-362**

---

### 8. frmPartneri.vb — Partners / B2B Management
**Risk: 🟠 P1**

**22-parametarni procedure poziv bez transakcije:**
```vb
mysqlExScalar("INSERT INTO partneri (naziv, adresa, pib, pdv, racun, ... [22 polja]) VALUES (...)")
```
- Ako koji od 22 polja failuje validaciju → djelimičan insert
- Nema transakcije koja bi rollback-ovala

---

### 9. frmPlacanjeSlozeno.vb — Complex Payment
**Risk: 🟠 P1**

**Dead code > 50%:**
- Forma je refaktorisana u frmPlacanje.vb
- Stara implementacija ostala komentovana/nikad pozivana
- **Maintenance burden, confusion rizik**

---

### 10. frmPlacanjePo.vb — Payment Per Night
**Risk: 🟡 P2**

**Division by zero:**
```vb
cijenaPoNocenju = ukupno / brojNocenja
' Ako rezervacija ima 0 noćenja (data entry greška) → DivideByZeroException
```
- Nema provjere `brojNocenja > 0` prije dijeljenja
- **CWE-369 Division by Zero**

---

### 11. frmDnevniIzvjestaj.vb — Night Audit / Daily Report
**Risk: 🟠 P1**

- Pokreće "night audit" proces: zaključivanje dana, transfer stanja
- Proces nije transakcijski zaštićen u cijelosti
- Ako se reporting job prekine na pola → nekompletni financial period

---

### 12. Explorer1.vb — File Browser
**Risk: 🟡 P2**

- Otvara file sistem za dodavanje attachmenta
- Nema provjere tipa fajla (može se priložiti .exe)
- **CWE-434 Unrestricted Upload**

---

### 13. frmPredracun.vb — Pro-forma Invoice
**Risk: 🟠 P1**

- Generiše predračun (pro-forma) koji nije fiskalan
- Može biti konvertovan u pravi račun
- Konverzija nema atomsku transakciju

---

### 14. frmSobe.vb — Room Management
**Risk: 🟠 P1**

**Memory leak — Image objekti:**
```vb
Dim img As New Bitmap("C:\...\sobe\" & sobaID & ".jpg")
PictureBox1.Image = img
' img.Dispose() nikad nije pozvan
```
- Bitmap objekti se ne dispose-uju
- Dugotrajna sesija → growing memory footprint
- **CWE-401 Memory Leak**

**Inconsistentno stanje:**
- Promjena statusa sobe iz ove forme može biti override-ovana check-in procesom
- Nema locking mechanizma

---

### 15. frmTarife.vb — Rate/Tariff Management
**Risk: 🟠 P1**

- Multi-step tarifa update bez transakcije
- Ako se cijena promijeni a PDV stopa failuje → nekonzistentna tarifa
- **CWE-398 — Finansijski integritet**

---

### 16-21. Ostale forme
| Forma | Rizik | Napomena |
|---|---|---|
| frmDodatno.vb | 🟡 P2 | UI-only, dodatni sadržaj |
| frmDodaj.vb | 🟡 P2 | Standard CRUD forma |
| frmDodajDrzave.vb | 🟡 P2 | Lookup management |
| frmIzvjestaji.vb | 🟠 P1 | Date parametri u SQL queryjima |
| frmbanke.vb | 🟡 P2 | Bank master data |
| frmRezervacije.vb | 🟠 P1 | SQL injection u datumima, god class |

---

## SAŽETAK NALAZA

| Prioritet | Forma | Problem | OWASP | CWE |
|---|---|---|---|---|
| 🔴 P0 | frmRadnik | Plaintext password storage | A2 | CWE-256 |
| 🔴 P0 | frmKorisnik | SQL Injection u login | A3 | CWE-89 |
| 🔴 P0 | frmKorisnik | Plaintext password matching | A2 | CWE-256 |
| 🔴 P0 | frmTroskovi | SQL Injection (naziv usluge) | A3 | CWE-89 |
| 🔴 P0 | frmBaza | Nema autorizacije za DB switch | A5 | CWE-284 |
| 🟠 P1 | frmAlarm | SQL Injection (datum) | A3 | CWE-89 |
| 🟠 P1 | frmGrupe | MAX(ID)+1 race condition | — | CWE-362 |
| 🟠 P1 | frmSobe | Memory leak (Bitmap) | — | CWE-401 |
| 🟡 P2 | frmPlacanjePo | Division by zero | — | CWE-369 |
| 🟡 P2 | Explorer1 | Unrestricted file attachment | A1 | CWE-434 |

## MIGRACIONI ZAHTJEVI

1. Svi korisnički passwordi → bcrypt/Argon2 hashing
2. Login → JWT sa expiry, account lockout po N neuspjeha
3. Sve SQL operacije → parameterizovane
4. MAX(ID)+1 → PostgreSQL SEQUENCE ili UUID svuda
5. Night audit → transakcijsko zaključivanje perioda (atomic ledger close)
6. GDPR: JMBG ne smije biti DB ključ, mora biti encrypted
7. File attachmenti → whitelist ekstenzija + virus scan
