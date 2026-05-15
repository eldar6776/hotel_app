# FSD 14: Interaktivni Help Sistem

## Status analize
- **Fajlovi za analizu:** Nema značajnog legacy koda (samo bazični ToolTips u `frmGosti.vb`, `frmPrijava1.vb`)
- **Status:** NEW PROPOSAL
- **Analizirao:** 2026-05-15 - Antigravity (Claude Sonnet 3.5)

## 1. Cilj i vizija
Cilj je implementirati moderan, "Context-Aware" sistem pomoći koji će smanjiti potrebu za obukom novih radnika i minimizirati greške pri unosu podataka. Sistem treba biti nenametljiv, ali lako dostupan u svakom trenutku.

## 2. Prijedlog arhitekture (Moderni pristup)

### 2.1 Help Mode (Globalni prekidač)
U gornjem desnom uglu aplikacije postojaće ikona upitnika. Kada se aktivira:
- Sva polja koja imaju dokumentaciju dobijaju suptilni "glow" ili ikonu `(i)`.
- Klikom na polje otvara se "Side-panel" ili "Tooltip" sa detaljnim objašnjenjem, poslovnim pravilom i primjerom unosa.

### 2.2 Guided Tours (Interaktivni vodiči)
Za kompleksne procese (npr. grupni check-out sa avansom), sistem će nuditi "Step-by-step" vodič:
1. "Prvo kliknite ovdje da odaberete grupu..." (highlight dugmeta)
2. "Sada potvrdite listu troškova..."
3. "Odaberite način plaćanja..."

## 3. Mapiranje sadržaja (Na bazi RE-FSD dokumenata)

Sistem pomoći će crpiti podatke iz centralizovanog JSON/Markdown repozitorija koji smo definisali kroz analizu:

| Modul | Primjer Help sadržaja |
|:---|:---|
| **Sobe** | Objašnjenje statusa (npr. "Šta znači OOO status?"). |
| **Gosti** | Zakonska obaveza za broj dokumenta (zašto je obavezno za strance). |
| **Naplata** | Razlika između fakture i fiskalnog isječka. |
| **PABX** | Kako se ručno dodaje telefonski poziv ako centrala nije dostupna. |

## 4. Tehnička implementacija (Next.js / React)

- **Frontend**: Korištenje biblioteka kao što su `Intro.js` ili `react-joyride` za vodiče.
- **Backend**: CMS (Content Management System) za help tekstove kako bi ih admin mogao ažurirati bez programiranja.
- **Search**: "Command Palette" (Ctrl+K) za brzu pretragu help tema.

## 5. Prednosti u odnosu na legacy
- **Multijezičnost**: Help može biti na jeziku recepcionera i na jeziku gosta.
- **Video Tutoriali**: Mogućnost ugradnje kratkih video snimaka unutar pomoći.
- **Deep Linking**: Svako polje ima svoj jedinstveni ID, što omogućava podršci da pošalje link koji direktno otvara pomoć za to polje.

## 6. Otvorena pitanja za USER-a
- **OQ-10-001**: Da li želiš da help sistem bude dostupan i gostima (npr. na njihovom digitalnom foliu) ili samo osoblju hotela?
- **OQ-10-002**: Da li bi želio integraciju sa tvojim specifikacijama (Markdown fajlovi) tako da se help automatski ažurira kada promijenimo dokumentaciju?

## 7. Proaktivna Pomoć (Nudge System)
Ovo je ključni element za robusnost. Sistem neće samo čekati na klik, već će pratiti ponašanje korisnika:

- **Idle Detection**: Ako je kursor fokusiran na polje više od 5-7 sekundi bez unosa (npr. "PDV stopa" ili "Broj fiskalnog isječka"), sistem diskretno prikazuje "Nudge" (mali balončić sa tekstom): *"Trebate pomoć oko PDV stope? Kliknite ovdje."*
- **Error Prediction**: Ako korisnik unese format koji često uzrokuje greške (npr. slovo u polju za ID broj), sistem odmah nudi objašnjenje formata prije nego što korisnik klikne "Save".
- **Heatmap Analytics**: Admin panel će moći vidjeti na kojim poljima se recepcioneri najviše zadržavaju, što je signal da dokumentacija za to polje treba biti bolja.

## 8. Zaključak RE faze
Sa ovim modulom, kompletirali smo reverzni inženjering cijelog hotela: od IoT senzora u sobama, preko baze gostiju i kompleksnih rezervacija, do finalne fiskalizacije i interaktivne pomoći. 

Spremni smo za fazu implementacije.
