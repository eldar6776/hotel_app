# DATA FLOW DIAGRAMS — Legacy Hotel Application

**Datum:** 2026-05-17 (rekreirano)
**Scope:** Ključni tokovi podataka u legacy VB.NET aplikaciji
**Agent:** GitHub Copilot (Claude Sonnet 4.6)
**Format:** ASCII dijagrami sa sigurnosnim anotacijama

---

## DFD-01: CHECK-IN FLOW

```
RECEPCIONAR
    │
    ▼
[frmPrijava1.vb]
    │
    ├──► 1. Provjera dostupnosti sobe
    │        SQL: SELECT FROM sobe WHERE ... [⚠️ RACE COND]
    │
    ├──► 2. addRelGostSoba()
    │        INSERT INTO relgostsoba (PID=MAX+1) [🔴 RACE COND]
    │        ▼
    │       [MySQL: relgostsoba]
    │
    ├──► 3. Unesinocenja()  [za svaki datum u periodu]
    │        DELETE nocenja WHERE RID=x AND datum=d
    │        INSERT nocenja (rid, sid, pid, tarifa, ...)
    │        ▼
    │       [MySQL: nocenja]
    │
    ├──► 4. Otvaranje folija
    │        INSERT INTO posvetaFolio (pid=MAX+1) [🔴 RACE COND]
    │        ▼
    │       [MySQL: posvetaFolio]
    │
    └──► 5. UI refresh — soba označena zauzetom
             ▼
            [frmGlavni.vb] — MDI parent state update

SIGURNOSNI PROBLEMI:
  🔴 Nema atomske transakcije (koraci 2-4 mogu biti parcijalni)
  🔴 MAX(ID)+1 na relgostsoba i posvetaFolio
  ⚠️ Provjera dostupnosti nije lock
```

---

## DFD-02: CHECK-OUT FLOW

```
RECEPCIONAR
    │
    ▼
[frmOdjava1.vb]
    │
    ├──► 1. Prikaz otvorenih troškova i noćenja
    │        [MySQL: troskovi, nocenja za RID]
    │
    ├──► 2. Opciono: generisanje računa
    │        [frmRacuni.vb → printRacuni INSERT]
    │        ▼
    │       [Fiskalni printer] [⚠️ Bez retry logike]
    │
    ├──► 3. OdjavaSobe() [Data.vb]
    │        UPDATE relgostsoba SET odjava=now WHERE rid=@rid
    │        UPDATE nocenja SET status=closed WHERE rid=@rid
    │        [⚠️ Parcijalni rollback moguć]
    │
    ├──► 4. PrljavaSoba()
    │        UPDATE sobe SET clean=0 WHERE id=@sid
    │        → Trigger za housekeeping
    │
    └──► 5. Zaključivanje folija
             UPDATE posvetaFolio SET zatvoren=1
             ▼
            [MySQL: posvetaFolio, relgostsoba, nocenja]

SIGURNOSNI PROBLEMI:
  🔴 Nema atomske transakcije
  ⚠️ Fiskalni printer failure nije rollback-ovan
  ⚠️ Djelimična odjava (više gostiju u sobi) nema jasnu putanju
```

---

## DFD-03: PAYMENT FLOW

```
RECEPCIONAR
    │
    ▼
[frmPlacanje.vb — 318KB GOD CLASS]
    │
    ├──► 1. Odabir tipa plaćanja
    │        Gotovina | Kartica | Ček | Transfer | Kombinirano
    │
    ├──► 2. Unos iznosa i PDV-a
    │        [⚠️ Popust bez autorizacijske provjere]
    │
    ├──► 3. INSERT u placanje tabelu
    │        INSERT INTO placanje (rid, pid, iznos, tip, pdv)
    │        [⚠️ SQL concatenation]
    │
    ├──► 4. Fiskalizacija (za gotovinu)
    │        Call fiskalPrint(iznos, pdv, tip)
    │        [⚠️ UI thread, nema timeout/retry]
    │
    └──► 5. Stamp → printRacuni
             INSERT INTO printRacuni (...)
             ▼
            [MySQL: placanje, printRacuni, printRacunDetalji]

SIGURNOSNI PROBLEMI:
  🔴 Popust bez RBAC provjere
  🔴 Fiskalizacija u UI threadu (freeze risk)
  🔴 God class — UI + business + SQL
```

