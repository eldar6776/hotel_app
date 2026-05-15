# STATUS — HotelPRO

**Trenutni Status:** Faza 0 COMPLETED — Planiranje i Arhitektura. Sljedeci: T1.1 PENDING.
**Datum:** 2026-05-15
**Pokrivenost:** 89 taskova (Faze 1-18), svi PENDING osim Faze 0
**Dokumentacija:** Svi FSD-ovi AUTHORITATIVE, task fajlovi sa legacy mapping sekcijama

---

## 1. PREGLED FAZA RADA

### Faza 0: Planiranje i Arhitektura
- [x] **T0.1: Analiza legacy koda i baze** - [COMPLETED] - 2026-05-15 - Antigravity (Claude Opus)
- [x] **T0.2: Reorganizacija foldera i .agents struktura** - [COMPLETED] - 2026-05-15 - Antigravity (Claude Opus)

### Faza 1: Infrastruktura i DevOps
- [-] **T1.1: Docker Compose za PostgreSQL 18 i dev okruzenje** - [IN_PROGRESS] - 2026-05-15 - opencode
- [ ] **T1.2: .gitignore, EditorConfig i CI osnova**
- [ ] **T1.3: Environment varijable i secrets management**
- [ ] **T1.4: Automatski backup (Docker + S3) i restore procedura**

### Faza 2: Backend Foundation
- [ ] **T2.1: Inicijalizacija .NET 8 Web API projekta**
- [ ] **T2.2: EF Core konfiguracija i PostgreSQL konekcija**
- [ ] **T2.3: Migracija DB sheme — Sobe, VrsteSoba, Zgrade**
- [ ] **T2.4: Migracija DB sheme — Gosti, GostDokument, Partneri**
- [ ] **T2.5: Migracija DB sheme — Rezervacije, RezervacijeGrupe, RelGostSoba**
- [ ] **T2.6: Migracija DB sheme — Folio, Placanje, Troskovi**
- [ ] **T2.7: Migracija DB sheme — Radnici, Smjene, SobaricaLog**
- [ ] **T2.8: API verzioniranje (v1, v2, header api-supported-versions)**
- [ ] **T2.9: Rate limiting (staff, guest, auth)**
- [ ] **T2.10: Audit log (EF Core interceptor, JSONB)**
- [ ] **T2.11: Scheduled jobs (No-Show, Night audit, Backup, IoT check)**
- [ ] **T2.12: Feature flags (per-hotel, postepeno uvodjenje)**
- [ ] **T2.13: Multi-language podrska (hr, en, de, it)**
- [ ] **T2.14: Legacy MySQL → PostgreSQL ETL migrator**

### Faza 3: Autentifikacija i Autorizacija
- [ ] **T3.1: JWT autentifikacija i login endpoint**
- [ ] **T3.2: RBAC — uloge (admin, recepcija, sobarica, menadzer)**
- [ ] **T3.3: Multi-tenant podrska (vise hotela, jedan sistem)**

### Faza 4: Frontend Foundation
- [ ] **T4.1: Inicijalizacija Next.js projekta sa Tailwind CSS**
- [ ] **T4.2: Design System — boje, tipografija, spacing, Dark Mode**
- [ ] **T4.3: Layout — Sidebar navigacija, Navbar, responzivnost**
- [ ] **T4.4: Login stranica i JWT integracija**
- [ ] **T4.5: Dashboard stranica sa KPI karticama**
- [ ] **T4.6: Interaktivni Help sistem (Context-Aware, Guided Tours)**

### Faza 5: Upravljanje Sobama
- [ ] **T5.1: CRUD API za sobe, tipove soba i zgrade**
- [ ] **T5.2: Frontend — interaktivni pregled soba (Floor Plan / Grid)**
- [ ] **T5.3: Status sobe u realnom vremenu (State Machine + SignalR)**
- [ ] **T5.4: Upravljanje tarifama, sadrzajima i OOO statusom**

### Faza 6: Rezervacije (Booking Engine)
- [ ] **T6.1: CRUD API za rezervacije (pojedinacne i grupne)**
- [ ] **T6.2: Frontend — interaktivni Drag & Drop Gantt kalendar**
- [ ] **T6.3: Provjera dostupnosti i logika kolizija (double-booking prevention)**
- [ ] **T6.4: Email potvrda rezervacije**
- [ ] **T6.5: Grupne rezervacije — blokiranje soba, master racun, posebni cjenovnici**

