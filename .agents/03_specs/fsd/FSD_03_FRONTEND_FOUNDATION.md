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

## 5. Animacije

- Hover efekti na karticama: `transform: scale(1.02)`, `transition: 200ms ease`
- Sidebar tranzicija: `width transition 300ms ease`
- Page tranzicije: fade-in, 150ms
- Loading skeleton animacije umjesto spinnera

## 6. Restrikcije

- NE koristiti component biblioteke (MUI, Ant Design) — custom komponente sa Tailwind
- Svaka stranica mora imati SEO meta tagove
- Svaki interaktivni element mora imati unikatan `id` za testiranje
- Slike: koristiti `next/image` za optimizaciju
- Dark Mode mora biti persistent (localStorage)
