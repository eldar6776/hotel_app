# SPEC_INDEX

Status: AUTHORITATIVE
Last validated: 2026-05-15

## Koristi ovim redom

1. Nadji task u `../04_tasks/`
2. Otvori odgovarajuci FSD u `fsd/` po redoslijedu:
   - FSD_01..03 = Infrastruktura (Foundation)
   - FSD_04..08 = Core PMS moduli (Sobe, Rezervacije, Recepcija, Gosti, Naplata)
   - FSD_09..10 = Izvjestavanje i Odrzavanje
   - FSD_11..12 = Hardver i Telefon
   - FSD_13 = IoT
   - FSD_14 = Help sistem (proposal)
3. Ako task ukljucuje fiskalne kase ili RFID citace, otvori `protocols/HARDWARE_BRIDGE_PROTOCOL.md`
4. Ako task ukljucuje IoT, pametne brave ili senzore, otvori `protocols/IOT_MQTT_PROTOCOL.md`
5. Izvrsi task prema task dokumentu i FSD-u

## Sadrzaj

### FSD dokumenti (fsd/)

| # | Fajl | Faza | Opis |
|---|------|------|------|
| 01 | `FSD_01_INFRASTRUKTURA.md` | 1 | Docker, PostgreSQL, CI/CD |
| 02 | `FSD_02_BACKEND_FOUNDATION.md` | 2 | .NET 8 API, EF Core, DB migracije |
| 03 | `FSD_03_FRONTEND_FOUNDATION.md` | 4 | Next.js, Tailwind, Design System |
| 04 | `FSD_04_SOBE_I_KAPACITETI.md` | 5 | Sobe, tipovi, zgrade, statusi |
| 05 | `FSD_05_REZERVACIJE.md` | 6 | Rezervacije, Gantt, dostupnost |
| 06 | `FSD_06_RECEPCIJA_I_FOLIO.md` | 7 | Check-in/out, folio, troskovi |
| 07 | `FSD_07_GOSTI_I_CRM.md` | 8 | Gosti, partneri, TZ |
| 08 | `FSD_08_NAPLATA_I_FAKTURISANJE.md` | 9 | Racuni, fiskalizacija |
| 09 | `FSD_09_IZVJESTAJI_I_STATISTIKA.md` | 10 | Izvjestaji, dashboard, analitika |
| 10 | `FSD_10_HOUSEKEEPING.md` | 11 | Rad sobarica, status cistoce |
| 11 | `FSD_11_HARDVER_INTEGRACIJE.md` | 12 | RFID, brave, enkoderi |
| 12 | `FSD_12_TELEFONSKA_CENTRALA.md` | 12 | PABX, tarifiranje poziva |
| 13 | `FSD_13_IOT_INTEGRACIJE.md` | 14 | Smart hotel, senzori, MQTT |
| 14 | `FSD_14_HELP_SISTEM.md` | 16 | Help sistem (proposal) |

### Protokoli (protocols/)

| Fajl | Opis |
|------|------|
| `HARDWARE_BRIDGE_PROTOCOL.md` | USB/Serial komunikacija sa hardverom |
| `IOT_MQTT_PROTOCOL.md` | MQTT topici, payload formati i integracija |
