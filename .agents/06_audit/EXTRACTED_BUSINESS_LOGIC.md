# EXTRACTED BUSINESS LOGIC - INITIAL PASS

Status: WORKING - PARTIAL SOURCE EXTRACTION
Datum: 2026-05-17
Scope: prvi read-only prolaz kroz `legacy_app/Radna`

Vazno ogranicenje: nije procitan svaki legacy fajl. Inventar pokrivenosti je u `LEGACY_SOURCE_REVIEW_INVENTORY.md`. Do sada je uradjen inventar i ciljano citanje P0 tokova: sobe/status, check-in, check-out, folio, placanje, racun/storno/fiskal i rezervacije.

## 1. Cilj

Ovaj dokument izdvaja stvarnu poslovnu logiku iz legacy aplikacije. Ovo nije kompletna analiza cijelog sistema, nego početni P0/P1 sloj koji treba voditi migraciju.

Legacy izvor je lokalni-only i nije dio Git repozitorija.

## 2. Poslovni Model Koji Se Vidi Iz Legacy Koda

Legacy sistem nije jednostavan model `Room -> Booking -> Invoice`.

Stvarni operativni model je bliži ovome:

```text
Soba
  -> RelGostSoba: aktivni boravak, rezervacija ili prijava gosta u sobu
      -> PosjetaFolio/PID: folio/posjeta
          -> Nocenja: materializovana noćenja
          -> Troskovi: otvoreni troškovi po sobi/foliju
          -> Placanje/PlacanjeDetalji: naplate
          -> PrintRacuni/PrintRacunDetalji/Footer: račun kao fiskalno/print stanje
```

Ovo znači da moderni sistem mora imati ledger pristup, ne samo izračun iz trenutnog stanja.

## 3. RULE-ROOM-001: Izvedeni Status Sobe

Legacy lokacija:

- `ModuleKod.vb`, kreiranje funkcije `fnSobaStatus`
- dump: `novaBazaJHotel 20150602 0848.sql`, funkcija `fnSobaStatus`

Pravilo:

Soba nema samo ručno postavljen status. Status se izvodi iz:

- aktivnih redova u `relgostsoba`
- da li je red rezervacija ili stvarni boravak
- da li je rezervacija potvrđena
- da li je trenutni datum prije ili poslije `checkOutDate`
- da li je soba `ooo`

Legacy statusi:

| Kod | Značenje |
|---:|---|
| 0 | slobodna |
| 1 | zauzeta |
| 2 | zauzeta, odlazak danas ili prekoračen odlazak |
| 3 | rezervisana i potvrđena |
| 4 | rezervisana i zauzeta |
| 5 | van upotrebe |
| 6 | rezervisana i nije potvrđena |

Migracioni zahtjev:

- Novi sistem mora imati `RoomOccupancyPolicy`, ne samo `Room.Status`.
- UI može prikazivati izvedeni status i odvojeno tehnički status sobe.
- Testovi moraju pokriti svih 7 statusa.

Raskorak u novom kodu:

- `RoomStatusTransitions` nema statuse "confirmed reservation", "unconfirmed reservation", "reserved and occupied" i "due out/overdue".

## 4. RULE-STAY-001: Check-in Otvara Boravak I Folio

Legacy lokacije:

- `frmPrijava1.vb`
- procedura `addRelGostSoba`
- tabele `relgostsoba`, `posjetafolio`, `nocenja`

Pravilo:

Check-in u legacy sistemu nije samo promjena statusa rezervacije. On upisuje vezu gost-soba (`relgostsoba`) sa:

- gost
- soba
- check-in datum
- check-out datum
- radnik
- tarifa
- rezervacija/prijava flag
- grupa
- PID/folio
- popust
- napomena/usluga

Migracioni zahtjev:

- Novi `CheckInService` mora kreirati ili povezati `Stay/GuestRoomAssignment` i `Folio`.
- Mora čuvati legacy ID mapping za `relgostsoba.ID` i `PID`.
- Mora podržati više gostiju u istoj sobi i grupne prijave.

## 5. RULE-NIGHT-001: Noćenja Su Ledger, Ne Samo Izračun

Legacy lokacije:

