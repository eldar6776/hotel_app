# AGENT_TASK

Status: AUTHORITATIVE ENTRY POINT
Last validated: 2026-05-15

## Radi ovim redom

1. Procitaj `00_start_here/README_FOR_AGENTS.md`
2. Procitaj `00_start_here/REPO_REALITY_MAP.md`
3. Procitaj `01_governance/MASTER_INSTRUCTIONS.md`
4. Procitaj `01_governance/WORKFLOW_RULES.md`
5. Procitaj `STATUS.md`
6. Provjeri zavisnosti u `04_tasks/TASK_DEPENDENCIES.md` — uvjeri se da su svi dependency taskovi zavrseni
7. Pronadji task u `04_tasks/`

   **7.A — CLAIM TASK (OBAVEZNO PRIJE BILO KAKVOG RADA):**
   - Provjeri da li je task vec oznacen kao `[-] IN_PROGRESS` u `STATUS.md`
   - Ako jeste: **ODMAH STANI. Ne radi nista. Javi korisniku da je task vec zauzet.**
   - Ako nije: **odmah azuriraj STATUS.md** — promijeni `[ ]` u `[-]` i dodaj `[AGENT: <ime_agenta> - IN_PROGRESS <datum>]` u isti red
   - Ovo mora biti prva commit-sposobna izmjena u sesiji — prije citanja FSD-a, prije implementacije, prije svega

   **7.B — PROVJERA ZAVISNOSTI:**
   - Otvori `04_tasks/TASK_DEPENDENCIES.md` i pronadji task koji zelis raditi
   - Za svaki dependency naveden za taj task, provjeri u `STATUS.md` da li je zavrsen (`[x]`)
   - Ako bilo koji dependency nije zavrsen: **STANI. Javi korisniku koji dependency taskovi nedostaju. Ne pocinji implementaciju.**
   - Ako su svi dependency-ji zavrseni: nastavi

8. Procitaj odgovarajuci FSD u `03_specs/fsd/`
9. Ako task dira hardware bridge (fiskalne kase, kartice), procitaj `03_specs/protocols/HARDWARE_BRIDGE_PROTOCOL.md`
10. Ako task dira IoT integracije (brave, senzori, MQTT), procitaj `03_specs/protocols/IOT_MQTT_PROTOCOL.md`
11. Ako task ima zavisnosti prema drugim taskovima, procitaj i njihove task dokumente da bi razumio interfejse
12. Implementiraj samo ono sto task trazi
13. Pokreni relevantne testove
14. Azuriraj `STATUS.md` — promijeni `[-]` u `[x]`, dodaj `[COMPLETED <datum>]` i audit entry

## Pravilo

Ne preskaci redoslijed. Ne kreci sa izmjenama prije nego procitas task, FSD, status i dependency-je.
Ako postoji neslaganje izmedju aktivnog taska/FSD-a i starih dokumenata u `05_history/`, prednost imaju `STATUS.md`, aktivni FSD i aktivni task dokument.
Legacy VB.NET kod u `legacy_app/` se koristi iskljucivo kao referenca za reverzni inzenjering poslovne logike — nikada ga ne mijenjaj niti izvrsavaj.
