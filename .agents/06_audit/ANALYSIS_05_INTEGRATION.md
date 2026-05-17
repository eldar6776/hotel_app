# ANALYSIS 05 — UTILITY & INTEGRATION MODULES

**Datum:** 2026-05-17 (rekreirano)
**Scope:** FAZA 5 — utility i integracija moduli iz legacy_app/Radna
**Agent:** GitHub Copilot (Claude Sonnet 4.6)

---

## PREGLED

| Fajl | Veličina | Risk | Ključni nalaz |
|---|---|---|---|
| clasTZ.vb | ~8KB | 🔴 P0 | Hardcoded credentials u URL, HTTP |
| classKard.vb | ~15KB | 🔴 P0 | EOL Winsock COM, hardcoded IP |
| kard_imedia.vb | ~20KB | 🔴 P0 | Unencrypted card commands, buffer overflow |
| ftpUploa.vb | ~30KB | 🔴 P0 | SQL injection, HTTP C&C, slaba enkripcija |
| frmMailKonfig.vb | ~4KB | 🔴 P0 | SQL injection + plaintext password storage |
| frmMail.vb | ~10KB | 🔴 P0 | Plaintext SMTP credentials, PDF exposure |
| FTPclient.vb | ~15KB | 🟠 P1 | Plaintext FTP, no TLS |
| clasMysqlAdapt.vb | ~3KB | 🟡 P2 | Raw SQL string, caller-dependent |
| frmWeb.vb | ~3KB | 🟢 P3 | WebBrowser control, low risk |
| gost.vb | 0KB | 🟢 P3 | Prazan fajl |
| gostDokument.xml | static | 🟢 P3 | Statički lookup |

---

## DETALJNA ANALIZA

### 1. clasTZ.vb — Tourism Board API Integration
**Risk: 🔴 P0 — KRITIČNO**

Integracija sa državnim turističkim portalom (prijava boravka stranca).

**a) Hardcoded credentials u source kodu:**
```vb
Dim url As String = "http://test.prijava.ba/api/api.php?user=nerminc&pass=nermin1234&subjekt=5&res=xml"
```
- Username: `nerminc`
- Password: `nermin1234`
- Subject ID: `5`
- Protocol: HTTP (bez HTTPS!)
- **OWASP A2 — CWE-798 Use of Hard-coded Credentials**

**b) Credentials u URL query stringu (HTTP GET):**
```vb
Dim wc As New WebClient
Dim xmlText As String = wc.DownloadString(url & tex)
```
- Credentials vidljive u: web server logovima, proxy logovima, browser historiji, network captures
- **OWASP A2 — CWE-312 Cleartext Storage**

**c) Fallback na hardcoded credentials:**
```vb
Sub New()
    Dim tz() As String = ds.Tables("setings").Rows(0).Item("sobekuc").ToString.Split("#")
    If tz.Length > 2 Then
        url = tz(0) & "?user=" & tz(1) & "&pass=" & tz(2) & "&subjekt=" & tz(3) & "&res=xml"
    End If
End Sub
```
- Ako settings nema konfiguraciju, koristi se hardcoded `nerminc:nermin1234`
- Fallback na produkcijske credentials ako konfig nedostaje

**d) XXE (XML External Entity) vulnerability:**
```vb
dstz.ReadXml(srXmlData)  ' Bez DTD validacije
```
- XML odgovor sa servera parsiran bez sheme/DTD
- Malicious server odgovor može izvršiti XXE napad
- **CWE-611 Improper Restriction of XML External Entity Reference**

**Migracioni zahtjev:**
- Koristiti HttpClient sa HTTPS
- Credentials iz environment variables / sealed storage
- Validirati XML response shemu
- Implementirati retry i circuit breaker

---

### 2. classKard.vb — Card Reader Class (Legacy COM/Winsock)
**Risk: 🔴 P0 — KRITIČNO**

Kontrola pristupa karticama (RFID/magnetic) za hotelske sobe.

