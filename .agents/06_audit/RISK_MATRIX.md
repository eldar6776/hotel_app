# RISK MATRIX — Legacy Hotel Application

**Datum:** 2026-05-17 (rekreirano)
**Scope:** Kompletna legacy analiza + OWASP Top 10 mapiranje
**Agent:** GitHub Copilot (Claude Sonnet 4.6)

---

## METODOLOGIJA

- **Likelihood**: 1 (Low) → 5 (Critical)
- **Impact**: 1 (Minimal) → 5 (Catastrophic)
- **Risk Score = Likelihood × Impact**
- **CVSS v3 Score**: procjena prema NIST

---

## RISK MATRIX TABELA

| ID | Risk | Lokacija | Likelihood | Impact | Score | OWASP | CWE | CVSS |
|---|---|---|---|---|---|---|---|---|
| R-01 | SQL Injection — Login | frmKorisnik | 5 | 5 | **25** | A3 | CWE-89 | 9.8 CRITICAL |
| R-02 | Hardcoded Tourism API Credentials | clasTZ.vb | 4 | 4 | **16** | A2 | CWE-798 | 7.5 HIGH |
| R-03 | SQL Injection — Charges | frmTroskovi | 4 | 4 | **16** | A3 | CWE-89 | 8.8 HIGH |
| R-04 | Plaintext Employee Passwords | frmRadnik | 4 | 4 | **16** | A2 | CWE-256 | 7.5 HIGH |
| R-05 | HTTP C&C Communication | ftpUploa | 3 | 5 | **15** | A8 | CWE-494 | 9.0 CRITICAL |
| R-06 | EOL Winsock COM (CVE-prone) | classKard | 3 | 5 | **15** | A6 | CWE-1104 | 8.1 HIGH |
| R-07 | SQL Injection — Reporting | rptRacunFrm | 4 | 4 | **16** | A3 | CWE-89 | 8.8 HIGH |
| R-08 | Path Traversal — Report Stream | rptRacunFrm | 3 | 4 | **12** | A1 | CWE-22 | 7.5 HIGH |
| R-09 | XXE in TZ XML Parser | clasTZ | 3 | 4 | **12** | A5 | CWE-611 | 7.5 HIGH |
| R-10 | Plaintext SMTP Credentials | frmMail | 4 | 3 | **12** | A2 | CWE-312 | 6.5 MEDIUM |
| R-11 | Unencrypted Card Commands (TCP) | classKard | 3 | 5 | **15** | A2 | CWE-319 | 8.8 HIGH |
| R-12 | SQL Injection — Mail Config | frmMailKonfig | 3 | 4 | **12** | A3 | CWE-89 | 8.8 HIGH |
| R-13 | Plaintext DB Password in XML | Data.vb | 4 | 5 | **20** | A2 | CWE-312 | 8.6 HIGH |
| R-14 | Check-in Non-Atomic Transaction | frmPrijava1 | 4 | 4 | **16** | — | CWE-398 | — |
| R-15 | MAX(ID)+1 Race Condition | frmPrijava1, frmGrupe | 3 | 4 | **12** | — | CWE-362 | — |
| R-16 | GDPR — PII Unencrypted at Rest | frmGosti | 4 | 4 | **16** | A1 | CWE-312 | 7.5 HIGH |
| R-17 | SQL Injection — Login Form | frmKorisnik | 5 | 5 | **25** | A3 | CWE-89 | 9.8 CRITICAL |
| R-18 | No Authorization on DB Config | frmBaza | 3 | 5 | **15** | A5 | CWE-284 | 7.2 HIGH |
| R-19 | Trivial Password Obfuscation | ftpUploa, Data.vb | 4 | 4 | **16** | A2 | CWE-261 | 7.5 HIGH |
| R-20 | Log File Contains PII/SQL | funkcije.vb | 3 | 3 | **9** | A9 | CWE-532 | 5.5 MEDIUM |

---

## PRIORITY 0 — IMMEDIATELY CRITICAL

### R-01 / R-17: SQL Injection na Login
- **Lokacija**: `frmKorisnik.vb`
- **Payload**: `' OR '1'='1` → bypass svakog logina
- **Impact**: Kompletan bypass autentikacije
- **CVSS**: 9.8 CRITICAL
- **Fix**: Parameterizovani query + bcrypt verification

### R-13: Plaintext DB Password u XML
- **Lokacija**: `C:\Program Files\IMEDIA\HotelPro\set.xml`
- **Impact**: Direktan pristup cijeloj bazi; sve korisničke šifre, svi gosti, svi računi
- **CVSS**: 8.6 HIGH
- **Fix**: Windows DPAPI encrypted config, ne XML plaintext

### R-05: HTTP C&C u ftpUploa.vb
- **Lokacija**: `http://i-web.info/fdms1/snimiloc_h.php`
- **Impact**: Remote code execution ako je server kompromitovan; MITM može pushati malicious update
- **CVSS**: 9.0 CRITICAL
- **Fix**: HTTPS + package signature verification + remove auto-update

### R-11: Unencrypted Card Access Control
- **Lokacija**: `classKard.vb`, `kard_imedia.vb`
- **Impact**: Svako na LAN-u može otvoriti hotelsku sobu
- **CVSS**: 8.8 HIGH
- **Fix**: TLS + vendor SDK + autentikacija

