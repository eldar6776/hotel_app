# ROADMAP

Status: WORKING
Last validated: 2026-05-15

Operativni roadmap ostaje primarno u `STATUS.md`, a ovaj dokument sluzi kao kratki indeks.

## Aktivne i naredne cjeline

- Faza 1-2: Infrastruktura, Backend API i DB shema (temelj)
- Faza 3: Autentifikacija i RBAC (sigurnost)
- Faza 4: Frontend Foundation i Design System (UI temelj)
- Faza 5-9: Core PMS moduli (Sobe, Rezervacije, Recepcija, Gosti, Naplata)
- Faza 10-11: Izvjestavanje i Housekeeping
- Faza 12: Hardware Bridge za legacy uredjaje
- Faza 13-17: Napredne integracije (Channel Manager, IoT, Revenue, Self-Service, Payments)
- Faza 18: Stabilizacija, audit i produkcijski release

## Operativni redoslijed

1. Postaviti infrastrukturu (Docker, PostgreSQL) i backend API skeleton kroz Fazu 1 i 2.
2. Uspostaviti autentifikaciju i RBAC kroz Fazu 3 prije bilo kakvog poslovnog modula.
3. Paralelno sa Fazom 2-3 pokrenuti Frontend Foundation (Faza 4) jer su nezavisni.
4. Core PMS moduli (Faze 5-9) se rade sekvencijalno jer zavise jedan od drugog.
5. Izvjestavanje i Housekeeping (Faze 10-11) zavise od core PMS modula.
6. Hardware Bridge (Faza 12) je nezavisan i moze se raditi paralelno sa Fazama 5-11.
7. Napredne integracije (Faze 13-17) se otvaraju tek nakon stabilnih core modula.
8. Stabilizacija i release (Faza 18) je zavrsni sloj.

## Zavrsni audit nakon builda

Radi ovim redom:

1. FSD -> task -> kod -> testovi
2. hardkodirane tajne, magic numberi i debug/bypass logika
3. startup, shutdown, retry, timeout, logging i recovery tokovi
4. ispravke po prioritetu `P0`, `P1`, `P2`

Za stvarni status pojedinacnih taskova koristi `../STATUS.md`.
