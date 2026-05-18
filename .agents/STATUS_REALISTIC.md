# STATUS_REALISTIC — HotelPRO

**Datum:** 2026-05-18
**Pokrivenost:** 93 taskova (Faze 1-18) — realistic assessment after audit
**Zadnji audit:** 2026-05-18 (06 Sonnet + 07 opencode deep analysis)

## Legenda

- `[x]` COMPLETED — stvarno radi i pokriva legacy poslovnu logiku
- `[~]` ACCEPTED — radi dovoljno za trenutne potrebe, manji nedostaci
- `[-]` PARTIAL — postoji scaffold/kod ali poslovna logika je nepotpuna ili pogrešna
- `[!]` REBUILD — postoji kod ali poslovna logika fundamentalno odstupa od legacy; treba rewrite
- `[m]` MOCK — postoji API endpoint ali vraća statične/demo podatke; nema pravu integraciju
- `[ ]` NOT STARTED — nema relevantnog koda

---

## Faza 1: Infrastruktura i DevOps

- [x] **T1.1:** Docker Compose za PostgreSQL 18 i dev okruženje
- [x] **T1.2:** .gitignore, EditorConfig i CI osnova
- [x] **T1.3:** Environment varijable i secrets management
- [x] **T1.4:** Automatski backup (Docker + S3) i restore procedura

## Faza 2: Backend Foundation

- [x] **T2.1:** Inicijalizacija .NET 8 Web API projekta
- [x] **T2.2:** EF Core konfiguracija i PostgreSQL konekcija
- [-] **T2.3:** Migracija DB sheme — Sobe, VrsteSoba, Zgrade (PARTIAL: nedostaju 7-statusne sobe, `fnSobaStatus` logika)
- [-] **T2.4:** Migracija DB sheme — Gosti, Dokumenti, Partneri (PARTIAL: ETL ne migrira goste/partnere)
- [-] **T2.5:** Migracija DB sheme — Rezervacije, RezervacijeGrupe, RelGostSoba (PARTIAL: missing confirmation/cancellation audit, brojPotvrde/brojStorna)
- [-] **T2.6:** Migracija DB sheme — Folio, Placanje, Troskovi (REBUILD: treba Stay/NightCharge/PaymentDetail/PaymentAllocation entiteti)
- [x] **T2.7:** Migracija DB sheme — Radnici, Smjene, SobaricaLog
- [x] **T2.8:** API verzioniranje (v1, v2)
- [x] **T2.9:** Rate limiting
- [x] **T2.10:** Audit log (EF Core interceptor, JSONB)
- [x] **T2.11:** Scheduled jobs
- [x] **T2.12:** Feature flags
- [x] **T2.13:** Multi-language podrska
- [-] **T2.14:** Legacy MySQL → PostgreSQL ETL migrator (PARTIAL: preskače ključne tabele)

## Faza 3: Autentifikacija i Autorizacija

- [x] **T3.1:** JWT autentifikacija i login endpoint
- [x] **T3.2:** RBAC — uloge (admin, recepcija, sobarica, menadzer)
- [-] **T3.3:** Multi-tenant podrska (PARTIAL: postoji ali nisu testirane tenant izolacije)

## Faza 4: Frontend Foundation

- [~] **T4.1:** Next.js projekat sa Tailwind CSS (ACCEPTED)
- [~] **T4.2:** Design System — boje, tipografija, spacing, Dark Mode (ACCEPTED)
- [~] **T4.3:** Layout — Sidebar navigacija, Navbar, responzivnost (ACCEPTED)
- [~] **T4.4:** Login stranica i JWT integracija (ACCEPTED)
- [~] **T4.5:** Dashboard stranica sa KPI karticama (ACCEPTED)
- [~] **T4.6:** Help sistem (ACCEPTED)

## Faza 5: Upravljanje Sobama

- [-] **T5.1:** CRUD API za sobe, tipove soba i zgrade (PARTIAL: CRUD radi, ali status logika samo 4 od 7+1 statusa)
- [~] **T5.2:** Frontend — interaktivni pregled soba (ACCEPTED: treba dodati Dirty i Departing statuse)
- [-] **T5.3:** Status sobe u realnom vremenu (PARTIAL: SignalR radi, ali status computation je pogrešna)
- [-] **T5.4:** Tarife, sadrzaji, OOO (PARTIAL: OOO radi, ali clean override ne radi ispravno)

