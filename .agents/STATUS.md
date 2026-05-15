# STATUS — HotelPRO

**Trenutni Status:** Faza 0 COMPLETED — Planiranje i Arhitektura. Sljedeci: T1.1 PENDING.
**Datum:** 2026-05-15

---

## 1. PREGLED FAZA RADA

### Faza 0: Planiranje i Arhitektura
- [x] **T0.1: Analiza legacy koda i baze** - [COMPLETED] - 2026-05-15 - Antigravity (Claude Opus)
- [x] **T0.2: Reorganizacija foldera i .agents struktura** - [COMPLETED] - 2026-05-15 - Antigravity (Claude Opus)

### Faza 1: Infrastruktura i DevOps
- [ ] **T1.1: Docker Compose za PostgreSQL 18 i dev okruzenje**
- [ ] **T1.2: .gitignore, EditorConfig i CI osnova**
- [ ] **T1.3: Environment varijable i secrets management**

### Faza 2: Backend Foundation
- [ ] **T2.1: Inicijalizacija .NET 8 Web API projekta**
- [ ] **T2.2: EF Core konfiguracija i PostgreSQL konekcija**
- [ ] **T2.3: Migracija DB sheme — Sobe, VrsteSoba, Zgrade**
- [ ] **T2.4: Migracija DB sheme — Gosti, GostDokument, Partneri**
- [ ] **T2.5: Migracija DB sheme — Rezervacije, RezervacijeGrupe, RelGostSoba**
- [ ] **T2.6: Migracija DB sheme — Folio, Placanje, Troskovi**
- [ ] **T2.7: Migracija DB sheme — Radnici, Smjene, SobaricaLog**

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

### Faza 5: Upravljanje Sobama
- [ ] **T5.1: CRUD API za sobe, tipove soba i zgrade**
- [ ] **T5.2: Frontend — interaktivni pregled soba (Floor Plan / Grid)**
- [ ] **T5.3: Status sobe u realnom vremenu (slobodna, zauzeta, ciscenje)**

### Faza 6: Rezervacije (Booking Engine)
- [ ] **T6.1: CRUD API za rezervacije (pojedinacne i grupne)**
- [ ] **T6.2: Frontend — interaktivni Drag & Drop kalendar rezervacija**
- [ ] **T6.3: Provjera dostupnosti i logika kolizija**
- [ ] **T6.4: Email potvrda rezervacije**
- [ ] **T6.5: Grupne rezervacije — blokiranje soba, master racun, posebni cjenovnici**

### Faza 7: Recepcija (Check-in / Check-out)
- [ ] **T7.1: Check-in workflow API**
- [ ] **T7.2: Check-out workflow sa obracunom**
- [ ] **T7.3: Folio sistem — dodavanje troskova tokom boravka**
- [ ] **T7.4: Frontend — Recepcijski ekran**

### Faza 8: Gosti i CRM
- [ ] **T8.1: CRUD API za goste i dokumente**
- [ ] **T8.2: Pretraga i filtriranje gostiju**
- [ ] **T8.3: Istorija boravka i preferencije gosta**
- [ ] **T8.4: Frontend — Profil gosta**

### Faza 9: Naplata i Fakturisanje
- [ ] **T9.1: Generisanje racuna (PDF export)**
- [ ] **T9.2: Predracuni i avansne uplate**
- [ ] **T9.3: Storno i korekcija racuna**
- [ ] **T9.4: Integracija sa kursnom listom (valute)**
- [ ] **T9.5: POS integracija — automatsko knjizenje troskova restorana/bara na folio gosta**

### Faza 10: Izvjestavanje i Dashboard
- [ ] **T10.1: Dnevni izvjestaj popunjenosti**
- [ ] **T10.2: Finansijski izvjestaji**
- [ ] **T10.3: Knjiga stranih drzavljana (turisticka evidencija)**
- [ ] **T10.4: Statisticke vizualizacije (grafovi, trendovi)**

### Faza 11: Housekeeping
- [ ] **T11.1: API za upravljanje statusom soba (ciscenje, inspekcija)**
- [ ] **T11.2: Mobilno-prilagodjen UI za sobarice (tablet/telefon)**
- [ ] **T11.3: Notifikacije prema recepciji o zavrsenom ciscenju**
- [ ] **T11.4: Work Orders — prijava i pracenje kvarova (klima, TV, vodovod) sa prioritetima**

### Faza 12: Hardware Bridge
- [ ] **T12.1: Arhitektura lokalnog bridge servisa (cross-platform)**
- [ ] **T12.2: Integracija sa fiskalnim printerima**
- [ ] **T12.3: Integracija sa RFID citacima kartica za sobe**
- [ ] **T12.4: Integracija sa telefonskom centralom (CDR)**

