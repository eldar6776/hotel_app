'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'

export default function Breadcrumbs() {
  const pathname = usePathname()
  const segments = pathname.split('/').filter(Boolean)

  if (segments.length === 0) return null

  const labels: Record<string, string> = {
    dashboard: 'Dashboard',
    rooms: 'Sobe',
    bookings: 'Rezervacije',
    guests: 'Gosti',
    folio: 'Folio',
    housekeeping: 'Housekeeping',
    reports: 'Izvjestaji',
    settings: 'Postavke',
    profile: 'Profil',
    login: 'Prijava',
  }

  return (
    <nav className="hidden md:flex items-center gap-1 text-sm text-text-secondary">
      {segments.map((segment, index) => {
        const href = '/' + segments.slice(0, index + 1).join('/')
        const isLast = index === segments.length - 1
        const label = labels[segment] || segment

        return (
          <span key={href} className="flex items-center gap-1">
            {index > 0 && (
              <span className="text-border">/</span>
            )}
            {isLast ? (
              <span className="text-text">{label}</span>
            ) : (
              <Link
                href={href}
                className="hover:text-text transition-colors"
              >
                {label}
              </Link>
            )}
          </span>
        )
      })}
    </nav>
  )
}