- `ModuleKod.vb`, procedura `Unesinocenja`
- `frmPrijava1.vb`, pozivi `Unesinocenja`
- `frmSobaInfo.vb`, `dodajnocenja`
- tabela `nocenja`

Pravilo:

Procedura `Unesinocenja` briše postojeće noćenje za isti `RID` i datum, pa insertuje novo noćenje sa tarifom, sobom, folio PID-om, opisom i popustom.

Poslovno značenje:

- noćenje je materializovan finansijski događaj
- može biti popravljeno/reupisano za isti dan
- vezano je za `RID`, `SID`, `PID`
- ima vlastiti popust i opis

Migracioni zahtjev:

- `NightLedgerService` mora biti posebna komponenta.
- Račun ne smije izmišljati noćenja samo iz `ArrivalDate/DepartureDate`.
- Night audit mora biti idempotentan po stay/date.

## 6. RULE-CHECKOUT-001: Odjava Zatvara Više Poslovnih Stanja

Legacy lokacije:

- `Data.vb`, `OdjavaSobe`
- `frmOdjava1.vb`, pozivi `OdjavaSobe` i `PrljavaSoba`

Pravilo:

Odjava sobe radi transakcijski skup promjena:

- zaključuje aktivna noćenja (`PrijavaOdjava = 1`)
- upisuje dodatna noćenja za preostale goste ako se odjavljuje samo dio sobe
- označava `relgostsoba` kao odjavljen
- zapisuje `checkOutDate`, `checkOutRadnik`, `pl`
- zaključuje `posjetaFolio`
- zaključuje otvorene `troskovi`
- sobu označava prljavom (`sobe.clean = 0`)

Migracioni zahtjev:

- `CheckOutService` mora biti transakcijski workflow.
- Mora podržati djelimičnu odjavu gosta iz sobe.
- Mora znati razliku između odjave sobe i odjave pojedinačnog gosta.
- Mora zatvoriti folio i ledger pravilno, ne samo booking status.

Raskorak u novom kodu:

- Novi `CheckOutService` računa stay charges direktno iz booking dates i room price.
- Ne vidi se kompletna logika djelimične odjave, `PID`, `RID`, `PrijavaOdjava`, `pl`, niti povrat stanja za preostale goste.

## 7. RULE-FOLIO-001: Folio Obuhvata Noćenja, Troškove I Uplate

Legacy lokacije:

- `Data.vb`, `pripremaRcuna`, `gettroskovi`, `nocenjeSo`
- `frmPlacanje.vb`
- tabele `posjetafolio`, `nocenja`, `troskovi`, `placanjedetalji`

Pravilo:

Priprema računa čita:

- aktivne goste/sobu iz `relgostsoba`
- otvorene troškove `troskovi.zaklj = 0`
- već plaćena noćenja iz `placanjedetalji` gdje `art = 1` i `storno = 0`
- noćenja iz `nocenja`

Migracioni zahtjev:

- Folio mora biti ledger agregat, ne samo balance polje.
- Mora razlikovati noćenje, uslugu, uplatu, avans, storno i fiskalnu vezu.

## 8. RULE-INVOICE-001: Račun Je Snapshot Sa Detaljima, Footerom I Fiskalnim Vezama

Legacy lokacije:

- `frmPlacanje.vb`
- `frmRacuni.vb`
- tabele `printracuni`, `printracunidetalji`, `printracunifooter`, `placanje`, `placanjedetalji`

Pravilo:

Račun u legacy sistemu čuva:

- broj računa
- poslovnu/oznaku
- ime gosta/firme
- period
- tip plaćanja
- broj sobe
- storno status
- datum
- napomene
- fiskalni broj/vrijeme/iznos
- detalje sa PDV, cijenom bez PDV, iznosom PDV, popustom i načinom plaćanja
- footer za napomene i avans/noćenja

Migracioni zahtjev:

- Invoice mora biti immutable snapshot nakon izdavanja.
- Stavke računa moraju čuvati PDV breakdown.
- Hotel/firma podaci ne smiju biti hardkodirani.
- Fiskalni podaci moraju biti dio workflow-a.

Raskorak u novom kodu:

- Novi `InvoiceGenerator` hardkodira PDV 25%, hardkodira hotel VAT, koristi `FolioId = Guid.Empty` i ne prenosi legacy snapshot model.

