# HOW TO USE CURRENT PROJECT

Postojeci projekat nije nula, ali nije ni pouzdana poslovna osnova.

## Sta Se Smije Koristiti

Frontend:

- vizuelni stil
- layout
- sidebar/navbar
- dark mode
- UI komponente
- koncept Gantt ekrana
- koncept dashboarda
- settings layout

Backend:

- tehnoloski stack kao referenca
- nazivi nekih entiteta kao pocetna hipoteza
- test setup kao inspiracija
- API stil kao referenca

Audit:

- `.agents/06_audit/*` kao upozorenje i pocetna mapa rizika

## Sta Se Ne Smije Koristiti Kao Izvor Istine

- `STATUS.md` tvrdnje o zavrsenosti
- mock endpointi
- payment gateway implementacija
- bridge/fiscal implementacija
- IoT/channel/guest portal implementacija
- postojece check-out/fakturisanje pravilo bez legacy potvrde
- `Folio.Balance` kao dovoljan model folia
- postojece FSD/taskove kao kompletan dokaz

## GUI Koristenje

Nakon legacy analize, trenutni frontend se moze koristiti kao GUI shema.

Pravila:

- UI se prilagodjava domain modelu, ne obrnuto.
- API contracti se smiju promijeniti.
- Ekrani se smiju zadrzati vizuelno, ali tokovi moraju pratiti legacy-derived business rules.
- Mock stranice moraju biti jasno oznacene ili uklonjene.

## Backend Koristenje

Preporuka je novi core backend ili veliki kontrolisani rewrite P0 servisa.

Ako se zadrzi isti repo:

- napraviti novi bounded context za core PMS
- ne prosirivati mock servise
- prvo testovi iz golden scenarija
- zatim implementacija
- zatim adaptacija UI-ja

