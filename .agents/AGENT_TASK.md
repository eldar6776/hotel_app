# AGENT_TASK

Status: AUTHORITATIVE ENTRY POINT
Last validated: 2026-05-15

## Radi ovim redom

1. Procitaj `00_start_here/README_FOR_AGENTS.md`
2. Procitaj `00_start_here/REPO_REALITY_MAP.md`
3. Procitaj `01_governance/MASTER_INSTRUCTIONS.md`
4. Procitaj `01_governance/WORKFLOW_RULES.md`
5. Procitaj `STATUS.md`
6. Pronadji task u `04_tasks/`

   **6.A — CLAIM TASK (OBAVEZNO PRIJE BILO KAKVOG RADA):**
   - Provjeri da li je task vec oznacen kao `[-] IN_PROGRESS` u `STATUS.md`
   - Ako jeste: **ODMAH STANI. Ne radi nista. Javi korisniku da je task vec zauzet.**
   - Ako nije: **odmah azuriraj STATUS.md** — promijeni `[ ]` u `[-]` i dodaj `[AGENT: <ime_agenta> - IN_PROGRESS <datum>]` u isti red
   - Ovo mora biti prva commit-sposobna izmjena u sesiji — prije citanja FSD-a, prije implementacije, prije svega

7. Procitaj odgovarajuci FSD u `03_specs/fsd/`
8. Ako task dira hardware bridge (fiskalne kase, kartice), procitaj `03_specs/protocols/HARDWARE_BRIDGE_PROTOCOL.md`
9. Ako task dira IoT integracije (brave, senzori, MQTT), procitaj `03_specs/protocols/IOT_MQTT_PROTOCOL.md`
10. Implementiraj samo ono sto task trazi
11. Pokreni relevantne testove
12. Azuriraj `STATUS.md` — promijeni `[-]` u `[x]`, dodaj `[COMPLETED <datum>]` i audit entry

## Pravilo

Ne preskaci redoslijed. Ne kreci sa izmjenama prije nego procitas task, FSD i status.
Ako postoji neslaganje izmedju aktivnog taska/FSD-a i starih dokumenata u `05_history/`, prednost imaju `STATUS.md`, aktivni FSD i aktivni task dokument.
Legacy VB.NET kod u `legacy_app/` se koristi iskljucivo kao referenca za reverzni inzenjering poslovne logike — nikada ga ne mijenjaj niti izvrsavaj.
