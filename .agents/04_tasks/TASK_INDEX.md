# TASK_INDEX

Status: AUTHORITATIVE
Last validated: 2026-05-15

## Izbor taska

1. Procitaj `../STATUS.md`
2. Nadji fazu i task koji je aktivan ili `PENDING`
3. Otvori odgovarajuci folder `faza1/` do `faza18/`
4. Procitaj task fajl
5. Procitaj zavisni FSD
6. Tek onda radi izmjene

## Napomena

- redoslijed fajlova u folderu nije dovoljan
- `STATUS.md` odredjuje stvarni prioritet rada
- Faza 1 (Infrastruktura) i Faza 4 (Frontend) mogu se raditi paralelno
- Faza 2 (Backend) zavisi od Faze 1 (PostgreSQL mora biti podignut)
- Faza 3 (Auth) zavisi od Faze 2 (API mora postojati)
- Core PMS moduli (Faze 5-9) zavise od Faza 2+3+4
- Faza 12 (Hardware Bridge) je nezavisna od svih ostalih i moze se raditi u bilo kojem trenutku
- Faza 14 (IoT) infrastrukturno nezavisna, ali UI komponente zavise od Faze 4
- Legacy kod u `legacy_app/` se NIKADA ne mijenja — sluzi samo za citanje poslovne logike
- Za mapiranje legacy tabela na nove entitete obavezno procitati `03_specs/fsd/FSD_02_BACKEND_FOUNDATION.md` sekcija 3
