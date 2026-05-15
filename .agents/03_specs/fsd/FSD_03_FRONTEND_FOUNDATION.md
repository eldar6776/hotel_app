# FSD 3: Frontend Foundation

Status: AUTHORITATIVE
Last validated: 2026-05-15

## 1. Cilj

Kreirati modernu, premium Next.js web aplikaciju sa Tailwind CSS koja ce sluziti kao korisnicko sucelje za sve hotelske module. Aplikacija mora biti responzivna, sa Dark Mode podrskom, i mora izgledati "state of the art".

## 2. Projekat

Folder: `frontend/`
Framework: Next.js 15 (App Router)
Styling: Tailwind CSS v4
Tipografija: Google Fonts — Inter (sans-serif)
State Management: React Context + useState/useReducer (bez Redux/Zustand)
API Client: Axios sa interceptorima
Real-time: SignalR (@microsoft/signalr)

## 3. Design System

### 3.1 Boje (HSL paleta)

- Primary: `hsl(222, 47%, 31%)` — tamno plava (profesionalna)
- Accent: `hsl(38, 92%, 50%)` — zlatna (luksuzni detalji)
- Success: `hsl(152, 69%, 31%)`
- Warning: `hsl(38, 92%, 50%)`
- Error: `hsl(0, 72%, 51%)`
- Background Light: `hsl(220, 14%, 96%)`
- Background Dark: `hsl(222, 47%, 11%)`
- Surface Light: `hsl(0, 0%, 100%)`
- Surface Dark: `hsl(222, 47%, 15%)`

### 3.2 Tipografija

- Font: Inter (Google Fonts)
- h1: 2rem, semibold
- h2: 1.5rem, semibold
- body: 0.875rem, regular
- small: 0.75rem, regular

### 3.3 Spacing

- Sidebar sirina: 280px (desktop), kolapsirana na 64px (mobile)
- Navbar visina: 64px
- Card padding: 24px
- Gap izmedju kartica: 16px

## 4. Layout

- Sidebar sa lijeve strane — navigacija po modulima
- Navbar na vrhu — korisnicke informacije, notifikacije, Dark Mode toggle
- Main area — sadrzaj aktivnog modula
- Mobile: hamburger meni, sidebar overlay

## 5. Legacy UI → Novi Frontend Mapping

### 5.1 Legacy WinForms forme i njihove zamjene

| Legacy forma | Opis | Nova ruta | Komponente |
|---|---|---|---|
| `frmSobe.vb` | Pregled soba sa statusima | `/rooms` | RoomGrid, RoomCard, FloorPlan |
| `frmSobaInfo.vb` | Detalji sobe | `/rooms/[id]` | RoomDetail modal |
| `frmRezervacije.vb` | Lista rezervacija | `/bookings` | BookingList, BookingFilters |
| `frmRezervacije_unos.vb` | Unos rezervacije | `/bookings/new` | BookingForm |
| `frmRezervacijePregled.vb` | Pregled rezervacija | `/bookings` | GanttCalendar |
| `frmPrijava1.vb` | Check-in forma | `/reception/check-in` | CheckInModal, ArrivalsPanel |
| `frmOdjava1.vb` | Check-out forma | `/reception/check-out` | CheckOutModal, DeparturesPanel |
| `frmTroskovi.vb` | Unos troskova | `/folio/[id]/charges` | ChargeForm, FolioView |
| `frmPlacanje.vb` | Naplata i racuni | `/billing` | PaymentForm, InvoicePreview |
| `frmGosti.vb` | Pregled gostiju | `/guests` | GuestList, GuestSearch |
| `frmPartneri.vb` | Partneri/agencije | `/partners` | PartnerList |
| `frmIzvjestaji.vb` | Izvjestaji | `/reports` | ReportsDashboard |
| `frmSobarice.vb` | Housekeeping | `/housekeeping` | HousekeepingPWA |
| `frmRacuni.vb` | Arhiva racuna | `/billing/invoices` | InvoiceArchive |
| `frmFiskalni.vb` | Fiskalizacija | `/billing/fiscalize` | FiscalDialog |
| `frmUpisKard.vb` | RFID kartice | `/reception/rfid` | RfidEncoder |

### 5.2 Legacy DataGridView → React komponente

Legacy sistem koristi DataGridView za sve tabele. Zamjene:
- `DataGridView` → Custom `<Table>` komponenta sa virtualizacijom
- `DataGridView` sa color-coded statusima → `<DataTable>` sa conditional styling
- `Crystal Reports` → QuestPDF (backend) + Chart.js (frontend)

### 5.3 Legacy meni sistem → Sidebar navigacija

Legacy koristi MainMenu kontrolu sa hijerarhijskim stavkama. Zamjena:
- Sidebar sa collapsible sekcijama
- Svaka stavka ima ikonu (lucide-react)
- Active state highlighting
- Keyboard shortcuts (Ctrl+K command palette)

## 6. Routing Struktura