## 9. RULE-STORNO-001: Storno Vraća Operativno Stanje

Legacy lokacija:

- `frmRacuni.vb`

Pravilo:

Storno računa ne znači samo negativan račun. Legacy radi:

- `placanje.storno = 1`
- `placanjeDetalji.storno = 1`
- `printracuni.storno = 1`
- `printracuni.exp = 2`
- `printracuni.datstor = now`
- `troskovi.zaklj = 0`
- `troskovi.Brrac = null`
- briše određene noćenje stavke/troškove za `TID = 1`
- opciono stornira fiskalni račun na uređaju

Migracioni zahtjev:

- Storno mora biti workflow sa reversal side effects.
- Mora postojati audit trail i razlog.
- Fiskalni storno mora biti dio istog poslovnog slučaja, ali retry-safe.

## 10. RULE-FISCAL-001: Fiskalizacija Je Driver Workflow

Legacy lokacije:

- `frmRacuni.vb`
- `frmFiskal.vb`
- `Tring.Fiscal.Driver.dll`
- tabela `fisc`

Pravilo:

Legacy koristi `Tring.Fiscal.Driver.TringFiskalniPrinter`, artikle, poreske stope, vrste plaćanja, kupca i odgovor kase.

Migracioni zahtjev:

- Novi bridge mora podržati stvarne fiskalne drivere.
- Fiskalni odgovor mora biti sačuvan na računu.
- Neuspjela fiskalizacija mora imati retry/queue i ne smije izgubiti poslovni račun.

Raskorak u novom kodu:

- `BridgeService` je mock i generiše lažni JIR-like string.

## 11. RULE-RESERVATION-001: Rezervacije Imaju Potvrdu, Storno, Grupni Blok I Izvor

Legacy lokacije:

- `frmRezervacije_unos.vb`
- `frmRezervacijePregled.vb`
- tabele `rezervacije`, `rezervacijegrupe`, `rezervacijeizvor`, `rezervacijetip`
- procedure pregleda rezervacija u dumpu

Pravilo:

Rezervacije sadrže:

- gost
- datum check-in/check-out
- vrsta sobe
- broj rezervisanih soba
- tarifa
- potvrda
- broj potvrde
- storno i broj storna
- prijava flag
- grupa/blok
- izvor
- tip
- memo

Pregledi koriste interval-overlap logiku i odvajaju:

- sve aktivne
- dolaske na dan
- potvrđene
- stornirane

Migracioni zahtjev:

- Availability i reservation search moraju pokriti iste interval-overlap scenarije.
- Potvrda/storno/prijava nisu samo status string, nego imaju brojeve i audit.

## 12. RULE-ROOM-TRANSFER-001: Prebacivanje Sobe Može Prenijeti Goste, Troškove I Noćenja

Legacy lokacija:

- `frmSobaInfo.vb`

Pravilo:

Legacy podržava prebacivanje:

- svih gostiju sa svim troškovima
- jednog gosta
- samo sobe
- troškova
- noćenja
- promjenu `PID`
- zatvaranje starog folia i otvaranje novog

Migracioni zahtjev:

- Novi sistem mora imati `RoomTransferService`.
- Transfer mora biti ledger-safe i ostaviti audit trail.

## 13. Migraciona Hipoteza Za Novi Kod

Postojeći novi kod može ostati tehnička osnova, ali P0 poslovne servise treba preoblikovati oko legacy modela:

- `BookingService` može ostati, ali treba `ReservationPolicyService`
- `CheckInService` treba proširiti u `StayLifecycleService`
- `CheckOutService` treba značajan rewrite
- `FolioService` treba postati ledger-based
- `InvoiceGenerator` treba zamijeniti `InvoiceWorkflowService`
- `BridgeService` mora biti realan ili jasno ostati demo-only
- `LegacyMigrator` mora dobiti punu migraciju P0 tabela

## 14. RULE-CHECKOUT-002: Djelimicna Odjava Nije Ista Kao Odjava Sobe

Legacy lokacije:

- `legacy_app/Radna/Data.vb`, `OdjavaSobe(izn, gid, sid, pids, checkOutDate, uplatanocenja)`
- `legacy_app/Radna/frmOdjava1.vb`, pozivi odjave

