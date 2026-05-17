'use client'

import { useSidebar } from '@/components/layout/SidebarContext'
import SidebarItem, { type SidebarItemData } from '@/components/layout/SidebarItem'
import {
  LayoutDashboard,
  DoorOpen,
  CalendarCheck,
  Users,
  BellRing,
  Receipt,
  Sparkles,
  BarChart3,
  Settings,
  ChevronLeft,
  ChevronRight,
  Hotel,
  Building2,
  Shield,
  Mail,
  Globe,
} from 'lucide-react'

const sidebarItems: SidebarItemData[] = [
  { href: '/dashboard', icon: LayoutDashboard, label: 'Dashboard', helpId: 'sidebar-dashboard' },
  { href: '/rooms', icon: DoorOpen, label: 'Sobe', helpId: 'sidebar-rooms' },
  { href: '/bookings', icon: CalendarCheck, label: 'Rezervacije', helpId: 'sidebar-bookings' },
  { href: '/reception', icon: BellRing, label: 'Recepcija' },
  { href: '/guests', icon: Users, label: 'Gosti' },
  { href: '/folio', icon: Receipt, label: 'Folio' },
  { href: '/housekeeping', icon: Sparkles, label: 'Housekeeping' },
  { href: '/reports', icon: BarChart3, label: 'Izvjestaji' },
  { href: '/settings', icon: Settings, label: 'Postavke', helpId: 'sidebar-settings' },
  { href: '/settings/tariffs', icon: Receipt, label: 'Tarife' },
  { href: '/settings/amenities', icon: Sparkles, label: 'Sadrzaji' },
  { href: '/settings/buildings', icon: Building2, label: 'Zgrade' },
  { href: '/settings/room-types', icon: DoorOpen, label: 'Tipovi soba' },
  { href: '/settings/employees', icon: Users, label: 'Zaposleni' },
  { href: '/settings/roles', icon: Shield, label: 'Uloge' },
  { href: '/settings/security', icon: Shield, label: 'Sigurnost' },
  { href: '/settings/email', icon: Mail, label: 'Email' },
  { href: '/settings/language', icon: Globe, label: 'Jezik' },
  { href: '/settings/features', icon: Sparkles, label: 'Feature flags' },
]

export default function Sidebar() {
  const { isExpanded, isMobileOpen, toggleExpanded, closeMobile } = useSidebar()

  return (
    <>
      <aside
        className={`fixed inset-y-0 left-0 z-40 flex flex-col border-r border-border bg-surface transition-[width] duration-300 ease-in-out ${
          isExpanded ? 'w-[var(--sidebar-width)]' : 'w-[var(--sidebar-collapsed-width)]'
        }`}
      >
        <div
          className={`flex h-[var(--navbar-height)] items-center border-b border-border px-4 ${
            isExpanded ? 'gap-3' : 'justify-center'
          }`}
        >
          <Hotel className="h-6 w-6 shrink-0 text-primary-600" />
          {isExpanded && (
            <span className="text-lg font-semibold text-text">HotelPRO</span>
          )}
        </div>

        <nav className="flex-1 space-y-1 overflow-y-auto p-3">
          {sidebarItems.map((item) => (
            <SidebarItem key={item.href} item={item} />
          ))}
        </nav>

        <div className="border-t border-border p-3">
          <button
            onClick={toggleExpanded}
            className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm text-text-secondary hover:bg-surface-secondary dark:hover:bg-surface-tertiary transition-colors"
            title={isExpanded ? 'Skupi sidebar' : 'Prosiri sidebar'}
          >
            {isExpanded ? (
              <>
                <ChevronLeft className="h-4 w-4" />
                <span>Skupi</span>
              </>
            ) : (
              <ChevronRight className="h-4 w-4 mx-auto" />
            )}
          </button>
        </div>
      </aside>

      {isMobileOpen && (
        <div
          className="fixed inset-0 z-30 bg-black/50 lg:hidden"
          onClick={closeMobile}
        />
      )}
    </>
  )
}
