# TASK_DEPENDENCIES

Status: AUTHORITATIVE
Last validated: 2026-05-15

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

- `T1.1` nema zavisnosti — Docker Compose je prvi korak
- `T1.2` nema zavisnosti — .gitignore je nezavisan
- `T2.1` -> `T1.1` (backend treba PostgreSQL iz Dockera)
- `T2.2` -> `T2.1` (EF Core treba API projekat)
- `T2.3` do `T2.7` -> `T2.2` (migracije trebaju EF Core konfiguraciju)
- `T3.1` -> `T2.1` (JWT treba API projekat)
- `T3.2` -> `T3.1` (RBAC treba autentifikaciju)
- `T4.1` nema zavisnosti — moze paralelno sa svim
- `T4.2` -> `T4.1` (Design System treba Next.js projekat)
- `T4.3` -> `T4.2` (Layout treba Design System)
- `T4.4` -> `T4.3` + `T3.1` (Login treba Layout + JWT endpoint)
- `T4.5` -> `T4.3` (Dashboard treba Layout)
- `T5.1` -> `T2.3` (Sobe API treba DB shemu za sobe)
- `T5.2` -> `T5.1` + `T4.3` (Sobe UI treba API + Layout)
- `T6.1` -> `T2.5` + `T5.1` (Rezervacije treba DB shemu + Sobe API)
- `T6.2` -> `T6.1` + `T4.3` (Kalendar treba Rez API + Layout)
- `T7.1` -> `T6.1` + `T8.1` (Check-in treba Rezervacije + Goste)
- `T9.1` -> `T7.3` (Racuni treba Folio)
- `T12.1` nema zavisnosti — nezavisan servis
- `T13.1` -> `T6.1` + `T5.1` (Channel Manager treba Rez + Sobe)
- `T14.1` nema zavisnosti — MQTT broker nezavisan
- `T14.2` -> `T14.1` (Brave trebaju MQTT broker)
- `T16.2` -> `T14.2` (Dig. kljuc treba brave)
- `T17.1` -> `T9.1` (Stripe treba Naplatu)

## Paralelni tokovi

Ove grupe taskova se mogu raditi istovremeno:
- Grupa A: T1.1, T1.2, T1.3
- Grupa B: T4.1, T4.2 (paralelno sa Grupom A)
- Grupa C: T12.1, T12.2, T12.3, T12.4 (nezavisna od svih)
- Grupa D: T14.1, T14.2, T14.3 (nezavisna infrastrukturno)