Pravilo:

`OdjavaSobe` ima dvije grane:

- `gid = 0`: odjava cijele sobe/folia
- `gid <> 0`: odjava pojedinacnog gosta/reda, uz reinsert otvorenih nocenja za preostale goste

Side effects:

- `nocenja.PrijavaOdjava = 1`
- `nocenja.datumodj = checkOutDate`
- za preostale goste insertuje novo otvoreno nocenje na `checkOutDate`
- `relgostsoba.odjavljen = 1`
- `relgostsoba.checkOutDate`, `checkOutRadnik`, `pl`
- kod pune odjave zatvara `posjetaFolio`
- kod pune odjave zakljucava otvorene `troskovi`

Raskorak u novom kodu:

- `backend/src/HotelPro.Infrastructure/Services/CheckOutService.cs` radi na `Booking` i prvom `BookingRoom`.
- Nema model `RID/gid`, `PID`, djelimicnu odjavu, reinsert nocenja za preostale goste, niti zakljucavanje/otkljucavanje legacy tipa.

Migracioni zahtjev:

- Uvesti `StayLifecycleService` ili `CheckOutWorkflowService` koji radi nad `Stay/RoomAssignment/Folio/NightLedger`.
- Testirati najmanje: puna odjava sobe, odjava jednog gosta iz sobe sa dva gosta, odjava sa placenim nocenjima, odjava bez uplata.

## 15. RULE-FOLIO-002: Balance Polje Nije Dovoljno Za Folio

Legacy lokacije:

- `legacy_app/Radna/Data.vb`, `pripremaRcuna`
- `legacy_app/Radna/Data.vb`, `nocenjeSo`
- `legacy_app/Radna/Data.vb`, `gettroskovi`
- `legacy_app/Radna/frmPlacanje.vb`

Pravilo:

Folio za racun se ne dobija iz jednog balance polja. Legacy ga sklapa iz:

- aktivnih gostiju u sobi iz `relgostsoba`
- otvorenih troskova `troskovi.zaklj = 0`
- uplata/nocenja u `placanjedetalji` gdje je `art = 1` i `storno = 0`
- materializovanih `nocenja`

Raskorak u novom kodu:

- `FolioService` cuva `Folio.Balance` i dodaje `Charge`, ali nema ekvivalent `placanjedetalji.art`, `troskovi.zaklj`, `Brrac`, niti pripremu racuna kao snapshot.
- `StayNight` ima samo `FolioId`, `Date`, `RoomPrice`, `IsComp`, `Notes`; nema legacy `RID`, `SID`, `PID`, `PrijavaOdjava`, `Tarifa`, `popust`, `opis`, `soba`.

Migracioni zahtjev:

- Folio treba biti ledger agregat sa zasebnim linijama za nocenja, troskove, uplate, avanse, storno i fiskalne reference.
- `Balance` smije biti izveden ili cached, ali ne smije biti jedini izvor istine.

## 16. RULE-STORNO-002: Storno Racuna Otkljucava Troskove I Brise Nocenje-Trosak

Legacy lokacije:

- `legacy_app/Radna/frmRacuni.vb`, storno blok oko update-a `placanje`, `placanjeDetalji`, `printracuni`, `troskovi`

Pravilo:

Storno racuna:

- oznaci `placanje.storno = 1`
- oznaci `placanjeDetalji.storno = 1`
- oznaci `printracuni.storno = 1`, `exp = 2`, `datstor = Now`
- vrati nenocenje troskove u otvoreno stanje: `troskovi.zaklj = 0`, `Brrac = null` gdje `TID <> 1`
- brise troskove racuna gdje je `TID = 1`
- zatim pokrece fiskalni storno zavisno od konfiguracije uredjaja

Raskorak u novom kodu:

- `InvoiceGenerator.StornoInvoiceAsync` kreira negativan invoice i oznaci original kao cancelled.
- Ne vraca otvorene troskove, ne dira payment detalje, ne razlikuje `TID = 1`, nema fiskalni storno workflow.

Migracioni zahtjev:

- `InvoiceReversalService` mora biti operativni workflow, ne samo dokument.
- Mora vratiti folio/troskove u stanje iz kojeg se moze ponovo izdati ispravan racun.

