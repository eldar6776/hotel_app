# 07 LEGACY RECONSTRUCTION - START HERE

Ovaj folder je polazna tacka za novu, strogu analizu legacy VB.NET/MySQL HotelPRO aplikacije.

Korisnik ce u ovaj folder dodati:

```text
.agents/07_legacy_reconstruction/legacy_code/
```

Folder `legacy_code/` treba sadrzavati izvorni VB.NET projekat, SQL dumpove, konfiguracije, report fajlove i sve pratece artefakte koji postoje u staroj aplikaciji.

## Glavni Cilj

Ne praviti novi sistem iz pretpostavki.

Prvo treba dokazivo izvuci poslovnu logiku iz legacy koda, zatim napraviti canonical domain model, zatim mapu legacy -> novi sistem, pa tek onda plan i atomske zadatke za implementaciju.

Postojeci `backend/` i `frontend/` iz ovog repozitorija se ne smiju tretirati kao dokaz poslovne ispravnosti.

Postojeci frontend se smije koristiti kao GUI/UX referenca.

Postojeci backend se smije koristiti samo kao negativna/pozitivna referenca nakon sto se legacy pravilo nezavisno izvuce.

## Redoslijed Rada

1. Procitaj `00_MISSION_AND_SCOPE.md`.
2. Procitaj `01_AGENT_OPERATING_RULES.md`.
3. Procitaj `02_EXTRACTION_PROTOCOL.md`.
4. Popuni artefakte iz `03_REQUIRED_OUTPUTS.md`.
5. Koristi sablone iz `templates/`.
6. Ne pisi novi produkcijski kod dok ne postoje `GOLDEN_SCENARIOS.md` i `LEGACY_TO_NEW_MAPPING.md`.

## Izvor Istine

Prioritet izvora je:

1. Stvarni legacy kod u `legacy_code/`
2. Legacy baza / SQL dump
3. Runtime ponasanje stare aplikacije ako ga korisnik moze pokazati
4. Izvuceni audit dokumenti
5. FSD/task dokumenti
6. Postojeci novi kod

Ako se novi kod ne slaze sa legacy pravilom, legacy pravilo ima prioritet dok korisnik ne odluci drugacije.

