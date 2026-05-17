# MIGRACIONA POLITIKA KODA I POSLOVNE LOGIKE - HotelPRO

Status: AUTHORITATIVE PROPOSAL
Datum: 2026-05-17
Agent: Codex
Restore point prije ove faze: 70d469c (`docs: add initial plan vs code audit`)

## 1. Svrha

Ova politika definiše kako se legacy VB.NET/MySQL hotelska aplikacija smije analizirati i kako se poslovna logika smije migrirati u novi .NET/Next/PostgreSQL sistem.

Glavni cilj je spriječiti ponavljanje istog problema: nova aplikacija ne smije biti samo moderni CRUD/API scaffold. Mora dokazivo prenijeti poslovna pravila iz operativne legacy aplikacije.

## 2. Git i Sigurnost Izvornog Legacy Koda

`legacy_app/` i `graphify-out/` su lokalni-only folderi.

Pravila:

- `legacy_app/` se nikada ne commituje.
- `graphify-out/` se nikada ne commituje.
- U Git se smiju commitovati samo izvedeni audit dokumenti: mape, zaključci, test scenariji i migracioni plan.
- Ne smije se kopirati dugačak legacy source kod u dokumentaciju.
- Dozvoljeno je navesti legacy fajl, funkciju i kratak opis pravila.
- Legacy kod se tretira kao read-only referenca.

Implementirana zaštita:

- `.gitignore` ignoriše kompletan `legacy_app/`.
- `.gitignore` ignoriše kompletan `graphify-out/`.
- `git ls-files legacy_app graphify-out` ne vraća tracked fajlove.

## 3. Izvor Istine

Redoslijed izvora istine za migraciju:

1. Operativni legacy kod u `legacy_app/Radna/`
2. Legacy MySQL dump: `legacy_app/Radna/novaBazaJHotel 20150602 0848.sql`
3. Legacy Crystal/XML report definicije
4. Postojeći novi kod
5. `.agents` FSD/task dokumenti

Ako novi kod i legacy pravilo nisu usklađeni, legacy pravilo ima prioritet dok korisnik ne odluči drugačije.

## 4. Dokaz da je Pravilo Migrirano

Poslovno pravilo se smatra migriranim samo ako postoje sva četiri dokaza:

1. `Legacy source`: fajl + funkcija/procedura/tabela
2. `Modern target`: novi servis/API/entitet/UI mjesto
3. `Scenario`: ulazni podaci i očekivani ishod
4. `Test`: unit/integration/snapshot test ili ručna verifikacija sa zapisom

Bez ova četiri dokaza status je najviše `PARTIAL`, nikad `COMPLETED`.

## 5. Zabranjeni Migracioni Obrasci

Zabranjeno:

- Prepisati ekran kao CRUD bez poslovnih pravila.
- Označiti mock integraciju kao završenu.
- Uzimati FSD kao dokaz ako kod ne postoji.
- Uzimati postojeći novi kod kao dokaz ako nije upoređen sa legacy logikom.
- Migrirati finansijske tokove bez testova.
- Primati raw broj kartice kroz backend.
- Hardkodirati PDV, fiskalne brojeve, POS secret, hotel podatke ili sekvence računa.
- Izgubiti legacy polja jer "ne izgledaju moderno".

## 6. Migracioni Proces

### Faza A - Inventar Legacy Sistema

Napraviti katalog:

- forme
- klase
- globalni moduli
- stored procedure / funkcije
- tabele
- reporti
- integracije

Početno identificirani centralni fajlovi:

| Oblast | Legacy fajlovi |
|---|---|
| Globalna DB/procedure/logika | `Data.vb`, `ModuleKod.vb`, `funkcije.vb` |
| Sobe/status/transfer | `frmSobaInfo.vb`, `frmSobe.vb`, `frmSobe_Set.vb` |
| Rezervacije | `frmRezervacije_unos.vb`, `frmRezervacijePregled.vb`, `frmRezervacijePrebaci.vb`, `frmRezervacijeNove.vb` |
| Prijava/check-in | `frmPrijava1.vb`, `frmPrijavaGostiUnos.vb`, `frmPrijavaGostiKucice.vb` |
| Odjava/check-out | `frmOdjava1.vb`, `Data.vb` |
| Plaćanje/folio/avansi | `frmPlacanje.vb`, `frmPlacanjeSlozeno.vb`, `frmPredracun.vb` |
| Računi/storno/fiskal | `frmRacuni.vb`, `frmFiskal.vb`, `ModuleKod.vb` |
| Gosti/partneri | `frmGosti.vb`, `frmPartneri.vb`, `frmPartner1.vb` |
| Izvještaji | `frmIzvjestaji*.vb`, `*.rpt`, `*.xml` |
| RFID kartice | `classKard.vb`, `frmKard*.vb`, `kard_imedia*.vb` |
| Kontroleri soba / IoT-like | `ClassLuxM.vb` |
| Telefonija | `frmTelefon.vb`, `frmTelPostavke.vb`, `telpozivi` tabela |
| Turistička zajednica | `clasTZ.vb`, `PrijavaTurist*.rpt`, `rptTuristicka*` |

### Faza B - Ekstrakcija Poslovnih Pravila

Za svaki modul izvući:

- naziv pravila
- legacy lokacija
- ulazni podaci
- izlaz/side effect
- tabele koje mijenja
- edge case
- šta je novo ponašanje
- test scenario

Primjer formata:

```text
RULE: Soba dobija status "odlazak danas / prekoracen boravak"
Legacy: ModuleKod.vb, fnSobaStatus
Input: SoID, datumP, datumK, tod
Condition: postoji aktivni relgostsoba gdje tod >= CheckOutDate i rezervacija = 0
Result: status = 2
Modern target: RoomOccupancyPolicy.GetStatus(...)
Test: aktivan boravak sa checkout prije sadašnjeg vremena vraća DueOut/Overdue
```

### Faza C - Canonical Domain Model

Ne migrirati tabele 1:1 ako to zaključa loš dizajn, ali nijedno legacy značenje ne smije nestati.

Minimalni canonical moduli:

- Room Inventory
- Room Occupancy Status
- Reservation
- Stay / GuestRoomAssignment
- Visit/Folio
- Night Charge
- Charge
- Payment
- Invoice
- Fiscal Receipt
- Storno/Reversal
- Guest Identity Document
- Partner/Agency
- Tourist Registration
- Hardware Command
- Phone Call Charge

### Faza D - Implementacija u Tankim Vertikalnim Slojevima

Za svaki poslovni tok:

1. napisati test iz legacy scenarija
2. implementirati domenski servis
3. dodati API
4. spojiti UI
5. validirati izvještaj/side effects

Ne raditi prvo UI, jer UI može maskirati pogrešnu poslovnu logiku.

### Faza E - Migracija Podataka

Data migracija je poseban tok i ne smije biti "best effort".

Za svaku legacy tabelu:

- broj redova u legacy bazi
- ciljna tabela/entitet
- mapiranje kolona
- transformacije
- validacioni SQL
- tolerancije
- ručni izuzeci

Ako se tabela ne migrira, mora postojati odluka `DROP WITH REASON`.

## 7. Kritične Legacy Tabele

Početno identificirane tabele iz dumpa:

| Legacy tabela | Poslovno značenje | Migracioni status u novom kodu |
|---|---|---|
| `sobe` | fizičke sobe, OOO/clean/status polja | PARTIAL |
| `sobavrsta` | tipovi soba | PARTIAL |
| `sobatarifa` | tarife | PARTIAL |
| `relgostsoba` | boravak/prijava/odjava/rezervacija po sobi | CRITICAL |
| `posjetafolio` | folio/posjeta | CRITICAL |
| `nocenja` | generisana noćenja | CRITICAL |
| `troskovi` | otvoreni i zaključeni troškovi | CRITICAL |
| `troskovivrste` | vrste usluga/troškova | PARTIAL |
| `placanje` | plaćanje računa | CRITICAL |
| `placanjedetalji` | stavke plaćanja i noćenja | CRITICAL |
| `printracuni` | header računa | CRITICAL |
| `printracunidetalji` | stavke računa | CRITICAL |
| `printracunifooter` | napomene/avans/noćenja footer | P1 |
| `printracuniavans` | avansni računi | CRITICAL |
| `predracuni`, `predracunidet` | predračuni | P1 |
| `rezervacije` | rezervacije | CRITICAL |
| `rezervacijegrupe` | blok/grupa | P1 |
| `rezervacijeizvor` | izvor rezervacije | P1 |
| `rezervacijetip` | tip rezervacije | P1 |
| `gosti` | gosti | CRITICAL |
| `gostdokument` | tipovi dokumenata | P1 |
| `partneri` | partneri/agencije/firme | P1 |
| `radnici` | korisnici/radnici | CRITICAL |
| `sobaricalog` | housekeeping log | P1 |
| `telpozivi` | telefonski pozivi | P1 |
| `fisc` | fiskalne postavke/stanja | P0 |

## 8. Prvi Izvučeni Poslovni Zaključci

### 8.1 Status sobe nije običan enum

Legacy funkcija `fnSobaStatus` vraća statuse na osnovu kombinacije:

- postoji aktivan `relgostsoba`
- aktivna rezervacija
- rezervacija potvrđena/nepotvrđena
- trenutni datum u odnosu na `checkOutDate`
- soba je `ooo`
- soba može biti istovremeno rezervisana i zauzeta

Statusi iz komentara:

- `0`: slobodna
- `1`: zauzeta
- `2`: zauzeta, gost se odjavljuje danas ili je prekoracio datum odlaska
- `3`: rezervisana i potvrđena
- `4`: rezervisana i zauzeta
- `5`: van upotrebe
- `6`: rezervisana i nije potvrđena

Moderni `RoomStatus` enum ne pokriva sve ove izvedene statuse bez dodatne policy logike.

### 8.2 Check-out zatvara više tabela, ne samo booking

Legacy `Data.vb/OdjavaSobe` radi više side effecta:

- update `nocenja` na `PrijavaOdjava = 1`
- po potrebi insert dodatnog noćenja za druge goste u sobi
- update `relgostsoba` kao odjavljen
- update `posjetaFolio` kao zaključeno
- update `troskovi` kao zaključeno
- označava sobu prljavom preko `sobe.clean = 0`

Moderni check-out mora biti transakcijski servis, ne jednostavna promjena booking statusa.

### 8.3 Noćenje je materializovan zapis

Legacy koristi proceduru `Unesinocenja`, koja prije inserta briše postojeće noćenje za isti `RID` i datum, zatim upisuje:

- `RID`
- `DatumP`
- `Tarifa`
- `SID`
- `PID`
- `PrijavaOdjava = 0`
- `opis`
- `popust`
- `soba`

To znači da noćenja nisu samo izvedena iz datuma rezervacije; ona su poslovni ledger.

### 8.4 Račun/storno vraća troškove u otvoreno stanje

Legacy `frmRacuni.vb` kod storna radi:

- `placanje.storno = 1`
- `placanjeDetalji.storno = 1`
- `printracuni.storno = 1`
- `troskovi.zaklj = 0, Brrac = null` za nenocenje troškove
- briše noćenje/trošak računa za `TID = 1`
- opciono zove fiskalni storno

Moderni storno ne smije samo napraviti negativnu stavku; mora vratiti operativno stanje gdje legacy to radi.

### 8.5 Fiskalizacija je stvaran driver flow

Legacy koristi `Tring.Fiscal.Driver.TringFiskalniPrinter`, kreira artikle, poreske stope i račun. Novi `BridgeService` trenutno generiše mock fiskalni kod, što nije migracija.

### 8.6 Rezervacije imaju posebne statuse i preklopne upite