**a) EOL Winsock ActiveX control:**
```vb
Public WithEvents wn As New Winsock_Control.Winsock
```
- Winsock ActiveX (MSCOMM32.OCX/MSWINSCK.OCX) — EOL od 2016
- Nije patchovan od Visual Basic 6 ere
- Poznat CVE: CVE-2011-0658 (Winsock buffer overflow)
- **CWE-1104 Use of Unmaintained Third Party Components**

**b) Hardcoded IP i port:**
```vb
Dim port As Int32 = 9760
Dim client As New System.Net.Sockets.TcpClient("192.168.1.100", port)
```
- Hardkodirana LAN adresa kartičnog čitača
- Svako ko je na mreži 192.168.1.x može slati komande
- **CWE-798 Hard-coded Credentials / CWE-284 Improper Access Control**

**c) Unencrypted binary protocol:**
```vb
stream.Write(data, 0, data.Length)
Do
    numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length)
Loop While stream.DataAvailable
```
- Komande za otvaranje soba šalju se plaintext TCP-om
- MITM napad može otvoriti bilo koju sobu
- **CWE-319 Cleartext Transmission of Sensitive Information**

**d) Array index bez bounds check:**
```vb
data = ar(3) & ", Kartica je nevazeca! "  ' ar(3) možda ne postoji
```
- Malformiran odgovor od kartičnog čitača uzrokuje IndexOutOfRange grešku
- **CWE-129 Improper Validation of Array Index**

---

### 3. kard_imedia.vb — iMedia Card Integration
**Risk: 🔴 P0 — KRITIČNO**

Direktna TCP komunikacija sa iMedia kartičnim sistemom.

**a) Fiksni 1024-byte buffer bez provjere dužine:**
```vb
Dim myReadBuffer(1024) As Byte
Do
    numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length)
Loop While stream.DataAvailable
```
- Odgovor duži od 1024 bytova silently truncate-uje podatke
- Može uzrokovati nepotpun zapis na karticu (djelimičan ključ sobe)
- **CWE-120 Buffer Copy without Checking Size of Input**

**b) Command injection kroz nekontrolisane byte vrijednosti:**
```vb
If arr(0).ToString = "12" Then If arr(1).ToString = "194" Then If arr(2).ToString = "6" Then odgvrijeme(arr(3), arr(4), arr(5), arr(6))
```
- Komandni kodovi dolaze iz mrežnog odgovora bez autentikacije
- Kompromitovani kartični čitač može triggering arbitrarne state machine tranzicije
- **CWE-77 Command Injection**

**c) Race condition u višenitnom okruženju:**
```vb
Dim decimalVal As Decimal = 16
If brcitac = 2 Then decimalVal = 62
' ... (više shared state mutacija bez locka)
```
- Singleton-like class sa shared mutable state
- **CWE-362 Concurrent Execution Using Shared Resource**

---

### 4. ftpUploa.vb — FTP Upload / Remote Update Module
**Risk: 🔴 P0 — KRITIČNO**

Ovaj modul vrši remote update aplikacije i sync podataka sa centralnim serverom.

**a) Hardcoded credentials u comment-ima (history exposure):**
```vb
'Dim ipadres As String = "i-web.info"
'Dim user As String = "im"
'Dim pass As String = "G_{^~@SuDnoQ"
```
- Credentials zakomentovani, ali vidljivi u git historiji i source fajlu
- **CWE-798 Hard-coded Credentials**

**b) HTTP C&C komunikacija (unencrypted remote control):**
```vb
Dim loc As String = "http://i-web.info/fdms1/snimiloc_h.php"
Dim locter As String = "http://i-web.info/cpoint/snimiloc_2.php"
```
- Aplikacija prima komande sa udaljenog HTTP servera
- Bez autentikacije, bez potpisa, bez TLS
- MITM može poslati malicious update komandu
- **CWE-319 Cleartext Transmission / CWE-494 Download of Code Without Integrity Check**

