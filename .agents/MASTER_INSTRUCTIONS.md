# MASTER_INSTRUCTIONS

Status: AUTHORITATIVE ENTRY POINT — UVEK POCNI OVDE
Last validated: 2026-05-15

## O CEMU SE OVDE RADI

Ovo je glavni ulaz za svakog AI agenta koji radi na HotelPRO projektu.
MASTER_INSTRUCTIONS je autoritativni dokument koji ti govori:

- **GDE da pocnes** (entry point)
- **KOJIM redosledom** da citas dokumente
- **STA su pravila koja se ne smiju prekrsiti**
- **KAKO da izbjegnes najcesce greske**

## OBAVEZNA PUTANJA (čitaj ovim redom)

Korak 1: `AGENT_TASK.md`
- Ovo je tvoj operativni workflow. Sadrzi kompletan redoslijed koraka od preuzimanja taska do implementacije i prijave.
- Obavezno: sadrzi i instrukcije za CLAIMOVANJE taska i PROVJERU ZAVISNOSTI.

Korak 2: `INSTRUCTIONS_FOR_AGENTS.md`
- Rezime workflow-a i pravila.
- Koristan za brzi podsjetnik nakon sto si vec procitao AGENT_TASK.md.

Korak 3: `01_governance/WORKFLOW_RULES.md`
- Detaljna pravila koja se ne smiju prekršiti.
- Pravila o redoslijedu, prioritetu izvora, i zabranjenim operacijama.

Korak 4: `STATUS.md`
- Trenutno stanje projekta: koji su taskovi završeni, koji su u toku, koji su pending.

## NAJVAZNIJA PRAVILA

1. **NIŠTA ne mijenjaj prije nego procitaš task, FSD i STATUS.md.** Prva izmjena u sesiji mora biti claim taska u STATUS.md.
2. **UVIJEK provjeri TASK_DEPENDENCIES.md prije pocetka rada.** Ako dependency task nije zavrsen, ne smijes krenuti.
3. **FSD i task dokument su iznad tvog misljenja.** Ako postoji neslaganje izmedju koda i FSD-a, FSD je mjerodavan.
4. **Nikada ne mijenjaj legacy_app/ kod.** To je VB.NET legacy — samo referenca.

## HIJERARHIJA DOKUMENATA (ko je iznad koga)

| Nivo | Sta |
|------|-----|
| 1 (najvise) | STATUS.md |
| 2 | AGENT_TASK.md |
| 3 | Aktivni task dokument (`04_tasks/fazaX/tY.md`) |
| 4 | Aktivni FSD (`03_specs/fsd/FSD_X_*.md`) |
| 5 | WORKFLOW_RULES.md |
| 6 | Stvarni kod u `src/` |
| 7 (najnize) | Stari dokumenti u `05_history/` |

Ako se bilo sta na nizem nivou kosi sa visim nivoom — visi nivo pobjeđuje.

## STA AKO SI U NEDOUMICI

1. Otvori AGENT_TASK.md i prati korake
2. Provjeri STATUS.md
3. Provjeri TASK_DEPENDENCIES.md
4. Pitaj korisnika — ne pravi pretpostavke

## KRAJ

Ovo je jedini dokument koji garantuje da ces ispravno zapoceti rad na projektu.
Preskakanje = losa implementacija = gubljenje vremena.
