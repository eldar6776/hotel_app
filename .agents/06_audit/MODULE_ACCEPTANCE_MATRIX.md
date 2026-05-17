# MODULE ACCEPTANCE MATRIX - HotelPRO

Status: WORKING
Datum: 2026-05-17
Restore point: f984b8f

## Svrha

Ova matrica definiše šta znači "završeno" za svaki modul. Status iz `STATUS.md` nije dovoljan dokaz gotovosti.

## Matrica

| Faza | Modul | Minimum za ACCEPTED | Trenutni status | Odluka |
|---:|---|---|---|---|
| 1 | Infrastruktura | Dev + prod deployment, backup restore test, secrets | PARTIAL | Ne zatvarati prod dio |
| 2 | Backend foundation | EF schema + kompletan legacy ETL + migration test | PARTIAL | T2.14 reopen |
| 3 | Auth/RBAC/Tenant | JWT, role policies, tenant isolation tests | PARTIAL | Dodati tenant testove |
| 4 | Frontend foundation | Build/lint/browser smoke | PARTIAL | Ponoviti nakon npm install |
| 5 | Sobe | Legacy status rules + CRUD + SignalR | PARTIAL | Reaudit `fnSobaStatus` |
| 6 | Rezervacije | Availability, groups, edge cases, legacy scenarios | PARTIAL | Dodati scenario testove |
| 7 | Recepcija/Folio | Check-in/out, folio, night audit, real calculations | PARTIAL | P0 audit |
| 8 | Gosti/CRM | Guests/docs/partners/TZ + ETL | PARTIAL | ETL incomplete |
| 9 | Naplata | Invoice sequence, VAT, fiscal, storno, currency | PARTIAL | P0 rewrite needed |
| 10 | Izvještaji | Legacy report parity + scheduled email | PARTIAL | Snapshot tests |
| 11 | Housekeeping | API + PWA/offline + notifications + assignment | PARTIAL | Finish phase |
| 12 | Hardware Bridge | Real service + drivers + queue + health | MOCK | Reopen |
| 13 | Channel Manager | Real provider sync + webhook validation | MOCK | Reopen |
| 14 | IoT | MQTT broker + services + devices + dashboard | MOCK/MISSING | Reopen |
| 15 | Revenue | Rule engine + persisted seasonal rules + forecast | MOCK/PARTIAL | Reopen |
| 16 | Guest Portal | Guest auth + booking validation + real flows | MOCK | Reopen |
| 17 | Payments | Stripe PaymentIntent + webhooks + PCI-safe flow | MOCK | Reopen |
| 18 | Stabilizacija | Security scan, load test, PCI/GDPR evidence, release | MISSING/PARTIAL | Reopen |

## Acceptance Pravilo

Modul može biti `ACCEPTED` samo ako:

- nema mock/stub odgovora u glavnom toku
- ima test ili validaciju
- dokumentacija i kod se slažu
- za core PMS postoji legacy scenario potvrda