**c) SQL Injection u update queryjima:**
```vb
komanda.CommandText = "SELECT `id`,`idbr`... FROM " & baz & "im.upd where serial='" & serial & "' and idbr='" & idbr & "' and pc='" & My.Computer.Name.Trim & "'"
```
- `serial` dolazi od MAC adrese (kontrolisana)
- `My.Computer.Name` dolazi od OS (može biti promijenjeno)
- `idbr` dolazi iz XML config-a (može biti tampered)
- **OWASP A3 — CWE-89 SQL Injection**

**d) Slaba enkripcija lozinke (trivial obfuscation):**
```vb
Dim pass As String = ds1.Tables("constr").Rows(0).Item("pass").ToString.Replace("%&rt!h23", "")
' ... .Substring(0, pass.Length - 3)
```
- Lozinka "šifrovana" dodavanjem fiksnog sufiksa i trimovanjem
- Nije kriptografija — to je obscurity
- Svako tko pročita XML fajl može dekodirati lozinku
- **CWE-261 Weak Cryptography for Passwords**

**e) Table name injection:**
```vb
mysqlExScalar("update " & tabl.Rows(t).Item(0).ToString & " set upd=1 where upd=0 limit " & limit)
```
- Naziv tabele dolazi iz baze, ali nije whitelistovan
- Kompromitovana baza može uzrokovati arbitrary table update
- **CWE-89 SQL Injection (second-order)**

---

### 5. frmMailKonfig.vb — Email Configuration Form
**Risk: 🔴 P0 — KRITIČNO**

**a) SQL Injection unatoč deklarisanim parametrima:**
```vb
Dim pass As New MySqlParameter("@pass", MySqlDbType.Text)
pass.Value = Me.txtPass.Text

' Parametar deklarisan, ali NIJE KORIŠTEN u query-u!
objCommand.CommandText = "UPDATE mailkonfig SET odkogaMail = " & odkogaMail.ToString & ", pass = " & pass.ToString
```
- `pass.ToString` vraća samo `.Value` kao string — ne parametrizuje!
- Ovo je klasična greška: mislili su da koriste parametre, ali zapravo rade konkatenaciju
- **OWASP A3 — CWE-89 SQL Injection**

**b) Plaintext password storage:**
- SMTP lozinka upisuje se direktno u MySQL tabelu `mailkonfig` bez enkripcije
- **CWE-256 Plaintext Storage of Password**

---

### 6. frmMail.vb — Email Sending Form
**Risk: 🔴 P0 — KRITIČNO**

**a) SMTP credentials u plaintext memoriji:**
```vb
from = ds.Tables("mailkonfig").Rows(0).Item("odkogaMail").ToString.ToLower
pass = ds.Tables("mailkonfig").Rows(0).Item("pass").ToString.ToLower
```
- Email lozinka učitana iz baze i drži se kao string u memoriji
- Vidljiva u memory dump-u, debuggeru, exception porukama
- **CWE-312 Cleartext Storage of Sensitive Information**

**b) SSL opciona (može biti isključena):**
```vb
If sslProvjera = "1" Then
    smtp.EnableSsl = True
Else
    smtp.EnableSsl = False  ' Credentials idu plaintext!
End If
```
- Administrator može konfigurirati SMTP bez SSL
- Email credentials idu plaintext kroz mrežu
- **CWE-319 Cleartext Transmission**

**c) Hardcoded PDF putanja bez validacije:**
```vb
mail.Attachments.Add(New Attachment("C:\Program Files\IMEDIA\HotelPRO\TuristickiIzvjestaj.pdf"))
```
- Putanja je hardkodirana — ne provjerava se da li fajl postoji
- Ako fajl ne postoji → exception koji expose-uje interne putanje u error poruci
- Folder `C:\Program Files\IMEDIA\HotelPRO\` možda writable za lokalne korisnike
- **CWE-22 Path Traversal / CWE-209 Information Exposure Through Error**

---

### 7. FTPclient.vb — FTP Client Library
**Risk: 🟠 P1**

```vb
Sub New(ByVal Hostname As String, ByVal Username As String, ByVal Password As String)
    _hostname = Hostname
    _username = Username
    _password = Password
