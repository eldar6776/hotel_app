# AGENT OPERATING RULES

## Osnovno Pravilo

Agent ne implementira. Agent prvo cita, mapira i dokazuje.

Svaki zakljucak mora imati referencu na legacy fajl, funkciju, proceduru, SQL upit, tabelu ili report.

## Minimalni Koraci Za Svaki Modul

1. Napravi inventar fajlova relevantnih za modul.
2. Nadji ulazne tacke: forme, button handlere, public funkcije, menije, jobove.
3. Nadji sve pozvane funkcije.
4. Nadji sve SQL SELECT/INSERT/UPDATE/DELETE naredbe.
5. Popisi tabele i kolone koje tok cita ili mijenja.
6. Izvuci status kodove, magic numbers, flagove i enum-like vrijednosti.
7. Opisi poslovni ishod korisnickim jezikom.
8. Opisi edge case-ove i greske.
9. Napravi golden scenario.
10. Mapiraj na predlozeni novi domain model.

## Pravilo O Callerima

Prije zakljucka o funkciji, agent mora naci:

- ko je poziva
- kada se poziva
- sa kojim parametrima
- sta funkcija mijenja
- sta se desava ako funkcija ne uspije

Ako caller nije nadjen, status funkcije je `ORPHAN_OR_UNKNOWN`, ne "ne koristi se".

## Pravilo O SQL-u

Svaki SQL mora biti klasifikovan:

- READ: cita podatke
- WRITE: insert/update/delete
- LOCK: zakljucava ili oznacava stanje
- REPORT: sluzi izvjestaju
- MIGRATION: import/export
- CONFIG: konfiguracija/sifrarnik

Za svaki WRITE mora se napisati poslovni razlog.

## Pravilo O Statusima

Svi statusi moraju biti izvuceni kao tabela:

```text
source value | label | meaning | set by | read by | transition rule
```

Ne smije se automatski prevoditi u enum dok se ne zna cijeli lifecycle.

## Pravilo O Ledgeru

Ako legacy sistem ima materializovane zapise za nocenja, troskove, placanja ili racune, novi sistem ne smije to svesti na izracun iz datuma bez eksplicitne odluke korisnika.

Ledger zapis je poslovna cinjenica.

## Pravilo O Neizvjesnosti

Ako agent nije siguran, mora napisati:

```text
Status: UNKNOWN
Sta znamo:
Sta ne znamo:
Koji fajl/funkciju treba citati dalje:
Rizik ako pogrijesimo:
```

Nije dozvoljeno popuniti prazninu pretpostavkom.

