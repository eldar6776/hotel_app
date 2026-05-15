# STATUS_RULES

Status: AUTHORITATIVE
Last validated: 2026-05-15

## STATUS.md

- procitaj na pocetku rada
- koristi ga za izbor taska
- azuriraj ga nakon rada
- ne brisi postojeci audit trail
- ne upisuj status koji nije uskladjen sa stvarnim kodom

---

## Format oznaka taska

| Oznaka | Znacenje |
|--------|----------|
| `[ ]` | PENDING — niko ne radi |
| `[-]` | IN_PROGRESS — zauzet, agent ide na rad |
| `[x]` | COMPLETED — zavrseno i verifikovano |

Primjer red u IN_PROGRESS stanju:
```
- [-] **T1.1: Inicijalizacija .NET 8 API** - [IN_PROGRESS] - 2026-05-15 - Antigravity (Claude Opus)
```

---

## Pravilo zauzimanja taska (CLAIM LOCK)

**Ovo je jedina zastita od paralelnog rada vise agenata na istom tasku.**

1. Prije nego sto agent pocne bilo koji rad, mora provjeriti oznaku taska u `STATUS.md`
2. Ako task ima `[-]` — **STOP. Task je vec zauzet. Ne diraj nista.**
3. Ako task ima `[ ]` — agent mora **odmah** promjeniti u `[-]` sa vlastitim imenom i datumom
4. Ova izmjena u `STATUS.md` mora biti prva stvar u sesiji, prije implementacije i prije citanja FSD-a
5. Kada je task zavrsen — agent mijenja `[-]` u `[x]` i dodaje audit entry

**NAPOMENA:** Ovo je optimisticki lock — ne garantuje atomarnost. Ako dva agenta istovremeno procitaju `[ ]` i oba krenu, konflikt ce biti vidljiv u git historiji. U tom slucaju vlasnik odlucuje ko nastavlja.

---

## Azuriranje header linije STATUS.md

Linija `**Trenutni Status:**` mora biti azurirana kada se task mijenja:
- Na pocetku rada: dodati `TX.Y IN_PROGRESS (agent)`
- Na kraju rada: sljedeci task u nizu
