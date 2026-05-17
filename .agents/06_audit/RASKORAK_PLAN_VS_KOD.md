# RASKORAK PLAN VS KOD - HotelPRO

Status: WORKING AUDIT
Datum: 2026-05-17
Agent: Codex
Restore point: f984b8f (`checkpoint: before codex edit 2026-05-17T14-55-00`)

## 1. Executive Summary

Ovaj audit provjerava da li je plan iz `.agents` stvarno implementiran u kodu. Primarni dokaz nije status checklist nego postojeći kod, testovi i konkretni FSD/task zahtjevi.

Preliminarni nalaz: aplikacija nije kompletno implementirana prema izvornom planu. Postoji znatan dio korisne osnove za backend, frontend i core PMS module, ali status dokumentacija preuveličava gotovost. Više faza označenih kao završene sadrže mock/stub endpoint-e, statične odgovore ili samo API površinu bez stvarne poslovne/integracijske logike.

Najveći sistemski propust: agenti su u velikoj mjeri napravili moderni CRUD/API scaffold, ali nije dokazano da je kompletna poslovna logika operativne legacy aplikacije sistematski prenesena iz izvornog koda.

## 2. Metod Audita

Za svaki modul koristi se isti lanac dokaza:

1. FSD zahtjev
2. task Definition of Done
3. postojeći kod
4. testovi/verifikacija
5. status raskoraka
6. preporučena akcija

Prioritet izvora:

1. stvarni kod u `backend/`, `frontend/`, `hardware_bridge/`, `iot_services/`, `infrastructure/`
2. `.agents/STATUS.md`
3. task fajlovi u `.agents/04_tasks/`
4. FSD dokumenti u `.agents/03_specs/fsd/`
5. legacy kod/baza samo kao referenca za poslovnu logiku

## 3. Status Dokumentacije

| Nalaz | Dokaz | Uticaj | Prioritet |
|---|---|---:|---:|
| `STATUS.md` tvrdi da su sve faze kompletirane, ali Faza 11 ima otvorene stavke. | `.agents/STATUS.md` header i redovi T11.1-T11.4 | Status nije pouzdan kao izvor istine. | P0 |
| Task fajlovi nisu sinhronizovani sa `STATUS.md`. | `82` task fajla imaju `PENDING`, samo `6` imaju `COMPLETED`. | Nije moguće automatski vjerovati planu bez ručnog audita. | P0 |
| README tvrdi v1.0 i sve faze kompletirane. | `README.md` status/faze | Korisnik i agenti dobijaju pogrešan signal gotovosti. | P1 |

## 4. Raskoraci Po Modulima

Legenda:

- `OK`: dokazano implementirano u prihvatljivoj mjeri
- `PARTIAL`: postoji dio funkcionalnosti, ali ne pokriva FSD/DoD
- `MOCK`: postoji API/UI površina, ali nema stvarne implementacije
- `MISSING`: nema relevantnog koda
- `UNKNOWN`: treba dublji legacy/code audit

