# LEGACY SOURCE MANIFEST

Status: MANDATORY_REVIEW_MANIFEST
Date: 2026-05-17

This file is the gate for legacy reconstruction. No file is considered read because it was listed or grep-targeted. A file can move from `NOT_REVIEWED` to `FULLY_REVIEWED` only after the checklist below is completed and evidence is linked.

## Review Status Rules

| Status | Meaning | Allowed Use |
|---|---|---|
| `NOT_REVIEWED` | File is only discovered/listed. | No business conclusions may be made from it. |
| `TARGETED_READ` | Specific lines/functions were inspected for a known flow. | May support a narrow rule only with exact references. |
| `FULLY_REVIEWED` | Whole file was read and extracted. | May be used as complete evidence for its scope. |
| `BINARY_OR_TOOLING` | Not directly readable source, but may contain report/runtime behavior. | Must be handled with tool/runtime inspection or marked blocked. |
| `BLOCKED` | Cannot be read with available tools. | Must state blocker and risk. |

## Required Per-File Checklist

A file must not be marked `FULLY_REVIEWED` until all applicable items are done:

- Read the entire file, not just first/grep-hit lines.
- Extract classes/modules/forms and all functions/event handlers.
- Extract every SQL SELECT/INSERT/UPDATE/DELETE and classify READ/WRITE/LOCK/REPORT/MIGRATION/CONFIG.
- Find callers/callees for every business function, or mark `ORPHAN_OR_UNKNOWN`.
- Extract status values, magic numbers, flags, and enum-like values.
- Document database effects and edge cases.
- Link extracted rules/scenarios/mapping rows.
- Record reviewer/date/evidence path.

## Summary

| Metric | Count |
|---|---:|
| Source/analyzable files in manifest | 568 |
| .vb | 259 |
| .resx | 104 |
| .rpt | 80 |
| .xml | 77 |
| .in | 12 |
| .txt | 9 |
| .sql | 8 |
| .rdlc | 3 |
| .config | 3 |
| .manifest | 2 |
| .hml | 2 |
| .vbproj | 1 |
| .sln | 1 |
| .css | 1 |
| .md | 1 |
| .settings | 1 |
| .xsd | 1 |
| .xsc | 1 |
| .myapp | 1 |
| .xss | 1 |

## Manifest

