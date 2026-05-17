# EXTRACTION PROTOCOL

## Faza A - Inventar

Napraviti `LEGACY_FUNCTION_INVENTORY.md`.

Za svaki fajl:

- path
- tip fajla
- velicina
- glavna uloga
- relevantni moduli
- status citanja: INVENTORIED, TARGETED_READ, FULL_READ
- rizik: P0, P1, P2

Za svaki VB fajl izdvojiti:

- klase/moduli
- public/private funkcije
- event handlere
- SQL stringove
- globalne varijable
- dependency na druge fajlove

## Faza B - Baza

Napraviti `LEGACY_DATABASE_MAP.md`.

Za svaku tabelu:

- naziv
- poslovna uloga
- primarni kljuc
- strani kljucevi ako postoje ili implicitne veze
- status/flag kolone
- finansijske kolone
- datumske kolone
- ko cita tabelu
- ko pise u tabelu
- kandidat za novi entitet/ledger/value object

Posebno obraditi:

- `relgostsoba`
- `nocenja`
- `troskovi`
- `placanje`
- `placanjedetalji`
- `posjetaFolio`
- `rezervacije`
- `rezervacijegrupe`
- `gosti`
- `sobe`
- sifrarnike usluga, tarifa, placanja i poreza

## Faza C - Poslovna Pravila

Napraviti `BUSINESS_RULES_CATALOG.md`.

Svako pravilo mora imati ID:

```text
RULE-ROOM-001
RULE-STAY-001
RULE-FOLIO-001
RULE-INVOICE-001
RULE-STORNO-001
```

Format pravila:

- ID
- naziv
- legacy lokacija
- ulazni uslovi
- akcija
- promjene u bazi
- izlazni rezultat
- greske/edge cases
- otvorena pitanja
- golden scenario reference

## Faza D - Golden Scenarios

Napraviti `GOLDEN_SCENARIOS.md`.

Golden scenario je mali poslovni test izveden iz legacy ponasanja.

Svaki scenario mora imati:

- pocetno stanje baze
- korisnicku akciju
- ocekivane promjene po tabelama
- ocekivani UI/print/report rezultat ako postoji
- sta ne smije da se desi

Primjeri P0 scenarija:

- slobodna soba postaje zauzeta nakon check-in-a
- check-in kreira vezu gost-soba i folio stanje
- night audit materializuje nocenje
- checkout zakljucava troskove
- storno racuna otkljucava nenocenje troskove
- djelimicna odjava ne zatvara cijelu sobu ako ostaju gosti
- prebacivanje sobe prenosi nocenja/troskove gdje legacy to radi

## Faza E - Canonical Domain Model

Napraviti `CANONICAL_DOMAIN_MODEL.md`.

Ovaj model ne mora biti 1:1 legacy tabela, ali nijedno legacy znacenje ne smije nestati.

Za svaki domain koncept:

- naziv
- odgovornost
- legacy izvori
- kljucna stanja
- invariants
- koje module koristi
- sta ne smije znati

## Faza F - Legacy To New Mapping

Napraviti `LEGACY_TO_NEW_MAPPING.md`.

Mapiranje mora imati:

```text
legacy table/function/form
-> extracted business rule
-> canonical domain concept
-> new entity/service/API
-> acceptance test
-> implementation status
```

Statusi:

- NOT_STARTED
- ANALYZED
- SPEC_READY
- TEST_READY
- IMPLEMENTED
- VERIFIED
- REJECTED_BY_USER

## Faza G - Implementation Plan

Tek nakon prethodnih faza napraviti `IMPLEMENTATION_PLAN.md`.

Plan mora biti vertikalan:

1. Room status read model
2. Stay/check-in ledger
3. Night ledger
4. Folio preparation
5. Invoice snapshot
6. Storno restore
7. Checkout closure
8. Reports

Ne praviti plan po principu "CRUD za sve tabele".