| Faza | Modul | Stvarni status | Sažetak raskoraka | Prioritet |
|---:|---|---|---|---:|
| 1 | Infrastruktura / DevOps | PARTIAL | PostgreSQL i backup postoje, ali nema dokaza za kompletan deployment/CI opseg. | P1 |
| 2 | Backend Foundation | PARTIAL | .NET/EF/API osnova postoji, ali legacy migrator i dio servisa zahtijevaju dublju provjeru poslovnog mapiranja. | P1 |
| 3 | Auth / RBAC / Multi-tenant | PARTIAL | JWT/RBAC postoje, ali treba provjeriti tenant izolaciju kroz sve query-je i endpoint-e. | P1 |
| 4 | Frontend Foundation | PARTIAL | Next/Tailwind layout postoji, ali frontend lint trenutno nije izvršiv bez lokalnih dependency-ja. | P2 |
| 5 | Sobe i kapaciteti | PARTIAL | CRUD i statusi postoje, ali treba provjeriti kompletnu legacy `fnSobaStatus` logiku i edge case-ove. | P1 |
| 6 | Rezervacije | PARTIAL | Booking servis i testovi postoje, ali treba uporediti sa legacy pravilima rezervacija, grupa i kolizija. | P1 |
| 7 | Recepcija / Folio | PARTIAL | Check-in/out, folio i night audit postoje, ali treba dokazati obračun i tokove protiv legacy logike. | P0 |
| 8 | Gosti / CRM | PARTIAL | CRUD/pretraga/GDPR osnovno postoje, ali legacy document/partner/TZ tokovi nisu dokazani. | P1 |
| 9 | Naplata / Fakturisanje | PARTIAL | Invoice/folio osnova postoji, ali fiskalizacija, storno i multi-VAT moraju se provjeriti protiv FSD-a. | P0 |
| 10 | Izvještaji | PARTIAL | Postoje report endpoint-i, ali nije dokazano da odgovaraju Crystal/legacy izvještajima. | P1 |
| 11 | Housekeeping | PARTIAL | Kod postoji, ali `STATUS.md` sam kaže da su T11.1-T11.4 otvoreni; PWA/offline/notifikacije nisu dokazane. | P0 |
| 12 | Hardware Bridge | MOCK | Bridge radi u mock modu; `hardware_bridge/` nema stvarni servis. | P0 |
| 13 | Channel Manager | MOCK | Booking/Airbnb/availability/rate sync endpoint-i vraćaju mock odgovore. | P0 |
| 14 | IoT | MOCK/MISSING | IoT endpoint-i vraćaju statične/mock podatke; `iot_services/` nema stvarnu implementaciju. | P0 |
| 15 | Revenue Management | MOCK/PARTIAL | Postoji površina za revenue, ali competitor/rules engine nisu dokazani kao stvarna logika. | P1 |
| 16 | Guest Self-Service | MOCK | Guest portal vraća statične/demo odgovore i ne veže se na puni booking/payment/lock tok. | P0 |
| 17 | Payment Gateway | MOCK | Stripe nije integrisan; tokenizacija generiše lokalni GUID token iz poslanog broja kartice. | P0 |
| 18 | Stabilizacija / Release | PARTIAL/MISSING | Backend testovi prolaze, ali security/performance/PCI/GDPR/release tvrdnje nisu dokazima pokrivene. | P0 |

## 5. Kritični Dokazi Raskoraka

### 5.1 Mock integracije označene kao završene

- `ChannelManagerController` vraća `status = "mock"` za Booking.com, Airbnb, availability i rate sync.
- `IoTController` vraća `mqttBroker = "disconnected"`, brojeve uređaja `0` i `status = "mock_success"`.
- `PaymentGatewayController` generiše transakcije i tokene lokalno preko `Guid.NewGuid`, bez Stripe klijenta/webhook validacije.
- `GuestPortalController` vraća statične iznose, demo meni i generisani `hotelpro.app/key/...` URL.
- `BridgeService` vraća `HardwareMode = "Mock"` i simulira fiskalizaciju/RFID/PABX.

### 5.2 Poslovna logika nije dokazano prenesena iz legacy sistema

Postoje moderni entiteti, DTO-i, kontroleri i servisi, ali to nije dovoljan dokaz da je legacy aplikacija reverzno inženjerisana. Posebno treba ručno dokazati:

- obračun noćenja i late check-out
- folio po sobi/gostu/grupi
- storno i korekcije računa
- fiskalizacijski tok
- provjera zauzeća i double-booking edge case-ovi
- grupne rezervacije i master račun
- status sobe prema legacy `fnSobaStatus` logici
- turistička evidencija i knjiga stranih državljana
- Crystal report metričke definicije
- hardverski protokoli za fiskalne uređaje, RFID i PABX

## 6. Verifikacija Izvršena Tokom Audita

