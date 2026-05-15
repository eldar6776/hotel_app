# DEPENDENCY_MATRIX

Status: WORKING
Last validated: 2026-05-15

## Grubi redoslijed zavisnosti

1. Faza 1: Infrastruktura (Docker, PostgreSQL) — nema zavisnosti
2. Faza 2: Backend Foundation — zavisi od Faze 1 (baza mora postojati)
3. Faza 3: Autentifikacija — zavisi od Faze 2 (API mora postojati)
4. Faza 4: Frontend Foundation — nezavisna, moze paralelno sa Fazama 1-3
5. Faza 5-9: Core PMS — zavisi od Faze 2 (backend) + Faze 3 (auth) + Faze 4 (frontend)
6. Faza 10-11: Izvjestaji, Housekeeping — zavisi od Faze 5-9 (podaci moraju postojati)
7. Faza 12: Hardware Bridge — nezavisna, moze paralelno sa svim fazama
8. Faza 13: Channel Manager — zavisi od Faze 6 (Rezervacije) i Faze 5 (Sobe)
9. Faza 14: IoT — nezavisna infrastrukturno, ali UI zavisi od Faze 4
10. Faza 15: Revenue — zavisi od Faze 6 (Rezervacije) i Faze 9 (Naplata)
11. Faza 16: Self-Service — zavisi od Faze 7 (Recepcija) i Faze 14 (IoT za dig. kljuc)
12. Faza 17: Payment Gateway — zavisi od Faze 9 (Naplata)
13. Faza 18: Stabilizacija — zavrsni sloj, zavisi od svih prethodnih

## Eksplicitne zavisnosti

- `T2.1` -> `T1.1` (backend treba Docker PostgreSQL)
- `T3.1` -> `T2.1` (JWT treba API)
- `T4.4` -> `T3.1` (Login UI treba JWT endpoint)
- `T5.1` -> `T2.3` (Sobe API treba DB shemu)
- `T6.1` -> `T2.5` + `T5.1` (Rezervacije trebaju shemu + Sobe API)
- `T7.1` -> `T6.1` + `T8.1` (Check-in treba Rezervacije + Goste)
- `T9.1` -> `T7.3` (Racuni trebaju Folio)
- `T13.1` -> `T6.1` + `T5.1` (Channel Manager treba Rezervacije + Sobe)
- `T14.2` -> `T14.1` (Brave trebaju MQTT broker)
- `T16.2` -> `T14.2` (Digitalni kljuc treba pametne brave)

## Paralelni tokovi

Ove faze se mogu raditi istovremeno bez konflikta:
- Faza 1 + Faza 4 (infra + frontend scaffold)
- Faza 12 (hardware bridge) nezavisna od svih ostalih
- Faza 14 (IoT infrastruktura) nezavisna od core PMS modula

Za detaljnu zavisnost koristi task dokument i `STATUS.md`.
