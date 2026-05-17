# P0 FIX PLAN - HotelPRO

Status: PROPOSED
Datum: 2026-05-17
Restore point: f984b8f

## Cilj

Ovaj plan rangira najkritičnije popravke nakon prvog audita. Ne treba početi kodiranje dok korisnik ne odobri konkretan scope.

## P0 Lista

| Prioritet | Problem | Zašto je P0 | Preporučena akcija |
|---:|---|---|---|
| 1 | Status dokumentacije tvrdi `ALL PHASES COMPLETED` | Pogrešno usmjerava sav dalji rad | Ispraviti status nakon dogovora |
| 2 | Legacy poslovna logika nije dokazano prenesena | Aplikacija može izgledati gotova, a poslovno raditi pogrešno | Napraviti legacy scenario mapu za Faze 5-9 |
| 3 | Check-out/fakturisanje/PDV/fiskalizacija | Finansijski i zakonski kritično | Reaudit i rewrite pravila po legacy/FSD |
| 4 | Legacy ETL migrator je nepotpun | Stari operativni podaci se ne prenose | Završiti migraciju gostiju, partnera, plaćanja, usluga, radnika |
| 5 | Payments su mock i primaju card number | PCI/security rizik | Ukloniti raw card flow, dizajnirati Stripe PaymentIntent |
| 6 | Hardware/IoT/Channel su mock | Integracije označene kao završene, a nisu | Reopen Faze 12-14 |
| 7 | Guest portal je demo bez realnog auth/booking toka | Sigurnosni i poslovni rizik | Definisati guest token flow |
| 8 | Faza 18 nema stvarne audit dokaze | Release tvrdnje nisu pouzdane | Security/performance/release audit tek na kraju |

## Predloženi Redoslijed Rada

1. Dokumentacijski cleanup: statusi i jasno označavanje mock/demo modula.
2. Legacy business logic extraction za Faze 5-9.
3. Reimplementacija finansijskog jezgra: folio, noćenja, invoice, storno, PDV, fiskalizacija.
4. ETL migrator završetak.
5. Housekeeping završetak.
6. Tek poslije toga integracije: bridge, channel, IoT, payments, guest portal.

## Blokatori

- Treba pristup/identifikacija relevantnih legacy fajlova za ključne tokove.
- Treba odlučiti da li Faze 12-17 moraju biti stvarne integracije ili prihvatljiv mock za demo.
- Treba odlučiti da li ćemo mijenjati `STATUS.md` sada ili tek nakon završenog audit pregleda sa korisnikom.