| Provjera | Rezultat | Napomena |
|---|---|---|
| `git status --short` prije audita | clean | Repo čist prije dokumentacijskih izmjena. |
| Restore point | created | Commit `f984b8f`. |
| `dotnet test backend\HotelPro.sln` | passed | 57/57 testova prolazi, uz warninge. |
| `npm.cmd run lint` | failed to run | `eslint` nije pronađen; vjerovatno nije instaliran `node_modules`. |

## 7. Preporučeni Plan Oporavka

1. Zamrznuti tvrdnju "ALL PHASES COMPLETED" dok audit ne završi.
2. Ne popravljati nasumično. Prvo završiti matricu `FSD -> task -> kod -> test`.
3. Prioritetno auditirati core PMS poslovnu logiku: Faze 5-9.
4. Fazu 11 završiti jer je otvorena i u samom `STATUS.md`.
5. Faze 12-17 tretirati kao nezavršene dok ne postoji stvarna integracija ili svjesna odluka da ostaju mock feature flags.
6. Tek nakon toga raditi Fazu 18: security, PCI/GDPR, performance i release.

## 8. Sljedeći Audit Koraci

- [x] Infra i deployment audit - prvi prolaz
- [x] Backend foundation i EF schema audit - prvi prolaz
- [x] Auth/RBAC/tenant isolation audit - prvi prolaz
- [x] Rooms/status state machine audit - prvi prolaz
- [x] Booking/reservation rules audit - prvi prolaz
- [x] Reception/folio/night audit audit - prvi prolaz
- [x] Guests/CRM/GDPR audit - prvi prolaz
- [x] Billing/invoicing/fiscal audit - prvi prolaz
- [x] Reports/statistics audit - prvi prolaz
- [x] Housekeeping/PWA/work orders audit - prvi prolaz
- [x] Hardware bridge audit - prvi prolaz
- [x] Channel manager audit - prvi prolaz
- [x] IoT audit - prvi prolaz
- [x] Revenue audit - prvi prolaz
- [x] Guest portal audit - prvi prolaz
- [x] Payments/PCI audit - prvi prolaz
- [x] Stabilization/release audit - prvi prolaz

## 9. Modul Po Modul - Detaljni Prvi Prolaz

### 9.1 Faza 1 - Infrastruktura / DevOps

Status: PARTIAL

Dokazi:

- `infrastructure/docker-compose.yml` ima PostgreSQL i backup container.
- `infrastructure/backup/restore.ps1` i `sync-to-s3.ps1` postoje.
- Nije nađen nginx reverse proxy, SSL konfiguracija, kompletan produkcijski deploy stack ili dokaz CI izvršavanja u ovom prolazu.

Raskorak:

- Plan i README sugerišu produkcijski spreman sistem, ali dokazani scope je primarno lokalno/dev okruženje sa backupom.
- Faza 18 dodatno tvrdi produkcijski deployment, ali infrastruktura ne pokriva cijeli taj opseg.

Preporuka:

- Razdvojiti "dev infrastructure" od "production deployment".
- Dodati deployment acceptance checklist: reverse proxy, TLS, secrets, migrations, backup restore test, healthcheck, observability.

### 9.2 Faza 2 - Backend Foundation / EF / Legacy Migrator

Status: PARTIAL

Dokazi:

- `backend/HotelPro.sln` i projekti postoje.
- EF DbContext sadrži veliki broj entiteta i konfiguracija.
- `LegacyMigrator.RunAsync` poziva migraciju više legacy cjelina, ali mnoge funkcije vraćaju `0`: gosti, partneri, izvori rezervacija, tipovi rezervacija, metode plaćanja, katalog usluga i radnici.

Raskorak:

- T2.14 je označen kao kompletiran, ali ETL nije kompletan. Ovo je kritično jer je korisnik dao operativni legacy kod i bazu kao izvor poslovne istine.
- Migracija trenutno pokriva samo dio šifarnika/soba; ne prenosi ključne operativne podatke hotela.

