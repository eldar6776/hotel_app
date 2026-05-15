# TASK_DEPENDENCIES

Status: AUTHORITATIVE
Last validated: 2026-05-15 (T12.4 dependencies added)

## Pravilo

Task radi tek kada:

- zavisni taskovi su zavrseni ili svjesno preskoceni uz jasan razlog
- task dokument i FSD ne ulaze u konflikt sa stvarnim kodom
- znas kako ces testirati rezultat

## Prioritet izvora

1. `../STATUS.md`
2. task dokument
3. FSD
4. stvarni kod

## Kriticne zavisnosti

### Faza 1 — Infrastruktura
- `T1.1` nema zavisnosti — Docker Compose je prvi korak
- `T1.2` nema zavisnosti — .gitignore je nezavisan

### Faza 2 — Backend
- `T2.1` -> `T1.1` (backend treba PostgreSQL iz Dockera)
- `T2.2` -> `T2.1` (EF Core treba API projekat)
- `T2.3` do `T2.7` -> `T2.2` (migracije trebaju EF Core konfiguraciju)

### Faza 3 — Auth
- `T3.1` -> `T2.1` (JWT treba API projekat)
- `T3.2` -> `T3.1` (RBAC treba autentifikaciju)

### Faza 4 — Frontend
- `T4.1` nema zavisnosti — moze paralelno sa svim
- `T4.2` -> `T4.1` (Design System treba Next.js projekat)
- `T4.3` -> `T4.2` (Layout treba Design System)
- `T4.4` -> `T4.3` + `T3.1` (Login treba Layout + JWT endpoint)
- `T4.5` -> `T4.3` (Dashboard treba Layout)

### Faza 5 — Sobe
- `T5.1` -> `T2.3` (Sobe API treba DB shemu za sobe)
- `T5.2` -> `T5.1` + `T4.3` (Sobe UI treba API + Layout)

### Faza 6 — Rezervacije
- `T6.1` -> `T2.5` + `T5.1` (Rezervacije treba DB shemu + Sobe API)
- `T6.2` -> `T6.1` + `T4.3` (Kalendar treba Rez API + Layout)

### Faza 7 — Recepcija
- `T7.1` -> `T6.1` + `T8.1` (Check-in treba Rezervacije + Goste)

### Faza 9 — Naplata
- `T9.1` -> `T7.3` (Racuni treba Folio)

### Faza 12 — Nezavisni servis
- `T12.1` nema zavisnosti — nezavisan servis
- `T12.2` -> `T12.1` (Fiskalni printeri trebaju bridge arhitekturu)
- `T12.3` -> `T12.1` (RFID citaci trebaju bridge arhitekturu)
- `T12.4` -> `T12.1` (PABX/CDR treba bridge servis) + `T7.1` (CDR knjiženje treba check-in workflow za mapiranje soba)

### Faza 13 — Channel Manager
- `T13.1` -> `T6.1` + `T5.1` (Channel Manager treba Rez + Sobe)

### Faza 14 — IoT
- `T14.1` nema zavisnosti — MQTT broker nezavisan
- `T14.2` -> `T14.1` (Brave trebaju MQTT broker)

### Faza 15 — Dinamicko odredjivanje cijena
- `T15.1` -> `T5.1` + `T6.1` (Pricing engine treba sobe i rezervacije)
- `T15.2` -> `T15.1` (Sezonski modeli trebaju pricing engine)
- `T15.3` -> `T15.1` + `T6.1` (Konkurentska analiza treba pricing engine i rezervacije)

### Faza 16 — Self-service i mobilne funkcionalnosti
- `T16.1` -> `T8.1` + `T7.1` (Online check-in treba Guest API i Check-in workflow)
- `T16.2` -> `T14.2` (Digitalni kljuc treba pametne brave)
- `T16.3` -> `T6.1` + `T9.1` (In-room narudzbe trebaju rezervacije i naplatu)

### Faza 17 — Stripe integracija
- `T17.1` -> `T9.1` (Stripe treba naplatu i fakturisanje)
- `T17.2` -> `T17.1` (Tokenizacija treba Stripe integraciju)
- `T17.3` -> `T17.1` (No-show naplata treba Stripe integraciju)

### Faza 18 — Audit, optimizacija i deployment
- `T18.1` -> `T2.1` + `T4.1` (Security audit treba backend i frontend)
- `T18.2` -> `T18.1` (Performance optimizacija nakon security audita)
- `T18.3` -> `T18.2` (Produkcijski deployment nakon optimizacije)
- `T18.4` -> `T8.1` (GDPR compliance treba Guest sistem)
- `T18.5` -> `T17.2` (PCI DSS audit treba tokenizaciju)
- `T18.6` -> sve prethodne faze (Dokumentacija na kraju)

## Paralelni tokovi

Ove grupe taskova se mogu raditi istovremeno:
- Grupa A: T1.1, T1.2, T1.3
- Grupa B: T4.1, T4.2 (paralelno sa Grupom A)
- Grupa C: T12.1, T12.2, T12.3, T12.4 (nezavisna od svih)
- Grupa D: T14.1, T14.2, T14.3 (nezavisna infrastrukturno)
- Grupa E: T15.2, T15.3 (mogu paralelno nakon T15.1)
- Grupa F: T18.4, T18.5 (mogu paralelno nakon svojih zavisnosti)