## 17. RULE-PAYMENT-001: Slozeno Placanje Je Stavka Po Nacinu Placanja

Legacy lokacije:

- `legacy_app/Radna/frmPlacanje.vb`, `placanje_slozeno`
- SQL procedura `addPlacanjeSlozeno`

Pravilo:

Jedan racun moze imati vise nacina placanja. Legacy iterira `ds.Tables("PlacanjaSlozena")` i za svaki red poziva `addPlacanjeSlozeno` sa:

- rednim brojem racuna/placanja
- nacinom placanja
- iznosom

Raskorak u novom kodu:

- `CheckOutService` kreira jedan `Payment` sa `PaymentMethod = request.PaymentMethod`.
- Nema listu payment split stavki niti ravnanje ukupnog iznosa po nacinima placanja.

Migracioni zahtjev:

- `Payment` treba header ili transaction, a `PaymentAllocation/PaymentDetail` treba cuvati split po nacinima placanja.
- Checkout mora validirati da zbir split placanja odgovara ukupnom racunu.

## 18. RULE-RESERVATION-002: Rezervacija Koristi Header/Detail I Status Kodove

Legacy lokacije:

- `legacy_app/Radna/frmRezervacije_unos.vb`
- tabela `rezervacijeh`
- tabela `rezervadeth`

Pravilo:

Legacy rezervacija ima header/detail model:

- header `rezervacijeh`: broj, period `odd/doo`, broj soba, naziv/opis, kontakt, telefon, email, firma/agencija, odrasli/djeca/bebe, ukupno, depozit, status
- detail `rezervadeth`: soba, tip/atributi, cijena, gost/ime, logical delete `del`

Status iz posmatranog koda:

- `0`: aktivno/nepotvrdjeno ili vraceno iz potvrde/storna
- `1`: potvrdjeno
- `5`: stornirano

Raskorak u novom kodu:

- Novi booking model postoji, ali mora se dokazati da cuva header/detail semantiku, logical delete stavki, status potvrde/storna i depozit.

Migracioni zahtjev:

- Reservation modul mora imati `ReservationHeader` i `ReservationRoomLine` semantiku ili dokazivo ekvivalentan model.
- Potvrda i storno moraju imati audit podatke, ne samo promjenu status stringa.

## 19. RULE-GUEST-001: Gost Nije Samo CRM Profil

Legacy lokacije:

- `legacy_app/Radna/frmPrijavaGostiUnos.vb`, insert/update u `gosti`
- `legacy_app/Radna/frmGosti.vb`, pregledi i turisticka/evidencijska prijava
- tabele `gosti`, `gostdokument`, `drzave`, `relgostsoba`

Pravilo:

Guest podaci u legacy sistemu nisu samo kontakt profil. Koriste se za:

- prijavu gosta u sobu
- dokument i broj dokumenta
- drzavljanstvo i drzavu rodjenja
- pol, datum rodjenja i mjesto rodjenja
- knjigu stranaca / turisticku prijavu
- izvjestaje po boravku preko `relgostsoba`

Raskorak u novom kodu:

- `GuestService` ima bolji GDPR model i `GuestDocument`, ali treba dokazati mapiranje legacy polja `DID`, `dokument`, `brDokument`, `mjestodrzavaR`, `drzavljanstvo` i veze sa turistickom prijavom.
- `GuestProfile` racuna historiju iz `GuestStayHistory`/`Booking`, dok legacy izvjestaji vuku boravak iz `relgostsoba` i `nocenja`.

Migracioni zahtjev:

- Data migracija gostiju mora imati mapiranje za dokumente i drzave, ne samo ime/prezime/email.
- Turisticka prijava mora se tretirati kao zaseban P1/P0 regulatorni tok, ne kao obican report.

## 20. Sljedece Za Ekstrakciju

- Detaljno razložiti `frmPlacanje.vb` tok izdavanja računa.
- Detaljno razložiti `frmRacuni.vb` tok storna i fiskalizacije.
- Detaljno razložiti `frmPrijava1.vb` check-in.
- Detaljno razložiti `frmRezervacije_unos.vb` unos/izmjenu/storno rezervacije.
- Izvući ER mapu legacy tabela u `LEGACY_TO_MODERN_DATA_MAPPING.md`.
