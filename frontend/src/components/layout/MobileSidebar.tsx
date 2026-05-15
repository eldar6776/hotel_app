'use client'

import Link from 'next/link'
import { X, Hotel } from 'lucide-react'
import { useSidebar } from '@/components/layout/SidebarContext'
import SidebarItem, { type SidebarItemData } from '@/components/layout/SidebarItem'
import {
  LayoutDashboard,
  DoorOpen,
  CalendarCheck,
  Users,
  Receipt,
  Sparkles,
  BarChart3,
  Settings,
} from 'lucide-react'

const sidebarItems: SidebarItemData[] = [
  { href: '/dashboard', icon: LayoutDashboard, label: 'Dashboard' },
  { href: '/rooms', icon: DoorOpen, label: 'Sobe' },
  { href: '/bookings', icon: CalendarCheck, label: 'Rezervacije' },
  { href: '/guests', icon: Users, label: 'Gosti' },
  { href: '/folio', icon: Receipt, label: 'Folio' },
  { href: '/housekeeping', icon: Sparkles, label: 'Housekeeping' },
  { href: '/reports', icon: BarChart3, label: 'Izvjestaji' },
  { href: '/settings', icon: Settings, label: 'Postavke' },
]

export default function MobileSidebar() {
  const { isMobileOpen, closeMobile } = useSidebar()

  return (
    <>
      <aside
        className={`fixed inset-y-0 left-0 z-50 w-[var(--sidebar-width)] border-r border-border bg-surface transition-transform duration-300 ease-in-out lg:hidden ${
          isMobileOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <div className="flex h-[var(--navbar-height)] items-center justify-between border-b border-border px-4">
          <Link
            href="/dashboard"
            onClick={closeMobile}
            className="flex items-center gap-3"
          >
            <Hotel className="h-6 w-6 text-primary-600" />
            <span className="text-lg font-semibold text-text">HotelPRO</span>
          </Link>
          <button
            onClick={closeMobile}
            className="rounded-lg p-2 text-text-secondary hover:bg-surface-secondary dark:hover:bg-surface-tertiary"
            aria-label="Zatvori meni"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <nav className="flex-1 space-y-1 overflow-y-auto p-3">
          {sidebarItems.map((item) => (
            <div key={item.href} onClick={closeMobile}>
              <SidebarItem item={item} />
            </div>
          ))}
        </nav>
      </aside>
    </>
  )
}