---

## DFD-04: INVOICE REVERSAL (STORNO) FLOW

```
RECEPCIONAR / MANAGER
    │
    ▼
[frmRacuni.vb — 262KB]
    │
    ├──► 1. Odabir originalnog računa
    │        SELECT FROM printRacuni WHERE id=@rid
    │
    ├──► 2. Kreiranje storno računa
    │        UPDATE printRacuni SET storno=1 WHERE id=@orig  [korak A]
    │        INSERT INTO printRacuni (negative amounts)       [korak B]
    │        UPDATE posvetaFolio SET ...                      [korak C]
    │        [🔴 Nema transakcije — A/B/C mogu biti parcijalni]
    │
    └──► 3. Fiskalni storno
             [Fiskalni printer — referenca na originalni fiskalBroj]

SIGURNOSNI PROBLEMI:
  🔴 Storno bez atomske transakcije
  ⚠️ Nema RBAC — svaki recepcioner može stornirati
```

---

## DFD-05: TOURISM BOARD (TZ) INTEGRATION

```
[frmPrijava1.vb — Check-in]
    │
    ▼
[clasTZ.vb]
    │
    ├──► Build URL
    │    "http://test.prijava.ba/api/api.php?user=nerminc&pass=nermin1234&..."
    │    [🔴 Hardcoded credentials, HTTP bez HTTPS]
    │
    ├──► WebClient.DownloadString(url)
    │    [⚠️ Synchronous — blokira UI thread]
    │    [⚠️ Nema timeout]
    │
    └──► Parse XML response
         dstz.ReadXml(srXmlData)
         [🔴 XXE vulnerability — bez DTD restrikcija]
         ▼
        [MySQL: update boravak status]

PODACI KOJI IDU NA TZ SERVER (bez enkripcije):
  - Ime i prezime gosta
  - Broj dokumenta / pasoša
  - Datum dolaska/odlaska
  - Hotel ID / Subjekt ID
```

---

## DFD-06: EMAIL SENDING FLOW

```
RECEPCIONAR
    │
    ▼
[frmMail.vb]
    │
    ├──► Dohvati SMTP config
    │    SELECT FROM mailkonfig
    │    [🔴 Plaintext password u DataSet memoriji]
    │
    ├──► Pripremi email
    │    From: ds["odkogaMail"]
    │    Pass: ds["pass"]  [plaintext]
    │    Attachment: "C:\Program Files\IMEDIA\HotelPRO\TuristickiIzvjestaj.pdf"
    │    [🔴 Hardcoded path, nema exists check]
    │
    └──► SmtpClient.Send()
         If sslProvjera = "1" Then EnableSsl = True
         [⚠️ SSL opciona — može ići plaintext]
         ▼
        [SMTP Server] → [Email recipient]

SIGURNOSNI PROBLEMI:
  🔴 SMTP lozinka u plaintext memoriji
  🔴 Hardcoded PDF attachment path
  🔴 SSL nije obavezan
```

---

## DFD-07: RFID CARD ACCESS CONTROL

```
RECEPCIONER (check-in)
    │
    ▼
[frmPrijava1.vb / frmOdjava1.vb]
    │
    ▼
[kard_imedia.vb / classKard.vb]
    │
    ├──► TCP connect to 192.168.1.100:9760
    │    [🔴 Hardcoded IP i port]
    │    [🔴 Winsock EOL COM object ili TcpClient]
    │
    ├──► Pošalji binarnu komandu (plaintext TCP)
    │    byte[] command = BuildCardCommand(sobaID, validFrom, validTo)
    │    stream.Write(command, 0, command.Length)
    │    [🔴 Bez enkripcije — MITM može otvoriti sobu]
    │
    └──► Čekaj odgovor
         Byte[] buffer = new Byte[1024]  [⚠️ Fixed buffer]
         Parse response bytes            [🔴 Bez bounds check]
         ▼
        [RFID Controller Hardware]
        ▼
        [Hotelska soba — elektronska brava]
```