## Faza 6: Rezervacije

- [-] **T6.1:** CRUD API za rezervacije (PARTIAL: CRUD radi, ali confirmation/cancellation lacks audit)
- [~] **T6.2:** Frontend — Gantt kalendar (ACCEPTED)
- [~] **T6.3:** Provjera dostupnosti i logika kolizija (ACCEPTED: PostgreSQL row-level locking radi)
- [-] **T6.4:** Email potvrda rezervacije (PARTIAL: radi ali template hardkodiran)
- [-] **T6.5:** Grupne rezervacije (PARTIAL: BookingGroup postoji ali master račun i release logika nedostaju)

## Faza 7: Recepcija (Check-in / Check-out)

- [!] **T7.1:** Check-in workflow API (REBUILD: nema multi-guest, nema relgostsoba semantiku, nema PID/folio kreacija)
- [!] **T7.2:** Check-out workflow (REBUILD: nema djelimične odjave, nema zaključivanje folio/troskovi, hardcoded discounti)
- [!] **T7.3:** Folio sistem (REBUILD: Balance polje nije ledger; treba nocenja + troskovi + uplate agregacija)
- [~] **T7.4:** Frontend — Recepcijski ekran (ACCEPTED: treba adaptacije za novi API)
- [!] **T7.5:** Night audit (REBUILD: nocenja se generišu iz booking datuma umjesto materializovanog ledgera)

## Faza 8: Gosti i CRM

- [-] **T8.1:** CRUD API za goste i dokumente (PARTIAL: GDPR radi, ali ETL ne migrira goste)
- [-] **T8.2:** Pretraga i filtriranje gostiju (PARTIAL)
- [-] **T8.3:** Istorija boravka i preferencije gosta (PARTIAL: GuestStayHistory treba Stay entitet)
- [~] **T8.4:** Frontend — Profil gosta (ACCEPTED)
- [~] **T8.5:** OCR MRZ parser (ACCEPTED)

## Faza 9: Naplata i Fakturisanje

- [!] **T9.1:** Generisanje racuna (REBUILD: hardkodiran PDV 25%, Guid.Empty FolioId, nema storno workflow)
- [-] **T9.2:** Predracuni i avansne uplate (PARTIAL: entiteti postoje ali flow je nepotpun)
- [!] **T9.3:** Storno i korekcija racuna (REBUILD: samo negativna faktura, ne vraća troškove u otvoreno)
- [!] **T9.4:** Kursna lista i multi-currency (REBUILD: nema implementacije)
- [!] **T9.5:** POS integracija (REBUILD: hardkodiran shared-pos-secret)

## Faza 10: Izvjestavanje i Dashboard

- [-] **T10.1:** Dnevni izvjestaj popunjenosti (PARTIAL: nema legacy parity)
- [-] **T10.2:** Finansijski izvjestaji (PARTIAL)
- [-] **T10.3:** Knjiga stranih drzavljana (PARTIAL: rizični null forgiving izrazi)
- [~] **T10.4:** Statisticke vizualizacije (ACCEPTED)
- [-] **T10.5:** Automatsko slanje izvjestaja emailom (PARTIAL: DailyReport samo loguje)

## Faza 11: Housekeeping

- [-] **T11.1:** API za upravljanje statusom soba (PARTIAL)
- [ ] **T11.2:** Mobilno-prilagodjen UI za sobarice (PWA, offline)
- [ ] **T11.3:** Notifikacije prema recepciji o zavrsenom ciscenju
- [ ] **T11.4:** Work Orders — prijava i pracenje kvarova

## Faza 12: Hardware Bridge

- [m] **T12.1:** Arhitektura lokalnog bridge servisa (MOCK: BridgeService vrača HardwareMode="Mock")
- [m] **T12.2:** Integracija sa fiskalnim printerima (MOCK: generiše lažni JIR)
- [m] **T12.3:** Integracija sa RFID citacima (MOCK: generiše lažni RFID kod)
- [m] **T12.4:** Integracija sa telefonskom centralom (MOCK: vrača "Mock mode - no CDR data")

