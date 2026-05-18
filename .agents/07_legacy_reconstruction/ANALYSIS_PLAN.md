# Plan analize legacy poslovne logike

## Problem

568 datoteka u manifestu, file-po-file pristup traje sedmicama i ne hvata poslovnu logiku
jer ona prelazi preko vise formi. Vecina fajlova su Designer.vb (auto-generirani), .resx
(resursi), .rpt (binarni) - skoro bez poslovne vrijednosti.

## Strategija: Scenario-driven umjesto file-driven

Rradimo ~30 analiza po poslovnim tokovima. Svaka analiza uzima
grupu povezanih fajlova idokumentira poslovna pravila, SQL, statuse i edge case-ove
kao cjelinu.

---

## Faza 1: Infrastruktura i baza (RESITI PRVO - otkljuca sve ostalo)

### 1.1 SQL dump analiza - DATABASE MAP
- `bin/Database Backup 2019-02-01 08-31-26.sql` (9MB) - NAJVAZNIJI FAJL
- `bin/merona.sql` (1.4MB) - druga verzija baze
- `bin/New Project 20150407 2136.sql` (1.6MB) - treca verzija
- `HOTELVIP 20150602 0904.sql` (582KB) - cetvrta verzija
- `novaBazaJHotel 20150602 0848.sql` (634KB) - peta verzija
- `bazakasa.sql` (3.4KB) - kasa baza

**Cilj**: Izvuci sve tabele, kolone, stored procedure, funkcie, trigger-e, view-e.
Identifikovati: koja je NAJNOVIJA verzija baze (vjerovatno 9MB dump).
Napraviti DATABASE_SCHEMA.md sa svim tabelama, kolonama, tipovima.

**Output**: DATABASE_SCHEMA.md, DATABASE_WRITES.md, DATABASE_PROCEDURES.md

### 1.2 Globalni infrastrukturni fajlovi
- `Data.vb` (32KB) - globalne varijable, ConnStr, konekcija
- `ModuleKod.vb` (215KB) - NAJVECI FAJL, globalne funkcije,的核心 poslovne logike
- `Settings.Designer.vb` (27KB) - konfiguracija
- `app.config` (7KB) - konekcioni stringovi, postavke
- `clasMysqlAdapt.vb` (2KB) - data adapter (vec analiziran)
- `funkcije.vb` (29KB) - utility funkcije
- `konfiguracija.vb` (1KB) - konfiguracija

**Cilj**: Mapirati globalne varijable, konekcione stringove, utility funkcije,
i二十大 poslovne funkcije u ModuleKod.vb.

**Output**: GLOBALS.md, MODULEKOD_FUNCTIONS.md

---

## Faza 2: P0 poslovni tokovi (KRITICNO)

Svaki tok analizira relevantne .vb fajlove (NE Designer, NE resx)
kao grupu, sacross-reference na SQL i ModuleKod.

### 2.1 Check-in / Prijava gosta
- `frmPrijava1.vb` (59KB) - glavna forma za prijavu
- `frmPrijavaGostiKucice.vb` (77KB) - unos gostiju
- `frmPrijavaGostiUnos.vb` (137KB) - dosta velika, detaljan unos
- `frmPrijavaBoravkaPodaci.vb` (0.8KB) - podaci boravka
- `ModuleKod.vb` (relevantne funkcije) - fnSobaStatus, addRelGostSoba, itd.
- `Data.vb` - globalne varijable za prijavu

**Cilj**: Mapirati poslovni tok prijave gosta, status sobe, relgostsoba, nocenja.

### 2.2 Stay / Boravak / Nocenja
- `frmPrikazNocenja1.vb`, `frmPrikazNocenja2.vb`
- `ModuleKod.vb` - Unesinocenja, funkcije nocenja
- SQL procedure za nocenja

**Cilj**: Mapirati kako se nocenja racunaju i biljeze (materialized ledger).

### 2.3 Room Status / Sobe
- `frmSobe.vb` (35KB) - upravljanje sobama
- `FormSobe/Krivacuprija/frmSobe.vb` (45KB) - druga verzija?
- `frmSobaInfo.vb` (102KB) - informacije o sobi
- `frmSobaInfoPromjena.vb`, `frmSobaistorija.vb`
- `frmSobarice.vb`, `frmKardSobarica.vb`
- `ModuleKod.vb` - fnSobaStatus

**Cilj**: Mapirati statuse sobe (slobodna, zauzeta, ciscenje, OOO), fnSobaStatus logiku.

