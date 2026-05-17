# LEGACY SOURCE REVIEW INVENTORY

Status: WORKING
Datum: 2026-05-17
Scope: read-only inventar `legacy_app/Radna`
Restore point: `e7acf81 checkpoint: before codex edit 2026-05-17T15-35-00`

## 1. Pravilo iskrenog statusa

Ovaj dokument postoji zato sto se ne smije tvrditi da je pregledan "svaki fajl" dok to nije dokazano inventarom.

Statusi pregleda:

| Status | Znacenje |
|---|---|
| INVENTORIED | fajl je popisan i klasifikovan |
| TARGETED_READ | procitani su relevantni dijelovi za konkretan poslovni tok |
| DEEP_REVIEWED | fajl je procitan sistematski i pravila su izvadjena |
| COMPARED | pravila su uporedjena sa novim kodom |
| MIGRATION_SPEC_READY | postoje legacy dokaz, modern target, scenario i test zahtjev |

## 2. Popis izvora

Brojanje je uradjeno bez `Backup`, `bin`, `obj` i `.vs` foldera.

| Tip | Broj |
|---|---:|
| VB source fajlovi, bez Designer | 104 |
| VB Designer fajlovi | 100 |
| Report/code-behind fajlovi | 66 |
| Crystal `.rpt` fajlovi | 47 |
| SQL dump/script fajlovi | 3 |
| XML report/config fajlovi | 30 |

Zakljucak: do sada nije kompletno procitan svaki legacy fajl. Uradjen je inventar i ciljano citanje P0 poslovnih tokova.

## 3. Najveci legacy fajlovi po riziku

| Fajl | Velicina | Status |
|---|---:|---|
| `legacy_app/Radna/frmPlacanje.vb` | 318572 | TARGETED_READ |
| `legacy_app/Radna/frmGosti.vb` | 269008 | INVENTORIED |
| `legacy_app/Radna/frmRacuni.vb` | 262302 | TARGETED_READ |
| `legacy_app/Radna/ModuleKod.vb` | 230981 | TARGETED_READ |
| `legacy_app/Radna/frmGlavni.vb` | 198118 | INVENTORIED |
| `legacy_app/Radna/frmPrijavaGostiUnos.vb` | 137000 | INVENTORIED |
| `legacy_app/Radna/frmKardRw.vb` | 122643 | INVENTORIED |
| `legacy_app/Radna/frmSobaInfo.vb` | 105334 | TARGETED_READ |
| `legacy_app/Radna/frmBaza.vb` | 89210 | INVENTORIED |
| `legacy_app/Radna/frmpostavke.vb` | 86068 | INVENTORIED |
| `legacy_app/Radna/frmRezervacije_unos.vb` | 66608 | TARGETED_READ |
| `legacy_app/Radna/frmPrijava1.vb` | 60885 | TARGETED_READ |
| `legacy_app/Radna/frmOdjava1.vb` | 44431 | TARGETED_READ |

## 4. Do sada ciljano citani poslovni tokovi

| Tok | Legacy dokaz | Novi kod za poredjenje | Status |
|---|---|---|---|
| Status sobe | `ModuleKod.vb`, SQL `fnSobaStatus` | `RoomStatusTransitions.cs`, `RoomService.cs` | COMPARED |
| Check-in | `frmPrijava1.vb`, `addRelGostSoba`, `Unesinocenja` | `CheckInService.cs`, `FolioService.cs` | COMPARED |
| Check-out | `Data.vb`, `OdjavaSobe`, `PrljavaSoba` | `CheckOutService.cs` | COMPARED |
| Folio priprema | `Data.vb`, `pripremaRcuna`, `nocenjeSo`, `gettroskovi` | `FolioService.cs`, `StayNight.cs` | COMPARED |
| Placanje | `frmPlacanje.vb`, `placanje_slozeno`, `upisiAvans`, `snimi_trosak` | `CheckOutService.cs`, `Payment.cs`, `FolioService.cs` | TARGETED_READ |
| Racun/storno/fiskal | `frmRacuni.vb`, storno blok, `printfisc` | `InvoiceGenerator.cs` | COMPARED |
| Rezervacija | `frmRezervacije_unos.vb`, `rezervacijeh`, `rezervadeth`, status 0/1/5 | `BookingService.cs`, booking DTO/entity | TARGETED_READ |
| Gosti/dokumenti | `frmPrijavaGostiUnos.vb`, `frmGosti.vb`, `gosti`, `gostdokument`, `drzave` | `GuestService.cs`, `Guest.cs`, `GuestDocument` | TARGETED_READ |

## 5. SQL procedure/funkcije koje su P0

| SQL objekat | Linija u dumpu | Znacenje |
|---|---:|---|
| `fnSobaStatus` | 8457 | izvedeni status sobe |
| `addFolio` | 8502 | otvaranje folia |
| `addPlacanjeSlozeno` | 8517 | vise nacina placanja za jedan racun |
| `addRelGostSoba` | 8532 | check-in/veza gost-soba |
| `addTroskovi` | 8562 | dodavanje troska |
| `getSobeShema` | 8997 | sobna shema sa statusom, OOO i clean |
| `Unesinocenja` | 9222 | materializacija nocenja |
| `updateSobaClean` | 9282 | housekeeping clean state |
| `updateSobaOOO` | 9297 | out-of-order state |
| `vratiTrenutnoRezervisane` | 9372 | trenutno rezervisane sobe |
| `vratiTrenutnoSlobodne` | 9387 | trenutno slobodne sobe |
| `vratiTrenutnoZauzete` | 9402 | trenutno zauzete sobe |
| `vratiTrosakSoba` | 9417 | troskovi sobe |

## 6. Kriticna razlika nakon dubljeg citanja

Najveci propust nije samo da "neki taskovi nisu kompletirani". Najveci propust je da novi kod tretira hotelske tokove kao booking CRUD, dok legacy radi kroz operativni ledger:

- `relgostsoba` je stvarni boravak/gost-soba veza, ne samo `Booking`.
- `PID/posjetaFolio` je operativna folio osovina.
- `nocenja` su materializovani dogadjaji, ne samo rezultat `DepartureDate - ArrivalDate`.
- `troskovi` se zakljucavaju i otkljucavaju kroz racun/storno.
- `printracuni`, `placanje` i `placanjedetalji` su fiskalni/finansijski snapshot, ne samo PDF.
- `storno` vraca operativno stanje, ne samo kreira negativan dokument.

## 7. Redoslijed nastavka audita

1. Zatvoriti P0 finansije: `frmPlacanje.vb`, `frmRacuni.vb`, `Data.vb`, `ModuleKod.vb`.
2. Zatvoriti P0 lifecycle boravka: `frmPrijava1.vb`, `frmOdjava1.vb`, `frmSobaInfo.vb`.
3. Zatvoriti P0 rezervacije: `frmRezervacije_unos.vb`, pregledi i availability procedure.
4. Tek zatim ici na P1 goste, partnere, izvjestaje, turisticku prijavu, RFID, telefoniju i hardware.
