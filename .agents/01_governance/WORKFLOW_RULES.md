# WORKFLOW_RULES

Status: AUTHORITATIVE
Last validated: 2026-05-15

## Standardni tok

1. Procitaj `AGENT_TASK.md`
2. Procitaj `STATUS.md`
3. Identifikuj task koji treba raditi
4. **CLAIM TASK** — provjeri oznaku i zauzmi task (vidi `STATUS_RULES.md` sekcija "Claim Lock")
5. Procitaj task u `04_tasks/`
6. Procitaj zavisni FSD u `03_specs/fsd/`
7. Implementiraj
8. Testiraj
9. Azuriraj `STATUS.md` — oznaci `[x]`, dodaj audit entry

## Pravila

- jedan task u jednom toku rada, osim ako korisnik ne trazi drugacije
- ne mijenjaj scope taska bez razloga
- ne zatvaraj task bez verifikacije
- ako dokumentacija i kod nisu uskladjeni, prati stvarni kod i evidentiraj razliku
- svaka UI promjena mora biti testirana u browseru (Chrome + Safari)
- backend promjene moraju imati unit test ili integration test
- ne mijenjaj legacy kod u `/legacy_app` — koristi ga samo za citanje

## Pravilo paralelnog rada vise agenata

- Vise agenata MOZE raditi na projektu, ali NIKADA na istom tasku istovremeno
- Svaki agent MORA provjeriti `[-]` oznaku u `STATUS.md` prije pocetka rada
- Ako task vec ima `[-]` — agent mora stati i javiti korisniku, ne smije nastaviti
- Ako agent zavrsi ranije nego ocekivano i task je zauzet, moze poceti citati SLJEDECI slobodan task ali ne smije implementirati dok ga ne zauzme
- Ako git historija pokaze da su dva agenta radila na istom tasku istovremeno, vlasnik mora rucno rijesiti konflikt i odabrati jednu implementaciju