Preporuka:

- T2.14 vratiti na INCOMPLETE.
- Napraviti posebnu legacy mapping matricu za svaku tabelu: `legacy table -> target entity -> fields -> transform -> validation query`.
- Dodati test migracije na malom fixture dumpu.

### 9.3 Faza 3 - Auth / RBAC / Multi-tenant

Status: PARTIAL

Dokazi:

- JWT setup i authorization policies postoje u `Program.cs`.
- `TenantResolutionMiddleware` čita `X-Hotel-Code` ili subdomain.
- `HotelProDbContext` globalno filtrira entitete koji implementiraju `IHaveHotelId`.

Raskorak:

- Ako request nema hotel code, tenant filter vraća `null`, što može dovesti do praznih rezultata ili nedosljednog ponašanja zavisno od entiteta.
- Nije dokazano da svi poslovni entiteti koji moraju biti tenant-scoped implementiraju `IHaveHotelId`.
- Testovi ne pokrivaju tenant izolaciju kroz endpoint-e.

Preporuka:

- Napraviti tenant isolation audit: lista svih entiteta, da li imaju `HotelId`, da li su filtrirani.
- Dodati integration test: hotel A ne vidi podatke hotela B.

### 9.4 Faza 4 - Frontend Foundation

Status: PARTIAL

Dokazi:

- Next.js aplikacija, layout, sidebar, login, dashboard i help komponente postoje.
- `npm.cmd run lint` ne može proći jer `eslint` nije pronađen u lokalnom okruženju.

Raskorak:

- Dokumenti tvrde `npm run lint clean`, ali trenutna radna kopija to ne može dokazati bez dependency instalacije.
- Frontend postoji kao osnovna aplikacija, ali audit funkcionalne povezanosti sa backendom nije kompletiran.

Preporuka:

- Instalirati dependency-je i pokrenuti `npm run lint` i `npm run build`.
- Dodati minimalne browser smoke testove za login, dashboard i ključne module.

### 9.5 Faza 5 - Sobe i Kapaciteti

Status: PARTIAL

Dokazi:

- `RoomService` implementira CRUD, filtere i status update.
- `RoomStatusTransitions` sadrži osnovni state machine.
- `RoomsController` izlaže endpoint-e za sobe i status.

Raskorak:

- FSD pominje legacy `fnSobaStatus` logiku, čišćenje, SOS/fire alarm i access control. Prvi prolaz nije našao dokaz da je ta kompletna logika prenesena.
- `RoomService.UpdateRoomStatusAsync` kao hotel id za broadcast koristi `building?.Id`, što izgleda kao pogrešna semantika za `hotelId`.

Preporuka:

- Iz legacy koda izdvojiti `fnSobaStatus` i napraviti test matricu statusa.
- Odvojiti room status, cleaning status, OOO/OOS i alarm/access statuse ako legacy sistem to razlikuje.

### 9.6 Faza 6 - Rezervacije

Status: PARTIAL

Dokazi:

- `BookingService` postoji i ima validaciju datuma, gostiju i osnovne status tranzicije.
- `BookingAvailabilityService` provjerava konflikte i koristi serializable transakcije za PostgreSQL.
- Testovi za booking i availability postoje i prolaze.

Raskorak:

- `ReleaseRoomLockAsync` je no-op (`Task.CompletedTask`), a lock ID-jevi su generisani stringovi bez stvarnog lock lifecycle-a.
- Provjera dostupnosti je značajno modernizovana i nije dokazano mapirana na legacy pravila rezervacija.
- Potrebno je provjeriti grupne rezervacije, pending status i partial room assignment protiv legacy procesa.

Preporuka:

- Zadržati postojeći booking servis kao dobru osnovu, ali ga ne proglašavati kompletno reverse-engineered.
- Napraviti acceptance testove iz legacy scenarija: pojedinačna rezervacija, grupa, promjena termina, otkaz, no-show, overbooking edge case.