### Faza 7: Recepcija (Check-in / Check-out)
- [ ] **T7.1: Check-in workflow API (room assignment, RFID, dokumenti)**
- [ ] **T7.2: Check-out workflow sa obracunom (late check-out, payment)**
- [ ] **T7.3: Folio sistem — dodavanje troskova tokom boravka**
- [ ] **T7.4: Frontend — Recepcijski ekran (arrivals, departures, quick actions)**
- [ ] **T7.5: Night audit proces (automatsko generisanje nocenja)**

### Faza 8: Gosti i CRM
- [ ] **T8.1: CRUD API za goste i dokumente (GDPR, privacy logging)**
- [ ] **T8.2: Pretraga i filtriranje gostiju (multi-criteria, auto-suggest)**
- [ ] **T8.3: Istorija boravka i preferencije gosta (unified profile)**
- [ ] **T8.4: Frontend — Profil gosta (history, preferences, active stay)**
- [ ] **T8.5: OCR MRZ parser za dokumente (pasos, licna karta)**

### Faza 9: Naplata i Fakturisanje
- [ ] **T9.1: Generisanje racuna (QuestPDF, multi-VAT, invoice sequence)**
- [ ] **T9.2: Predracuni i avansne uplate**
- [ ] **T9.3: Storno i korekcija racuna (fiscal reversal, reprint)**
- [ ] **T9.4: Integracija sa kursnom listom (valute, multi-currency invoices)**
- [ ] **T9.5: POS integracija — automatsko knjizenje troskova restorana/bara na folio**

### Faza 10: Izvjestavanje i Dashboard
- [ ] **T10.1: Dnevni izvjestaj popunjenosti (ADR, RevPAR)**
- [ ] **T10.2: Finansijski izvjestaji (revenue by channel, turnover)**
- [ ] **T10.3: Knjiga stranih drzavljana (turisticka evidencija, TZ plugin)**
- [ ] **T10.4: Statisticke vizualizacije (Chart.js, trendovi)**
- [ ] **T10.5: Automatsko slanje izvjestaja emailom (scheduled, PDF)**

### Faza 11: Housekeeping
- [ ] **T11.1: API za upravljanje statusom soba (ciscenje, inspekcija)**
- [ ] **T11.2: Mobilno-prilagodjen UI za sobarice (PWA, offline support)**
- [ ] **T11.3: Notifikacije prema recepciji o zavrsenom ciscenju**
- [ ] **T11.4: Work Orders — prijava i pracenje kvarova sa prioritetima**

### Faza 12: Hardware Bridge
- [ ] **T12.1: Arhitektura lokalnog bridge servisa (cross-platform, mock driveri)**
- [ ] **T12.2: Integracija sa fiskalnim printerima (Tring, HCP, NSC)**
- [ ] **T12.3: Integracija sa RFID citacima kartica za sobe (Mifare)**
- [ ] **T12.4: Integracija sa telefonskom centralom (PABX/CDR, wake-up call)**

### Faza 13: Channel Manager
- [ ] **T13.1: API integracija sa Booking.com**
- [ ] **T13.2: API integracija sa Airbnb**
- [ ] **T13.3: Dvosmjerna sinhronizacija dostupnosti (webhook events)**
- [ ] **T13.4: Automatsko azuriranje cijena (rate plan sync)**

### Faza 14: Smart Hotel IoT Integracije
- [ ] **T14.1: MQTT broker setup i konfiguracija (Mosquitto, TLS)**
- [ ] **T14.2: Integracija sa pametnim bravama (BLE/NFC, MQTT)**
- [ ] **T14.3: Senzori za sobu (temperatura, prisutnost, prozori)**
- [ ] **T14.4: Automatizacija energetske efikasnosti (HVAC)**
- [ ] **T14.5: Dashboard za IoT monitoring (device status, energy consumption)**

### Faza 15: Revenue Management
- [ ] **T15.1: Engine za dinamicko odredjivanje cijena**
- [ ] **T15.2: Sezonski modeli i pravila (minimum stay, early bird)**
- [ ] **T15.3: Konkurentska analiza i sugestije**