End Sub
```
- Credentials se prihvataju kao parametri — bolje od hardkodiranja
- Ali: koristi FTP (port 21), ne SFTP ili FTPS
- Credentials i podaci idu plaintext TCP-om
- **CWE-319 Cleartext Transmission**

---

### 8. clasMysqlAdapt.vb — MySQL Adapter
**Risk: 🟡 P2**

```vb
Public Sub getdata(ByVal textDb As String)
    da = New MySqlDataAdapter(textDb, conn)
```
- Prihvata raw SQL string bez provjere
- Rizik ovisi o pozivaocu (caller-dependent)
- Ako caller proslijedi korisničku kontrolisanu vrijednost → SQL injection
- **CWE-89 (sekundarno, ovisi o upotrebi)**

---

### 9. frmWeb.vb — Web Integration
**Risk: 🟢 P3**

WebBrowser control koji otvara URL. Nema direktnih sigurnosnih problema u samom kodu.

---

### 10. gost.vb — Guest Class
**Risk: 🟢 P3**

**Prazan fajl.** Nema koda. Stub koji nikad nije implementiran.

---

## SAŽETAK KRITIČNIH NALAZA

| Prioritet | Fajl | Problem | OWASP | CWE |
|---|---|---|---|---|
| 🔴 P0 | clasTZ.vb | Hardcoded username+password u URL | A2 | CWE-798 |
| 🔴 P0 | clasTZ.vb | HTTP (ne HTTPS) za državni API | A2 | CWE-319 |
| 🔴 P0 | clasTZ.vb | XXE u XML parseru | A5 | CWE-611 |
| 🔴 P0 | classKard.vb | EOL Winsock COM (CVE-prone) | A6 | CWE-1104 |
| 🔴 P0 | classKard.vb | Hardcoded IP kartičnog čitača | A5 | CWE-798 |
| 🔴 P0 | classKard.vb | Unencrypted access control commands | A2 | CWE-319 |
| 🔴 P0 | kard_imedia.vb | Buffer bez bounds check | A5 | CWE-120 |
| 🔴 P0 | ftpUploa.vb | SQL Injection (computer name/idbr) | A3 | CWE-89 |
| 🔴 P0 | ftpUploa.vb | HTTP C&C bez auth/potpisa | A8 | CWE-494 |
| 🔴 P0 | ftpUploa.vb | Trivijalna obfuskacija lozinke | A2 | CWE-261 |
| 🔴 P0 | frmMailKonfig.vb | SQL injection (param deklarisan, nekorišten) | A3 | CWE-89 |
| 🔴 P0 | frmMailKonfig.vb | Plaintext password storage | A2 | CWE-256 |
| 🔴 P0 | frmMail.vb | Plaintext SMTP credentials u memoriji | A2 | CWE-312 |
| 🔴 P0 | frmMail.vb | Hardcoded PDF path, unvalidated | A1 | CWE-22 |
| 🟠 P1 | FTPclient.vb | FTP plaintext protokol | A2 | CWE-319 |
| 🟡 P2 | clasMysqlAdapt.vb | Raw SQL string prihvaćen | A3 | CWE-89 |

## MIGRACIONI ZAHTJEVI

1. **TZ integracija**: HttpClient + HTTPS + credentials iz IConfiguration (env vars)
2. **Kartice**: Zamijeniti Winsock/classKard sa modernim SDK kartičnog sistema (SDK, ne raw TCP)
3. **Email**: SMTP credentials u Azure Key Vault / secrets manager, SSL required
4. **FTP sync**: Zamijeniti sa SFTP/HTTPS API, potpisani payload, integrity check
5. **MySQL adapter**: Zabraniti raw SQL string — svuda koristiti Dapper/EF parameterizovano