### 9.7 Faza 7 - Recepcija / Check-in / Check-out / Folio

Status: PARTIAL, P0

Dokazi:

- `CheckInService` kreira folio, dodaje dokumente, mijenja booking/room status.
- `CheckOutService` računa boravak, late checkout fee, discount, payment i invoice.
- `NightAuditService` i testovi postoje.

Raskorak:

- `CheckOutService` koristi hardkodirana pravila popusta: grupa 10%, corporate 5%, loyalty 3%. Nije dokazano da ta pravila dolaze iz legacy aplikacije.
- `CheckOutService` kreira invoice broj preko `Guid.NewGuid()` i postavlja `TotalVat = 0`, što je problem za stvarno fakturisanje.
- RFID u check-in toku je samo URL string prema hardware endpointu, ne stvaran proces enkodiranja i potvrde.
- Folio logika postoji, ali treba dokazati da odgovara legacy razlikama: folio po sobi, po gostu, master račun, subfolio, storno i noćenja.

Preporuka:

- Ovo je jedan od najvažnijih modula za poslovnu logiku. Potrebno je napraviti legacy workflow rekonstrukciju prije daljeg kodiranja.
- Dodati testove za obračun noćenja, late checkout, split folio, master folio i zatvaranje dana.

### 9.8 Faza 8 - Gosti / CRM / GDPR

Status: PARTIAL

Dokazi:

- `GuestService` implementira CRUD, search, profile i privacy access log.
- MRZ parser postoji u Core servisima.
- GDPR consent se traži pri kreiranju gosta.

Raskorak:

- Legacy migrator ne migrira goste.
- Partneri/agencije nisu dokazano funkcionalni kroz puni workflow.
- Turistička evidencija koristi `ReportsService.GetGuestRegistrationBookAsync`, ali ima rizične null-forgiving izraze za dokumente i sobe.

Preporuka:

- Prioritetno implementirati migraciju gostiju, dokumenata, partnera i agencija.
- Napraviti izvještaj "knjiga stranih državljana" prema stvarnim lokalnim pravilima, ne samo generički query.

### 9.9 Faza 9 - Naplata / Fakturisanje / POS

Status: PARTIAL, P0

Dokazi:

- `InvoiceGenerator` postoji i koristi QuestPDF.
- `FolioService` podržava charge, delete charge, storno charge i close folio.
- `PosWebhookController` knjiži POS stavke na otvoreni folio.

Raskorak:

- `InvoiceGenerator` hardkodira PDV na `25m`, hardkodira PDF podatke hotela (`VAT: HR123456789`) i koristi `FolioId = Guid.Empty`.
- Check-out invoice ima `TotalVat = 0`.
- POS webhook koristi hardkodiran secret `shared-pos-secret`.
- Nije implementirana stvarna fiskalizacija; fiskalni bridge generiše mock JIR kod.

Preporuka:

- Fakturisanje tretirati kao nezavršeno za produkciju.
- Napraviti finansijski rule engine: PDV stope, sekvence računa, fiskalni status, storno pravila, valuta/kurs, avans, proforma -> invoice.
- POS secret izbaciti iz koda u konfiguraciju/secrets.

### 9.10 Faza 10 - Izvještaji i Statistika

Status: PARTIAL

Dokazi:

- `ReportsService` implementira daily, financial, guest book i revenue by channel endpoint-e.
- Dashboard endpoint-i postoje.

Raskorak:

- Nema dokaza da izvještaji odgovaraju legacy Crystal Reports definicijama.
- Guest book query koristi `FirstOrDefault()!` nad dokumentima i sobama, što može pucati ili dati neispravne podatke.
- Automatsko slanje izvještaja emailom nije dokazano kao realna funkcionalnost; `DailyReportJob` samo loguje occupancy.

Preporuka:

- Za svaki legacy izvještaj napraviti definiciju formule i SQL/EF ekvivalent.
- Dodati snapshot testove izvještaja na poznatim podacima.

### 9.11 Faza 11 - Housekeeping

Status: PARTIAL, P0

Dokazi:

- `HousekeepingController` ima endpoint-e za dirty rooms, clean, inspect i work-orders.
- Frontend `housekeeping/page.tsx` ima UI za dirty rooms i work orders.
- `STATUS.md` još drži T11.1 kao IN_PROGRESS i T11.2-T11.4 kao PENDING.

Raskorak:

- Task status i kod su kontradiktorni.
- FSD/task traže PWA/offline support i notifikacije prema recepciji; prvi prolaz nije našao dokaz toga.
- API nema dodjelu sobarice, `IN_PROGRESS` workflow, konflikt dodjele sobarice ili real-time recepcijske notifikacije.

Preporuka:

- Završiti Fazu 11 prije naprednih integracija.
- Razdvojiti cleaning lifecycle od generalnog room statusa ako je potrebno.
- Dodati SignalR event za završeno čišćenje i PWA/offline queue ako ostaje u scope-u.

### 9.12 Faza 12 - Hardware Bridge

Status: MOCK, P0

Dokazi:

- `BridgeService.GetStatus()` vraća `HardwareMode = "Mock"`.
- Fiskalizacija generiše lažni `JIR-...`.
- RFID encode vraća lažni `RFID-...`.
- PABX CDR import vraća `Mock mode - no CDR data`.
- `hardware_bridge/` nema stvarnu servisnu implementaciju u prvom prolazu.

Raskorak:

- Faza 12 je označena kao kompletirana, ali stvarni bridge ne postoji kao cross-platform servis sa driverima.
- Fiskalni printeri, RFID i PABX su simulirani, ne integrisani.

Preporuka:

- Donijeti arhitektonsku odluku: mock-only za demo ili pravi bridge servis.
- Ako pravi servis: prvo T12.1, zatim driver interface, persistent queue, retry, signed local API, healthcheck.

### 9.13 Faza 13 - Channel Manager

Status: MOCK, P0

Dokazi:

- `ChannelManagerController` vraća `connected = false` za kanale.
- Booking.com, Airbnb, availability push i rate sync endpoint-i vraćaju `status = "mock"`.
- Webhook events vraćaju prazan niz.

Raskorak:

- Nema stvarne Booking.com/Airbnb integracije, credential modela, webhook validacije, availability mappinga ili rate plan sync-a.

Preporuka:

- Vratiti Fazu 13 na nezavršeno.
- Implementirati provider abstraction i jedan kanal end-to-end prije širenja.

### 9.14 Faza 14 - IoT

Status: MOCK/MISSING, P0

Dokazi:

- `IoTController` vraća broker `disconnected`, uređaje `0`, statičnu temperaturu i `mock_success`.
- `IoTDeviceCheckJob` je prazan (`Task.CompletedTask`).
- `iot_services/` nema stvarnu servisnu implementaciju u prvom prolazu.
- Docker compose nema Mosquitto/MQTT servis.

Raskorak:

- FSD traži MQTT broker, topic strukturu, brave, senzore i energy dashboard. Stvarni kod to ne implementira.

Preporuka:

- Fazu 14 tretirati kao neimplementiranu osim API mocka.
- Početi od Mosquitto konfiguracije, ACL/TLS i stvarnog MQTT consumer servisa.

### 9.15 Faza 15 - Revenue Management

Status: MOCK/PARTIAL

Dokazi:

- `RevenueController` vraća statične pricing suggestions i random forecast.
- Competitor analysis vraća `not_configured`.
- Seasonal rule endpoint samo vraća `"saved"` bez perzistencije dokaza.

Raskorak:

- Nema stvarnog revenue engine-a, sezonskog modela, minimum stay pravila, competitor feeda ili učitanih pravila.

Preporuka:

- Ostaviti iza core PMS-a.
- Prvo modelirati rate plans i rule evaluation, zatim forecast.

### 9.16 Faza 16 - Guest Self-Service

Status: MOCK, P0

Dokazi:

- `GuestPortalController` nema `[Authorize]` ili guest token model u klasi.
- Online check-in/check-out vraćaju statične success odgovore.
- Digital key generiše demo URL.
- Room service menu je hardkodiran.

Raskorak:

- Nema stvarnog portal auth-a, booking validationa, payment/folio/IoT integracije ili narudžbe prema POS/folio toku.

Preporuka:

- Ne smije se smatrati gotovim.
- Dizajnirati guest-scoped token flow i jedan realni online check-in tok.

### 9.17 Faza 17 - Payment Gateway / PCI

Status: MOCK, P0

Dokazi:

- `PaymentGatewayController` generiše charge/refund/token preko `Guid.NewGuid`.
- `TokenizeRequest` prima `CardNumber` u API body.
- Nema Stripe SDK klijenta, PaymentIntent flow-a, webhook signature validationa ili transaction entiteta.

Raskorak:

- Ovo nije Stripe integracija i nije PCI-safe produkcijski flow.
- README/status tvrdnja o Stripe i PCI DSS je netačna u odnosu na kod.

Preporuka:

- Odmah označiti kao demo-only.
- Za stvarnu implementaciju: Stripe Elements/Checkout, server-side PaymentIntent, webhook signature validation, bez prolaska raw card broja kroz backend.

### 9.18 Faza 18 - Stabilizacija / Security / Release

Status: PARTIAL/MISSING, P0

Dokazi:

- Backend testovi prolaze: 57/57.
- `AdminController.SecurityAudit` vraća `owaspStatus = "not_scanned"` i `penetrationTest = "pending"`.
- `AdminController.PciStatus` vraća `compliance = "pending_audit"`.
- Performance endpoint vraća statične vrijednosti.

Raskorak:

- Security audit, penetration test, PCI audit i performance/load test nisu dokazano urađeni.
- Frontend lint/build nije potvrđen u trenutnom okruženju.

Preporuka:

- Fazu 18 vratiti na "not started / pending" dok se ne izvrše stvarni skenovi, load testovi i release procedure.

## 10. Najveći Propust - Poslovna Logika

Najveći propust je odsustvo dokazive, sistematske rekonstrukcije poslovne logike iz operativne legacy aplikacije.

Postoje tri različite stvari koje se trenutno miješaju:

1. Postoji moderan tehnički skeleton: kontroleri, entiteti, DTO-i, servisi i UI stranice.
2. Postoji dio poslovnih pravila napisan od nule: osnovne rezervacije, check-in/out, folio, night audit.
3. Ne postoji dovoljno dokaza da su ta pravila vjerno prenesena iz legacy aplikacije.

Za hotelsku aplikaciju to je kritično, jer poslovna vrijednost nije u CRUD-u nego u pravilima:

- kada je soba stvarno slobodna
- šta tačno ulazi u noćenje
- kada se generiše trošak
- kako se obračunava boravišna/turistička evidencija
- kako se radi storniranje
- kako se zaključuje dan
- šta se dešava s avansom, grupom, master računom i valutama
- kada se fiskalizuje i šta se čuva

Trenutni kod ima neke od tih tema, ali ih treba ponovo potvrditi protiv legacy izvora.

## 11. Preporučeni Redoslijed Popravke

1. Zaključati dokumentacijski status: prestati koristiti "ALL PHASES COMPLETED".
2. Napraviti `LEGACY_BUSINESS_LOGIC_MAP.md`: legacy forma/klasa/funkcija/tabela -> novi kod -> status -> test.
3. Reauditirati Faze 5-9 protiv legacy izvora, jer one nose stvarnu hotelsku operativu.
4. Završiti Fazu 11 jer je otvorena i u statusu.
5. Faze 12-17 označiti kao demo/mock dok se ne donese odluka o stvarnim integracijama.
6. Fazu 18 raditi tek poslije stvarnog core i integration završetka.