### 2.4 Rezervacije / Booking
- `frmRezervacije.vb` (69KB) - glavna forma
- `frmRezervacije_unos.vb` (59KB) - unos rezervacije
- `frmRezervacijeNove.vb` (22KB) - nove rezervacije
- `frmRezervacijePrebaci.vb` (44KB) - prebacivanje
- `frmRezervacijePregled.vb` (39KB) - pregled
- `ModuleKod.vb` - rezervacione funkcije

**Cilj**: Mapirati tok rezervacije, potvrde, otkazivanja, veza sa sobama.

### 2.5 Placanje / Payment
- `frmPlacanje.vb` (315KB) - NAJVECI FAJL! kljucni za placanje
- `frmPlacanjeSlozeno.vb` (9KB) - slozeno placanje
- `frmPlacanjePo.vb` (3KB) - placanje po stavci
- `frmPlacanjeTarifa.vb` (3KB) - placanje tarifa
- `frmPlati1.vb` (45KB) - drugi dio placanja
- `frmPlacproc.vb` (2KB) - proces placanja

**Cilj**: Mapirati tok placanja, nacine placanja, racuni, storno, fiskalizacija.

### 2.6 Troskovi / Expenses
- `frmTroskovi.vb` (48KB) - troskovi
- `frmTroskoviNoc.vb` (18KB) - troskovi po nocenju
- `frmTrosSvi.vb` (1KB) - svi troskovi

**Cilj**: Mapirati troskove, kategorije, zakljucavanje (zaklj), veza sa folio.

### 2.7 Racuni / Invoice
- `frmRacuni.vb` (227KB) - upravljanje racunima
- `frmRacun.vb` (0.6KB) - pojedinacni racun
- `frmRaccopy.vb` (12KB) - kopiranje racuna
- `frmOdjava1.vb` (44KB) - odjava (checkout)
- `ModuleKod.vb` - addFolio, print racuni

**Cilj**: Mapirati tok racuna, invoice kao snapshot, storno racuna, checkout.

### 2.8 Fiskalizacija
- `frmFiskal.vb` (37KB) - fiskalna kasa
- `frmFiskall.vb` (0.7KB)
- Fajlovi: `0FS_rr.in`, `0rr.in`, `GKexp.in`, `JRexp.in`, `KIFexp.in`, `rr.in`, `rrH.in`
  (fiskalni ulazni templejti)

**Cilj**: Mapirati fiskalni tok, stampu fiskalnog racuna, veza sa placanjem.

### 2.9 Predracuni / Proforma
- `frmPredracun.vb` (40KB) - predracuni

**Cilj**: Mapirati tok predracuna, veza sa rezervacijom i placanjem.

---

## Faza 3: P1 poslovni tokovi

### 3.1 Gosti / Guests
- `frmGosti.vb` (258KB) - upravljanje gostima
- `Data.vb` - gost varijable

### 3.2 Izvjestaji / Reports
- `frmIzvjestaji.vb` (34KB) - glavna forma
- `frmIzvjestajiDnevni.vb` (74KB) - dnevni izvjestaji
- `rptRacunFrm.vb` (23KB) - stampa racuna
- Svi report .vb fajlovi (imena patter rptXxx.vb)

### 3.3 Partneri / Agencies
- `frmPartner1.vb` (17KB), `frmPartneri.vb` (15KB)

### 3.4 Tarife i popusti
- `frmTarife.vb` (57KB)

### 3.5 Karte / RFID
- `classKard.vb` (12KB), `frmKardRw.vb` (123KB), `frmKardPro.vb` (68KB)
- `kard_imedia.vb` (21KB), `kard_imedia1.vb` (41KB)

### 3.6 Korisnici, smjene, postavke
- `frmLogin.vb` (23KB), `frmpostavke.vb` (83KB)
- `frmRadnik.vb` (3KB), `frmZurnal1.vb` (61KB)

### 3.7 Export / Integracije
- `frmExport.vb` (32KB), `frmMail.vb` (10KB), `frmMailKonfig.vb` (5KB)

### 3.8 Baza podataka (admin)
- `frmBaza.vb` (89KB), `frmBazaPas.vb` (1KB), `frmPosBaze.vb` (12KB)
- `frmKonta.vb` (2KB), `frmKontaP.vb` (1KB)

---

## Faza 4: P2 / Nisko prioriterno

### 4.1 Telefone / PABX
- `frmTelefonskiImenik.vb`, `frmTelefonskiImenikUnos.vb`, `frmTelPostavke.vb`, `frmTelefon.vb`

