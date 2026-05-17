# Legacy Reconstruction

Goal: extract all business logic from the legacy HotelPRO codebase before any new implementation work.

Use these files:

| File/Folder | Purpose |
|---|---|
| `AGENT_PROMPT.md` | Copy this prompt into each small AI agent. |
| `LEGACY_ANALYSIS_MASTER.md` | Single source of work and single output document. Agents claim and fill `SRC-####` sections here. |
| `LEGACY_SOURCE_MANIFEST.md` | Manifest/reference list of all source/analyzable legacy files. |
| `legacy_code/` | Primary evidence: VB.NET source, SQL dumps, reports, config, and artifacts. |

Rules:

- Do not use old drafts as proof.
- Do not create side analysis documents.
- Do not implement production code from guesses.
- Each agent claims exactly one `SRC-####` before reading source.
- Every business conclusion must cite legacy evidence with file/line references.
