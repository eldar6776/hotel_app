# MISSION AND SCOPE

## Misija

Izvuci kompletnu poslovnu logiku legacy HotelPRO aplikacije tako da novi sistem moze biti implementiran dokazivo, testabilno i bez izmisljanja pravila.

Ovo nije UI rewrite.
Ovo nije CRUD analiza.
Ovo nije "napravi modernu aplikaciju".

Ovo je forenzicka poslovna rekonstrukcija.

## Sta Agent Mora Isporuciti

Agent mora napraviti dokumentovan lanac dokaza:

```text
legacy source location
-> business rule
-> database effect
-> edge cases
-> modern domain concept
-> acceptance scenario
-> implementation task
```

Bez tog lanca pravilo se smatra neizvucenim.

## Kriticni Poslovni Tokovi

P0 tokovi:

- status sobe i izvedeni statusi
- rezervacija, potvrda, storno, grupa, izvor
- check-in / prijava
- boravak i veza gost-soba
- nocenja kao materializovan ledger
- folio priprema
- troskovi i njihovo zakljucavanje
- placanje i placanje detalji
- racun kao snapshot
- storno racuna
- fiskalizacija
- check-out / odjava
- djelimicna odjava
- prebacivanje sobe
- gosti, dokumenti i turisticka evidencija
- migracija legacy podataka

P1 tokovi:

- partneri/agencije
- popusti i tarife
- izvjestaji
- POS knjizenje
- sobarice/housekeeping
- RFID kartice
- PABX/CDR
- korisnici, role, smjene

P2 tokovi:

- channel manager
- IoT
- guest self-service
- online payment
- revenue management

P2 tokovi se ne smiju raditi prije P0/P1 jezgra osim ako korisnik izricito promijeni prioritet.

## Sta Se Ne Smije Raditi

- Ne smije se prepisati ekran kao CRUD bez poslovnih pravila.
- Ne smije se oznaciti mock kao zavrsen.
- Ne smije se izmisljati pravilo zato sto "ima smisla".
- Ne smije se izgubiti legacy polje zato sto ne izgleda moderno.
- Ne smije se koristiti postojeci novi backend kao dokaz.
- Ne smije se preskociti SQL upis/azuriranje/brisanje.
- Ne smije se praviti atomski task bez acceptance scenarija.