### 4.2 Ostale forme
- `frmGlavni.vb` (184KB) - glavni meni, navigacija
- `frmDodaj.vb`, `frmDodajDrzave.vb`, `frmDodatno.vb`
- `frmSobaInfo.vb`, `frmSobaistorija.vb`
- `frmWeb.vb`, `frmExpNo.vb`, `frmNoviIzvor.vb`, `frmNoviTip.vb`

### 4.3 Report templejti (nizak prioritet)
- XML templejti za Crystal Reports / RDLC
- .rpt binarni (zahtijevaju specijalan tool)

---

## Faza 5: Cross-referenciranje i validacija

### 5.1 SQL mapiranje
Kad se Faza 1 završi, svaka Faza 2-4 analiza referencira tabele/kolone/procedure
iz DATABASE_SCHEMA.md sa konkretnim linijama iz .vb fajlova.

### 5.2 Status matrica
Kad se identificiraju svi statusi (sobe, rezervacije, placanja, troskovi),
napraviti STATUS_MATRIX.md sa svim tranzicijama.

### 5.3 Golden scenarios
Na kraju, za svaki P0 flow napisati golden scenario (korak-po-korak)
sa referencama na dokaze.

---

## Redosled izvrsavanja

| Korak | Sto | Agenti | Vrijeme (procjena) |
|-------|-----|--------|---------------------|
| 1 | SQL dump analiza (najnoviji 9MB) | 1 agent | 2-3 sata |
| 2 | Data.vb + ModuleKod.vb analiza | 1-2 agenta | 3-4 sata |
| 3 | P0 tokovi paralelno (2.1-2.9) | do 9 agenata paralelno | 4-6 sati po toku |
| 4 | P1 tokovi paralelno (3.1-3.8) | do 8 agenata paralelno | 2-3 sata po toku |
| 5 | P2 + cross-ref | 2-3 agenta | 2-3 sata |
| **UKUPNO** | | | **~20-30 sati** umjesto sedmica |

## Kljucna pravila

1. **Ne citamo** Designer.vb, .resx, .rpt, bin/ obj/ fajlove - to je sum
2. **Svaki dokaz** mora imati `legacy_code/path:line` referencu
3. **Poslovno pravilo** = ono sto korisnik vidi ili sto utice na podatke u bazi
4. **SQL first** - kad god je moguce, citamo SQL dump da pronadjemo tabele i procedure,
   pa onda trazimo gdje se pozivaju u .vb kodu
5. **Ne pitamo se "sta bi trebalo biti"** - pitamo se "sta kod radi"
6. **Grupisemo** - vise formi koje ucestvuju u istom toku analiziramo zajedno
7. **Cross-reference** - kad naidjemo na funkciju iz ModuleKod.vb, biljezimo je
   i povezujemo sa tokovima koji je pozivaju

## Output struktura

```
LEGACY_ANALYSIS/
  00_DATABASE_SCHEMA.md       # Faza 1.1 - tabela, kolone, procedure
  01_GLOBALS.md               # Faza 1.2 - globalne varijable, ConnStr
  02_MODULEKOD_FUNCTIONS.md   # Faza 1.2 - sve funkcije iz ModuleKod.vb
  10_CHECKIN.md               # Faza 2.1
  11_STAY_NIGHTS.md           # Faza 2.2
  12_ROOM_STATUS.md           # Faza 2.3
  13_RESERVATIONS.md          # Faza 2.4
  14_PAYMENT.md               # Faza 2.5
  15_EXPENSES.md              # Faza 2.6
  16_INVOICE_CHECKOUT.md      # Faza 2.7
  17_FISCAL.md                 # Faza 2.8
  18_PROFORMA.md              # Faza 2.9
  20_GUESTS.md                # Faza 3.1
  21_REPORTS.md               # Faza 3.2
  22_PARTNERS.md              # Faza 3.3
  23_TARIFFS_DISCOUNTS.md     # Faza 3.4
  24_CARDS_RFID.md            # Faza 3.5
  25_USERS_SHIFTS.md          # Faza 3.6
  26_EXPORT_INTEGRATION.md    # Faza 3.7
  27_DATABASE_ADMIN.md        # Faza 3.8
  30_STATUS_MATRIX.md          # Faza 5.2
  40_GOLDEN_SCENARIOS.md      # Faza 5.3
  50_LEGACY_TO_NEW_MAPPING.md # Final mapping
```

Svaki dokument koristi ISTI evidence format:
`legacy_code/path:line` za svaku tvrdnju.