### Faza 13: Channel Manager
- [ ] **T13.1: API integracija sa Booking.com**
- [ ] **T13.2: API integracija sa Airbnb**
- [ ] **T13.3: Dvosmjerna sinhronizacija dostupnosti**
- [ ] **T13.4: Automatsko azuriranje cijena**

### Faza 14: Smart Hotel IoT Integracije
- [ ] **T14.1: MQTT broker setup i konfiguracija**
- [ ] **T14.2: Integracija sa pametnim bravama (Bluetooth/NFC)**
- [ ] **T14.3: Senzori za sobu (temperatura, prisutnost, prozori)**
- [ ] **T14.4: Automatizacija energetske efikasnosti (klima, grijanje)**
- [ ] **T14.5: Dashboard za IoT monitoring**

### Faza 15: Revenue Management
- [ ] **T15.1: Engine za dinamicko odredjivanje cijena**
- [ ] **T15.2: Sezonski modeli i pravila**
- [ ] **T15.3: Konkurentska analiza i sugestije**

### Faza 16: Guest Self-Service
- [ ] **T16.1: Online Check-in / Check-out portal**
- [ ] **T16.2: Digitalni kljuc za sobu (mobilna aplikacija)**
- [ ] **T16.3: In-room narudzbe preko web portala**

### Faza 17: Payment Gateway
- [ ] **T17.1: Integracija sa Stripe-om**
- [ ] **T17.2: Tokenizacija kreditnih kartica**
- [ ] **T17.3: No-show naplata**

### Faza 18: Stabilizacija i Release
- [ ] **T18.1: Security audit (OWASP top 10)**
- [ ] **T18.2: Performance optimizacija i load testiranje**
- [ ] **T18.3: Produkcijski deployment (Docker, reverse proxy)**
- [ ] **T18.4: GDPR compliance — export/forget guest data endpointi, log pristupa licnim podacima**
- [ ] **T18.5: PCI DSS audit — tokenizacija kartica, necuvanje brojeva, sigurnost payment flow-a**
- [ ] **T18.6: Korisnicka dokumentacija**

---

## 2. AUDIT TRAIL

### 2026-05-15 — Antigravity (Claude Opus)
- Zavrsena analiza legacy koda i MySQL baze
- Reorganizovani fajlovi u `legacy_app/`
- Kreirana nova struktura foldera
- Kreirana `.agents` struktura po Argus standardu
- Checkpoint: `checkpoint: before agents restructure 2026-05-15T10-21`

### 2026-05-15 — HIGH priority items integrated into plan
- **T6.5**: Grupne rezervacije (blokiranje soba, master racun, posebni cjenovnici)
- **T9.5**: POS integracija (automatsko knjizenje troskova restorana/bara na folio)
- **T11.4**: Work Orders (prijava i pracenje kvarova sa prioritetima)
- **T18.4**: GDPR compliance (export/forget guest data)
- **T18.5**: PCI DSS audit (tokenizacija kartica, sigurnost payment flow-a)

### 2026-05-15 — Otvorena pitanja dokument
- Kreiran `docs/otvorena_pitanja.md` — 13 sekcija, ~60 pitanja koja treba odgovoriti
- Organizovano po modulima (od infrastrukture do smart room)
- Svako pitanje ima mjesto za upisati odluku (✅ / ⬜ / ❌)

### 2026-05-15 — Cross-cutting improvements
Sljedece stavke su dodate u FSD dokumente kao implementation notes:

**FSD_01 (Infrastruktura):**
- Automatski PostgreSQL backup (Docker, daily, 30 dana)
- Off-site backup na S3
- Restore procedura

**FSD_02 (Backend):**
- API verzioniranje (v1, v2, header `api-supported-versions`)
- Rate limiting (staff: 100/10s, guest: 10/10s, auth: 5/min)
- Audit log (JSONB, EF Core interceptor)
- Legacy MySQL → PostgreSQL ETL migrator
- Scheduled jobs (No-Show, Night audit, Daily report, Backup)
- Feature flags (per-hotel, postepeno uvodjenje)
- Multi-language support (hr, en, de, it)

**FSD_05 (Rezervacije):**
- Gantt virtuelizacija (react-virtualized, horizontalna)

**FSD_11 (Hardver):**
- Mock driveri za testiranje (dev mod)
- Offline cache za bridge (persistent queue, exponential backoff)
- Webhook sistem (booking.created, payment.received, room.status.changed)
- Subscriber management per event type