```
frontend/src/app/
├── (auth)/
│   ├── login/page.tsx           — Login stranica
│   └── layout.tsx               — Auth layout (bez sidebara)
├── (app)/
│   ├── dashboard/page.tsx       — Dashboard sa KPI
│   ├── rooms/page.tsx           — Pregled soba
│   ├── rooms/[id]/page.tsx      — Detalji sobe
│   ├── bookings/page.tsx        — Rezervacije + Gantt
│   ├── bookings/new/page.tsx    — Nova rezervacija
│   ├── guests/page.tsx          — Gosti
│   ├── guests/[id]/page.tsx     — Profil gosta
│   ├── reception/page.tsx       — Recepcijski ekran
│   ├── folio/[id]/page.tsx      — Folio pregled
│   ├── billing/page.tsx         — Naplata
│   ├── housekeeping/page.tsx    — Housekeeping
│   ├── reports/page.tsx         — Izvjestaji
│   ├── settings/page.tsx        — Postavke
│   └── layout.tsx               — App layout (sidebar + navbar)
├── guest-portal/
│   ├── check-in/page.tsx        — Online check-in
│   ├── check-out/page.tsx       — Online check-out
│   └── digital-key/page.tsx     — Digitalni kljuc
├── room/[roomNumber]/
│   ├── menu/page.tsx            — Room service menu
│   └── orders/page.tsx          — Historija narudzbi
└── api/                          — API routes (auth proxy, webhooks)
```

## 7. State Management

### 7.1 Globalni state (React Context)

- `AuthContext` — korisnik, token, role
- `ThemeContext` — dark/light mode
- `SidebarContext` — expanded/collapsed state
- `HotelContext` — trenutni hotel (multi-tenant)

### 7.2 Server state (React Query / SWA)

- Koristiti `@tanstack/react-query` za:
  - Caching API odgovora
  - Auto-refetch na focus
  - Optimistic updates
  - Background refetch

### 7.3 Form state

- Koristiti `react-hook-form` + `zod` za validaciju
- Server-side validacija se uvijek radi paralelno

## 8. API Client Konfiguracija

### 8.1 Axios instance

```typescript
// frontend/src/lib/api/client.ts
const apiClient = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  timeout: 30000,
  headers: { 'Content-Type': 'application/json' },
})
```

### 8.2 Interceptori

- **Request**: Dodaje `Authorization: Bearer <token>` i `X-Hotel-Code` header
- **Response**: Na 401 automatski refresh token, na 403 redirect na /unauthorized
- **Error**: Standardizovani error format `{ code, message, details }`

### 8.3 Retry logika

- GET zahtjevi: 3 retry sa exponential backoff (1s, 2s, 4s)
- POST/PUT/DELETE: bez retry-a (idempotency nije garantovana)

## 9. Error Handling

### 9.1 Error boundary

- `<ErrorBoundary>` wrapper za sve page komponente
- Fallback UI sa "Pokusaj ponovo" dugmetom
- Logovanje errora na server (Sentry ili custom endpoint)

### 9.2 Validacija errora

- Form errors: inline prikaz ispod polja
- API errors: toast notifikacija + inline prikaz
- Network errors: full-screen error state sa retry

### 9.3 Loading state

- Skeleton loading za sve liste i tabele
- Spinner za akcije (submit, delete)
- Progress bar za duze operacije (export, import)

## 10. Animacije

- Hover efekti na karticama: `transform: scale(1.02)`, `transition: 200ms ease`
- Sidebar tranzicija: `width transition 300ms ease`
- Page tranzicije: fade-in, 150ms
- Loading skeleton animacije umjesto spinnera
- Toast notifikacije: slide-in from right, 5s auto-dismiss

## 11. Accessibility (WCAG 2.1 AA)

- Svi interaktivni elementi moraju biti accessible preko tastature
- ARIA labeli za sve ikone bez teksta
- Focus trap za modale
- Color contrast ratio minimum 4.5:1
- Screen reader podrska za sve tabele i forme
- Skip-to-content link na vrhu stranice

## 12. Performanse

- Lazy loading za sve rute osim `/dashboard`
- Image optimizacija sa `next/image`
- Code splitting po route segmentima
- Virtualizacija za liste sa 100+ stavki (react-window)
- Bundle size budzet: max 200KB initial load

## 13. Testiranje

- Unit testovi: Vitest + React Testing Library
- E2E testovi: Playwright
- Coverage target: 80%+ za utils i services
- Testovi za: auth flow, form validacija, API interceptori

## 14. Restrikcije

- NE koristiti component biblioteke (MUI, Ant Design) — custom komponente sa Tailwind
- Svaka stranica mora imati SEO meta tagove
- Svaki interaktivni element mora imati unikatan `id` za testiranje
- Slike: koristiti `next/image` za optimizaciju
- Dark Mode mora biti persistent (localStorage)
- NE koristiti next-auth ili Auth.js — custom JWT rjesenje
- NE koristiti Redux — React Context + React Query su dovoljni