## 12. Minimalni Sljedeći Artefakti

Predlažem sljedeće dodatne dokumente prije velikih kod izmjena:

- `.agents/06_audit/LEGACY_BUSINESS_LOGIC_MAP.md`
- `.agents/06_audit/MODULE_ACCEPTANCE_MATRIX.md`
- `.agents/06_audit/P0_FIX_PLAN.md`

Ovaj dokument ostaje centralni nalaz raskoraka, a dodatni artefakti treba da budu operativni plan za popravke.

## 13. Dopuna Nakon Dubljeg Legacy Citanja - 2026-05-17

Ova dopuna ispravlja raniji rizik povrsnog zakljucka: nije pregledan svaki legacy fajl. Napravljen je poseban inventar pokrivenosti u `LEGACY_SOURCE_REVIEW_INVENTORY.md`.

### 13.1 Najveci poslovni raskorak

Najveci raskorak je da novi kod ima moderan booking/folio skeleton, ali nema dokazivo prenesen legacy operativni ledger.

Legacy P0 osovina:

- `relgostsoba` = stvarni boravak/gost-soba veza
- `PID/posjetaFolio` = operativni folio
- `nocenja` = materializovana nocenja i osnov za obracun
- `troskovi` = otvoreni/zakljucani troskovi vezani za racun
- `placanje` i `placanjedetalji` = naplata i stavke/split placanja
- `printracuni*` = fiskalno/print stanje racuna

Novi kod u P0 tokovima uglavnom radi iz:

- `Booking`
- `BookingRoom`
- `Folio.Balance`
- jedan `Payment`
- `Invoice`/PDF

To nije dovoljno da se aplikacija smatra poslovno migriranom.

### 13.2 Konkretni dokazi

| Tok | Legacy dokaz | Novi dokaz | Raskorak |
|---|---|---|---|
| Djelimicna odjava | `Data.vb`, `OdjavaSobe`, grana `gid <> 0` reinsertuje nocenja za preostale goste | `CheckOutService.cs` radi nad jednim bookingom i prvim roomom | Nema djelimicne odjave ni RID/PID semantike |
| Folio priprema | `Data.vb`, `pripremaRcuna`, cita `relgostsoba`, `troskovi`, `placanjedetalji`, `nocenja` | `FolioService.cs` koristi `Balance`, `Charges`, `StayNights` | Balance nije izvor istine u legacy modelu |
| Storno racuna | `frmRacuni.vb`, update `placanje`, `placanjeDetalji`, `printracuni`, `troskovi` | `InvoiceGenerator.StornoInvoiceAsync` kreira negativan invoice | Ne vraca troskove u otvoreno stanje i nema fiskalni storno workflow |
| Slozeno placanje | `frmPlacanje.vb`, `placanje_slozeno`, SQL `addPlacanjeSlozeno` | `CheckOutService.cs` kreira jedan `Payment` | Nema split placanja po nacinima |
| Rezervacije | `frmRezervacije_unos.vb`, `rezervacijeh` + `rezervadeth`, status 0/1/5 | `Booking` + `BookingRoom` | Treba dokazati header/detail, depozit, potvrdu/storno audit i logical delete stavki |

### 13.3 Posljedica za plan implementacije

Faze 5-9 ne treba tretirati kao zavrsene dok ne postoje testovi iz legacy scenarija za:

- 7 legacy statusa sobe iz `fnSobaStatus`
- check-in sa vise gostiju u istoj sobi i istim `PID`
- materializaciju nocenja kroz `Unesinocenja`
- punu i djelimicnu odjavu
- pripremu racuna iz `nocenja + troskovi + uplate`
- slozeno placanje
- storno koje vraca operativno stanje
- rezervaciju header/detail, potvrdu i storno
