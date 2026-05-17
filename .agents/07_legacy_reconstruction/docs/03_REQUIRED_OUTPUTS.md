# REQUIRED OUTPUTS

Ovo su obavezni izlazni dokumenti nakon legacy analize.

## 1. LEGACY_FUNCTION_INVENTORY.md

Svrha: dokaz da je agent znao sta postoji u source kodu.

Mora sadrzavati:

- popis svih relevantnih fajlova
- popis funkcija/event handlera
- procjenu rizika po fajlu
- status citanja
- prioritet za dublju analizu

## 2. LEGACY_DATABASE_MAP.md

Svrha: razumjeti stvarni persistence model.

Mora sadrzavati:

- tabele
- kolone
- implicitne veze
- ko cita/pise
- business meaning
- migracioni rizik

## 3. SQL_WRITE_MAP.md

Svrha: naci sve poslovne promjene stanja.

Mora sadrzavati svaki INSERT/UPDATE/DELETE:

- SQL fragment ili kratak opis
- fajl/funkcija
- tabela/kolone
- poslovni razlog
- povezani scenario

## 4. BUSINESS_RULES_CATALOG.md

Svrha: centralni katalog pravila.

Mora imati stable IDs.

## 5. STATUS_AND_TRANSITION_MATRIX.md

Svrha: dokazati sve statuse i prelaze.

Posebno:

- soba
- rezervacija
- boravak
- folio
- trosak
- placanje
- racun
- fiskalizacija

## 6. GOLDEN_SCENARIOS.md

Svrha: testovi prije implementacije.

Bez ovog fajla nema atomskih taskova.

## 7. CANONICAL_DOMAIN_MODEL.md

Svrha: novi poslovni model.

Mora razlikovati:

- booking
- stay
- room assignment
- night ledger
- expense ledger
- payment ledger
- invoice snapshot
- fiscal workflow

## 8. LEGACY_TO_NEW_MAPPING.md

Svrha: veza izmedju starog i novog.

Mora pokazati gdje svako legacy pravilo zivi u novom sistemu.

## 9. IMPLEMENTATION_PLAN.md

Svrha: redoslijed izgradnje.

Mora biti vertikalan i test-driven.

## 10. ATOMIC_TASKS_DRAFT.md

Svrha: draft zadataka za agente.

Svaki task mora imati:

- legacy references
- affected domain concept
- expected behavior
- acceptance scenario
- files/modules to touch
- files/modules not to touch
- validation command
- rollback note

