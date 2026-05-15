# INSTRUCTIONS_FOR_AGENTS

Status: AUTHORITATIVE
Last validated: 2026-05-15

## Obavezno citanje

1. `AGENT_TASK.md` — entry point i workflow
2. `01_governance/WORKFLOW_RULES.md` — pravila rada
3. `STATUS.md` — trenutni status svih taskova
4. `04_tasks/TASK_DEPENDENCIES.md` — provjera zavisnosti prije pocetka rada

## Workflow (sažeto)

1. Procitaj AGENT_TASK.md i prati redoslijed
2. **PRIJE implementacije:**
   - Claim-uj task u STATUS.md (`[ ]` -> `[-]`)
   - Provjeri dependency-je u TASK_DEPENDENCIES.md
   - Otvori STATUS.md i provjeri da li su dependency-ji zavrseni
3. Procitaj odgovarajuci FSD
4. Ako task ima zavisnosti, procitaj i task dokumente zavisnih taskova (da bi razumio interfejse i ugovore)
5. Implementiraj
6. Testiraj
7. Azuriraj STATUS.md

## Ako nesto nije jasno

- Otvori relevantni FSD
- Otvori task dokumente zavisnih taskova
- Provjeri `05_history/` za prethodne odluke
- Ako i dalje nije jasno: pitaj korisnika, ne improvizuj