| ID | Path | Type | Size | Priority | Module Guess | Review Status | Required Extraction | Evidence Links | Reviewer | Reviewed At | Notes |
|---|---|---|---:|---|---|---|---|---|---|---|---|
| SRC-0001 | `_UpgradeReport_Files/UpgradeReport.css` | Upgrade/report style | 3348 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0002 | `0FS_rr.in` | Fiscal/device input template | 550 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0003 | `0FS_rr_tr.in` | Fiscal/device input template | 10 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0004 | `0rr.in` | Fiscal/device input template | 550 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0005 | `0trigger.in` | Fiscal/device input template | 10 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0006 | `app.config` | App/runtime config | 6863 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0007 | `ApplicationEvents.vb` | VB.NET source | 0 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0008 | `AssemblyInfo.vb` | VB.NET source | 1243 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0009 | `bazakasa.sql` | SQL schema/data/procedure | 3407 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | tables/procedures/functions/writes/statuses/seed data |  |  |  |  |
| SRC-0010 | `bin/akci.txt` | Text artifact | 2 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0011 | `bin/bazakasa.sql` | SQL schema/data/procedure | 3412 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | tables/procedures/functions/writes/statuses/seed data |  |  |  |  |
| SRC-0012 | `bin/cft.txt` | Text artifact | 54 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0013 | `bin/CrystalDecisions.CrystalReports.Engine.xml` | XML config/report template | 86778 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0014 | `bin/CrystalDecisions.ReportSource.xml` | XML config/report template | 1762 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0015 | `bin/CrystalDecisions.Shared.xml` | XML config/report template | 117633 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0016 | `bin/CrystalDecisions.Windows.Forms.xml` | XML config/report template | 17620 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0017 | `bin/Database Backup 2019-02-01 08-28-05.sql` | SQL schema/data/procedure | 0 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | tables/procedures/functions/writes/statuses/seed data |  |  |  |  |
| SRC-0018 | `bin/Database Backup 2019-02-01 08-31-26.sql` | SQL schema/data/procedure | 9077290 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | tables/procedures/functions/writes/statuses/seed data |  |  |  |  |
| SRC-0019 | `bin/det.xml` | XML config/report template | 412 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0020 | `bin/Dnevni.xml` | XML config/report template | 98 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0021 | `bin/DnevniPlacanje.xml` | XML config/report template | 62063 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0022 | `bin/DnevniSUB.xml` | XML config/report template | 1027 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0023 | `bin/drzave.xml` | XML config/report template | 6088 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0024 | `bin/dtgostd.xml` | XML config/report template | 626 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0025 | `bin/dtgosti.hml` | Unknown text artifact | 5468 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0026 | `bin/dtgosti.xml` | XML config/report template | 6155 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0027 | `bin/dtgostis.hml` | Unknown text artifact | 5943 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0028 | `bin/dtgostis.xml` | XML config/report template | 5943 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0029 | `bin/estranac.xml` | XML config/report template | 313916 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0030 | `bin/GKexp.in` | Fiscal/device input template | 1157 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0031 | `bin/GostiLista.xml` | XML config/report template | 6911 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0032 | `bin/GostiListaTurist.xml` | XML config/report template | 1719 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0033 | `bin/HotelPRO.exe.config` | App/runtime config | 6863 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0034 | `bin/HotelPRO.vshost.exe.config` | App/runtime config | 6863 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0035 | `bin/HotelPRO.vshost.exe.manifest` | Assembly manifest | 1478 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0036 | `bin/HotelPRO.xml` | XML config/report template | 1583 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0037 | `bin/izstat.xml` | XML config/report template | 213 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0038 | `bin/IzvjestajNaplata.xml` | XML config/report template | 780 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0039 | `bin/IzvjestajRezervacijaHeader.xml` | XML config/report template | 1680 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0040 | `bin/IzvjestajRezervacijaPoj.xml` | XML config/report template | 4149 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0041 | `bin/IzvjestajRezervacijaPojHead.xml` | XML config/report template | 870 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0042 | `bin/izvjestajStatistika.xml` | XML config/report template | 846 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0043 | `bin/JRexp.in` | Fiscal/device input template | 422 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0044 | `bin/KIFexp.in` | Fiscal/device input template | 304 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0045 | `bin/merona.sql` | SQL schema/data/procedure | 1359317 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | tables/procedures/functions/writes/statuses/seed data |  |  |  |  |
| SRC-0046 | `bin/Microsoft.ReportViewer.WinForms.xml` | XML config/report template | 108039 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0047 | `bin/new file path.xml` | XML config/report template | 1201 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0048 | `bin/New Project 20150407 2136.sql` | SQL schema/data/procedure | 1601975 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | tables/procedures/functions/writes/statuses/seed data |  |  |  |  |
| SRC-0049 | `bin/presjek.xml` | XML config/report template | 98 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0050 | `bin/printFooter.xml` | XML config/report template | 1003 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0051 | `bin/printGore.xml` | XML config/report template | 1756 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0052 | `bin/printSredina.xml` | XML config/report template | 1838 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0053 | `bin/Prodaja_0.xml` | XML config/report template | 166 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0054 | `bin/Prodaja_122.xml` | XML config/report template | 373 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0055 | `bin/Prodaja_126.xml` | XML config/report template | 612 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0056 | `bin/ra.in` | Fiscal/device input template | 213 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0057 | `bin/Rad_.xml` | XML config/report template | 182 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0058 | `bin/Repperiod.xml` | XML config/report template | 138 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0059 | `bin/restoranDorucak.xml` | XML config/report template | 1087 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0060 | `bin/rez.xml` | XML config/report template | 6606 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0061 | `bin/rez1.xml` | XML config/report template | 1906 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0062 | `bin/rr.in` | Fiscal/device input template | 256 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0063 | `bin/rrH.in` | Fiscal/device input template | 460 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0064 | `bin/sobaricaShema.xml` | XML config/report template | 1452 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0065 | `bin/sobaricaShema1.xml` | XML config/report template | 1860 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0066 | `bin/st.txt` | Text artifact | 0 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0067 | `bin/te.xml` | XML config/report template | 14992 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0068 | `bin/TelefonskiRacun.xml` | XML config/report template | 1249 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0069 | `bin/trenutniDnevni.xml` | XML config/report template | 1444 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0070 | `bin/ver.txt` | Text artifact | 7 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0071 | `clasMysqlAdapt.vb` | VB.NET source | 1863 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0072 | `classKard.vb` | VB.NET source | 12378 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0073 | `ClassLuxM.vb` | VB.NET source | 4159 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0074 | `clasTZ.vb` | VB.NET source | 5214 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0075 | `Data.vb` | VB.NET source | 32116 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0076 | `DnevniIzvjestaj.rpt` | Crystal report binary/artifact | 32768 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0077 | `DnevniIzvjestaj.vb` | VB.NET source | 5939 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0078 | `DnevniIzvjestajrpt.Designer.vb` | VB.NET source | 4322 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0079 | `DnevniIzvjestajrpt.resx` | WinForms/resource XML | 5814 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0080 | `DnevniIzvjestajrpt.vb` | VB.NET source | 823 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0081 | `DnevniSUB1.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0082 | `DnevniSUB1.vb` | VB.NET source | 5560 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0083 | `Explorer1.Designer.vb` | VB.NET source | 35758 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0084 | `Explorer1.resx` | WinForms/resource XML | 39407 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0085 | `Explorer1.vb` | VB.NET source | 8231 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0086 | `FormSobe/Krivacuprija/frmSobe.resx` | WinForms/resource XML | 69771 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0087 | `FormSobe/Krivacuprija/frmSobe.vb` | VB.NET source | 45313 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0088 | `frmAlarm.Designer.vb` | VB.NET source | 18653 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0089 | `frmAlarm.resx` | WinForms/resource XML | 7470 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0090 | `frmAlarm.vb` | VB.NET source | 7430 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0091 | `frmAlarmshow.Designer.vb` | VB.NET source | 5837 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0092 | `frmAlarmshow.resx` | WinForms/resource XML | 44833 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0093 | `frmAlarmshow.vb` | VB.NET source | 2378 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0094 | `frmBaza.resx` | WinForms/resource XML | 5814 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0095 | `frmBaza.vb` | VB.NET source | 89210 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0096 | `frmBazaPas.Designer.vb` | VB.NET source | 5767 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0097 | `frmBazaPas.resx` | WinForms/resource XML | 64335 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0098 | `frmBazaPas.vb` | VB.NET source | 1048 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0099 | `frmDodaj.Designer.vb` | VB.NET source | 3214 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0100 | `frmDodaj.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0101 | `frmDodaj.vb` | VB.NET source | 383 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0102 | `frmDodajDrzave.Designer.vb` | VB.NET source | 5640 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0103 | `frmDodajDrzave.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0104 | `frmDodajDrzave.vb` | VB.NET source | 524 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0105 | `frmDodatno.Designer.vb` | VB.NET source | 6555 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0106 | `frmDodatno.resx` | WinForms/resource XML | 249104 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0107 | `frmDodatno.vb` | VB.NET source | 679 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0108 | `frmExpNo.Designer.vb` | VB.NET source | 3883 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0109 | `frmExpNo.resx` | WinForms/resource XML | 12849 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0110 | `frmExpNo.vb` | VB.NET source | 199 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0111 | `frmExport.Designer.vb` | VB.NET source | 29590 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0112 | `frmExport.resx` | WinForms/resource XML | 9205 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0113 | `frmExport.vb` | VB.NET source | 31710 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0114 | `frmFiskal.Designer.vb` | VB.NET source | 13565 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0115 | `frmFiskal.resx` | WinForms/resource XML | 10359 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0116 | `frmFiskal.vb` | VB.NET source | 37438 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0117 | `frmFiskall.Designer.vb` | VB.NET source | 5082 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0118 | `frmFiskall.resx` | WinForms/resource XML | 10269 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0119 | `frmFiskall.vb` | VB.NET source | 653 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0120 | `frmGlavni.hr.resx` | WinForms/resource XML | 6548 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0121 | `frmGlavni.resx` | WinForms/resource XML | 293197 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0122 | `frmGlavni.vb` | VB.NET source | 183840 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0123 | `frmGosti.resx` | WinForms/resource XML | 25625 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0124 | `frmGosti.vb` | VB.NET source | 258402 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0125 | `frmGrupe.Designer.vb` | VB.NET source | 2765 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0126 | `frmGrupe.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0127 | `frmGrupe.vb` | VB.NET source | 3645 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0128 | `frmGrupeIzmjena.Designer.vb` | VB.NET source | 3406 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0129 | `frmGrupeIzmjena.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0130 | `frmGrupeIzmjena.vb` | VB.NET source | 3954 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0131 | `frmIzvjestaji.resx` | WinForms/resource XML | 5814 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0132 | `frmIzvjestaji.vb` | VB.NET source | 33764 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0133 | `frmIzvjestajiDnevni.Designer.vb` | VB.NET source | 25960 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0134 | `frmIzvjestajiDnevni.resx` | WinForms/resource XML | 5814 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0135 | `frmIzvjestajiDnevni.vb` | VB.NET source | 74460 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0136 | `frmKard.Designer.vb` | VB.NET source | 5816 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0137 | `frmKard.resx` | WinForms/resource XML | 5814 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0138 | `frmKard.vb` | VB.NET source | 1340 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0139 | `frmKardIme.Designer.vb` | VB.NET source | 3482 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0140 | `frmKardIme.resx` | WinForms/resource XML | 8869 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0141 | `frmKardIme.vb` | VB.NET source | 2010 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0142 | `frmKardkol.Designer.vb` | VB.NET source | 5961 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0143 | `frmKardkol.resx` | WinForms/resource XML | 6008 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0144 | `frmKardkol.vb` | VB.NET source | 4819 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0145 | `frmKardPro.Designer.vb` | VB.NET source | 5356 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0146 | `frmKardPro.resx` | WinForms/resource XML | 6212 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0147 | `frmKardPro.vb` | VB.NET source | 68384 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0148 | `frmKardRw.designer.vb` | VB.NET source | 19908 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0149 | `frmKardRw.resx` | WinForms/resource XML | 6843 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0150 | `frmKardRw.vb` | VB.NET source | 122643 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0151 | `frmKardSobarica.Designer.vb` | VB.NET source | 4808 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0152 | `frmKardSobarica.resx` | WinForms/resource XML | 5814 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0153 | `frmKardSobarica.vb` | VB.NET source | 556 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0154 | `frmKonta.Designer.vb` | VB.NET source | 13727 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0155 | `frmKonta.resx` | WinForms/resource XML | 28051 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0156 | `frmKonta.vb` | VB.NET source | 1672 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0157 | `frmKontaP.Designer.vb` | VB.NET source | 4841 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0158 | `frmKontaP.resx` | WinForms/resource XML | 6358 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0159 | `frmKontaP.vb` | VB.NET source | 1236 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0160 | `frmKursna.Designer.vb` | VB.NET source | 3358 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0161 | `frmKursna.resx` | WinForms/resource XML | 5814 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0162 | `frmKursna.vb` | VB.NET source | 1171 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0163 | `frmlic.Designer.vb` | VB.NET source | 4164 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0164 | `frmlic.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0165 | `frmlic.vb` | VB.NET source | 1386 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0166 | `frmLogin.Designer.vb` | VB.NET source | 16158 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0167 | `frmLogin.resx` | WinForms/resource XML | 40831 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0168 | `frmLogin.vb` | VB.NET source | 23088 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0169 | `frmMail.Designer.vb` | VB.NET source | 8055 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0170 | `frmMail.resx` | WinForms/resource XML | 6012 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0171 | `frmMail.vb` | VB.NET source | 10178 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0172 | `frmMailKonfig.Designer.vb` | VB.NET source | 11853 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0173 | `frmMailKonfig.resx` | WinForms/resource XML | 5814 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0174 | `frmMailKonfig.vb` | VB.NET source | 4760 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0175 | `frmNapomene.Designer.vb` | VB.NET source | 8383 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0176 | `frmNapomene.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0177 | `frmNapomene.vb` | VB.NET source | 6125 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0178 | `frmNoviIzvor.Designer.vb` | VB.NET source | 2657 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0179 | `frmNoviIzvor.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0180 | `frmNoviIzvor.vb` | VB.NET source | 2187 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0181 | `frmNoviTip.Designer.vb` | VB.NET source | 2598 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0182 | `frmNoviTip.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0183 | `frmNoviTip.vb` | VB.NET source | 2181 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0184 | `frmOdjava1.Designer.vb` | VB.NET source | 37090 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0185 | `frmOdjava1.resx` | WinForms/resource XML | 5814 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0186 | `frmOdjava1.vb` | VB.NET source | 43830 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0187 | `frmodjG.Designer.vb` | VB.NET source | 3697 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0188 | `frmodjG.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0189 | `frmodjG.vb` | VB.NET source | 310 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0190 | `frmPartner1.Designer.vb` | VB.NET source | 36491 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0191 | `frmPartner1.resx` | WinForms/resource XML | 5814 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0192 | `frmPartner1.vb` | VB.NET source | 16865 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0193 | `frmPartneri.Designer.vb` | VB.NET source | 34908 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0194 | `frmPartneri.resx` | WinForms/resource XML | 5814 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0195 | `frmPartneri.vb` | VB.NET source | 15386 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0196 | `frmPlacanje.Designer.vb` | VB.NET source | 74394 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0197 | `frmPlacanje.resx` | WinForms/resource XML | 290006 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0198 | `frmPlacanje.vb` | VB.NET source | 314584 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0199 | `frmPlacanjePo.Designer.vb` | VB.NET source | 14623 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0200 | `frmPlacanjePo.resx` | WinForms/resource XML | 5814 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0201 | `frmPlacanjePo.vb` | VB.NET source | 3486 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0202 | `frmPlacanjeSlozeno.Designer.vb` | VB.NET source | 19384 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0203 | `frmPlacanjeSlozeno.resx` | WinForms/resource XML | 5814 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0204 | `frmPlacanjeSlozeno.vb` | VB.NET source | 8556 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0205 | `frmPlacanjeTarifa.Designer.vb` | VB.NET source | 6122 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0206 | `frmPlacanjeTarifa.resx` | WinForms/resource XML | 5814 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0207 | `frmPlacanjeTarifa.vb` | VB.NET source | 3396 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0208 | `frmPlacproc.Designer.vb` | VB.NET source | 8314 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0209 | `frmPlacproc.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0210 | `frmPlacproc.vb` | VB.NET source | 2047 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0211 | `frmPlati1.Designer.vb` | VB.NET source | 83254 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0212 | `frmPlati1.resx` | WinForms/resource XML | 2128709 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0213 | `frmPlati1.vb` | VB.NET source | 45374 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0214 | `frmPosBaze.designer.vb` | VB.NET source | 29184 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0215 | `frmPosBaze.resx` | WinForms/resource XML | 127776 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0216 | `frmPosBaze.vb` | VB.NET source | 11645 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0217 | `frmpostavke.Designer.vb` | VB.NET source | 188491 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0218 | `frmpostavke.resx` | WinForms/resource XML | 294780 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0219 | `frmpostavke.vb` | VB.NET source | 83304 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0220 | `frmPredracun.Designer.vb` | VB.NET source | 48571 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0221 | `frmPredracun.resx` | WinForms/resource XML | 586826 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0222 | `frmPredracun.vb` | VB.NET source | 39851 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0223 | `frmPrijava1.Designer.vb` | VB.NET source | 33776 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0224 | `frmPrijava1.resx` | WinForms/resource XML | 6019 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0225 | `frmPrijava1.vb` | VB.NET source | 59313 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0226 | `frmPrijavaBoravkaPodaci.Designer.vb` | VB.NET source | 8190 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0227 | `frmPrijavaBoravkaPodaci.resx` | WinForms/resource XML | 5814 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0228 | `frmPrijavaBoravkaPodaci.vb` | VB.NET source | 778 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0229 | `frmPrijavaGostiKucice.resx` | WinForms/resource XML | 6183 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0230 | `frmPrijavaGostiKucice.vb` | VB.NET source | 76518 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0231 | `frmPrijavaGostiUnos.resx` | WinForms/resource XML | 202295 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0232 | `frmPrijavaGostiUnos.vb` | VB.NET source | 136656 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0233 | `frmPrikazNocenja1.Designer.vb` | VB.NET source | 5300 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0234 | `frmPrikazNocenja1.resx` | WinForms/resource XML | 5814 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0235 | `frmPrikazNocenja1.vb` | VB.NET source | 1557 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0236 | `frmPrikazNocenja2.Designer.vb` | VB.NET source | 4304 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0237 | `frmPrikazNocenja2.resx` | WinForms/resource XML | 5814 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0238 | `frmPrikazNocenja2.vb` | VB.NET source | 2897 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0239 | `frmRaccopy.Designer.vb` | VB.NET source | 5207 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0240 | `frmRaccopy.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0241 | `frmRaccopy.vb` | VB.NET source | 11539 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0242 | `frmRacun.Designer.vb` | VB.NET source | 8934 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0243 | `frmRacun.resx` | WinForms/resource XML | 20520 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0244 | `frmRacun.vb` | VB.NET source | 641 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0245 | `frmRacuni.Designer.vb` | VB.NET source | 115438 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0246 | `frmRacuni.resx` | WinForms/resource XML | 6426 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0247 | `frmRacuni.vb` | VB.NET source | 227482 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0248 | `frmRadnik.Designer.vb` | VB.NET source | 13865 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0249 | `frmRadnik.resx` | WinForms/resource XML | 12265 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0250 | `frmRadnik.vb` | VB.NET source | 3425 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0251 | `frmReport.Designer.vb` | VB.NET source | 2130 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0252 | `frmReport.resx` | WinForms/resource XML | 5814 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0253 | `frmReport.vb` | VB.NET source | 361 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0254 | `frmReportPrijavaBoravka.Designer.vb` | VB.NET source | 2358 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0255 | `frmReportPrijavaBoravka.resx` | WinForms/resource XML | 5814 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0256 | `frmReportPrijavaBoravka.vb` | VB.NET source | 2120 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0257 | `frmReportRacun.Designer.vb` | VB.NET source | 2150 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0258 | `frmReportRacun.resx` | WinForms/resource XML | 5814 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0259 | `frmReportRacun.vb` | VB.NET source | 728 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0260 | `frmReportRezervacije.Designer.vb` | VB.NET source | 2174 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0261 | `frmReportRezervacije.resx` | WinForms/resource XML | 5814 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0262 | `frmReportRezervacije.vb` | VB.NET source | 397 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0263 | `frmReportTel.Designer.vb` | VB.NET source | 2430 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0264 | `frmReportTel.resx` | WinForms/resource XML | 249104 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0265 | `frmReportTel.vb` | VB.NET source | 638 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0266 | `frmReportTurist.Designer.vb` | VB.NET source | 2776 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0267 | `frmReportTurist.resx` | WinForms/resource XML | 5814 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0268 | `frmReportTurist.vb` | VB.NET source | 1479 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0269 | `frmReportTuristicki.Designer.vb` | VB.NET source | 2168 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0270 | `frmReportTuristicki.resx` | WinForms/resource XML | 5814 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0271 | `frmReportTuristicki.vb` | VB.NET source | 382 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0272 | `frmRezervacije.Designer.vb` | VB.NET source | 14490 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0273 | `frmRezervacije.resx` | WinForms/resource XML | 49288 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0274 | `frmRezervacije.vb` | VB.NET source | 69079 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0275 | `frmRezervacije_unos.Designer.vb` | VB.NET source | 95032 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0276 | `frmRezervacije_unos.resx` | WinForms/resource XML | 25080 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0277 | `frmRezervacije_unos.vb` | VB.NET source | 59180 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0278 | `frmRezervacije1.Designer.vb` | VB.NET source | 2865 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0279 | `frmRezervacije1.resx` | WinForms/resource XML | 5814 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0280 | `frmRezervacije1.vb` | VB.NET source | 218 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0281 | `frmRezervacijeNove.Designer.vb` | VB.NET source | 19257 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0282 | `frmRezervacijeNove.resx` | WinForms/resource XML | 5814 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0283 | `frmRezervacijeNove.vb` | VB.NET source | 21604 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0284 | `frmRezervacijePrebaci.Designer.vb` | VB.NET source | 15355 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0285 | `frmRezervacijePrebaci.resx` | WinForms/resource XML | 5814 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0286 | `frmRezervacijePrebaci.vb` | VB.NET source | 43894 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0287 | `frmRezervacijePregled.Designer.vb` | VB.NET source | 18895 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0288 | `frmRezervacijePregled.resx` | WinForms/resource XML | 5814 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0289 | `frmRezervacijePregled.vb` | VB.NET source | 38705 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0290 | `frmSobaInfo.Designer.vb` | VB.NET source | 77245 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0291 | `frmSobaInfo.resx` | WinForms/resource XML | 302011 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0292 | `frmSobaInfo.vb` | VB.NET source | 101734 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0293 | `frmSobaInfoPromjena.Designer.vb` | VB.NET source | 6282 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0294 | `frmSobaInfoPromjena.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0295 | `frmSobaInfoPromjena.vb` | VB.NET source | 2965 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0296 | `frmSobaistorija.Designer.vb` | VB.NET source | 4806 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0297 | `frmSobaistorija.resx` | WinForms/resource XML | 8869 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0298 | `frmSobaistorija.vb` | VB.NET source | 4311 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0299 | `frmSobarice.Designer.vb` | VB.NET source | 3319 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0300 | `frmSobarice.resx` | WinForms/resource XML | 489546 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0301 | `frmSobarice.vb` | VB.NET source | 320 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0302 | `frmSobe.resx` | WinForms/resource XML | 329096 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0303 | `frmSobe.vb` | VB.NET source | 34705 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0304 | `frmSobe_Set.resx` | WinForms/resource XML | 249661 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0305 | `frmSobe_Set.vb` | VB.NET source | 20293 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0306 | `frmTarife.Designer.vb` | VB.NET source | 41516 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0307 | `frmTarife.resx` | WinForms/resource XML | 6545 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0308 | `frmTarife.vb` | VB.NET source | 57116 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0309 | `frmTelefon.resx` | WinForms/resource XML | 6012 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0310 | `frmTelefon.vb` | VB.NET source | 26029 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0311 | `frmTelefonskiImenik.Designer.vb` | VB.NET source | 26407 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0312 | `frmTelefonskiImenik.resx` | WinForms/resource XML | 6012 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0313 | `frmTelefonskiImenik.vb` | VB.NET source | 17182 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0314 | `frmTelefonskiImenikUnos.Designer.vb` | VB.NET source | 13412 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0315 | `frmTelefonskiImenikUnos.resx` | WinForms/resource XML | 5814 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0316 | `frmTelefonskiImenikUnos.vb` | VB.NET source | 10193 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0317 | `frmTelPostavke.Designer.vb` | VB.NET source | 14885 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0318 | `frmTelPostavke.resx` | WinForms/resource XML | 7972 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0319 | `frmTelPostavke.vb` | VB.NET source | 6205 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0320 | `frmTroskovi.Designer.vb` | VB.NET source | 23084 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0321 | `frmTroskovi.resx` | WinForms/resource XML | 266151 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0322 | `frmTroskovi.vb` | VB.NET source | 47652 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0323 | `frmTroskoviNoc.Designer.vb` | VB.NET source | 13246 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0324 | `frmTroskoviNoc.resx` | WinForms/resource XML | 265064 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0325 | `frmTroskoviNoc.vb` | VB.NET source | 18096 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0326 | `frmTrosSvi.Designer.vb` | VB.NET source | 5647 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0327 | `frmTrosSvi.resx` | WinForms/resource XML | 296146 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0328 | `frmTrosSvi.vb` | VB.NET source | 1328 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0329 | `frmWeb.Designer.vb` | VB.NET source | 8376 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0330 | `frmWeb.resx` | WinForms/resource XML | 73550 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0331 | `frmWeb.vb` | VB.NET source | 2972 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0332 | `frmZurnal.Designer.vb` | VB.NET source | 3852 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0333 | `frmZurnal.resx` | WinForms/resource XML | 5814 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0334 | `frmZurnal.vb` | VB.NET source | 11383 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0335 | `frmZurnal1.Designer.vb` | VB.NET source | 15025 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0336 | `frmZurnal1.resx` | WinForms/resource XML | 6019 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0337 | `frmZurnal1.vb` | VB.NET source | 60906 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0338 | `funkcije.vb` | VB.NET source | 28836 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0339 | `gost.vb` | VB.NET source | 0 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0340 | `gostDokument.xml` | XML config/report template | 508 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0341 | `GostiLista.xml` | XML config/report template | 1314 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0342 | `GostiListaTurist.xml` | XML config/report template | 1715 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0343 | `GostiListing.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 check-in/stay/guest | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0344 | `GostiListing.vb` | VB.NET source | 6268 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0345 | `GostiListingGrupa.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 check-in/stay/guest | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0346 | `GostiListingGrupa.vb` | VB.NET source | 6298 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0347 | `GostiListingrpt.Designer.vb` | VB.NET source | 2211 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0348 | `GostiListingrpt.resx` | WinForms/resource XML | 5814 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0349 | `GostiListingrpt.vb` | VB.NET source | 821 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0350 | `HotelStatistika.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0351 | `HotelStatistika.vb` | VB.NET source | 5590 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0352 | `HotelStatistika1.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0353 | `HotelStatistika1.vb` | VB.NET source | 5596 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0354 | `HOTELVIP 20150602 0904.sql` | SQL schema/data/procedure | 581959 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | tables/procedures/functions/writes/statuses/seed data |  |  |  |  |
| SRC-0355 | `iMediaIzvjestaj.Designer.vb` | VB.NET source | 120759 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0356 | `iMediaIzvjestaj.vb` | VB.NET source | 56 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0357 | `iMediaIzvjestaj.xsc` | DataSet metadata | 361 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0358 | `iMediaIzvjestaj.xsd` | DataSet schema | 15372 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0359 | `iMediaIzvjestaj.xss` | DataSet metadata | 1064 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0360 | `IzvjestajNaplata.xml` | XML config/report template | 781 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0361 | `IzvjestajRezervacija.xml` | XML config/report template | 935 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0362 | `IzvjestajRezervacijaHeader.xml` | XML config/report template | 1680 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0363 | `IzvjestajRezervacijaPoj.xml` | XML config/report template | 4224 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0364 | `IzvjestajRezervacijaPojHead.xml` | XML config/report template | 870 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0365 | `izvjestajStatistika.xml` | XML config/report template | 846 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0366 | `kard_imedia.vb` | VB.NET source | 20665 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0367 | `kard_imedia1.vb` | VB.NET source | 40914 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0368 | `konfiguracija.vb` | VB.NET source | 1188 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0369 | `Kursna.xml` | XML config/report template | 199 | P1 | P1 admin/integrations/accounting | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0370 | `localhostFS_rr.in` | Fiscal/device input template | 142 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0371 | `localhosttrigger.in` | Fiscal/device input template | 10 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0372 | `ModuleKod.vb` | VB.NET source | 215360 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0373 | `My Project/app.manifest` | Assembly manifest | 1478 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0374 | `My Project/Application.Designer.vb` | VB.NET source | 1734 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0375 | `My Project/Application.myapp` | VB application metadata | 514 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0376 | `My Project/Resources.Designer.vb` | VB.NET source | 5924 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0377 | `My Project/Resources.resx` | WinForms/resource XML | 8611 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0378 | `My Project/Settings.Designer.vb` | VB.NET source | 27207 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0379 | `My Project/Settings.settings` | VB settings | 6774 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0380 | `NocenjeSum.xml` | XML config/report template | 1324 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0381 | `novaBazaJHotel 20150602 0848.sql` | SQL schema/data/procedure | 633981 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | tables/procedures/functions/writes/statuses/seed data |  |  |  |  |
| SRC-0382 | `obj/Debug/HotelPRO.xml` | XML config/report template | 1583 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0383 | `obj/Debug/printFooter.xml` | XML config/report template | 851 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0384 | `obj/Debug/printGore.xml` | XML config/report template | 1227 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0385 | `obj/Debug/printSredina.xml` | XML config/report template | 1462 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0386 | `obj/Debug/Radna.DepozitReport.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0387 | `obj/Debug/Radna.DnevniIzvjestaj.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0388 | `obj/Debug/Radna.DnevniIzvjestajSub.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0389 | `obj/Debug/Radna.GostiListing.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 check-in/stay/guest | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0390 | `obj/Debug/Radna.HotelStatistika.rpt` | Crystal report binary/artifact | 49152 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0391 | `obj/Debug/Radna.OdjavaReport.rpt` | Crystal report binary/artifact | 49152 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0392 | `obj/Debug/Radna.PlacanjeMali.rpt` | Crystal report binary/artifact | 49152 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0393 | `obj/Debug/Radna.PrijavaBoravista.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 check-in/stay/guest | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0394 | `obj/Debug/Radna.PrijavaTurist.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 check-in/stay/guest | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0395 | `obj/Debug/Radna.RacunDetalji.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0396 | `obj/Debug/Radna.RacunDetaljiNocenje.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0397 | `obj/Debug/Radna.RacunPlacanja.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0398 | `obj/Debug/Radna.RacunPodaci.rpt` | Crystal report binary/artifact | 32768 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0399 | `obj/Debug/Radna.restoranDorucak.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0400 | `obj/Debug/Radna.RezervacijeListing.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0401 | `obj/Debug/Radna.rptBlank.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0402 | `obj/Debug/Radna.rptPlacanjeFooter.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0403 | `obj/Debug/Radna.rptPlacanjeGrupno.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0404 | `obj/Debug/Radna.rptPlacanjeGrupnoSUB.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0405 | `obj/Debug/Radna.rptPlacanjePojedininacno.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0406 | `obj/Debug/Radna.rptPlacanjeSUB.rpt` | Crystal report binary/artifact | 49152 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0407 | `obj/Debug/Radna.rptPlacanjeSUBStorno.rpt` | Crystal report binary/artifact | 49152 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0408 | `obj/Debug/Radna.rptPlacanjeSUBvar.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0409 | `obj/Debug/Radna.rptPrijavaBoravka1.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 check-in/stay/guest | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0410 | `obj/Debug/Radna.rptPrintReportSub.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0411 | `obj/Debug/Radna.rptRezervacijePoj1.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0412 | `obj/Debug/Radna.rptRezervacijePojHead.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0413 | `obj/Debug/Radna.rptRezervacijeZauzete1.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0414 | `obj/Debug/Radna.rptRezervacijeZauzeteSub.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0415 | `obj/Debug/Radna.rptRezervacijeZauzeteSub1.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0416 | `obj/Debug/Radna.sobaricaJutarnji.rpt` | Crystal report binary/artifact | 16384 | P0 | P0/P1 room/housekeeping | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0417 | `obj/Debug/Radna.sobaricaJutarnji1.rpt` | Crystal report binary/artifact | 16384 | P0 | P0/P1 room/housekeeping | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0418 | `obj/Debug/Radna.TelReport.rpt` | Crystal report binary/artifact | 49152 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0419 | `obj/Debug/Radna.TurizamSuma.rpt` | Crystal report binary/artifact | 32768 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0420 | `obj/Debug/Radna.vbproj.FileListAbsolute.txt` | Text artifact | 8412 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0421 | `obj/Radna.vbproj.FileList.txt` | Text artifact | 3473 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0422 | `obj/Radna.vbproj.FileListAbsolute.txt` | Text artifact | 11949 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0423 | `obj/Release/HotelPRO.xml` | XML config/report template | 1677 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0424 | `obj/Release/Radna.vbproj.FileListAbsolute.txt` | Text artifact | 7128 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0425 | `obj/x86/Release/HotelPRO.xml` | XML config/report template | 1583 | P1 | P1 hardware/telephony | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0426 | `obj/x86/Release/Radna.vbproj.FileListAbsolute.txt` | Text artifact | 8022 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0427 | `OdjavaReport.rpt` | Crystal report binary/artifact | 229376 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0428 | `OdjavaReport.vb` | VB.NET source | 5572 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0429 | `OdjavaReportrpt.Designer.vb` | VB.NET source | 2456 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0430 | `OdjavaReportrpt.resx` | WinForms/resource XML | 249107 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0431 | `OdjavaReportrpt.vb` | VB.NET source | 471 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0432 | `placanjeNacin.xml` | XML config/report template | 593 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0433 | `PrijavaBoravista.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 check-in/stay/guest | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0434 | `PrijavaBoravista.vb` | VB.NET source | 7666 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0435 | `PrijavaTurist.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 check-in/stay/guest | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0436 | `PrijavaTurist.vb` | VB.NET source | 5578 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0437 | `printEvidencija.xml` | XML config/report template | 2211 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0438 | `printFooter.xml` | XML config/report template | 1003 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0439 | `printGore.xml` | XML config/report template | 1756 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0440 | `printPredracunDetalji.xml` | XML config/report template | 1008 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0441 | `printPredracunHead.xml` | XML config/report template | 1008 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0442 | `printSredina.xml` | XML config/report template | 1838 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0443 | `RacunDetalji.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0444 | `RacunDetalji.vb` | VB.NET source | 5572 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0445 | `RacunDetaljiNocenje.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0446 | `RacunDetaljiNocenje.vb` | VB.NET source | 5614 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0447 | `RacunPlacanja.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0448 | `RacunPlacanja.vb` | VB.NET source | 5578 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0449 | `RacunPodaci.rpt` | Crystal report binary/artifact | 32768 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0450 | `RacunPodaci.vb` | VB.NET source | 5566 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0451 | `Radna.sln` | Visual Studio solution | 1248 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0452 | `Radna.vbproj` | VB project | 69453 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0453 | `README.md` | Documentation | 415 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0454 | `restoranDorucak.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0455 | `restoranDorucak.vb` | VB.NET source | 6286 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0456 | `restoranDorucak.xml` | XML config/report template | 1011 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0457 | `RezervacijeListing.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0458 | `RezervacijeListing.vb` | VB.NET source | 6304 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0459 | `rptAvans.rpt` | Crystal report binary/artifact | 163840 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0460 | `rptAvans.vb` | VB.NET source | 5548 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0461 | `rptBlank.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0462 | `rptBlank.vb` | VB.NET source | 5548 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0463 | `rptDnevniIzvje.Designer.vb` | VB.NET source | 2218 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0464 | `rptDnevniIzvje.resx` | WinForms/resource XML | 5814 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0465 | `rptDnevniIzvje.vb` | VB.NET source | 2694 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0466 | `rptDnevniIzvje1.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0467 | `rptDnevniIzvje1.vb` | VB.NET source | 5933 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0468 | `rptDnevniIzvjestaj.Designer.vb` | VB.NET source | 4322 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0469 | `rptDnevniIzvjestaj.resx` | WinForms/resource XML | 5814 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0470 | `rptDnevniIzvjestaj.vb` | VB.NET source | 472 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0471 | `rptDnevniSve.Designer.vb` | VB.NET source | 2208 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0472 | `rptDnevniSve.resx` | WinForms/resource XML | 5814 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0473 | `rptDnevniSve.vb` | VB.NET source | 632 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0474 | `rptDnevniSve2.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0475 | `rptDnevniSve2.vb` | VB.NET source | 6617 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0476 | `rptfrmdetalji.Designer.vb` | VB.NET source | 3501 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0477 | `rptfrmdetalji.resx` | WinForms/resource XML | 6230 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0478 | `rptfrmdetalji.vb` | VB.NET source | 6122 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0479 | `rptPlacanjeFooter.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0480 | `rptPlacanjeFooter.vb` | VB.NET source | 5602 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0481 | `rptPlacanjeGrupno.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0482 | `rptPlacanjeGrupno.vb` | VB.NET source | 5190 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0483 | `rptPlacanjeGrupno1.Designer.vb` | VB.NET source | 2166 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0484 | `rptPlacanjeGrupno1.resx` | WinForms/resource XML | 5814 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0485 | `rptPlacanjeGrupno1.vb` | VB.NET source | 772 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0486 | `rptPlacanjeGrupnoSUB.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0487 | `rptPlacanjeGrupnoSUB.vb` | VB.NET source | 5205 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0488 | `rptPlacanjePojedinacno1.Designer.vb` | VB.NET source | 2271 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0489 | `rptPlacanjePojedinacno1.resx` | WinForms/resource XML | 5814 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0490 | `rptPlacanjePojedinacno1.vb` | VB.NET source | 1345 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0491 | `rptPlacanjeStornirano.Designer.vb` | VB.NET source | 2278 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0492 | `rptPlacanjeStornirano.resx` | WinForms/resource XML | 5814 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0493 | `rptPlacanjeStornirano.vb` | VB.NET source | 808 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0494 | `rptPlacanjeSUB.rpt` | Crystal report binary/artifact | 32768 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0495 | `rptPlacanjeSUB.vb` | VB.NET source | 5927 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0496 | `rptPlacanjeSUBStorno.rpt` | Crystal report binary/artifact | 32768 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0497 | `rptPlacanjeSUBStorno.vb` | VB.NET source | 5963 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0498 | `rptPlacanjeSUBvar.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0499 | `rptPlacanjeSUBvar.vb` | VB.NET source | 5602 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0500 | `rptPredr.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0501 | `rptPredr.vb` | VB.NET source | 5893 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0502 | `rptPredracun.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0503 | `rptPredracun.vb` | VB.NET source | 5915 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0504 | `rptPredracunSUB.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0505 | `rptPredracunSUB.vb` | VB.NET source | 5590 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0506 | `rptPrijavaBoravka.Designer.vb` | VB.NET source | 5885 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0507 | `rptPrijavaBoravka.resx` | WinForms/resource XML | 5814 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0508 | `rptPrijavaBoravka.vb` | VB.NET source | 4606 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0509 | `rptPrijavaBoravka1.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 check-in/stay/guest | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0510 | `rptPrijavaBoravka1.vb` | VB.NET source | 5608 | P0 | P0 check-in/stay/guest | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0511 | `rptPrintReportSub.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0512 | `rptPrintReportSub.vb` | VB.NET source | 5602 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0513 | `rptRacun.rdlc` | RDLC report definition | 62131 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0514 | `rptRacun1.rdlc` | RDLC report definition | 48270 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0515 | `rptRacunDet.rdlc` | RDLC report definition | 25449 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0516 | `rptRacunFrm.Designer.vb` | VB.NET source | 5544 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0517 | `rptRacunFrm.resx` | WinForms/resource XML | 6230 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0518 | `rptRacunFrm.vb` | VB.NET source | 22803 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0519 | `rptRezervacije.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0520 | `rptRezervacije.vb` | VB.NET source | 5584 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0521 | `rptRezervacijePoj.Designer.vb` | VB.NET source | 2168 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0522 | `rptRezervacijePoj.resx` | WinForms/resource XML | 5814 | P0 | P0 booking/reservation | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0523 | `rptRezervacijePoj.vb` | VB.NET source | 803 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0524 | `rptRezervacijePoj1.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0525 | `rptRezervacijePoj1.vb` | VB.NET source | 5608 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0526 | `rptRezervacijePojHead.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0527 | `rptRezervacijePojHead.vb` | VB.NET source | 5626 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0528 | `rptRezervacijeZauzete1.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0529 | `rptRezervacijeZauzete1.vb` | VB.NET source | 5632 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0530 | `rptRezervacijeZauzeteSub.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0531 | `rptRezervacijeZauzeteSub.vb` | VB.NET source | 5227 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0532 | `rptRezervacijeZauzeteSub1.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 booking/reservation | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0533 | `rptRezervacijeZauzeteSub1.vb` | VB.NET source | 5650 | P0 | P0 booking/reservation | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0534 | `rptTroskovi.rpt` | Crystal report binary/artifact | 16384 | P0 | P0 checkout/payment/invoice/expense | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0535 | `rptTroskovi.vb` | VB.NET source | 5566 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0536 | `rptTrtaxe.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0537 | `rptTrtaxe.vb` | VB.NET source | 5554 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0538 | `rptTurdrz.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0539 | `rptTurdrz.vb` | VB.NET source | 5554 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0540 | `rptTuristicka.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0541 | `rptTuristicka.vb` | VB.NET source | 5578 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0542 | `rptZaglavljePodaci.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0543 | `rptZaglavljePodaci.vb` | VB.NET source | 5608 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0544 | `rptZaglavljeSLIKA.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0545 | `rptZaglavljeSLIKA.vb` | VB.NET source | 5602 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0546 | `Rslobodne.xml` | XML config/report template | 1016 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0547 | `set.xml` | XML config/report template | 234 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0548 | `Settings.vb` | VB.NET source | 498 | P0 | P0 infrastructure/database/shared | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0549 | `sobaricaJutarnji.rpt` | Crystal report binary/artifact | 16384 | P0 | P0/P1 room/housekeeping | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0550 | `sobaricaJutarnji.vb` | VB.NET source | 6292 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0551 | `sobaricaJutarnji1.rpt` | Crystal report binary/artifact | 16384 | P0 | P0/P1 room/housekeeping | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0552 | `sobaricaJutarnji1.vb` | VB.NET source | 5951 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0553 | `sobaricaShema.xml` | XML config/report template | 1458 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0554 | `sobaricaShema1.xml` | XML config/report template | 1866 | P0 | P0/P1 room/housekeeping | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0555 | `SplashScreen1.Designer.vb` | VB.NET source | 4477 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0556 | `SplashScreen1.resx` | WinForms/resource XML | 79781 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0557 | `SplashScreen1.vb` | VB.NET source | 670 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0558 | `TelefonskiRacun.xml` | XML config/report template | 1249 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0559 | `TelReport.rpt` | Crystal report binary/artifact | 16384 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0560 | `TelReport.vb` | VB.NET source | 5554 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0561 | `trenutniDnevni.xml` | XML config/report template | 1444 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0562 | `troskoviVrste.xml` | XML config/report template | 2592 | P0 | P0 checkout/payment/invoice/expense | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0563 | `TurizamSuma.rpt` | Crystal report binary/artifact | 32768 | P1 | P1 reports/tourist records | `BINARY_OR_TOOLING` | report queries/fields/grouping/output semantics |  |  |  |  |
| SRC-0564 | `TurizamSuma.vb` | VB.NET source | 6327 | P1 | P1 reports/tourist records | `NOT_REVIEWED` | classes/functions/events/sql/callers/statuses/rules |  |  |  |  |
| SRC-0565 | `UpgradeLog.XML` | XML config/report template | 7535 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0566 | `UpgradeLog2.XML` | XML config/report template | 1186 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0567 | `UpgradeLog3.XML` | XML config/report template | 1182 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
| SRC-0568 | `UpgradeLog4.XML` | XML config/report template | 1190 | P2/UNKNOWN | UNKNOWN | `NOT_REVIEWED` | config/resources/schema/business strings/references |  |  |  |  |
