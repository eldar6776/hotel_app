# AGENTS.md - Legacy Reconstruction Rules

Ovaj folder ima poseban rezim rada.

## Obavezni Izvor Istine

Za svaku tvrdnju o poslovnoj logici mora postojati dokaz iz `legacy_code/`.

Dozvoljeni dokazi:

- VB.NET fajl i funkcija/event handler
- SQL query iz koda
- SQL dump tabela/kolona
- report definicija
- konfiguracioni fajl
- korisnicki opis runtime ponasanja, ali samo kao dopuna

Nedozvoljeni dokazi:

- "tako obicno radi hotel"
- postojeci novi backend
- status checklist
- UI izgled
- FSD/task tvrdnja bez legacy potvrde

## Pravilo Rada

1. Prvo inventar.
2. Zatim SQL i database map.
3. Zatim poslovna pravila.
4. Zatim golden scenariji.
5. Zatim canonical domain model.
6. Zatim legacy-to-new mapping.
7. Tek onda implementation plan.
8. Tek na kraju atomski taskovi.

## Zabranjeno

- Ne implementirati novi kod tokom legacy analize.
- Ne popravljati trenutni backend.
- Ne mijenjati legacy kod.
- Ne kopirati velike dijelove legacy source koda u dokumente.
- Ne oznacavati UNKNOWN kao DONE.
- Ne praviti task bez acceptance scenarija.
- Ne praviti CRUD task ako poslovni tok zahtijeva ledger/workflow.

## Obavezni Statusi

Za svako pravilo, modul ili mapiranje koristiti jedan od statusa:

- `NOT_STARTED`
- `INVENTORIED`
- `TARGETED_READ`
- `FULL_READ`
- `RULE_EXTRACTED`
- `SCENARIO_READY`
- `SPEC_READY`
- `UNKNOWN`
- `BLOCKED`

## Definition Of Done Za Analizu Pravila

Pravilo je analizirano tek kada postoje:

- legacy lokacija
- opis pravila
- database effects
- statusi/prelazi ako postoje
- edge cases
- golden scenario
- otvorena pitanja
- preporuceni moderni mapping

Ako fali bilo koja stavka, status nije `SPEC_READY`.