### R-06: EOL Winsock COM
- **Lokacija**: `classKard.vb`
- **Impact**: Poznate CVEs, nema security patch-eva od 2016
- **CVSS**: 8.1 HIGH
- **Fix**: Zamijeniti sa modernim .NET Socket ili vendor SDK

---

## PRIORITY 1 — HIGH SEVERITY

### R-03, R-07, R-12: SQL Injection u 5+ lokacija
| Lokacija | Query type | Injected field |
|---|---|---|
| frmTroskovi | INSERT | naziv usluge |
| rptRacunFrm | SELECT | report parameter |
| frmMailKonfig | UPDATE | email config field |
| frmAlarm | SELECT | datum |
| frmRezervacije | SELECT | datumi |

**Remediation pattern:**
```csharp
// NE:
$"SELECT * FROM table WHERE id='{userInput}'"

// DA:
"SELECT * FROM table WHERE id=@id"
command.Parameters.AddWithValue("@id", userInput);
```

### R-02: Hardcoded Tourism API Credentials
- Username `nerminc` / Password `nermin1234` u HTTP URL-u
- Eksponiran u git historiji, network logovima, proxy logovima
- **Fix**: Credentials u `IConfiguration` + HTTPS

### R-16: GDPR — PII bez enkripcije
- Ime, prezime, pasoš broj, JMBG, adresa gostiju u MySQL bez enkripcije
- GDPR Article 32 zahtijeva "appropriate technical measures"
- **Fix**: Column-level encryption za posebnu kategoriju podataka

### R-04: Plaintext Employee Passwords
- Zaposlenici sistema vidljivi u bazi u plaintext
- Admin ima pristup svim lozinkama
- **Fix**: bcrypt/Argon2 sa per-user salt

---

## PRIORITY 2 — MEDIUM SEVERITY

### R-08: Path Traversal u Reporting
- `"..\..\" + name + "." + ext` u Crystal Reports stream
- Potencijalni pristup sistemskim fajlovima
- **Fix**: Canonicalize putanje, whitelist ekstenzija

### R-09: XXE u TZ XML parseru
- XML bez DTD restrikcija
- Malicious server može eksploatovati XXE
- **Fix**: `XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit }`

### R-10: Plaintext SMTP Credentials
- Email lozinka u DataSet memoriji
- **Fix**: `IConfiguration` + encrypted secrets

### R-20: PII u Log fajlovima
- SQL queries sa imenima gostiju u `log.txt`
- **Fix**: Serilog sa PII scrubbing middleware

---

## PRIORITY 3 — OPERATIONAL RISKS

| R-ID | Problem | Lokacija | Type |
|---|---|---|---|
| R-14 | Non-atomic check-in | frmPrijava1 | Data integrity |
| R-15 | MAX(ID)+1 race condition | više formi | Concurrency |
| — | Memory leak (Bitmap) | frmSobe | Resource management |
| — | Division by zero | frmPlacanjePo | Reliability |
| — | 50% dead code | frmPlacanjeSlozeno | Maintenance |
| — | No session token | frmKorisnik | Authentication |

---

## OWASP TOP 10 COVERAGE

| OWASP | Naziv | Prisutan | Lokacije |
|---|---|---|---|
| A1 | Injection | ✅ DA | frmKorisnik, frmTroskovi, rptRacunFrm, frmAlarm, frmMailKonfig |
| A2 | Broken Authentication | ✅ DA | frmKorisnik (no hash, no session), frmRadnik |
| A3 | Sensitive Data Exposure | ✅ DA | set.xml, mailkonfig, clasTZ URL |
| A4 | XML External Entities | ✅ DA | clasTZ XML parser |
| A5 | Broken Access Control | ✅ DA | frmBaza (no authz), popusti bez role check |
| A6 | Security Misconfiguration | ✅ DA | EOL Winsock COM, HTTP C&C |
| A7 | XSS | ⚠️ N/A | WinForms app — ne primjenjuje se |
| A8 | Insecure Deserialization | ✅ DA | ftpUploa HTTP auto-update |
| A9 | Using Known Vulnerable Components | ✅ DA | Winsock OCX (EOL 2016) |
| A10 | Insufficient Logging | ✅ DA | log.txt sa PII, bez structured logging |

**ZAKLJUČAK: 9 od 10 OWASP kategorija potvrđeno prisutno.**

---

## UKUPNA PROCJENA

| Metrika | Vrijednost |
|---|---|
| Ukupan broj rizika | 20 |
| P0 (Kritično) | 5 |
| P1 (Visoko) | 9 |
| P2 (Srednje) | 4 |
| P3 (Nisko) | 5 |
| Potvrđenih SQL Injection lokacija | 7+ |
| Potvrđenih hardcoded credentials | 4 |
| OWASP kategorija prisutnih | 9/10 |
| GDPR compliance | ❌ 0% |
| Autentikacija quality | ❌ Kritično loša |
| Enkripcija at-rest | ❌ Nema |
| Enkripcija in-transit | ❌ HTTP na svim integr. |
