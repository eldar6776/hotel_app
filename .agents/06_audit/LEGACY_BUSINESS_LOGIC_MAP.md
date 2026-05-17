# LEGACY BUSINESS LOGIC MAP - HotelPRO

Status: WORKING
Datum: 2026-05-17
Restore point: f984b8f

## Svrha

Ovaj dokument mapira poslovnu logiku iz operativne legacy aplikacije na novi kod. Cilj je odvojiti "postoji moderni kod" od "legacy poslovno pravilo je stvarno preneseno".

## Pravilo Dokazivanja

Funkcionalnost se ne smatra prenesenom dok ne postoje:

1. legacy izvor: forma, klasa, funkcija, SQL ili tabela
2. novi kod: servis/kontroler/entitet
3. test: unit, integration ili snapshot
4. rezultat: isti poslovni ishod na istom scenariju

## Kritična Mapa

| Poslovna oblast | Legacy izvor | Novi kod | Status | Rizik |
|---|---|---|---|---:|
| Status sobe / zauzeće | `fnSobaStatus` i povezani room workflow | `RoomService`, `RoomStatusTransitions` | PARTIAL - treba dokazati pravila | P0 |
| Rezervacija | legacy rezervacije forme/tabele | `BookingService`, `BookingAvailabilityService` | PARTIAL - moderan model postoji | P0 |
| Grupne rezervacije | legacy grupni tok | `BookingGroupService` | PARTIAL - treba scenarije | P1 |
| Check-in | legacy recepcijski workflow | `CheckInService` | PARTIAL - RFID je samo URL | P0 |
| Check-out | legacy obračun odlaska | `CheckOutService` | PARTIAL - hardkodirani popusti/PDV | P0 |
| Folio | legacy troškovi/noćenja | `FolioService`, `NightAuditService` | PARTIAL - treba dokazati pravila | P0 |
| Račun / fiskalizacija | legacy print/fiskalni tok | `InvoiceGenerator`, `BridgeService` | MOCK/PARTIAL | P0 |
| Storno | legacy storno/korekcije | `FolioService.StornoChargeAsync`, `InvoiceGenerator.StornoInvoiceAsync` | PARTIAL | P0 |
| Gosti/dokumenti | legacy gosti/dokumenti | `GuestService`, `MrzParser` | PARTIAL - ETL ne migrira goste | P1 |
| Partneri/agencije | legacy partneri | `Partner`, `SalesAgent` entities | UNKNOWN/PARTIAL | P1 |
| Turistička evidencija | legacy izvještaji/TZ | `ReportsService.GetGuestRegistrationBookAsync` | PARTIAL - rizičan query | P1 |
| Crystal izvještaji | legacy report definitions | `ReportsService`, dashboard | UNKNOWN/PARTIAL | P1 |
| POS knjiženje | legacy POS/folio tok | `PosWebhookController` | PARTIAL - hardkodiran secret | P1 |
| Fiskalni uređaji | legacy fiskalni wrapper | `BridgeService` | MOCK | P0 |
| RFID kartice | legacy card encoder | `BridgeService` | MOCK | P0 |
| PABX/CDR | legacy telefonija | `BridgeService`, `PhoneExtensionsController` | MOCK/PARTIAL | P1 |

## Prvi Zaključak

Novi kod ima tehničke module, ali ne postoji dovoljno dokaza da su legacy poslovna pravila prenesena sistematski. Najkritičniji kandidati za ponovnu rekonstrukciju su:

1. check-out obračun
2. folio/noćenja
3. račun/fiskalizacija/storno
4. status sobe i zauzeće
5. rezervacije i grupe

## Sljedeći Koraci

- Izvući stvarne legacy nazive formi, klasa i funkcija po modulu.
- Za svaki legacy scenario napisati očekivani ishod.
- Tek onda mijenjati novi kod i dodavati testove.