### Faza 16: Guest Self-Service
- [ ] **T16.1: Online Check-in / Check-out portal**
- [ ] **T16.2: Digitalni kljuc za sobu (mobilna aplikacija, BLE)**
- [ ] **T16.3: In-room narudzbe preko web portala (QR code)**

### Faza 17: Payment Gateway
- [ ] **T17.1: Integracija sa Stripe-om (charge, refund, webhook)**
- [ ] **T17.2: Tokenizacija kreditnih kartica (PCI DSS)**
- [ ] **T17.3: No-show naplata (automatic charge)**

### Faza 18: Stabilizacija i Release
- [ ] **T18.1: Security audit (OWASP top 10, penetration testing)**
- [ ] **T18.2: Performance optimizacija i load testiranje**
- [ ] **T18.3: Produkcijski deployment (Docker, nginx reverse proxy, SSL)**
- [ ] **T18.4: GDPR compliance — export/forget guest data, privacy log**
- [ ] **T18.5: PCI DSS audit — tokenizacija kartica, payment flow security**
- [ ] **T18.6: Korisnicka dokumentacija (user manual, API docs)**

---

## 2. PARALELNI TOKOVI

Sljedece grupe se mogu raditi istovremeno:
- **Grupa A (Infra):** T1.1, T1.2, T1.3, T1.4
- **Grupa B (Frontend):** T4.1, T4.2 (paralelno sa Grupom A)
- **Grupa C (Bridge):** T12.1, T12.2, T12.3, T12.4 (nezavisna od svih)
- **Grupa D (MQTT/IoT):** T14.1, T14.2, T14.3 (nezavisna infrastrukturno)

## 3. AUDIT TRAIL

### 2026-05-15 — Antigravity (Claude Opus)
- Zavrsena analiza legacy koda i MySQL baze
- Reorganizovani fajlovi u `legacy_app/`
- Kreirana `.agents` struktura po Argus standardu
- Checkpoint: `checkpoint: before agents restructure 2026-05-15T10-21`

### 2026-05-15 — HIGH priority items integrated into plan
- **T6.5**: Grupne rezervacije (blokiranje soba, master racun, posebni cjenovnici)
- **T9.5**: POS integracija (automatsko knjizenje troskova restorana/bara na folio)
- **T11.4**: Work Orders (prijava i pracenje kvarova sa prioritetima)
- **T18.4**: GDPR compliance (export/forget guest data)
- **T18.5**: PCI DSS audit (tokenizacija kartica, sigurnost payment flow-a)

### 2026-05-15 — Otvorena pitanja dokument
- Kreiran `docs/otvorena_pitanja.md` — 13 sekcija, ~60 pitanja

### 2026-05-15 — Potpuna atomska dekompozicija zadataka
- Kreirano 89 taskova u `04_tasks/` (Faze 1-18) po jedinstvenom formatu
- Svaki task sadrzi: CILJ, KONTEKST, TEHNICKI ZAHTJEVI, DEFINICIJA GOTOVOG, RESTRIKCIJE
- Azurirani master dokumenti: AGENT_TASK.md, INSTRUCTIONS_FOR_AGENTS.md, MASTER_INSTRUCTIONS.md, TASK_DEPENDENCIES.md
- Svi novi taskovi u PENDING statusu, spremni za claim i implementaciju

### 2026-05-15 — Dokumentacija audit i ispravke
- Ispravljene FSD reference u svim task fajlovima (Faze 1-18) da odgovaraju SPEC_INDEX.md
- Svi FSD-ovi azurirani na AUTHORITATIVE status
- FSD_03_FRONTEND_FOUNDATION.md prosiren sa legacy UI mapping, routing strukturom, state management, API client konfiguracijom
- Dodate legacy-to-new mapping sekcije u kljucne taskove: T5.1, T6.1, T7.1, T8.1, T9.1, T10.1, T11.1, T13.1, T14.1, T15.1, T16.1, T17.1, T18.1
- TASK_DEPENDENCIES.md azuriran sa T12.4 zavisnostima (PABX/CDR → T12.1 + T7.1)