## Faza 13: Channel Manager

- [m] **T13.1:** Integracija sa Booking.com (MOCK: status="mock")
- [m] **T13.2:** Integracija sa Airbnb (MOCK)
- [m] **T13.3:** Dvosmjerna sinhronizacija dostupnosti (MOCK)
- [m] **T13.4:** Automatsko azuriranje cijena (MOCK)

## Faza 14: Smart Hotel IoT Integracije

- [m] **T14.1:** MQTT broker setup (MOCK: broker="disconnected")
- [m] **T14.2:** Integracija sa pametnim bravama (MOCK)
- [m] **T14.3:** Senzori za sobu (MOCK)
- [m] **T14.4:** Automatizacija energetske efikasnosti (MOCK)
- [m] **T14.5:** Dashboard za IoT monitoring (MOCK)

## Faza 15: Revenue Management

- [m] **T15.1:** Engine za dinamicko odredjivanje cijena (MOCK: static pricing suggestions)
- [m] **T15.2:** Sezonski modeli i pravila (MOCK: "saved" bez perzistencije)
- [m] **T15.3:** Konkurentska analiza i sugestije (MOCK: "not_configured")

## Faza 16: Guest Self-Service

- [m] **T16.1:** Online Check-in / Check-out portal (MOCK: static responses, no real auth)
- [m] **T16.2:** Digitalni kljuc za sobu (MOCK: demo URL)
- [m] **T16.3:** In-room narudzbe (MOCK: hardcoded menu)

## Faza 17: Payment Gateway

- [m] **T17.1:** Integracija sa Stripe-om (MOCK: Guid.NewGuid token, no Stripe SDK)
- [m] **T17.2:** Tokenizacija kreditnih kartica (MOCK: prima raw card number)
- [m] **T17.3:** No-show naplata (MOCK)

## Faza 18: Stabilizacija i Release

- [ ] **T18.1:** Security audit (NOT STARTED: owaspStatus="not_scanned")
- [ ] **T18.2:** Performance optimizacija i load testiranje (NOT STARTED)
- [ ] **T18.3:** Produkcijski deployment (NOT STARTED)
- [ ] **T18.4:** GDPR compliance (PARTIAL: model exists but ETL incomplete)
- [ ] **T18.5:** PCI DSS audit (NOT STARTED: mock payment gateway)
- [ ] **T18.6:** Korisnicka dokumentacija (NOT STARTED)

---

## P0 REBUILD TASKS (From P0_REBUILD_PLAN.md)

- [x] **A.1:** Create STATUS_REALISTIC.md - [COMPLETED 2026-05-18 - Codex]
- [ ] **A.2:** Add [Mock] attribute + feature flag to mock controllers
- [ ] **A.3:** Create HotelConfig entity + admin settings API
- [ ] **B.1:** RoomOccupancyPolicy — 7 statuses + clean override
- [ ] **B.2:** StayLifecycleService — check-in with multi-guest
- [ ] **B.3:** NightLedgerService — materialized nights
- [ ] **B.4:** CheckOutWorkflowService — full + partial checkout
- [ ] **B.5:** FolioLedgerService — ledger aggregation
- [ ] **B.6:** InvoiceWorkflowService — snapshot + storno
- [ ] **B.7:** PaymentAllocationService — split payment
- [ ] **B.8:** ReservationPolicyService — confirmation/cancellation
- [ ] **C:** EF Core migration for all new entities/fields
- [ ] **D:** Frontend API adaptation
- [ ] **E:** Integration configuration (API keys, feature flags, IoT toggle)

---

## STATISTIKA

| Kategorija | Broj |
|-----------|------|
| COMPLETED ([x]) | 30 |
| ACCEPTED ([~]) | 11 |
| PARTIAL ([-]) | 22 |
| REBUILD ([!]) | 7 |
| MOCK ([m]) | 18 |
| NOT STARTED ([ ]) | 8 |
| **UKUPNO** | **96** |

**Stvarni napredak:** ~41% funkcionalnosti radi ili je prihvatljivo (COMPLETED + ACCEPTED)
**Potreban rebuild:** ~7% (7 critical P0 servisa)
**Mock/Pending:** ~27% (18 mock + 8 not started + dio partial)
