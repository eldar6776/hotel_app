# MODEL PROMPTS

Ovaj dokument sadrzi preporucene promptove za jake modele koji analiziraju legacy kod.

## Prompt 1 - Globalni Inventar

```text
Ti si senior software archeologist i domain analyst. Analiziras legacy VB.NET/MySQL hotelsku aplikaciju.

Tvoj cilj nije rewrite i nije refaktor. Tvoj cilj je izvuci poslovnu logiku iz source koda.

Procitaj folder legacy_code i napravi LEGACY_FUNCTION_INVENTORY.md:
- svi relevantni fajlovi
- klase, moduli, forme
- funkcije i event handleri
- SQL upiti
- tabele koje se citaju/pisu
- poslovni modul kojem pripadaju
- rizik P0/P1/P2
- status citanja

Ne izmisljaj. Ako nisi siguran, oznaci UNKNOWN.
Za svaki zakljucak navedi fajl i funkciju.
```

## Prompt 2 - Poslovna Pravila

```text
Analiziraj legacy_code za P0 hotelske tokove:
- status sobe
- rezervacija
- check-in
- nocenja
- folio
- troskovi
- placanje
- racun
- storno
- fiskalizacija
- check-out

Napravi BUSINESS_RULES_CATALOG.md.

Za svako pravilo navedi:
- legacy fajl/funkcija
- ulazne uslove
- SQL/tabele koje se mijenjaju
- poslovni ishod
- edge cases
- greske
- otvorena pitanja

Ne koristi postojeci novi backend kao izvor istine.
```

## Prompt 3 - Database Map

```text
Analiziraj SQL dump i sve SQL upite u VB.NET kodu.

Napravi LEGACY_DATABASE_MAP.md i SQL_WRITE_MAP.md.

Za svaku tabelu objasni:
- poslovnu ulogu
- kljucne kolone
- status/flag kolone
- finansijske kolone
- datumske kolone
- ko cita
- ko pise
- implicitne relacije
- migracioni rizik

Posebno obradi relgostsoba, nocenja, troskovi, placanje, placanjedetalji, posjetaFolio.
```

## Prompt 4 - Golden Scenarios

```text
Na osnovu izvucene legacy poslovne logike napravi GOLDEN_SCENARIOS.md.

Svaki scenario mora imati:
- pocetno stanje baze
- korisnicku akciju
- ocekivane promjene po tabelama
- ocekivani racun/report/status
- sta ne smije da se desi

Scenariji moraju biti dovoljno konkretni da ih drugi agent moze pretvoriti u automated test.
```

## Prompt 5 - Canonical Model

```text
Na osnovu legacy poslovnih pravila predlozi CANONICAL_DOMAIN_MODEL.md za novi .NET/PostgreSQL sistem.

Ne kopiraj legacy tabele 1:1 ako postoji bolji model, ali nijedno legacy znacenje ne smije nestati.

Posebno modeliraj:
- Stay kao odvojeno od Booking
- RoomAssignment
- NightLedger
- ExpenseLedger
- PaymentLedger
- InvoiceSnapshot
- FiscalWorkflow
- Storno/Correction workflow

Za svaki koncept navedi legacy izvore i invariants.
```

## Prompt 6 - Task Generator

```text
Koristeci BUSINESS_RULES_CATALOG.md, GOLDEN_SCENARIOS.md i CANONICAL_DOMAIN_MODEL.md, napravi ATOMIC_TASKS_DRAFT.md.

Svaki task mora biti mali, testabilan i vezan za legacy scenario.

Task nije validan ako nema:
- legacy referencu
- acceptance scenario
- expected database effect
- validation command
- definition of done
```