Legacy rezervacije razlikuju:

- `potvrda`
- `stornirana`
- `prijava`
- `brojPotvrde`
- `brojStorna`
- `blokID`
- `izvorID`
- `tipID`
- `brojRezSoba`

Pregled rezervacija koristi preklop intervala, ne samo jedan jednostavni date range.

## 9. Migracioni Prioriteti

### P0 - Bez ovoga aplikacija nije poslovno pouzdana

1. Room status policy iz `fnSobaStatus`
2. Stay/Folio model iz `relgostsoba` + `posjetafolio`
3. Night ledger iz `nocenja`
4. Check-in/check-out transakcije
5. Invoice/payment/storno/fiscal flow
6. Legacy ETL za goste, boravke, račune, plaćanja, troškove i noćenja

### P1 - Operativno važno

1. Rezervacije i grupne rezervacije
2. Partneri/agencije
3. Turistička evidencija
4. Report parity
5. Housekeeping clean/OOO/sobarica log
6. RFID/PABX

### P2 - Nakon core migracije

1. Channel manager
2. Revenue management
3. Guest portal
4. IoT dashboard

## 10. Predložena Nova Arhitektura Poslovne Logike

Ne ugrađivati pravila direktno u kontrolere. Kreirati domenske servise:

| Servis | Odgovornost |
|---|---|
| `RoomOccupancyPolicy` | izvedeni status sobe iz boravaka, rezervacija, OOO i datuma |
| `StayLifecycleService` | check-in, room assignment, transfer, check-out |
| `NightLedgerService` | generisanje, korekcija i zatvaranje noćenja |
| `FolioLedgerService` | charges, transfers, partial charges, closing |
| `InvoiceWorkflowService` | invoice/proforma/advance/storno lifecycle |
| `FiscalizationService` | fiscal request/response, retries, reversals |
| `ReservationPolicyService` | availability, confirmation, cancellation, group block |
| `LegacyMigrationService` | audited data migration with row reconciliation |

## 11. Validaciona Strategija

Za svaku migraciju:

- `golden legacy scenario`: opis scenarija iz legacy aplikacije
- `input fixture`: minimalni podaci
- `expected ledger`: očekivane promjene u tabelama
- `modern test`: test nad novim servisom
- `reconciliation`: poređenje brojeva i suma

Minimalni P0 test set:

1. check-in gosta u slobodnu sobu
2. soba sa potvrđenom rezervacijom
3. soba zauzeta i checkout danas
4. generisanje noćenja za jednu noć
5. check-out zatvara boravak i prlja sobu
6. trošak restorana ulazi u folio
7. račun zaključava troškove
8. storno računa vraća troškove
9. avans se povezuje sa konačnim računom
10. fiskalni neuspjeh ne smije izgubiti račun

## 12. Odluke Koje Treba Potvrditi Sa Korisnikom

1. Da li novi sistem mora 100% čuvati legacy finansijski model ili smije imati modernizovan ledger uz punu migraciju?
2. Da li fiskalizacija mora podržati iste uređaje odmah ili prvo mock + jasna oznaka demo?
3. Da li se historijski računi migriraju kao read-only arhiva ili kao aktivni ledger?
4. Da li se legacy `PID`/`relgostsoba.ID` čuvaju kao javni referentni brojevi ili samo kao legacy mapping?
5. Da li se turistička evidencija mora uskladiti sa konkretnom državom/kantonom/opštinom prije release-a?

## 13. Sljedeći Radni Koraci

1. Napraviti `EXTRACTED_BUSINESS_LOGIC.md` sa pravilima iz P0 modula.
2. Izvući detaljno `fnSobaStatus`, `Unesinocenja`, `OdjavaSobe`, storno i fiskalni flow.
3. Uporediti svaki P0 flow sa postojećim modernim servisima.
4. Napisati `LEGACY_TO_MODERN_DATA_MAPPING.md`.
5. Tek nakon odobrenja početi implementacijske izmjene.