---

## DFD-08: FTP SYNC / REMOTE UPDATE

```
[ftpUploa.vb — pozvan iz frmGlavni?]
    │
    ├──► HTTP GET "http://i-web.info/fdms1/snimiloc_h.php"
    │    [🔴 HTTP C&C — bez auth, bez potpisa]
    │    ▼
    │   Response: update instructions / file list
    │
    ├──► SQL query za sync status
    │    WHERE serial='" & serial & "' AND pc='" & My.Computer.Name & "'"
    │    [🔴 SQL injection — computer name]
    │
    ├──► FTPclient upload/download
    │    FTP (port 21, plaintext)
    │    [🔴 Sve u cleartext-u]
    │
    └──► Primjena updatea
         UPDATE tabela SET upd=1 [⚠️ Table name iz baze]
         ▼
        [MySQL: lokalna baza]

KRITIČNO: HTTP C&C + auto-update bez integrity check = Remote Code Execution
```

---

## DFD-09: NIGHT AUDIT FLOW

```
NOĆNI AUDITOR / SISTEM
    │
    ▼
[frmDnevniIzvjestaj.vb]
    │
    ├──► 1. Izračun dnevnog prometa
    │        SELECT FROM nocenja, placanje, troskovi WHERE datum=today
    │
    ├──► 2. Zaključivanje perioda
    │        UPDATE settings SET datumDo=today  [⚠️ Nema transakcije]
    │
    ├──► 3. Generisanje izvještaja (Crystal Reports)
    │        rptDnevniIzvjestaj.rpt → DataSet
    │        [⚠️ DB credentials u Crystal Reports ConnectionInfo]
    │
    └──► 4. Prenos podataka (FTP sync)
             [ftpUploa.vb] → centralni server
             ▼
            [MySQL: arhivska kopija]

SIGURNOSNI PROBLEMI:
  🔴 DB credentials u Crystal Reports fajlu
  ⚠️ Zaključivanje perioda nije atomsko
  ⚠️ Nema recovery ako proces pukne na pola
```

---

## DFD-10: AUTHENTICATION FLOW

```
KORISNIK
    │ (username + plaintext password)
    ▼
[frmKorisnik.vb — Login forma]
    │
    └──► SQL: "WHERE korisnik='" & user & "' AND pass='" & pass & "'"
         [🔴 SQL INJECTION — bypass moguć]
         [🔴 Password u plaintext u MySQL]
         ▼
        [MySQL: radnici tabela]
         │
         ▼
    IF rows.Count > 0 → LOGIN USPJEŠAN
    korisnikID = rows(0)["id"]  [Globalna var, nema session token]
         │
         ▼
    [frmGlavni.vb — MDI main window]
    Global state: korisnikID, korisnikNivo, korisnikIme

SIGURNOSNI PROBLEMI:
  🔴 SQL Injection bypass
  🔴 Plaintext password u bazi
  🔴 Nema session token (samo global int)
  🔴 Nema lockout po N neuspjeha
  🔴 Nema MFA
```

---

## ZAJEDNIČKI SIGURNOSNI PATERN — LEGACY

```
SVAKA FORMA:
    UI Event Handler
         │
         ▼ (direktno, bez servisnog sloja)
    String SQL Assembly [🔴 INJECTION RIZIK]
         │
         ▼
    MySqlCommand.ExecuteNonQuery()
    [Kroz globalnu konekciju iz Data.vb]
         │
         ▼
    [MySQL — bez at-rest enkripcije]

NOVI SISTEM MORA IMATI:
    UI Event Handler
         │
         ▼ HTTP Request
    API Controller (ASP.NET Core)
         │
         ▼ DTO validation
    Service Layer (business logic)
         │
         ▼ Repository
    Dapper / EF Core [parameterizovano]
         │
         ▼
    PostgreSQL [at-rest enkriptovano, TLS in-transit]
```
